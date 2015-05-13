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
//     * Neither the name of the [ORGANIZATION] nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
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
    using System.IO;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Expressions.Operators.Standard;
    using XtraLiteTemplates.NUnit.Inside;
    using XtraLiteTemplates.ObjectModel;
    using XtraLiteTemplates.Parsing;

    [TestFixture]
    public class InterpreterTests : TestBase
    {
        [Test]
        public void TestCaseInterpreterConstructionExceptions()
        {
            ExpectArgumentNullException("tokenizer", () => new Interpreter((ITokenizer)null, StringComparer.Ordinal));
            ExpectArgumentNullException("comparer", () => new Interpreter(new Tokenizer("irrelevant"), null));

            ExpectArgumentNullException("reader", () => new Interpreter((TextReader)null, StringComparer.Ordinal));
            ExpectArgumentNullException("comparer", () => new Interpreter(new StringReader("irrelevant"), null));

            ExpectArgumentNullException("text", () => new Interpreter((String)null, StringComparer.Ordinal));
            ExpectArgumentNullException("comparer", () => new Interpreter("irrelevant", null));
        }

        [Test]
        public void TestCaseInterpreterConstruction()
        {
            var interpreter = new Interpreter(new Tokenizer("irrelevant"), StringComparer.CurrentCulture);
            Assert.AreEqual(StringComparer.CurrentCulture, interpreter.Comparer);

            interpreter = new Interpreter(new StringReader("irrelevant"), StringComparer.CurrentCulture);
            Assert.AreEqual(StringComparer.CurrentCulture, interpreter.Comparer);

            interpreter = new Interpreter("irrelevant", StringComparer.CurrentCulture);
            Assert.AreEqual(StringComparer.CurrentCulture, interpreter.Comparer);
        }

        [Test]
        public void TestCaseInterpreterRegistrationAndExceptions()
        {
            var interpreter = new Interpreter("irrelevant", StringComparer.CurrentCulture);

            ExpectArgumentNullException("directive", () => interpreter.RegisterDirective(null));
            ExpectArgumentNullException("operator", () => interpreter.RegisterOperator(null));

            interpreter.RegisterOperator(new NeutralOperator("A"));
            ExpectOperatorAlreadyRegisteredException("A", () => interpreter.RegisterOperator(new NeutralOperator("A")));
            interpreter.RegisterOperator(new SumOperator("A"));
            ExpectOperatorAlreadyRegisteredException("A", () => interpreter.RegisterOperator(new SumOperator("A")));
            interpreter.RegisterOperator(new SubscriptOperator("O", "C"));
            ExpectOperatorAlreadyRegisteredException("OC", () => interpreter.RegisterOperator(new SubscriptOperator("O", "C")));
            ExpectOperatorAlreadyRegisteredException("AM", () => interpreter.RegisterOperator(new SubscriptOperator("A", "M")));

            interpreter.RegisterOperator(new SumOperator("L"));
            ExpectOperatorAlreadyRegisteredException("KL", () => interpreter.RegisterOperator(new SubscriptOperator("K", "L")));
            interpreter.RegisterOperator(new NeutralOperator("L"));

            interpreter.RegisterOperator(new SubscriptOperator("W", "V"));
            ExpectOperatorAlreadyRegisteredException("W", () => interpreter.RegisterOperator(new NeutralOperator("W")));
            ExpectOperatorAlreadyRegisteredException("V", () => interpreter.RegisterOperator(new SumOperator("V")));
            interpreter.RegisterOperator(new NeutralOperator("V"));
            interpreter.RegisterOperator(new SumOperator("W"));

            var d1 = new RippedOpenDirective(Tag.Parse("HELLO"));
            var d2 = new RippedOpenDirective(Tag.Parse("WORLD"));
            interpreter.RegisterDirective(d1);
            interpreter.RegisterDirective(d2);
            interpreter.RegisterDirective(d1);
        }
    }
}

