using NUnit.Framework;
using System;
using System.IO;

namespace XtraLiteTemplates.NUnit
{
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

        private static void AssertUnexpectedCharacter(Int32 index, Char character, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Unexpected character '{0}' found at position {1}.", character, index), e.Message);
                if (e is ParseException)
                {
                    Assert.AreEqual(index, (e as ParseException).CharacterIndex);
                }
            }
        }

        private static void AssertInvalidEscapeCharacter(Int32 index, Char character, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Invalid escape character '{0}' at position {1}.", character, index), e.Message);
                if (e is ParseException)
                {
                    Assert.AreEqual(index, (e as ParseException).CharacterIndex);
                }
            }
        }

        private static void AssertUnexpectedEndOfStream(Int32 index, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Unexpected end of stream detected at position {0}.", index), e.Message);
                if (e is ParseException)
                {
                    Assert.AreEqual(index, (e as ParseException).CharacterIndex);
                }
            }
        }

        [Test]
        public void TestCaseStandardConfiguration()
        {
            const String test = "anything";
            var tokenizer = new Tokenizer(test);

            Assert.AreEqual('{', tokenizer.DirectiveStartCharacter);
            Assert.AreEqual('}', tokenizer.DirectiveEndCharacter);
            Assert.AreEqual('"', tokenizer.StringStartCharacter);
            Assert.AreEqual('"', tokenizer.StringEndCharacter);
            Assert.AreEqual('\\', tokenizer.StringEscapeCharacter);
        }

        [Test]
        public void TestCaseCustomConfiguration()
        {
            const String test = "<{hello-r-n-}}>";
            var tokenizer = new Tokenizer(new StringReader(test), '<', '>', '{', '}', '-');

            Assert.AreEqual('<', tokenizer.DirectiveStartCharacter);
            Assert.AreEqual('>', tokenizer.DirectiveEndCharacter);
            Assert.AreEqual('{', tokenizer.StringStartCharacter);
            Assert.AreEqual('}', tokenizer.StringEndCharacter);
            Assert.AreEqual('-', tokenizer.StringEscapeCharacter);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.String, 1, 13, "hello\r\n}");
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

            AssertToken(tokenizer.ReadNext(), Token.TokenType.Unparsed, 0, test.Length, "{ { }} }}");
            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseSingleDirectiveWithOneIdentifier()
        {
            const String test = "{identifier}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.Identifier, 1, "identifier");
            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseEscapedDirectiveCharacterAndSingleDirectiveWithOneIdentifier()
        {
            const String test = "{{{identifier}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.Unparsed, 0, 2, "{");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Identifier, 3, "identifier");
            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseDirectivesWithNumbers()
        {
            const String test = "{0}{123}{1.11}{.22}{A.0.99.2B..C}";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 1, "0");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 4, "123");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 9, "1.11");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 15, ".22");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Identifier, 20, "A");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 21, ".0");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 23, ".99");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 26, ".2");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Identifier, 28, "B");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 29, ".");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 30, ".");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Identifier, 31, "C");
            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseInvalidNumberFormat_1()
        {
            const String test = "{100.}";
            var tokenizer = new Tokenizer(test);

            AssertUnexpectedCharacter(5, '}', () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseInvalidNumberFormat_2()
        {
            String test = "{100.a}";
            var tokenizer = new Tokenizer(test);

            AssertUnexpectedCharacter(5, 'a', () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseDirectiveWithSymbolsAndWhiteSpaces()
        {
            String test = "{..  + /<200 ABC   }";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 1, ".");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 2, ".");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Whitespace, 3, "  ");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 5, "+");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Whitespace, 6, " ");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 7, "/");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Symbol, 8, "<");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Number, 9, "200");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Whitespace, 12, " ");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Identifier, 13, "ABC");
            AssertToken(tokenizer.ReadNext(), Token.TokenType.Whitespace, 16, "   ");
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

            AssertInvalidEscapeCharacter(3, 'i', () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedStartDirectiveCharacter()
        {
            const String test = "{ {";
            var tokenizer = new Tokenizer(test);

            AssertToken(tokenizer.ReadNext(), Token.TokenType.Whitespace, 1, " ");
            AssertUnexpectedCharacter(2, '{', () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseEmptyDirective()
        {
            const String test = "{}";
            var tokenizer = new Tokenizer(test);

            Assert.IsNull(tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_1()
        {
            const String test = "{";
            var tokenizer = new Tokenizer(test);

            AssertUnexpectedEndOfStream(1, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_2()
        {
            const String test = "{ ";
            var tokenizer = new Tokenizer(test);

            AssertUnexpectedEndOfStream(2, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_3()
        {
            const String test = "{0";
            var tokenizer = new Tokenizer(test);

            AssertUnexpectedEndOfStream(2, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_4()
        {
            const String test = "{0.";
            var tokenizer = new Tokenizer(test);

            AssertUnexpectedEndOfStream(3, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_5()
        {
            const String test = "{0.0";
            var tokenizer = new Tokenizer(test);

            AssertUnexpectedEndOfStream(4, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_6()
        {
            const String test = "{A";
            var tokenizer = new Tokenizer(test);

            AssertUnexpectedEndOfStream(2, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_7()
        {
            const String test = "{\"-\"";
            var tokenizer = new Tokenizer(test);

            AssertUnexpectedEndOfStream(4, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_8()
        {
            const String test = "{\"";
            var tokenizer = new Tokenizer(test);

            AssertUnexpectedEndOfStream(2, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseUnexpectedEndOfStream_9()
        {
            const String test = "{\"\\";
            var tokenizer = new Tokenizer(test);

            AssertUnexpectedEndOfStream(3, () => tokenizer.ReadNext());
        }

        [Test]
        public void TestCaseConstructionExceptions()
        {
            AssertArgumentEmptyException("reader", () => new Tokenizer((TextReader)null));
        }

        [Test]
        public void TestCaseConstructionExceptionsExclusionRules()
        {
            var dummyReader = new StringReader("valid");
            AssertArgumentsEqualException("directiveStartCharacter", "directiveEndCharacter", 
                () => new Tokenizer(dummyReader, '{', '{', '"', '"', '\\'));
            AssertArgumentsEqualException("stringStartCharacter", "directiveStartCharacter", 
                () => new Tokenizer(dummyReader, '{', '}', '{', '"', '\\'));
            AssertArgumentsEqualException("stringStartCharacter", "directiveEndCharacter", 
                () => new Tokenizer(dummyReader, '{', '}', '}', '"', '\\'));
            AssertArgumentsEqualException("stringEndCharacter", "directiveStartCharacter", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '{', '\\'));
            AssertArgumentsEqualException("stringEndCharacter", "directiveEndCharacter", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '}', '\\'));
            AssertArgumentsEqualException("stringEscapeCharacter", "stringEndCharacter", 
                () => new Tokenizer(dummyReader, '{', '}', '<', '>', '>'));
        }

        [Test]
        public void TestCaseConstructionExceptionsCharacterSets()
        {
            var dummyReader = new StringReader("valid");

            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, 'a', '}', '"', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, ' ', '}', '"', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '0', '}', '"', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '_', '}', '"', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '.', '}', '"', '"', '\\'));

            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', 'a', '"', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', ' ', '"', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '0', '"', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '_', '"', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '.', '"', '"', '\\'));

            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', 'a', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', ' ', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '0', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '_', '"', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '.', '"', '\\'));

            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', 'a', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', ' ', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '0', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '_', '\\'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '.', '\\'));

            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '"', 'a'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '"', ' '));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '"', '0'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '"', '_'));
            AssertArgumentConditionNotTrueException("allowed set of characters", 
                () => new Tokenizer(dummyReader, '{', '}', '"', '"', '.'));
        }
    }
}

