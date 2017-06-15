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

namespace XtraLiteTemplates.NUnit
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;

    using Expressions;

    using global::NUnit.Framework;

    using Parsing;

    using XtraLiteTemplates.Dialects.Standard.Operators;

    [TestFixture]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class LexerTests : TestBase
    {
        private static void AssertUnParsedLex(Lex lex, int firstCharacterIndex, int originalLength, string unParsedText)
        {
            Assert.IsInstanceOf<UnParsedLex>(lex);
            var unParsedLex = lex as UnParsedLex;
            Debug.Assert(unParsedLex != null);

            Assert.AreEqual(firstCharacterIndex, unParsedLex.FirstCharacterIndex);
            Assert.AreEqual(originalLength, unParsedLex.OriginalLength);
            Assert.AreEqual(unParsedText, unParsedLex.UnParsedText);
        }

        private static void AssertUnParsedLex(Lex lex, int firstCharacterIndex, string unParsedText)
        {
            AssertUnParsedLex(lex, firstCharacterIndex, unParsedText.Length, unParsedText);
        }

        private static void AssertTagLex(Lex lex, int firstCharacterIndex, int originalLength, Tag tag, string asText)
        {
            Assert.IsInstanceOf<TagLex>(lex);
            var tagLex = lex as TagLex;
            Debug.Assert(tagLex != null);

            Assert.AreEqual(firstCharacterIndex, tagLex.FirstCharacterIndex);
            Assert.AreEqual(originalLength, tagLex.OriginalLength);
            Assert.AreEqual(tag, tagLex.Tag);

            var allText = string.Join("|", tagLex.Components.Select(s => s.ToString()));
            Assert.AreEqual(asText, allText);
        }


        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseConstruction1()
        {
            var tokenizer = new Tokenizer("irrelevant");

            ExpectArgumentNullException("tokenizer", () => new Lexer(null, ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase));
            ExpectArgumentNullException("expressionFlowSymbols", () => new Lexer(tokenizer, null, StringComparer.OrdinalIgnoreCase));
            ExpectArgumentNullException("comparer", () => new Lexer(tokenizer, ExpressionFlowSymbols.Default, null));

            var lexer = new Lexer(tokenizer, ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase);

            Assert.AreSame(tokenizer, lexer.Tokenizer);
            Assert.AreSame(StringComparer.OrdinalIgnoreCase, lexer.Comparer);
        }

        [Test]
        [SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
        public void TestCaseTagRegistration()
        {
            var tokenizer = new Tokenizer("irrelevant");
            var lexer = new Lexer(tokenizer, ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase);
            var emptyTag = new Tag();

            Assert.AreEqual(0, lexer.Tags.Count);

            ExpectArgumentNullException("tag", () => lexer.RegisterTag(null));
            ExpectCannotRegisterTagWithNoComponentsException(() => lexer.RegisterTag(emptyTag));

            emptyTag.Expression();
            lexer.RegisterTag(emptyTag);
            Assert.AreEqual(emptyTag, lexer.Tags.Single());

            lexer.RegisterTag(emptyTag);
            Assert.AreEqual(emptyTag, lexer.Tags.Single());
        }

        [Test]
        [SuppressMessage("ReSharper", "ImplicitlyCapturedClosure")]
        public void TestCaseOperatorRegistration()
        {
            var tokenizer = new Tokenizer("irrelevant");
            var lexer = new Lexer(tokenizer, ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase);

            var neutral = new ArithmeticNeutralOperator(TypeConverter);
            var sum = new ArithmeticSumOperator(TypeConverter);

            ExpectArgumentNullException("operator", () => lexer.RegisterOperator(null));

            Assert.AreEqual(lexer, lexer.RegisterOperator(neutral));
            Assert.AreEqual(lexer, lexer.RegisterOperator(sum));

            ExpectOperatorAlreadyRegisteredException(neutral.ToString(), () => lexer.RegisterOperator(neutral));
            ExpectOperatorAlreadyRegisteredException(sum.ToString(), () => lexer.RegisterOperator(sum));

            /* Clashing with specials */
            lexer.RegisterSpecial("FALSE", false);

            var clashingOp1 = new ArithmeticNeutralOperator("FALSE", TypeConverter);
            var clashingOp2 = new ArithmeticSumOperator("FALSE", TypeConverter);

            ExpectOperatorAlreadyRegisteredException(clashingOp1.ToString(), () => lexer.RegisterOperator(clashingOp1));
            ExpectOperatorAlreadyRegisteredException(clashingOp2.ToString(), () => lexer.RegisterOperator(clashingOp2));

            /* Flow */
            ExpectOperatorAlreadyRegisteredException("(", () => lexer.RegisterOperator(new ArithmeticNeutralOperator("(", TypeConverter)));
            lexer.RegisterOperator(new ArithmeticNeutralOperator(")", TypeConverter));
            lexer.RegisterOperator(new ArithmeticNeutralOperator(".", TypeConverter));
            lexer.RegisterOperator(new ArithmeticNeutralOperator(",", TypeConverter));
            lexer.RegisterOperator(new ArithmeticSumOperator("(", TypeConverter));
            ExpectOperatorAlreadyRegisteredException(")", () => lexer.RegisterOperator(new ArithmeticSumOperator(")", TypeConverter)));
            ExpectOperatorAlreadyRegisteredException(".", () => lexer.RegisterOperator(new ArithmeticSumOperator(".", TypeConverter)));
            ExpectOperatorAlreadyRegisteredException(",", () => lexer.RegisterOperator(new ArithmeticSumOperator(",", TypeConverter)));
        }

        [Test]
        public void TestCaseSpecialRegistration()
        {
            var tokenizer = new Tokenizer("irrelevant");
            var lexer = new Lexer(tokenizer, ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase);

            /* Exceptions */
            ExpectArgumentNullException("keyword", () => lexer.RegisterSpecial(null, null));
            ExpectArgumentEmptyException("keyword", () => lexer.RegisterSpecial(string.Empty, null));
            ExpectArgumentNotIdentifierException("keyword", () => lexer.RegisterSpecial("12ABC", null));

            var clashingOp1 = new ArithmeticNeutralOperator("T1", TypeConverter);
            var clashingOp2 = new ArithmeticSumOperator("T2", TypeConverter);

            lexer.RegisterOperator(clashingOp1);
            lexer.RegisterOperator(clashingOp2);

            ExpectSpecialCannotBeRegisteredException("T1", () => lexer.RegisterSpecial("T1", true));
            ExpectSpecialCannotBeRegisteredException("T2", () => lexer.RegisterSpecial("T2", true));

            Assert.AreEqual(lexer, lexer.RegisterSpecial("FALSE", false));
            Assert.AreEqual(lexer, lexer.RegisterSpecial("FALSE", false));
        }

        [Test]
        public void TestCaseEmptyInputString()
        {
            var test = string.Empty;
            var lexer = new Lexer(new Tokenizer(test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase);

            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseUnParsedOnlyInputString()
        {
            const string Test = "unparsed text is here baby {{ and some other {{{{ tokens also.";
            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase);

            AssertUnParsedLex(lexer.ReadNext(), 0, Test.Length, Test.Replace("{{", "{"));
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseOneKeywordTagOnly()
        {
            const string Test = "{TAG}";
            var tag = new Tag().Keyword("TAG");
            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase).RegisterTag(tag);

            AssertTagLex(lexer.ReadNext(), 0, Test.Length, tag, "TAG");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseTwoKeywordTagsAndUnParsed()
        {
            const string Test = "{K1 K2}unparsed{K1 K3}";

            var tag1 = new Tag().Keyword("K1").Keyword("K2");
            var tag2 = new Tag().Keyword("K1").Keyword("K3");

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase).
                RegisterTag(tag1).
                RegisterTag(tag2);

            AssertTagLex(lexer.ReadNext(), 0, 7, tag1, "K1|K2");
            AssertUnParsedLex(lexer.ReadNext(), 7, "unparsed");
            AssertTagLex(lexer.ReadNext(), 15, 7, tag2, "K1|K3");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseEmptyTag()
        {
            const string Test = "{}";

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase);
            ExpectNoMatchingTagsLeftException(null, 1, 1, "}", Token.TokenType.EndTag, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseEndOfStream1()
        {
            const string Test = "{A ";

            var lexer = new Lexer(new Tokenizer(Test),
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase).RegisterTag(Tag.Parse("?"));

            ExpectUnexpectedEndOfStreamException(3, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseUnidentifiedTag1()
        {
            const string Test = "{BAG}";
            var tag = new Tag().Keyword("TAG");
            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase).RegisterTag(tag);

            ExpectNoMatchingTagsLeftException(null, 1, 3, "BAG", Token.TokenType.Word, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseUnidentifiedTag2()
        {
            const string Test = "{TAG AHA}";
            var tag = new Tag().Keyword("TAG");
            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase).RegisterTag(tag);

            ExpectNoMatchingTagsLeftException(new object[] { "TAG" }, 5, 3, "AHA", Token.TokenType.Word, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseJustExpressionTag()
        {
            const string Test = "{1.33}";
            var tag = new Tag().Expression();
            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase).RegisterTag(tag);

            AssertTagLex(lexer.ReadNext(), 0, 6, tag, 1.33.ToString(CultureInfo.InvariantCulture));
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseExoticSymbolOrdering()
        {
            const string Test = "{100 <<-100<<.100-----.1<<-1}";

            var tag = new Tag().Expression();
            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag)
                .RegisterOperator(new BitwiseShiftLeftOperator(TypeConverter))
                .RegisterOperator(new ArithmeticSubtractOperator(TypeConverter))
                .RegisterOperator(new ArithmeticNegateOperator(TypeConverter));

            AssertTagLex(lexer.ReadNext(), 0, 29, tag, "100 << -100 << " + 0.1.ToString(CultureInfo.CurrentCulture) + " - ----" + 0.1.ToString(CultureInfo.CurrentCulture) + " << -1");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseKeywordAndExpressionTag()
        {
            const string Test = "{IF 10 == 10}";

            var ifTag = new Tag().Keyword("IF").Expression();
            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterTag(ifTag)
                .RegisterOperator(new RelationalEqualsOperator(StringComparer.CurrentCulture, TypeConverter));

            AssertTagLex(lexer.ReadNext(), 0, 13, ifTag, "IF|10 == 10");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseSpecialKeyword()
        {
            const string Test = "{IF TRUE}";

            var ifTag = new Tag().Keyword("IF").Expression();
            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterTag(ifTag)
                .RegisterSpecial("TRUE", 100.5);

            AssertTagLex(lexer.ReadNext(), 0, 9, ifTag, "IF|" + 100.5.ToString(CultureInfo.CurrentCulture));
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseSpecialKeywordMismatch()
        {
            const string Test = "{IF TRUE}";

            var ifTag = new Tag().Keyword("IF").Keyword("TRUE").Expression();
            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterTag(ifTag)
                .RegisterSpecial("TRUE", 100.5);

            ExpectNoMatchingTagsLeftException(new object[] { "IF" }, 4, 4, "TRUE", Token.TokenType.Word, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseAmbiguousTags1()
        {
            const string Test = "{IF SET Alpha}";

            var ifTag1 = new Tag().Keyword("IF").Expression();
            var ifTag2 = new Tag().Keyword("IF").Keyword("SET").Expression();

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterTag(ifTag1)
                .RegisterTag(ifTag2);

            AssertTagLex(lexer.ReadNext(), 0, 14, ifTag2, "IF|SET|@Alpha");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseAmbiguousTags2()
        {
            const string Var1 = "{1}";
            const string Var2 = "{A}";
            const string Var3 = "{1 THEN}";
            const string Var4 = "{A THEN}";

            var tag1 = Tag.Parse("$");
            var tag2 = Tag.Parse("$ THEN");

            var lexer1 = new Lexer(new Tokenizer(Var1), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag1).RegisterTag(tag2);
            var lexer2 = new Lexer(new Tokenizer(Var2), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag1).RegisterTag(tag2);
            var lexer3 = new Lexer(new Tokenizer(Var3), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag1).RegisterTag(tag2);
            var lexer4 = new Lexer(new Tokenizer(Var4), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag1).RegisterTag(tag2);

            AssertTagLex(lexer1.ReadNext(), 0, 3, tag1, "1");
            AssertTagLex(lexer2.ReadNext(), 0, 3, tag1, "@A");
            AssertTagLex(lexer3.ReadNext(), 0, 8, tag2, "1|THEN");
            AssertTagLex(lexer4.ReadNext(), 0, 8, tag2, "@A|THEN");
        }

        [Test]
        public void TestCaseAmbiguousTags3()
        {
            const string Test = "{i}";

            var tag1 = Tag.Parse("$");
            var tag2 = Tag.Parse("?");

            var lexer1 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag1).RegisterTag(tag2);
            var lexer2 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer1.ReadNext(), 0, 3, tag2, "i");
            AssertTagLex(lexer2.ReadNext(), 0, 3, tag2, "i");
        }

        [Test]
        public void TestCaseAmbiguousTags4()
        {
            const string Test = "{i}";

            var tag1 = Tag.Parse("$");
            var tag2 = Tag.Parse("? THEN");

            var lexer1 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag1).RegisterTag(tag2);
            var lexer2 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer1.ReadNext(), 0, 3, tag1, "@i");
            AssertTagLex(lexer2.ReadNext(), 0, 3, tag1, "@i");
        }

        [Test]
        public void TestCaseAmbiguousTags5()
        {
            const string Test = "{a}";

            var tag1 = Tag.Parse("?");
            var tag2 = Tag.Parse("a");

            var lexer1 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag1).RegisterTag(tag2);
            var lexer2 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer1.ReadNext(), 0, 3, tag2, "a");
            AssertTagLex(lexer2.ReadNext(), 0, 3, tag2, "a");
        }

        [Test]
        public void TestCaseAmbiguousTags6()
        {
            const string Test = "{a}";

            var tag1 = Tag.Parse("?");
            var tag2 = Tag.Parse("A");

            var lexer1 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.Ordinal).RegisterTag(tag1).RegisterTag(tag2);
            var lexer2 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.Ordinal).RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer1.ReadNext(), 0, 3, tag1, "a");
            AssertTagLex(lexer2.ReadNext(), 0, 3, tag1, "a");
        }

        [Test]
        public void TestCaseAmbiguousTags7()
        {
            const string Test = "{a B}";

            var tag1 = Tag.Parse("a b");
            var tag2 = Tag.Parse("a B");
            var tag3 = Tag.Parse("A b");
            var tag4 = Tag.Parse("A B");

            var lexer1 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.Ordinal)
                .RegisterTag(tag1).RegisterTag(tag2).RegisterTag(tag3).RegisterTag(tag4);

            AssertTagLex(lexer1.ReadNext(), 0, 5, tag2, "a|B");
        }

        [Test]
        public void TestCaseAmbiguousTags8()
        {
            const string Test = "{IF A THEN B}";

            var tag1 = Tag.Parse("IF ? THEN $");
            var tag2 = Tag.Parse("IF $ THEN ?");

            var lexer1 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag1).RegisterTag(tag2);
            var lexer2 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer1.ReadNext(), 0, 13, tag2, "IF|@A|THEN|B");
            AssertTagLex(lexer2.ReadNext(), 0, 13, tag2, "IF|@A|THEN|B");
        }

        [Test]
        public void TestCaseAmbiguousTags9()
        {
            const string Test = "{IF A THEN 1}";

            var tag1 = Tag.Parse("IF $ THEN ?");
            var tag2 = Tag.Parse("IF $ THEN $");

            var lexer1 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag1).RegisterTag(tag2);
            var lexer2 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer1.ReadNext(), 0, 13, tag2, "IF|@A|THEN|1");
            AssertTagLex(lexer2.ReadNext(), 0, 13, tag2, "IF|@A|THEN|1");
        }

        [Test]
        public void TestCaseAmbiguousTags10()
        {
            const string Test = "{IF A THEN}";

            var tag1 = Tag.Parse("IF ? THEN OTHER");
            var tag2 = Tag.Parse("IF (A B C) THEN");
            var tag3 = Tag.Parse("IF $ THEN");

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag1)
                .RegisterTag(tag2)
                .RegisterTag(tag3);

            AssertTagLex(lexer.ReadNext(), 0, 11, tag2, "IF|A|THEN");
        }

        [Test]
        public void TestCaseAmbiguousTags11()
        {
            const string Test = "{IF A THEN}";

            var tag1 = Tag.Parse("IF ? THEN OTHER");
            var tag2 = Tag.Parse("IF (A B C) THEN SOME");
            var tag3 = Tag.Parse("IF $ THEN");

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag1)
                .RegisterTag(tag2)
                .RegisterTag(tag3);

            AssertTagLex(lexer.ReadNext(), 0, 11, tag3, "IF|@A|THEN");
        }

        [Test]
        [SuppressMessage("ReSharper", "AccessToModifiedClosure")]
        public void TestCaseBadTokenOrder()
        {
            var tag = new Tag().Expression();

            var lexer = new Lexer(new Tokenizer("{10ALPHA}"), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(3, 5, "ALPHA", Token.TokenType.Word, () => lexer.ReadNext());

            lexer = new Lexer(new Tokenizer("{10\"ALPHA\"}"), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(3, 7, "ALPHA", Token.TokenType.String, () => lexer.ReadNext());

            lexer = new Lexer(new Tokenizer("{AB\"ALPHA\"}"), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(3, 7, "ALPHA", Token.TokenType.String, () => lexer.ReadNext());

            lexer = new Lexer(new Tokenizer("{\"ALPHA\"AB}"), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(8, 2, "AB", Token.TokenType.Word, () => lexer.ReadNext());

            lexer = new Lexer(new Tokenizer("{\"ALPHA\"10}"), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(8, 2, "10", Token.TokenType.Number, () => lexer.ReadNext());

            lexer = new Lexer(new Tokenizer("{\"ALPHA\"\"BETA\"}"), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(8, 6, "BETA", Token.TokenType.String, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseIncompleteTag()
        {
            const string Test = "{A any B 2 C }";

            var tag = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C").Keyword("D");
            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase).RegisterTag(tag);

            ExpectNoMatchingTagsLeftException(new object[] {"A", "any", "B", "2", "C" }, 13, 1, "}", Token.TokenType.EndTag, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseIncompleteAndCompleteSelectionTags()
        {
            const string Test = "{A any B 2 C }";

            var tag1 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C").Keyword("D");
            var tag2 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C");

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag1).RegisterTag(tag2);

            AssertTagLex(lexer.ReadNext(), 0, 14, tag2, "A|any|B|2|C");
        }

        [Test]
        public void TestCaseAmbiguousTagSelection1()
        {
            const string Test = "{A any B 2 C }";

            var tag1 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C");
            var tag2 = new Tag().Keyword("A").Expression().Keyword("B").Expression().Keyword("C");

            var lexer12 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag1).RegisterTag(tag2);
            var lexer21 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer12.ReadNext(), 0, 14, tag1, "A|any|B|2|C");
            AssertTagLex(lexer21.ReadNext(), 0, 14, tag1, "A|any|B|2|C");
        }

        [Test]
        public void TestCaseAmbiguousTagSelection2()
        {
            const string Test = "{A any B 2 C }";

            var tag1 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C");
            var tag2 = new Tag().Keyword("A").Identifier("any").Keyword("B").Expression().Keyword("C");

            var lexer12 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag1).RegisterTag(tag2);
            var lexer21 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer12.ReadNext(), 0, 14, tag1, "A|any|B|2|C");
            AssertTagLex(lexer21.ReadNext(), 0, 14, tag2, "A|any|B|2|C");
        }

        [Test]
        public void TestCaseAmbiguousTagSelection3()
        {
            const string Test = "{A any B 2 C }";

            var tag1 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C");
            var tag2 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C");

            var lexer12 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag1).RegisterTag(tag2);
            var lexer21 = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer12.ReadNext(), 0, 14, tag1, "A|any|B|2|C");
            AssertTagLex(lexer21.ReadNext(), 0, 14, tag2, "A|any|B|2|C");
        }

        [Test]
        public void TestCaseBrokenExpression1()
        {
            const string Test = "{1 +}";

            var exprTag = new Tag().Expression();

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterTag(exprTag).RegisterOperator(new ArithmeticSumOperator(TypeConverter));

            ExpectUnbalancedExpressionCannotBeFinalizedException(4, "}", Token.TokenType.EndTag, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseBrokenExpression2()
        {
            const string Test = "{1 + END}";

            var exprTag = new Tag().Expression().Keyword("END");

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterTag(exprTag).RegisterOperator(new ArithmeticSumOperator(TypeConverter));

            ExpectUnbalancedExpressionCannotBeFinalizedException(5, "END", Token.TokenType.Word, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseCaseSensitivity1()
        {
            const string Test = "{tag}";
            var tag = new Tag().Keyword("TAG");

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.Ordinal)
                .RegisterTag(tag);

            ExpectNoMatchingTagsLeftException(null, 1, 3, "tag", Token.TokenType.Word, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseCaseSensitivity2()
        {
            const string Test = "{keyword identifier}";
            var tag = new Tag().Keyword("keyword").Identifier("IDENTIFIER", "identifier");

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, 
                StringComparer.Ordinal)
                .RegisterTag(tag);

            AssertTagLex(lexer.ReadNext(), 0, Test.Length, tag, "keyword|identifier");
        }

        [Test]
        public void TestCaseCaseSensitivity3()
        {
            const string Test = "{k w}";
            var tag1 = new Tag().Keyword("k").Identifier("W");
            var tag2 = new Tag().Keyword("K").Identifier("w");
            var tag3 = new Tag().Keyword("K").Identifier("W");
            var tag4 = new Tag().Keyword("k").Identifier("w");

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.Ordinal)
                .RegisterTag(tag1)
                .RegisterTag(tag2)
                .RegisterTag(tag3)
                .RegisterTag(tag4);

            AssertTagLex(lexer.ReadNext(), 0, Test.Length, tag4, "k|w");
        }

        [Test]
        public void TestCaseCaseSensitivity4()
        {
            const string Test = "{1 SHL 2}";
            var exprTag = new Tag().Expression();

            var lexer = new Lexer(new Tokenizer(Test), ExpressionFlowSymbols.Default, StringComparer.Ordinal)
                .RegisterTag(exprTag).RegisterOperator(new BitwiseShiftLeftOperator("shl", TypeConverter));

            ExpectUnexpectedOrInvalidExpressionTokenException(3, 3, "SHL", Token.TokenType.Word, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseDecimalPointSensitivity()
        {
            const string TwoNumbers = "{11[22}";

            var exprTag = new Tag().Expression();

            var lexer = new Lexer(new Tokenizer(new StringReader(TwoNumbers), '{', '}', '"', '"', '\\', '['),
                ExpressionFlowSymbols.Default, StringComparer.Ordinal).RegisterTag(exprTag);

            AssertTagLex(lexer.ReadNext(), 0, TwoNumbers.Length, exprTag, 11.22.ToString(CultureInfo.CurrentCulture));
        }
    }
}

