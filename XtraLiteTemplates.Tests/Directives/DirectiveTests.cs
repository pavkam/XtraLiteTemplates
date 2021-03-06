﻿//
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

namespace XtraLiteTemplates.Tests.Directives
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using global::NUnit.Framework;
    using XtraLiteTemplates.Tests.Inside;
    using XtraLiteTemplates.Parsing;

    [TestFixture]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class DirectiveTests : TestBase
    {
        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseDirectiveConstructorExceptions()
        {
            var tag = Tag.Parse("HELLO WORLD");

            ExpectArgumentEmptyException("tags", () => new RippedOpenDirective());
            ExpectArgumentNullException("tags", () => new RippedOpenDirective((Tag)null));
            ExpectArgumentNullException("tags", () => new RippedOpenDirective(tag, null));
            ExpectCannotRegisterTagWithNoComponentsException(() => new RippedOpenDirective(new Tag()));
            ExpectCannotRegisterTagWithNoComponentsException(() => new RippedOpenDirective(tag, new Tag()));
        }

        [Test]
        public void TestCaseDirectiveConstructorOneTag()
        {
            var tag = Tag.Parse("HELLO $ WORLD");

            var directive = new RippedOpenDirective(tag);
            var repr = directive.ToString();

            Assert.AreEqual("{HELLO $ WORLD}", repr);
        }

        [Test]
        public void TestCaseDirectiveConstructorTwoTags()
        {
            var tag1 = Tag.Parse("IF $ THEN");
            var tag2 = Tag.Parse("END IF");

            var directive = new RippedOpenDirective(tag1, tag2);
            var repr = directive.ToString();

            Assert.AreEqual("{IF $ THEN}...{END IF}", repr);
        }

        [Test]
        public void TestCaseDirectiveConstructorThreeTags()
        {
            var tag1 = Tag.Parse("IF $ THEN");
            var tag2 = Tag.Parse("ELSE");
            var tag3 = Tag.Parse("END IF");

            var directive = new RippedOpenDirective(tag1, tag2, tag3);
            var repr = directive.ToString();

            Assert.AreEqual("{IF $ THEN}...{ELSE}...{END IF}", repr);
        }

        [Test]
        public void TestCaseDirectiveConstructorSameTagAFewTimes()
        {
            var tag = Tag.Parse("I AM ? TAG");

            var directive = new RippedOpenDirective(tag, tag, tag);
            var repr = directive.ToString();

            Assert.AreEqual("{I AM ? TAG}...{I AM ? TAG}...{I AM ? TAG}", repr);
        }

        [Test]
        public void TestCaseDirectiveEqualityExceptions()
        {
            var directive1 = new RippedOpenDirective(Tag.Parse("NOTHING"));

            ExpectArgumentNullException("comparer", () => directive1.Equals(directive1, null));
            ExpectArgumentNullException("comparer", () => directive1.GetHashCode(null));
        }

        [Test]
        public void TestCaseDirectiveEquality()
        {
            var directive1 = new RippedOpenDirective(
                Tag.Parse("IF ? IS (BAD GOOD) TO $ THEN"),
                Tag.Parse("ELSE IF NOT SET"),
                Tag.Parse("END ALL $ LIFE"));

            var directive2 = new RippedOpenDirective(
                Tag.Parse("if ? is (good bad) to $ then"),
                Tag.Parse("else if not set"),
                Tag.Parse("end all $ life"));

            Assert.AreEqual(directive1, directive1);
            Assert.IsFalse(directive1.Equals(null, StringComparer.OrdinalIgnoreCase));
            Assert.IsTrue(directive1.Equals(directive1, StringComparer.OrdinalIgnoreCase));
            Assert.IsTrue(directive1.Equals(directive1, StringComparer.CurrentCulture));

            Assert.AreEqual(directive2, directive2);
            Assert.IsFalse(directive2.Equals(null, StringComparer.OrdinalIgnoreCase));
            Assert.IsTrue(directive2.Equals(directive2, StringComparer.OrdinalIgnoreCase));
            Assert.IsTrue(directive2.Equals(directive2, StringComparer.CurrentCulture));

            Assert.AreNotEqual(directive1, directive2);
            Assert.IsTrue(directive1.Equals(directive2, StringComparer.OrdinalIgnoreCase));
            Assert.IsFalse(directive1.Equals(directive2, StringComparer.CurrentCulture));

            Assert.AreNotEqual(directive1.GetHashCode(), directive2.GetHashCode());
            Assert.AreEqual(directive1.GetHashCode(StringComparer.CurrentCultureIgnoreCase), directive2.GetHashCode(StringComparer.CurrentCultureIgnoreCase));
        }
    }
}

