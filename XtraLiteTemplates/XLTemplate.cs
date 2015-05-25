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

    /// <summary>
    /// Facade class that uses all components exposed by the XtraLiteTemplates library. XLTemplate class uses an instance of <see cref="IDialect"/> interface
    /// to peform the parsing, lexing and interpretation of the template.
    /// 
    /// A private implementation of <see cref="IEvaluationContext"/> interface is used to perform the actual evaluation of the compiled template.
    /// </summary>
    public sealed class XLTemplate
    {
        private sealed class EvaluationContext : IEvaluationContext
        {
            private sealed class Frame
            {
                public Dictionary<string, object> Variables;
                public HashSet<object> StateObjects;
            }

            private Stack<Frame> m_frames;
            private Dictionary<Type, SimpleTypeDisemboweler> m_disembowelers;
            private IEqualityComparer<string> m_identifierComparer;
            private bool m_ignoreEvaluationExceptions;
            private Func<IExpressionEvaluationContext, string, string> m_unparsedTextHandler;

            public EvaluationContext(bool ignoreEvaluationExceptions,
                IEqualityComparer<string> identifierComparer, Func<IExpressionEvaluationContext, string, string> unparsedTextHandler)
            {
                Debug.Assert(identifierComparer != null);
                Debug.Assert(unparsedTextHandler != null);

                m_identifierComparer = identifierComparer;
                m_ignoreEvaluationExceptions = ignoreEvaluationExceptions;
                m_unparsedTextHandler = unparsedTextHandler;

                m_disembowelers = new Dictionary<Type, SimpleTypeDisemboweler>();
                m_frames = new Stack<Frame>();
            }

            public string ProcessUnparsedText(string value)
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

            public void SetVariable(string identifier, object value)
            {
                var topFrame = TopFrame;
                if (topFrame.Variables == null)
                    topFrame.Variables = new Dictionary<String, Object>(m_identifierComparer);

                topFrame.Variables[identifier] = value;
            }

            public object GetVariable(string identifier)
            {
                foreach (var frame in m_frames)
                {
                    object result;
                    if (frame.Variables != null && frame.Variables.TryGetValue(identifier, out result))
                        return result;
                }

                return null;
            }

            public object GetProperty(object variable, string memberName)
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


            public void AddStateObject(object state)
            {
                var topFrame = TopFrame;
                if (topFrame.StateObjects == null)
                    topFrame.StateObjects = new HashSet<Object>();

                topFrame.StateObjects.Add(state);
            }

            public void RemoveStateObject(object state)
            {
                var topFrame = TopFrame;
                if (topFrame.StateObjects != null)
                    topFrame.StateObjects.Remove(state);
            }

            public bool ContainsStateObject(object state)
            {
                var topFrame = TopFrame;
                return topFrame.StateObjects != null && topFrame.StateObjects.Contains(state);
            }
        }

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

        /// <summary>
        /// <value>
        /// Specifies the domain-specific dialect used to compile the template.
        /// </value>
        /// <remarks>
        /// This property is provided by the caller at construction time.
        /// </remarks>
        /// </summary>
        public IDialect Dialect { get; private set; }

        /// <summary>
        /// <value>
        /// Specifies the original template <see cref="String"/> that was compiled.
        /// </value>
        /// <remarks>
        /// This property is provided by the caller at construction time.
        /// </remarks>
        /// </summary>
        public string Template { get; private set; }


        /// <summary>
        /// Creates a new instance if <see cref="XLTemplate"/> class.
        /// <remarks>
        /// The value supplied in the <paramref name="template"/> parameter will be parsed, lexed and interpreted in the constructor. There are a number of exceptions
        /// that can be trown at this stage.
        /// </remarks>
        /// </summary>
        /// <param name="dialect">An instance <see cref="IDialect"/> used to define the domain-specific language properties.</param>
        /// <param name="template">A <see cref="String"/> value that is compiled and used for evaluation.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="dialect"/> or <paramref name="template"/> parameters are <c>null</c>.</exception>
        /// <exception cref="ParseException">Parsing error during template compilation.</exception>
        /// <exception cref="ExpressionException">Expression parsing error during template compilation.</exception>
        /// <exception cref="InterpreterException">Lexical error during template compilation.</exception>
        public XLTemplate(IDialect dialect, string template)
        {
            Expect.NotNull("dialect", dialect);
            Expect.NotNull("template", template);

            Dialect = dialect;
            Template = template;

            /* Compile template */
            m_evaluable = Compile();
        }

        /// <summary>
        /// Evaluates the result of the compiled template. The resulting text is written to <paramref name="writer"/>. All key-value-pairs
        /// contained in <paramref name="variables"/> are exposed to the compiled template as variables.
        /// </summary>
        /// <param name="writer">A <see cref="TextWriter"/> instance which will be written to.</param>
        /// <param name="variables">A <see cref="IReadOnlyDictionary{String,Object}"/> storing all variables exposed to the template at evaluation time.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="writer"/> or <paramref name="variables"/> parameters are <c>null</c>.</exception>
        /// <exception cref="EvaluationException">Any unrecoverable evaluation error.</exception>
        public void Evaluate(TextWriter writer, IReadOnlyDictionary<string, object> variables)
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

        /// <summary>
        /// Evaluates the result of the compiled template. All key-value-pairs contained in <paramref name="variables"/> are exposed to the compiled template as variables.
        /// </summary>
        /// <param name="variables">A <see cref="IReadOnlyDictionary{String, Object}"/> storing all variables exposed to the template at evaluation time.</param>
        /// <returns>The result of evaluating the template.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="variables"/> is <c>null</c>.</exception>
        /// <exception cref="EvaluationException">Any unrecoverable evaluation error.</exception>
        public string Evaluate(IReadOnlyDictionary<string, object> variables)
        {
            /* Call the original version with a created writer. */
            using (var writer = new StringWriter())
            {
                Evaluate(writer, variables);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Returns a human-readable <see cref="System.String"/> representation of the compiled template.
        /// </summary>
        /// <returns>The <see cref="System.String"/> value.</returns>
        public override string ToString()
        {
            Debug.Assert(m_evaluable != null);
            return m_evaluable.ToString();
        }

        /// <summary>
        /// An easy-to-use facade method that compiles a template and immediately evaluates it.
        /// <remarks>
        /// An optional set of <paramref name="arguments"/> can be provided to the template. These objects will be exposed to the compiled template at evaluation time as follows:
        /// <para>
        /// The first object in <paramref name="arguments"/> can be refereced as <c>_0</c> inside the template. The second object can be referenced as <c>_1</c> and so on. In a sense,
        /// it tries to mimic the behavior of <see cref="String.Format(IFormatProvider,String,Object[])"/> method.</para>
        /// </remarks>
        /// </summary>
        /// <param name="dialect">An instance <see cref="XtraLiteTemplates.Dialects.IDialect"/> used to define the domain-specific language properties.</param>
        /// <param name="template">A <see cref="System.String"/> value that is compiled and used for evaluation.</param>
        /// <param name="arguments">A <see cref="IReadOnlyDictionary{String, Object}"/> storing all variables exposed to the template at evaluation time.</param>
        /// <returns>The result of evaluating the template.</returns>
        /// <exception cref="ArgumentNullException">Either <paramref name="dialect"/>, <paramref name="template"/> or <paramref name="arguments"/> parameters are <c>null</c>.</exception>
        /// <exception cref="ParseException">Parsing error during template compilation.</exception>
        /// <exception cref="ExpressionException">Expression parsing error during template compilation.</exception>
        /// <exception cref="InterpreterException">Lexical error during template compilation.</exception>
        /// <exception cref="EvaluationException">Any unrecoverable evaluation error.</exception>
        public static string Evaluate(IDialect dialect, string template, params object[] arguments)
        {
            Expect.NotNull("dialect", dialect);
            Expect.NotNull("template", template);
            Expect.NotNull("arguments", arguments);

            var instance = new XLTemplate(dialect, template);

            /* Instatiate the variables */
            var variables = new Dictionary<String, Object>();
            for (var i = 0; i < arguments.Length; i++)
                variables.Add(string.Format("_{0}", i), arguments[i]);
            
            return instance.Evaluate(variables);
        }
    }
}
