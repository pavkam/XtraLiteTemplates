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

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Does not apply.")]

namespace XtraLiteTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using XtraLiteTemplates.Compilation;
    using XtraLiteTemplates.Dialects;
    using XtraLiteTemplates.Dialects.Standard;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Introspection;
    using XtraLiteTemplates.Parsing;

    /// <summary>
    /// Facade class that uses all components exposed by the <c>XtraLiteTemplates library</c>. XLTemplate class uses an instance of <see cref="IDialect" /> interface
    /// to perform the <c>parsing</c>, <c>lexing</c> and <c>interpretation</c> of the template.
    /// </summary>
    public sealed class XLTemplate
    {
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private CompiledTemplate<EvaluationContext> compiledTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="XLTemplate"/> class.
        /// </summary>
        /// <param name="dialect">An instance <see cref="IDialect" /> used to define the domain-specific language properties.</param>
        /// <param name="template">A <see cref="String" /> value that is compiled and used for evaluation.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="dialect" /> or <paramref name="template" /> parameters are <c>null</c>.</exception>
        /// <exception cref="ParseException">Parsing error during template compilation.</exception>
        /// <exception cref="ExpressionException">Expression parsing error during template compilation.</exception>
        /// <exception cref="InterpreterException">Lexical error during template compilation.</exception>
        /// <remarks>
        /// The value supplied in the <paramref name="template" /> parameter will be parsed, <c>lexed</c> and interpreted in the constructor. There are a number of exceptions
        /// that can be thrown at this stage.
        /// </remarks>
        public XLTemplate(IDialect dialect, string template)
        {
            Expect.NotNull("dialect", dialect);
            Expect.NotNull("template", template);

            this.Dialect = dialect;
            this.Template = template;

            /* Compile template */
            this.compiledTemplate = this.Compile();
        }

        /// <summary>
        /// <value>
        /// Gets the domain-specific dialect used to compile the template.
        /// </value>
        /// <remarks>
        /// This property is provided by the caller at construction time.
        /// </remarks>
        /// </summary>
        public IDialect Dialect { get; private set; }

        /// <summary>
        /// <value>
        /// Gets the original template <see cref="String"/> that was compiled.
        /// </value>
        /// <remarks>
        /// This property is provided by the caller at construction time.
        /// </remarks>
        /// </summary>
        public string Template { get; private set; }

        /// <summary>
        /// An easy-to-use facade method that compiles a template and immediately evaluates it.
        /// <remarks>
        /// An optional set of <paramref name="arguments"/> can be provided to the template. These objects will be exposed to the compiled template at evaluation time as follows:
        /// <para>
        /// The first object in <paramref name="arguments"/> can be referenced as <c>_0</c> inside the template. The second object can be referenced as <c>_1</c> and so on. In a sense,
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
            var variables = new Dictionary<string, object>();
            for (var i = 0; i < arguments.Length; i++)
            {
                variables.Add(string.Format("_{0}", i), arguments[i]);
            }

            return instance.Evaluate(variables);
        }

        /// <summary>
        /// Evaluates the result of the compiled template. The resulting text is written to <paramref name="writer"/>. All key-value-pairs
        /// contained in <paramref name="variables"/> are exposed to the compiled template as variables.
        /// </summary>
        /// <param name="writer">A <see cref="TextWriter"/> instance which will be written to.</param>
        /// <param name="variables">A <see cref="IReadOnlyDictionary{String,Object}"/> storing all variables exposed to the template at evaluation time.</param>
        /// <returns><c>true</c> if the evaluation ended within the allocated time; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Either <paramref name="writer"/> or <paramref name="variables"/> parameters are <c>null</c>.</exception>
        /// <exception cref="EvaluationException">Any unrecoverable evaluation error.</exception>
        public void Evaluate(TextWriter writer, IReadOnlyDictionary<string, object> variables)
        {
            Expect.NotNull("writer", writer);
            Expect.NotNull("variables", variables);

            /* No thread scheduling. */
            this.EvaluateInternal(writer, variables, CancellationToken.None);
        }

        /// <summary>
        /// Evaluates the result of the compiled template asynchronously. The resulting text is written to <paramref name="writer"/>. All key-value-pairs
        /// contained in <paramref name="variables"/> are exposed to the compiled template as variables.
        /// </summary>
        /// <param name="writer">A <see cref="TextWriter"/> instance which will be written to.</param>
        /// <param name="variables">A <see cref="IReadOnlyDictionary{String,Object}"/> storing all variables exposed to the template at evaluation time.</param>
        /// <param name="cancellationToken">A <see cref="CancellationToken"/> instance.</param>
        /// <returns>The <see cref="Task"/> instance representing the asynchronous task.</returns>
        /// <exception cref="ArgumentNullException">Either <paramref name="writer"/> or <paramref name="variables"/> parameters are <c>null</c>.</exception>
        /// <exception cref="EvaluationException">Any unrecoverable evaluation error.</exception>
        public async Task EvaluateAsync(TextWriter writer, IReadOnlyDictionary<string, object> variables, CancellationToken cancellationToken)
        {
            Expect.NotNull("writer", writer);
            Expect.NotNull("variables", variables);

            await Task.Run(
                () =>
                {
                    EvaluateInternal(writer, variables, cancellationToken);
                }, 
                cancellationToken);
        }

        /// <summary>
        /// Evaluates the result of the compiled template asynchronously. The resulting text is written to <paramref name="writer"/>. All key-value-pairs
        /// contained in <paramref name="variables"/> are exposed to the compiled template as variables.
        /// </summary>
        /// <param name="writer">A <see cref="TextWriter"/> instance which will be written to.</param>
        /// <param name="variables">A <see cref="IReadOnlyDictionary{String,Object}"/> storing all variables exposed to the template at evaluation time.</param>
        /// <returns>The <see cref="Task"/> instance representing the asynchronous task.</returns>
        /// <exception cref="ArgumentNullException">Either <paramref name="writer"/> or <paramref name="variables"/> parameters are <c>null</c>.</exception>
        /// <exception cref="EvaluationException">Any unrecoverable evaluation error.</exception>
        public async Task EvaluateAsync(TextWriter writer, IReadOnlyDictionary<string, object> variables)
        {
            Expect.NotNull("writer", writer);
            Expect.NotNull("variables", variables);

            await Task.Run(() =>
            {
                EvaluateInternal(writer, variables, CancellationToken.None);
            });
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
            Debug.Assert(this.compiledTemplate != null, "compiledTemplate cannot be null.");
            return this.compiledTemplate.ToString();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private void EvaluateInternal(TextWriter writer, IReadOnlyDictionary<string, object> variables, CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null, "Argument writer cannot be null.");
            Debug.Assert(variables != null, "Argument variables cannot be null.");

            /* Create a standard evaluation context that will be used for evaluation of said template. */
            var context = new EvaluationContext(
                true,
                cancellationToken,
                this.Dialect.IdentifierComparer,
                this.Dialect.ObjectFormatter,
                this.Dialect.Self,
                this.Dialect.DecorateUnparsedText);

            /* Load in the variables. */
            foreach (var variable in variables)
            {
                context.SetProperty(variable.Key, variable.Value);
            }

            /* Evaluate. */
            this.compiledTemplate.Evaluate(writer, context);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private CompiledTemplate<EvaluationContext> Compile()
        {
            using (var reader = new StringReader(this.Template))
            {
                var tokenizer = new Tokenizer(
                    reader,
                    this.Dialect.StartTagCharacter,
                    this.Dialect.EndTagCharacter,
                    this.Dialect.StartStringLiteralCharacter,
                    this.Dialect.EndStringLiteralCharacter,
                    this.Dialect.StringLiteralEscapeCharacter,
                    this.Dialect.NumberDecimalSeparatorCharacter);

                var interpreter = new Interpreter(tokenizer, this.Dialect.FlowSymbols, this.Dialect.IdentifierComparer);

                /* Register all directives and operators into the interpreter. */
                foreach (var directive in this.Dialect.Directives)
                {
                    interpreter.RegisterDirective(directive);
                }

                foreach (var @operator in this.Dialect.Operators)
                {
                    interpreter.RegisterOperator(@operator);
                }

                foreach (var keyword in this.Dialect.SpecialKeywords)
                {
                    interpreter.RegisterSpecial(keyword.Key, keyword.Value);
                }

                /* Construct the template and obtain the evaluable object. */
                return interpreter.Compile();
            }
        }
    }
}
