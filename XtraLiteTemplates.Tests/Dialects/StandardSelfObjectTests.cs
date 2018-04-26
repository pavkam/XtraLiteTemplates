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

namespace XtraLiteTemplates.Tests.Dialects
{
    using System.Diagnostics.CodeAnalysis;
    using global::NUnit.Framework;
    using XtraLiteTemplates.Dialects.Standard;
    using XtraLiteTemplates.Introspection;

    [TestFixture]
    public class StandardSelfObjectTests : TestBase
    {
        protected virtual StandardSelfObject CreateSelfObject(IPrimitiveTypeConverter typeConverter)
        {
            return new StandardSelfObject(typeConverter);
        }

        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseConstruction()
        {
            ExpectArgumentNullException("typeConverter", () => CreateSelfObject(null));

            var selfObject = CreateSelfObject(TypeConverter);
            Assert.AreEqual(selfObject.TypeConverter, TypeConverter);
        }

        [Test]
        public void TestCaseBooleanMethod()
        {
            var selfObject = CreateSelfObject(TypeConverter);

            Assert.AreEqual(false, selfObject.Boolean(default(byte)));
            Assert.AreEqual(true, selfObject.Boolean((byte)10));
            Assert.AreEqual(false, selfObject.Boolean(default(sbyte)));
            Assert.AreEqual(true, selfObject.Boolean((sbyte)10));
            Assert.AreEqual(false, selfObject.Boolean(default(short)));
            Assert.AreEqual(true, selfObject.Boolean((short)10));
            Assert.AreEqual(false, selfObject.Boolean(default(ushort)));
            Assert.AreEqual(true, selfObject.Boolean((ushort)10));
            Assert.AreEqual(false, selfObject.Boolean(default(int)));
            Assert.AreEqual(true, selfObject.Boolean(10));
            Assert.AreEqual(false, selfObject.Boolean(default(uint)));
            Assert.AreEqual(true, selfObject.Boolean((uint)10));
            Assert.AreEqual(false, selfObject.Boolean(default(long)));
            Assert.AreEqual(true, selfObject.Boolean((long)10));
            Assert.AreEqual(false, selfObject.Boolean(default(ulong)));
            Assert.AreEqual(true, selfObject.Boolean((ulong)10));
            Assert.AreEqual(false, selfObject.Boolean(default(float)));
            Assert.AreEqual(true, selfObject.Boolean((float)0.1));
            Assert.AreEqual(false, selfObject.Boolean(default(double)));
            Assert.AreEqual(true, selfObject.Boolean(0.1));
            Assert.AreEqual(false, selfObject.Boolean(default(decimal)));
            Assert.AreEqual(true, selfObject.Boolean((decimal)0.1));
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
            var selfObject = CreateSelfObject(TypeConverter);

            Assert.AreEqual("10", selfObject.String((byte)10));
            Assert.AreEqual("10", selfObject.String((sbyte)10));
            Assert.AreEqual("10", selfObject.String((short)10));
            Assert.AreEqual("10", selfObject.String((ushort)10));
            Assert.AreEqual("10", selfObject.String(10));
            Assert.AreEqual("10", selfObject.String((uint)10));
            Assert.AreEqual("10", selfObject.String((long)10));
            Assert.AreEqual("10", selfObject.String((ulong)10));
            Assert.AreEqual("10.3299999237061", selfObject.String((float)10.33));
            Assert.AreEqual("10.33", selfObject.String(10.33));
            Assert.AreEqual("10.33", selfObject.String((decimal)10.33));
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
            var selfObject = CreateSelfObject(TypeConverter);

            Assert.AreEqual(10, selfObject.Number((byte)10));
            Assert.AreEqual(-10, selfObject.Number((sbyte)(-10)));
            Assert.AreEqual(-10, selfObject.Number((short)(-10)));
            Assert.AreEqual(10, selfObject.Number((ushort)10));
            Assert.AreEqual(-10, selfObject.Number(-10));
            Assert.AreEqual(10, selfObject.Number((uint)10));
            Assert.AreEqual(-10, selfObject.Number((long)(-10)));
            Assert.AreEqual(10, selfObject.Number((ulong)10));
            Assert.AreEqual(10.329999923706055, selfObject.Number((float)10.33));
            Assert.AreEqual(10.33, selfObject.Number(10.33));
            Assert.AreEqual(10.33, selfObject.Number((decimal)10.33));
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

