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
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Expressions.Operators.Standard;

    [TestFixture]
    public class ExpressionOperatorTests : TestBase
    {
        private void AssertEvaluation<T>(GroupOperator @operator, T arg, T expected)
        {
            Object result;
            Assert.IsTrue(@operator.Evaluate(arg, out result));
            Assert.IsInstanceOf<T>(result);
            Assert.AreEqual(expected, result);
        }

        private void AssertEvaluation<T, R>(UnaryOperator @operator, T arg, R expected)
        {
            Object result;
            Assert.IsTrue(@operator.Evaluate(arg, out result));
            Assert.IsInstanceOf<R>(result);
            Assert.AreEqual(expected, result);
        }

        private void AssertEvaluation<T>(UnaryOperator @operator, T arg, T expected)
        {
            AssertEvaluation<T, T>(@operator, arg, expected);
        }

        private void AssertEvaluation<T, R>(BinaryOperator @operator, T left, T right, R expected)
        {
            Object result;
            Assert.IsTrue(@operator.Evaluate(left, right, out result));
            Assert.IsInstanceOf<R>(result);
            Assert.AreEqual(expected, result);
        }

        private void AssertEvaluation<T>(BinaryOperator @operator, T left, T right, T expected)
        {
            AssertEvaluation<T, T>(@operator, left, right, expected);
        }

        private void AssertUnsupportedEvaluation<L, R>(BinaryOperator @operator)
        {
            Object result;
            Assert.IsFalse(@operator.Evaluate(default(L), default(R), out result));
            Assert.IsNull(result);
        }

        private void AssertUnsupportedEvaluation<T>(BinaryOperator @operator)
        {
            AssertUnsupportedEvaluation<T, T>(@operator);
        }

        private void AssertUnsupportedEvaluation<T>(UnaryOperator @operator)
        {
            Object result;
            Assert.IsFalse(@operator.Evaluate(default(T), out result));
            Assert.IsNull(result);
        }


        private void AssertUnsupportedLeftEvaluation<L>(BinaryOperator @operator)
        {
            Object result;
            Assert.IsFalse(@operator.Evaluate(default(L), out result));
        }

        private void AssertUnsupportedAnyLeftEvaluation(BinaryOperator @operator)
        {
            AssertUnsupportedLeftEvaluation<Int64>(@operator);
            AssertUnsupportedLeftEvaluation<Boolean>(@operator);
            AssertUnsupportedLeftEvaluation<Double>(@operator);
            AssertUnsupportedLeftEvaluation<String>(@operator);
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

            Assert.NotNull(SubscriptOperator.CStyle);
            Assert.AreEqual("(", SubscriptOperator.CStyle.Symbol);
            Assert.AreEqual(")", SubscriptOperator.CStyle.Terminator);

            Assert.NotNull(SubscriptOperator.PascalStyle);
            Assert.AreEqual("(", SubscriptOperator.PascalStyle.Symbol);
            Assert.AreEqual(")", SubscriptOperator.PascalStyle.Terminator);

            var op = new SubscriptOperator("start", "end");
            Assert.AreEqual("start", op.Symbol);
            Assert.AreEqual("end", op.Terminator);
            Assert.AreEqual(Int32.MaxValue, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 100L, 100L);
            AssertEvaluation<Double>(op, 100.00, 100.00);
            AssertEvaluation<String>(op, "Hello World", "Hello World");
            AssertEvaluation<Boolean>(op, true, true);
        }

        [Test]
        public void TestCaseStandardOperatorAnd()
        {
            ExpectArgumentEmptyException("symbol", () => new AndOperator(null));
            ExpectArgumentEmptyException("symbol", () => new AndOperator(String.Empty));

            Assert.NotNull(AndOperator.CStyle);
            Assert.AreEqual("&", AndOperator.CStyle.Symbol);

            Assert.NotNull(AndOperator.PascalStyle);
            Assert.AreEqual("and", AndOperator.PascalStyle.Symbol);

            var op = new AndOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(8, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 0xEEAABBFF, 0xFF00FF00, 0xEE00BB00);
            AssertEvaluation<Int64>(op, 0xFFFFFFFFFFFF, 0, 0);
            AssertEvaluation<Int64>(op, 1, 2, 0);
            AssertEvaluation<Int64>(op, 3, 2, 2);

            AssertEvaluation<Boolean>(op, true, true, true);
            AssertEvaluation<Boolean>(op, true, false, false);
            AssertEvaluation<Boolean>(op, false, true, false);
            AssertEvaluation<Boolean>(op, false, false, false);

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Double>(op);

            Object result;
            Assert.IsFalse(op.Evaluate(true, out result));
            Assert.IsTrue(op.Evaluate(false, out result) && result.Equals(false));
        }

        [Test]
        public void TestCaseStandardOperatorOr()
        {
            ExpectArgumentEmptyException("symbol", () => new OrOperator(null));
            ExpectArgumentEmptyException("symbol", () => new OrOperator(String.Empty));

            Assert.NotNull(OrOperator.CStyle);
            Assert.AreEqual("|", OrOperator.CStyle.Symbol);

            Assert.NotNull(OrOperator.PascalStyle);
            Assert.AreEqual("or", OrOperator.PascalStyle.Symbol);

            var op = new OrOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(10, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 0xAA00BB00CC, 0x00DD00EE00, 0xAADDBBEECC);
            AssertEvaluation<Int64>(op, 0xFFFFFFFFFFFF, 0, 0xFFFFFFFFFFFF);
            AssertEvaluation<Int64>(op, 1, 2, 3);
            AssertEvaluation<Int64>(op, 3, 2, 3);

            AssertEvaluation<Boolean>(op, true, true, true);
            AssertEvaluation<Boolean>(op, true, false, true);
            AssertEvaluation<Boolean>(op, false, true, true);
            AssertEvaluation<Boolean>(op, false, false, false);

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Double>(op);

            Object result;
            Assert.IsFalse(op.Evaluate(false, out result));
            Assert.IsTrue(op.Evaluate(true, out result) && result.Equals(true));
        }

        [Test]
        public void TestCaseStandardOperatorXor()
        {
            ExpectArgumentEmptyException("symbol", () => new XorOperator(null));
            ExpectArgumentEmptyException("symbol", () => new XorOperator(String.Empty));

            Assert.NotNull(XorOperator.CStyle);
            Assert.AreEqual("^", XorOperator.CStyle.Symbol);

            Assert.NotNull(XorOperator.PascalStyle);
            Assert.AreEqual("xor", XorOperator.PascalStyle.Symbol);

            var op = new XorOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(9, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 0xAA00BB00CC, 0x00DD00EE00, 0xAADDBBEECC);
            AssertEvaluation<Int64>(op, 0xFFFFFFFFFFFF, 0, 0xFFFFFFFFFFFF);
            AssertEvaluation<Int64>(op, 1, 2, 3);
            AssertEvaluation<Int64>(op, 0xCC, 5, 0xC9);

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Double>(op);
            AssertUnsupportedEvaluation<Boolean>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorShiftLeft()
        {
            ExpectArgumentEmptyException("symbol", () => new ShiftLeftOperator(null));
            ExpectArgumentEmptyException("symbol", () => new ShiftLeftOperator(String.Empty));

            Assert.NotNull(ShiftLeftOperator.CStyle);
            Assert.AreEqual("<<", ShiftLeftOperator.CStyle.Symbol);

            Assert.NotNull(ShiftLeftOperator.PascalStyle);
            Assert.AreEqual("shl", ShiftLeftOperator.PascalStyle.Symbol);

            var op = new ShiftLeftOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(5, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 0xFF, 4L, 0x0FF0);
            AssertEvaluation<Int64>(op, 0x10, 64, 0x10);
            AssertEvaluation<Int64>(op, 1, 2, 4);

            AssertUnsupportedEvaluation<Boolean>(op);
            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Double>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorShiftRight()
        {
            ExpectArgumentEmptyException("symbol", () => new ShiftRightOperator(null));
            ExpectArgumentEmptyException("symbol", () => new ShiftRightOperator(String.Empty));

            Assert.NotNull(ShiftRightOperator.CStyle);
            Assert.AreEqual(">>", ShiftRightOperator.CStyle.Symbol);

            Assert.NotNull(ShiftRightOperator.PascalStyle);
            Assert.AreEqual("shr", ShiftRightOperator.PascalStyle.Symbol);

            var op = new ShiftRightOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(5, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 0xFFAA, 4L, 0x0FFA);
            AssertEvaluation<Int64>(op, 0x10, 64, 0x10);
            AssertEvaluation<Int64>(op, 4, 1, 2);

            AssertUnsupportedEvaluation<Boolean>(op);
            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Double>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorNot()
        {
            ExpectArgumentEmptyException("symbol", () => new NotOperator(null));
            ExpectArgumentEmptyException("symbol", () => new NotOperator(String.Empty));

            Assert.NotNull(NotOperator.CStyle);
            Assert.AreEqual("!", NotOperator.CStyle.Symbol);

            Assert.NotNull(NotOperator.PascalStyle);
            Assert.AreEqual("not", NotOperator.PascalStyle.Symbol);

            var op = new NotOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 1, ~1L);
            AssertEvaluation<Int64>(op, 0, ~0L);
            AssertEvaluation<Int64>(op, Int64.MinValue, ~Int64.MinValue);

            AssertEvaluation<Boolean>(op, true, false);
            AssertEvaluation<Boolean>(op, false, true);

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Double>(op);            
        }

        [Test]
        public void TestCaseStandardOperatorSubtract()
        {
            ExpectArgumentEmptyException("symbol", () => new SubtractOperator(null));
            ExpectArgumentEmptyException("symbol", () => new SubtractOperator(String.Empty));

            Assert.NotNull(SubtractOperator.CStyle);
            Assert.AreEqual("-", SubtractOperator.CStyle.Symbol);

            Assert.NotNull(SubtractOperator.PascalStyle);
            Assert.AreEqual("-", SubtractOperator.PascalStyle.Symbol);

            var op = new SubtractOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(4, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 0, 100, -100);
            AssertEvaluation<Int64>(op, 0, 0, 0);
            AssertEvaluation<Int64>(op, -1, -2, 1);

            AssertEvaluation<Double>(op, 0, 100, -100);
            AssertEvaluation<Double>(op, 0, 0, 0);
            AssertEvaluation<Double>(op, -1, -2, 1);

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Boolean>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorSum()
        {
            ExpectArgumentEmptyException("symbol", () => new SumOperator(null));
            ExpectArgumentEmptyException("symbol", () => new SumOperator(String.Empty));

            Assert.NotNull(SumOperator.CStyle);
            Assert.AreEqual("+", SumOperator.CStyle.Symbol);

            Assert.NotNull(SumOperator.PascalStyle);
            Assert.AreEqual("+", SumOperator.PascalStyle.Symbol);

            var op = new SumOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(4, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 0, 100, 100);
            AssertEvaluation<Int64>(op, 0, 0, 0);
            AssertEvaluation<Int64>(op, -1, -2, -3);

            AssertEvaluation<Double>(op, 0, 100, 100);
            AssertEvaluation<Double>(op, 0, 0, 0);
            AssertEvaluation<Double>(op, -1, -2, -3);

            AssertEvaluation<String>(op, "Hello ", "World", "Hello World");

            AssertUnsupportedEvaluation<Boolean>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorDivide()
        {
            ExpectArgumentEmptyException("symbol", () => new DivideOperator(null));
            ExpectArgumentEmptyException("symbol", () => new DivideOperator(String.Empty));

            Assert.NotNull(DivideOperator.CStyle);
            Assert.AreEqual("/", DivideOperator.CStyle.Symbol);

            Assert.NotNull(DivideOperator.PascalStyle);
            Assert.AreEqual("/", DivideOperator.PascalStyle.Symbol);

            var op = new DivideOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(3, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 1, 2, 0);
            AssertEvaluation<Int64>(op, -5, 3, -1);
            AssertEvaluation<Int64>(op, Int64.MaxValue, Int64.MaxValue, 1);
            AssertEvaluation<Int64>(op, Int64.MaxValue, 1, Int64.MaxValue);

            AssertEvaluation<Double>(op, 1, 2, (1.00 / 2.00));
            AssertEvaluation<Double>(op, 5, -3, (5.00 / -3.00));

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Boolean>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorModulo()
        {
            ExpectArgumentEmptyException("symbol", () => new ModuloOperator(null));
            ExpectArgumentEmptyException("symbol", () => new ModuloOperator(String.Empty));

            Assert.NotNull(ModuloOperator.CStyle);
            Assert.AreEqual("%", ModuloOperator.CStyle.Symbol);

            Assert.NotNull(ModuloOperator.PascalStyle);
            Assert.AreEqual("mod", ModuloOperator.PascalStyle.Symbol);

            var op = new ModuloOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(3, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 1, 2, 1);
            AssertEvaluation<Int64>(op, -5, 3, -2);
            AssertEvaluation<Int64>(op, Int64.MaxValue, Int64.MaxValue, 0);

            AssertUnsupportedEvaluation<Double>(op);
            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Boolean>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorMultiply()
        {
            ExpectArgumentEmptyException("symbol", () => new MultiplyOperator(null));
            ExpectArgumentEmptyException("symbol", () => new MultiplyOperator(String.Empty));

            Assert.NotNull(MultiplyOperator.CStyle);
            Assert.AreEqual("*", MultiplyOperator.CStyle.Symbol);

            Assert.NotNull(MultiplyOperator.PascalStyle);
            Assert.AreEqual("*", MultiplyOperator.PascalStyle.Symbol);

            var op = new MultiplyOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(3, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 1, 2, 2);
            AssertEvaluation<Int64>(op, -5, 3, -15);
            AssertEvaluation<Int64>(op, 1, Int64.MaxValue, Int64.MaxValue);
            AssertEvaluation<Int64>(op, Int64.MinValue, 0, 0);

            AssertEvaluation<Double>(op, 1.5, 2, 1.50 * 2.00);
            AssertEvaluation<Double>(op, 0.33, 0, 0.33 * 0);
            AssertEvaluation<Double>(op, -100, 0.5, -100 * 0.5);

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Boolean>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorNegate()
        {
            ExpectArgumentEmptyException("symbol", () => new NegateOperator(null));
            ExpectArgumentEmptyException("symbol", () => new NegateOperator(String.Empty));

            Assert.NotNull(NegateOperator.CStyle);
            Assert.AreEqual("-", NegateOperator.CStyle.Symbol);

            Assert.NotNull(NegateOperator.PascalStyle);
            Assert.AreEqual("-", NegateOperator.PascalStyle.Symbol);

            var op = new NegateOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, 1, -1);
            AssertEvaluation<Int64>(op, 0, 0);

            AssertEvaluation<Double>(op, 1.33, -1.33);
            AssertEvaluation<Double>(op, 0, 0);

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Boolean>(op);
        }

        [Test]
        public void TestCaseStandardOperatorNeutral()
        {
            ExpectArgumentEmptyException("symbol", () => new NeutralOperator(null));
            ExpectArgumentEmptyException("symbol", () => new NeutralOperator(String.Empty));

            Assert.NotNull(NeutralOperator.CStyle);
            Assert.AreEqual("+", NeutralOperator.CStyle.Symbol);

            Assert.NotNull(NeutralOperator.PascalStyle);
            Assert.AreEqual("+", NeutralOperator.PascalStyle.Symbol);

            var op = new NeutralOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64>(op, Int64.MinValue, Int64.MinValue);
            AssertEvaluation<Int64>(op, Int64.MaxValue, Int64.MaxValue);
            AssertEvaluation<Int64>(op, 0, 0);

            AssertEvaluation<Double>(op, 0, 0);
            AssertEvaluation<Double>(op, Double.NaN, Double.NaN);
            AssertEvaluation<Double>(op, -Double.NaN, -Double.NaN);

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Boolean>(op);
        }

        [Test]
        public void TestCaseStandardOperatorEquals()
        {
            ExpectArgumentEmptyException("symbol", () => new EqualsOperator(null));
            ExpectArgumentEmptyException("symbol", () => new EqualsOperator(String.Empty));

            Assert.NotNull(EqualsOperator.CStyle);
            Assert.AreEqual("==", EqualsOperator.CStyle.Symbol);

            Assert.NotNull(EqualsOperator.PascalStyle);
            Assert.AreEqual("=", EqualsOperator.PascalStyle.Symbol);

            var op = new EqualsOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(7, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, 0, false);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, true);
            AssertEvaluation<Double, Boolean>(op, 3.33, 3.34, false);

            AssertEvaluation<String, Boolean>(op, "Hello", "Hello", true);
            AssertEvaluation<String, Boolean>(op, "world", "WORLD", false);

            AssertEvaluation<Boolean, Boolean>(op, false, false, true);
            AssertEvaluation<Boolean, Boolean>(op, true, false, false);

            AssertUnsupportedEvaluation<Int64, String>(op);
            AssertUnsupportedEvaluation<Int64, Boolean>(op);
            AssertUnsupportedEvaluation<String, Int64>(op);
            AssertUnsupportedEvaluation<String, Double>(op);
            AssertUnsupportedEvaluation<String, Boolean>(op);
            AssertUnsupportedEvaluation<Double, String>(op);
            AssertUnsupportedEvaluation<Double, Boolean>(op);
            AssertUnsupportedEvaluation<Boolean, Int64>(op);
            AssertUnsupportedEvaluation<Boolean, String>(op);
            AssertUnsupportedEvaluation<Boolean, Double>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorNotEquals()
        {
            ExpectArgumentEmptyException("symbol", () => new NotEqualsOperator(null));
            ExpectArgumentEmptyException("symbol", () => new NotEqualsOperator(String.Empty));

            Assert.NotNull(NotEqualsOperator.CStyle);
            Assert.AreEqual("!=", NotEqualsOperator.CStyle.Symbol);

            Assert.NotNull(NotEqualsOperator.PascalStyle);
            Assert.AreEqual("<>", NotEqualsOperator.PascalStyle.Symbol);

            var op = new NotEqualsOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(7, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, 0, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, false);
            AssertEvaluation<Double, Boolean>(op, 3.33, 3.34, true);

            AssertEvaluation<String, Boolean>(op, "Hello", "Hello", false);
            AssertEvaluation<String, Boolean>(op, "world", "WORLD", true);

            AssertEvaluation<Boolean, Boolean>(op, false, false, false);
            AssertEvaluation<Boolean, Boolean>(op, true, false, true);

            AssertUnsupportedEvaluation<Int64, String>(op);
            AssertUnsupportedEvaluation<Int64, Boolean>(op);
            AssertUnsupportedEvaluation<String, Int64>(op);
            AssertUnsupportedEvaluation<String, Double>(op);
            AssertUnsupportedEvaluation<String, Boolean>(op);
            AssertUnsupportedEvaluation<Double, String>(op);
            AssertUnsupportedEvaluation<Double, Boolean>(op);
            AssertUnsupportedEvaluation<Boolean, Int64>(op);
            AssertUnsupportedEvaluation<Boolean, String>(op);
            AssertUnsupportedEvaluation<Boolean, Double>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorGreaterThan()
        {
            ExpectArgumentEmptyException("symbol", () => new GreaterThanOperator(null));
            ExpectArgumentEmptyException("symbol", () => new GreaterThanOperator(String.Empty));

            Assert.NotNull(GreaterThanOperator.CStyle);
            Assert.AreEqual(">", GreaterThanOperator.CStyle.Symbol);

            Assert.NotNull(GreaterThanOperator.PascalStyle);
            Assert.AreEqual(">", GreaterThanOperator.PascalStyle.Symbol);

            var op = new GreaterThanOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, 0, Int64.MinValue, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, false);
            AssertEvaluation<Double, Boolean>(op, 3.34, 3.33, true);

            AssertUnsupportedEvaluation<Int64, String>(op);
            AssertUnsupportedEvaluation<Int64, Boolean>(op);
            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<String, Int64>(op);
            AssertUnsupportedEvaluation<String, Double>(op);
            AssertUnsupportedEvaluation<String, Boolean>(op);
            AssertUnsupportedEvaluation<Double, String>(op);
            AssertUnsupportedEvaluation<Double, Boolean>(op);
            AssertUnsupportedEvaluation<Boolean>(op);
            AssertUnsupportedEvaluation<Boolean, Int64>(op);
            AssertUnsupportedEvaluation<Boolean, String>(op);
            AssertUnsupportedEvaluation<Boolean, Double>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorGreaterThanOrEquals()
        {
            ExpectArgumentEmptyException("symbol", () => new GreaterThanOrEqualsOperator(null));
            ExpectArgumentEmptyException("symbol", () => new GreaterThanOrEqualsOperator(String.Empty));

            Assert.NotNull(GreaterThanOrEqualsOperator.CStyle);
            Assert.AreEqual(">=", GreaterThanOrEqualsOperator.CStyle.Symbol);

            Assert.NotNull(GreaterThanOrEqualsOperator.PascalStyle);
            Assert.AreEqual(">=", GreaterThanOrEqualsOperator.PascalStyle.Symbol);

            var op = new GreaterThanOrEqualsOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, 0, Int64.MinValue, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, true);
            AssertEvaluation<Double, Boolean>(op, -0.1, 0, false);
            AssertEvaluation<Double, Boolean>(op, 3.34, 3.33, true);

            AssertUnsupportedEvaluation<Int64, String>(op);
            AssertUnsupportedEvaluation<Int64, Boolean>(op);
            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<String, Int64>(op);
            AssertUnsupportedEvaluation<String, Double>(op);
            AssertUnsupportedEvaluation<String, Boolean>(op);
            AssertUnsupportedEvaluation<Double, String>(op);
            AssertUnsupportedEvaluation<Double, Boolean>(op);
            AssertUnsupportedEvaluation<Boolean>(op);
            AssertUnsupportedEvaluation<Boolean, Int64>(op);
            AssertUnsupportedEvaluation<Boolean, String>(op);
            AssertUnsupportedEvaluation<Boolean, Double>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorLowerThan()
        {
            ExpectArgumentEmptyException("symbol", () => new LowerThanOperator(null));
            ExpectArgumentEmptyException("symbol", () => new LowerThanOperator(String.Empty));

            Assert.NotNull(LowerThanOperator.CStyle);
            Assert.AreEqual("<", LowerThanOperator.CStyle.Symbol);

            Assert.NotNull(LowerThanOperator.PascalStyle);
            Assert.AreEqual("<", LowerThanOperator.PascalStyle.Symbol);

            var op = new LowerThanOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, 0, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, false);
            AssertEvaluation<Double, Boolean>(op, 3.33, 3.34, true);

            AssertUnsupportedEvaluation<Int64, String>(op);
            AssertUnsupportedEvaluation<Int64, Boolean>(op);
            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<String, Int64>(op);
            AssertUnsupportedEvaluation<String, Double>(op);
            AssertUnsupportedEvaluation<String, Boolean>(op);
            AssertUnsupportedEvaluation<Double, String>(op);
            AssertUnsupportedEvaluation<Double, Boolean>(op);
            AssertUnsupportedEvaluation<Boolean>(op);
            AssertUnsupportedEvaluation<Boolean, Int64>(op);
            AssertUnsupportedEvaluation<Boolean, String>(op);
            AssertUnsupportedEvaluation<Boolean, Double>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorLowerThanOrEquals()
        {
            ExpectArgumentEmptyException("symbol", () => new LowerThanOrEqualsOperator(null));
            ExpectArgumentEmptyException("symbol", () => new LowerThanOrEqualsOperator(String.Empty));

            Assert.NotNull(LowerThanOrEqualsOperator.CStyle);
            Assert.AreEqual("<=", LowerThanOrEqualsOperator.CStyle.Symbol);

            Assert.NotNull(LowerThanOrEqualsOperator.PascalStyle);
            Assert.AreEqual("<=", LowerThanOrEqualsOperator.PascalStyle.Symbol);

            var op = new LowerThanOrEqualsOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, 0, Int64.MinValue, false);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, true);
            AssertEvaluation<Double, Boolean>(op, -0.1, 0, true);
            AssertEvaluation<Double, Boolean>(op, 3.34, 3.33, false);

            AssertUnsupportedEvaluation<Int64, String>(op);
            AssertUnsupportedEvaluation<Int64, Boolean>(op);
            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<String, Int64>(op);
            AssertUnsupportedEvaluation<String, Double>(op);
            AssertUnsupportedEvaluation<String, Boolean>(op);
            AssertUnsupportedEvaluation<Double, String>(op);
            AssertUnsupportedEvaluation<Double, Boolean>(op);
            AssertUnsupportedEvaluation<Boolean>(op);
            AssertUnsupportedEvaluation<Boolean, Int64>(op);
            AssertUnsupportedEvaluation<Boolean, String>(op);
            AssertUnsupportedEvaluation<Boolean, Double>(op);

            AssertUnsupportedAnyLeftEvaluation(op);
        }

        [Test]
        public void TestCaseStandardOperatorMemberAccess()
        {
            ExpectArgumentEmptyException("symbol", () => new MemberAccessOperator(null, StringComparer.Ordinal));
            ExpectArgumentEmptyException("symbol", () => new MemberAccessOperator(String.Empty, StringComparer.Ordinal));
            ExpectArgumentNullException("comparer", () => new MemberAccessOperator(".", null));

            Assert.NotNull(MemberAccessOperator.CStyle);
            Assert.AreEqual(".", MemberAccessOperator.CStyle.Symbol);
            Assert.AreEqual(StringComparer.Ordinal, MemberAccessOperator.CStyle.Comparer);

            Assert.NotNull(MemberAccessOperator.PascalStyle);
            Assert.AreEqual(".", MemberAccessOperator.PascalStyle.Symbol);
            Assert.AreEqual(StringComparer.OrdinalIgnoreCase, MemberAccessOperator.PascalStyle.Comparer);

            var op = new MemberAccessOperator("operator", StringComparer.InvariantCultureIgnoreCase);
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(0, op.Precedence);
            Assert.AreEqual(StringComparer.InvariantCultureIgnoreCase, op.Comparer);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(true, op.ExpectRhsIdentifier);

            Object result;
            Assert.IsFalse(op.Evaluate(null, out result));
            Assert.IsFalse(op.Evaluate("some_string", out result));
            Assert.IsFalse(op.Evaluate(this, out result));
            Assert.IsFalse(op.Evaluate(10, out result));
            Assert.IsFalse(op.Evaluate(11.00, out result));
            Assert.IsFalse(op.Evaluate(false, out result));

            Assert.IsTrue(op.Evaluate("1234567890", "Length", out result));
            Assert.AreEqual(10, result);
            Assert.IsTrue(op.Evaluate(Tuple.Create(100), "Item1", out result));
            Assert.AreEqual(100, result);
            Assert.IsFalse(op.Evaluate(100, "non_existant_property", out result));
        }
    }
}

