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

namespace XtraLiteTemplates.NUnit.Operators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions.Operators;

    [TestFixture]
    public class ValueFormatOperatorTests : OperatorTestsBase
    {
        [Test]
        public void TestCaseConstruction1()
        {
            ExpectArgumentNullException("symbol", () => new ValueFormatOperator(null, CultureInfo.CurrentCulture, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new ValueFormatOperator(String.Empty, CultureInfo.CurrentCulture, CreateTypeConverter()));
            ExpectArgumentEmptyException("formatProvider", () => new ValueFormatOperator("operator", null, CreateTypeConverter()));
            ExpectArgumentEmptyException("formatProvider", () => new ValueFormatOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new ValueFormatOperator("operator", CultureInfo.CurrentCulture, null));
            ExpectArgumentEmptyException("typeConverter", () => new ValueFormatOperator(CultureInfo.CurrentCulture, null));
        }

        [Test]
        public void TestCaseConstruction2()
        {
            var @operator = new ValueFormatOperator(CultureInfo.CurrentCulture, CreateTypeConverter());

            Assert.AreEqual(":", @operator.Symbol);
        }

        [Test]
        public void TestCaseConstruction3()
        {
            var @operator = new ValueFormatOperator("operator", CultureInfo.InvariantCulture, CreateTypeConverter());

            Assert.AreEqual("operator", @operator.Symbol);
            Assert.AreEqual(2, @operator.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, @operator.Associativity);
            Assert.AreEqual(false, @operator.ExpectLhsIdentifier);
            Assert.AreEqual(false, @operator.ExpectRhsIdentifier);
            Assert.AreEqual(CultureInfo.InvariantCulture, @operator.FormatProvider);
        }

        [Test]
        public void TestCaseEvaluation()
        {
            var @operator = new ValueFormatOperator("operator", CultureInfo.InvariantCulture, CreateTypeConverter());

            Assert.AreEqual("1.23", @operator.Evaluate(1.23, "N"));
            Assert.IsNull(@operator.Evaluate(null, "N"));
            Assert.IsNull(@operator.Evaluate("Hello", "8742398472984"));
            Assert.IsNull(@operator.Evaluate("Hello", null));

            var now = DateTime.Now;
            Assert.AreEqual(now.ToString("g", CultureInfo.InvariantCulture), @operator.Evaluate(now, "g"));
        }
    }
}

