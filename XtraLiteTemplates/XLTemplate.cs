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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Parsing;

    public sealed class XLTemplate
    {
        private sealed class EvaluationContext : IEvaluationContext
        {
            private Stack<Dictionary<String, Object>> m_frames;
            private IEqualityComparer<String> m_identifierComparer;
            private Boolean m_ignoreEvaluationExceptions;
            private Func<String, String> m_unparsedTextHandler;

            public EvaluationContext(Boolean ignoreEvaluationExceptions, 
                IEqualityComparer<String> identifierComparer, Func<String, String> unparsedTextHandler)
            {
                Debug.Assert(identifierComparer != null);
                Debug.Assert(unparsedTextHandler != null);

                m_identifierComparer = identifierComparer;
                m_ignoreEvaluationExceptions = ignoreEvaluationExceptions;
                m_unparsedTextHandler = unparsedTextHandler;

                m_frames = new Stack<Dictionary<String, Object>>();
            }
        
            public String ProcessUnparsedText(String value)
            {
                return m_unparsedTextHandler(value);
            }

            public void OpenEvaluationFrame()
            {
                m_frames.Push(new Dictionary<String, Object>(m_identifierComparer));
            }

            public void CloseEvaluationFrame()
            {
                Debug.Assert(m_frames.Count > 0);
                m_frames.Pop();
            }

            public void SetVariable(String identifier, Object value)
            {
                Debug.Assert(m_frames.Count > 0);

                var topFrame = m_frames.Peek();
                topFrame[identifier] = value;
            }

            public Object GetVariable(String identifier)
            {
                foreach (var frame in m_frames)
                {
                    Object result;
                    if (frame.TryGetValue(identifier, out result))
                        return result;
                }

                return null;
            }

            public bool IgnoreEvaluationExceptions
            {
                get
                {
                    return m_ignoreEvaluationExceptions;
                }
            }
        }

        public IDialect Dialect { get; private set; }
        public String Template { get; private set; }

        private IEvaluable m_evaluable;

        private IEvaluable Compile()
        {
            using (var reader = new StringReader(Template))
            {
                var tokenizer = new Tokenizer(reader, Dialect.StartTagCharacter, Dialect.EndTagCharacter, 
                    Dialect.StartStringLiteralCharacter, Dialect.EndStringLiteralCharacter, Dialect.StringLiteralEscapeCharacter, 
                    Dialect.NumberDecimalSeparatorCharacter);

                var interpreter = new Interpreter(tokenizer, Dialect.Culture, Dialect.IdentifierComparer);

                /* Register all directives and operators into the interpreter. */
                foreach (var directive in Dialect.Directives)
                    interpreter.RegisterDirective(directive);

                foreach (var @operator in Dialect.Operators)
                    interpreter.RegisterOperator(@operator);

                foreach (var keyword in Dialect.SpecialKeywords)
                    interpreter.RegisterSpecial(keyword.Key, keyword.Value);

                /* Construct the template and obtain the evaluable object. */
                return interpreter.Construct();
            }
        }

        public XLTemplate(IDialect dialect, String template)
        {
            Expect.NotNull("dialect", dialect);
            Expect.NotNull("template", template);

            Dialect = dialect;
            Template = template;

            /* Compile template */
            m_evaluable = Compile();
        }

        public void Evaluate(TextWriter writer, IReadOnlyDictionary<String, Object> variables)
        {
            Expect.NotNull("writer", writer);
            Expect.NotNull("variables", variables);

            /* Create a standard evaluation context that will be used for evaluation of said template. */
            var context = new EvaluationContext(true, Dialect.IdentifierComparer, Dialect.DecorateUnparsedText);

            /* Load in the variables. */
            context.OpenEvaluationFrame();
            foreach (var variable in variables)
                context.SetVariable(variable.Key, variable.Value);

            /* Evaluate. */
            m_evaluable.Evaluate(writer, context);
            context.CloseEvaluationFrame();
        }

        public String Evaluate(IReadOnlyDictionary<String, Object> variables)
        {
            /* Call the original version with a created writer. */
            using (var writer = new StringWriter())
            {
                Evaluate(writer, variables);
                return writer.ToString();
            }
        }

        public override String ToString()
        {
            if (m_evaluable != null)
                return m_evaluable.ToString();
            else
                return null;
        }


        public static String Evaluate(String template, Boolean ignoreCase, IReadOnlyDictionary<String, Object> variables)
        {
            Expect.NotNull("template", template);
            Expect.NotNull("variables", variables);

            var dialect = ignoreCase ? StandardDialect.CurrentCultureIgnoreCase : StandardDialect.CurrentCulture;
            var instance = new XLTemplate(dialect, template);

            return instance.Evaluate(variables);
        }

        public static String Evaluate(String template, IReadOnlyDictionary<String, Object> variables)
        {
            return Evaluate(template, true, variables);
        }

        public static String Evaluate(String template, params Tuple<String, Object>[] variables)
        {
            Expect.NotNull("variables", variables);
            return Evaluate(template, variables.ToDictionary(k => k.Item1, v => v.Item2));
        }
    }
}
