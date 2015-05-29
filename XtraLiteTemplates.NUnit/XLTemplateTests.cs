//
//  Author:
//    Alexandru Ciobanu alex@ciobanu.org
//
//  Copyright (c) 2015, Alexandru Ciobanu (alex@ciobanu.org)
//
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in
//       the documentation and/or other materials provided with the distribution.
//     * Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
//
using NUnit.Framework;

namespace XtraLiteTemplates.NUnit
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using XtraLiteTemplates.NUnit.Inside;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Parsing;
    using System.Globalization;
    using XtraLiteTemplates.Dialects.Standard;

    [TestFixture]
    public class XLTemplateTests : TestBase
    {
        [Test]
        public void TestCaseConstruction1()
        {
            ExpectArgumentNullException("dialect", () => new XLTemplate(null, ""));
            ExpectArgumentNullException("template", () => new XLTemplate(StandardDialect.Default, null));
        }

        [Test]
        public void TestCaseConstruction2()
        {
            var text = "{IF true THEN}Say True Baby!{END}";
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, text);
            
            Assert.AreEqual(StandardDialect.DefaultIgnoreCase, template.Dialect);
            Assert.AreEqual(text, template.Template);
        }

        [Test]
        public void TestCaseEvaluate1()
        {
            var text = "contents are irrelevant";
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, text);

            var variables = new Dictionary<String, Object>()
            {
            };
  
            ExpectArgumentNullException("variables", () => template.Evaluate(null));
            ExpectArgumentNullException("writer", () => template.Evaluate(null, variables));
        }

        [Test]
        public void TestCaseEvaluate2()
        {
            var text = "V = {variable}";
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, text);

            var variables = new Dictionary<String, Object>()
            {
                { "Variable", 1 }
            };
            
            using (var writer = new StringWriter())
            {
                template.Evaluate(writer, variables);
                Assert.AreEqual("V =1", writer.ToString());
            }

            Assert.AreEqual("V =1", template.Evaluate(variables));
        }

        [Test]
        public void TestCaseEvaluate3()
        {
            var text = "V = {variable}";
            var template = new XLTemplate(StandardDialect.Default, text);

            var variables1 = new Dictionary<String, Object>()
            {
                { "Variable", 1 }
            };
            var variables2 = new Dictionary<String, Object>()
            {
                { "variable", 1 }
            };
            var variables3 = new Dictionary<String, Object>()
            {
                { "Variable", 1 },
                { "variable", 2 }
            };

            Assert.AreEqual("V =UNDEFINED", template.Evaluate(variables1));
            Assert.AreEqual("V =1", template.Evaluate(variables2));
            Assert.AreEqual("V =2", template.Evaluate(variables3));
        }

        [Test]
        public void TestCaseToString1()
        {
            var text = "{if true then}eat a cookie!{else}{variable}{end}";
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, text);

            Assert.AreEqual("(({if True then}eat a cookie!{else}({@variable}){end}))", template.ToString());
        }

        [Test]
        public void TestCaseToString2()
        {
            var text = "";
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, text);

            Assert.AreEqual("()", template.ToString());
        }

        [Test]
        public void TestCaseStaticEvaluate1()
        {
            ExpectArgumentNullException("dialect", () => XLTemplate.Evaluate(null, ""));
            ExpectArgumentNullException("template", () => XLTemplate.Evaluate(StandardDialect.Default, null));
            ExpectArgumentNullException("arguments", () => XLTemplate.Evaluate(StandardDialect.Default, "", (Object[])null));
        }

        [Test]
        public void TestCaseStaticEvaluate2()
        {
            var result = XLTemplate.Evaluate(StandardDialect.DefaultIgnoreCase, "{_0} -> {_1}", "string", 100.33);
            Assert.AreEqual("string->100.33", result);
        }

        [Test]
        public void TestCasePreformattedEvaluate()
        {
            var result = XLTemplate.Evaluate(StandardDialect.DefaultIgnoreCase, "{preformatted}{_0} -> {_1}{end}", "string", 100.33);
            Assert.AreEqual("string -> 100.33", result);
        }

        [Test]
        public void TestCaseEvaluationFrames()
        {
            String template = @"
            {_0} /
            {FOR EACH _0 IN 1..2}
                {_0} /
                {FOR EACH _0 IN 3 ..4}
                {preformatted}  {_0}  {end}    /
                {END}
                {_0} /
            {END}
            {_0}";

            var result = XLTemplate.Evaluate(StandardDialect.DefaultIgnoreCase, template, "initial");
            Assert.AreEqual("initial/1/  3  /  4  /1/2/  3  /  4  /2/initial", result);
        }

        [Test]
        public void TestCaseEvaluationWithMethods1()
        {
            var result = XLTemplate.Evaluate(StandardDialect.DefaultIgnoreCase, @"{_0.GetType().Name}", this);
            Assert.AreEqual(this.GetType().Name, result);
        }

        [Test]
        public void TestCaseEvaluationWithMethods2()
        {
            var result = XLTemplate.Evaluate(CodeMonkeyDialect.DefaultIgnoreCase, @"{_0.Replace('Hello', _1)}", "Hello World", "Funky");

            Assert.AreEqual("Funky World", result);
        }

        [Test]
        public void TestCaseEvaluationWithSelf()
        {
            var result = XLTemplate.Evaluate(CodeMonkeyDialect.DefaultIgnoreCase, @"{Number('199') + Number(true)}");
            Assert.AreEqual("200", result);

            result = XLTemplate.Evaluate(CodeMonkeyDialect.DefaultIgnoreCase, @"{Number('Ha')}");
            Assert.AreEqual("NaN", result);

            result = XLTemplate.Evaluate(CodeMonkeyDialect.DefaultIgnoreCase, @"{String(100) + 1 + true}");
            Assert.AreEqual("1001True", result);

            result = XLTemplate.Evaluate(CodeMonkeyDialect.DefaultIgnoreCase, @"{Boolean(-3.95)}");
            Assert.AreEqual("True", result);
        }
    }
}

