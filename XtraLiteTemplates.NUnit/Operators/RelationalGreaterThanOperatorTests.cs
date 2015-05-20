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

namespace XtraLiteTemplates.NUnit.Operators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions.Operators;

    [TestFixture]
    public class RelationalGreaterThanOperatorTests : OperatorTestsBase
    {
       
        [Test]
        public void TestCaseStandardOperatorRelationalGreaterThan()
        {
            ExpectArgumentNullException("symbol", () => new RelationalGreaterThanOperator(null, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new RelationalGreaterThanOperator(String.Empty, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentNullException("stringComparer", () => new RelationalGreaterThanOperator("operator", null, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalGreaterThanOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalGreaterThanOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new RelationalGreaterThanOperator(null, CreateTypeConverter()));

            var standard = new RelationalGreaterThanOperator(CreateTypeConverter());
            Assert.AreEqual(">", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new RelationalGreaterThanOperator("operator", StringComparer.Ordinal, CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(6, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, op.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, false);
            AssertEvaluation<Int64, Boolean>(op, 0, Int64.MinValue, true);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, false);
            AssertEvaluation<Double, Boolean>(op, 3.34, 3.33, true);
        }

    }
}

