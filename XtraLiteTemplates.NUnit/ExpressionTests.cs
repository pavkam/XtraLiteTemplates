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
    using System.Collections.Generic;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Expressions.Operators.Standard;
    using XtraLiteTemplates.NUnit.Inside;

    [TestFixture]
    public class ExpressionTests : TestBase
    {
        private static void ExpectOperatorAlreadyRegisteredException(Operator @operator, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual(String.Format("Operator identified by symbol '{0}' has already been registered with expression.", @operator), e.Message);
            }
        }

        private static void ExpectCannotModifyAConstructedExpressionException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Cannot modify a finalized expression.", e.Message);
            }
        }

        private static void ExpectCannotRegisterOperatorsForStartedExpressionException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Operator registration must be performed before construction.", e.Message);
            }
        }

        private static void ExpectCannotEvaluateUnconstructedExpressionException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Expression has not been finalized.", e.Message);
            }
        }

        private static void ExpectInvalidExpressionTermException(Object term, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ExpressionException), e);
                Assert.AreEqual(String.Format("Invalid expression term: '{0}'.", term), e.Message);
            }
        }

        private static void ExpectCannotConstructExpressionInvalidStateException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ExpressionException), e);
                Assert.AreEqual("Unbalanced expressions cannot be finalized.", e.Message);
            }
        }
        

        private Expression CreateTestExpression(String exprString)
        {
            Debug.Assert(!String.IsNullOrEmpty(exprString));
            var split = exprString.Split(' ');

            Expression result = Expression.CreateStandardCStyle();
            foreach (var term in split)
            {
                Int64 _integer;
                Double _float;
                Boolean _boolean;

                if (Int64.TryParse(term, out _integer))
                    result.FeedConstant(_integer);
                else if (Double.TryParse(term, out _float))
                    result.FeedConstant(_float);
                else if (Boolean.TryParse(term, out _boolean))
                    result.FeedConstant(_boolean);
                else if (term.StartsWith("'") && term.EndsWith("'"))
                    result.FeedConstant(term.Substring(1, term.Length - 2));
                else
                    result.FeedSymbol(term);
            }

            result.Construct();
            return result;
        }

        private IEvaluationContext CreateStandardTestEvaluationContext(Expression e, Boolean gatherErrors = false)
        {
            var variables = new Dictionary<String, Object>()
            {
                { "a", -2 },
                { "b", -1 },
                { "c", 0 },
                { "d", 1 },
                { "e", 2 },
            };

            return new TestExpressionEvaluationContext(gatherErrors, e.Comparer, variables);
        }

        [Test]
        public void TestCaseContruction_1()
        {
            ExpectArgumentNullException("comparer", () => new Expression(null));

            var expression_default = new Expression();
            Assert.AreEqual(StringComparer.OrdinalIgnoreCase, expression_default.Comparer);

            var expression_withCulture = new Expression(StringComparer.CurrentCulture);
            Assert.AreEqual(StringComparer.CurrentCulture, expression_withCulture.Comparer);

            Assert.AreEqual(0, expression_default.SupportedOperators.Count);
            Assert.AreEqual(0, expression_withCulture.SupportedOperators.Count);
        }

        [Test]
        public void TestCaseSupportedOperators()
        {
            var expression = new Expression();

            /* Unary */
            expression.RegisterOperator(NeutralOperator.PascalStyle);
            Assert.AreEqual(1, expression.SupportedOperators.Count);
            Assert.AreEqual(NeutralOperator.PascalStyle, expression.SupportedOperators[0]);
            Assert.IsTrue(expression.IsSupportedOperator(NeutralOperator.PascalStyle.Symbol));

            /* Binary */
            expression.RegisterOperator(SumOperator.PascalStyle);
            Assert.AreEqual(2, expression.SupportedOperators.Count);
            Assert.AreEqual(SumOperator.PascalStyle, expression.SupportedOperators[1]);
            Assert.IsTrue(expression.IsSupportedOperator(SumOperator.PascalStyle.Symbol));

            /* Group */
            expression.RegisterOperator(SubscriptOperator.PascalStyle);
            Assert.AreEqual(3, expression.SupportedOperators.Count);
            Assert.AreEqual(SubscriptOperator.PascalStyle, expression.SupportedOperators[2]);
            Assert.IsTrue(expression.IsSupportedOperator(SubscriptOperator.PascalStyle.Symbol));
            Assert.IsTrue(expression.IsSupportedOperator(SubscriptOperator.PascalStyle.Terminator));

            ExpectArgumentEmptyException("symbol", () => expression.IsSupportedOperator(null));
            ExpectArgumentEmptyException("symbol", () => expression.IsSupportedOperator(String.Empty));
        }

        [Test]
        public void TestCaseOperatorRegistration_1()
        {
            var expression = new Expression();

            var unaryOp_plus = new NeutralOperator("+");
            var binaryOp_plus = new SumOperator("+");
            var groupOp_plus = new SubscriptOperator("+", "-");

            expression.RegisterOperator(unaryOp_plus);
            ExpectOperatorAlreadyRegisteredException(unaryOp_plus, () => expression.RegisterOperator(unaryOp_plus));
            ExpectOperatorAlreadyRegisteredException(groupOp_plus, () => expression.RegisterOperator(groupOp_plus));
            expression.RegisterOperator(binaryOp_plus);
            ExpectOperatorAlreadyRegisteredException(binaryOp_plus, () => expression.RegisterOperator(binaryOp_plus));
        }

        [Test]
        public void TestCaseOperatorRegistration_2()
        {
            var expression = new Expression();

            var unaryOp_plus = new NeutralOperator("+");
            var binaryOp_plus = new SumOperator("+");
            var groupOp_plus = new SubscriptOperator("+", "-");

            expression.RegisterOperator(binaryOp_plus);
            ExpectOperatorAlreadyRegisteredException(binaryOp_plus, () => expression.RegisterOperator(binaryOp_plus));
            ExpectOperatorAlreadyRegisteredException(groupOp_plus, () => expression.RegisterOperator(groupOp_plus));
            expression.RegisterOperator(unaryOp_plus);
            ExpectOperatorAlreadyRegisteredException(unaryOp_plus, () => expression.RegisterOperator(unaryOp_plus));
        }

        [Test]
        public void TestCaseOperatorRegistration_3()
        {
            var expression = new Expression();

            var unaryOp_plus = new NeutralOperator("+");
            var unaryOp_minus = new NeutralOperator("-");
            var binaryOp_plus = new SumOperator("+");
            var binaryOp_minus = new SumOperator("-");
            var groupOp_plus_minus = new SubscriptOperator("+", "-");
            var groupOp_minus_plus = new SubscriptOperator("-", "+");

            expression.RegisterOperator(groupOp_plus_minus);
            ExpectOperatorAlreadyRegisteredException(groupOp_plus_minus, () => expression.RegisterOperator(groupOp_plus_minus));
            ExpectOperatorAlreadyRegisteredException(groupOp_minus_plus, () => expression.RegisterOperator(groupOp_minus_plus));

            ExpectOperatorAlreadyRegisteredException(unaryOp_plus, () => expression.RegisterOperator(unaryOp_plus));
            ExpectOperatorAlreadyRegisteredException(unaryOp_minus, () => expression.RegisterOperator(unaryOp_minus));

            ExpectOperatorAlreadyRegisteredException(binaryOp_plus, () => expression.RegisterOperator(binaryOp_plus));
            ExpectOperatorAlreadyRegisteredException(binaryOp_minus, () => expression.RegisterOperator(binaryOp_minus));
        }

        [Test]
        public void TestCaseExpressionStates()
        {
            var expression1 = new Expression();
            Assert.IsFalse(expression1.Started);
            Assert.IsFalse(expression1.Constructed);
            ExpectCannotConstructExpressionInvalidStateException(() => expression1.Construct());

            expression1.RegisterOperator(SumOperator.CStyle);

            expression1.FeedConstant(1);
            Assert.IsTrue(expression1.Started);
            Assert.IsFalse(expression1.Constructed);

            var context = CreateStandardTestEvaluationContext(expression1);

            ExpectCannotRegisterOperatorsForStartedExpressionException(() => expression1.RegisterOperator(SumOperator.CStyle));
            ExpectCannotEvaluateUnconstructedExpressionException(() => expression1.Evaluate(context));

            expression1.Construct();
            Assert.IsTrue(expression1.Started);
            Assert.IsTrue(expression1.Constructed);

            ExpectCannotModifyAConstructedExpressionException(() => expression1.FeedConstant(2));
            ExpectCannotModifyAConstructedExpressionException(() => expression1.FeedSymbol("abracadabra"));

            /* Should run fine. */
            ExpectArgumentNullException("context", () => expression1.Evaluate(null));
            expression1.Evaluate(context);
        }

        [Test]
        public void TestCaseEvaluation_1()
        {
            var expression = CreateTestExpression("+ ( - ( ( a - + b ) / - - ( c + 1 ) ) >> 8 ) * ( d / ( ! e + 1 ) )");
            Assert.AreEqual("+( -( ( @a - +@b ) / --( @c + 1 ) ) >> 8 ) * ( @d / ( !@e + 1 ) )", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void TestCaseEvaluation_2()
        {
            var expression = CreateTestExpression("1 + 2 * 3 + 4 / 5 - 6 + 7 + 8 / a + 9 % 10");
            Assert.AreEqual("8 + 8 / @a + 9", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(13, result);
        }

        [Test]
        public void TestCaseEvaluation_3a()
        {
            var expression = CreateTestExpression("false & a > 0");
            Assert.AreEqual("False", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(false, result);
        }

        [Test]
        public void TestCaseEvaluation_3b()
        {
            var expression = CreateTestExpression("a > -100 & true");
            Assert.AreEqual("@a > -100 & True", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestCaseEvaluation_EmptyGroup()
        {
            var expression = Expression.CreateStandardCStyle();

            expression.FeedSymbol("(");            
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
        }

        [Test]
        public void TestCaseFeedingErrors()
        {
            var expression = Expression.CreateStandardCStyle();
            ExpectInvalidExpressionTermException("*", () => expression.FeedSymbol("*"));
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));

            expression.FeedConstant("Hello");
            ExpectInvalidExpressionTermException("!", () => expression.FeedSymbol("!"));
            ExpectInvalidExpressionTermException("(", () => expression.FeedSymbol("("));
            ExpectInvalidExpressionTermException("World", () => expression.FeedConstant("World"));
            ExpectInvalidExpressionTermException("reference", () => expression.FeedSymbol("reference"));

            expression.FeedSymbol("+");
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
            expression.FeedSymbol("(");
            ExpectInvalidExpressionTermException("/", () => expression.FeedSymbol("/"));
            expression.FeedConstant(100);
            ExpectInvalidExpressionTermException(200, () => expression.FeedConstant(200));
            ExpectInvalidExpressionTermException("reference", () => expression.FeedSymbol("reference"));
            expression.FeedSymbol(")");
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
            ExpectInvalidExpressionTermException("!", () => expression.FeedSymbol("!"));
            ExpectInvalidExpressionTermException("reference", () => expression.FeedSymbol("reference"));
            ExpectInvalidExpressionTermException(true, () => expression.FeedConstant(true));
        }

        [Test]
        public void TestCaseBalancingErrors()
        {
            var expression = Expression.CreateStandardCStyle();

            expression.FeedSymbol("+");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedSymbol("-");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedConstant(10);
            expression.FeedSymbol("*");
            expression.FeedSymbol("(");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedSymbol("chakalaka");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedSymbol("/");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedSymbol("-");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedConstant(10);
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedSymbol(")");

            expression.Construct();
        }

        [Test]
        public void TestCaseEvaluationUndefined()
        {
            var expression = CreateTestExpression("true / 100.0 * 'string' >> false >= -10 | 'something_else'");
            var result_wg = expression.Evaluate(CreateStandardTestEvaluationContext(expression, true));
            var result_ng = expression.Evaluate(CreateStandardTestEvaluationContext(expression, false));

            Assert.AreEqual("(True/100*string>>False>=-10|something_else)", result_wg.ToString());
            Assert.IsNull(result_ng);
        }

        [Test]
        public void TestCaseCaseSensitivity()
        {
            var expression_ins = new Expression(StringComparer.OrdinalIgnoreCase);
            var expression_sens = new Expression(StringComparer.Ordinal);

            var testOperator = new NeutralOperator("lower_case_operator");

            expression_ins.RegisterOperator(testOperator);
            expression_sens.RegisterOperator(testOperator);

            Assert.IsTrue(expression_ins.IsSupportedOperator(testOperator.Symbol.ToUpper()));
            Assert.IsTrue(expression_ins.IsSupportedOperator(testOperator.Symbol));
            Assert.IsFalse(expression_sens.IsSupportedOperator(testOperator.Symbol.ToUpper()));
            Assert.IsTrue(expression_sens.IsSupportedOperator(testOperator.Symbol));

            expression_ins.FeedSymbol(testOperator.Symbol.ToUpper());
            expression_sens.FeedSymbol(testOperator.Symbol.ToUpper());

            Assert.AreEqual("lower_case_operator{??}", expression_ins.ToString(ExpressionFormatStyle.Canonical));
            Assert.AreEqual("@LOWER_CASE_OPERATOR", expression_sens.ToString(ExpressionFormatStyle.Canonical));
        }

        [Test]
        public void TestCaseCaseToString_1()
        {
            var expression = CreateTestExpression("a + 'b' % ( ( c ) ) >> ( ( ( + + + t - 3 ) / ! false ) )");

            var s_def = expression.ToString();
            var s_arithm = expression.ToString(ExpressionFormatStyle.Arithmetic);
            var s_canon = expression.ToString(ExpressionFormatStyle.Canonical);
            var s_polish = expression.ToString(ExpressionFormatStyle.Polish);

            Assert.AreEqual(s_def, s_arithm);
            Assert.AreEqual("@a + \"b\" % ( ( @c ) ) >> ( ( ( +++@t - 3 ) / True ) )", s_arithm);
            Assert.AreEqual(">>{+{@a,%{\"b\",(){(){@c}}}},(){(){/{(){-{+{+{+{@t}}},3}},True}}}}", s_canon);
            Assert.AreEqual(">> + @a % \"b\" ((@c)) ((/ (- +++@t 3) True))", s_polish);
        }

        [Test]
        public void TestCaseCaseToString_2()
        {
            var expression = Expression.CreateStandardCStyle();
            var def1 = expression.ToString();

            expression.FeedSymbol("!");
            var def2 = expression.ToString();

            expression.FeedConstant("a");
            var def3 = expression.ToString();

            expression.FeedSymbol("+");
            var def4 = expression.ToString();

            expression.FeedConstant("5");
            var def5 = expression.ToString();

            Assert.AreEqual("??", def1);
            Assert.AreEqual("!??", def2);
            Assert.AreEqual("!\"a\"", def3);
            Assert.AreEqual("!\"a\" + ??", def4);
            Assert.AreEqual("!\"a\" + \"5\"", def5);
        }

        [Test]
        public void TestCaseCreateMethod_CStyle()
        {
            var expression = Expression.CreateStandardCStyle();
            var allOperators = new HashSet<Operator>(expression.SupportedOperators);

            Assert.AreEqual(StringComparer.Ordinal, expression.Comparer);
            Assert.AreEqual(18, allOperators.Count);

            allOperators.Remove(SubscriptOperator.CStyle);
            allOperators.Remove(OrOperator.CStyle);
            allOperators.Remove(AndOperator.CStyle);
            allOperators.Remove(NotOperator.CStyle);
            allOperators.Remove(ShiftLeftOperator.CStyle);
            allOperators.Remove(ShiftRightOperator.CStyle);
            allOperators.Remove(XorOperator.CStyle);
            allOperators.Remove(EqualsOperator.CStyle);
            allOperators.Remove(NotEqualsOperator.CStyle);
            allOperators.Remove(GreaterThanOperator.CStyle);
            allOperators.Remove(GreaterThanOrEqualsOperator.CStyle);
            allOperators.Remove(LowerThanOperator.CStyle);
            allOperators.Remove(LowerThanOrEqualsOperator.CStyle);
            allOperators.Remove(NeutralOperator.CStyle);
            allOperators.Remove(NegateOperator.CStyle);
            allOperators.Remove(ModuloOperator.CStyle);
            allOperators.Remove(DivideOperator.CStyle);
            allOperators.Remove(MultiplyOperator.CStyle);
            allOperators.Remove(SubtractOperator.CStyle);
            allOperators.Remove(SumOperator.CStyle);

            Assert.AreEqual(0, allOperators.Count);
        }

        [Test]
        public void TestCaseCreateMethod_PascalStyle()
        {
            var expression = Expression.CreateStandardPascalStyle();
            var allOperators = new HashSet<Operator>(expression.SupportedOperators);

            Assert.AreEqual(StringComparer.OrdinalIgnoreCase, expression.Comparer);
            Assert.AreEqual(18, allOperators.Count);

            allOperators.Remove(SubscriptOperator.PascalStyle);
            allOperators.Remove(OrOperator.PascalStyle);
            allOperators.Remove(AndOperator.PascalStyle);
            allOperators.Remove(NotOperator.PascalStyle);
            allOperators.Remove(ShiftLeftOperator.PascalStyle);
            allOperators.Remove(ShiftRightOperator.PascalStyle);
            allOperators.Remove(XorOperator.PascalStyle);
            allOperators.Remove(EqualsOperator.PascalStyle);
            allOperators.Remove(NotEqualsOperator.PascalStyle);
            allOperators.Remove(GreaterThanOperator.PascalStyle);
            allOperators.Remove(GreaterThanOrEqualsOperator.PascalStyle);
            allOperators.Remove(LowerThanOperator.PascalStyle);
            allOperators.Remove(LowerThanOrEqualsOperator.PascalStyle);
            allOperators.Remove(NeutralOperator.PascalStyle);
            allOperators.Remove(NegateOperator.PascalStyle);
            allOperators.Remove(ModuloOperator.PascalStyle);
            allOperators.Remove(DivideOperator.PascalStyle);
            allOperators.Remove(MultiplyOperator.PascalStyle);
            allOperators.Remove(SubtractOperator.PascalStyle);
            allOperators.Remove(SumOperator.PascalStyle);

            Assert.AreEqual(0, allOperators.Count);
        }
    }
}

