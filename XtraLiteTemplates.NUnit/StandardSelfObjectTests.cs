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
using NUnit.Framework;

namespace XtraLiteTemplates.NUnit
{
    using System;
    using XtraLiteTemplates.Dialects.Standard;

    [TestFixture]
    public class StandardSelfObjectTests : TestBase
    {
        [Test]
        public void TestCaseConstruction()
        {
            ExpectArgumentNullException("typeConverter", () => new StandardSelfObject(null));

            var selfObject = new StandardSelfObject(TypeConverter);
            Assert.AreEqual(selfObject.TypeConverter, TypeConverter);
        }

        [Test]
        public void TestCaseBooleanMethod()
        {
            var selfObject = new StandardSelfObject(TypeConverter);

            Assert.AreEqual(false, selfObject.Boolean(default(Byte)));
            Assert.AreEqual(true, selfObject.Boolean((Byte)10));
            Assert.AreEqual(false, selfObject.Boolean(default(SByte)));
            Assert.AreEqual(true, selfObject.Boolean((SByte)10));
            Assert.AreEqual(false, selfObject.Boolean(default(Int16)));
            Assert.AreEqual(true, selfObject.Boolean((Int16)10));
            Assert.AreEqual(false, selfObject.Boolean(default(UInt16)));
            Assert.AreEqual(true, selfObject.Boolean((UInt16)10));
            Assert.AreEqual(false, selfObject.Boolean(default(int)));
            Assert.AreEqual(true, selfObject.Boolean((int)10));
            Assert.AreEqual(false, selfObject.Boolean(default(UInt32)));
            Assert.AreEqual(true, selfObject.Boolean((UInt32)10));
            Assert.AreEqual(false, selfObject.Boolean(default(Int64)));
            Assert.AreEqual(true, selfObject.Boolean((Int64)10));
            Assert.AreEqual(false, selfObject.Boolean(default(UInt64)));
            Assert.AreEqual(true, selfObject.Boolean((UInt64)10));
            Assert.AreEqual(false, selfObject.Boolean(default(Single)));
            Assert.AreEqual(true, selfObject.Boolean((Single)0.1));
            Assert.AreEqual(false, selfObject.Boolean(default(Double)));
            Assert.AreEqual(true, selfObject.Boolean((Double)0.1));
            Assert.AreEqual(false, selfObject.Boolean(default(Decimal)));
            Assert.AreEqual(true, selfObject.Boolean((Decimal)0.1));
            Assert.AreEqual(false, selfObject.Boolean(false));
            Assert.AreEqual(true, selfObject.Boolean(true));
            Assert.AreEqual(false, selfObject.Boolean(string.Empty));
            Assert.AreEqual(true, selfObject.Boolean("anything"));
            Assert.AreEqual(false, selfObject.Boolean(null));
            Assert.AreEqual(true, selfObject.Boolean(this));
        }

        [Test]
        public void TestCaseStringMethod()
        {
            var selfObject = new StandardSelfObject(TypeConverter);

            Assert.AreEqual("10", selfObject.String((Byte)10));
            Assert.AreEqual("10", selfObject.String((SByte)10));
            Assert.AreEqual("10", selfObject.String((Int16)10));
            Assert.AreEqual("10", selfObject.String((UInt16)10));
            Assert.AreEqual("10", selfObject.String((int)10));
            Assert.AreEqual("10", selfObject.String((UInt32)10));
            Assert.AreEqual("10", selfObject.String((Int64)10));
            Assert.AreEqual("10", selfObject.String((UInt64)10));
            Assert.AreEqual("10.3299999237061", selfObject.String((Single)10.33));
            Assert.AreEqual("10.33", selfObject.String((Double)10.33));
            Assert.AreEqual("10.33", selfObject.String((Decimal)10.33));
            Assert.AreEqual("False", selfObject.String(false));
            Assert.AreEqual("True", selfObject.String(true));
            Assert.AreEqual(string.Empty, selfObject.String(string.Empty));
            Assert.AreEqual("anything", selfObject.String("anything"));
            Assert.AreEqual("!undefined!", selfObject.String(null));
            Assert.AreEqual(ToString(), selfObject.String(this));
        }

        [Test]
        public void TestCaseNumberMethod()
        {
            var selfObject = new StandardSelfObject(TypeConverter);

            Assert.AreEqual(10, selfObject.Number((Byte)10));
            Assert.AreEqual(-10, selfObject.Number((SByte)(-10)));
            Assert.AreEqual(-10, selfObject.Number((Int16)(-10)));
            Assert.AreEqual(10, selfObject.Number((UInt16)10));
            Assert.AreEqual(-10, selfObject.Number((int)(-10)));
            Assert.AreEqual(10, selfObject.Number((UInt32)10));
            Assert.AreEqual(-10, selfObject.Number((Int64)(-10)));
            Assert.AreEqual(10, selfObject.Number((UInt64)10));
            Assert.AreEqual(10.329999923706055, selfObject.Number((Single)10.33));
            Assert.AreEqual(10.33, selfObject.Number((Double)10.33));
            Assert.AreEqual(10.33, selfObject.Number((Decimal)10.33));
            Assert.AreEqual(0, selfObject.Number(false));
            Assert.AreEqual(1, selfObject.Number(true));
            Assert.AreEqual(0, selfObject.Number(string.Empty));
            Assert.AreEqual(double.NaN, selfObject.Number("anything"));
            Assert.AreEqual(-10.33, selfObject.Number("-10.33"));
            Assert.AreEqual(double.NaN, selfObject.Number(null));
            Assert.AreEqual(double.NaN, selfObject.Number(this));
        }
    }
}

