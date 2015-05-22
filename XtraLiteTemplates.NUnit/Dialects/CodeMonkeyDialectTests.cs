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

namespace XtraLiteTemplates.NUnit.Dialects
{
    using System;
    using System.IO;
    using System.Linq;
    using XtraLiteTemplates.NUnit.Inside;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Parsing;
    using XtraLiteTemplates.Dialects.Standard.Directives;
    using System.Globalization;
    using XtraLiteTemplates.Dialects.Standard;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions.Operators;

    [TestFixture]
    public class CodeMonkeyDialectTests : TestBase
    {
        private CodeMonkeyDialect TestDialect(DialectCasing casing)
        {
            var dialect = new CodeMonkeyDialect(casing);

            Assert.AreEqual(dialect.Culture, CultureInfo.InvariantCulture);
            Assert.AreEqual('\'', dialect.EndStringLiteralCharacter);
            Assert.AreEqual('}', dialect.EndTagCharacter);
            Assert.AreEqual('.', dialect.NumberDecimalSeparatorCharacter);
            Assert.AreEqual('\'', dialect.StartStringLiteralCharacter);
            Assert.AreEqual('{', dialect.StartTagCharacter);
            Assert.AreEqual('\\', dialect.StringLiteralEscapeCharacter);

            Assert.NotNull(dialect.FlowSymbols);
            Assert.AreEqual("(", dialect.FlowSymbols.GroupOpen);
            Assert.AreEqual(")", dialect.FlowSymbols.GroupClose);
            Assert.AreEqual(",", dialect.FlowSymbols.Separator);
            Assert.AreEqual(".", dialect.FlowSymbols.MemberAccess);

            var expectedComparer = StringComparer.Create(dialect.Culture, casing == DialectCasing.IgnoreCase);
            Assert.AreEqual(dialect.IdentifierComparer, expectedComparer);
            Assert.AreEqual(dialect.StringLiteralComparer, expectedComparer);

            Func<String, String> transformer = input => input;
            if (casing == DialectCasing.LowerCase)
                transformer = input => input.ToLowerInvariant();

            Assert.AreEqual(5, dialect.SpecialKeywords.Count);
            Assert.IsTrue(dialect.SpecialKeywords.ContainsKey(transformer("TRUE")) && dialect.SpecialKeywords[transformer("TRUE")].Equals(true));
            Assert.IsTrue(dialect.SpecialKeywords.ContainsKey(transformer("FALSE")) && dialect.SpecialKeywords[transformer("FALSE")].Equals(false));
            Assert.IsTrue(dialect.SpecialKeywords.ContainsKey(transformer("UNDEFINED")) && dialect.SpecialKeywords[transformer("UNDEFINED")] == null);
            Assert.IsTrue(dialect.SpecialKeywords.ContainsKey(transformer("NAN")) && dialect.SpecialKeywords[transformer("NAN")].Equals(Double.NaN));
            Assert.IsTrue(dialect.SpecialKeywords.ContainsKey(transformer("INFINITY")) && dialect.SpecialKeywords[transformer("INFINITY")].Equals(Double.PositiveInfinity));

            Assert.AreEqual(7, dialect.Directives.Count);
            foreach (var directive in dialect.Directives)
            {
                if (directive is ConditionalInterpolationDirective)
                    Assert.AreEqual(transformer("{$ IF $}"), directive.ToString());
                else if (directive is ForDirective)
                    Assert.AreEqual(transformer("{FOR $}...{END}"), directive.ToString());
                else if (directive is ForEachDirective)
                    Assert.AreEqual(transformer("{FOR ? IN $}...{END}"), directive.ToString());
                else if (directive is IfDirective)
                    Assert.AreEqual(transformer("{IF $}...{END}"), directive.ToString());
                else if (directive is IfElseDirective)
                    Assert.AreEqual(transformer("{IF $}...{ELSE}...{END}"), directive.ToString());
                else if (directive is InterpolationDirective)
                    Assert.AreEqual("{$}", directive.ToString());
                else if (directive is PreFormattedUnparsedTextDirective)
                    Assert.AreEqual(transformer("{PRE}...{END}"), directive.ToString());
                else
                    Assert.Fail();
            }

            Assert.AreEqual(25, dialect.Operators.Count);
            foreach (var @operator in dialect.Operators)
            {
                if (@operator is RelationalEqualsOperator)
                    Assert.AreEqual("==", @operator.ToString());
                else if (@operator is RelationalNotEqualsOperator)
                    Assert.AreEqual("!=", @operator.ToString());
                else if (@operator is RelationalGreaterThanOperator)
                    Assert.AreEqual(">", @operator.ToString());
                else if (@operator is RelationalGreaterThanOrEqualsOperator)
                    Assert.AreEqual(">=", @operator.ToString());
                else if (@operator is RelationalLowerThanOperator)
                    Assert.AreEqual("<", @operator.ToString());
                else if (@operator is RelationalLowerThanOrEqualsOperator)
                    Assert.AreEqual("<=", @operator.ToString());
                else if (@operator is LogicalAndOperator)
                    Assert.AreEqual("&&", @operator.ToString());
                else if (@operator is LogicalOrOperator)
                    Assert.AreEqual("||", @operator.ToString());
                else if (@operator is LogicalNotOperator)
                    Assert.AreEqual("!", @operator.ToString());
                else if (@operator is BitwiseAndOperator)
                    Assert.AreEqual("&", @operator.ToString());
                else if (@operator is BitwiseOrOperator)
                    Assert.AreEqual("|", @operator.ToString());
                else if (@operator is BitwiseXorOperator)
                    Assert.AreEqual("^", @operator.ToString());
                else if (@operator is BitwiseNotOperator)
                    Assert.AreEqual("~", @operator.ToString());
                else if (@operator is BitwiseShiftLeftOperator)
                    Assert.AreEqual("<<", @operator.ToString());
                else if (@operator is BitwiseShiftRightOperator)
                    Assert.AreEqual(">>", @operator.ToString());
                else if (@operator is ArithmeticDivideOperator)
                    Assert.AreEqual("/", @operator.ToString());
                else if (@operator is ArithmeticModuloOperator)
                    Assert.AreEqual("%", @operator.ToString());
                else if (@operator is ArithmeticMultiplyOperator)
                    Assert.AreEqual("*", @operator.ToString());
                else if (@operator is ArithmeticNegateOperator)
                    Assert.AreEqual("-", @operator.ToString());
                else if (@operator is ArithmeticNeutralOperator)
                    Assert.AreEqual("+", @operator.ToString());
                else if (@operator is ArithmeticSubtractOperator)
                    Assert.AreEqual("-", @operator.ToString());
                else if (@operator is ArithmeticSumOperator)
                    Assert.AreEqual("+", @operator.ToString());
                else if (@operator is MemberAccessOperator)
                    Assert.AreEqual(".", @operator.ToString());
                else if (@operator is IntegerRangeOperator)
                    Assert.AreEqual("..", @operator.ToString());
                else if (@operator is ValueFormatOperator)
                    Assert.AreEqual(":", @operator.ToString());
                else
                    Assert.Fail();
            }

            return dialect;
        }

