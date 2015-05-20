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
    public class StandardDialectTests : TestBase
    {
        private StandardDialect TestDialect(CultureInfo culture, DialectCasing casing)
        {
            var dialect = new StandardDialect(culture, casing);

            Assert.AreEqual(dialect.Culture, culture);
            Assert.AreEqual('"', dialect.EndStringLiteralCharacter);
            Assert.AreEqual('}', dialect.EndTagCharacter);
            Assert.AreEqual(culture.NumberFormat.CurrencyDecimalSeparator[0], dialect.NumberDecimalSeparatorCharacter);
            Assert.AreEqual('"', dialect.StartStringLiteralCharacter);
            Assert.AreEqual('{', dialect.StartTagCharacter);
            Assert.AreEqual('\\', dialect.StringLiteralEscapeCharacter);

            var expectedComparer = StringComparer.Create(dialect.Culture, casing == DialectCasing.IgnoreCase);
            Assert.AreEqual(dialect.IdentifierComparer, expectedComparer);
            Assert.AreEqual(dialect.StringLiteralComparer, expectedComparer);

            Func<String, String> transformer = input => input;
            if (casing == DialectCasing.LowerCase)
                transformer = input => input.ToLower(culture);

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
                else if (directive is ForEachDirective)
                    Assert.AreEqual(transformer("{FOR EACH ? IN $}...{END}"), directive.ToString());
                else if (directive is IfDirective)
                    Assert.AreEqual(transformer("{IF $ THEN}...{END}"), directive.ToString());
                else if (directive is IfElseDirective)
                    Assert.AreEqual(transformer("{IF $ THEN}...{ELSE}...{END}"), directive.ToString());
                else if (directive is InterpolationDirective)
                    Assert.AreEqual("{$}", directive.ToString());
                else if (directive is RepeatDirective)
                    Assert.AreEqual(transformer("{REPEAT $ TIMES}...{END}"), directive.ToString());
                else if (directive is PreFormattedUnparsedTextDirective)
                    Assert.AreEqual(transformer("{PREFORMATTED}...{END}"), directive.ToString());
                else
                    Assert.Fail();
            }

            Assert.AreEqual(26, dialect.Operators.Count);
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
                else if (@operator is SubscriptOperator)
                    Assert.AreEqual("()", @operator.ToString());
                else
                    Assert.Fail();
            }

            return dialect;
        }

        [Test]
        public void TestCaseConstructor1()
        {
            ExpectArgumentNullException("culture", () => new StandardDialect(null, DialectCasing.IgnoreCase));
        }

        [Test]
        public void TestCaseNewInvariantIgnoreCase()
        {
            var dialect = TestDialect(CultureInfo.InvariantCulture, DialectCasing.IgnoreCase);

            Assert.AreEqual("(Ignore Case)", dialect.ToString());
        }

        [Test]
        public void TestCaseNewInvariantUpperCase()
        {
            var dialect = TestDialect(CultureInfo.InvariantCulture, DialectCasing.UpperCase);
            Assert.AreEqual("(Upper Case)", dialect.ToString());
        }

        [Test]
        public void TestCaseNewInvariantLowerCase()
        {
            var dialect = TestDialect(CultureInfo.InvariantCulture, DialectCasing.LowerCase);
            Assert.AreEqual("(Lower Case)", dialect.ToString());
        }

        [Test]
        public void TestCaseNewCurrentIgnoreCase()
        {
            var dialect = TestDialect(CultureInfo.CurrentCulture, DialectCasing.IgnoreCase);
            Assert.AreEqual(String.Format("{0} (Ignore Case)", CultureInfo.CurrentCulture.Name), dialect.ToString());
        }

        [Test]
        public void TestCaseNewCurrentLowerCase()
        {
            var dialect = TestDialect(CultureInfo.CurrentCulture, DialectCasing.LowerCase);
            Assert.AreEqual(String.Format("{0} (Lower Case)", CultureInfo.CurrentCulture.Name), dialect.ToString());
        }

        [Test]
        public void TestCaseNewCurrentUpperCase()
        {
            var dialect = TestDialect(CultureInfo.CurrentCulture, DialectCasing.UpperCase);
            Assert.AreEqual(String.Format("{0} (Upper Case)", CultureInfo.CurrentCulture.Name), dialect.ToString());
        }

        [Test]
        public void TestCaseEquality()
        {
            /* Build dialects. */
            var iic = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.IgnoreCase);
            var iuc = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.UpperCase);
            var ilc = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.LowerCase);
            var ccic = new StandardDialect(CultureInfo.CurrentCulture, DialectCasing.IgnoreCase);
            var ccuc = new StandardDialect(CultureInfo.CurrentCulture, DialectCasing.UpperCase);
            var cclc = new StandardDialect(CultureInfo.CurrentCulture, DialectCasing.LowerCase);

            /* 1 */
            Assert.AreEqual(iic, iic);           
            Assert.AreNotEqual(iic, iuc);
            Assert.AreNotEqual(iic.GetHashCode(), iuc.GetHashCode());
            Assert.AreNotEqual(iic, ilc);
            Assert.AreNotEqual(iic.GetHashCode(), ilc.GetHashCode());
            Assert.AreNotEqual(iic, ccic);
            Assert.AreNotEqual(iic.GetHashCode(), ccic.GetHashCode());
            Assert.AreNotEqual(iic, ccuc);
            Assert.AreNotEqual(iic.GetHashCode(), ccuc.GetHashCode());
            Assert.AreNotEqual(iic, cclc);
            Assert.AreNotEqual(iic.GetHashCode(), cclc.GetHashCode());

            /* 2 */
            Assert.AreEqual(iuc, iuc);
            Assert.AreNotEqual(iuc, ilc);
            Assert.AreNotEqual(iuc.GetHashCode(), ilc.GetHashCode());
            Assert.AreNotEqual(iuc, ccic);
            Assert.AreNotEqual(iuc.GetHashCode(), ccic.GetHashCode());
            Assert.AreNotEqual(iuc, ccuc);
            Assert.AreNotEqual(iuc.GetHashCode(), ccuc.GetHashCode());
            Assert.AreNotEqual(iuc, cclc);
            Assert.AreNotEqual(iuc.GetHashCode(), cclc.GetHashCode());

            /* 3 */
            Assert.AreEqual(ilc, ilc);
            Assert.AreNotEqual(ilc, ccic);
            Assert.AreNotEqual(ilc.GetHashCode(), ccic.GetHashCode());
            Assert.AreNotEqual(ilc, ccuc);
            Assert.AreNotEqual(ilc.GetHashCode(), ccuc.GetHashCode());
            Assert.AreNotEqual(ilc, cclc);
            Assert.AreNotEqual(ilc.GetHashCode(), cclc.GetHashCode());

            /* 4 */
            Assert.AreEqual(ccic, ccic);            
            Assert.AreNotEqual(ccic, ccuc);
            Assert.AreNotEqual(ccic.GetHashCode(), ccuc.GetHashCode());
            Assert.AreNotEqual(ccic, cclc);
            Assert.AreNotEqual(ccic.GetHashCode(), cclc.GetHashCode());

            /* 5 */
            Assert.AreEqual(ccuc, ccuc);
            Assert.AreNotEqual(ccuc, cclc);
            Assert.AreNotEqual(ccuc.GetHashCode(), cclc.GetHashCode());

            /* 6 */
            Assert.AreEqual(cclc, cclc);

            /* Special */
            Assert.AreNotEqual(cclc, this);
            Assert.AreNotEqual(cclc, null);
        }

        [Test]
        public void TestCaseDefaultConstructedAndStatic()
        {
            var parameterless = new StandardDialect();
            var iic = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.IgnoreCase);
            var iuc = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.UpperCase);

            Assert.AreEqual(iic, parameterless);
            Assert.AreEqual(iic.GetHashCode(), parameterless.GetHashCode());

            Assert.AreEqual(iuc, StandardDialect.Default);
            Assert.AreEqual(iuc.GetHashCode(), StandardDialect.Default.GetHashCode());

            Assert.AreEqual(iic, StandardDialect.DefaultIgnoreCase);
            Assert.AreEqual(iic.GetHashCode(), StandardDialect.DefaultIgnoreCase.GetHashCode());
        }


        [Test]
        public void TestCaseUnparsedTextDecoration()
        {
            var dialect = new StandardDialect();
            var context = new TestEvaluationContext(StringComparer.OrdinalIgnoreCase);
            context.OpenEvaluationFrame();

            ExpectArgumentNullException("context", () => dialect.DecorateUnparsedText(null, String.Empty));

            Assert.AreEqual("", dialect.DecorateUnparsedText(context, null));
            Assert.AreEqual("", dialect.DecorateUnparsedText(context, String.Empty));
            Assert.AreEqual("", dialect.DecorateUnparsedText(context, "    \r\n"));
        }
    }
}