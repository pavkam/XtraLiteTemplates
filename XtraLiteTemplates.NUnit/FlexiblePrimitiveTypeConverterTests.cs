//
//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex+git@ciobanu.org)
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
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using XtraLiteTemplates.Dialects.Standard;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Introspection;
    using XtraLiteTemplates.NUnit.Inside;

    [TestFixture]
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

            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(Byte)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(SByte)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(Int16)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(UInt16)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(Int32)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(UInt32)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(Int64)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(UInt64)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(Single)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(Double)));
            Assert.AreEqual(PrimitiveType.Number, converter.TypeOf(default(Decimal)));
            Assert.AreEqual(PrimitiveType.Boolean, converter.TypeOf(default(Boolean)));
            Assert.AreEqual(PrimitiveType.String, converter.TypeOf(String.Empty));
            Assert.AreEqual(PrimitiveType.Undefined, converter.TypeOf(null));
            Assert.AreEqual(PrimitiveType.Sequence, converter.TypeOf(new Object[] { 1, 2 }));
            Assert.AreEqual(PrimitiveType.Sequence, converter.TypeOf(new Int32[] { 1, 2 }));
            Assert.AreEqual(PrimitiveType.Object, converter.TypeOf(this));
        }

        [Test]
        public void TestCaseConvertToBoolean()
        {
            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

            Assert.AreEqual(false, converter.ConvertToBoolean(default(Byte)));
            Assert.AreEqual(true, converter.ConvertToBoolean((Byte)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(SByte)));
            Assert.AreEqual(true, converter.ConvertToBoolean((SByte)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(Int16)));
            Assert.AreEqual(true, converter.ConvertToBoolean((Int16)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(UInt16)));
            Assert.AreEqual(true, converter.ConvertToBoolean((UInt16)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(Int32)));
            Assert.AreEqual(true, converter.ConvertToBoolean((Int32)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(UInt32)));
            Assert.AreEqual(true, converter.ConvertToBoolean((UInt32)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(Int64)));
            Assert.AreEqual(true, converter.ConvertToBoolean((Int64)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(UInt64)));
            Assert.AreEqual(true, converter.ConvertToBoolean((UInt64)10));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(Single)));
            Assert.AreEqual(true, converter.ConvertToBoolean((Single)0.1));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(Double)));
            Assert.AreEqual(true, converter.ConvertToBoolean((Double)0.1));
            Assert.AreEqual(false, converter.ConvertToBoolean(default(Decimal)));
            Assert.AreEqual(true, converter.ConvertToBoolean((Decimal)0.1));
            Assert.AreEqual(false, converter.ConvertToBoolean(false));
            Assert.AreEqual(true, converter.ConvertToBoolean(true));
            Assert.AreEqual(false, converter.ConvertToBoolean(String.Empty));
            Assert.AreEqual(true, converter.ConvertToBoolean("anything"));
            Assert.AreEqual(false, converter.ConvertToBoolean(null));
            Assert.AreEqual(true, converter.ConvertToBoolean(this));
            Assert.AreEqual(true, converter.ConvertToBoolean(new String[0]));
            Assert.AreEqual(true, converter.ConvertToBoolean(new Int32[] { 1 } ));
            Assert.AreEqual(true, converter.ConvertToBoolean(new Object[] { "hello", 1 }));
        }

        [Test]
        public void TestCaseConvertToString()
        {
            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

            Assert.AreEqual("10", converter.ConvertToString((Byte)10));
            Assert.AreEqual("10", converter.ConvertToString((SByte)10));
            Assert.AreEqual("10", converter.ConvertToString((Int16)10));
            Assert.AreEqual("10", converter.ConvertToString((UInt16)10));
            Assert.AreEqual("10", converter.ConvertToString((Int32)10));
            Assert.AreEqual("10", converter.ConvertToString((UInt32)10));
            Assert.AreEqual("10", converter.ConvertToString((Int64)10));
            Assert.AreEqual("10", converter.ConvertToString((UInt64)10));
            Assert.AreEqual("10.3299999237061", converter.ConvertToString((Single)10.33));
            Assert.AreEqual("10.33", converter.ConvertToString((Double)10.33));
            Assert.AreEqual("10.33", converter.ConvertToString((Decimal)10.33));
            Assert.AreEqual("False", converter.ConvertToString(false));
            Assert.AreEqual("True", converter.ConvertToString(true));
            Assert.AreEqual(String.Empty, converter.ConvertToString(String.Empty));
            Assert.AreEqual("anything", converter.ConvertToString("anything"));
            Assert.AreEqual("!undefined!", converter.ConvertToString(null));
            Assert.AreEqual(this.ToString(), converter.ConvertToString(this));
            Assert.AreEqual(String.Empty, converter.ConvertToString(new Object[0]));
            Assert.AreEqual("1", converter.ConvertToString(new Int32[] { 1 }));
            Assert.AreEqual("hello,1", converter.ConvertToString(new Object[] { "hello", 1 }));
            Assert.AreEqual("1,2,3", converter.ConvertToString(new Object[] { 1, new Object[] { 2, 3 } }));
        }

        [Test]
        public void TestCaseConvertToNumber()
        {
            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

            Assert.AreEqual(10, converter.ConvertToNumber((Byte)10));
            Assert.AreEqual(-10, converter.ConvertToNumber((SByte)(-10)));
            Assert.AreEqual(-10, converter.ConvertToNumber((Int16)(-10)));
            Assert.AreEqual(10, converter.ConvertToNumber((UInt16)10));
            Assert.AreEqual(-10, converter.ConvertToNumber((Int32)(-10)));
            Assert.AreEqual(10, converter.ConvertToNumber((UInt32)10));
            Assert.AreEqual(-10, converter.ConvertToNumber((Int64)(-10)));
            Assert.AreEqual(10, converter.ConvertToNumber((UInt64)10));
            Assert.AreEqual(10.329999923706055, converter.ConvertToNumber((Single)10.33));
            Assert.AreEqual(10.33, converter.ConvertToNumber((Double)10.33));
            Assert.AreEqual(10.33, converter.ConvertToNumber((Decimal)10.33));
            Assert.AreEqual(0, converter.ConvertToNumber(false));
            Assert.AreEqual(1, converter.ConvertToNumber(true));
            Assert.AreEqual(0, converter.ConvertToNumber(String.Empty));
            Assert.AreEqual(Double.NaN, converter.ConvertToNumber("anything"));
            Assert.AreEqual(-10.33, converter.ConvertToNumber("-10.33"));
            Assert.AreEqual(Double.NaN, converter.ConvertToNumber(null));
            Assert.AreEqual(Double.NaN, converter.ConvertToNumber(this));
            Assert.AreEqual(0, converter.ConvertToNumber(new Object[0]));
            Assert.AreEqual(12, converter.ConvertToNumber(new Int32[] { 12 }));
            Assert.AreEqual(Double.NaN, converter.ConvertToNumber(new Int32[] { 1, 2 }));
            Assert.AreEqual(0, converter.ConvertToNumber(new Object[] { "" }));
            Assert.AreEqual(-77, converter.ConvertToNumber(new Object[] { "-77" }));
            Assert.AreEqual(Double.NaN, converter.ConvertToNumber(new Object[] { 1, new Object[] { 2, 3 } }));
        }

        [Test]
        public void TestCaseConvertToInteger()
        {
            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

            Assert.AreEqual(1, converter.ConvertToInteger((Byte)1));
            Assert.AreEqual(-1, converter.ConvertToInteger((SByte)(-1)));
            Assert.AreEqual(-1, converter.ConvertToInteger((Int16)(-1)));
            Assert.AreEqual(1, converter.ConvertToInteger((UInt16)1));
            Assert.AreEqual(-1, converter.ConvertToInteger((Int32)(-1)));
            Assert.AreEqual(1, converter.ConvertToInteger((UInt32)1));
            Assert.AreEqual(1, converter.ConvertToInteger(1L));
            Assert.AreEqual(-1, converter.ConvertToInteger(-1L));
            Assert.AreEqual(-10, converter.ConvertToInteger((Single)(-10.83)));
            Assert.AreEqual(-10, converter.ConvertToInteger((Double)(-10.83)));
            Assert.AreEqual(-10, converter.ConvertToInteger((Decimal)(-10.83)));
            Assert.AreEqual(0, converter.ConvertToInteger(false));
            Assert.AreEqual(1, converter.ConvertToInteger(true));
            Assert.AreEqual(0, converter.ConvertToInteger(String.Empty));
            Assert.AreEqual(0, converter.ConvertToInteger("anything"));
            Assert.AreEqual(-10, converter.ConvertToInteger("-10.88"));
            Assert.AreEqual(0, converter.ConvertToInteger(null));
            Assert.AreEqual(0, converter.ConvertToInteger(this));
            Assert.AreEqual(0, converter.ConvertToInteger(new Object[0]));
            Assert.AreEqual(12, converter.ConvertToInteger(new Int32[] { 12 }));
            Assert.AreEqual(0, converter.ConvertToInteger(new Int32[] { 1, 2 }));
            Assert.AreEqual(0, converter.ConvertToInteger(new Object[] { "" }));
            Assert.AreEqual(-77, converter.ConvertToInteger(new Object[] { "-77" }));
            Assert.AreEqual(0, converter.ConvertToInteger(new Object[] { 1, new Object[] { 2, 3 } }));
        }

        [Test]
        public void TestCaseConvertToSequence()
        {
            var converter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

            /* Undefined */
            Assert.IsNull(converter.ConvertToSequence(null));

            /* Single element */
            var sequence = converter.ConvertToSequence(-10);
            Assert.IsInstanceOf<IEnumerable<Object>>(sequence);
            var consolidated = (sequence as IEnumerable<Object>).ToArray();
            Assert.AreEqual(1, consolidated.Length);
            Assert.AreEqual(-10, consolidated[0]);

            /* Two objects */
            sequence = converter.ConvertToSequence(new String[] { "Hello", "World" });
            Assert.IsInstanceOf<IEnumerable<Object>>(sequence);
            consolidated = (sequence as IEnumerable<Object>).ToArray();
            Assert.AreEqual(2, consolidated.Length);
            Assert.AreEqual("Hello", consolidated[0]);
            Assert.AreEqual("World", consolidated[1]);

            /* String */
            sequence = converter.ConvertToSequence("STR");
            Assert.IsInstanceOf<IEnumerable<Object>>(sequence);
            consolidated = (sequence as IEnumerable<Object>).ToArray();
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

