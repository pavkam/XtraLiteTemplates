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
using NUnit.Framework;

namespace XtraLiteTemplates.NUnit
{
    using System;
    using System.Dynamic;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using XtraLiteTemplates.Dialects.Standard;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Introspection;

    [TestFixture]
    public class SimpleTypeDisembowelerTests : TestBase
    {
        [Test]
        public void TestCaseConstruction()
        {
            ExpectArgumentNullException("type", () => new SimpleTypeDisemboweler(null, StringComparer.Ordinal, ObjectFormatter));
            ExpectArgumentNullException("memberComparer", () => new SimpleTypeDisemboweler(typeof(String), null, ObjectFormatter));
            ExpectArgumentNullException("objectFormatter", () => new SimpleTypeDisemboweler(typeof(String), StringComparer.Ordinal, null));

            var disemboweler = new SimpleTypeDisemboweler(
                typeof(String),
                StringComparer.CurrentCultureIgnoreCase,
                ObjectFormatter);

            Assert.AreEqual(typeof(String), disemboweler.Type);
            Assert.AreEqual(StringComparer.CurrentCultureIgnoreCase, disemboweler.Comparer);
            Assert.AreEqual(ObjectFormatter, disemboweler.ObjectFormatter);
        }

        [Test]
        public void TestCaseSensitive()
        {
            var disemboweler = new SimpleTypeDisemboweler(
                typeof(String),
                StringComparer.Ordinal, 
                ObjectFormatter);

            var value = "Hello World";

            Assert.AreEqual(value.Length, disemboweler.Invoke(value, "Length"));
            Assert.AreEqual("HELLO WORLD", disemboweler.Invoke(value, "ToUpper"));
            Assert.IsNull(disemboweler.Invoke(value, "length"));
            Assert.IsNull(disemboweler.Invoke(value, "LENGTH"));
            Assert.IsNull(disemboweler.Invoke(value, "Toupper"));
        }

        [Test]
        public void TestCaseInsensitive()
        {
            var disemboweler = new SimpleTypeDisemboweler(
                typeof(String),
                StringComparer.OrdinalIgnoreCase, 
                ObjectFormatter);

            var value = "Hello World";

            Assert.AreEqual(value.Length, disemboweler.Invoke(value, "Length"));
            Assert.AreEqual(value.Length, disemboweler.Invoke(value, "length"));
            Assert.AreEqual(value.Length, disemboweler.Invoke(value, "LENGTH"));
            Assert.AreEqual(value.ToUpper(), disemboweler.Invoke(value, "ToUpper"));
            Assert.AreEqual(value.ToUpper(), disemboweler.Invoke(value, "toupper"));
            Assert.AreEqual(value.ToUpper(), disemboweler.Invoke(value, "TOUPPER"));
        }

        [Test]
        public void TestCaseCasingSelectionTactics1()
        {
            var anon1 = new
            {
                VALUE = "1",
                Value = "2",
            };

            var disemboweler1 = new SimpleTypeDisemboweler(
                anon1.GetType(),
                StringComparer.OrdinalIgnoreCase, ObjectFormatter);

            Assert.AreEqual("1", disemboweler1.Invoke(anon1, "Value"));
            Assert.AreEqual("1", disemboweler1.Invoke(anon1, "value"));
            Assert.AreEqual("1", disemboweler1.Invoke(anon1, "VALUE"));
        }

        [Test]
        public void TestCaseCasingSelectionTactics2()
        {
            var anon1 = new
            {
                VALUE = "1",
                Value = "2",
            };

            var disemboweler1 = new SimpleTypeDisemboweler(
                anon1.GetType(),
                StringComparer.Ordinal, 
                ObjectFormatter);

            Assert.AreEqual("1", disemboweler1.Invoke(anon1, "VALUE"));
            Assert.AreEqual("2", disemboweler1.Invoke(anon1, "Value"));
            Assert.AreEqual(null, disemboweler1.Invoke(anon1, "value"));
        }

        [Test]
        public void TestCaseParamErrors()
        {
            var disemboweler = new SimpleTypeDisemboweler(
                typeof(String), 
                StringComparer.Ordinal, 
                ObjectFormatter);

            ExpectArgumentNullException("member", () => disemboweler.Invoke(String.Empty, null));
            ExpectArgumentEmptyException("member", () => disemboweler.Invoke(String.Empty, ""));
            ExpectArgumentNotIdentifierException("member", () => disemboweler.Invoke(String.Empty, "+Value"));
        }

        private class SelectiveObject
        {
            private List<string> journal = new List<string>();

            public void Function1(Object arg)
            {
                journal.Add(String.Format("Object({0})", arg));
            }

            public void Function1(String arg)
            {
                journal.Add(String.Format("String({0})", arg));
            }

            public void Function1(Boolean arg)
            {
                journal.Add(String.Format("Boolean({0})", arg));
            }

            public void Function1(Int32 arg)
            {
                journal.Add(String.Format("Int32({0})", arg));
            }

            public void Function1(Double arg)
            {
                journal.Add(String.Format("Double({0})", arg));
            }

            public override string ToString()
            {
                return String.Join(", ", journal);
            }
        }

        [Test]
        public void TestCaseOverloadedMethodSelection1()
        {
            var selectiveObject = new SelectiveObject();

            var disemboweler = new SimpleTypeDisemboweler(
                typeof(SelectiveObject),
                StringComparer.Ordinal,
                ObjectFormatter);

            disemboweler.Invoke(selectiveObject, "Function1", null);
            disemboweler.Invoke(selectiveObject, "Function1", new Object[] { "text" });
            disemboweler.Invoke(selectiveObject, "Function1", new Object[] { StringComparer.InvariantCulture } );
            disemboweler.Invoke(selectiveObject, "Function1", new Object[] { 100 } );
            disemboweler.Invoke(selectiveObject, "Function1", new Object[] { 100.0 } );
            disemboweler.Invoke(selectiveObject, "Function1", new Object[] { true });

            Assert.AreEqual("Object(), String(text), Object(System.CultureAwareComparer), Int32(100), Double(100), Boolean(True)", selectiveObject.ToString());
        }
    }
}

