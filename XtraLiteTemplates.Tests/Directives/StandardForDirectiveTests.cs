﻿//
//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2018, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.Tests.Directives
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using global::NUnit.Framework;
    using XtraLiteTemplates.Dialects.Standard.Directives;

    [TestFixture]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class StandardForDirectiveTests : TestBase
    {
        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseConstructor1()
        {
            ExpectInvalidTagMarkupException(null, () => new ForDirective(null, "END", TypeConverter));
            ExpectInvalidTagMarkupException(null, () => new ForDirective("$", null, TypeConverter));
            ExpectArgumentNullException("typeConverter", () => new ForDirective("$", "END", null));

            ExpectArgumentConditionNotTrueException("expressionComponents", () => new ForDirective("?", "END", TypeConverter));
            ExpectArgumentConditionNotTrueException("expressionComponents", () => new ForDirective("$ AND $ THEN ?", "END", TypeConverter));
        }

        [Test]
        public void TestCaseConstructor2()
        {
            var directive = new ForDirective(TypeConverter);
            Assert.AreEqual("{FOR $}...{END}", directive.ToString());
        }

        [Test]
        public void TestCaseConstructor3()
        {
            var directive = new ForDirective("OVER $", "TERMINATE", TypeConverter);
            Assert.AreEqual("{OVER $}...{TERMINATE}", directive.ToString());
        }

        [Test]
        public void TestCaseEvaluation1()
        {
            var directive = new ForDirective("BOO $", "BEER", TypeConverter);

            Assert.AreEqual(".", Evaluate("{BOO 1}.{BEER}", directive));
        }

        [Test]
        public void TestCaseEvaluation2()
        {
            var directive = new ForDirective("BOO $", "BEER", TypeConverter);

            Assert.AreEqual(string.Empty, Evaluate("{BOO undefined}.{BEER}", directive, new KeyValuePair<string, object>("undefined", null)));
        }

        [Test]
        public void TestCaseEvaluation3()
        {
            var directive = new ForDirective("BOO $", "BEER", TypeConverter);

            Assert.AreEqual(".........", Evaluate("{BOO \"123456789\"}.{BEER}", directive));
        }

        [Test]
        public void TestCaseEvaluation4()
        {
            var directive = new ForDirective("BOO $", "BEER", TypeConverter);

            Assert.AreEqual(string.Empty, Evaluate("{BOO \"\"}.{BEER}", directive));
        }

        [Test]
        public void TestCaseEvaluation5()
        {
            var directive = new ForDirective("BOO $", "BEER", TypeConverter);

            Assert.AreEqual("...", Evaluate("{BOO list}.{BEER}", directive, new KeyValuePair<string, object>("list", new[] { 1, 2, 3 })));
        }
    }
}

