﻿//
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
    public class IntegerRangeOperatorTests : OperatorTestsBase
    {
        [Test]
        public void TestCaseConstruction1()
        {
            ExpectArgumentNullException("symbol", () => new IntegerRangeOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new IntegerRangeOperator(String.Empty, TypeConverter));
            ExpectArgumentNullException("typeConverter", () => new IntegerRangeOperator("operator", null));
            ExpectArgumentNullException("typeConverter", () => new IntegerRangeOperator(null));
        }

        [Test]
        public void TestCaseConstruction2()
        {
            var @operator = new IntegerRangeOperator(TypeConverter);

            Assert.AreEqual("..", @operator.Symbol);
        }

        [Test]
        public void TestCaseConstruction3()
        {
            var @operator = new IntegerRangeOperator("operator", TypeConverter);

            Assert.AreEqual("operator", @operator.Symbol);
            Assert.AreEqual(2, @operator.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, @operator.Associativity);
            Assert.AreEqual(false, @operator.ExpectLhsIdentifier);
            Assert.AreEqual(false, @operator.ExpectRhsIdentifier);
        }

        [Test]
        public void TestCaseEvaluationExceptions()
        {
            var @operator = new IntegerRangeOperator(TypeConverter);

            Object dummy;
            ExpectArgumentNullException("context", () => @operator.Evaluate(null, 1, 2));
            ExpectArgumentNullException("context", () => @operator.EvaluateLhs(null, 1, out dummy));
        }

        [Test]
        public void TestCaseEvaluation()
        {
            var @operator = new IntegerRangeOperator(TypeConverter);

            var enumerable = @operator.Evaluate(EmptyEvaluationContext, 1.8, 2.1) as IEnumerable<Int32>;
            Assert.IsNotNull(enumerable);
            Assert.AreEqual("1,2", String.Join(",", enumerable));

            enumerable = @operator.Evaluate(EmptyEvaluationContext, 1.1, 1.8) as IEnumerable<Int32>;
            Assert.IsNotNull(enumerable);
            Assert.AreEqual("1", String.Join(",", enumerable));

            enumerable = @operator.Evaluate(EmptyEvaluationContext, 2, 1) as IEnumerable<Int32>;
            Assert.IsNull(enumerable);
        }
    }
}
