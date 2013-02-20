using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using FubuCore;
using FubuCore.Reflection;
using FubuLocalization;
using FubuMVC.Core.Endpoints;
using OpenQA.Selenium;
using Serenity.Fixtures.Grammars;
using StoryTeller;
using StoryTeller.Assertions;
using StoryTeller.Engine;

namespace Serenity.Fixtures
{
    public class ScreenFixture : Fixture
    {
        private readonly Stack<ISearchContext> _searchContexts = new Stack<ISearchContext>();
        private IApplicationUnderTest _application;

        protected ScreenFixture()
        {
        }

        protected ISearchContext SearchContext
        {
            get
            {
                if (_searchContexts.Count == 0)
                {
                    _searchContexts.Push(_application.Driver);
                }

                return _searchContexts.Peek();
            }
        }

        protected IApplicationUnderTest Application
        {
            get { return _application; }
        }

        protected NavigationDriver Navigation
        {
            get { return _application.Navigation; }
        }

        protected EndpointDriver Endpoints
        {
            get { return _application.Endpoints(); }
        }

        protected IWebDriver Driver
        {
            get { return _application.Driver; }
        }

        public override sealed void SetUp(ITestContext context)
        {
            _application = context.Retrieve<IApplicationUnderTest>();

            beforeRunning();
        }

        protected virtual void beforeRunning()
        {
        }

        protected IGrammar Click(By selector = null, string id = null, string css = null, string name = null,
                                 string label = null, string template = null)
        {
            var by = selector ?? id.ById() ?? css.ByCss() ?? name.ByName();

            if (by == null)
                throw new InvalidOperationException("Must specify either the selector, css, or name property");

            label = label ?? by.ToString().Replace("By.", "");

            var config = new GestureConfig{
                Template = template ?? "Click " + label,
                Description = "Click " + label,
                Finder = () => SearchContext.FindElement(by),
                FinderDescription = by.ToString()
            };

            return new ClickGrammar(config);
        }

        // TODO -- UT this some how
        protected IGrammar JQueryClick(string template, string id = null, string className = null, string css = null, string tagName = null)
        {
            string command = buildJQuerySearch(css, id, className, tagName);

            return Do(template, () =>
            {
                Retry.Twice(() => Driver.InjectJavascript(command));
            });
        }

        private static string buildJQuerySearch(string css, string id, string className, string tagName)
        {
            var search = css;

            if (id.IsNotEmpty())
            {
                search = "#" + id;
            }

            if (className.IsNotEmpty())
            {
                search = "." + className;
            }

            if (tagName.IsNotEmpty())
            {
                search = tagName + search;
            }

            return "$('{0}').click();".ToFormat(search);
        }

        protected void ClickWithJQuery(string id = null, string className = null, string css = null, string tagName = null)
        {
            string command = buildJQuerySearch(css, id, className, tagName);

            Retry.Twice(() => Driver.InjectJavascript(command));
        }

        protected void PushElementContext(ISearchContext context)
        {
            _searchContexts.Push(context);
        }

        protected void PushElementContext(By selector)
        {
            var element = SearchContext.FindElement(selector);
            StoryTellerAssert.Fail(element == null, () => "Unable to find element with " + selector);

            PushElementContext(element);
        }

        protected void waitForElement(By elementSearch, int millisecondPolling = 500, int timeoutInMilliseconds = 5000)
        {
            Wait.Until(() => SearchContext.FindElement(elementSearch) != null, millisecondPolling, timeoutInMilliseconds);
        }
        
        protected void PopElementContext()
        {
            _searchContexts.Pop();
        }

        protected string GetData(IWebElement element)
        {
            return Driver.GetData(element);
        }

        protected string GetData(By finder)
        {
            var element = SearchContext.FindElement(finder);
            return SearchContext.GetData(element);
        }

        protected void SetData(IWebElement element, string data)
        {
            SearchContext.SetData(element, data);
        }

        protected void SetData(By finder, string data)
        {
            var element = SearchContext.FindElement(finder);
            SetData(element, data);
        }
    }

    public class ScreenFixture<T> : ScreenFixture
    {
        private void enterValue(Expression<Func<T, object>> expression, string value)
        {
            // TODO -- use the field naming convention?
            var name = expression.ToAccessor().Name;
            SetData(By.Name(name), value);
        }

        public string readValue(Expression<Func<T, object>> expression)
        {
            var name = expression.ToAccessor().Name;
            return GetData(By.Name(name));
        }

        private GestureConfig getGesture(Expression<Func<T, object>> expression, string label = null, string key = null)
        {
            // TODO -- later on, use the naming convention from fubu instead of pretending
            // that this rule is always true
            var config = GestureForProperty(expression);
            if (key.IsNotEmpty())
            {
                config.CellName = key;
            }

            config.Label = label ?? LocalizationManager.GetHeader(expression);

            return config;
        }


        protected IGrammar EnterScreenValue(Expression<Func<T, object>> expression, string label = null,
                                            string key = null)
        {
            var config = getGesture(expression, label, key);

            config.Template = "Enter {" + config.CellName + "} for " + config.Label;
            config.Description = "Enter data for property " + FubuCore.Reflection.ReflectionExtensions.ToAccessor(expression).Name;

            return new EnterValueGrammar(config);
        }

        protected IGrammar CheckScreenValue(Expression<Func<T, object>> expression, string label = null,
                                            string key = null)
        {
            var config = getGesture(expression, label, key);

            config.Template = "The text of " + config.Label + " should be {" + config.CellName + "}";
            config.Description = "Check data for property " + FubuCore.Reflection.ReflectionExtensions.ToAccessor(expression).Name;

            return new CheckValueGrammar(config);
        }


        protected GestureConfig GestureForProperty(Expression<Func<T, object>> expression)
        {
            return GestureConfig.ByProperty(() => SearchContext, expression);
        }

        protected void EditableElement(Expression<Func<T, object>> expression, string label = null)
        {
            var accessor = expression.ToAccessor();
            var name = accessor.Name;

            this["Check" + name] = CheckScreenValue(expression, label);
            this["Enter" + name] = EnterScreenValue(expression, label);
        }

        protected void EditableElementsForAllImmediateProperties()
        {
			typeof (T)
			    .GetProperties(BindingFlags.Public | BindingFlags.Instance).Where(x => x.CanRead && x.CanWrite).Each(prop =>
			    {
			        var accessor = new SingleProperty(prop);
			        var expression = accessor.ToExpression<T>();

			        EditableElement(expression);
			    });
        }

        //public void EditableElements(params)
    }
}