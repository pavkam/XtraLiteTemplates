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

namespace XtraLiteTemplates.NUnit.Dialects
{
    using System;
    using System.Diagnostics.CodeAnalysis;

    using XtraLiteTemplates.Dialects.Standard.Directives;
    using System.Globalization;
    using global::NUnit.Framework;
    using XtraLiteTemplates.Dialects.Standard;
    using XtraLiteTemplates.Dialects.Standard.Operators;

    [TestFixture]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class CodeMonkeyDialectTests : TestBase
    {
        private CodeMonkeyDialect TestDialect(DialectCasing casing)
        {
            var dialect = new CodeMonkeyDialect(casing);

            Assert.IsInstanceOf<StandardSelfObject>(dialect.Self);
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

            Func<string, string> transformer = input => input;
            if (casing == DialectCasing.LowerCase)
                transformer = input => input.ToLowerInvariant();

            Assert.AreEqual(5, dialect.SpecialKeywords.Count);
            Assert.IsTrue(dialect.SpecialKeywords.ContainsKey(transformer("TRUE")) && dialect.SpecialKeywords[transformer("TRUE")].Equals(true));
            Assert.IsTrue(dialect.SpecialKeywords.ContainsKey(transformer("FALSE")) && dialect.SpecialKeywords[transformer("FALSE")].Equals(false));
            Assert.IsTrue(dialect.SpecialKeywords.ContainsKey(transformer("UNDEFINED")) && dialect.SpecialKeywords[transformer("UNDEFINED")] == null);
            Assert.IsTrue(dialect.SpecialKeywords.ContainsKey(transformer("NAN")) && dialect.SpecialKeywords[transformer("NAN")].Equals(double.NaN));
            Assert.IsTrue(dialect.SpecialKeywords.ContainsKey(transformer("INFINITY")) && dialect.SpecialKeywords[transformer("INFINITY")].Equals(double.PositiveInfinity));

            Assert.AreEqual(9, dialect.Directives.Count);
            foreach (var directive in dialect.Directives)
            {
                if (directive is ConditionalInterpolationDirective)
                    Assert.AreEqual(transformer("{$ IF $}"), directive.ToString());
                else if (directive is UsingDirective)
                    Assert.AreEqual(transformer("{? AS $}...{END}"), directive.ToString());
                else if (directive is ForDirective)
                    Assert.AreEqual(transformer("{FOR $}...{END}"), directive.ToString());
                else if (directive is ForEachDirective)
                    Assert.AreEqual(transformer("{FOR ? IN $}...{END}"), directive.ToString());
                else if (directive is SeparatedForEachDirective)
                    Assert.AreEqual(transformer("{FOR ? IN $}...{WITH}...{END}"), directive.ToString());
                else if (directive is IfDirective)
                    Assert.AreEqual(transformer("{IF $}...{END}"), directive.ToString());
                else if (directive is IfElseDirective)
                    Assert.AreEqual(transformer("{IF $}...{ELSE}...{END}"), directive.ToString());
                else if (directive is InterpolationDirective)
                    Assert.AreEqual("{$}", directive.ToString());
                else if (directive is PreFormattedUnParsedTextDirective)
                    Assert.AreEqual(transformer("{PRE}...{END}"), directive.ToString());
                else
                    Assert.Fail();
            }

            Assert.AreEqual(24, dialect.Operators.Count);
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
                else if (@operator is SequenceOperator)
                    Assert.AreEqual("..", @operator.ToString());
                else if (@operator is FormatOperator)
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

            /* 4 */
            var siic = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.IgnoreCase);
            var siuc = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.UpperCase);
            var silc = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.LowerCase);

            Assert.AreNotEqual(iic, siic);
            Assert.AreNotEqual(iuc, siuc);
            Assert.AreNotEqual(ilc, silc);
            Assert.AreNotEqual(iic.GetHashCode(), siic.GetHashCode());
            Assert.AreNotEqual(iuc.GetHashCode(), siuc.GetHashCode());
            Assert.AreNotEqual(ilc.GetHashCode(), silc.GetHashCode());

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
            var context = CreateContext(StringComparer.OrdinalIgnoreCase);

            ExpectArgumentNullException("context", () => dialect.DecorateUnParsedText(null, string.Empty));

            Assert.AreEqual("", dialect.DecorateUnParsedText(context, null));
            Assert.AreEqual("", dialect.DecorateUnParsedText(context, string.Empty));
            Assert.AreEqual("", dialect.DecorateUnParsedText(context, "    \r\n"));
        }

        [Test]
        public void TestCaseSpecialKeywordsCaseSensitiveUpper()
        {
            var dialect = new CodeMonkeyDialect(DialectCasing.UpperCase);

            var undefined = XLTemplate.Evaluate(dialect, "{UNDEFINED}");
            var _true = XLTemplate.Evaluate(dialect, "{TRUE}");
            var _false = XLTemplate.Evaluate(dialect, "{FALSE}");
            var nan = XLTemplate.Evaluate(dialect, "{NAN}");
            var infinity = XLTemplate.Evaluate(dialect, "{INFINITY}");

            Assert.AreEqual("UNDEFINED", undefined);
            Assert.AreEqual("TRUE", _true);
            Assert.AreEqual("FALSE", _false);
            Assert.AreEqual("NAN", nan);
            Assert.AreEqual("INFINITY", infinity);
        }

        [Test]
        public void TestCaseSpecialKeywordsCaseSensitiveLower()
        {
            var dialect = new CodeMonkeyDialect(DialectCasing.LowerCase);

            var undefined = XLTemplate.Evaluate(dialect, "{undefined}");
            var _true = XLTemplate.Evaluate(dialect, "{true}");
            var _false = XLTemplate.Evaluate(dialect, "{false}");
            var nan = XLTemplate.Evaluate(dialect, "{nan}");
            var infinity = XLTemplate.Evaluate(dialect, "{infinity}");

            Assert.AreEqual("undefined", undefined);
            Assert.AreEqual("true", _true);
            Assert.AreEqual("false", _false);
            Assert.AreEqual("nan", nan);
            Assert.AreEqual("infinity", infinity);
        }

        [Test]
        public void TestCaseSpecialKeywordsCaseInsensitive()
        {
            var dialect = new CodeMonkeyDialect(DialectCasing.IgnoreCase);

            var undefined = XLTemplate.Evaluate(dialect, "{undeFIned}");
            var _true = XLTemplate.Evaluate(dialect, "{tRUe}");
            var _false = XLTemplate.Evaluate(dialect, "{faLSe}");
            var nan = XLTemplate.Evaluate(dialect, "{nAn}");
            var infinity = XLTemplate.Evaluate(dialect, "{infinitY}");

            Assert.AreEqual("Undefined", undefined);
            Assert.AreEqual("True", _true);
            Assert.AreEqual("False", _false);
            Assert.AreEqual("NaN", nan);
            Assert.AreEqual("Infinity", infinity);
        }

        [Test]
        public void TestCaseShowcase1()
        {
            var customer = new
                               {
                                   FirstName = "John",
                                   LastName = "McMann",
                                   Age = 31,
                                   Loves = new[] { "Apples", "Bikes", "Everything Nice" }
                               };

            var result = XLTemplate.Evaluate(CodeMonkeyDialect.DefaultIgnoreCase, @"{pre}Hello, {_0.FirstName} {_0.LastName}. You are {_0.Age} years old and you love: {for entity in _0.loves}{entity}, {end}{end}", customer);
            Assert.AreEqual("Hello, John McMann. You are 31 years old and you love: Apples,Bikes,Everything Nice,", result);
        }

        [Test]
        public void TestCaseShowcase2()
        {
            var result = XLTemplate.Evaluate(CodeMonkeyDialect.DefaultIgnoreCase, "{Abs(_0)}", "-1");

            Assert.AreEqual("1", result);
        }
    }
}