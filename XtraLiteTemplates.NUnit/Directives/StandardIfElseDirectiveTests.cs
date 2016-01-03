//
//  Author:
//    Alexandru Ciobanu alex@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex@ciobanu.org)
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

namespace XtraLiteTemplates.NUnit.Directives
{
    using System;
    using System.IO;
    using System.Linq;
    using XtraLiteTemplates.NUnit.Inside;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Parsing;
    using XtraLiteTemplates.Dialects.Standard.Directives;
    using System.Globalization;
    using System.Collections.Generic;

    [TestFixture]
    public class StandardIfElseDirectiveTests : TestBase
    {
        [Test]
        public void TestCaseConstructor1()
        {
            ExpectInvalidTagMarkupException(null, () => new IfElseDirective(null, "ELSE", "END", TypeConverter));
            ExpectInvalidTagMarkupException(null, () => new IfElseDirective("IF $", null, "END", TypeConverter));
            ExpectInvalidTagMarkupException(null, () => new IfElseDirective("IF $", "ELSE", null, TypeConverter));
            ExpectArgumentNullException("typeConverter", () => new IfElseDirective("IF $", "ELSE", "END", null));

            ExpectArgumentConditionNotTrueException("one expression component", () => new IfElseDirective("?", "ELSE", "END", TypeConverter));
            ExpectArgumentConditionNotTrueException("one expression component", () => new IfElseDirective("$ AND $ THEN ?", "ELSE", "END", TypeConverter));
        }

        [Test]
        public void TestCaseConstructor2()
        {
            var directive = new IfElseDirective(TypeConverter);
            Assert.AreEqual("{IF $ THEN}...{ELSE}...{END}", directive.ToString());
        }

        [Test]
        public void TestCaseConstructor3()
        {
            var directive = new IfElseDirective("MAYBE $ IN WHICH CASE", "OTHERWISE", "TERMINATE", TypeConverter);
            Assert.AreEqual("{MAYBE $ IN WHICH CASE}...{OTHERWISE}...{TERMINATE}", directive.ToString());
        }

        [Test]
        public void TestCaseEvaluation1()
        {
            var directive = new IfElseDirective("MAYBE $", "OTHERWISE", "DONE", TypeConverter);

            Assert.AreEqual("yes", Evaluate("{MAYBE 1}yes{OTHERWISE}no{DONE}", directive));
        }

        [Test]
        public void TestCaseEvaluation2()
        {
            var directive = new IfElseDirective("MAYBE $", "OTHERWISE", "DONE", TypeConverter);

            Assert.AreEqual("yes", Evaluate("{MAYBE \"text\"}yes{OTHERWISE}no{DONE}", directive));
        }

        [Test]
        public void TestCaseEvaluation3()
        {
            var directive = new IfElseDirective("MAYBE $", "OTHERWISE", "DONE", TypeConverter);

            Assert.AreEqual("no", Evaluate("{MAYBE \"\"}yes{OTHERWISE}no{DONE}", directive));
        }

        [Test]
        public void TestCaseEvaluation4()
        {
            var directive = new IfElseDirective("MAYBE $", "OTHERWISE",  "DONE", TypeConverter);

            Assert.AreEqual("no", Evaluate("{MAYBE 0}yes{OTHERWISE}no{DONE}", directive));
        }

        [Test]
        public void TestCaseEvaluation5()
        {
            var directive = new IfElseDirective("MAYBE $", "OTHERWISE", "DONE", TypeConverter);

            Assert.AreEqual("no", Evaluate("{MAYBE undefined}yes{OTHERWISE}no{DONE}", directive, new KeyValuePair<String, Object>("undefined", null)));
        }

        [Test]
        public void TestCaseEvaluation6()
        {
            var directive = new IfElseDirective("MAYBE $", "OTHERWISE", "DONE", TypeConverter);

            Assert.AreEqual("yes", Evaluate("{MAYBE defined}yes{OTHERWISE}no{DONE}", directive, new KeyValuePair<String, Object>("defined", this)));
        }
    }
}

