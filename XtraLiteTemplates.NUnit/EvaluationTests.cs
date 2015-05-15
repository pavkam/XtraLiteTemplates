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
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using XtraLiteTemplates.Expressions.Operators.Standard;
    using XtraLiteTemplates.NUnit.Inside;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Evaluation.Directives.Standard;
    using XtraLiteTemplates.Parsing;
    using System.Globalization;

    [TestFixture]
    public class EvaluationTests : TestBase
    {
        private KeyValuePair<String, Object> kw(String key, Object value)
        {
            return new KeyValuePair<String, Object>(key, value);
        }

        private String Evaluate(IEvaluable evaluable, StringComparer comparer, params KeyValuePair<String, Object>[] values)
        {
            var context = new TestEvaluationContext(values, comparer);

            String result = null;
            using (var sw = new StringWriter())
            {
                evaluable.Evaluate(sw, context);
                result = sw.ToString();
            }

            return result;
        }



        [Test]
        public void TestCaseEvaluationSingleTagDirective()
        {
            var directive = new RippedOpenDirective(
                Tag.Parse("T"));

            var evaluable = new Interpreter(new Tokenizer("{T}"), 
                CultureInfo.InvariantCulture, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive).Construct();

            var exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase);
            Assert.AreEqual("-> {T} -> Restart -> () -> {T} -> Terminate ->", exo);
        }

        [Test]
        public void TestCaseEvaluationTwoTagDirective()
        {
            var directive = new RippedOpenDirective(
                Tag.Parse("T1"),
                Tag.Parse("T2"));

            var evaluable = new Interpreter(new Tokenizer("{T1}text{T2}"), 
                CultureInfo.InvariantCulture, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive).Construct();

            var exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase);
            Assert.AreEqual("-> {T1} -> Evaluate -> (text) -> {T2} -> Restart -> () -> {T1} -> Skip -> () -> {T2} -> Terminate ->", exo);
        }

        [Test]
        public void TestCaseEvaluationThreeTagDirective()
        {
            var directive = new RippedOpenDirective(
                Tag.Parse("T1"),
                Tag.Parse("T2"),
                Tag.Parse("T3"));

            var evaluable = new Interpreter(new Tokenizer("{T1}first{T2}second{T3}"), CultureInfo.InvariantCulture, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive).Construct();

            var exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase);
            Assert.AreEqual("-> {T1} -> Evaluate -> (first) -> {T2} -> Evaluate -> (second) -> {T3} -> Restart -> () -> {T1} -> Skip -> () -> {T2} -> Skip -> () -> {T3} -> Terminate ->", exo);
        }

        [Test]
        public void TestCaseEvaluationStandardInterpolationDirective()
        {
            var evaluable = new Interpreter(new Tokenizer("{1}, {var_string}, {var_integer}, {var_float}, {var_boolean}, {var_object.Item1}"), 
                CultureInfo.InvariantCulture, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(new InterpolationDirective(CreateTypeConverter()))
                .RegisterOperator(new MemberAccessOperator(StringComparer.OrdinalIgnoreCase))
                .Construct();

            var exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase, 
                kw("var_string", "some string"), 
                kw("var_integer", 123), 
                kw("var_float", 1.33), 
                kw("var_boolean", false),
                kw("var_object", new Tuple<String>("inner item"))
            );

            Assert.AreEqual("1, some string, 123, 1.33, False, inner item", exo);
        }

        [Test]
        public void TestCaseEvaluationStandardConditionalInterpolationDirective()
        {
            var evaluable = new Interpreter(new Tokenizer("{\"DEAR\" + \" \" IF Dude.IsDear}{Dude.FirstName} {Dude.LastName}"), 
                CultureInfo.InvariantCulture, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(new InterpolationDirective(CreateTypeConverter()))
                .RegisterDirective(new ConditionalInterpolationDirective(CreateTypeConverter()))
                .RegisterOperator(new MemberAccessOperator(StringComparer.OrdinalIgnoreCase))
                .RegisterOperator(new ArithmeticSumOperator(CreateTypeConverter()))
                .Construct();

            var dude = new
            {
                IsDear = false,
                FirstName = "Jenny",
                LastName = "O'Peters",
            };

            var dear_dude = new
            {
                IsDear = true,
                FirstName = "John",
                LastName = "McDude",
            };

            var exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase,
                kw("Dude", dude));
            var dear_exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase,
                kw("Dude", dear_dude));

            Assert.AreEqual("Jenny O'Peters", exo);
            Assert.AreEqual("DEAR John McDude", dear_exo);
        }

        [Test]
        public void TestCaseEvaluationStandardRepeatDirective()
        {
            var evaluable = new Interpreter(new Tokenizer("{REPEAT 5 TIMES}text-{END REPEAT}"), 
                CultureInfo.InvariantCulture, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(new RepeatDirective(CreateTypeConverter()))
                .RegisterOperator(new ArithmeticSumOperator(CreateTypeConverter()))
                .Construct();

            var exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase);

            Assert.AreEqual("text-text-text-text-text-", exo);
        }

        [Test]
        public void TestCaseEvaluationStandardForEachDirective1()
        {
            var evaluable = new Interpreter(new Tokenizer("{FOR EACH name IN names}{name},{END FOR EACH}"), 
                CultureInfo.InvariantCulture, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(new InterpolationDirective(CreateTypeConverter()))
                .RegisterDirective(new ForEachDirective(CreateTypeConverter()))
                .Construct();

            String[] name = new String[] { "Mary", "Joe", "Peter" };
            var exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase, kw("names", name));

            Assert.AreEqual("Mary,Joe,Peter,", exo);
        }

        [Test]
        public void TestCaseEvaluationStandardForEachDirective2()
        {
            var evaluable = new Interpreter(new Tokenizer("{FOR EACH x IN 1:2}{FOR EACH y IN 3:4}{x + \"-\" + y},{END FOR EACH}{END FOR EACH}"), 
                CultureInfo.InvariantCulture, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(new InterpolationDirective(CreateTypeConverter()))
                .RegisterDirective(new ForEachDirective(CreateTypeConverter()))
                .RegisterOperator(new IntegerRangeOperator(CreateTypeConverter()))
                .RegisterOperator(new ArithmeticSumOperator(CreateTypeConverter()))
                .Construct();

            var exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase);

            Assert.AreEqual("1-3,1-4,2-3,2-4,", exo);
        }

        [Test]
        public void TestCaseEvaluationStandardIfDirective()
        {
            var evaluable = new Interpreter(new Tokenizer("{IF true THEN}this{IF true THEN}_will_{END IF}evaluate{END IF}{IF false THEN}but this won't!{END IF}"), 
                CultureInfo.InvariantCulture, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(new IfDirective(CreateTypeConverter()))
                .RegisterOperator(new ArithmeticSumOperator(CreateTypeConverter()))
                .Construct();

            var exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase, kw("true", true), kw("false", false));

            Assert.AreEqual("this_will_evaluate", exo);
        }

        [Test]
        public void TestCaseEvaluationStandardIfElseDirective()
        {
            var evaluable = new Interpreter(new Tokenizer("{IF true THEN}1{ELSE}2{END IF}{IF false THEN}3{ELSE}4{END IF}"), 
                CultureInfo.InvariantCulture, StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(new IfElseDirective(CreateTypeConverter()))
                .RegisterOperator(new ArithmeticSumOperator(CreateTypeConverter()))
                .Construct();

            var exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase, kw("true", true), kw("false", false));

            Assert.AreEqual("14", exo);
        }
    }
}

