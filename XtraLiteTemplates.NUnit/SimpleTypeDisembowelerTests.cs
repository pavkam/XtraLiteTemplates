//
//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2017, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.NUnit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using global::NUnit.Framework;
    using Introspection;

    [TestFixture]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
    public class SimpleTypeDisembowelerTests : TestBase
    {
        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseConstruction()
        {
            ExpectArgumentNullException("type", () => new SimpleTypeDisemboweler(null, StringComparer.Ordinal, ObjectFormatter));
            ExpectArgumentNullException("memberComparer", () => new SimpleTypeDisemboweler(typeof(string), null, ObjectFormatter));
            ExpectArgumentNullException("objectFormatter", () => new SimpleTypeDisemboweler(typeof(string), StringComparer.Ordinal, null));

            var disemboweler = new SimpleTypeDisemboweler(
                typeof(string),
                StringComparer.CurrentCultureIgnoreCase,
                ObjectFormatter);

            Assert.AreEqual(typeof(string), disemboweler.Type);
            Assert.AreEqual(StringComparer.CurrentCultureIgnoreCase, disemboweler.Comparer);
            Assert.AreEqual(ObjectFormatter, disemboweler.ObjectFormatter);
        }

        [Test]
        public void TestCaseSensitive()
        {
            var disemboweler = new SimpleTypeDisemboweler(
                typeof(string),
                StringComparer.Ordinal, 
                ObjectFormatter);

            const string Value = "Hello World";

            Assert.AreEqual(Value.Length, disemboweler.Invoke(Value, "Length"));
            Assert.AreEqual("HELLO WORLD", disemboweler.Invoke(Value, "ToUpper"));
            Assert.IsNull(disemboweler.Invoke(Value, "length"));
            Assert.IsNull(disemboweler.Invoke(Value, "LENGTH"));
            Assert.IsNull(disemboweler.Invoke(Value, "Toupper"));
        }

        [Test]
        public void TestCaseInsensitive()
        {
            var disemboweler = new SimpleTypeDisemboweler(
                typeof(string),
                StringComparer.OrdinalIgnoreCase, 
                ObjectFormatter);

            const string Value = "Hello World";

            Assert.AreEqual(Value.Length, disemboweler.Invoke(Value, "Length"));
            Assert.AreEqual(Value.Length, disemboweler.Invoke(Value, "length"));
            Assert.AreEqual(Value.Length, disemboweler.Invoke(Value, "LENGTH"));
            Assert.AreEqual(Value.ToUpper(), disemboweler.Invoke(Value, "ToUpper"));
            Assert.AreEqual(Value.ToUpper(), disemboweler.Invoke(Value, "toupper"));
            Assert.AreEqual(Value.ToUpper(), disemboweler.Invoke(Value, "TOUPPER"));
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
                typeof(string), 
                StringComparer.Ordinal, 
                ObjectFormatter);

            ExpectArgumentNullException("member", () => disemboweler.Invoke(string.Empty, null));
            ExpectArgumentEmptyException("member", () => disemboweler.Invoke(string.Empty, string.Empty));
            ExpectArgumentNotIdentifierException("member", () => disemboweler.Invoke(string.Empty, "+Value"));
        }

        [SuppressMessage("ReSharper", "UnusedMember.Local")]
        private class SelectiveObject
        {
            private readonly List<string> _journal = new List<string>();

            public void Function1(object arg)
            {
                _journal.Add($"Object({arg})");
            }

            public void Function1(string arg)
            {
                _journal.Add($"String({arg})");
            }

            public void Function1(bool arg)
            {
                _journal.Add($"Boolean({arg})");
            }

            public void Function1(short arg)
            {
                _journal.Add($"Int16({arg})");
            }

            public void Function1(int arg)
            {
                _journal.Add($"Int32({arg})");
            }

            public void Function1(double arg)
            {
                _journal.Add($"Double({arg})");
            }

            public void Function2(object arg)
            {
                _journal.Add($"Object({arg})");
            }

            public void Function3(object arg0, int arg1)
            {
                _journal.Add($"[Object({arg0}) Int32({arg1})]");
            }

            public void Function3(object arg0, object arg1)
            {
                _journal.Add($"[Object({arg0}) Object({arg1})]");
            }

            public void Function4(params string[] args)
            {
                _journal.Add($"[...String({string.Join(",", args)})]");
            }

            public void Function5(params int[] args)
            {
                _journal.Add($"[...Int32({string.Join(",", args)})]");
            }

            public void Function6(object arg0, object arg1)
            {
                _journal.Add($"[Object({arg0}) Object({arg1})]");
            }

            public void Function6(object arg0, params object[] args)
            {
                _journal.Add($"[Object({arg0}) ...Object({string.Join(",", args)})]");
            }

            public override string ToString()
            {
                return string.Join(", ", _journal);
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

            disemboweler.Invoke(selectiveObject, "Function1");
            disemboweler.Invoke(selectiveObject, "Function1", new object[] { "text" });
            disemboweler.Invoke(selectiveObject, "Function1", new object[] { StringComparer.InvariantCulture } );
            disemboweler.Invoke(selectiveObject, "Function1", new object[] { 100 } );
            disemboweler.Invoke(selectiveObject, "Function1", new object[] { (short)99 });
            disemboweler.Invoke(selectiveObject, "Function1", new object[] { 100.0 } );
            disemboweler.Invoke(selectiveObject, "Function1", new object[] { true });

            Assert.AreEqual("Object(), String(text), Object(System.CultureAwareComparer), Int32(100), Int16(99), Double(100), Boolean(True)", selectiveObject.ToString());
        }

        [Test]
        public void TestCaseOverloadedMethodSelection2()
        {
            var selectiveObject = new SelectiveObject();

            var disemboweler = new SimpleTypeDisemboweler(
                typeof(SelectiveObject),
                StringComparer.Ordinal,
                ObjectFormatter);

            disemboweler.Invoke(selectiveObject, "Function2");
            disemboweler.Invoke(selectiveObject, "Function2", new object[] { "text", 34, 78 });
            disemboweler.Invoke(selectiveObject, "Function2", new object[] { StringComparer.InvariantCulture, 18 });
            disemboweler.Invoke(selectiveObject, "Function2", new object[] { 100, this });
            disemboweler.Invoke(selectiveObject, "Function2", new object[] { (short)99, string.Empty });
            disemboweler.Invoke(selectiveObject, "Function2", new object[] { 100.0, true });
            disemboweler.Invoke(selectiveObject, "Function2", new object[] { true, 1, 2 });

            Assert.AreEqual("Object(), Object(text), Object(System.CultureAwareComparer), Object(100), Object(99), Object(100), Object(True)", selectiveObject.ToString());
        }

        [Test]
        public void TestCaseOverloadedMethodSelection3()
        {
            var selectiveObject = new SelectiveObject();

            var disemboweler = new SimpleTypeDisemboweler(
                typeof(SelectiveObject),
                StringComparer.Ordinal,
                ObjectFormatter);

            disemboweler.Invoke(selectiveObject, "Function3");
            disemboweler.Invoke(selectiveObject, "Function3", new object[] { null, null } );
            disemboweler.Invoke(selectiveObject, "Function3", new object[] { "text", 34, 78 });
            disemboweler.Invoke(selectiveObject, "Function3", new object[] { StringComparer.InvariantCulture, (byte)18 });
            disemboweler.Invoke(selectiveObject, "Function3", new object[] { 100, null });
            disemboweler.Invoke(selectiveObject, "Function3", new object[] { (short)99, string.Empty });
            disemboweler.Invoke(selectiveObject, "Function3", new object[] { 100.0, true });
            disemboweler.Invoke(selectiveObject, "Function3", new object[] { true, 1.99, 2 });

            Assert.AreEqual("[Object() Object()], [Object() Object()], [Object(text) Int32(34)], [Object(System.CultureAwareComparer) Int32(18)], [Object(100) Object()], [Object(99) Object()], [Object(100) Object(True)], [Object(True) Int32(2)]", selectiveObject.ToString());
        }

        [Test]
        public void TestCaseOverloadedMethodSelection4()
        {
            var selectiveObject = new SelectiveObject();

            var disemboweler = new SimpleTypeDisemboweler(
                typeof(SelectiveObject),
                StringComparer.Ordinal,
                ObjectFormatter);

            disemboweler.Invoke(selectiveObject, "Function4");
            disemboweler.Invoke(selectiveObject, "Function4", new object[] { null, null });
            disemboweler.Invoke(selectiveObject, "Function4", new object[] { true, 1.99, 2 });

            Assert.AreEqual("[...String()], [...String(!undefined!,!undefined!)], [...String(True,1.99,2)]", selectiveObject.ToString());
        }

        [Test]
        public void TestCaseOverloadedMethodSelection5()
        {
            var selectiveObject = new SelectiveObject();

            var disemboweler = new SimpleTypeDisemboweler(
                typeof(SelectiveObject),
                StringComparer.Ordinal,
                ObjectFormatter);

            disemboweler.Invoke(selectiveObject, "Function5");
            disemboweler.Invoke(selectiveObject, "Function5", new object[] { 1, 2 });
            disemboweler.Invoke(selectiveObject, "Function5", new object[] { (byte)1, (ushort)2, (uint)3, (long)4, 5.33, 6.23M });

            Assert.AreEqual("[...Int32()], [...Int32(1,2)], [...Int32(1,2,3,4,5,6)]", selectiveObject.ToString());
        }

        [Test]
        public void TestCaseOverloadedMethodSelection6()
        {
            var selectiveObject = new SelectiveObject();

            var disemboweler = new SimpleTypeDisemboweler(
                typeof(SelectiveObject),
                StringComparer.Ordinal,
                ObjectFormatter);

            disemboweler.Invoke(selectiveObject, "Function6");
            disemboweler.Invoke(selectiveObject, "Function6", new object[] { null });
            disemboweler.Invoke(selectiveObject, "Function6", new object[] { null, null });
            disemboweler.Invoke(selectiveObject, "Function6", new object[] { null, null, null });
            disemboweler.Invoke(selectiveObject, "Function6", new object[] { 1 });
            disemboweler.Invoke(selectiveObject, "Function6", new object[] { 1, "a" });
            disemboweler.Invoke(selectiveObject, "Function6", new object[] { 1, "a", false });

            Assert.AreEqual("[Object() ...Object()], [Object() ...Object()], [Object() Object()], [Object() ...Object()], [Object(1) ...Object()], [Object(1) Object(a)], [Object(1) ...Object(a,False)]", selectiveObject.ToString());
        }

        [Test]
        public void TestCaseStringFormat()
        {
            var disemboweler = new SimpleTypeDisemboweler(typeof(string), StringComparer.Ordinal, ObjectFormatter);
            var result = disemboweler.Invoke(string.Empty, "Format", new object[] { "{0} {1} {2}!", "Hello", "weird", "world" } );

            Assert.AreEqual("Hello weird world!", result);
        }

        [Test]
        public void TestCaseCasingSelectionRejection()
        {
            var anon1 = new
            {
                A = "1",
                a = "2",
            };

            var disemboweler1 = new SimpleTypeDisemboweler(
                anon1.GetType(),
                StringComparer.OrdinalIgnoreCase, ObjectFormatter);

            disemboweler1.ValidateMember += (sender, e) =>
            {
                Assert.AreSame(disemboweler1, sender);
                Assert.IsTrue(e.Accepted);
                Assert.NotNull(e.Member);
                Assert.AreSame(e.Member.DeclaringType, anon1.GetType());

                if (e.Member.Name == "A")
                {
                    e.Accepted = false;
                }
            };

            Assert.AreEqual("2", disemboweler1.Invoke(anon1, "A"));
            Assert.AreEqual("2", disemboweler1.Invoke(anon1, "a"));
        }
    }
}

