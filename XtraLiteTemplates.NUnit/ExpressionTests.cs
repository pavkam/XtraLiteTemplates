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

namespace XtraLiteTemplates.NUnit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.NUnit.Inside;

    [TestFixture]
    public class ExpressionTests : TestBase
    { 
        private Expression CreateBloatedExpression()
        {
            var comparer = StringComparer.Ordinal;

            var operators = new List<Operator>()
            {
                new RelationalEqualsOperator(comparer, TypeConverter),
                new RelationalNotEqualsOperator(comparer, TypeConverter),
                new RelationalGreaterThanOperator(comparer, TypeConverter),
                new RelationalGreaterThanOrEqualsOperator(comparer, TypeConverter),
                new RelationalLowerThanOperator(comparer, TypeConverter),
                new RelationalLowerThanOrEqualsOperator(comparer, TypeConverter),
                new LogicalAndOperator(TypeConverter),
                new LogicalOrOperator(TypeConverter),
                new LogicalNotOperator(TypeConverter),
                new BitwiseAndOperator(TypeConverter),
                new BitwiseNotOperator(TypeConverter),
                new BitwiseOrOperator(TypeConverter),
                new BitwiseXorOperator(TypeConverter),
                new BitwiseShiftLeftOperator(TypeConverter),
                new BitwiseShiftRightOperator(TypeConverter),
                new ArithmeticDivideOperator(TypeConverter),
                new ArithmeticModuloOperator(TypeConverter),
                new ArithmeticMultiplyOperator(TypeConverter),
                new ArithmeticNegateOperator(TypeConverter),
                new ArithmeticNeutralOperator(TypeConverter),
                new ArithmeticSubtractOperator(TypeConverter),
                new ArithmeticSumOperator(TypeConverter),
                new SubscriptOperator(),
                new IntegerRangeOperator(TypeConverter),
                new ValueFormatOperator(CultureInfo.InvariantCulture, TypeConverter),
                new MemberAccessOperator(comparer),
            };

            var expression = new Expression(StringComparer.Ordinal);
            foreach (var op in operators)
            {
                expression.RegisterOperator(op);
            }

            return expression;
        }

        private Expression CreateTestExpression(String exprString)
        {
            Debug.Assert(!String.IsNullOrEmpty(exprString));
            var split = exprString.Split(' ');

            Expression result = CreateBloatedExpression();
            foreach (var term in split)
            {
                Int64 _integer;
                Double _float;
                Boolean _boolean;

                if (Int64.TryParse(term, out _integer))
                    result.FeedLiteral(_integer);
                else if (Double.TryParse(term, NumberStyles.Float, CultureInfo.InvariantCulture, out _float))
                    result.FeedLiteral(_float);
                else if (Boolean.TryParse(term, out _boolean))
                    result.FeedLiteral(_boolean);
                else if (term.StartsWith("'", StringComparison.Ordinal) && term.EndsWith("'", StringComparison.Ordinal))
                    result.FeedLiteral(term.Substring(1, term.Length - 2));
                else
                    result.FeedSymbol(term);
            }

            result.Construct();
            return result;
        }

        private IExpressionEvaluationContext CreateStandardTestEvaluationContext(Expression e)
        {
            var variables = new Dictionary<String, Object>()
            {
                { "a", -2 },
                { "b", -1 },
                { "c", 0 },
                { "d", 1 },
                { "e", 2 },
                { "s", "Hello World" },
            };

            var result = new TestEvaluationContext(e.Comparer);

            result.OpenEvaluationFrame();
            foreach(var kvp in variables)
                result.SetVariable(kvp.Key, kvp.Value);

            return result;
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

            var neutral = new ArithmeticNeutralOperator(TypeConverter);
            var sum = new ArithmeticSumOperator(TypeConverter);
            var subscript = new SubscriptOperator();

            /* Unary */
            expression.RegisterOperator(neutral);
            Assert.AreEqual(1, expression.SupportedOperators.Count);
            Assert.AreEqual(neutral, expression.SupportedOperators[0]);
            Assert.IsTrue(expression.IsSupportedOperator(neutral.Symbol));

            /* Binary */
            expression.RegisterOperator(sum);
            Assert.AreEqual(2, expression.SupportedOperators.Count);
            Assert.AreEqual(sum, expression.SupportedOperators[1]);
            Assert.IsTrue(expression.IsSupportedOperator(sum.Symbol));

            /* Group */
            expression.RegisterOperator(subscript);
            Assert.AreEqual(3, expression.SupportedOperators.Count);
            Assert.AreEqual(subscript, expression.SupportedOperators[2]);
            Assert.IsTrue(expression.IsSupportedOperator(subscript.Symbol));
            Assert.IsTrue(expression.IsSupportedOperator(subscript.Terminator));

            ExpectArgumentNullException("symbol", () => expression.IsSupportedOperator(null));
            ExpectArgumentEmptyException("symbol", () => expression.IsSupportedOperator(String.Empty));
        }

        [Test]
        public void TestCaseOperatorRegistration_1()
        {
            var expression = new Expression();

            var unaryOp_plus = new ArithmeticNeutralOperator("+", TypeConverter);
            var binaryOp_plus = new ArithmeticSumOperator("+", TypeConverter);
            var groupOp_plus = new SubscriptOperator("+", "-");

            expression.RegisterOperator(unaryOp_plus);
            ExpectOperatorAlreadyRegisteredException(unaryOp_plus.ToString(), () => expression.RegisterOperator(unaryOp_plus));
            ExpectOperatorAlreadyRegisteredException(groupOp_plus.ToString(), () => expression.RegisterOperator(groupOp_plus));
            expression.RegisterOperator(binaryOp_plus);
            ExpectOperatorAlreadyRegisteredException(binaryOp_plus.ToString(), () => expression.RegisterOperator(binaryOp_plus));
        }

        [Test]
        public void TestCaseOperatorRegistration_2()
        {
            var expression = new Expression();

            var neutral = new ArithmeticNeutralOperator(TypeConverter);
            var sum = new ArithmeticSumOperator(TypeConverter);
            var subscript = new SubscriptOperator();

            expression.RegisterOperator(neutral);
            expression.RegisterOperator(sum);
            expression.RegisterOperator(subscript);

            ExpectOperatorAlreadyRegisteredException(neutral.ToString(), () => expression.RegisterOperator(neutral));
            ExpectOperatorAlreadyRegisteredException(sum.ToString(), () => expression.RegisterOperator(sum));
            ExpectOperatorAlreadyRegisteredException(subscript.ToString(), () => expression.RegisterOperator(subscript));

            var _ss1 = new SubscriptOperator("+", ">");
            ExpectOperatorAlreadyRegisteredException(_ss1.ToString(), () => expression.RegisterOperator(_ss1));

            var _ss2 = new SubscriptOperator("<", "+");
            ExpectOperatorAlreadyRegisteredException(_ss2.ToString(), () => expression.RegisterOperator(_ss2));

            var _ss3 = new SubscriptOperator("(", ">");
            ExpectOperatorAlreadyRegisteredException(_ss3.ToString(), () => expression.RegisterOperator(_ss3));

            var _ss4 = new SubscriptOperator("<", ")");
            ExpectOperatorAlreadyRegisteredException(_ss4.ToString(), () => expression.RegisterOperator(_ss4));
        }

        [Test]
        public void TestCaseExpressionStates()
        {
            var expression1 = new Expression();
            Assert.IsFalse(expression1.Started);
            Assert.IsFalse(expression1.Constructed);
            ExpectCannotConstructExpressionInvalidStateException(() => expression1.Construct());

            expression1.RegisterOperator(new ArithmeticSumOperator(TypeConverter));

            expression1.FeedLiteral(1);
            Assert.IsTrue(expression1.Started);
            Assert.IsFalse(expression1.Constructed);

            var context = CreateStandardTestEvaluationContext(expression1);

            ExpectCannotRegisterOperatorsForStartedExpressionException(() => expression1.RegisterOperator(new ArithmeticSumOperator(TypeConverter)));
            ExpectCannotEvaluateUnconstructedExpressionException(() => expression1.Evaluate(context));

            expression1.Construct();
            Assert.IsTrue(expression1.Started);
            Assert.IsTrue(expression1.Constructed);

            ExpectCannotModifyAConstructedExpressionException(() => expression1.FeedLiteral(2));
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
            Assert.AreEqual("1 + 2 * 3 + 4 / 5 - 6 + 7 + 8 / @a + 9 % 10", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(13.8, result);
        }

        [Test]
        public void TestCaseEvaluation_3a()
        {
            var expression = CreateTestExpression("false && a > 0");
            Assert.AreEqual("False && @a > 0", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(false, result);
        }

        [Test]
        public void TestCaseEvaluation_3b()
        {
            var expression = CreateTestExpression("a > -100 && true");
            Assert.AreEqual("@a > -100 && True", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestCaseEvaluation_EmptyGroup()
        {
            var expression = CreateBloatedExpression();

            expression.FeedSymbol("(");            
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
        }

        [Test]
        public void TestCaseFeedingErrors()
        {
            var expression = CreateBloatedExpression();
            ExpectInvalidExpressionTermException("*", () => expression.FeedSymbol("*"));
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));

            expression.FeedLiteral("Hello");
            ExpectInvalidExpressionTermException("!", () => expression.FeedSymbol("!"));
            ExpectInvalidExpressionTermException("(", () => expression.FeedSymbol("("));
            ExpectInvalidExpressionTermException("World", () => expression.FeedLiteral("World"));
            ExpectInvalidExpressionTermException("reference", () => expression.FeedSymbol("reference"));

            expression.FeedSymbol("+");
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
            expression.FeedSymbol("(");
            ExpectInvalidExpressionTermException("/", () => expression.FeedSymbol("/"));
            expression.FeedLiteral(100);
            ExpectInvalidExpressionTermException(200, () => expression.FeedLiteral(200));
            ExpectInvalidExpressionTermException("reference", () => expression.FeedSymbol("reference"));
            expression.FeedSymbol(")");
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
            ExpectInvalidExpressionTermException("!", () => expression.FeedSymbol("!"));
            ExpectInvalidExpressionTermException("reference", () => expression.FeedSymbol("reference"));
            ExpectInvalidExpressionTermException(true, () => expression.FeedLiteral(true));
        }

        [Test]
        public void TestCaseBalancingErrors()
        {
            var expression = CreateBloatedExpression();

            expression.FeedSymbol("+");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedSymbol("-");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedLiteral(10);
            expression.FeedSymbol("*");
            expression.FeedSymbol("(");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedSymbol("chakalaka");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedSymbol("/");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedSymbol("-");
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedLiteral(10);
            ExpectCannotConstructExpressionInvalidStateException(() => expression.Construct());
            expression.FeedSymbol(")");

            expression.Construct();
        }

        [Test]
        public void TestCaseEvaluationFull()
        {
            var expression = CreateTestExpression("true / 100.0 * string >> false >= -10 | something_else");
            var result_wg = expression.Evaluate(CreateStandardTestEvaluationContext(expression));

            Assert.AreEqual("1", result_wg.ToString());
        }

        [Test]
        public void TestCaseCaseSensitivity()
        {
            var expression_ins = new Expression(StringComparer.OrdinalIgnoreCase);
            var expression_sens = new Expression(StringComparer.Ordinal);

            var testOperator = new ArithmeticNeutralOperator("lower_case_operator", TypeConverter);

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
            Assert.AreEqual("@a + \"b\" % ( ( @c ) ) >> ( ( ( +++@t - 3 ) / !False ) )", s_arithm);
            Assert.AreEqual(">>{+{@a,%{\"b\",(){(){@c}}}},(){(){/{(){-{+{+{+{@t}}},3}},!{False}}}}}", s_canon);
            Assert.AreEqual(">> + @a % \"b\" ((@c)) ((/ (- +++@t 3) !False))", s_polish);
        }

        [Test]
        public void TestCaseCaseToString_2()
        {
            var expression = CreateBloatedExpression();
            var def1 = expression.ToString();

            expression.FeedSymbol("!");
            var def2 = expression.ToString();

            expression.FeedLiteral("a");
            var def3 = expression.ToString();

            expression.FeedSymbol("+");
            var def4 = expression.ToString();

            expression.FeedLiteral("5");
            var def5 = expression.ToString();

            Assert.AreEqual("??", def1);
            Assert.AreEqual("!??", def2);
            Assert.AreEqual("!\"a\"", def3);
            Assert.AreEqual("!\"a\" + ??", def4);
            Assert.AreEqual("!\"a\" + \"5\"", def5);
        }

        [Test]
        public void TestCaseMemberAccess1()
        {
            var expression = CreateTestExpression("s . Length + 10");
            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));

            Assert.AreEqual(21, result);
        }

        [Test]
        public void TestCaseMemberAccess2()
        {
            var expression = CreateTestExpression("s . length");
            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));

            Assert.IsNull(result);
        }

        [Test]
        public void TestCaseMemberAccess3()
        {
            var expression = CreateTestExpression("'hello_world' . Length");
            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));

            Assert.AreEqual(11, result);
        }

        [Test]
        public void TestCaseMemberLhs1a()
        {
            var expression = new Expression();

            expression.RegisterOperator(new LhsIdentTestOperator(".", 0));

            expression.FeedLiteral(1);
            ExpectUnexpectedExpressionTermException(".", () => expression.FeedSymbol("."));
        }

        [Test]
        public void TestCaseMemberLhs1b()
        {
            var expression = new Expression();

            expression.RegisterOperator(new LhsIdentTestOperator(".", 0));

            expression.FeedLiteral(1.00);
            ExpectUnexpectedExpressionTermException(".", () => expression.FeedSymbol("."));
        }

        [Test]
        public void TestCaseMemberLhs1c()
        {
            var expression = new Expression();

            expression.RegisterOperator(new LhsIdentTestOperator(".", 0));

            expression.FeedLiteral(true);
            ExpectUnexpectedExpressionTermException(".", () => expression.FeedSymbol("."));
        }

        [Test]
        public void TestCaseMemberLhs1d()
        {
            var expression = new Expression();

            expression.RegisterOperator(new LhsIdentTestOperator(".", 0));

            expression.FeedLiteral("term");
            ExpectUnexpectedExpressionTermException(".", () => expression.FeedSymbol("."));
        }

        [Test]
        public void TestCaseMemberLhs1e()
        {
            var expression = new Expression();

            expression.RegisterOperator(new LhsIdentTestOperator(".", 0));

            expression.FeedSymbol("symbol_1");
            expression.FeedSymbol(".");
            expression.FeedLiteral("literal_1");
            ExpectUnexpectedExpressionTermException(".", () => expression.FeedSymbol("."));

            expression.Construct();
        }

        [Test]
        public void TestCaseMemberLhs1f()
        {
            var expression = new Expression();

            expression.RegisterOperator(new LhsIdentTestOperator(".", 0));

            expression.FeedSymbol("symbol_1");
            expression.FeedSymbol(".");
            expression.FeedSymbol("symbol_2");
            expression.FeedSymbol(".");
            expression.FeedSymbol("symbol_3");
            expression.Construct();

            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);
            Assert.AreEqual(".{symbol_1,.{symbol_2,@symbol_3}}", canonical);
        }

        [Test]
        public void TestCaseMemberLhs1g()
        {
            var expression = new Expression();

            expression.RegisterOperator(new ArithmeticSumOperator(TypeConverter));
            expression.RegisterOperator(new LhsIdentTestOperator(".", 0));

            expression.FeedSymbol("symbol_1");
            expression.FeedSymbol("+");
            expression.FeedSymbol("symbol_2");
            expression.FeedSymbol(".");
            expression.FeedSymbol("symbol_3");
            expression.Construct();

            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);
            Assert.AreEqual("+{@symbol_1,.{symbol_2,@symbol_3}}", canonical);
        }

        [Test]
        public void TestCaseMemberLhs1h()
        {
            var expression = new Expression();

            expression.RegisterOperator(new ArithmeticSumOperator(TypeConverter));
            expression.RegisterOperator(new LhsIdentTestOperator(".", 100));

            expression.FeedSymbol("symbol_1");
            expression.FeedSymbol("+");
            expression.FeedSymbol("symbol_2");

            ExpectUnexpectedExpressionTermException(".", () => expression.FeedSymbol("."));
        }

        [Test]
        public void TestCaseMemberLhsRhsComplex()
        {
            var expression = new Expression();

            expression.RegisterOperator(new ArithmeticMultiplyOperator(TypeConverter));
            expression.RegisterOperator(new LogicalAndOperator(TypeConverter));
            expression.RegisterOperator(new LhsRhsIdentTestOperator("."));

            expression.FeedSymbol("a");
            expression.FeedSymbol(".");
            expression.FeedSymbol("b");
            ExpectUnexpectedExpressionTermException("*", () => expression.FeedSymbol("*"));
            expression.FeedSymbol("&&");
            expression.FeedSymbol("c");
            expression.FeedSymbol(".");
            expression.FeedSymbol("d");
            expression.FeedSymbol("&&");
            expression.FeedSymbol("e");
            expression.FeedSymbol("*");
            expression.FeedSymbol("f");
            ExpectUnexpectedExpressionTermException(".", () => expression.FeedSymbol("."));

            expression.Construct();

            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);
            Assert.AreEqual("&&{&&{.{a,b},.{c,d}},*{@e,@f}}", canonical);
        }


        [Test]
        public void TestCaseAssociativityRtl()
        {
            var expression = new Expression();

            expression.RegisterOperator(new RtlAssocTestOperator("+"));

            expression.FeedSymbol("a");
            expression.FeedSymbol("+");
            expression.FeedSymbol("b");
            expression.FeedSymbol("+");
            expression.FeedSymbol("c");
            expression.Construct();

            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);
            Assert.AreEqual("+{@a,+{@b,@c}}", canonical);
        }

        [Test]
        public void TestCaseAssociativityLtr()
        {
            var expression = new Expression();

            expression.RegisterOperator(new LtrAssocTestOperator("+"));

            expression.FeedSymbol("a");
            expression.FeedSymbol("+");
            expression.FeedSymbol("b");
            expression.FeedSymbol("+");
            expression.FeedSymbol("c");
            expression.Construct();

            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);
            Assert.AreEqual("+{+{@a,@b},@c}", canonical);
        }

        [Test]
        public void TestCaseAssociativityRtlLtr()
        {
            var expression = new Expression();

            expression.RegisterOperator(new LtrAssocTestOperator("+"));
            expression.RegisterOperator(new RtlAssocTestOperator("*"));

            expression.FeedSymbol("a");
            expression.FeedSymbol("+");
            expression.FeedSymbol("b");
            expression.FeedSymbol("*");
            expression.FeedSymbol("c");
            expression.FeedSymbol("+");
            expression.FeedSymbol("d");
            expression.FeedSymbol("+");
            expression.FeedSymbol("e");
            expression.FeedSymbol("*");
            expression.FeedSymbol("f");
            expression.FeedSymbol("*");
            expression.FeedSymbol("g");

            expression.Construct();

            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);
            Assert.AreEqual("+{+{+{@a,*{@b,@c}},@d},*{@e,*{@f,@g}}}", canonical);
        }

        [Test]
        public void TestCaseMemberAccess4()
        {
            var expression = CreateBloatedExpression();

            expression.FeedSymbol("!");
            expression.FeedSymbol("variable");
            expression.FeedSymbol(".");

            ExpectUnexpectedExpressionTermException("+", () => expression.FeedSymbol("+"));
            ExpectUnexpectedExpressionTermException("(", () => expression.FeedSymbol("("));
            ExpectUnexpectedExpressionTermException("length", () => expression.FeedLiteral("length"));
            ExpectUnexpectedExpressionTermException(1, () => expression.FeedLiteral(1));
            ExpectUnexpectedExpressionTermException(1.00, () => expression.FeedLiteral(1.00));
            ExpectUnexpectedExpressionTermException(false, () => expression.FeedLiteral(false));
            ExpectUnexpectedExpressionTermException(null, () => expression.FeedLiteral(null));

            expression.FeedSymbol("length");
            expression.FeedSymbol(".");

            ExpectUnexpectedExpressionTermException("+", () => expression.FeedSymbol("+"));
            ExpectUnexpectedExpressionTermException("(", () => expression.FeedSymbol("("));
            ExpectUnexpectedExpressionTermException("some_else", () => expression.FeedLiteral("some_else"));
            ExpectUnexpectedExpressionTermException(1, () => expression.FeedLiteral(1));
            ExpectUnexpectedExpressionTermException(1.00, () => expression.FeedLiteral(1.00));
            ExpectUnexpectedExpressionTermException(false, () => expression.FeedLiteral(false));
            ExpectUnexpectedExpressionTermException(null, () => expression.FeedLiteral(null));

            expression.FeedSymbol("some_else");
            expression.FeedSymbol("*");
            expression.FeedLiteral(100);

            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);
            Assert.AreEqual("*{!{.{.{@variable,length},some_else}},100}", canonical);
        }
    }
}

