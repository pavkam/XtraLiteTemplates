//
//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2017, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using System.Threading;

    using global::NUnit.Framework;

    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Tests.Inside;

    [TestFixture]
    [SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    [SuppressMessage("ReSharper", "PossibleNullReferenceException")]
    public class ExpressionTests : TestBase
    { 
        private static Expression CreateBloatedExpression()
        {
            var comparer = StringComparer.Ordinal;

            var operators = new List<Operator>
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
                new SequenceOperator(TypeConverter),
                new FormatOperator(CultureInfo.InvariantCulture, TypeConverter)
            };

            var expression = new Expression(ExpressionFlowSymbols.Default, StringComparer.Ordinal);
            foreach (var op in operators)
            {
                expression.RegisterOperator(op);
            }

            return expression;
        }

        private static Expression CreateTestExpression(string exprString)
        {
            Debug.Assert(!string.IsNullOrEmpty(exprString));
            var split = exprString.Split(' ');

            var result = CreateBloatedExpression();
            foreach (var term in split)
            {

                if (long.TryParse(term, out long integer))
                    result.FeedLiteral(integer);
                else if (double.TryParse(term, NumberStyles.Float, CultureInfo.InvariantCulture, out double _float))
                    result.FeedLiteral(_float);
                else if (bool.TryParse(term, out bool boolean))
                    result.FeedLiteral(boolean);
                else if (term.StartsWith("'", StringComparison.Ordinal) && term.EndsWith("'", StringComparison.Ordinal))
                    result.FeedLiteral(term.Substring(1, term.Length - 2));
                else
                    result.FeedSymbol(term);
            }

            result.Construct();
            return result;
        }

        private static IExpressionEvaluationContext CreateStandardTestEvaluationContext(Expression e)
        {
            var variables = new Dictionary<string, object>
                                {
                { "a", -2 },
                { "b", -1 },
                { "c", 0 },
                { "d", 1 },
                { "e", 2 },
                { "s", "Hello World" }
            };

            var result = CreateContext(e.Comparer);

            foreach(var kvp in variables)
                result.SetProperty(kvp.Key, kvp.Value);

            return result;
        }

        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseConstruction1()
        {
            ExpectArgumentNullException("comparer", () => new Expression(ExpressionFlowSymbols.Default, null));
            ExpectArgumentNullException("flowSymbols", () => new Expression(null, StringComparer.InvariantCulture));

            var expressionDefault = new Expression();
            Assert.AreEqual(StringComparer.OrdinalIgnoreCase, expressionDefault.Comparer);

            var expressionWithCulture = new Expression(ExpressionFlowSymbols.Default, StringComparer.CurrentCulture);
            Assert.AreEqual(StringComparer.CurrentCulture, expressionWithCulture.Comparer);
            Assert.AreEqual(ExpressionFlowSymbols.Default, expressionWithCulture.FlowSymbols);

            Assert.AreEqual(0, expressionDefault.SupportedOperators.Count);
            Assert.AreEqual(0, expressionWithCulture.SupportedOperators.Count);
        }

        [Test]
        public void TestCaseSupportedOperators()
        {
            var expression = new Expression();

            var neutral = new ArithmeticNeutralOperator(TypeConverter);
            var sum = new ArithmeticSumOperator(TypeConverter);

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

            ExpectArgumentNullException("symbol", () => expression.IsSupportedOperator(null));
            ExpectArgumentEmptyException("symbol", () => expression.IsSupportedOperator(string.Empty));
        }

        [Test]
        public void TestCaseOperatorRegistration1()
        {
            var expression = new Expression();

            var unaryOpPlus = new ArithmeticNeutralOperator("+", TypeConverter);
            var binaryOpPlus = new ArithmeticSumOperator("+", TypeConverter);

            expression.RegisterOperator(unaryOpPlus);
            ExpectOperatorAlreadyRegisteredException(unaryOpPlus.ToString(), () => expression.RegisterOperator(unaryOpPlus));
            expression.RegisterOperator(binaryOpPlus);
            ExpectOperatorAlreadyRegisteredException(binaryOpPlus.ToString(), () => expression.RegisterOperator(binaryOpPlus));
        }

        [Test]
        public void TestCaseOperatorRegistration2()
        {
            var expression = new Expression();

            var unary1 = new ArithmeticNeutralOperator("(", TypeConverter);
            var unary2 = new ArithmeticNeutralOperator(")", TypeConverter);
            var unary3 = new ArithmeticNeutralOperator(".", TypeConverter);
            var unary4 = new ArithmeticNeutralOperator(",", TypeConverter);

            var binary1 = new ArithmeticSumOperator("(", TypeConverter);
            var binary2 = new ArithmeticSumOperator(")", TypeConverter);
            var binary3 = new ArithmeticSumOperator(".", TypeConverter);
            var binary4 = new ArithmeticSumOperator(",", TypeConverter);

            ExpectOperatorAlreadyRegisteredException(unary1.ToString(), () => expression.RegisterOperator(unary1));
            ExpectOperatorAlreadyRegisteredException(unary2.ToString(), () => expression.RegisterOperator(unary2));
            ExpectOperatorAlreadyRegisteredException(unary3.ToString(), () => expression.RegisterOperator(unary3));
            ExpectOperatorAlreadyRegisteredException(unary4.ToString(), () => expression.RegisterOperator(unary4));

            ExpectOperatorAlreadyRegisteredException(binary1.ToString(), () => expression.RegisterOperator(binary1));
            ExpectOperatorAlreadyRegisteredException(binary2.ToString(), () => expression.RegisterOperator(binary2));
            ExpectOperatorAlreadyRegisteredException(binary3.ToString(), () => expression.RegisterOperator(binary3));
            ExpectOperatorAlreadyRegisteredException(binary4.ToString(), () => expression.RegisterOperator(binary4));
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
            ExpectCannotEvaluateUnConstructedExpressionException(() => expression1.Evaluate(context));

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
        public void TestCaseEvaluation1()
        {
            var expression = CreateTestExpression("+ ( - ( ( a - + b ) / - - ( c + 1 ) ) >> 8 ) * ( d / ( ! e + 1 ) )");
            Assert.AreEqual("+( -( ( @a - +@b ) / --( @c + 1 ) ) >> 8 ) * ( @d / ( !@e + 1 ) )", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(0, result);
        }

        [Test]
        public void TestCaseEvaluation2()
        {
            var expression = CreateTestExpression("1 + 2 * 3 + 4 / 5 - 6 + 7 + 8 / a + 9 % 10");
            Assert.AreEqual("1 + 2 * 3 + 4 / 5 - 6 + 7 + 8 / @a + 9 % 10", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(13.8, result);
        }

        [Test]
        public void TestCaseEvaluation_3A()
        {
            var expression = CreateTestExpression("false && a > 0");
            Assert.AreEqual("False && @a > 0", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(false, result);
        }

        [Test]
        public void TestCaseEvaluation_3B()
        {
            var expression = CreateTestExpression("a > -100 && true");
            Assert.AreEqual("@a > -100 && True", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(true, result);
        }

        [Test]
        public void TestCaseEvaluation4()
        {
            var expression = CreateTestExpression("0 * ( 1 + ( 2 + c ) )");
            Assert.AreEqual("0 * ( 1 + ( 2 + @c ) )", expression.ToString());

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.AreEqual(0, result);
        }


        [Test]
        public void TestCaseFeedingErrors1()
        {
            var expression = CreateBloatedExpression();
            ExpectUnexpectedExpressionOperatorException("*", () => expression.FeedSymbol("*"));
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));

            expression.FeedLiteral("Hello");
            ExpectUnexpectedExpressionOperatorException("!", () => expression.FeedSymbol("!"));
            ExpectInvalidExpressionTermException("(", () => expression.FeedSymbol("("));
            ExpectUnexpectedLiteralRequiresOperatorException("World", () => expression.FeedLiteral("World"));
            ExpectInvalidExpressionTermException("reference", () => expression.FeedSymbol("reference"));

            expression.FeedSymbol("+");
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
            expression.FeedSymbol("(");
            ExpectUnexpectedExpressionOperatorException("/", () => expression.FeedSymbol("/"));
            expression.FeedLiteral(100);
            ExpectUnexpectedLiteralRequiresOperatorException(200, () => expression.FeedLiteral(200));
            ExpectInvalidExpressionTermException("reference", () => expression.FeedSymbol("reference"));
            expression.FeedSymbol(")");
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
            ExpectUnexpectedExpressionOperatorException("!", () => expression.FeedSymbol("!"));
            ExpectInvalidExpressionTermException("reference", () => expression.FeedSymbol("reference"));
            ExpectUnexpectedLiteralRequiresOperatorException(true, () => expression.FeedLiteral(true));
        }

        [Test]
        public void TestCaseFeedingErrors2()
        {
            var expression = CreateBloatedExpression();

            expression.FeedSymbol("(");
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
            ExpectInvalidExpressionTermException(",", () => expression.FeedSymbol(","));
            expression.FeedSymbol("a");
            expression.FeedSymbol(",");
            ExpectInvalidExpressionTermException(",", () => expression.FeedSymbol(","));
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
            expression.FeedSymbol("b");
            expression.FeedSymbol(")");
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));

            Assert.AreEqual("(){@a , @b}", expression.ToString(ExpressionFormatStyle.Canonical));
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
            var resultWg = expression.Evaluate(CreateStandardTestEvaluationContext(expression));

            Assert.AreEqual("1", resultWg.ToString());
        }

        [Test]
        public void TestCaseCaseSensitivity()
        {
            var expressionIns = new Expression(ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase);
            var expressionSens = new Expression(ExpressionFlowSymbols.Default, StringComparer.Ordinal);

            var testOperator = new ArithmeticNeutralOperator("lower_case_operator", TypeConverter);

            expressionIns.RegisterOperator(testOperator);
            expressionSens.RegisterOperator(testOperator);

            Assert.IsTrue(expressionIns.IsSupportedOperator(testOperator.Symbol.ToUpper()));
            Assert.IsTrue(expressionIns.IsSupportedOperator(testOperator.Symbol));
            Assert.IsFalse(expressionSens.IsSupportedOperator(testOperator.Symbol.ToUpper()));
            Assert.IsTrue(expressionSens.IsSupportedOperator(testOperator.Symbol));

            expressionIns.FeedSymbol(testOperator.Symbol.ToUpper());
            expressionSens.FeedSymbol(testOperator.Symbol.ToUpper());

            Assert.AreEqual("lower_case_operator{??}", expressionIns.ToString(ExpressionFormatStyle.Canonical));
            Assert.AreEqual("@LOWER_CASE_OPERATOR", expressionSens.ToString(ExpressionFormatStyle.Canonical));
        }

        [Test]
        public void TestCaseCaseToString1()
        {
            var expression = CreateTestExpression("a + 'b' % ( ( c ) ) >> ( ( ( + + + t - 3 ) / ! false ) )");

            var sDef = expression.ToString();
            var sArithm = expression.ToString(ExpressionFormatStyle.Arithmetic);
            var sCanon = expression.ToString(ExpressionFormatStyle.Canonical);
            var sPolish = expression.ToString(ExpressionFormatStyle.Polish);

            Assert.AreEqual(sDef, sArithm);
            Assert.AreEqual("@a + \"b\" % ( ( @c ) ) >> ( ( ( +++@t - 3 ) / !False ) )", sArithm);
            Assert.AreEqual(">>{+{@a,%{\"b\",(){(){@c}}}},(){(){/{(){-{+{+{+{@t}}},3}},!{False}}}}}", sCanon);
            Assert.AreEqual(">> + @a % \"b\" ((@c)) ((/ (- +++@t 3) !False))", sPolish);
        }

        [Test]
        public void TestCaseCaseToString2()
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
        public void TestCaseCaseToString3()
        {
            var expression = CreateTestExpression("a + b , 1 + ( c - d - - e , 2 , 3 , ( m , n , o ) )");

            var sDef = expression.ToString();
            var sArithm = expression.ToString(ExpressionFormatStyle.Arithmetic);
            var sCanon = expression.ToString(ExpressionFormatStyle.Canonical);
            var sPolish = expression.ToString(ExpressionFormatStyle.Polish);

            Assert.AreEqual(sDef, sArithm);
            Assert.AreEqual("@a + @b , 1 + ( @c - @d - -@e , 2 , 3 , ( @m , @n , @o ) )", sArithm);
            Assert.AreEqual("+{@a,@b} , +{1,(){-{-{@c,@d},-{@e}} , 2 , 3 , (){@m , @n , @o}}}", sCanon);
            Assert.AreEqual("+ @a @b , + 1 (- - @c @d -@e , 2 , 3 , (@m , @n , @o))", sPolish);
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
        public void TestCaseMemberAccess4()
        {
            var expression = CreateBloatedExpression();

            expression.FeedSymbol("!");
            expression.FeedSymbol("variable");
            expression.FeedSymbol(".");
            ExpectUnexpectedExpressionOperatorException(".", () => expression.FeedSymbol("."));
            ExpectUnexpectedExpressionOperatorException("+", () => expression.FeedSymbol("+"));
            ExpectInvalidExpressionTermException("(", () => expression.FeedSymbol("("));
            ExpectUnexpectedLiteralRequiresIdentifierException(".", "literal", () => expression.FeedLiteral("literal"));
            ExpectUnexpectedLiteralRequiresIdentifierException(".", 1, () => expression.FeedLiteral(1));
            ExpectUnexpectedLiteralRequiresIdentifierException(".", 1.00, () => expression.FeedLiteral(1.00));
            ExpectUnexpectedLiteralRequiresIdentifierException(".", false, () => expression.FeedLiteral(false));
            ExpectUnexpectedLiteralRequiresIdentifierException(".", null, () => expression.FeedLiteral(null));

            expression.FeedSymbol("length");
            expression.FeedSymbol(".");

            ExpectUnexpectedExpressionOperatorException("+", () => expression.FeedSymbol("+"));
            ExpectInvalidExpressionTermException("(", () => expression.FeedSymbol("("));
            ExpectUnexpectedLiteralRequiresIdentifierException(".", "some_else", () => expression.FeedLiteral("some_else"));
            ExpectUnexpectedLiteralRequiresIdentifierException(".", 1, () => expression.FeedLiteral(1));
            ExpectUnexpectedLiteralRequiresIdentifierException(".", 1.00, () => expression.FeedLiteral(1.00));
            ExpectUnexpectedLiteralRequiresIdentifierException(".", false, () => expression.FeedLiteral(false));
            ExpectUnexpectedLiteralRequiresIdentifierException(".", null, () => expression.FeedLiteral(null));

            expression.FeedSymbol("some_else");
            expression.FeedSymbol("*");
            expression.FeedLiteral(100);

            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);
            Assert.AreEqual("*{!{@variable.length.some_else},100}", canonical);
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
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void TestCaseSeparatorEvaluation()
        {
            var expression = CreateTestExpression("( 1 , 2 , 3 + 4 , ( 1 , 4 , 6 ) )");
            
            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);
            Assert.AreEqual("(){1 , 2 , +{3,4} , (){1 , 4 , 6}}", canonical);

            var result = expression.Evaluate(CreateStandardTestEvaluationContext(expression));
            Assert.IsInstanceOf<IEnumerable<object>>(result);

            var list = (result as IEnumerable<object>).ToArray();
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(2, list[1]);
            Assert.AreEqual(7, list[2]);

            Assert.IsInstanceOf<IEnumerable<object>>(list[3]);
            list = (list[3] as IEnumerable<object>).ToArray();
            Assert.AreEqual(1, list[0]);
            Assert.AreEqual(4, list[1]);
            Assert.AreEqual(6, list[2]);
        }

        [Test]
        public void TestCaseFunctions1()
        {
            var expression = CreateBloatedExpression();

            expression.FeedSymbol("a");
            expression.FeedSymbol("(");
            expression.FeedSymbol("b");
            expression.FeedSymbol(")");
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
            expression.FeedSymbol("+");
            ExpectInvalidExpressionTermException(")", () => expression.FeedSymbol(")"));
            expression.FeedSymbol("c");
            expression.Construct();

            var arithmetic = expression.ToString(ExpressionFormatStyle.Arithmetic);
            var polish = expression.ToString(ExpressionFormatStyle.Polish);
            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);

            Assert.AreEqual("@a( @b ) + @c", arithmetic);
            Assert.AreEqual("+ @a(@b) @c", polish);
            Assert.AreEqual("+{@a(){@b},@c}", canonical);
        }

        [Test]
        public void TestCaseFunctions2()
        {
            var expression = CreateBloatedExpression();

            expression.FeedSymbol("a");
            expression.FeedSymbol("(");
            expression.FeedSymbol("(");
            expression.FeedSymbol("b");
            expression.FeedSymbol(",");
            expression.FeedSymbol("c");
            expression.FeedSymbol(")");
            expression.FeedSymbol(")");
            expression.Construct();

            var arithmetic = expression.ToString(ExpressionFormatStyle.Arithmetic);
            var polish = expression.ToString(ExpressionFormatStyle.Polish);
            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);

            Assert.AreEqual("@a( ( @b , @c ) )", arithmetic);
            Assert.AreEqual("@a((@b , @c))", polish);
            Assert.AreEqual("@a(){(){@b , @c}}", canonical);
        }

        [Test]
        public void TestCaseFunctions3()
        {
            var expression = CreateBloatedExpression();

            expression.FeedSymbol("a");
            expression.FeedSymbol(".");
            expression.FeedSymbol("b");
            expression.FeedSymbol("(");
            ExpectInvalidExpressionTermException(",", () => expression.FeedSymbol(","));
            expression.FeedSymbol("c");
            expression.FeedSymbol(")");
            expression.FeedSymbol(".");
            expression.FeedSymbol("d");
            expression.FeedSymbol("(");
            expression.FeedSymbol("e");
            expression.FeedSymbol(",");
            expression.FeedSymbol("f");
            expression.FeedSymbol(")");
            expression.Construct();

            var arithmetic = expression.ToString(ExpressionFormatStyle.Arithmetic);
            var polish = expression.ToString(ExpressionFormatStyle.Polish);
            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);

            Assert.AreEqual("@a.b( @c ).d( @e , @f )", arithmetic);
            Assert.AreEqual("@a.b(@c).d(@e , @f)", polish);
            Assert.AreEqual("@a.b(){@c}.d(){@e , @f}", canonical);
        }

        [Test]
        public void TestCaseFunctions4()
        {
            var expression = CreateBloatedExpression();

            expression.FeedSymbol("a");
            expression.FeedSymbol("(");
            expression.FeedSymbol("b");
            expression.FeedSymbol("(");
            expression.FeedSymbol("c");
            expression.FeedSymbol(")");
            expression.FeedSymbol(".");
            expression.FeedSymbol("d");
            expression.FeedSymbol("(");
            expression.FeedSymbol("e");
            expression.FeedSymbol(",");
            expression.FeedSymbol("f");
            expression.FeedSymbol(")");
            expression.FeedSymbol(")");
            expression.Construct();

            var arithmetic = expression.ToString(ExpressionFormatStyle.Arithmetic);
            var polish = expression.ToString(ExpressionFormatStyle.Polish);
            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);

            Assert.AreEqual("@a( @b( @c ).d( @e , @f ) )", arithmetic);
            Assert.AreEqual("@a(@b(@c).d(@e , @f))", polish);
            Assert.AreEqual("@a(){@b(){@c}.d(){@e , @f}}", canonical);
        }

        [Test]
        public void TestCaseFunctions5()
        {
            var expression = CreateBloatedExpression();

            expression.FeedSymbol("a");
            expression.FeedSymbol("(");
            expression.FeedSymbol("b");
            expression.FeedSymbol("(");
            expression.FeedSymbol(")");
            expression.FeedSymbol(")");
            expression.Construct();

            var arithmetic = expression.ToString(ExpressionFormatStyle.Arithmetic);
            var polish = expression.ToString(ExpressionFormatStyle.Polish);
            var canonical = expression.ToString(ExpressionFormatStyle.Canonical);

            Assert.AreEqual("@a( @b( ) )", arithmetic);
            Assert.AreEqual("@a(@b())", polish);
            Assert.AreEqual("@a(){@b(){}}", canonical);
        }

        [Test]
        public void EvaluateWithACancelingTokenRaisesTheExpectedException()
        {
            var cancellationTokenSource = new CancellationTokenSource();
            var evaluationContext = new EvaluationContext(true, cancellationTokenSource.Token, StringComparer.InvariantCulture,
                ObjectFormatter, new Sleeper(), (context, text) => text);

            var expression = CreateTestExpression("( Sleep ( 50 ) + Sleep ( 50 ) ) * Sleep ( 50 )");
            cancellationTokenSource.CancelAfter(50);

            Assert.Throws<OperationCanceledException>(() => expression.Evaluate(evaluationContext));
        }
    }
}

