﻿using System;
using FubuCore;
using FubuTestingSupport;
using HtmlTags;
using NUnit.Framework;
using OpenQA.Selenium;
using Serenity.Fixtures;
using Serenity.Testing.Fixtures;

namespace Serenity.Testing.WebDriver
{
    public class WebDriverExtensionsTester : ScreenManipulationTester
    {
        private const string EmptyTextboxId = "emptytextboxid";
        private const string EmptyTextboxName = "emptytextboxname";

        private const string DelayAddElementButtonId = "delayAddButton";
        private const string AddedElementsClass = "added-elements";

        protected override void configureDocument(HtmlDocument document)
        {
            var textbox = new TextboxTag(EmptyTextboxName, string.Empty)
                .Id(EmptyTextboxId)
                .Data("value", "some value");

            document.Add(textbox);

            var button = new HtmlTag("button")
                .Id(DelayAddElementButtonId)
                .Text("Append element");

            document.Add(button);

            var appendedElementsDiv = new DivTag().AddClass(AddedElementsClass);

            document.Add(appendedElementsDiv);

            document.ReferenceJavaScriptFile("file:///" + "jquery-2.0.3.min.js".ToFullPath());

            document.AddJavaScript("$(function() { $('#" + DelayAddElementButtonId + "').click(function() { setTimeout(function() { $('." + AddedElementsClass + "').append('<p>blah</p>'); }, 3000); }); });");
        }

        [Test]
        public void HasAttributeReturnsTrue()
        {
            theDriver.FindElement(By.Id(EmptyTextboxId)).HasAttribute("data-value").ShouldBeTrue();
        }

        [Test]
        public void HasAttributeReturnsFalse()
        {
            theDriver.FindElement(By.Id(EmptyTextboxId)).HasAttribute("data-is-not-there").ShouldBeFalse();
        }

        [Test]
        public void WaitForElementWaitsForElementToBeOnTheDom()
        {
            theDriver.FindElement(By.Id(DelayAddElementButtonId)).Click();
            theDriver.WaitForElement(By.CssSelector(".{0} p".ToFormat(AddedElementsClass))).ShouldNotBeNull();
        }

        [Test]
        public void WaitForElementWaitsForElementToBeOnTheDomNeverShowsUp()
        {
            Exception<NoSuchElementException>.ShouldBeThrownBy(() =>
            {
                theDriver.WaitForElement(By.CssSelector(".there-is-no-element".ToFormat(AddedElementsClass)), timeout: TimeSpan.FromSeconds(2));
            });
        }
    }
}