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
    using System.Globalization;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Expressions.Operators.Standard;

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



        private IPrimitiveTypeConverter TypeConverter
        {
            get
            {
                return new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture);
            }
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
        public void TestCaseStandardOperatorLogicalAnd()
        {
            ExpectArgumentNullException("symbol", () => new LogicalAndOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new LogicalAndOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalAndOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalAndOperator(null));

            var standard = new LogicalAndOperator(TypeConverter);
            Assert.AreEqual("&", standard.Symbol);

            var op = new LogicalAndOperator("operator", TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(8, op.Precedence);
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
            ExpectArgumentNullException("symbol", () => new LogicalOrOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new LogicalOrOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalOrOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalOrOperator(null));

            var standard = new LogicalOrOperator(TypeConverter);
            Assert.AreEqual("|", standard.Symbol);

            var op = new LogicalOrOperator("operator", TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(10, op.Precedence);
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
            ExpectArgumentNullException("symbol", () => new LogicalNotOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new LogicalNotOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalNotOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalNotOperator(null));

            var standard = new LogicalNotOperator(TypeConverter);
            Assert.AreEqual("!", standard.Symbol);

            var op = new LogicalNotOperator("operator", TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Boolean>(op, true, false);
            AssertEvaluation<Boolean>(op, false, true);
        }


        [Test]
        public void TestCaseStandardOperatorBitwiseNot()
        {
            ExpectArgumentNullException("symbol", () => new BitwiseNotOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new BitwiseNotOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseNotOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseNotOperator(null));

            var standard = new BitwiseNotOperator(TypeConverter);
            Assert.AreEqual("~", standard.Symbol);

            var op = new BitwiseNotOperator("operator", TypeConverter);
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
            ExpectArgumentNullException("symbol", () => new BitwiseXorOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new BitwiseXorOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseXorOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseXorOperator(null));

            var standard = new BitwiseXorOperator(TypeConverter);
            Assert.AreEqual("^", standard.Symbol);

            var op = new BitwiseXorOperator("operator", TypeConverter);
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
            ExpectArgumentNullException("symbol", () => new BitwiseAndOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new BitwiseAndOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseAndOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseAndOperator(null));

            var standard = new BitwiseAndOperator(TypeConverter);
            Assert.AreEqual("&", standard.Symbol);

            var op = new LogicalAndOperator("operator", TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(8, op.Precedence);
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
            ExpectArgumentNullException("symbol", () => new BitwiseOrOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new BitwiseOrOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseOrOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseOrOperator(null));

            var standard = new BitwiseOrOperator(TypeConverter);
            Assert.AreEqual("|", standard.Symbol);

            var op = new LogicalOrOperator("operator", TypeConverter);
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
            ExpectArgumentNullException("symbol", () => new BitwiseShiftLeftOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new BitwiseShiftLeftOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseShiftLeftOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseShiftLeftOperator(null));

            var standard = new BitwiseShiftLeftOperator(TypeConverter);
            Assert.AreEqual("<<", standard.Symbol);

            var op = new BitwiseShiftLeftOperator("operator", TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(5, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int32>(op, 0xFF, 4L, 0x0FF0);
            AssertEvaluation<Int32>(op, 0x10, 64, 0x10);
            AssertEvaluation<Int32>(op, 1, 2, 4);
        }

        [Test]
        public void TestCaseStandardOperatorBitwiseShiftRight()
        {
            ExpectArgumentNullException("symbol", () => new BitwiseShiftRightOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new BitwiseShiftRightOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseShiftRightOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new BitwiseShiftRightOperator(null));

            var standard = new BitwiseShiftRightOperator(TypeConverter);
            Assert.AreEqual(">>", standard.Symbol);

            var op = new BitwiseShiftRightOperator("operator", TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(5, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int32>(op, 0xFFAA, 4L, 0x0FFA);
            AssertEvaluation<Int32>(op, 0x10, 64, 0x10);
            AssertEvaluation<Int32>(op, 4, 1, 2);
        }



        [Test]
        public void TestCaseStandardOperatorArithmeticSubtract()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticSubtractOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticSubtractOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticSubtractOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticSubtractOperator(null));

            var standard = new ArithmeticSubtractOperator(TypeConverter);
            Assert.AreEqual("-", standard.Symbol);

            var op = new ArithmeticSubtractOperator("operator", TypeConverter);
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
            ExpectArgumentNullException("symbol", () => new ArithmeticSumOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticSumOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticSumOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticSumOperator(null));

            var standard = new ArithmeticSumOperator(TypeConverter);
            Assert.AreEqual("+", standard.Symbol);

            var op = new ArithmeticSumOperator("operator", TypeConverter);
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
            ExpectArgumentNullException("symbol", () => new ArithmeticDivideOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticDivideOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticDivideOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticDivideOperator(null));

            var standard = new ArithmeticDivideOperator(TypeConverter);
            Assert.AreEqual("/", standard.Symbol);

            var op = new ArithmeticDivideOperator("operator", TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(3, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Double>(op, Int64.MaxValue, Int64.MaxValue, 1);
            AssertEvaluation<Double>(op, Int64.MaxValue, 1, Int64.MaxValue);
            AssertEvaluation<Double>(op, 1, 2, (1.00 / 2.00));
            AssertEvaluation<Double>(op, 5, -3, (5.00 / -3.00));
        }

        [Test]
        public void TestCaseStandardOperatorArithmeticModulo()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticModuloOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticModuloOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticModuloOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticModuloOperator(null));

            var standard = new ArithmeticModuloOperator(TypeConverter);
            Assert.AreEqual("%", standard.Symbol);

            var op = new ArithmeticModuloOperator("operator", TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(3, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Double>(op, 1.8, 2, 1);
            AssertEvaluation<Double>(op, -5.5, 3, -2);
            AssertEvaluation<Double>(op, Int32.MaxValue, Int32.MaxValue, 0);
        }

        [Test]
        public void TestCaseStandardOperatorArithmeticMultiply()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticMultiplyOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticMultiplyOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticMultiplyOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticMultiplyOperator(null));

            var standard = new ArithmeticMultiplyOperator(TypeConverter);
            Assert.AreEqual("*", standard.Symbol);

            var op = new ArithmeticMultiplyOperator("operator", TypeConverter);
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
            ExpectArgumentNullException("symbol", () => new ArithmeticNegateOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticNegateOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticNegateOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticNegateOperator(null));

            var standard = new ArithmeticNegateOperator(TypeConverter);
            Assert.AreEqual("-", standard.Symbol);

            var op = new ArithmeticNegateOperator("operator", TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Double>(op, 1.33, -1.33);
            AssertEvaluation<Double>(op, 0, 0);
        }

        [Test]
        public void TestCaseStandardOperatorArithmeticNeutral()
        {
            ExpectArgumentNullException("symbol", () => new ArithmeticNeutralOperator(null, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new ArithmeticNeutralOperator(String.Empty, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticNeutralOperator("operator", null));
            ExpectArgumentEmptyException("typeConverter", () => new ArithmeticNeutralOperator(null));

            var standard = new ArithmeticNeutralOperator(TypeConverter);
            Assert.AreEqual("+", standard.Symbol);

            var op = new ArithmeticNeutralOperator("operator", TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Double>(op, 0, 0);
            AssertEvaluation<Double>(op, Double.NaN, Double.NaN);
            AssertEvaluation<Double>(op, -Double.NaN, -Double.NaN);
        }



        [Test]
        public void TestCaseStandardOperatorLogicalEquals()
        {
            ExpectArgumentNullException("symbol", () => new LogicalEqualsOperator(null, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new LogicalEqualsOperator(String.Empty, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentNullException("stringComparer", () => new LogicalEqualsOperator("operator", null, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalEqualsOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalEqualsOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new LogicalEqualsOperator(null, TypeConverter));

            var standard = new LogicalEqualsOperator(TypeConverter);
            Assert.AreEqual("==", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new LogicalEqualsOperator("operator", StringComparer.Ordinal, TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(7, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, standard.StringComparer);

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
        public void TestCaseStandardOperatorLogicalNotEquals()
        {
            ExpectArgumentNullException("symbol", () => new LogicalNotEqualsOperator(null, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new LogicalNotEqualsOperator(String.Empty, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentNullException("stringComparer", () => new LogicalNotEqualsOperator("operator", null, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalNotEqualsOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalNotEqualsOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new LogicalNotEqualsOperator(null, TypeConverter));

            var standard = new LogicalNotEqualsOperator(TypeConverter);
            Assert.AreEqual("!=", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new LogicalNotEqualsOperator("operator", StringComparer.Ordinal, TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(7, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, standard.StringComparer);

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
        public void TestCaseStandardOperatorLogicalGreaterThan()
        {
            ExpectArgumentNullException("symbol", () => new LogicalGreaterThanOperator(null, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new LogicalGreaterThanOperator(String.Empty, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentNullException("stringComparer", () => new LogicalGreaterThanOperator("operator", null, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalGreaterThanOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalGreaterThanOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new LogicalGreaterThanOperator(null, TypeConverter));

            var standard = new LogicalGreaterThanOperator(TypeConverter);
            Assert.AreEqual(">", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new LogicalGreaterThanOperator("operator", StringComparer.Ordinal, TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, standard.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, 0, Int64.MinValue, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, false);
            AssertEvaluation<Double, Boolean>(op, 3.34, 3.33, true);
        }

        [Test]
        public void TestCaseStandardOperatorLogicalGreaterThanOrEquals()
        {
            ExpectArgumentNullException("symbol", () => new LogicalGreaterThanOrEqualsOperator(null, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new LogicalGreaterThanOrEqualsOperator(String.Empty, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentNullException("stringComparer", () => new LogicalGreaterThanOrEqualsOperator("operator", null, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalGreaterThanOrEqualsOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalGreaterThanOrEqualsOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new LogicalGreaterThanOrEqualsOperator(null, TypeConverter));

            var standard = new LogicalGreaterThanOrEqualsOperator(TypeConverter);
            Assert.AreEqual(">=", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new LogicalGreaterThanOrEqualsOperator("operator", StringComparer.Ordinal, TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, standard.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, 0, Int64.MinValue, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, true);
            AssertEvaluation<Double, Boolean>(op, -0.1, 0, false);
            AssertEvaluation<Double, Boolean>(op, 3.34, 3.33, true);

        }

        [Test]
        public void TestCaseStandardOperatorLogicalLowerThan()
        {
            ExpectArgumentNullException("symbol", () => new LogicalLowerThanOperator(null, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new LogicalLowerThanOperator(String.Empty, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentNullException("stringComparer", () => new LogicalLowerThanOperator("operator", null, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalLowerThanOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalLowerThanOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new LogicalLowerThanOperator(null, TypeConverter));

            var standard = new LogicalLowerThanOperator(TypeConverter);
            Assert.AreEqual("<", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new LogicalLowerThanOperator("operator", StringComparer.Ordinal, TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, standard.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, 0, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, false);
            AssertEvaluation<Double, Boolean>(op, 3.33, 3.34, true);
        }

        [Test]
        public void TestCaseStandardOperatorLogicalLowerThanOrEquals()
        {
            ExpectArgumentNullException("symbol", () => new LogicalLowerThanOrEqualsOperator(null, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentEmptyException("symbol", () => new LogicalLowerThanOrEqualsOperator(String.Empty, StringComparer.Ordinal, TypeConverter));
            ExpectArgumentNullException("stringComparer", () => new LogicalLowerThanOrEqualsOperator("operator", null, TypeConverter));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalLowerThanOrEqualsOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new LogicalLowerThanOrEqualsOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new LogicalLowerThanOrEqualsOperator(null, TypeConverter));

            var standard = new LogicalLowerThanOrEqualsOperator(TypeConverter);
            Assert.AreEqual("<=", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new LogicalLowerThanOrEqualsOperator("operator", StringComparer.Ordinal, TypeConverter);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, standard.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, 0, Int64.MinValue, false);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, true);
            AssertEvaluation<Double, Boolean>(op, -0.1, 0, true);
            AssertEvaluation<Double, Boolean>(op, 3.34, 3.33, false);
        }
    }
}

