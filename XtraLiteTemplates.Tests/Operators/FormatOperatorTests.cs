//
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

namespace XtraLiteTemplates.Tests.Operators
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using global::NUnit.Framework;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions.Operators;

    [TestFixture]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class FormatOperatorTests : OperatorTestsBase
    {
        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseConstruction1()
        {
            ExpectArgumentNullException("symbol", () => new FormatOperator(null, CultureInfo.CurrentCulture, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new FormatOperator(string.Empty, CultureInfo.CurrentCulture, TypeConverter));
            ExpectArgumentNullException("formatProvider", () => new FormatOperator("operator", null, TypeConverter));
            ExpectArgumentNullException("formatProvider", () => new FormatOperator(null, TypeConverter));
            ExpectArgumentNullException("typeConverter", () => new FormatOperator("operator", CultureInfo.CurrentCulture, null));
            ExpectArgumentNullException("typeConverter", () => new FormatOperator(CultureInfo.CurrentCulture, null));
        }

        [Test]
        public void TestCaseConstruction2()
        {
            var @operator = new FormatOperator(CultureInfo.CurrentCulture, TypeConverter);

            Assert.AreEqual(":", @operator.Symbol);
        }

        [Test]
        public void TestCaseConstruction3()
        {
            var @operator = new FormatOperator("operator", CultureInfo.InvariantCulture, TypeConverter);

            Assert.AreEqual("operator", @operator.Symbol);
            Assert.AreEqual(2, @operator.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, @operator.Associativity);
            Assert.AreEqual(CultureInfo.InvariantCulture, @operator.FormatProvider);
        }

        [Test]
        public void TestCaseEvaluationExceptions()
        {
            var @operator = new FormatOperator(CultureInfo.InvariantCulture, TypeConverter);

            ExpectArgumentNullException("context", () => @operator.Evaluate(null, 1, 2));
            ExpectArgumentNullException("context", () => @operator.EvaluateLhs(null, 1, out object dummy));
        }

        [Test]
        public void TestCaseEvaluation()
        {
            var @operator = new FormatOperator(CultureInfo.InvariantCulture, TypeConverter);

            Assert.AreEqual("1.23", @operator.Evaluate(EmptyEvaluationContext, 1.23, "N"));
            Assert.IsNull(@operator.Evaluate(EmptyEvaluationContext, null, "N"));
            Assert.IsNull(@operator.Evaluate(EmptyEvaluationContext, "Hello", "8742398472984"));
            Assert.IsNull(@operator.Evaluate(EmptyEvaluationContext, "Hello", null));

            var now = DateTime.Now;
            Assert.AreEqual(now.ToString("g", CultureInfo.InvariantCulture), @operator.Evaluate(EmptyEvaluationContext, now, "g"));
        }
    }
}

