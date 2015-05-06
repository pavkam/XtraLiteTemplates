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


        [Test]
        public void TestCaseStandardOperatorSubscript()
        {
            AssertArgumentEmptyException("symbol", () => new SubscriptOperator(null, ")"));
            AssertArgumentEmptyException("terminator", () => new SubscriptOperator("(", null));
            AssertArgumentEmptyException("symbol", () => new SubscriptOperator(null, null));
            AssertArgumentEmptyException("symbol", () => new SubscriptOperator(String.Empty, ")"));
            AssertArgumentEmptyException("terminator", () => new SubscriptOperator("(", String.Empty));
            AssertArgumentEmptyException("symbol", () => new SubscriptOperator(String.Empty, String.Empty));
            AssertArgumentsEqualException("symbol", "terminator", () => new SubscriptOperator("same", "same"));

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

            AssertEvaluation<Int64>(op, 100L, 100L);
            AssertEvaluation<Double>(op, 100.00, 100.00);
            AssertEvaluation<String>(op, "Hello World", "Hello World");
            AssertEvaluation<Boolean>(op, true, true);
        }

        [Test]
        public void TestCaseStandardOperatorAnd()
        {
            AssertArgumentEmptyException("symbol", () => new AndOperator(null));
            AssertArgumentEmptyException("symbol", () => new AndOperator(String.Empty));

            Assert.NotNull(AndOperator.CStyle);
            Assert.AreEqual("&", AndOperator.CStyle.Symbol);

            Assert.NotNull(AndOperator.PascalStyle);
            Assert.AreEqual("and", AndOperator.PascalStyle.Symbol);

            var op = new AndOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(8, op.Precedence);

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
        }

        [Test]
        public void TestCaseStandardOperatorOr()
        {
            AssertArgumentEmptyException("symbol", () => new OrOperator(null));
            AssertArgumentEmptyException("symbol", () => new OrOperator(String.Empty));

            Assert.NotNull(OrOperator.CStyle);
            Assert.AreEqual("|", OrOperator.CStyle.Symbol);

            Assert.NotNull(OrOperator.PascalStyle);
            Assert.AreEqual("or", OrOperator.PascalStyle.Symbol);

            var op = new OrOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(10, op.Precedence);

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
        }

        [Test]
        public void TestCaseStandardOperatorXor()
        {
            AssertArgumentEmptyException("symbol", () => new XorOperator(null));
            AssertArgumentEmptyException("symbol", () => new XorOperator(String.Empty));

            Assert.NotNull(XorOperator.CStyle);
            Assert.AreEqual("^", XorOperator.CStyle.Symbol);

            Assert.NotNull(XorOperator.PascalStyle);
            Assert.AreEqual("xor", XorOperator.PascalStyle.Symbol);

            var op = new XorOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(9, op.Precedence);

            AssertEvaluation<Int64>(op, 0xAA00BB00CC, 0x00DD00EE00, 0xAADDBBEECC);
            AssertEvaluation<Int64>(op, 0xFFFFFFFFFFFF, 0, 0xFFFFFFFFFFFF);
            AssertEvaluation<Int64>(op, 1, 2, 3);
            AssertEvaluation<Int64>(op, 0xCC, 5, 0xC9);

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Double>(op);
            AssertUnsupportedEvaluation<Boolean>(op);
        }

        [Test]
        public void TestCaseStandardOperatorShiftLeft()
        {
            AssertArgumentEmptyException("symbol", () => new ShiftLeftOperator(null));
            AssertArgumentEmptyException("symbol", () => new ShiftLeftOperator(String.Empty));

            Assert.NotNull(ShiftLeftOperator.CStyle);
            Assert.AreEqual("<<", ShiftLeftOperator.CStyle.Symbol);

            Assert.NotNull(ShiftLeftOperator.PascalStyle);
            Assert.AreEqual("shl", ShiftLeftOperator.PascalStyle.Symbol);

            var op = new ShiftLeftOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(5, op.Precedence);

            AssertEvaluation<Int64>(op, 0xFF, 4L, 0x0FF0);
            AssertEvaluation<Int64>(op, 0x10, 64, 0x10);
            AssertEvaluation<Int64>(op, 1, 2, 4);

            AssertUnsupportedEvaluation<Boolean>(op);
            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Double>(op);
        }

        [Test]
        public void TestCaseStandardOperatorShiftRight()
        {
            AssertArgumentEmptyException("symbol", () => new ShiftRightOperator(null));
            AssertArgumentEmptyException("symbol", () => new ShiftRightOperator(String.Empty));

            Assert.NotNull(ShiftRightOperator.CStyle);
            Assert.AreEqual(">>", ShiftRightOperator.CStyle.Symbol);

            Assert.NotNull(ShiftRightOperator.PascalStyle);
            Assert.AreEqual("shr", ShiftRightOperator.PascalStyle.Symbol);

            var op = new ShiftRightOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(5, op.Precedence);

            AssertEvaluation<Int64>(op, 0xFFAA, 4L, 0x0FFA);
            AssertEvaluation<Int64>(op, 0x10, 64, 0x10);
            AssertEvaluation<Int64>(op, 4, 1, 2);

            AssertUnsupportedEvaluation<Boolean>(op);
            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Double>(op);
        }

        [Test]
        public void TestCaseStandardOperatorNot()
        {
            AssertArgumentEmptyException("symbol", () => new NotOperator(null));
            AssertArgumentEmptyException("symbol", () => new NotOperator(String.Empty));

            Assert.NotNull(NotOperator.CStyle);
            Assert.AreEqual("!", NotOperator.CStyle.Symbol);

            Assert.NotNull(NotOperator.PascalStyle);
            Assert.AreEqual("not", NotOperator.PascalStyle.Symbol);

            var op = new NotOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);

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
            AssertArgumentEmptyException("symbol", () => new SubtractOperator(null));
            AssertArgumentEmptyException("symbol", () => new SubtractOperator(String.Empty));

            Assert.NotNull(SubtractOperator.CStyle);
            Assert.AreEqual("-", SubtractOperator.CStyle.Symbol);

            Assert.NotNull(SubtractOperator.PascalStyle);
            Assert.AreEqual("-", SubtractOperator.PascalStyle.Symbol);

            var op = new SubtractOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(4, op.Precedence);

            AssertEvaluation<Int64>(op, 0, 100, -100);
            AssertEvaluation<Int64>(op, 0, 0, 0);
            AssertEvaluation<Int64>(op, -1, -2, 1);

            AssertEvaluation<Double>(op, 0, 100, -100);
            AssertEvaluation<Double>(op, 0, 0, 0);
            AssertEvaluation<Double>(op, -1, -2, 1);

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Boolean>(op);
        }

        [Test]
        public void TestCaseStandardOperatorSum()
        {
            AssertArgumentEmptyException("symbol", () => new SumOperator(null));
            AssertArgumentEmptyException("symbol", () => new SumOperator(String.Empty));

            Assert.NotNull(SumOperator.CStyle);
            Assert.AreEqual("+", SumOperator.CStyle.Symbol);

            Assert.NotNull(SumOperator.PascalStyle);
            Assert.AreEqual("+", SumOperator.PascalStyle.Symbol);

            var op = new SumOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(4, op.Precedence);

            AssertEvaluation<Int64>(op, 0, 100, 100);
            AssertEvaluation<Int64>(op, 0, 0, 0);
            AssertEvaluation<Int64>(op, -1, -2, -3);

            AssertEvaluation<Double>(op, 0, 100, 100);
            AssertEvaluation<Double>(op, 0, 0, 0);
            AssertEvaluation<Double>(op, -1, -2, -3);

            AssertEvaluation<String>(op, "Hello ", "World", "Hello World");

            AssertUnsupportedEvaluation<Boolean>(op);
        }

        [Test]
        public void TestCaseStandardOperatorDivide()
        {
            AssertArgumentEmptyException("symbol", () => new DivideOperator(null));
            AssertArgumentEmptyException("symbol", () => new DivideOperator(String.Empty));

            Assert.NotNull(DivideOperator.CStyle);
            Assert.AreEqual("/", DivideOperator.CStyle.Symbol);

            Assert.NotNull(DivideOperator.PascalStyle);
            Assert.AreEqual("/", DivideOperator.PascalStyle.Symbol);

            var op = new DivideOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(3, op.Precedence);

            AssertEvaluation<Int64>(op, 1, 2, 0);
            AssertEvaluation<Int64>(op, -5, 3, -1);
            AssertEvaluation<Int64>(op, Int64.MaxValue, Int64.MaxValue, 1);
            AssertEvaluation<Int64>(op, Int64.MaxValue, 1, Int64.MaxValue);

            AssertEvaluation<Double>(op, 1, 2, (1.00 / 2.00));
            AssertEvaluation<Double>(op, 5, -3, (5.00 / -3.00));

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Boolean>(op);
        }

        [Test]
        public void TestCaseStandardOperatorModulo()
        {
            AssertArgumentEmptyException("symbol", () => new ModuloOperator(null));
            AssertArgumentEmptyException("symbol", () => new ModuloOperator(String.Empty));

            Assert.NotNull(ModuloOperator.CStyle);
            Assert.AreEqual("%", ModuloOperator.CStyle.Symbol);

            Assert.NotNull(ModuloOperator.PascalStyle);
            Assert.AreEqual("mod", ModuloOperator.PascalStyle.Symbol);

            var op = new ModuloOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(3, op.Precedence);

            AssertEvaluation<Int64>(op, 1, 2, 1);
            AssertEvaluation<Int64>(op, -5, 3, -2);
            AssertEvaluation<Int64>(op, Int64.MaxValue, Int64.MaxValue, 0);

            AssertUnsupportedEvaluation<Double>(op);
            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Boolean>(op);
        }

        [Test]
        public void TestCaseStandardOperatorMultiply()
        {
            AssertArgumentEmptyException("symbol", () => new MultiplyOperator(null));
            AssertArgumentEmptyException("symbol", () => new MultiplyOperator(String.Empty));

            Assert.NotNull(MultiplyOperator.CStyle);
            Assert.AreEqual("*", MultiplyOperator.CStyle.Symbol);

            Assert.NotNull(MultiplyOperator.PascalStyle);
            Assert.AreEqual("*", MultiplyOperator.PascalStyle.Symbol);

            var op = new MultiplyOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(3, op.Precedence);

            AssertEvaluation<Int64>(op, 1, 2, 2);
            AssertEvaluation<Int64>(op, -5, 3, -15);
            AssertEvaluation<Int64>(op, 1, Int64.MaxValue, Int64.MaxValue);
            AssertEvaluation<Int64>(op, Int64.MinValue, 0, 0);

            AssertEvaluation<Double>(op, 1.5, 2, 1.50 * 2.00);
            AssertEvaluation<Double>(op, 0.33, 0, 0.33 * 0);
            AssertEvaluation<Double>(op, -100, 0.5, -100 * 0.5);

            AssertUnsupportedEvaluation<String>(op);
            AssertUnsupportedEvaluation<Boolean>(op);
        }

        [Test]
        public void TestCaseStandardOperatorNegate()
        {
            AssertArgumentEmptyException("symbol", () => new NegateOperator(null));
            AssertArgumentEmptyException("symbol", () => new NegateOperator(String.Empty));

            Assert.NotNull(NegateOperator.CStyle);
            Assert.AreEqual("-", NegateOperator.CStyle.Symbol);

            Assert.NotNull(NegateOperator.PascalStyle);
            Assert.AreEqual("-", NegateOperator.PascalStyle.Symbol);

            var op = new NegateOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);

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
            AssertArgumentEmptyException("symbol", () => new NeutralOperator(null));
            AssertArgumentEmptyException("symbol", () => new NeutralOperator(String.Empty));

            Assert.NotNull(NeutralOperator.CStyle);
            Assert.AreEqual("+", NeutralOperator.CStyle.Symbol);

            Assert.NotNull(NeutralOperator.PascalStyle);
            Assert.AreEqual("+", NeutralOperator.PascalStyle.Symbol);

            var op = new NeutralOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(1, op.Precedence);

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
            AssertArgumentEmptyException("symbol", () => new EqualsOperator(null));
            AssertArgumentEmptyException("symbol", () => new EqualsOperator(String.Empty));

            Assert.NotNull(EqualsOperator.CStyle);
            Assert.AreEqual("==", EqualsOperator.CStyle.Symbol);

            Assert.NotNull(EqualsOperator.PascalStyle);
            Assert.AreEqual("=", EqualsOperator.PascalStyle.Symbol);

            var op = new EqualsOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(7, op.Precedence);

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
        }

        [Test]
        public void TestCaseStandardOperatorNotEquals()
        {
            AssertArgumentEmptyException("symbol", () => new NotEqualsOperator(null));
            AssertArgumentEmptyException("symbol", () => new NotEqualsOperator(String.Empty));

            Assert.NotNull(NotEqualsOperator.CStyle);
            Assert.AreEqual("!=", NotEqualsOperator.CStyle.Symbol);

            Assert.NotNull(NotEqualsOperator.PascalStyle);
            Assert.AreEqual("<>", NotEqualsOperator.PascalStyle.Symbol);

            var op = new NotEqualsOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(7, op.Precedence);

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
        }

        [Test]
        public void TestCaseStandardOperatorGreaterThan()
        {
            AssertArgumentEmptyException("symbol", () => new GreaterThanOperator(null));
            AssertArgumentEmptyException("symbol", () => new GreaterThanOperator(String.Empty));

            Assert.NotNull(GreaterThanOperator.CStyle);
            Assert.AreEqual(">", GreaterThanOperator.CStyle.Symbol);

            Assert.NotNull(GreaterThanOperator.PascalStyle);
            Assert.AreEqual(">", GreaterThanOperator.PascalStyle.Symbol);

            var op = new GreaterThanOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);

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
        }

        [Test]
        public void TestCaseStandardOperatorGreaterThanOrEquals()
        {
            AssertArgumentEmptyException("symbol", () => new GreaterThanOrEqualsOperator(null));
            AssertArgumentEmptyException("symbol", () => new GreaterThanOrEqualsOperator(String.Empty));

            Assert.NotNull(GreaterThanOrEqualsOperator.CStyle);
            Assert.AreEqual(">=", GreaterThanOrEqualsOperator.CStyle.Symbol);

            Assert.NotNull(GreaterThanOrEqualsOperator.PascalStyle);
            Assert.AreEqual(">=", GreaterThanOrEqualsOperator.PascalStyle.Symbol);

            var op = new GreaterThanOrEqualsOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);

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
        }

        [Test]
        public void TestCaseStandardOperatorLowerThan()
        {
            AssertArgumentEmptyException("symbol", () => new LowerThanOperator(null));
            AssertArgumentEmptyException("symbol", () => new LowerThanOperator(String.Empty));

            Assert.NotNull(LowerThanOperator.CStyle);
            Assert.AreEqual("<", LowerThanOperator.CStyle.Symbol);

            Assert.NotNull(LowerThanOperator.PascalStyle);
            Assert.AreEqual("<", LowerThanOperator.PascalStyle.Symbol);

            var op = new LowerThanOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);

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
        }

        [Test]
        public void TestCaseStandardOperatorLowerThanOrEquals()
        {
            AssertArgumentEmptyException("symbol", () => new LowerThanOrEqualsOperator(null));
            AssertArgumentEmptyException("symbol", () => new LowerThanOrEqualsOperator(String.Empty));

            Assert.NotNull(LowerThanOrEqualsOperator.CStyle);
            Assert.AreEqual("<=", LowerThanOrEqualsOperator.CStyle.Symbol);

            Assert.NotNull(LowerThanOrEqualsOperator.PascalStyle);
            Assert.AreEqual("<=", LowerThanOrEqualsOperator.PascalStyle.Symbol);

            var op = new LowerThanOrEqualsOperator("operator");
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);

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
        }
    }
}

