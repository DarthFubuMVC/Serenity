using FubuCore;
using OpenQA.Selenium;

namespace Serenity.Fixtures.Handlers
{
    public class TextboxElementHandler : IElementHandler
    {
        public virtual bool Matches(IWebElement element)
        {
            // TODO --- Change to psuedo CSS class?  Less repeated logic
            var tagName = element.TagName.ToLower();
            var type = element.GetAttribute("type");
            type = type == null ? null : type.ToLower();
            return tagName == "input" && (type == "text" || type == "password");
       }

        public virtual void EraseData(ISearchContext context, IWebElement element)
        {
            if (element.GetAttribute("value").IsNotEmpty())
            {
                element.Click();
                element.SendKeys(Keys.Home + Keys.Shift + Keys.End + Keys.Delete);
            }
        }

        public virtual void EnterData(ISearchContext context, IWebElement element, object data)
        {
            EraseData(context, element);
            element.SendKeys(data as string ?? string.Empty);
        }

        public virtual string GetData(ISearchContext context, IWebElement element)
        {
            return element.GetAttribute("value");
        }
    }
}
