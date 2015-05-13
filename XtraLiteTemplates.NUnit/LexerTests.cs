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
    using System.IO;
    using System.Linq;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Expressions.Operators.Standard;
    using XtraLiteTemplates.Parsing;

    [TestFixture]
    public class LexerTests : TestBase
    {


        private static void AssertUnparsedLex(Lex lex, Int32 firstCharacterIndex, Int32 originalLength, String unparsedText)
        {
            Assert.IsInstanceOf<UnparsedLex>(lex);
            var unparsedLex = lex as UnparsedLex;

            Assert.AreEqual(firstCharacterIndex, unparsedLex.FirstCharacterIndex);
            Assert.AreEqual(originalLength, unparsedLex.OriginalLength);
            Assert.AreEqual(unparsedText, unparsedLex.UnparsedText);
        }

        private static void AssertUnparsedLex(Lex lex, Int32 firstCharacterIndex, String unparsedText)
        {
            AssertUnparsedLex(lex, firstCharacterIndex, unparsedText.Length, unparsedText);
        }

        private static void AssertTagLex(Lex lex, Int32 firstCharacterIndex, Int32 originalLength, Tag tag, String asText)
        {
            Assert.IsInstanceOf<TagLex>(lex);
            var tagLex = lex as TagLex;

            Assert.AreEqual(firstCharacterIndex, tagLex.FirstCharacterIndex);
            Assert.AreEqual(originalLength, tagLex.OriginalLength);
            Assert.AreEqual(tag, tagLex.Tag);

            var allText = String.Join("|", tagLex.Components.Select(s => s.ToString()));
            Assert.AreEqual(asText, allText);
        }

        [Test]
        public void TestCaseConstruction1()
        {
            var tokenizer = new Tokenizer("irrelevant");

            ExpectArgumentNullException("tokenizer", () => new Lexer((ITokenizer)null, StringComparer.OrdinalIgnoreCase));
            ExpectArgumentNullException("comparer", () => new Lexer(tokenizer, null));

            var lexer = new Lexer(tokenizer, StringComparer.OrdinalIgnoreCase);

            Assert.AreSame(tokenizer, lexer.Tokenizer);
            Assert.AreSame(StringComparer.OrdinalIgnoreCase, lexer.Comparer);
        }

        [Test]
        public void TestCaseConstruction2()
        {
            var reader = new StringReader("irrelevant");

            ExpectArgumentNullException("reader", () => new Lexer((StringReader)null, StringComparer.OrdinalIgnoreCase));
            ExpectArgumentNullException("comparer", () => new Lexer(reader, null));

            var lexer = new Lexer(reader, StringComparer.OrdinalIgnoreCase);

            Assert.AreSame(StringComparer.OrdinalIgnoreCase, lexer.Comparer);

            AssertUnparsedLex(lexer.ReadNext(), 0, "irrelevant");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseConstruction3()
        {
            var text = "irrelevant";

            ExpectArgumentNullException("text", () => new Lexer((String)null, StringComparer.OrdinalIgnoreCase));
            ExpectArgumentNullException("comparer", () => new Lexer(text, null));

            var lexer = new Lexer(text, StringComparer.OrdinalIgnoreCase);

            Assert.AreSame(StringComparer.OrdinalIgnoreCase, lexer.Comparer);

            AssertUnparsedLex(lexer.ReadNext(), 0, "irrelevant");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseTagRegistration()
        {
            var tokenizer = new Tokenizer("irrelevant");
            var lexer = new Lexer(tokenizer, StringComparer.OrdinalIgnoreCase);
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
        public void TestCaseOperatorRegistration()
        {
            var tokenizer = new Tokenizer("irrelevant");
            var lexer = new Lexer(tokenizer, StringComparer.OrdinalIgnoreCase);

            ExpectArgumentNullException("operator", () => lexer.RegisterOperator(null));

            Assert.AreEqual(lexer, lexer.RegisterOperator(NeutralOperator.Standard));
            Assert.AreEqual(lexer, lexer.RegisterOperator(SumOperator.Standard));
            Assert.AreEqual(lexer, lexer.RegisterOperator(new SubscriptOperator("<", ">")));

            ExpectOperatorAlreadyRegisteredException(NeutralOperator.Standard, () => lexer.RegisterOperator(NeutralOperator.Standard));
            ExpectOperatorAlreadyRegisteredException(SumOperator.Standard, () => lexer.RegisterOperator(SumOperator.Standard));

            var _ss1 = new SubscriptOperator("+", ")");
            ExpectOperatorAlreadyRegisteredException(_ss1, () => lexer.RegisterOperator(_ss1));

            var _ss2 = new SubscriptOperator("(", "+");
            ExpectOperatorAlreadyRegisteredException(_ss2, () => lexer.RegisterOperator(_ss2));

            var _ss3 = new SubscriptOperator("<", ")");
            ExpectOperatorAlreadyRegisteredException(_ss3, () => lexer.RegisterOperator(_ss3));

            var _ss4 = new SubscriptOperator("(", ">");
            ExpectOperatorAlreadyRegisteredException(_ss4, () => lexer.RegisterOperator(_ss4));
        }

        [Test]
        public void TestCaseEmptyInputString()
        {
            const String test = "";
            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase);

            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseUparsedOnlyInputString()
        {
            const String test = "unparsed text is here baby {{ and some other {{{{ tokens also.";
            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase);

            AssertUnparsedLex(lexer.ReadNext(), 0, test.Length, test.Replace("{{", "{"));
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseOneKeywordTagOnly()
        {
            const String test = "{TAG}";
            var tag = new Tag().Keyword("TAG");
            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase).RegisterTag(tag);

            AssertTagLex(lexer.ReadNext(), 0, test.Length, tag, "TAG");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseTwoKeywordTagsAndUnparsed()
        {
            const String test = "{K1 K2}unparsed{K1 K3}";

            var tag1 = new Tag().Keyword("K1").Keyword("K2");
            var tag2 = new Tag().Keyword("K1").Keyword("K3");

            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase).
                RegisterTag(tag1).
                RegisterTag(tag2);

            AssertTagLex(lexer.ReadNext(), 0, 7, tag1, "K1|K2");
            AssertUnparsedLex(lexer.ReadNext(), 7, "unparsed");
            AssertTagLex(lexer.ReadNext(), 15, 7, tag2, "K1|K3");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseEmptyTag()
        {
            const String test = "{}";

            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase);
            ExpectUnexpectedTokenException(1, "}", Token.TokenType.EndTag, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseUnidentifiedTag1()
        {
            const String test = "{BAG}";
            var tag = new Tag().Keyword("TAG");
            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase).RegisterTag(tag);

            ExpectUnexpectedTokenException(1, "BAG", Token.TokenType.Word, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseUnidentifiedTag2()
        {
            const String test = "{TAG AHA}";
            var tag = new Tag().Keyword("TAG");
            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase).RegisterTag(tag);

            ExpectUnexpectedTokenException(5, "AHA", Token.TokenType.Word, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseJustExpressionTag()
        {
            const String test = "{1.33}";
            var tag = new Tag().Expression();
            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase).RegisterTag(tag);

            AssertTagLex(lexer.ReadNext(), 0, 6, tag, "1.33");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseKeywordAndExpressionTag()
        {
            const String test = "{IF 10 = 10}";

            var ifTag = new Tag().Keyword("IF").Expression();
            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase)
                .RegisterTag(ifTag)
                .RegisterOperator(EqualsOperator.Pascal);

            AssertTagLex(lexer.ReadNext(), 0, 12, ifTag, "IF|True");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseAmbiguousTags()
        {
            const String test = "{IF SET Alpha}";

            var ifTag1 = new Tag().Keyword("IF").Expression();
            var ifTag2 = new Tag().Keyword("IF").Keyword("SET").Expression();

            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase)
                .RegisterTag(ifTag1)
                .RegisterTag(ifTag2);

            AssertTagLex(lexer.ReadNext(), 0, 14, ifTag2, "IF|SET|@Alpha");
            Assert.IsNull(lexer.ReadNext());
        }

        [Test]
        public void TestCaseBadTokenOrder()
        {
            var tag = new Tag().Expression();

            var lexer = new Lexer(new Tokenizer("{10ALPHA}"), StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(3, "ALPHA", Token.TokenType.Word, () => lexer.ReadNext());

            lexer = new Lexer(new Tokenizer("{10\"ALPHA\"}"), StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(3, "ALPHA", Token.TokenType.String, () => lexer.ReadNext());

            lexer = new Lexer(new Tokenizer("{AB\"ALPHA\"}"), StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(3, "ALPHA", Token.TokenType.String, () => lexer.ReadNext());

            lexer = new Lexer(new Tokenizer("{\"ALPHA\"AB}"), StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(8, "AB", Token.TokenType.Word, () => lexer.ReadNext());

            lexer = new Lexer(new Tokenizer("{\"ALPHA\"10}"), StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(8, "10", Token.TokenType.Number, () => lexer.ReadNext());

            lexer = new Lexer(new Tokenizer("{\"ALPHA\"\"BETA\"}"), StringComparer.OrdinalIgnoreCase).RegisterTag(tag);
            ExpectUnexpectedTokenException(8, "BETA", Token.TokenType.String, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseIncompleteTag()
        {
            const String test = "{A any B 2 C }";

            var tag = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C").Keyword("D");
            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase).RegisterTag(tag);

            ExpectUnexpectedTokenException(13, "}", Token.TokenType.EndTag, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseIncompleteAndCompleteSelectionTags()
        {
            const String test = "{A any B 2 C }";

            var tag1 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C").Keyword("D");
            var tag2 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C");

            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag1).RegisterTag(tag2);

            AssertTagLex(lexer.ReadNext(), 0, 14, tag2, "A|any|B|2|C");
        }

        [Test]
        public void TestCaseAmbiguousTagSelection1()
        {
            const String test = "{A any B 2 C }";

            var tag1 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C");
            var tag2 = new Tag().Keyword("A").Expression().Keyword("B").Expression().Keyword("C");

            var lexer12 = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag1).RegisterTag(tag2);
            var lexer21 = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer12.ReadNext(), 0, 14, tag1, "A|any|B|2|C");
            AssertTagLex(lexer21.ReadNext(), 0, 14, tag1, "A|any|B|2|C");
        }

        [Test]
        public void TestCaseAmbiguousTagSelection2()
        {
            const String test = "{A any B 2 C }";

            var tag1 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C");
            var tag2 = new Tag().Keyword("A").Identifier("any").Keyword("B").Expression().Keyword("C");

            var lexer12 = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag1).RegisterTag(tag2);
            var lexer21 = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer12.ReadNext(), 0, 14, tag1, "A|any|B|2|C");
            AssertTagLex(lexer21.ReadNext(), 0, 14, tag2, "A|any|B|2|C");
        }

        [Test]
        public void TestCaseAmbiguousTagSelection3()
        {
            const String test = "{A any B 2 C }";

            var tag1 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C");
            var tag2 = new Tag().Keyword("A").Identifier().Keyword("B").Expression().Keyword("C");

            var lexer12 = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag1).RegisterTag(tag2);
            var lexer21 = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase)
                .RegisterTag(tag2).RegisterTag(tag1);

            AssertTagLex(lexer12.ReadNext(), 0, 14, tag1, "A|any|B|2|C");
            AssertTagLex(lexer21.ReadNext(), 0, 14, tag2, "A|any|B|2|C");
        }

        [Test]
        public void TestCaseBrokenExpression1()
        {
            const String test = "{1 +}";

            var exprTag = new Tag().Expression();

            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase)
                .RegisterTag(exprTag).RegisterOperator(SumOperator.Standard);

            ExpectUnbalancedExpressionCannotBeFinalizedException(4, "}", Token.TokenType.EndTag, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseBrokenExpression2()
        {
            const String test = "{1 + END}";

            var exprTag = new Tag().Expression().Keyword("END");

            var lexer = new Lexer(new Tokenizer(test), StringComparer.OrdinalIgnoreCase)
                .RegisterTag(exprTag).RegisterOperator(SumOperator.Standard);

            ExpectUnbalancedExpressionCannotBeFinalizedException(5, "END", Token.TokenType.Word, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseCaseSensitivity1()
        {
            const String test = "{tag}";
            var tag = new Tag().Keyword("TAG");

            var lexer = new Lexer(new Tokenizer(test), StringComparer.Ordinal)
                .RegisterTag(tag);

            ExpectUnexpectedTokenException(1, "tag", Token.TokenType.Word, () => lexer.ReadNext());
        }

        [Test]
        public void TestCaseCaseSensitivity2()
        {
            const String test = "{keyword identifier}";
            var tag = new Tag().Keyword("keyword").Identifier("IDENTIFIER", "identifier");

            var lexer = new Lexer(new Tokenizer(test), StringComparer.Ordinal)
                .RegisterTag(tag);

            AssertTagLex(lexer.ReadNext(), 0, test.Length, tag, "keyword|identifier");
        }

        [Test]
        public void TestCaseCaseSensitivity3()
        {
            const String test = "{k w}";
            var tag1 = new Tag().Keyword("k").Identifier("W");
            var tag2 = new Tag().Keyword("K").Identifier("w");
            var tag3 = new Tag().Keyword("K").Identifier("W");
            var tag4 = new Tag().Keyword("k").Identifier("w");

            var lexer = new Lexer(new Tokenizer(test), StringComparer.Ordinal)
                .RegisterTag(tag1)
                .RegisterTag(tag2)
                .RegisterTag(tag3)
                .RegisterTag(tag4);

            AssertTagLex(lexer.ReadNext(), 0, test.Length, tag4, "k|w");
        }

        [Test]
        public void TestCaseCaseSensitivity4()
        {
            const String test = "{1 SHL 2}";
            var exprTag = new Tag().Expression();

            var lexer = new Lexer(new Tokenizer(test), StringComparer.Ordinal)
                .RegisterTag(exprTag).RegisterOperator(ShiftLeftOperator.Pascal);

            ExpectUnexpectedOrInvalidExpressionTokenException(3, "SHL", Token.TokenType.Word, () => lexer.ReadNext());
        }
    }
}

