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

namespace XtraLiteTemplates.NUnit.Operators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions.Operators;

    [TestFixture]
    public class ArithmeticDivideOperatorTests : OperatorTestsBase
    {
        [Test]
        public void TestCaseConstruction1()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticDivideOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticDivideOperator(String.Empty, TypeConverter));
            ExpectArgumentNullException("typeConverter", () => new ArithmeticDivideOperator("operator", null));
            ExpectArgumentNullException("typeConverter", () => new ArithmeticDivideOperator(null));
        }

        [Test]
        public void TestCaseConstruction2()
        {
            var standard = new ArithmeticDivideOperator(TypeConverter);
            Assert.AreEqual("/", standard.Symbol);
        }

        [Test]
        public void TestCaseConstruction3()
        {
            var @operator = new ArithmeticDivideOperator("operator", TypeConverter);
            Assert.AreEqual("operator", @operator.Symbol);
            Assert.AreEqual(3, @operator.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, @operator.Associativity);
        }

        [Test]
        public void TestCaseEvaluationExceptions()
        {
            var @operator = new ArithmeticDivideOperator(TypeConverter);

            Object dummy;
            ExpectArgumentNullException("context", () => @operator.Evaluate(null, 1, 2));
            ExpectArgumentNullException("context", () => @operator.EvaluateLhs(null, 1, out dummy));
        }

        [Test]
        public void TestCaseEvaluation()
        {
            var @operator = new ArithmeticDivideOperator(TypeConverter);
           
            AssertEvaluation<Double>(@operator, Int64.MaxValue, Int64.MaxValue, 1);
            AssertEvaluation<Double>(@operator, Int64.MaxValue, 1, Int64.MaxValue);
            AssertEvaluation<Double>(@operator, 1, 2, (1.00 / 2.00));
            AssertEvaluation<Double>(@operator, 5, -3, (5.00 / -3.00));
            AssertEvaluation<Double>(@operator, 1, 0, Double.PositiveInfinity);
            AssertEvaluation<Double>(@operator, -1, 0, Double.NegativeInfinity);
            AssertEvaluation<Double>(@operator, 0, 0, Double.NaN);
        }
    }
}

