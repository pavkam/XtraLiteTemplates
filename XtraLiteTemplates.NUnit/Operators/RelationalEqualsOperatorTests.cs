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

namespace XtraLiteTemplates.NUnit.Operators
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions.Operators;

    [TestFixture]
    public class RelationalEqualsOperatorTests : OperatorTestsBase
    {
      
        [Test]
        public void TestCaseStandardOperatorRelationalEquals()
        {
            ExpectArgumentNullException("symbol", () => new RelationalEqualsOperator(null, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentEmptyException("symbol", () => new RelationalEqualsOperator(String.Empty, StringComparer.Ordinal, CreateTypeConverter()));
            ExpectArgumentNullException("stringComparer", () => new RelationalEqualsOperator("operator", null, CreateTypeConverter()));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalEqualsOperator("operator", StringComparer.Ordinal, null));
            ExpectArgumentEmptyException("typeConverter", () => new RelationalEqualsOperator(null));
            ExpectArgumentEmptyException("stringComparer", () => new RelationalEqualsOperator(null, CreateTypeConverter()));

            var standard = new RelationalEqualsOperator(CreateTypeConverter());
            Assert.AreEqual("==", standard.Symbol);
            Assert.AreEqual(StringComparer.CurrentCulture, standard.StringComparer);

            var op = new RelationalEqualsOperator("operator", StringComparer.Ordinal, CreateTypeConverter());
            Assert.AreEqual("operator", op.Symbol);
            Assert.AreEqual(7, op.Precedence);
            Assert.AreEqual(Associativity.LeftToRight, op.Associativity);
            Assert.AreEqual(false, op.ExpectLhsIdentifier);
            Assert.AreEqual(false, op.ExpectRhsIdentifier);
            Assert.AreEqual(StringComparer.Ordinal, op.StringComparer);

            AssertEvaluation<Int64, Boolean>(op, Int64.MaxValue, Int64.MaxValue, true);
            AssertEvaluation<Int64, Boolean>(op, Int64.MinValue, 0, false);

            AssertEvaluation<Double, Boolean>(op, -0.5, -0.5, true);
            AssertEvaluation<Double, Boolean>(op, 3.33, 3.34, false);

            AssertEvaluation<String, Boolean>(op, "Hello", "Hello", true);
            AssertEvaluation<String, Boolean>(op, "world", "WORLD", false);

            AssertEvaluation<Boolean, Boolean>(op, false, false, true);
            AssertEvaluation<Boolean, Boolean>(op, true, false, false);
        }

    }
}

