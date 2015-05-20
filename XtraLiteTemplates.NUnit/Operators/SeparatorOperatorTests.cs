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
    public class SeparatorOperatorTests : OperatorTestsBase
    {
        [Test]
        public void TestCaseConstruction1()
        {
            ExpectArgumentNullException("symbol", () => new SeparatorOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new SeparatorOperator(String.Empty, TypeConverter));
            ExpectArgumentNullException("typeConverter", () => new SeparatorOperator("operator", null));
            ExpectArgumentNullException("typeConverter", () => new SeparatorOperator(null));
        }

        [Test]
        public void TestCaseConstruction2()
        {
            var @operator = new SeparatorOperator(TypeConverter);

            Assert.AreEqual(",", @operator.Symbol);
        }

        [Test]
        public void TestCaseConstruction3()
        {
            var @operator = new SeparatorOperator("operator", TypeConverter);

            Assert.AreEqual("operator", @operator.Symbol);
            Assert.AreEqual(Int32.MaxValue - 1, @operator.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, @operator.Associativity);
            Assert.AreEqual(false, @operator.ExpectLhsIdentifier);
            Assert.AreEqual(false, @operator.ExpectRhsIdentifier);
        }

        [Test]
        public void TestCaseEvaluationExceptions()
        {
            var @operator = new SeparatorOperator(TypeConverter);

            Object dummy;
            ExpectArgumentNullException("context", () => @operator.Evaluate(null, 1, 2));
            ExpectArgumentNullException("context", () => @operator.EvaluateLhs(null, 1, out dummy));
        }

        [Test]
        public void TestCaseEvaluation()
        {
            var @operator = new SeparatorOperator(TypeConverter);

            var group1 = @operator.Evaluate(EmptyEvaluationContext, 1, 2);
            var group2 = @operator.Evaluate(EmptyEvaluationContext, group1, 3);
            var group3 = @operator.Evaluate(EmptyEvaluationContext, group2, new Int32[] { 4 });

            Func<IEnumerable, Object[]> extract = (enumerable) =>
            {
                var list = new List<Object>();
                foreach (var o in enumerable)
                    list.Add(o);
                return list.ToArray();
            };

            Assert.IsInstanceOf<IEnumerable>(group1);
            var items = extract(group1 as IEnumerable);
            Assert.AreEqual(2, items.Length);
            Assert.AreEqual(1, items[0]);
            Assert.AreEqual(2, items[1]);

            Assert.IsInstanceOf<IEnumerable>(group2);
            items = extract(group2 as IEnumerable);
            Assert.AreEqual(3, items.Length);
            Assert.AreEqual(1, items[0]);
            Assert.AreEqual(2, items[1]);
            Assert.AreEqual(3, items[2]);

            Assert.IsInstanceOf<IEnumerable>(group3);
            items = extract(group3 as IEnumerable);
            Assert.AreEqual(4, items.Length);
            Assert.AreEqual(1, items[0]);
            Assert.AreEqual(2, items[1]);
            Assert.AreEqual(3, items[2]);

            Assert.IsInstanceOf<Int32[]>(items[3]);
            Assert.AreEqual(4, ((Int32[])items[3])[0]);
        }
    }
}

