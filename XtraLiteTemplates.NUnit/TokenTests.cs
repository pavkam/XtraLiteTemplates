using NUnit.Framework;
using System;

namespace XtraLiteTemplates.NUnit
{
    [TestFixture]
    public class TokenTests : TestBase
    {
        [Test]
        public void TestCaseContruction_1()
        {
            var token = new Token(Token.TokenType.Unparsed, "some_value", 100, 999);

            Assert.AreEqual(100, token.CharacterIndex);
            Assert.AreEqual(999, token.OriginalLength);
            Assert.AreEqual(Token.TokenType.Unparsed, token.Type);
            Assert.AreEqual("some_value", token.Value);
        }

        [Test]
        public void TestCaseContruction_2()
        {
            var token = new Token(Token.TokenType.String, null, 0, 1);

            Assert.AreEqual(0, token.CharacterIndex);
            Assert.AreEqual(1, token.OriginalLength);
            Assert.AreEqual(Token.TokenType.String, token.Type);
            Assert.AreEqual(String.Empty, token.Value);
        }

        [Test]
        public void TestCaseConstructionExceptions()
        {
            AssertArgumentLessThanException("characterIndex", 0, () => new Token(Token.TokenType.Number, "100", -1, 1));
            AssertArgumentLessThanOrEqualException("originalLength", 0, () => new Token(Token.TokenType.Number, "100", 1, 0));

            AssertArgumentEmptyException("value", () => new Token(Token.TokenType.Identifier, null, 0, 1));
            AssertArgumentEmptyException("value", () => new Token(Token.TokenType.Identifier, String.Empty, 0, 1));
        }
    }
}

