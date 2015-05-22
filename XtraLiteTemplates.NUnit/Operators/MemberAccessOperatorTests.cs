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
    public class MemberAccessOperatorTests : OperatorTestsBase
    {
        [Test]
        public void TestCaseConstruction1()
        {
            ExpectArgumentNullException("symbol", () => new MemberAccessOperator(null, StringComparer.Ordinal));
            ExpectArgumentEmptyException("symbol", () => new MemberAccessOperator(String.Empty, StringComparer.Ordinal));
            ExpectArgumentNullException("comparer", () => new MemberAccessOperator(".", null));
        }

        [Test]
        public void TestCaseConstruction2()
        {
            var @operator = new MemberAccessOperator(StringComparer.Ordinal);

            Assert.AreEqual(".", @operator.Symbol);
            Assert.AreEqual(StringComparer.Ordinal, @operator.Comparer);
        }

        [Test]
        public void TestCaseConstruction3()
        {
            var @operator = new MemberAccessOperator("operator", StringComparer.InvariantCultureIgnoreCase);

            Assert.AreEqual("operator", @operator.Symbol);
            Assert.AreEqual(0, @operator.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, @operator.Associativity);
            Assert.AreEqual(StringComparer.InvariantCultureIgnoreCase, @operator.Comparer);
            Assert.AreEqual(false, @operator.ExpectLhsIdentifier);
            Assert.AreEqual(true, @operator.ExpectRhsIdentifier);
        }

        [Test]
        public void TestCaseEvaluationExceptions()
        {
            var @operator = new MemberAccessOperator(StringComparer.InvariantCultureIgnoreCase);

            Object dummy;
            ExpectArgumentNullException("context", () => @operator.Evaluate(null, 1, "ident"));
            ExpectArgumentNullException("context", () => @operator.EvaluateLhs(null, 1, out dummy));

            ExpectArgumentNullException("right", () => @operator.Evaluate(EmptyEvaluationContext, this, null));
            ExpectArgumentEmptyException("right", () => @operator.Evaluate(EmptyEvaluationContext, this, String.Empty));
            ExpectArgumentNotIdentifierException("right", () => @operator.Evaluate(EmptyEvaluationContext, this, 11));
            ExpectArgumentNotIdentifierException("right", () => @operator.Evaluate(EmptyEvaluationContext, this, this));
        }

        [Test]
        public void TestCaseEvaluationIgnoreCase()
        {
            var @operator = new MemberAccessOperator(StringComparer.InvariantCultureIgnoreCase);

            Assert.AreEqual(10, @operator.Evaluate(EmptyEvaluationContext, "1234567890", "length"));
            Assert.AreEqual(10, @operator.Evaluate(EmptyEvaluationContext, "1234567890", "Length"));

            Assert.AreEqual(100, @operator.Evaluate(EmptyEvaluationContext, Tuple.Create(100), "Item1"));
            Assert.AreEqual(99, @operator.Evaluate(EmptyEvaluationContext, Tuple.Create(99), "ITEM1"));
        }

        [Test]
        public void TestCaseEvaluation()
        {
            var @operator = new MemberAccessOperator(StringComparer.Ordinal);

            Assert.IsNull(@operator.Evaluate(EmptyEvaluationContext, "1234567890", "length"));
            Assert.AreEqual(10, @operator.Evaluate(EmptyEvaluationContext, "1234567890", "Length"));

            Assert.AreEqual(100, @operator.Evaluate(EmptyEvaluationContext, Tuple.Create(100), "Item1"));
            Assert.IsNull(@operator.Evaluate(EmptyEvaluationContext, Tuple.Create(99), "ITEM1"));
        }
    }
}
