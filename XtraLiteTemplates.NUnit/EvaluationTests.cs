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
    using XtraLiteTemplates.Expressions.Operators.Standard;
    using XtraLiteTemplates.NUnit.Inside;
    using XtraLiteTemplates.ObjectModel;
    using XtraLiteTemplates.ObjectModel.Directives.Standard;
    using XtraLiteTemplates.Parsing;

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
        public void TestCaseRepeat1()
        {
            Double a = 99999999999999999999999999999.0 ;
            Int64 v = (Int64)a;
            if (v == 0)
                return;
       /*     var directive = new RepeatDirective();
            var interpreter = new Interpreter("{repeat count*2}R{end}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive)
                .RegisterOperator(MultiplyOperator.Standard);

            var document = interpreter.Construct();
            var context = new TestEvaluationContext("count", 5);

            using (var writer = new StringWriter())
            {
                document.Evaluate(writer, context, context);
                Assert.AreEqual("RRRRRRRRRR", writer.ToString());
            }
        * */
        }

        [Test]
        public void TestCaseEvaluationSingleTagDirective()
        {
            var directive = new RippedOpenDirective(
                Tag.Parse("T"));

            var evaluable = new Interpreter("{T}", StringComparer.OrdinalIgnoreCase)
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

            var evaluable = new Interpreter("{T1}text{T2}", StringComparer.OrdinalIgnoreCase)
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

            var evaluable = new Interpreter("{T1}first{T2}second{T3}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive).Construct();

            var exo = Evaluate(evaluable, StringComparer.OrdinalIgnoreCase);
            Assert.AreEqual("-> {T1} -> Evaluate -> (first) -> {T2} -> Evaluate -> (second) -> {T3} -> Restart -> () -> {T1} -> Skip -> () -> {T2} -> Skip -> () -> {T3} -> Terminate ->", exo);
        }

        [Test]
        public void TestCaseEvaluationStandardInterpolationDirective()
        {
            var evaluable = new Interpreter("{1}, {var_string}, {var_integer}, {var_float}, {var_boolean}, {var_object.Item1}", StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(InterpolationDirective.Standard)
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
    }
}

