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
                Assert.AreEqual("Cannot modify a contructed expression.", e.Message);
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
                Assert.AreEqual("Expression has not been contructed.", e.Message);
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
                    result.FeedConstant(term.Substring(0, term.Length - 2));
                else
                    result.FeedSymbol(term);
            }

            result.Construct();
            return result;
        }

        private IEvaluationContext CreateStandardTestEvaluationContext(Expression e)
        {
            var variables = new Dictionary<String, Object>()
            {
                { "a", -2 },
                { "b", -1 },
                { "c", 0 },
                { "d", 1 },
                { "e", 2 },
            };

            return new TestExpressionEvaluationContext(e.Comparer, variables);
        }

        [Test]
        public void TestCaseContruction()
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
    }
}

