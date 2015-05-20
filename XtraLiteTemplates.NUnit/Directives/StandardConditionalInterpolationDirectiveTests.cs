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

    [TestFixture]
    public class StandardConditionalInterpolationDirectiveTests : TestBase
    {
        [Test]
        public void TestCaseConstructor1()
        {
            ExpectInvalidTagMarkupException(null, () => new ConditionalInterpolationDirective(null, false, TypeConverter));
            ExpectArgumentNullException("typeConverter", () => new ConditionalInterpolationDirective("$ __ $", false, null));

            ExpectArgumentConditionNotTrueException("two expression components", () => new ConditionalInterpolationDirective("$ A $ B $", false, TypeConverter));
            ExpectArgumentConditionNotTrueException("two expression components", () => new ConditionalInterpolationDirective("$", false, TypeConverter));
            ExpectArgumentConditionNotTrueException("two expression components", () => new ConditionalInterpolationDirective("A $ B ? C $ D $", false, TypeConverter));
        }

        [Test]
        public void TestCaseConstructor2()
        {
            var directive = new ConditionalInterpolationDirective(TypeConverter);
            Assert.AreEqual("{$ IF $}", directive.ToString());
        }

        [Test]
        public void TestCaseConstructor3()
        {
            var directive = new ConditionalInterpolationDirective("$ THEN $", true, TypeConverter);
            Assert.AreEqual("{$ THEN $}", directive.ToString());
        }

        [Test]
        public void TestCaseEvaluation1()
        {
            var directive = new ConditionalInterpolationDirective("$ IF $", false, TypeConverter);

            Assert.AreEqual("100", Evaluate("{100 IF 1}", directive));
            Assert.AreEqual("", Evaluate("{100 IF 0}", directive));
        }

        [Test]
        public void TestCaseEvaluation2()
        {
            var directive = new ConditionalInterpolationDirective("$ THEN $", true, TypeConverter);

            Assert.AreEqual("100", Evaluate("{1 THEN 100}", directive));
            Assert.AreEqual("", Evaluate("{0 THEN 100}", directive));
        }
    }
}

