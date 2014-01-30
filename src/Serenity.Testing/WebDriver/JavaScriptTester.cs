using System.Collections;
using System.Collections.Generic;
using FubuTestingSupport;
using NUnit.Framework;
using Serenity.WebDriver;
using Serenity.WebDriver.JavaScriptBuilders;

namespace Serenity.Testing.WebDriver
{
    public class JavaScriptTester
    {
        [TestCase("does nothing to this string", Result = "$(\"does nothing to this string\")")]
        [TestCase(".test", Result = "$(\".test\")")]
        public string CreatesJQueryByWithGivenSelector(string selector)
        {
            return (string) JavaScript.CreateJQuery(selector).Statement;
        }

        [Test]
        public void JQueryByCanBeImplicitlyCastToSeleniumBy()
        {
            OpenQA.Selenium.By implicitCast = By.jQuery(".test");
            implicitCast.ShouldNotBeNull();
            implicitCast.ShouldBeOfType<JavaScriptBy>();
        }

        [Test]
        public void JQueryByCanBeImplicitlyCastToSerenityJqueryBy()
        {
            By implicitCast = By.jQuery(".test");
            implicitCast.ShouldNotBeNull();
            implicitCast.ShouldBeOfType<JavaScriptBy>();
        }

        [Test]
        public void ByJQueryBuildsSelector()
        {
            string selector = By.jQuery(".test").Statement;
            selector.ShouldEqual("$(\".test\")");
        }

        [Test]
        public void BuildsStatementWithAdditionalMethod()
        {
            dynamic javaScript = new JavaScript("$(\".test\")");
            string statement = javaScript.Find().Statement;
            statement.ShouldEqual("$(\".test\").find()");
        }

        [TestCase(null, Result="$(\".test\").find()")]
        [TestCase("", Result="$(\".test\").find(\"\")")]
        [TestCase(".child", Result="$(\".test\").find(\".child\")")]
        [TestCase(123, Result="$(\".test\").find(123)")]
        [TestCase(123.45, Result="$(\".test\").find(123.45)")]
        // TODO: Test DateTime?
        public string BuildsSelectorWithAdditionalMethodOneParameter(object param)
        {
            dynamic javaScript = new JavaScript("$(\".test\")");
            return (string) javaScript.Find(param).Statement;
        }

        [TestCase(null, null, Result="$(\".test\").find()")]
        [TestCase("", null, Result="$(\".test\").find(\"\")")]
        [TestCase(".child", null, Result="$(\".test\").find(\".child\")")]
        [TestCase(123, null, Result="$(\".test\").find(123)")]
        [TestCase(123.45, null, Result="$(\".test\").find(123.45)")]

        [TestCase(null, "", Result="$(\".test\").find(undefined, \"\")")]
        [TestCase(null, ".child", Result="$(\".test\").find(undefined, \".child\")")]
        [TestCase(null, 123, Result="$(\".test\").find(undefined, 123)")]
        [TestCase(null, 123.45, Result="$(\".test\").find(undefined, 123.45)")]

        [TestCase("", "", Result="$(\".test\").find(\"\", \"\")")]
        [TestCase(123, "", Result="$(\".test\").find(123, \"\")")]
        [TestCase("", 123, Result="$(\".test\").find(\"\", 123)")]
        // TODO: Test DateTime?
        public string BuildsSelectorWithAdditionalMethodTwoParameters(object param1, object param2)
        {
            dynamic javaScript = new JavaScript("$(\".test\")");
            return (string) javaScript.Find(param1, param2).Statement;
        }

        [Test]
        public void TestJavaScriptFunctionAsParameter()
        {
            var javaScript = JavaScript.Create("$(this)")
                .Filter(JavaScript.Function(new[]{ "x" },
                    JavaScript.Create("$(this)").Text().Trim().ModifyStatement("return {0} === x;")));

            string raw = javaScript.Statement;
            raw.ShouldEqual("$(this).filter(function(x) { return $(this).text().trim() === x; })");
        }

        [Test]
        public void LowerCasesFirstLetterOnly()
        {
            dynamic javaScript = new JavaScript("$(\".test\")");
            string selector = javaScript.CamelCaseWords().Statement;
            selector.ShouldEqual("$(\".test\").camelCaseWords()");
        }

        [Test]
        public void RegistersJavaScriptBuilders()
        {
            TestJavaScript.Builders.Count.ShouldEqual(4);
            TestJavaScript.Builders[0].ShouldBeOfType<NullObjectJavaScriptBuilder>();
            TestJavaScript.Builders[1].ShouldBeOfType<StringJavaScriptBuilder>();
            TestJavaScript.Builders[2].ShouldBeOfType<JavaScriptBuilder>();
            TestJavaScript.Builders[3].ShouldBeOfType<DefaultJavaScriptBuilder>();
        }

        public class TestJavaScript : JavaScript
        {
            public TestJavaScript(string statement) : base(statement) { }

            public static IList<IJavaScriptBuilder> Builders { get { return JavaScriptBuilders; } }
        }
    }
}