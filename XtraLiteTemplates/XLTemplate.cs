//  Copyright (c) 2015-2017, Alexandru Ciobanu (alex+git@ciobanu.org)
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
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    using JetBrains.Annotations;

    using XtraLiteTemplates.Compilation;
    using XtraLiteTemplates.Dialects;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Parsing;

    /// <summary>
    /// Facade class that uses all components exposed by the <c>XtraLiteTemplates library</c>. XLTemplate class uses an instance of <see cref="IDialect" /> interface
    /// to perform the <c>parsing</c>, <c>lexing</c> and <c>interpretation</c> of the template.
    /// </summary>
    [PublicAPI]
    public sealed class XLTemplate
    {
        [NotNull]
        private readonly CompiledTemplate<EvaluationContext> _compiledTemplate;

        /// <summary>
        /// Initializes a new instance of the <see cref="XLTemplate"/> class.
        /// </summary>
        /// <param name="dialect">An instance <see cref="IDialect" /> used to define the domain-specific language properties.</param>
        /// <param name="template">A <see cref="string" /> value that is compiled and used for evaluation.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="dialect" /> or <paramref name="template" /> parameters are <c>null</c>.</exception>
        /// <exception cref="ParseException">Parsing error during template compilation.</exception>
        /// <exception cref="ExpressionException">Expression parsing error during template compilation.</exception>
        /// <exception cref="InterpreterException">Lexical error during template compilation.</exception>
        /// <remarks>
        /// The value supplied in the <paramref name="template" /> parameter will be parsed, <c>lexed</c> and interpreted in the constructor. There are a number of exceptions
        /// that can be thrown at this stage.
        /// </remarks>
        public XLTemplate([NotNull] IDialect dialect, [NotNull] string template)
        {
            Expect.NotNull(nameof(dialect), dialect);
            Expect.NotNull(nameof(template), template);

            Dialect = dialect;
            Template = template;

            /* Compile template */
            _compiledTemplate = Compile();
        }

        /// <summary>
        /// <value>
        /// Gets the domain-specific dialect used to compile the template.
        /// </value>
        /// <remarks>
        /// This property is provided by the caller at construction time.
        /// </remarks>
        /// </summary>
        [NotNull]
        public IDialect Dialect { get; }

        /// <summary>
        /// <value>
        /// Gets the original template <see cref="string"/> that was compiled.
        /// </value>
        /// <remarks>
        /// This property is provided by the caller at construction time.
        /// </remarks>
        /// </summary>
        [NotNull]
        public string Template { get; }

        /// <summary>
        /// An easy-to-use facade method that compiles a template and immediately evaluates it.
        /// <remarks>
        /// An optional set of <paramref name="arguments"/> can be provided to the template. These objects will be exposed to the compiled template at evaluation time as follows:
        /// <para>
        /// The first object in <paramref name="arguments"/> can be referenced as <c>_0</c> inside the template. The second object can be referenced as <c>_1</c> and so on. In a sense,
        /// it tries to mimic the behavior of <see cref="string.Format(IFormatProvider,string,object[])"/> method.</para>
        /// </remarks>
        /// </summary>
        /// <param name="dialect">An instance <see cref="IDialect"/> used to define the domain-specific language properties.</param>
        /// <param name="template">A <see cref="string"/> value that is compiled and used for evaluation.</param>
        /// <param name="arguments">A <see cref="IReadOnlyDictionary{TKey,TValue}"/> storing all variables exposed to the template at evaluation time.</param>
        /// <returns>The result of evaluating the template.</returns>
        /// <exception cref="ArgumentNullException">Either <paramref name="dialect"/>, <paramref name="template"/> or <paramref name="arguments"/> parameters are <c>null</c>.</exception>
        /// <exception cref="ParseException">Parsing error during template compilation.</exception>
        /// <exception cref="ExpressionException">Expression parsing error during template compilation.</exception>
        /// <exception cref="InterpreterException">Lexical error during template compilation.</exception>
        /// <exception cref="EvaluationException">Any unrecoverable evaluation error.</exception>
        [CanBeNull]
        public static string Evaluate([NotNull] IDialect dialect, [NotNull] string template, [NotNull] params object[] arguments)
        {
            Expect.NotNull(nameof(dialect), dialect);
            Expect.NotNull(nameof(template), template);
            Expect.NotNull(nameof(arguments), arguments);

            var instance = new XLTemplate(dialect, template);

            /* Instantiate the variables */
            var variables = new Dictionary<string, object>();
            for (var i = 0; i < arguments.Length; i++)
            {
                variables.Add($"_{i}", arguments[i]);
            }

            return instance.Evaluate(variables);
        }

        /// <summary>
        /// Evaluates the result of the compiled template. The resulting text is written to <paramref name="writer"/>. All key-value-pairs
        /// contained in <paramref name="variables"/> are exposed to the compiled template as variables.
        /// </summary>
        /// <param name="writer">A <see cref="TextWriter"/> instance which will be written to.</param>
        /// <param name="variables">A <see cref="IReadOnlyDictionary{String,Object}"/> storing all variables exposed to the template at evaluation time.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="writer"/> or <paramref name="variables"/> parameters are <c>null</c>.</exception>
        /// <exception cref="EvaluationException">Any unrecoverable evaluation error.</exception>
        public void Evaluate([NotNull] TextWriter writer, [NotNull] IReadOnlyDictionary<string, object> variables)
        {
            Expect.NotNull(nameof(writer), writer);
            Expect.NotNull(nameof(variables), variables);

            /* No thread scheduling. */
            EvaluateInternal(writer, variables, CancellationToken.None);
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
        [NotNull]
        public async Task EvaluateAsync(
            [NotNull] TextWriter writer, 
            [NotNull] IReadOnlyDictionary<string, object> variables, 
            CancellationToken cancellationToken)
        {
            Expect.NotNull(nameof(writer), writer);
            Expect.NotNull(nameof(variables), variables);

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
        [NotNull]
        public async Task EvaluateAsync([NotNull] TextWriter writer, [NotNull] IReadOnlyDictionary<string, object> variables)
        {
            Expect.NotNull(nameof(writer), writer);
            Expect.NotNull(nameof(variables), variables);

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
        public string Evaluate([NotNull] IReadOnlyDictionary<string, object> variables)
        {
            /* Call the original version with a created writer. */
            using (var writer = new StringWriter())
            {
                Evaluate(writer, variables);
                return writer.ToString();
            }
        }

        /// <summary>
        /// Returns a human-readable <see cref="string"/> representation of the compiled template.
        /// </summary>
        /// <returns>The <see cref="string"/> value.</returns>
        public override string ToString()
        {
            Debug.Assert(_compiledTemplate != null, "compiledTemplate cannot be null.");
            return _compiledTemplate.ToString();
        }

        private void EvaluateInternal(
            [NotNull] TextWriter writer, 
            [NotNull] IReadOnlyDictionary<string, object> variables, 
            CancellationToken cancellationToken)
        {
            Debug.Assert(writer != null, "Argument writer cannot be null.");
            Debug.Assert(variables != null, "Argument variables cannot be null.");

            /* Create a standard evaluation context that will be used for evaluation of said template. */
            var context = new EvaluationContext(
                true,
                cancellationToken,
                Dialect.IdentifierComparer,
                Dialect.ObjectFormatter,
                Dialect.Self,
                Dialect.DecorateUnParsedText);

            /* Load in the variables. */
            foreach (var variable in variables)
            {
                context.SetProperty(variable.Key, variable.Value);
            }

            /* Evaluate. */
            _compiledTemplate.Evaluate(writer, context);
        }

        [NotNull]
        private CompiledTemplate<EvaluationContext> Compile()
        {
            using (var reader = new StringReader(Template))
            {
                var tokenizer = new Tokenizer(
                    reader,
                    Dialect.StartTagCharacter,
                    Dialect.EndTagCharacter,
                    Dialect.StartStringLiteralCharacter,
                    Dialect.EndStringLiteralCharacter,
                    Dialect.StringLiteralEscapeCharacter,
                    Dialect.NumberDecimalSeparatorCharacter);

                var interpreter = new Interpreter(tokenizer, Dialect.FlowSymbols, Dialect.IdentifierComparer);

                /* Register all directives and operators into the interpreter. */
                foreach (var directive in Dialect.Directives)
                {
                    interpreter.RegisterDirective(directive);
                }

                foreach (var @operator in Dialect.Operators)
                {
                    interpreter.RegisterOperator(@operator);
                }

                foreach (var keyword in Dialect.SpecialKeywords)
                {
                    interpreter.RegisterSpecial(keyword.Key, keyword.Value);
                }

                /* Construct the template and obtain the evaluable object. */
                return interpreter.Compile();
            }
        }
    }
}