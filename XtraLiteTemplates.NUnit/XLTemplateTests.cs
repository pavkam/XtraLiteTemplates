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
//     * Neither the name of the [ORGANIZATION] nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
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
    using XtraLiteTemplates.Expressions.Operators.Standard;
    using XtraLiteTemplates.NUnit.Inside;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Evaluation.Directives.Standard;
    using XtraLiteTemplates.Parsing;
    using System.Globalization;

    [TestFixture]
    public class XLTemplateTests : TestBase
    {
        [Test]
        public void TestCaseEvaluationSingleTagDirective()
        {
            var dialect = new StandardDialect(CultureInfo.CurrentCulture, 
                StringComparer.InvariantCulture);

            var template = new XLTemplate(dialect, "{IF 10 > 2 THEN}YES{END IF}");
            var result = template.Evaluate(new Dictionary<String, Object>());

            Assert.AreEqual("YES", result);
        }

        [Test]
        public void TestCaseEvaluationSingleTagDirective11()
        {
            var result = XLTemplate.Evaluate("{IF 10 > 2 THEN}YES{END IF}");
            Assert.AreEqual("YES", result);
        }

        [Test]
        public void TestCaseEvaluationSingleTagDirective12()
        {
            var result = XLTemplate.Evaluate("{name}'s age is {AGE}!", 
                Tuple.Create<String, Object>("name", "John"),
                Tuple.Create<String, Object>("age", 30));

            Assert.AreEqual("John's age is 30!", result);
        }

        [Test]
        public void TestCaseEvaluationSingleTagDirective132()
        {
            var result = XLTemplate.Evaluate("{if true then}write me{end if}");

            Assert.AreEqual("write me", result);
        }
    }
}

