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

namespace XtraLiteTemplates.NUnit.Directives
{
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using global::NUnit.Framework;

    using XtraLiteTemplates.Dialects.Standard.Directives;

    [TestFixture]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class StandardSeparatedForEachDirectiveTests : TestBase
    {
        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseConstructor1()
        {
            ExpectInvalidTagMarkupException(null, () => new SeparatedForEachDirective(null, "MID", "END", TypeConverter));
            ExpectInvalidTagMarkupException(null, () => new SeparatedForEachDirective("START", null, "END", TypeConverter));
            ExpectInvalidTagMarkupException(null, () => new SeparatedForEachDirective("START", "MID", null, TypeConverter));
            ExpectArgumentNullException("typeConverter", () => new SeparatedForEachDirective("START", "MID", "END", null));

            ExpectArgumentConditionNotTrueException("one expression component", () => new SeparatedForEachDirective("?", "MID", "END", TypeConverter));
            ExpectArgumentConditionNotTrueException("one identifier component", () => new SeparatedForEachDirective("$", "MID", "END", TypeConverter));
            ExpectArgumentConditionNotTrueException("one expression component", () => new SeparatedForEachDirective("$ AND $ THEN ?", "MID", "END", TypeConverter));
            ExpectArgumentConditionNotTrueException("one identifier component", () => new SeparatedForEachDirective("? AND ? THEN $", "MID", "END", TypeConverter));
        }

        [Test]
        public void TestCaseConstructor2()
        {
            var directive = new SeparatedForEachDirective(TypeConverter);
            Assert.AreEqual("{FOR EACH ? IN $}...{WITH}...{END}", directive.ToString());
        }

        [Test]
        public void TestCaseConstructor3()
        {
            var directive = new SeparatedForEachDirective("OVER $ GET EACH ?", "MID", "TERMINATE", TypeConverter);
            Assert.AreEqual("{OVER $ GET EACH ?}...{MID}...{TERMINATE}", directive.ToString());
        }

        [Test]
        public void TestCaseEvaluation1()
        {
            var directive = new SeparatedForEachDirective("BOO ? OF $", "MAN", "BEER", TypeConverter);

            Assert.AreEqual("1", Evaluate("{BOO I OF 1}{I}{MAN},{BEER}", directive));
        }

        [Test]
        public void TestCaseEvaluation2()
        {
            var directive = new SeparatedForEachDirective("BOO ? OF $", "MAN", "BEER", TypeConverter);

            Assert.AreEqual(string.Empty, Evaluate("{BOO I OF undefined}{I}{MAN},{BEER}", directive, new KeyValuePair<string, object>("undefined", null)));
        }

        [Test]
        public void TestCaseEvaluation3()
        {
            var directive = new SeparatedForEachDirective("BOO ? OF $", "MAN", "BEER", TypeConverter);

            Assert.AreEqual("1,2,3,4,5,6,7,8,9", Evaluate("{BOO I OF \"123456789\"}{I}{MAN},{BEER}", directive));
        }

        [Test]
        public void TestCaseEvaluation4()
        {
            var directive = new SeparatedForEachDirective("BOO ? OF $", "MAN", "BEER", TypeConverter);

            Assert.AreEqual(string.Empty, Evaluate("{BOO I OF \"\"}{I}{MAN},{BEER}", directive));
        }

        [Test]
        public void TestCaseEvaluation5()
        {
            var directive = new SeparatedForEachDirective("BOO ? OF $", "MAN", "BEER", TypeConverter);

            Assert.AreEqual("10,20,30", Evaluate("{BOO I OF list}{I}{MAN},{BEER}", directive, new KeyValuePair<string, object>("list", new[] { 10, 20, 30 })));
        }
    }
}

