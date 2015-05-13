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
    using XtraLiteTemplates.ObjectModel.Directives;
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

        [Test]
        public void TestCaseInterpreterConstructNoMatchingTag1()
        {
            var directive = new RippedOpenDirective(
                Tag.Parse("OPEN"),
                Tag.Parse("CLOSE"));

            var interpreter = new Interpreter("{OPEN}text goes here", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive }, 0, () => interpreter.Construct());
        }

        [Test]
        public void TestCaseInterpreterConstructNoMatchingTag2()
        {
            var directive = new RippedOpenDirective(
                Tag.Parse("OPEN"),
                Tag.Parse("INSIDE"),
                Tag.Parse("CLOSE"));

            var interpreter = new Interpreter("{OPEN}text goes{INSIDE}here", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive }, 0, () => interpreter.Construct());
        }

        [Test]
        public void TestCaseInterpreterConstructNoMatchingTag3()
        {
            var directive1 = new RippedOpenDirective(
                Tag.Parse("T1"),
                Tag.Parse("T2"));
            var directive2 = new RippedOpenDirective(
                Tag.Parse("T1"),
                Tag.Parse("T3"));

            var interpreter = new Interpreter("{T1}multiple choice", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive1, directive2 }, 0, () => interpreter.Construct());
        }

        [Test]
        public void TestCaseInterpreterConstructNoMatchingTag4()
        {
            var directive1 = new RippedOpenDirective(
                Tag.Parse("T1"),
                Tag.Parse("T2"));
            var directive2 = new RippedOpenDirective(
                Tag.Parse("T1"),
                Tag.Parse("T3"));

            var interpreter = new Interpreter("{T1}{T1}{T3}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive1, directive2 }, 0, () => interpreter.Construct());
        }

        [Test]
        public void TestCaseInterpreterConstructNoMatchingTag5()
        {
            var directive1 = new RippedOpenDirective(
                Tag.Parse("T1"),
                Tag.Parse("T"));
            var directive2 = new RippedOpenDirective(
                Tag.Parse("T2"),
                Tag.Parse("T"));

            var interpreter = new Interpreter("{T1}{T2}{T}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive1 }, 0, () => interpreter.Construct());
        }

        [Test]
        public void TestCaseInterpreterConstructNoMatchingTag6()
        {
            var directive1 = new RippedOpenDirective(
                Tag.Parse("T"),
                Tag.Parse("T1"));
            var directive2 = new RippedOpenDirective(
                Tag.Parse("T"),
                Tag.Parse("T2"));

            var interpreter = new Interpreter("{T}{T}{T2}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive1, directive2 }, 0, () => interpreter.Construct());
        }

        [Test]
        public void TestCaseInterpreterConstructUnexpectedTag1()
        {
            var directive = new RippedOpenDirective(
                Tag.Parse("A"),
                Tag.Parse("B"));

            var interpreter = new Interpreter("{B}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive);

            ExpectUnexpectedTagException("B", 0, () => interpreter.Construct());
        }

        [Test]
        public void TestCaseInterpreterConstructUnexpectedTag2()
        {
            var directive = new RippedOpenDirective(
                Tag.Parse("A"),
                Tag.Parse("B"),
                Tag.Parse("C"));

            var interpreter = new Interpreter("{A}{C}{B}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive);

            ExpectUnexpectedTagException("C", 3, () => interpreter.Construct());
        }

        [Test]
        public void TestCaseInterpreterConstructUnexpectedTag3()
        {
            var directive1 = new RippedOpenDirective(
                Tag.Parse("A"),
                Tag.Parse("B"));
            var directive2 = new RippedOpenDirective(
                Tag.Parse("C"),
                Tag.Parse("D"));

            var interpreter = new Interpreter("{A}..{D}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            ExpectUnexpectedTagException("D", 5, () => interpreter.Construct());
        }

        [Test]
        public void TestCaseInterpreterConstructSelection1()
        {
            var directive1 = new RippedOpenDirective(
                Tag.Parse("A"),
                Tag.Parse("B"));
            var directive2 = new RippedOpenDirective(
                Tag.Parse("C"),
                Tag.Parse("D"));

            var interpreter = new Interpreter("{A}1{C}2{A}3{B}4{D}5{B}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            var evaluable = interpreter.Construct();
            var repr = evaluable.ToString();

            Assert.AreEqual("(({A}1({C}2({A}3{B})4{D})5{B}))", repr);
        }

        [Test]
        public void TestCaseInterpreterConstructSelection2()
        {
            var directive1 = new RippedOpenDirective(Tag.Parse("MATCH ME ?"));
            var directive2 = new RippedOpenDirective(Tag.Parse("MATCH ME $"));

            var interpreter1 = new Interpreter("{MATCH ME identifier}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);
            var interpreter2 = new Interpreter("{MATCH ME identifier}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive2).RegisterDirective(directive1);

            var repr1 = interpreter1.Construct().ToString();
            var repr2 = interpreter2.Construct().ToString();

            Assert.AreEqual("(({MATCH ME identifier}))", repr1);
            Assert.AreEqual("(({MATCH ME identifier}))", repr2);
        }

        [Test]
        public void TestCaseInterpreterConstructSelection3()
        {
            var directive1 = new RippedOpenDirective(
                Tag.Parse("START"),
                Tag.Parse("MID"),
                Tag.Parse("END"));
            var directive2 = new RippedOpenDirective(
                Tag.Parse("START"),
                Tag.Parse("MID"),
                Tag.Parse("OTHER END"));

            var repr = new Interpreter("{START}{MID}{OTHER END}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1)
                .RegisterDirective(directive2)
                .Construct()
                .ToString();

            Assert.AreEqual("(({START}{MID}{OTHER END}))", repr);
        }

        [Test]
        public void TestCaseInterpreterConstructSelection4()
        {
            var directive1 = new RippedOpenDirective(
                Tag.Parse("IF ?"),
                Tag.Parse("END"));
            var directive2 = new RippedOpenDirective(
                Tag.Parse("IF ?"),
                Tag.Parse("ELSE"),
                Tag.Parse("END"));

            var repr = new Interpreter("{IF A}1{IF B}2{ELSE}3{END}4{IF A}5{END}6{END}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1)
                .RegisterDirective(directive2)
                .Construct()
                .ToString();

            Assert.AreEqual("(({IF A}1({IF B}2{ELSE}3{END})4({IF A}5{END})6{END}))", repr);
        }
    }
}

