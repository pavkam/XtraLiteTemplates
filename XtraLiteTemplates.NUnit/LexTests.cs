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
    using XtraLiteTemplates.Parsing;

    [TestFixture]
    public class LexTests : TestBase
    {
        [Test]
        public void TestCaseUnparsedLexContruction1()
        {
            var lex = new UnparsedLex("some_value", 100, 999);

            Assert.AreEqual(100, lex.FirstCharacterIndex);
            Assert.AreEqual(999, lex.OriginalLength);
            Assert.AreEqual("some_value", lex.UnparsedText);
        }

        [Test]
        public void TestCaseUnparsedLexContruction2()
        {
            ExpectArgumentLessThanException("firstCharacterIndex", 0, () => new UnparsedLex("100", -1, 1));
            ExpectArgumentLessThanOrEqualException("originalLength", 0, () => new UnparsedLex("100", 1, 0));

            ExpectArgumentEmptyException("unparsedText", () => new UnparsedLex(null, 0, 1));
            ExpectArgumentEmptyException("unparsedText", () => new UnparsedLex(String.Empty, 0, 1));
        }

        [Test]
        public void TestCaseTagLexContruction1()
        {
            var tag = new Tag().Keyword("Hello").Expression();
            var components = new Object[] { "Hello", "Some" };

            var lex = new TagLex(tag, components, 100, 999);

            Assert.AreEqual(100, lex.FirstCharacterIndex);
            Assert.AreEqual(999, lex.OriginalLength);
            Assert.AreSame(tag, lex.Tag);
            Assert.AreSame(components, lex.Components);
        }

        [Test]
        public void TestCaseTagLexContruction2()
        {
            var tag = new Tag().Keyword("Hello").Expression();
            var components = new Object[] { "Hello", "Some" };

            ExpectArgumentLessThanException("firstCharacterIndex", 0, () => new TagLex(tag, components, -1, 1));
            ExpectArgumentLessThanOrEqualException("originalLength", 0, () => new TagLex(tag, components, 1, 0));

            ExpectArgumentNullException("tag", () => new TagLex(null, components, 0, 1));
            ExpectArgumentNullException("components", () => new TagLex(tag, null, 0, 1));
            ExpectArgumentEmptyException("components", () => new TagLex(tag, new Object[0], 0, 1));
        }
    }
}

