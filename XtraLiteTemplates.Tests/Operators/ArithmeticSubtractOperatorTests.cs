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
namespace XtraLiteTemplates.Tests.Operators
{
    using System.Diagnostics.CodeAnalysis;
    using global::NUnit.Framework;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions.Operators;

    [TestFixture]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class ArithmeticSubtractOperatorTests : OperatorTestsBase
    {
        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseConstruction1()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticSubtractOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticSubtractOperator(string.Empty, TypeConverter));
            ExpectArgumentNullException("typeConverter", () => new ArithmeticSubtractOperator("operator", null));
            ExpectArgumentNullException("typeConverter", () => new ArithmeticSubtractOperator(null));
        }

        [Test]
        public void TestCaseConstruction2()
        {
            var @operator = new ArithmeticSubtractOperator(TypeConverter);

            Assert.AreEqual("-", @operator.Symbol);
        }

        [Test]
        public void TestCaseConstruction3()
        {
            var @operator = new ArithmeticSubtractOperator("operator", TypeConverter);

            Assert.AreEqual("operator", @operator.Symbol);
            Assert.AreEqual(4, @operator.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, @operator.Associativity);
        }

        [Test]
        public void TestCaseEvaluationExceptions()
        {
            var @operator = new ArithmeticSubtractOperator(TypeConverter);

            ExpectArgumentNullException("context", () => @operator.Evaluate(null, 1, 2));
            ExpectArgumentNullException("context", () => @operator.EvaluateLhs(null, 1, out object dummy));
        }

        [Test]
        public void TestCaseEvaluation()
        {
            var @operator = new ArithmeticSubtractOperator(TypeConverter);

            AssertEvaluation<double>(@operator, 0, 100, -100);
            AssertEvaluation<double>(@operator, 0, 0, 0);
            AssertEvaluation<double>(@operator, -1, -2, 1);
        }
    }
}

