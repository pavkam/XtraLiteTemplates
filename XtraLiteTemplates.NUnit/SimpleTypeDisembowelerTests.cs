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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using XtraLiteTemplates.Dialects.Standard;
    using XtraLiteTemplates.Expressions;

    [TestFixture]
    public class SimpleTypeDisembowelerTests : TestBase
    {
        [Test]
        public void TestCaseConstruction()
        {
            ExpectArgumentNullException("type", () => new SimpleTypeDisemboweler(null, SimpleTypeDisemboweler.EvaluationOptions.None, StringComparer.Ordinal));
            ExpectArgumentNullException("memberComparer", () => new SimpleTypeDisemboweler(typeof(String), SimpleTypeDisemboweler.EvaluationOptions.None, null));

            var disemboweler = new SimpleTypeDisemboweler(typeof(String),
                SimpleTypeDisemboweler.EvaluationOptions.TreatParameterlessFunctionsAsProperties |
                SimpleTypeDisemboweler.EvaluationOptions.TreatAllErrorsAsNull, StringComparer.CurrentCultureIgnoreCase);

            Assert.AreEqual(typeof(String), disemboweler.Type);
            Assert.AreEqual(SimpleTypeDisemboweler.EvaluationOptions.TreatParameterlessFunctionsAsProperties | 
                SimpleTypeDisemboweler.EvaluationOptions.TreatAllErrorsAsNull, disemboweler.Options);
            Assert.AreEqual(StringComparer.CurrentCultureIgnoreCase, disemboweler.Comparer);
        }

        [Test]
        public void TestCaseNoMethodsCaseSensitive()
        {
            var disemboweler = new SimpleTypeDisemboweler(typeof(String),
                SimpleTypeDisemboweler.EvaluationOptions.TreatAllErrorsAsNull, StringComparer.Ordinal);

            var value = "Hello World";

            Assert.AreEqual(value.Length, disemboweler.Read("Length", value));
            Assert.IsNull(disemboweler.Read("length", value));
            Assert.IsNull(disemboweler.Read("LENGTH", value));
            Assert.IsNull(disemboweler.Read("ToUpper", value));
        }

        [Test]
        public void TestCaseWithMethodsCaseSensitive()
        {
            var disemboweler = new SimpleTypeDisemboweler(typeof(String),
                SimpleTypeDisemboweler.EvaluationOptions.TreatParameterlessFunctionsAsProperties |
                SimpleTypeDisemboweler.EvaluationOptions.TreatAllErrorsAsNull, StringComparer.Ordinal);

            var value = "Hello World";

            Assert.AreEqual(value.Length, disemboweler.Read("Length", value));
            Assert.IsNull(disemboweler.Read("length", value));
            Assert.IsNull(disemboweler.Read("LENGTH", value));
            Assert.AreEqual(value.ToUpper(), disemboweler.Read("ToUpper", value));
        }

        [Test]
        public void TestCaseWithMethodsCaseInsensitive()
        {
            var disemboweler = new SimpleTypeDisemboweler(typeof(String),
                SimpleTypeDisemboweler.EvaluationOptions.TreatParameterlessFunctionsAsProperties |
                SimpleTypeDisemboweler.EvaluationOptions.TreatAllErrorsAsNull, StringComparer.OrdinalIgnoreCase);

            var value = "Hello World";

            Assert.AreEqual(value.Length, disemboweler.Read("Length", value));
            Assert.AreEqual(value.Length, disemboweler.Read("length", value));
            Assert.AreEqual(value.Length, disemboweler.Read("LENGTH", value));
            Assert.AreEqual(value.ToUpper(), disemboweler.Read("ToUpper", value));
            Assert.AreEqual(value.ToUpper(), disemboweler.Read("toupper", value));
            Assert.AreEqual(value.ToUpper(), disemboweler.Read("TOUPPER", value));
        }

        [Test]
        public void TestCaseSelectionTactics1()
        {
            var anon1 = new
            {
                VALUE = "1",
                Value = "2",
            };

            var disemboweler1 = new SimpleTypeDisemboweler(anon1.GetType(),
                SimpleTypeDisemboweler.EvaluationOptions.TreatParameterlessFunctionsAsProperties, StringComparer.OrdinalIgnoreCase);

            Assert.AreEqual("2", disemboweler1.Read("Value", anon1));
            Assert.AreEqual("2", disemboweler1.Read("value", anon1));
            Assert.AreEqual("2", disemboweler1.Read("VALUE", anon1));
        }

        [Test]
        public void TestCaseSelectionTactics2()
        {
            var anon1 = new
            {
                VALUE = "1",
                Value = "2",
            };

            var disemboweler1 = new SimpleTypeDisemboweler(anon1.GetType(),
                SimpleTypeDisemboweler.EvaluationOptions.TreatParameterlessFunctionsAsProperties | 
                SimpleTypeDisemboweler.EvaluationOptions.TreatAllErrorsAsNull, StringComparer.Ordinal);

            Assert.AreEqual("1", disemboweler1.Read("VALUE", anon1));
            Assert.AreEqual("2", disemboweler1.Read("Value", anon1));
            Assert.AreEqual(null, disemboweler1.Read("value", anon1));
        }

        [Test]
        public void TestCaseError1()
        {
            var disemboweler = new SimpleTypeDisemboweler(typeof(String), SimpleTypeDisemboweler.EvaluationOptions.None, StringComparer.Ordinal);
            var test = "test";

            ExpectInvalidObjectMemberNameException("length", () => disemboweler.Read("length", test));
            ExpectInvalidObjectMemberNameException("ToString", () => disemboweler.Read("ToString", test));
        }

        private class ThrowsErrorOnRead
        {
            public String Value
            {
                get
                {
                    throw new InvalidOperationException("Whoops!");
                }
            }
        }

        [Test]
        public void TestCaseError2()
        {
            var disemboweler = new SimpleTypeDisemboweler(typeof(ThrowsErrorOnRead), SimpleTypeDisemboweler.EvaluationOptions.None, StringComparer.Ordinal);
            var test = new ThrowsErrorOnRead();

            ExpectObjectMemberEvaluationErrorException("Value", () => disemboweler.Read("Value", test));
        }

        [Test]
        public void TestCaseError3()
        {
            var disemboweler = new SimpleTypeDisemboweler(typeof(String), SimpleTypeDisemboweler.EvaluationOptions.None, StringComparer.Ordinal);

            ExpectArgumentNullException("instance", () => disemboweler.Read("Value", null));
        }

        [Test]
        public void TestCaseError4()
        {
            var disemboweler = new SimpleTypeDisemboweler(typeof(String), SimpleTypeDisemboweler.EvaluationOptions.None, StringComparer.Ordinal);

            ExpectArgumentNullException("property", () => disemboweler.Read(null, String.Empty));
            ExpectArgumentEmptyException("property", () => disemboweler.Read("", String.Empty));
            ExpectArgumentNotIdentifierException("property", () => disemboweler.Read("+Value", String.Empty));
        }
    }
}

