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
    public class StandardPreFormattedUnparsedTextDirectiveTests : TestBase
    {
        [Test]
        public void TestCaseConstructor1()
        {
            ExpectInvalidTagMarkupException(null, () => new PreFormattedUnparsedTextDirective(null, "END", String.Empty, TypeConverter));
            ExpectInvalidTagMarkupException(null, () => new PreFormattedUnparsedTextDirective("PREF", null, String.Empty, TypeConverter));

            ExpectArgumentNullException("stateObject", () => new PreFormattedUnparsedTextDirective("PREF", "END", null, TypeConverter));
            ExpectArgumentNullException("typeConverter", () => new PreFormattedUnparsedTextDirective("PREF", "END", String.Empty, null));
        }

        [Test]
        public void TestCaseConstructor2()
        {
            var directive = new PreFormattedUnparsedTextDirective(String.Empty, TypeConverter);
            Assert.AreEqual("{PREFORMATTED}...{END}", directive.ToString());
        }

        [Test]
        public void TestCaseConstructor3()
        {
            var directive = new PreFormattedUnparsedTextDirective("PREPRE", "SUPPAPRE", String.Empty, TypeConverter);
            Assert.AreEqual("{PREPRE}...{SUPPAPRE}", directive.ToString());
        }

        [Test]
        public void TestCaseEvaluation1()
        {
            var directive = new PreFormattedUnparsedTextDirective("P", "E", String.Empty, TypeConverter);

            Assert.AreEqual("   1   ", Evaluate("{P}   1   {E}", directive));
            Assert.AreEqual("\n\r1   ", Evaluate("{P}\n\r1   {E}", directive));
        }
    }
}

