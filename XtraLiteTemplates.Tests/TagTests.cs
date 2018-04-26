//
//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2018, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.Tests
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using global::NUnit.Framework;
    using XtraLiteTemplates.Parsing;

    [TestFixture]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class TagTests : TestBase
    {
        [Test]
        public void TestCaseEmptyTag()
        {
            var tag = new Tag();

            Assert.AreEqual(0, tag.ComponentCount);
            Assert.AreEqual(string.Empty, tag.ToString());
        }

        [Test]
        public void TestCaseKeywordTag()
        {
            var tag = new Tag();

            ExpectArgumentNotIdentifierException("keyword", () => tag.Keyword("0number"));
            ExpectArgumentNotIdentifierException("keyword", () => tag.Keyword("-sign"));
            ExpectArgumentNotIdentifierException("keyword", () => tag.Keyword(" white space"));
            ExpectArgumentNotIdentifierException("keyword", () => tag.Keyword("whitespace inside"));
            ExpectArgumentNotIdentifierException("keyword", () => tag.Keyword("symbol|inside"));

            Assert.AreSame(tag, tag.Keyword("alpha"));
            Assert.AreSame(tag, tag.Keyword("_underscore"));
            Assert.AreSame(tag, tag.Keyword("with007_numbers"));
            Assert.AreEqual(3, tag.ComponentCount);

            Assert.AreEqual("alpha _underscore with007_numbers", tag.ToString());
        }

        [Test]
        public void TestCaseAnyIdentifierTag()
        {
            var tag = new Tag();

            Assert.AreSame(tag, tag.Identifier());
            Assert.AreSame(tag, tag.Expression());
            ExpectTagAnyIdentifierCannotFollowExpressionException(() => tag.Identifier());

            Assert.AreEqual("? $", tag.ToString());
        }

        [Test]
        public void TestCaseExpressionTag()
        {
            var tag = new Tag();

            Assert.AreSame(tag, tag.Expression());
            ExpectTagExpressionCannotFollowExpressionException(() => tag.Expression());

            Assert.AreEqual("$", tag.ToString());
        }

        [Test]
        public void TestCaseIdentifierTag()
        {
            var tag = new Tag();

            ExpectArgumentEmptyException("candidates", () => tag.Identifier(new string[0]));

            ExpectArgumentNotIdentifierException("candidates", () => tag.Identifier("__good0ne", "0number"));
            ExpectArgumentNotIdentifierException("candidates", () => tag.Identifier("__good0ne", "-sign"));
            ExpectArgumentNotIdentifierException("candidates", () => tag.Identifier("__good0ne", " white space"));
            ExpectArgumentNotIdentifierException("candidates", () => tag.Identifier("__good0ne", "whitespace inside"));
            ExpectArgumentNotIdentifierException("candidates", () => tag.Identifier("__good0ne", "symbol|inside"));

            Assert.AreSame(tag, tag.Identifier("candidate1", "candidate2", "candidate3"));
            Assert.AreEqual("(candidate1 candidate2 candidate3)", tag.ToString());
        }

        [Test]
        [SuppressMessage("ReSharper", "StringLiteralTypo")]
        public void TestCaseComplexTag()
        {
            var tag = new Tag();

            Assert.AreSame(tag, tag.Keyword("if"));
            Assert.AreSame(tag, tag.Expression());
            Assert.AreSame(tag, tag.Identifier("then", "otherwise", "something", "else"));
            Assert.AreSame(tag, tag.Expression());
            Assert.AreSame(tag, tag.Keyword("kewl"));
            Assert.AreSame(tag, tag.Identifier());
            Assert.AreEqual(6, tag.ComponentCount);

            Assert.AreEqual("if $ (then otherwise something else) $ kewl ?", tag.ToString());
        }

        [Test]
        public void TestCaseTryParseFailures()
        {
            Assert.IsFalse(Tag.TryParse(null, out _));
            Assert.IsFalse(Tag.TryParse(string.Empty, out _));
            Assert.IsFalse(Tag.TryParse(" ", out _));
            Assert.IsFalse(Tag.TryParse("{", out _));
            Assert.IsFalse(Tag.TryParse("9", out _));
            Assert.IsFalse(Tag.TryParse("KEYWORD+FAILURE", out _));
            Assert.IsFalse(Tag.TryParse("(LIST OF ITEMS", out _));
            Assert.IsFalse(Tag.TryParse("LIST OF ITEMS)", out _));
            Assert.IsFalse(Tag.TryParse("??", out _));
            Assert.IsFalse(Tag.TryParse("(?)", out _));
            Assert.IsFalse(Tag.TryParse("($)", out _));
            Assert.IsFalse(Tag.TryParse("EMPTY () GROUP", out _));
            Assert.IsFalse(Tag.TryParse("(ITEM () ITEM)", out _));
            Assert.IsFalse(Tag.TryParse("$$", out _));
            Assert.IsFalse(Tag.TryParse("?$", out _));
            Assert.IsFalse(Tag.TryParse("IF $ THEN $ ?", out _));
            Assert.IsFalse(Tag.TryParse("$ $", out _));
        }

        [Test]
        public void TestCaseTryParse()
        {

            Assert.IsTrue(Tag.TryParse("$", out Tag tag) && tag.ToString() == "$");
            Assert.IsTrue(Tag.TryParse("$  ", out tag) && tag.ToString() == "$");
            Assert.IsTrue(Tag.TryParse("  $", out tag) && tag.ToString() == "$");
            Assert.IsTrue(Tag.TryParse("TERM  $      TERM", out tag) && tag.ToString() == "TERM $ TERM");
            Assert.IsTrue(Tag.TryParse(" ( T1 T2  T3   )(V1  )$ K1 ? K2", out tag) && tag.ToString() == "(T1 T2 T3) (V1) $ K1 ? K2");
            Assert.IsTrue(Tag.TryParse("(item1 item2 item1 item3)", out tag) && tag.ToString() == "(item1 item2 item3)");
        }

        [Test]
        public void TestCaseParse()
        {
            ExpectInvalidTagMarkupException("IF $ $", () => Tag.Parse("IF $ $"));
            ExpectInvalidTagMarkupException(string.Empty, () => Tag.Parse(string.Empty));

            var tag = Tag.Parse("HELLO ? WORLD $ (A1 A2 A3)");
            Assert.AreEqual("HELLO ? WORLD $ (A1 A2 A3)", tag.ToString());
        }

        [Test]
        public void TestCaseEquality1()
        {
            var tag1 = Tag.Parse("(A1 A2)");
            var tag2 = Tag.Parse("(A2 A1)");

            Assert.AreEqual(tag1, tag2);
            Assert.AreEqual(tag1.GetHashCode(), tag2.GetHashCode());
            Assert.AreNotEqual(0, tag1.GetHashCode());
        }

        [Test]
        public void TestCaseEquality2()
        {
            var tag1 = Tag.Parse("$");
            var tag2 = Tag.Parse("$");

            Assert.AreEqual(tag1, tag2);
            Assert.AreEqual(tag1.GetHashCode(), tag2.GetHashCode());
            Assert.AreNotEqual(0, tag1.GetHashCode());
        }

        [Test]
        public void TestCaseEquality3()
        {
            var tag1 = Tag.Parse("?");
            var tag2 = Tag.Parse("?");

            Assert.AreEqual(tag1, tag2);
            Assert.AreEqual(tag1.GetHashCode(), tag2.GetHashCode());
            Assert.AreNotEqual(0, tag1.GetHashCode());
        }

        [Test]
        public void TestCaseEquality4()
        {
            var tag1 = Tag.Parse("? $");
            var tag2 = Tag.Parse("? $");

            Assert.AreEqual(tag1, tag2);
            Assert.AreEqual(tag1.GetHashCode(), tag2.GetHashCode());
            Assert.AreNotEqual(0, tag1.GetHashCode());
        }

        [Test]
        public void TestCaseEquality5()
        {
            var tag1 = Tag.Parse("HELLO WORLD");
            var tag2 = Tag.Parse("hello world");

            Assert.AreNotEqual(tag1, tag2);
            Assert.AreNotEqual(tag1.GetHashCode(), tag2.GetHashCode());

            Assert.IsTrue(tag1.Equals(tag2, StringComparer.OrdinalIgnoreCase));
            Assert.AreEqual(tag1.GetHashCode(StringComparer.OrdinalIgnoreCase), tag2.GetHashCode(StringComparer.OrdinalIgnoreCase));
        }

        [Test]
        public void TestCaseEquality6()
        {
            var tag1 = Tag.Parse("TEST $ ANY (OTHER OPERATOR) ? IN THE ? $ WORLD");
            var tag2 = Tag.Parse("TEST $ ANY (OTHER operator) ? IN THE ? $ WORLD");

            Assert.AreNotEqual(tag1, tag2);
            Assert.AreNotEqual(tag1.GetHashCode(), tag2.GetHashCode());

            Assert.IsTrue(tag1.Equals(tag2, StringComparer.OrdinalIgnoreCase));
            Assert.AreEqual(tag1.GetHashCode(StringComparer.OrdinalIgnoreCase), tag2.GetHashCode(StringComparer.OrdinalIgnoreCase));
        }

        [Test]
        public void TestCaseEquality7()
        {
            var tag1 = Tag.Parse("(K1)");
            var tag2 = Tag.Parse("(K1 K2)");

            Assert.AreNotEqual(tag1, tag2);
            Assert.AreNotEqual(tag1.GetHashCode(), tag2.GetHashCode());
        }

        [Test]
        public void TestCaseEquality8()
        {
            var tag1 = Tag.Parse("(K1)");
            Assert.AreNotEqual(tag1, null);
        }

        [Test]
        public void TestCaseEqualityExceptions()
        {
            var tag1 = Tag.Parse("(K1)");

            ExpectArgumentNullException("comparer", () => tag1.GetHashCode(null));
            ExpectArgumentNullException("comparer", () => tag1.Equals(tag1, null));
        }
    }
}

