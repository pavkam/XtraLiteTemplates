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
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using global::NUnit.Framework;
    using XtraLiteTemplates.Introspection;
    using XtraLiteTemplates.Tests.Inside;

    [TestFixture]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class FlexiblePrimitiveTypeConverterTests : TestBase
    {
        [Test]
        public void TestCaseConstruction()
        {
            var objectFormatter = new TestObjectFormatter(CultureInfo.CurrentCulture);

            ExpectArgumentNullException("formatProvider", () => new FlexiblePrimitiveTypeConverter(null, objectFormatter));
            ExpectArgumentNullException("objectFormatter", () => new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, null));

            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, objectFormatter);
            Assert.AreEqual(CultureInfo.InvariantCulture, converter.FormatProvider);
            Assert.AreEqual(objectFormatter, converter.ObjectFormatter);
        }

        [Test]
        public void TestCaseTypeOf()
        {
            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(byte)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(sbyte)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(short)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(ushort)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(int)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(uint)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(long)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(ulong)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(float)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(double)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(decimal)));
            Assert.AreEqual(PrimitiveType.Boolean, converter.TypeOf(default(bool)));
            Assert.AreEqual(PrimitiveType.String, converter.TypeOf(string.Empty));
            Assert.AreEqual(PrimitiveType.Undefined, converter.TypeOf(null));
            Assert.AreEqual(PrimitiveType.Sequence, converter.TypeOf(new object[] { 1, 2 }));
            Assert.AreEqual(PrimitiveType.Sequence, converter.TypeOf(new[] { 1, 2 }));
            Assert.AreEqual(PrimitiveType.Object, converter.TypeOf(this));
        }

        [Test]
        public void TestCaseConvertToBoolean()
        {
            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

            Assert.AreEqual(false, converter.ConvertToBoolean(default(byte)));
            Assert.AreEqual(true, converter.ConvertToBoolean((byte)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(sbyte)));
            Assert.AreEqual(true, converter.ConvertToBoolean((sbyte)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(short)));
            Assert.AreEqual(true, converter.ConvertToBoolean((short)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(ushort)));
            Assert.AreEqual(true, converter.ConvertToBoolean((ushort)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(int)));
            Assert.AreEqual(true, converter.ConvertToBoolean(10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(uint)));
            Assert.AreEqual(true, converter.ConvertToBoolean((uint)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(long)));
            Assert.AreEqual(true, converter.ConvertToBoolean((long)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(ulong)));
            Assert.AreEqual(true, converter.ConvertToBoolean((ulong)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(float)));
            Assert.AreEqual(true, converter.ConvertToBoolean((float)0.1));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(double)));
            Assert.AreEqual(true, converter.ConvertToBoolean(0.1));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(decimal)));
            Assert.AreEqual(true, converter.ConvertToBoolean((decimal)0.1));
            Assert.AreEqual(false, converter.ConvertToBoolean(false));
            Assert.AreEqual(true, converter.ConvertToBoolean(true));
            Assert.AreEqual(false, converter.ConvertToBoolean(string.Empty));
            Assert.AreEqual(true, converter.ConvertToBoolean("anything"));
            Assert.AreEqual(false, converter.ConvertToBoolean(null));
            Assert.AreEqual(true, converter.ConvertToBoolean(this));
            Assert.AreEqual(true, converter.ConvertToBoolean(new string[0]));
            Assert.AreEqual(true, converter.ConvertToBoolean(new[] { 1 } ));
            Assert.AreEqual(true, converter.ConvertToBoolean(new object[] { "hello", 1 }));
        }

        [Test]
        public void TestCaseConvertToString()
        {
            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

            Assert.AreEqual("10", converter.ConvertToString((byte)10));
            Assert.AreEqual("10", converter.ConvertToString((sbyte)10));
            Assert.AreEqual("10", converter.ConvertToString((short)10));
            Assert.AreEqual("10", converter.ConvertToString((ushort)10));
            Assert.AreEqual("10", converter.ConvertToString(10));
            Assert.AreEqual("10", converter.ConvertToString((uint)10));
            Assert.AreEqual("10", converter.ConvertToString((long)10));
            Assert.AreEqual("10", converter.ConvertToString((ulong)10));
            Assert.AreEqual("10.3299999237061", converter.ConvertToString((float)10.33));
            Assert.AreEqual("10.33", converter.ConvertToString(10.33));
            Assert.AreEqual("10.33", converter.ConvertToString((decimal)10.33));
            Assert.AreEqual("False", converter.ConvertToString(false));
            Assert.AreEqual("True", converter.ConvertToString(true));
            Assert.AreEqual(string.Empty, converter.ConvertToString(string.Empty));
            Assert.AreEqual("anything", converter.ConvertToString("anything"));
            Assert.AreEqual("!undefined!", converter.ConvertToString(null));
            Assert.AreEqual(ToString(), converter.ConvertToString(this));
            Assert.AreEqual(string.Empty, converter.ConvertToString(new object[0]));
            Assert.AreEqual("1", converter.ConvertToString(new[] { 1 }));
            Assert.AreEqual("hello,1", converter.ConvertToString(new object[] { "hello", 1 }));
            Assert.AreEqual("1,2,3", converter.ConvertToString(new object[] { 1, new object[] { 2, 3 } }));
        }

        [Test]
        public void TestCaseConvertToNumber()
        {
            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

            Assert.AreEqual(10, converter.ConvertToNumber((byte)10));
            Assert.AreEqual(-10, converter.ConvertToNumber((sbyte)(-10)));
            Assert.AreEqual(-10, converter.ConvertToNumber((short)(-10)));
            Assert.AreEqual(10, converter.ConvertToNumber((ushort)10));
            Assert.AreEqual(-10, converter.ConvertToNumber(-10));
            Assert.AreEqual(10, converter.ConvertToNumber((uint)10));
            Assert.AreEqual(-10, converter.ConvertToNumber((long)(-10)));
            Assert.AreEqual(10, converter.ConvertToNumber((ulong)10));
            Assert.AreEqual(10.329999923706055, converter.ConvertToNumber((float)10.33));
            Assert.AreEqual(10.33, converter.ConvertToNumber(10.33));
            Assert.AreEqual(10.33, converter.ConvertToNumber((decimal)10.33));
            Assert.AreEqual(0, converter.ConvertToNumber(false));
            Assert.AreEqual(1, converter.ConvertToNumber(true));
            Assert.AreEqual(0, converter.ConvertToNumber(string.Empty));
            Assert.AreEqual(double.NaN, converter.ConvertToNumber("anything"));
            Assert.AreEqual(-10.33, converter.ConvertToNumber("-10.33"));
            Assert.AreEqual(double.NaN, converter.ConvertToNumber(null));
            Assert.AreEqual(double.NaN, converter.ConvertToNumber(this));
            Assert.AreEqual(0, converter.ConvertToNumber(new object[0]));
            Assert.AreEqual(12, converter.ConvertToNumber(new[] { 12 }));
            Assert.AreEqual(double.NaN, converter.ConvertToNumber(new[] { 1, 2 }));
            Assert.AreEqual(0, converter.ConvertToNumber(new object[] { string.Empty }));
            Assert.AreEqual(-77, converter.ConvertToNumber(new object[] { "-77" }));
            Assert.AreEqual(double.NaN, converter.ConvertToNumber(new object[] { 1, new object[] { 2, 3 } }));
        }

        [Test]
        public void TestCaseConvertToInteger()
        {
            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

            Assert.AreEqual(1, converter.ConvertToInteger((byte)1));
            Assert.AreEqual(-1, converter.ConvertToInteger((sbyte)(-1)));
            Assert.AreEqual(-1, converter.ConvertToInteger((short)(-1)));
            Assert.AreEqual(1, converter.ConvertToInteger((ushort)1));
            Assert.AreEqual(-1, converter.ConvertToInteger(-1));
            Assert.AreEqual(1, converter.ConvertToInteger((uint)1));
            Assert.AreEqual(1, converter.ConvertToInteger(1L));
            Assert.AreEqual(-1, converter.ConvertToInteger(-1L));
            Assert.AreEqual(-10, converter.ConvertToInteger((float)(-10.83)));
            Assert.AreEqual(-10, converter.ConvertToInteger(-10.83));
            Assert.AreEqual(-10, converter.ConvertToInteger((decimal)(-10.83)));
            Assert.AreEqual(0, converter.ConvertToInteger(false));
            Assert.AreEqual(1, converter.ConvertToInteger(true));
            Assert.AreEqual(0, converter.ConvertToInteger(string.Empty));
            Assert.AreEqual(0, converter.ConvertToInteger("anything"));
            Assert.AreEqual(-10, converter.ConvertToInteger("-10.88"));
            Assert.AreEqual(0, converter.ConvertToInteger(null));
            Assert.AreEqual(0, converter.ConvertToInteger(this));
            Assert.AreEqual(0, converter.ConvertToInteger(new object[0]));
            Assert.AreEqual(12, converter.ConvertToInteger(new[] { 12 }));
            Assert.AreEqual(0, converter.ConvertToInteger(new[] { 1, 2 }));
            Assert.AreEqual(0, converter.ConvertToInteger(new object[] { string.Empty }));
            Assert.AreEqual(-77, converter.ConvertToInteger(new object[] { "-77" }));
            Assert.AreEqual(0, converter.ConvertToInteger(new object[] { 1, new object[] { 2, 3 } }));
        }

        [Test]
        public void TestCaseConvertToSequence()
        {
            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

            /* Undefined */
            Assert.IsNull(converter.ConvertToSequence(null));

            /* Single element */
            var sequence = converter.ConvertToSequence(-10);
            Assert.IsInstanceOf<IEnumerable<object>>(sequence);
            var consolidated = sequence.ToArray();
            Assert.AreEqual(1, consolidated.Length);
            Assert.AreEqual(-10, consolidated[0]);

            /* Two objects */
            sequence = converter.ConvertToSequence(new[] { "Hello", "World" });
            Assert.IsInstanceOf<IEnumerable<object>>(sequence);
            consolidated = sequence.ToArray();
            Assert.AreEqual(2, consolidated.Length);
            Assert.AreEqual("Hello", consolidated[0]);
            Assert.AreEqual("World", consolidated[1]);

            /* String */
            sequence = converter.ConvertToSequence("STR");
            Assert.IsInstanceOf<IEnumerable<object>>(sequence);
            consolidated = sequence.ToArray();
            Assert.AreEqual(3, consolidated.Length);
            Assert.AreEqual('S', consolidated[0]);
            Assert.AreEqual('T', consolidated[1]);
            Assert.AreEqual('R', consolidated[2]);
        }

        [Test]
        public void TestCaseCultureSpecific()
        {
            var culture = CultureInfo.GetCultureInfo("ro-RO");
            var objectFormatter = new TestObjectFormatter(culture);
            var converter = new FlexiblePrimitiveTypeConverter(culture, objectFormatter);

            Assert.AreEqual(-1.234, converter.ConvertToNumber("-1,234"));
            Assert.AreEqual("-1234,5", converter.ConvertToString(-1234.5));
        }
    }
}

