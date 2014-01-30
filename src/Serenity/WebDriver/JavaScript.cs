﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Dynamic;
using System.Linq;
using FubuCore;
using OpenQA.Selenium;
using Serenity.WebDriver.JavaScriptBuilders;

namespace Serenity.WebDriver
{
    public class JavaScript : DynamicObject
    {
        public string Statement { get; private set; }

        protected static IList<IJavaScriptBuilder> JavaScriptBuilders { get; private set; }

        static JavaScript()
        {
            JavaScriptBuilders = new ReadOnlyCollection<IJavaScriptBuilder>(new IJavaScriptBuilder[]
            {
                new NullObjectJavaScriptBuilder(),
                new StringJavaScriptBuilder(),
                new JavaScriptBuilder(),
                new DefaultJavaScriptBuilder()
            });
        }

        public JavaScript(string statement)
        {
            Statement = statement;
        }

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var javascriptFriendlyName = char.ToLowerInvariant(binder.Name[0]) + binder.Name.Substring(1);
            result = new JavaScript(AppendFunction(javascriptFriendlyName, args));
            return true;
        }

        public object ExecuteAndGet(IJavaScriptExecutor executor)
        {
            return executor.ExecuteScript("return {0};".ToFormat(Statement));
        }

        public T ExecuteAndGet<T>(IJavaScriptExecutor executor) where T : class
        {
            return ExecuteAndGet(executor) as T;
        }

        public void Execute(IJavaScriptExecutor executor)
        {
            executor.ExecuteScript(Statement);
        }

        public dynamic ModifyStatement(string format)
        {
            return new JavaScript(format.ToFormat(Statement));
        }

        private string AppendFunction(string func, params object[] args)
        {
            var argsString = args == null
                ? ""
                : args
                    .Reverse()
                    .SkipWhile(arg => arg == null)
                    .Reverse()
                    .Select(arg => JavaScriptBuilders.First(x => x.Matches(arg)).Build(arg))
                    .Join(", ");

            return "{0}.{1}({2})".ToFormat(Statement, func, argsString);
        }

        public static dynamic Create(string javaScript)
        {
            return new JavaScript(javaScript);
        }

        public static dynamic CreateJQuery(string selector)
        {
            return new JavaScript("$(\"" + selector + "\")");
        }

        public static dynamic Function(JavaScript body)
        {
            return Function(Enumerable.Empty<string>(), body);
        }

        public static dynamic Function(IEnumerable<string> args, JavaScript body)
        {
            if (args == null)
                throw new ArgumentNullException("args");

            return new JavaScript("function({0}) {{ {1} }}".ToFormat(args.Join(", "), body.Statement));
        }

        public static implicit operator By(JavaScript source)
        {
            return (JavaScriptBy) source;
        }

        public static implicit operator OpenQA.Selenium.By(JavaScript source)
        {
            return (JavaScriptBy) source;
        }

        public static implicit operator JavaScriptBy(JavaScript source)
        {
            return new JavaScriptBy(source);
        }

        public static implicit operator JavaScript(JavaScriptBy source)
        {
            return (JavaScript) source.JavaScript;
        }
    }
}