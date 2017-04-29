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
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.NUnit.Inside;

    [TestFixture]
    public class ExpressionFlowSymbolsTests : TestBase
    { 
        [Test]
        public void TestCaseContruction_1()
        {
            ExpectArgumentNullException("separatorSymbol", () => new ExpressionFlowSymbols(null, ".", "(", ")"));
            ExpectArgumentNullException("memberAccessSymbol", () => new ExpressionFlowSymbols(",", null, "(", ")"));
            ExpectArgumentNullException("groupOpenSymbol", () => new ExpressionFlowSymbols(",", ".", null, ")"));
            ExpectArgumentNullException("groupCloseSymbol", () => new ExpressionFlowSymbols(",", ".", "(", null));

            ExpectArgumentEmptyException("separatorSymbol", () => new ExpressionFlowSymbols(String.Empty, ".", "(", ")"));
            ExpectArgumentEmptyException("memberAccessSymbol", () => new ExpressionFlowSymbols(",", String.Empty, "(", ")"));
            ExpectArgumentEmptyException("groupOpenSymbol", () => new ExpressionFlowSymbols(",", ".", String.Empty, ")"));
            ExpectArgumentEmptyException("groupCloseSymbol", () => new ExpressionFlowSymbols(",", ".", "(", String.Empty));
        }

        [Test]
        public void TestCaseContruction_2()
        {
            ExpectArgumentsEqualException("separatorSymbol", "memberAccessSymbol", () => new ExpressionFlowSymbols("A", "A", "C", "D"));
            ExpectArgumentsEqualException("separatorSymbol", "groupOpenSymbol", () => new ExpressionFlowSymbols("A", "B", "A", "D"));
            ExpectArgumentsEqualException("separatorSymbol", "groupCloseSymbol", () => new ExpressionFlowSymbols("A", "B", "C", "A"));
            ExpectArgumentsEqualException("memberAccessSymbol", "groupOpenSymbol", () => new ExpressionFlowSymbols("A", "B", "B", "D"));
            ExpectArgumentsEqualException("memberAccessSymbol", "groupCloseSymbol", () => new ExpressionFlowSymbols("A", "B", "C", "B"));
            ExpectArgumentsEqualException("groupOpenSymbol", "groupCloseSymbol", () => new ExpressionFlowSymbols("A", "B", "C", "C"));
        }

        [Test]
        public void TestCaseContruction_3()
        {
            var flowSymbols = new ExpressionFlowSymbols("A", "B", "C", "D");

            Assert.AreEqual("A", flowSymbols.Separator);
            Assert.AreEqual("B", flowSymbols.MemberAccess);
            Assert.AreEqual("C", flowSymbols.GroupOpen);
            Assert.AreEqual("D", flowSymbols.GroupClose);
        }

        [Test]
        public void TestCaseContructionDefault()
        {
            var flowSymbols = ExpressionFlowSymbols.Default;

            Assert.AreEqual(",", flowSymbols.Separator);
            Assert.AreEqual(".", flowSymbols.MemberAccess);
            Assert.AreEqual("(", flowSymbols.GroupOpen);
            Assert.AreEqual(")", flowSymbols.GroupClose);
        }
    }
}

