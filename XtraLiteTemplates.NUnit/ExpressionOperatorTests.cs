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
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions.Operators;

    [TestFixture]
    public class ExpressionOperatorTests : TestBase
    {
        private void AssertEvaluation<T, R>(UnaryOperator @operator, T arg, R expected)
        {
            Object result = @operator.Evaluate(arg);
            Assert.IsInstanceOf<R>(result);
            Assert.AreEqual(expected, result);
        }

        private void AssertEvaluation<T>(UnaryOperator @operator, T arg, T expected)
        {
            AssertEvaluation<T, T>(@operator, arg, expected);
        }

        private void AssertEvaluation<T, R>(BinaryOperator @operator, T left, T right, R expected)
        {
            Object result = @operator.Evaluate(left, right);
            Assert.IsInstanceOf<R>(result);
            Assert.AreEqual(expected, result);
        }

        private void AssertEvaluation<T>(BinaryOperator @operator, T left, T right, T expected)
        {
            AssertEvaluation<T, T>(@operator, left, right, expected);
        }


        [Test]
        public void TestCaseStandardOperatorSubscript()
        {
            ExpectArgumentEmptyException("symbol", () => new SubscriptOperator(null, ")"));
            ExpectArgumentEmptyException("terminator", () => new SubscriptOperator("(", null));
            ExpectArgumentEmptyException("symbol", () => new SubscriptOperator(null, null));
            ExpectArgumentEmptyException("symbol", () => new SubscriptOperator(String.Empty, ")"));
            ExpectArgumentEmptyException("terminator", () => new SubscriptOperator("(", String.Empty));
            ExpectArgumentEmptyException("symbol", () => new SubscriptOperator(String.Empty, String.Empty));
            ExpectArgumentsEqualException("symbol", "terminator", () => new SubscriptOperator("same", "same"));

            var standard = new SubscriptOperator();
            Assert.AreEqual("(", standard.Symbol);
            Assert.AreEqual(")", standard.Terminator);

            var op = new SubscriptOperator("start", "end");
            Assert.AreEqual("start", op.Symbol);
            Assert.AreEqual("end", op.Terminator);
            Assert.AreEqual(Int32.MaxValue, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
        }

        [Test]
        public void TestCaseStandardOperatorMemberAccess()
        {
            ExpectArgumentEmptyException("symbol", () => new MemberAccessOperator(null, StringComparer.Ordinal));
            ExpectArgumentEmptyException("symbol", () => new MemberAccessOperator(String.Empty, StringComparer.Ordinal));
            ExpectArgumentNullException("comparer", () => new MemberAccessOperator(".", null));

            var standard = new MemberAccessOperator(StringComparer.Ordinal);
            Assert.AreEqual(".", standard.Symbol);
            Assert.AreEqual(StringComparer.Ordinal, standard.Comparer);

            var op = new MemberAccessOperator("operator", StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(0, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(StringComparer.InvariantCultureIgnoreCase, op.Comparer);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(true, op.ExpectRhsIdentifier);

            Assert.AreEqual(10, op.Evaluate("1234567890", "Length"));
            Assert.AreEqual(100, op.Evaluate(Tuple.Create(100), "Item1"));
        }

        [Test]
        public void TestCaseStandardOperatorIntegerRange()
        {
            ExpectArgumentNullException("symbol", () => new IntegerRangeOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new IntegerRangeOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new IntegerRangeOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new IntegerRangeOperator(null));

            var standard = new IntegerRangeOperator(CreateTypeConverter());
            Assert.AreEqual(":", standard.Symbol);

            var op = new IntegerRangeOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(2, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            var enumerable = op.Evaluate(1.8, 2.1) as IEnumerable<Int32>;
            Assert.IsNotNull(enumerable);
            Assert.AreEqual("1,2", String.Join(",", enumerable));

            enumerable = op.Evaluate(1.1, 1.8) as IEnumerable<Int32>;
            Assert.IsNotNull(enumerable);
            Assert.AreEqual("1", String.Join(",", enumerable));

            enumerable = op.Evaluate(2, 1) as IEnumerable<Int32>;
            Assert.IsNull(enumerable);
        }

        [Test]
        public void TestCaseStandardOperatorLogicalAnd()
        {
            ExpectArgumentNullException("symbol", () => new LogicalAndOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new LogicalAndOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalAndOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalAndOperator(null));

            var standard = new LogicalAndOperator(CreateTypeConverter());
            Assert.AreEqual("&&", standard.Symbol);

            var op = new LogicalAndOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(11, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Boolean>(op, true, true, true);
            AssertEvaluation<Boolean>(op, true, false, false);
            AssertEvaluation<Boolean>(op, false, true, false);
            AssertEvaluation<Boolean>(op, false, false, false);

            Object result;
            Assert.IsFalse(op.EvaluateLhs(true, out result));
            Assert.IsTrue(op.EvaluateLhs(false, out result) && result.Equals(false));
        }

        [Test]
        public void TestCaseStandardOperatorLogicalOr()
        {
            ExpectArgumentNullException("symbol", () => new LogicalOrOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new LogicalOrOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalOrOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalOrOperator(null));

            var standard = new LogicalOrOperator(CreateTypeConverter());
            Assert.AreEqual("||", standard.Symbol);

            var op = new LogicalOrOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(12, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Boolean>(op, true, true, true);
            AssertEvaluation<Boolean>(op, true, false, true);
            AssertEvaluation<Boolean>(op, false, true, true);
            AssertEvaluation<Boolean>(op, false, false, false);

            Object result;
            Assert.IsFalse(op.EvaluateLhs(false, out result));
            Assert.IsTrue(op.EvaluateLhs(true, out result) && result.Equals(true));
        }

        [Test]
        public void TestCaseStandardOperatorLogicalNot()
        {
            ExpectArgumentNullException("symbol", () => new LogicalNotOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new LogicalNotOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalNotOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalNotOperator(null));

            var standard = new LogicalNotOperator(CreateTypeConverter());
            Assert.AreEqual("!", standard.Symbol);

            var op = new LogicalNotOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Boolean>(op, true, false);
            AssertEvaluation<Boolean>(op, false, true);
        }


        [Test]
        public void TestCaseStandardOperatorBitwiseNot()
        {
            ExpectArgumentNullException("symbol", () => new BitwiseNotOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new BitwiseNotOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseNotOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseNotOperator(null));

            var standard = new BitwiseNotOperator(CreateTypeConverter());
            Assert.AreEqual("~", standard.Symbol);

            var op = new BitwiseNotOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int32>(op, 1, ~1);
            AssertEvaluation<Int32>(op, 0, ~0);
            AssertEvaluation<Int32>(op, Int32.MinValue, ~Int32.MinValue);
        }

        [Test]
        public void TestCaseStandardOperatorBitwiseXor()
        {
            ExpectArgumentNullException("symbol", () => new BitwiseXorOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new BitwiseXorOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseXorOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseXorOperator(null));

            var standard = new BitwiseXorOperator(CreateTypeConverter());
            Assert.AreEqual("^", standard.Symbol);

            var op = new BitwiseXorOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(9, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int32>(op, 0xAA00CC, 0x00DD00, 0xAADDCC);
            AssertEvaluation<Int32>(op, 0x7FFFFFFF, 0, 0x7FFFFFFF);
            AssertEvaluation<Int32>(op, 1, 2, 3);
            AssertEvaluation<Int32>(op, 0xCC, 5, 0xC9);
        }

        [Test]
        public void TestCaseStandardOperatorBitwiseAnd()
        {
            ExpectArgumentNullException("symbol", () => new BitwiseAndOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new BitwiseAndOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseAndOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseAndOperator(null));

            var standard = new BitwiseAndOperator(CreateTypeConverter());
            Assert.AreEqual("&", standard.Symbol);

            var op = new BitwiseAndOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(9, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int32>(op, 0xEEAAFF, 0xFF0000, 0xEE0000);
            AssertEvaluation<Int32>(op, 0x7FFFFFFF, 0, 0);
            AssertEvaluation<Int32>(op, 1, 2, 0);
            AssertEvaluation<Int32>(op, 3, 2, 2);
        }

        [Test]
        public void TestCaseStandardOperatorBitwiseOr()
        {
            ExpectArgumentNullException("symbol", () => new BitwiseOrOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new BitwiseOrOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseOrOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseOrOperator(null));

            var standard = new BitwiseOrOperator(CreateTypeConverter());
            Assert.AreEqual("|", standard.Symbol);

            var op = new BitwiseOrOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(10, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int32>(op, 0x00BBCC, 0xDD0000, 0xDDBBCC);
            AssertEvaluation<Int32>(op, 0x7FFFFFFF, 0, 0x7FFFFFFF);
            AssertEvaluation<Int32>(op, 1, 2, 3);
            AssertEvaluation<Int32>(op, 3, 2, 3);
        }

        [Test]
        public void TestCaseStandardOperatorBitwiseShiftLeft()
        {
            ExpectArgumentNullException("symbol", () => new BitwiseShiftLeftOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new BitwiseShiftLeftOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseShiftLeftOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseShiftLeftOperator(null));

            var standard = new BitwiseShiftLeftOperator(CreateTypeConverter());
            Assert.AreEqual("<<", standard.Symbol);

            var op = new BitwiseShiftLeftOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(5, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int32>(op, 0xFF, 4, 0x0FF0);
            AssertEvaluation<Int32>(op, 0x10, 64, 0x10);
            AssertEvaluation<Int32>(op, 1, 2, 4);
        }

        [Test]
        public void TestCaseStandardOperatorBitwiseShiftRight()
        {
            ExpectArgumentNullException("symbol", () => new BitwiseShiftRightOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new BitwiseShiftRightOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseShiftRightOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseShiftRightOperator(null));

            var standard = new BitwiseShiftRightOperator(CreateTypeConverter());
            Assert.AreEqual(">>", standard.Symbol);

            var op = new BitwiseShiftRightOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(5, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int32>(op, 0xFFAA, 4, 0x0FFA);
            AssertEvaluation<Int32>(op, 0x10, 64, 0x10);
            AssertEvaluation<Int32>(op, 4, 1, 2);
        }



        [Test]
        public void TestCaseStandardOperatorArithmeticSubtract()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticSubtractOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticSubtractOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticSubtractOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticSubtractOperator(null));

            var standard = new ArithmeticSubtractOperator(CreateTypeConverter());
            Assert.AreEqual("-", standard.Symbol);

            var op = new ArithmeticSubtractOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(4, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Double>(op, 0, 100, -100);
            AssertEvaluation<Double>(op, 0, 0, 0);
            AssertEvaluation<Double>(op, -1, -2, 1);
        }

        [Test]
        public void TestCaseStandardOperatorArithmeticSum()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticSumOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticSumOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticSumOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticSumOperator(null));

            var standard = new ArithmeticSumOperator(CreateTypeConverter());
            Assert.AreEqual("+", standard.Symbol);

            var op = new ArithmeticSumOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(4, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Double>(op, 0, 100, 100);
            AssertEvaluation<Double>(op, 0, 0, 0);
            AssertEvaluation<Double>(op, -1, -2, -3);

            AssertEvaluation<String>(op, "Hello ", "World", "Hello World");
        }

        [Test]
        public void TestCaseStandardOperatorArithmeticDivide()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticDivideOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticDivideOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticDivideOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticDivideOperator(null));

            var standard = new ArithmeticDivideOperator(CreateTypeConverter());
            Assert.AreEqual("/", standard.Symbol);

            var op = new ArithmeticDivideOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(3, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Double>(op, Int64.MaxValue, Int64.MaxValue, 1);
            AssertEvaluation<Double>(op, Int64.MaxValue, 1, Int64.MaxValue);
            AssertEvaluation<Double>(op, 1, 2, (1.00 / 2.00));
            AssertEvaluation<Double>(op, 5, -3, (5.00 / -3.00));
            AssertEvaluation<Double>(op, 1, 0, Double.PositiveInfinity);
            AssertEvaluation<Double>(op, -1, 0, Double.NegativeInfinity);
            AssertEvaluation<Double>(op, 0, 0, Double.NaN);
        }

        [Test]
        public void TestCaseStandardOperatorArithmeticModulo()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticModuloOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticModuloOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticModuloOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticModuloOperator(null));

            var standard = new ArithmeticModuloOperator(CreateTypeConverter());
            Assert.AreEqual("%", standard.Symbol);

            var op = new ArithmeticModuloOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(3, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Double, Int32>(op, 1.8, 2, 1);
            AssertEvaluation<Double, Int32>(op, -5.5, 3, -2);
            AssertEvaluation<Double, Int32>(op, Int32.MaxValue, Int32.MaxValue, 0);
        }

        [Test]
        public void TestCaseStandardOperatorArithmeticMultiply()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticMultiplyOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticMultiplyOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticMultiplyOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticMultiplyOperator(null));

            var standard = new ArithmeticMultiplyOperator(CreateTypeConverter());
            Assert.AreEqual("*", standard.Symbol);

            var op = new ArithmeticMultiplyOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(3, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Double>(op, 1, Int64.MaxValue, Int64.MaxValue);
            AssertEvaluation<Double>(op, Int64.MinValue, 0, 0);
            AssertEvaluation<Double>(op, 1.5, 2, 1.50 * 2.00);
            AssertEvaluation<Double>(op, 0.33, 0, 0.33 * 0);
            AssertEvaluation<Double>(op, -100, 0.5, -100 * 0.5);
        }

        [Test]
        public void TestCaseStandardOperatorArithmeticNegate()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticNegateOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticNegateOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticNegateOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticNegateOperator(null));

            var standard = new ArithmeticNegateOperator(CreateTypeConverter());
            Assert.AreEqual("-", standard.Symbol);

            var op = new ArithmeticNegateOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Double>(op, 1.33, -1.33);
            AssertEvaluation<Double>(op, 0, 0);
        }

        [Test]
        public void TestCaseStandardOperatorArithmeticNeutral()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticNeutralOperator(null, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticNeutralOperator(String.Empty, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticNeutralOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticNeutralOperator(null));

            var standard = new ArithmeticNeutralOperator(CreateTypeConverter());
            Assert.AreEqual("+", standard.Symbol);

            var op = new ArithmeticNeutralOperator("operator", CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Double>(op, 0, 0);
            AssertEvaluation<Double>(op, Double.NaN, Double.NaN);
            AssertEvaluation<Double>(op, -Double.NaN, -Double.NaN);
        }



        [Test]
        public void TestCaseStandardOperatorRelationalEquals()
        {
            ExpectArgumentNullException("symbol", () => new RelationalEqualsOperator(null, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new RelationalEqualsOperator(String.Empty, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentNullException("stringComparer", () => new RelationalEqualsOperator("operator", null, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalEqualsOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalEqualsOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new RelationalEqualsOperator(null, CreateTypeConverter()));

            var standard = new RelationalEqualsOperator(CreateTypeConverter());
            Assert.AreEqual("==", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new RelationalEqualsOperator("operator", StringComparer.Ordinal, CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(7, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, op.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, 0, false);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, true);
            AssertEvaluation<Double, Boolean>(op, 3.33, 3.34, false);

            AssertEvaluation<String, Boolean>(op, "Hello", "Hello", true);
            AssertEvaluation<String, Boolean>(op, "world", "WORLD", false);

            AssertEvaluation<Boolean, Boolean>(op, false, false, true);
            AssertEvaluation<Boolean, Boolean>(op, true, false, false);
        }

        [Test]
        public void TestCaseStandardOperatorRelationalNotEquals()
        {
            ExpectArgumentNullException("symbol", () => new RelationalNotEqualsOperator(null, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new RelationalNotEqualsOperator(String.Empty, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentNullException("stringComparer", () => new RelationalNotEqualsOperator("operator", null, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalNotEqualsOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalNotEqualsOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new RelationalNotEqualsOperator(null, CreateTypeConverter()));

            var standard = new RelationalNotEqualsOperator(CreateTypeConverter());
            Assert.AreEqual("!=", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new RelationalNotEqualsOperator("operator", StringComparer.Ordinal, CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(7, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, op.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, 0, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, false);
            AssertEvaluation<Double, Boolean>(op, 3.33, 3.34, true);

            AssertEvaluation<String, Boolean>(op, "Hello", "Hello", false);
            AssertEvaluation<String, Boolean>(op, "world", "WORLD", true);

            AssertEvaluation<Boolean, Boolean>(op, false, false, false);
            AssertEvaluation<Boolean, Boolean>(op, true, false, true);
        }

        [Test]
        public void TestCaseStandardOperatorRelationalGreaterThan()
        {
            ExpectArgumentNullException("symbol", () => new RelationalGreaterThanOperator(null, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new RelationalGreaterThanOperator(String.Empty, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentNullException("stringComparer", () => new RelationalGreaterThanOperator("operator", null, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalGreaterThanOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalGreaterThanOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new RelationalGreaterThanOperator(null, CreateTypeConverter()));

            var standard = new RelationalGreaterThanOperator(CreateTypeConverter());
            Assert.AreEqual(">", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new RelationalGreaterThanOperator("operator", StringComparer.Ordinal, CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, op.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, 0, Int64.MinValue, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, false);
            AssertEvaluation<Double, Boolean>(op, 3.34, 3.33, true);
        }

        [Test]
        public void TestCaseStandardOperatorRelationalGreaterThanOrEquals()
        {
            ExpectArgumentNullException("symbol", () => new RelationalGreaterThanOrEqualsOperator(null, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new RelationalGreaterThanOrEqualsOperator(String.Empty, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentNullException("stringComparer", () => new RelationalGreaterThanOrEqualsOperator("operator", null, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalGreaterThanOrEqualsOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalGreaterThanOrEqualsOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new RelationalGreaterThanOrEqualsOperator(null, CreateTypeConverter()));

            var standard = new RelationalGreaterThanOrEqualsOperator(CreateTypeConverter());
            Assert.AreEqual(">=", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new RelationalGreaterThanOrEqualsOperator("operator", StringComparer.Ordinal, CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, op.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, 0, Int64.MinValue, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, true);
            AssertEvaluation<Double, Boolean>(op, -0.1, 0, false);
            AssertEvaluation<Double, Boolean>(op, 3.34, 3.33, true);

        }

        [Test]
        public void TestCaseStandardOperatorRelationalLowerThan()
        {
            ExpectArgumentNullException("symbol", () => new RelationalLowerThanOperator(null, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new RelationalLowerThanOperator(String.Empty, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentNullException("stringComparer", () => new RelationalLowerThanOperator("operator", null, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalLowerThanOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalLowerThanOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new RelationalLowerThanOperator(null, CreateTypeConverter()));

            var standard = new RelationalLowerThanOperator(CreateTypeConverter());
            Assert.AreEqual("<", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new RelationalLowerThanOperator("operator", StringComparer.Ordinal, CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, op.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, 0, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, false);
            AssertEvaluation<Double, Boolean>(op, 3.33, 3.34, true);
        }

        [Test]
        public void TestCaseStandardOperatorRelationalLowerThanOrEquals()
        {
            ExpectArgumentNullException("symbol", () => new RelationalLowerThanOrEqualsOperator(null, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new RelationalLowerThanOrEqualsOperator(String.Empty, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentNullException("stringComparer", () => new RelationalLowerThanOrEqualsOperator("operator", null, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalLowerThanOrEqualsOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalLowerThanOrEqualsOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new RelationalLowerThanOrEqualsOperator(null, CreateTypeConverter()));

            var standard = new RelationalLowerThanOrEqualsOperator(CreateTypeConverter());
            Assert.AreEqual("<=", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new RelationalLowerThanOrEqualsOperator("operator", StringComparer.Ordinal, CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, op.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, 0, Int64.MinValue, false);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, true);
            AssertEvaluation<Double, Boolean>(op, -0.1, 0, true);
            AssertEvaluation<Double, Boolean>(op, 3.34, 3.33, false);
        }
    }
}

