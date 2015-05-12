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
    using XtraLiteTemplates.Expressions.Operators.Standard;
    using XtraLiteTemplates.Parsing;

    [TestFixture]
    public class TagTests : TestBase
    {
        private static void ExpectTagAnyIndentifierCannotFollowExpressionException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Indentifier tag component cannot follow an expression.", e.Message);
                return;
            }

            Assert.Fail();
        }

        private static void ExpectTagExpressionCannotFollowExpressionException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Expression tag component cannot follow another expression.", e.Message);
                return;
            }

            Assert.Fail();
        }

        private static void ExpectInvalidTagMarkupException(String markup, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(FormatException), e);
                Assert.AreEqual(String.Format("Invalid tag markup: '{0}'", markup), e.Message);
                return;
            }

            Assert.Fail();
        }


        [Test]
        public void TestCaseEmptyTag()
        {
            var tag = new Tag();

            Assert.AreEqual(String.Empty, tag.ToString());
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

            Assert.AreEqual("alpha _underscore with007_numbers", tag.ToString());
        }

        [Test]
        public void TestCaseAnyIdentifierTag()
        {
            var tag = new Tag();

            Assert.AreSame(tag, tag.Identifier());
            Assert.AreSame(tag, tag.Expression());
            ExpectTagAnyIndentifierCannotFollowExpressionException(() => tag.Identifier());

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

            ExpectArgumentEmptyException("candidates", () => tag.Identifier(new String[0]));

            ExpectArgumentNotIdentifierException("candidate", () => tag.Identifier("__good0ne", "0number"));
            ExpectArgumentNotIdentifierException("candidate", () => tag.Identifier("__good0ne", "-sign"));
            ExpectArgumentNotIdentifierException("candidate", () => tag.Identifier("__good0ne", " white space"));
            ExpectArgumentNotIdentifierException("candidate", () => tag.Identifier("__good0ne", "whitespace inside"));
            ExpectArgumentNotIdentifierException("candidate", () => tag.Identifier("__good0ne", "symbol|inside"));

            Assert.AreSame(tag, tag.Identifier("candidate1", "candidate2", "candidate3"));
            Assert.AreEqual("(candidate1 candidate2 candidate3)", tag.ToString());
        }

        [Test]
        public void TestCaseComplexTag()
        {
            var tag = new Tag();

            Assert.AreSame(tag, tag.Keyword("if"));
            Assert.AreSame(tag, tag.Expression());
            Assert.AreSame(tag, tag.Identifier("then", "otherwise", "something", "else"));
            Assert.AreSame(tag, tag.Expression());
            Assert.AreSame(tag, tag.Keyword("kewl"));
            Assert.AreSame(tag, tag.Identifier());

            Assert.AreEqual("if $ (then otherwise something else) $ kewl ?", tag.ToString());
        }

        [Test]
        public void TestCaseTryParseFailures()
        {
            Tag tag;

            Assert.IsFalse(Tag.TryParse(null, out tag));
            Assert.IsFalse(Tag.TryParse(String.Empty, out tag));
            Assert.IsFalse(Tag.TryParse(" ", out tag));
            Assert.IsFalse(Tag.TryParse("{", out tag));
            Assert.IsFalse(Tag.TryParse("9", out tag));
            Assert.IsFalse(Tag.TryParse("KEYWORD+FAILURE", out tag));
            Assert.IsFalse(Tag.TryParse("(LIST OF ITEMS", out tag));
            Assert.IsFalse(Tag.TryParse("LIST OF ITEMS)", out tag));
            Assert.IsFalse(Tag.TryParse("??", out tag));
            Assert.IsFalse(Tag.TryParse("(?)", out tag));
            Assert.IsFalse(Tag.TryParse("($)", out tag));
            Assert.IsFalse(Tag.TryParse("EMPTY () GROUP", out tag));
            Assert.IsFalse(Tag.TryParse("(ITEM () ITEM)", out tag));
            Assert.IsFalse(Tag.TryParse("$$", out tag));
            Assert.IsFalse(Tag.TryParse("?$", out tag));
            Assert.IsFalse(Tag.TryParse("IF $ THEN $ ?", out tag));
            Assert.IsFalse(Tag.TryParse("$ $", out tag));
        }

        [Test]
        public void TestCaseTryParse()
        {
            Tag tag;

            Assert.IsTrue(Tag.TryParse("$", out tag) && tag.ToString() == "$");
            Assert.IsTrue(Tag.TryParse("$  ", out tag) && tag.ToString() == "$");
            Assert.IsTrue(Tag.TryParse("  $", out tag) && tag.ToString() == "$");
            Assert.IsTrue(Tag.TryParse("TERM  $      TERM", out tag) && tag.ToString() == "TERM $ TERM");
            Assert.IsTrue(Tag.TryParse(" ( T1 T2  T3   )(V1  )$ K1 ? K2", out tag) && tag.ToString() == "(T1 T2 T3) (V1) $ K1 ? K2");
        }

        [Test]
        public void TestCaseParse()
        {
            ExpectInvalidTagMarkupException("IF $ $", () => Tag.Parse("IF $ $"));
            ExpectInvalidTagMarkupException("", () => Tag.Parse(""));

            var tag = Tag.Parse("HELLO ? WORLD $ (A1 A2 A3)");
            Assert.AreEqual("HELLO ? WORLD $ (A1 A2 A3)", tag.ToString());
        }
    }
}

