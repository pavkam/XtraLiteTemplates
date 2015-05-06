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

