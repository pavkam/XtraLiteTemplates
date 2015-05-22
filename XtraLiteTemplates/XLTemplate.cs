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
    using XtraLiteTemplates.Dialects;
    using XtraLiteTemplates.Dialects.Standard;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Parsing;

    public sealed class XLTemplate
    {
        private sealed class EvaluationContext : IEvaluationContext
        {
            private sealed class Frame
            {
                public Dictionary<String, Object> Variables;
                public HashSet<Object> StateObjects;
            }

            private Stack<Frame> m_frames;
            private Dictionary<Type, SimpleTypeDisemboweler> m_disembowelers;
            private IEqualityComparer<String> m_identifierComparer;
            private Boolean m_ignoreEvaluationExceptions;
            private Func<IExpressionEvaluationContext, String, String> m_unparsedTextHandler;

            public EvaluationContext(Boolean ignoreEvaluationExceptions, 
                IEqualityComparer<String> identifierComparer, Func<IExpressionEvaluationContext, String, String> unparsedTextHandler)
            {
                Debug.Assert(identifierComparer != null);
                Debug.Assert(unparsedTextHandler != null);

                m_identifierComparer = identifierComparer;
                m_ignoreEvaluationExceptions = ignoreEvaluationExceptions;
                m_unparsedTextHandler = unparsedTextHandler;

                m_disembowelers = new Dictionary<Type, SimpleTypeDisemboweler>();
                m_frames = new Stack<Frame>();
            }

            public String ProcessUnparsedText(String value)
            {
                return m_unparsedTextHandler(this, value);
            }

            private Frame TopFrame
            {
                get
                {
                    Debug.Assert(m_frames.Count > 0);
                    return m_frames.Peek();
                }
            }


            public void OpenEvaluationFrame()
            {
                m_frames.Push(new Frame());
            }

            public void CloseEvaluationFrame()
            {
                Debug.Assert(m_frames.Count > 0);
                m_frames.Pop();
            }

            public void SetVariable(String identifier, Object value)
            {
                var topFrame = TopFrame;
                if (topFrame.Variables == null)
                    topFrame.Variables = new Dictionary<String, Object>(m_identifierComparer);

                topFrame.Variables[identifier] = value;
            }

            public Object GetVariable(String identifier)
            {
                foreach (var frame in m_frames)
                {
                    Object result;
                    if (frame.Variables != null && frame.Variables.TryGetValue(identifier, out result))
                        return result;
                }

                return null;
            }

            public Object GetProperty(Object variable, String memberName)
            {
                Expect.Identifier("memberName", memberName);

                if (variable != null)
                {
                    var type = variable.GetType();

                    SimpleTypeDisemboweler disemboweler;
                    if (!m_disembowelers.TryGetValue(type, out disemboweler))
                    {
                        disemboweler = new SimpleTypeDisemboweler(type,
                            SimpleTypeDisemboweler.EvaluationOptions.TreatAllErrorsAsNull |
                            SimpleTypeDisemboweler.EvaluationOptions.TreatParameterlessFunctionsAsProperties, m_identifierComparer);

                        m_disembowelers.Add(type, disemboweler);
                    }

                    return disemboweler.Read(memberName, variable);
                }
                else
                    return null;
            }

            public bool IgnoreEvaluationExceptions
            {
                get
                {
                    return m_ignoreEvaluationExceptions;
                }
            }


            public void AddStateObject(Object state)
            {
                var topFrame = TopFrame;
                if (topFrame.StateObjects == null)
                    topFrame.StateObjects = new HashSet<Object>();

                topFrame.StateObjects.Add(state);
            }

            public void RemoveStateObject(Object state)
            {
                var topFrame = TopFrame;
                if (topFrame.StateObjects != null)
                    topFrame.StateObjects.Remove(state);
            }

            public bool ContainsStateObject(Object state)
            {
                var topFrame = TopFrame;
                return topFrame.StateObjects != null && topFrame.StateObjects.Contains(state);
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

                var interpreter = new Interpreter(tokenizer, Dialect.FlowSymbols, Dialect.IdentifierComparer);

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


        public static String Evaluate(IDialect dialect, String template, params Object[] arguments)
        {
            Expect.NotNull("dialect", dialect);
            Expect.NotNull("template", template);
            Expect.NotNull("arguments", arguments);

            var instance = new XLTemplate(dialect, template);

            /* Instatiate the variables */
            var variables = new Dictionary<String, Object>();
            for (var i = 0; i < arguments.Length; i++)
                variables.Add(String.Format("_{0}", i), arguments[i]);
            
            return instance.Evaluate(variables);
        }
    }
}
