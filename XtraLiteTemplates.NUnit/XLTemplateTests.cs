//
//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2017, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.NUnit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    using global::NUnit.Framework;

    using XtraLiteTemplates.Dialects.Standard;

    [TestFixture]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class XLTemplateTests : TestBase
    {
        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseConstruction1()
        {
            ExpectArgumentNullException("dialect", () => new XLTemplate(null, string.Empty));
            ExpectArgumentNullException("template", () => new XLTemplate(StandardDialect.Default, null));
        }

        [Test]
        public void TestCaseConstruction2()
        {
            const string Text = "{IF true THEN}Say True Baby!{END}";
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, Text);

            Assert.AreEqual(StandardDialect.DefaultIgnoreCase, template.Dialect);
            Assert.AreEqual(Text, template.Template);
        }

        [Test]
        [SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
        public void TestCaseEvaluate1()
        {
            const string Text = "contents are irrelevant";
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, Text);

            var variables = new Dictionary<string, object>();

            ExpectArgumentNullException(
                "variables",
                () => template.Evaluate((IReadOnlyDictionary<string, object>)null));
            ExpectArgumentNullException("variables", () => template.Evaluate((Expression<Func<object, object>>[])null));
            ExpectArgumentNullException(
                "variables",
                () => template.Evaluate(new StringWriter(), (IReadOnlyDictionary<string, object>)null));
            ExpectArgumentNullException("writer", () => template.Evaluate(null, variables));
        }

        [Test]
        public void TestCaseEvaluate2()
        {
            const string Text = "V = {variable}";
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, Text);

            var variables = new Dictionary<string, object> { { "Variable", 1 } };

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
            const string Text = "V = {variable}";
            var template = new XLTemplate(StandardDialect.Default, Text);

            var variables1 = new Dictionary<string, object> { { "Variable", 1 } };
            var variables2 = new Dictionary<string, object> { { "variable", 1 } };
            var variables3 = new Dictionary<string, object> { { "Variable", 1 }, { "variable", 2 } };

            Assert.AreEqual("V =UNDEFINED", template.Evaluate(variables1));
            Assert.AreEqual("V =1", template.Evaluate(variables2));
            Assert.AreEqual("V =2", template.Evaluate(variables3));
        }

        [Test]
        public void TestCaseEvaluate4()
        {
            const string Text = "irrelevant";
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, Text);

            ExpectArgumentNullException(
                "variables",
                () => template.Evaluate(new StringWriter(), (Expression<Func<object, object>>[])null));
            ExpectArgumentNullException("writer", () => template.Evaluate((TextWriter)null, a => a));
        }

        [Test]
        public void TestCaseEvaluate5()
        {
            const string Text = "{variable1}={variable2}";
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, Text);

            using (var writer = new StringWriter())
            {
                template.Evaluate(writer, variable1 => variable1, variable2 => 100);
                Assert.AreEqual("variable1=100", writer.ToString());
            }

            Assert.AreEqual("variable1=100", template.Evaluate(variable1 => variable1, variable2 => 100));
        }

        [Test]
        public void ToStringForAComplexTemplateReturnsTheExpectedRepresentation()
        {
            const string Text = "{if true then}eat a cookie!{else}{variable}{end}";
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, Text);

            Assert.AreEqual("(({if True then}eat a cookie!{else}({@variable}){end}))", template.ToString());
        }

        [Test]
        public void ToStringForAnEmptyTemplateReturnsEmptyRepresentation()
        {
            var template = new XLTemplate(StandardDialect.DefaultIgnoreCase, string.Empty);

            Assert.AreEqual("()", template.ToString());
        }

        [Test]
        public void EvaluateUsingNullDialectRaisesException()
        {
            Assert.Throws<ArgumentNullException>(() => XLTemplate.Evaluate(null, string.Empty));
        }

        [Test]
        public void EvaluateUsingNullTemplateRaisesException()
        {
            Assert.Throws<ArgumentNullException>(() => XLTemplate.Evaluate(StandardDialect.Default, null));
        }

        [Test]
        public void EvaluateUsingNullArgumentsRaisesException()
        {
            Assert.Throws<ArgumentNullException>(
                () => XLTemplate.Evaluate(StandardDialect.Default, string.Empty, null));
        }

        [Test]
        public void EvaluateWithoutPreformattedDirectiveRemovesExtraWhitespaces()
        {
            var result = XLTemplate.Evaluate(
                StandardDialect.DefaultIgnoreCase,
                "{v0} -> {v1}",
                v0 => "string",
                v1 => 100.33);
            Assert.AreEqual("string->100.33", result);
        }

        [Test]
        public void EvaluateUsingPreformattedDirectiveProducesUnFormattedResult()
        {
            var result = XLTemplate.Evaluate(
                StandardDialect.DefaultIgnoreCase,
                "{preformatted}{v0} -> {v1}{end}",
                v0 => "string",
                v1 => 100.33);
            Assert.AreEqual("string -> 100.33", result);
        }

        [Test]
        public void EvaluateForTwoNestedLoopsProducesTheExpectedResult()
        {
            const string Template = @"
            {initial} /
            {FOR EACH initial IN 1..2}
                {initial} /
                {FOR EACH initial IN 3 ..4}
                {preformatted}  {initial}  {end}    /
                {END}
                {initial} /
            {END}
            {initial}";

            var result = XLTemplate.Evaluate(StandardDialect.DefaultIgnoreCase, Template, initial => initial);
            Assert.AreEqual("initial/1/  3  /  4  /1/2/  3  /  4  /2/initial", result);
        }

        [Test]
        public void EvaluateInvokingObjectGetTypeMethodProducesTheExpectedResult()
        {
            var result = XLTemplate.Evaluate(StandardDialect.DefaultIgnoreCase, @"{me.GetType().Name}", me => this);
            Assert.AreEqual(GetType().Name, result);
        }

        [Test]
        public void EvaluateInvokingStringReplaceMethodProducesTheExpectedResult()
        {
            var result = XLTemplate.Evaluate(
                CodeMonkeyDialect.DefaultIgnoreCase,
                @"{hw.Replace('Hello', f)}",
                hw => "Hello World",
                f => "Funky");
            Assert.AreEqual("Funky World", result);
        }

        [Test]
        public void EvaluateUsingStandardSelfMethodsCompletesAsExpected()
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

        [Test]
        public void EvaluateAsyncTokenForNullTextWriterRaisesException1()
        {
            Assert.ThrowsAsync<ArgumentNullException>(
                async () =>
                    {
                        await new XLTemplate(StandardDialect.DefaultIgnoreCase, @"text").EvaluateAsync(
                            null,
                            CancellationToken.None,
                            new Dictionary<string, object>());
                    });
        }

        [Test]
        public void EvaluateAsyncForNullTextWriterRaisesException2()
        {
            Assert.ThrowsAsync<ArgumentNullException>(
                async () =>
                    {
                        await new XLTemplate(StandardDialect.DefaultIgnoreCase, @"text").EvaluateAsync(
                            null,
                            CancellationToken.None,
                            a => a);
                    });
        }

        [Test]
        public void EvaluateAsyncForNullVariablesDictionaryRaisesException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await new XLTemplate(StandardDialect.DefaultIgnoreCase, @"text").EvaluateAsync(
                                new StringWriter(),
                                CancellationToken.None,
                                (IReadOnlyDictionary<string, object>)null));
        }

        [Test]
        public void EvaluateAsyncForNullVariablesArrayRaisesException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await new XLTemplate(StandardDialect.DefaultIgnoreCase, @"text").EvaluateAsync(
                                new StringWriter(),
                                CancellationToken.None,
                                (Expression<Func<object, object>>[])null));
        }

        [Test]
        public void EvaluateAsyncCancelledByTokenRaisesException()
        {
            var tokenSource = new CancellationTokenSource();
            var xlTemplate = new XLTemplate(StandardDialect.DefaultIgnoreCase, @"{FOR EACH X IN 0..99999999}{X}{END}");

            tokenSource.CancelAfter(50);
            Assert.ThrowsAsync<TaskCanceledException>(
                async () => await xlTemplate.EvaluateAsync(
                                new StringWriter(),
                                tokenSource.Token,
                                new Dictionary<string, object>()));
        }

        [Test]
        public async Task EvaluateAsyncWithAZeroTokenCompletesAsExpected1()
        {
            var xlTemplate = new XLTemplate(StandardDialect.DefaultIgnoreCase, @"text");

            using (var sw = new StringWriter())
            {
                await xlTemplate.EvaluateAsync(sw, CancellationToken.None, new Dictionary<string, object>());
                Assert.AreEqual("text", sw.ToString());
            }
        }

        [Test]
        public async Task EvaluateAsyncWithAZeroTokenCompletesAsExpected2()
        {
            var xlTemplate = new XLTemplate(StandardDialect.DefaultIgnoreCase, @"{a}");

            using (var sw = new StringWriter())
            {
                await xlTemplate.EvaluateAsync(sw, CancellationToken.None, a => "text");
                Assert.AreEqual("text", sw.ToString());
            }
        }

        [Test]
        public async Task EvaluateAsyncCompletesAsExpected1()
        {
            var xlTemplate = new XLTemplate(StandardDialect.DefaultIgnoreCase, @"text");

            using (var sw = new StringWriter())
            {
                await xlTemplate.EvaluateAsync(sw, CancellationToken.None, new Dictionary<string, object>());
                Assert.AreEqual("text", sw.ToString());
            }
        }

        [Test]
        public async Task EvaluateAsyncCompletesAsExpected2()
        {
            var xlTemplate = new XLTemplate(StandardDialect.DefaultIgnoreCase, @"{a}");

            using (var sw = new StringWriter())
            {
                await xlTemplate.EvaluateAsync(sw, CancellationToken.None, a => "text");
                Assert.AreEqual("text", sw.ToString());
            }
        }

        [Test]
        public void EvaluateThrowsExceptionIfVariableExpressionIsNull1()
        {
            var sw = new StringWriter();
            Assert.Throws<ArgumentException>(
                () => new XLTemplate(StandardDialect.Default, "template").Evaluate(sw, a => a, null));
            Assert.Throws<ArgumentException>(
                () => new XLTemplate(StandardDialect.Default, "template").Evaluate(
                    sw,
                    a => a,
                    b => ((string)null).Length));
            Assert.Throws<ArgumentException>(
                () => new XLTemplate(StandardDialect.Default, "template").Evaluate(a => a, null));
            Assert.Throws<ArgumentException>(
                () => new XLTemplate(StandardDialect.Default, "template").Evaluate(a => a, b => ((string)null).Length));
        }

        [Test]
        public void EvaluateThrowsExceptionIfVariableExpressionIsNull2()
        {
            var sw = new StringWriter();
            Assert.ThrowsAsync<ArgumentException>(
                async () => await new XLTemplate(StandardDialect.Default, "template").EvaluateAsync(
                                sw,
                                CancellationToken.None,
                                a => a,
                                null));
            Assert.ThrowsAsync<ArgumentException>(
                async () => await new XLTemplate(StandardDialect.Default, "template").EvaluateAsync(
                                sw,
                                CancellationToken.None,
                                a => a,
                                b => ((string)null).Length));
        }



        [Test]
        public void EvaluateAsyncStaticForNullVariablesArrayRaisesException()
        {
            Assert.ThrowsAsync<ArgumentNullException>(
                async () => await XLTemplate.EvaluateAsync(
                                StandardDialect.DefaultIgnoreCase,
                                @"text",
                                CancellationToken.None,
                                null));
        }

        [Test]
        public void EvaluateAsyncStaticCancelledByTokenRaisesException()
        {
            var tokenSource = new CancellationTokenSource();

            tokenSource.CancelAfter(50);
            Assert.ThrowsAsync<TaskCanceledException>(
                async () => await XLTemplate.EvaluateAsync(
                                StandardDialect.DefaultIgnoreCase,
                                @"{FOR EACH X IN 0..99999999}{X}{END}",
                                tokenSource.Token,
                                a => a));
        }

        [Test]
        public async Task EvaluateAsyncStaticCompletesAsExpected()
        {
            var result = await XLTemplate.EvaluateAsync(
                             StandardDialect.DefaultIgnoreCase,
                             @"{a}",
                             CancellationToken.None,
                             a => "text");
            Assert.AreEqual("text", result);
        }
    }
}