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

namespace XtraLiteTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Parsing;

    public sealed class XLTemplate
    {
        public IDialect Dialect { get; private set; }
        public String Template { get; private set; }

        private IEvaluable m_evaluable { get; private set; }

        private IEvaluable Compile(String template)
        {
            Debug.Assert(template != null);

            var tokenizer = new Tokenizer(Template);
            var interpreter = new Interpreter(tokenizer, Dialect.Comparer);

            /* Register all directives and operators into the interpreter. */
            foreach (var directive in Dialect.Directives)
                interpreter.RegisterDirective(directive);

            foreach (var @operator in Dialect.Operators)
                interpreter.RegisterOperator(@operator);

            /* Construct the template and obtain the evaluable object. */
            return interpreter.Construct();
        }

        public XLTemplate(IDialect dialect, String template)
        {
            Expect.NotNull("dialect", dialect);
            Expect.NotNull("template", template);

            /* Compile template */
            m_evaluable = Compile(template);
        }

        public void Evaluate(TextWriter writer)
        {
            Expect.NotNull("writer", writer);

            /* Create a standard evaluation context that will be used for evaluation of said template. */
            var context = new StandardEvaluationContext(true, Dialect.Comparer, Dialect.Culture);
            m_evaluable.Evaluate(writer, context);
        }

        public String Evaluate()
        {
            /* Call the original version with a created writer. */
            using (var writer = new StringWriter())
            {
                Evaluate(writer);
                return writer.ToString();
            }
        }
    }
}