        [Test]
        public void TestCaseNewInvariantIgnoreCase()
        {
            var dialect = TestDialect(DialectCasing.IgnoreCase);

            Assert.AreEqual("Code Monkey (Ignore Case)", dialect.ToString());
        }

        [Test]
        public void TestCaseNewInvariantUpperCase()
        {
            var dialect = TestDialect(DialectCasing.UpperCase);
            Assert.AreEqual("Code Monkey (Upper Case)", dialect.ToString());
        }

        [Test]
        public void TestCaseNewInvariantLowerCase()
        {
            var dialect = TestDialect(DialectCasing.LowerCase);
            Assert.AreEqual("Code Monkey (Lower Case)", dialect.ToString());
        }

        [Test]
        public void TestCaseEquality()
        {
            /* Build dialects. */
            var iic = new CodeMonkeyDialect(DialectCasing.IgnoreCase);
            var iuc = new CodeMonkeyDialect(DialectCasing.UpperCase);
            var ilc = new CodeMonkeyDialect(DialectCasing.LowerCase);

            /* 1 */
            Assert.AreEqual(iic, iic);           
            Assert.AreNotEqual(iic, iuc);
            Assert.AreNotEqual(iic.GetHashCode(), iuc.GetHashCode());
            Assert.AreNotEqual(iic, ilc);
            Assert.AreNotEqual(iic.GetHashCode(), ilc.GetHashCode());

            /* 2 */
            Assert.AreEqual(iuc, iuc);
            Assert.AreNotEqual(iuc, ilc);
            Assert.AreNotEqual(iuc.GetHashCode(), ilc.GetHashCode());

            /* 3 */
            Assert.AreEqual(ilc, ilc);

            /* Special */
            Assert.AreNotEqual(iic, this);
            Assert.AreNotEqual(iic, null);
        }

        [Test]
        public void TestCaseDefaultConstructedAndStatic()
        {
            var parameterless = new CodeMonkeyDialect();
            var iic = new CodeMonkeyDialect(DialectCasing.IgnoreCase);
            var iuc = new CodeMonkeyDialect(DialectCasing.UpperCase);

            Assert.AreEqual(iic, parameterless);
            Assert.AreEqual(iic.GetHashCode(), parameterless.GetHashCode());

            Assert.AreEqual(iuc, CodeMonkeyDialect.Default);
            Assert.AreEqual(iuc.GetHashCode(), CodeMonkeyDialect.Default.GetHashCode());

            Assert.AreEqual(iic, CodeMonkeyDialect.DefaultIgnoreCase);
            Assert.AreEqual(iic.GetHashCode(), CodeMonkeyDialect.DefaultIgnoreCase.GetHashCode());
        }

        [Test]
        public void TestCaseUnparsedTextDecoration()
        {
            var dialect = new CodeMonkeyDialect();
            var context = new TestEvaluationContext(StringComparer.OrdinalIgnoreCase);
            context.OpenEvaluationFrame();

            ExpectArgumentNullException("context", () => dialect.DecorateUnparsedText(null, String.Empty));

            Assert.AreEqual("", dialect.DecorateUnparsedText(context, null));
            Assert.AreEqual("", dialect.DecorateUnparsedText(context, String.Empty));
            Assert.AreEqual("", dialect.DecorateUnparsedText(context, "    \r\n"));
        }

        [Test]
        public void TestCaseShowcase1()
        {
            var customer = new
            {
                FirstName = "John",
                LastName = "McMann",
                Age = 31,
                Loves = new String [] { "Apples", "Bikes", "Everything Nice" }
            };

            var result = XLTemplate.Evaluate(CodeMonkeyDialect.DefaultIgnoreCase, @"{pre}Hello, {_0.FirstName} {_0.LastName}. You are {_0.Age} years old and you love: {for entity in _0.loves}{entity}, {end}{end}", customer);            
            Assert.AreEqual("Hello, John McMann. You are 31 years old and you love: Apples,Bikes,Everything Nice,", result);
        }
    }
}