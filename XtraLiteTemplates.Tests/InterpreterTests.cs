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
    using System;
    using System.Diagnostics.CodeAnalysis;
    using global::NUnit.Framework;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Tests.Inside;
    using XtraLiteTemplates.Parsing;

    [TestFixture]
    [SuppressMessage("ReSharper", "IdentifierTypo")]
    [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
    public class InterpreterTests : TestBase
    {
        [Test]
        [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
        public void TestCaseInterpreterConstructionExceptions()
        {
            ExpectArgumentNullException("tokenizer", () => new Interpreter(null, ExpressionFlowSymbols.Default, StringComparer.Ordinal));
            ExpectArgumentNullException("expressionFlowSymbols", () => new Interpreter(new Tokenizer("irrelevant"), null, StringComparer.Ordinal));
            ExpectArgumentNullException("comparer", () => new Interpreter(new Tokenizer("irrelevant"), ExpressionFlowSymbols.Default, null));
        }

        [Test]
        public void TestCaseInterpreterConstruction()
        {
            var interpreter = new Interpreter(new Tokenizer("irrelevant"), ExpressionFlowSymbols.Default, StringComparer.CurrentCulture);

            Assert.AreEqual(StringComparer.CurrentCulture, interpreter.Comparer);
        }

        [Test]
        public void TestCaseInterpreterRegistrationAndExceptions()
        {
            var interpreter = new Interpreter(new Tokenizer("irrelevant"), ExpressionFlowSymbols.Default, StringComparer.CurrentCulture);

            ExpectArgumentNullException("directive", () => interpreter.RegisterDirective(null));
            ExpectArgumentNullException("operator", () => interpreter.RegisterOperator(null));

            interpreter.RegisterOperator(new ArithmeticNeutralOperator("A", TypeConverter));
            ExpectOperatorAlreadyRegisteredException("A", () => interpreter.RegisterOperator(new ArithmeticNeutralOperator("A", TypeConverter)));
            interpreter.RegisterOperator(new ArithmeticSumOperator("A", TypeConverter));
            ExpectOperatorAlreadyRegisteredException("A", () => interpreter.RegisterOperator(new ArithmeticSumOperator("A", TypeConverter)));

            ExpectOperatorAlreadyRegisteredException("(", () => interpreter.RegisterOperator(new ArithmeticNeutralOperator("(", TypeConverter)));
            interpreter.RegisterOperator(new ArithmeticNeutralOperator(")", TypeConverter));
            interpreter.RegisterOperator(new ArithmeticNeutralOperator(".", TypeConverter));
            interpreter.RegisterOperator(new ArithmeticNeutralOperator(",", TypeConverter));

            interpreter.RegisterOperator(new ArithmeticSumOperator("(", TypeConverter));
            ExpectOperatorAlreadyRegisteredException(")", () => interpreter.RegisterOperator(new ArithmeticSumOperator(")", TypeConverter)));
            ExpectOperatorAlreadyRegisteredException(".", () => interpreter.RegisterOperator(new ArithmeticSumOperator(".", TypeConverter)));
            ExpectOperatorAlreadyRegisteredException(",", () => interpreter.RegisterOperator(new ArithmeticSumOperator(",", TypeConverter)));

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

            var interpreter = new Interpreter(new Tokenizer("{OPEN}text goes here"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive }, 0, () => interpreter.Compile());
        }

        [Test]
        public void TestCaseInterpreterConstructNoMatchingTag2()
        {
            var directive = new RippedOpenDirective(
                Tag.Parse("OPEN"),
                Tag.Parse("INSIDE"),
                Tag.Parse("CLOSE"));

            var interpreter = new Interpreter(new Tokenizer("{OPEN}text goes{INSIDE}here"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive }, 0, () => interpreter.Compile());
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

            var interpreter = new Interpreter(new Tokenizer("{T1}multiple choice"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive1, directive2 }, 0, () => interpreter.Compile());
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

            var interpreter = new Interpreter(new Tokenizer("{T1}{T1}{T3}"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive1, directive2 }, 0, () => interpreter.Compile());
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

            var interpreter = new Interpreter(new Tokenizer("{T1}{T2}{T}"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive1 }, 0, () => interpreter.Compile());
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

            var interpreter = new Interpreter(new Tokenizer("{T}{T}{T2}"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            ExpectUnmatchedDirectiveTagException(new Directive[] { directive1, directive2 }, 0, () => interpreter.Compile());
        }

        [Test]
        public void TestCaseInterpreterConstructUnexpectedTag1()
        {
            var directive = new RippedOpenDirective(
                Tag.Parse("A"),
                Tag.Parse("B"));

            var interpreter = new Interpreter(new Tokenizer("{B}"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive);

            ExpectUnexpectedTagException("B", 0, () => interpreter.Compile());
        }

        [Test]
        public void TestCaseInterpreterConstructUnexpectedTag2()
        {
            var directive = new RippedOpenDirective(
                Tag.Parse("A"),
                Tag.Parse("B"),
                Tag.Parse("C"));

            var interpreter = new Interpreter(new Tokenizer("{A}{C}{B}"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive);

            ExpectUnexpectedTagException("C", 3, () => interpreter.Compile());
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

            var interpreter = new Interpreter(new Tokenizer("{A}..{D}"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            ExpectUnexpectedTagException("D", 5, () => interpreter.Compile());
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

            var interpreter = new Interpreter(new Tokenizer("{A}1{C}2{A}3{B}4{D}5{B}"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);

            var evaluable = interpreter.Compile();
            var repr = evaluable.ToString();

            Assert.AreEqual("(({A}1({C}2({A}3{B})4{D})5{B}))", repr);
        }

        [Test]
        public void TestCaseInterpreterConstructSelection2()
        {
            var directive1 = new RippedOpenDirective(Tag.Parse("MATCH ME ?"));
            var directive2 = new RippedOpenDirective(Tag.Parse("MATCH ME $"));

            var interpreter1 = new Interpreter(new Tokenizer("{MATCH ME identifier}"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1).RegisterDirective(directive2);
            var interpreter2 = new Interpreter(new Tokenizer("{MATCH ME identifier}"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive2).RegisterDirective(directive1);

            var repr1 = interpreter1.Compile().ToString();
            var repr2 = interpreter2.Compile().ToString();

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

            var repr = new Interpreter(new Tokenizer("{START}{MID}{OTHER END}"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1)
                .RegisterDirective(directive2)
                .Compile()
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

            var repr = new Interpreter(new Tokenizer("{IF A}1{IF B}2{ELSE}3{END}4{IF A}5{END}6{END}"), 
                ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive1)
                .RegisterDirective(directive2)
                .Compile()
                .ToString();

            Assert.AreEqual("(({IF A}1({IF B}2{ELSE}3{END})4({IF A}5{END})6{END}))", repr);
        }
    }
}

