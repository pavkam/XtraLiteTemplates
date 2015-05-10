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
    using XtraLiteTemplates.Parsing;

    [TestFixture]
    public class TokenizerTest : TestBase
    {
        private static void AssertToken(Token token, Token.TokenType type, Int32 charIndex, Int32 length, String value)
        {
            Assert.IsNotNull(token);
            Assert.AreEqual(charIndex, token.CharacterIndex);
            Assert.AreEqual(length, token.OriginalLength);
            Assert.AreEqual(type, token.Type);
            Assert.AreEqual(value, token.Value);
        }

        private static void AssertToken(Token token, Token.TokenType type, Int32 charIndex, String value)
        {
            Assert.IsNotNull(token);
            AssertToken(token, type, charIndex, value.Length, value);
        }

        private static void ExpectUnexpectedCharacterException(Int32 index, Char character, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Unexpected character '{0}' found at position {1}.", character, index), e.Message);
                if (e is ParseException)
                {
                    Assert.AreEqual(index, (e as ParseException).CharacterIndex);
                }
                return;
            }

            Assert.Fail();
        }

        private static void ExpectInvalidEscapeCharacterException(Int32 index, Char character, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Invalid escape character '{0}' at position {1}.", character, index), e.Message);
                if (e is ParseException)
                {
                    Assert.AreEqual(index, (e as ParseException).CharacterIndex);
                }
                return;
            }

            Assert.Fail();
        }

        private static void ExpectUnexpectedEndOfStreamException(Int32 index, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Unexpected end of stream detected at position {0}.", index), e.Message);
                if (e is ParseException)
                {
                    Assert.AreEqual(index, (e as ParseException).CharacterIndex);
                }
                return;
            }

            Assert.Fail();
        }

        [Test]
        public void TestCaseStandardConfiguration()
        {
            const String test = "anything";
            var tokenizer = new Tokenizer(test);

            Assert.AreEqual('{', tokenizer.TagStartCharacter);
            Assert.AreEqual('}', tokenizer.TagEndCharacter);
            Assert.AreEqual('"', tokenizer.StringStartCharacter);
            Assert.AreEqual('"', tokenizer.StringEndCharacter);
            Assert.AreEqual('\\', tokenizer.StringEscapeCharacter);
        }

        [Test]
        public void TestCaseCustomConfiguration()
        {
            const String test = "<{hello-r-n-}}>";
            var tokenizer = new Tokenizer(new StringReader(test), '<', '>', '{', '}', '-');

            Assert.AreEqual('<', tokenizer.TagStartCharacter);
            Assert.AreEqual('>', tokenizer.TagEndCharacter);
            Assert.AreEqual('{', tokenizer.StringStartCharacter);
            Assert.AreEqual('}', tokenizer.StringEndCharacter);
            Assert.AreEqual('-', tokenizer.StringEscapeCharacter);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 0, "<");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.String, 1, 13, "hello\r\n}");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 14, ">");

            Assert.IsNull(tokenizer.ReadNext());
        }


        [Test]
        public void TestCaseEmptyOrNullText()
        {
            var tokenizer = new Tokenizer((String)null);
            Assert.IsNull(tokenizer.ReadNext());

            tokenizer = new Tokenizer(String.Empty);
            Assert.IsNull(tokenizer.ReadNext());

            tokenizer = new Tokenizer(new StringReader(String.Empty));
            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseOneUnparsedCharacter()
        {
            const String test = "T";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.Unparsed, 0, 1, "T");
            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseEscapedDirectiveCharacters()
        {
            const String test = "{{ {{ }} }}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.Unparsed, 0, 2, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Unparsed, 2, 1, " ");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Unparsed, 3, 2, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Unparsed, 5, 6, " }} }}");
            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseSingleDirectiveWithOneIdentifier()
        {
            const String test = "{identifier}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 0, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Word, 1, "identifier");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 11, "}");

            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseEscapedDirectiveCharacterAndSingleDirectiveWithOneIdentifier()
        {
            const String test = "{{{identifier}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.Unparsed, 0, 2, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 2, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Word, 3, "identifier");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 13, "}");

            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseDirectivesWithNumbers()
        {
            const String test = "{0}{123}{1.11}{.22}{A.0.99.2B..C}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 0, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 1, "0");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 2, "}");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 3, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 4, "123");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 7, "}");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 8, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 9, "1.11");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 13, "}");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 14, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 15, ".22");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 18, "}");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 19, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Word, 20, "A");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 21, ".0");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 23, ".99");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 26, ".2");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Word, 28, "B");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 29, ".");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 30, ".");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Word, 31, "C");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 32, "}");
            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseInvalidNumberFormat_1()
        {
            const String test = "{100.}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 0, "{");
            ExpectUnexpectedCharacterException(5, '}', () => tokenizer.ReadNext());
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 5, "}");
        }

        [Test]
        public void TestCaseInvalidNumberFormat_2()
        {
            const String test = "{100.a}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 0, "{");
            ExpectUnexpectedCharacterException(5, 'a', () => tokenizer.ReadNext());
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 6, "}");
        }

        [Test]
        public void TestCaseDirectiveWithSymbolsAndWhiteSpaces()
        {
            const String test = "{..  + /<200 ABC   }";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 0, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 1, ".");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 2, ".");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Whitespace, 3, "  ");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 5, "+");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Whitespace, 6, " ");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 7, "/");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 8, "<");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 9, "200");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Whitespace, 12, " ");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Word, 13, "ABC");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Whitespace, 16, "   ");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 19, "}");

            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseDirectiveStringAndEscapeCharacters()
        {
            const String test = "{\"\",\" \", \"\\\\\\\"\\a\\b\\f\\n\\r\\t\\v\\'\\?\"}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.String, 1, 2, "");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 3, ",");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.String, 4, 3, " ");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 7, ",");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Whitespace, 8, " ");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.String, 9, 24, "\\\"\a\b\f\n\r\t\v'?");
            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseInvalidEscapeCharacter()
        {
            const String test = "{\"\\i\"}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 0, "{");
            ExpectInvalidEscapeCharacterException(3, 'i', () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedStartDirectiveCharacter()
        {
            const String test = "{ {";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 0, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Whitespace, 1, " ");
            ExpectUnexpectedCharacterException(2, '{', () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseEmptyDirective()
        {
            const String test = "{}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.StartTag, 0, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.EndTag, 1, "}");
            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_1()
        {
            const String test = "{";
            var tokenizer = new Tokenizer(test);

            ExpectUnexpectedEndOfStreamException(1, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_2()
        {
            const String test = "{ ";
            var tokenizer = new Tokenizer(test);

            ExpectUnexpectedEndOfStreamException(2, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_3()
        {
            const String test = "{0";
            var tokenizer = new Tokenizer(test);

            ExpectUnexpectedEndOfStreamException(2, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_4()
        {
            const String test = "{0.";
            var tokenizer = new Tokenizer(test);

            ExpectUnexpectedEndOfStreamException(3, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_5()
        {
            const String test = "{0.0";
            var tokenizer = new Tokenizer(test);

            ExpectUnexpectedEndOfStreamException(4, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_6()
        {
            const String test = "{A";
            var tokenizer = new Tokenizer(test);

            ExpectUnexpectedEndOfStreamException(2, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_7()
        {
            const String test = "{\"-\"";
            var tokenizer = new Tokenizer(test);

            ExpectUnexpectedEndOfStreamException(4, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_8()
        {
            const String test = "{\"";
            var tokenizer = new Tokenizer(test);

            ExpectUnexpectedEndOfStreamException(2, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_9()
        {
            const String test = "{\"\\";
            var tokenizer = new Tokenizer(test);

            ExpectUnexpectedEndOfStreamException(3, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseConstructionExceptions()
        {
            ExpectArgumentEmptyException("reader", () => new Tokenizer((TextReader)null));
        }

        [Test]
        public void TestCaseConstructionExceptionsExclusionRules()
        {
            var dummyReader = new StringReader("valid");
            ExpectArgumentsEqualException("tagStartCharacter", "tagEndCharacter", 
                () => new Tokenizer(dummyReader, '{', '{', '"', '"', '\\'));
            ExpectArgumentsEqualException("stringStartCharacter", "tagStartCharacter", 
                () => new Tokenizer(dummyReader, '{', '}', '{', '"', '\\'));
            ExpectArgumentsEqualException("stringStartCharacter", "tagEndCharacter", 
                () => new Tokenizer(dummyReader, '{', '}', '}', '"', '\\'));
            ExpectArgumentsEqualException("stringEndCharacter", "tagStartCharacter", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '{', '\\'));
            ExpectArgumentsEqualException("stringEndCharacter", "tagEndCharacter", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '}', '\\'));
            ExpectArgumentsEqualException("stringEscapeCharacter", "stringEndCharacter", 
                () => new Tokenizer(dummyReader, '{', '}', '<', '>', '>'));
        }

        [Test]
        public void TestCaseConstructionExceptionsCharacterSets()
        {
            var dummyReader = new StringReader("valid");

            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, 'a', '}', '"', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, ' ', '}', '"', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '0', '}', '"', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '_', '}', '"', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '.', '}', '"', '"', '\\'));

            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', 'a', '"', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', ' ', '"', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '0', '"', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '_', '"', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '.', '"', '"', '\\'));

            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', 'a', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', ' ', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '0', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '_', '"', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '.', '"', '\\'));

            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', 'a', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', ' ', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '0', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '_', '\\'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '.', '\\'));

            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '"', 'a'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '"', ' '));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '"', '0'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '"', '_'));
            ExpectArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '"', '.'));
        }
    }
}

