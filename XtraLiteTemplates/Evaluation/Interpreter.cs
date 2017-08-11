//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
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

namespace XtraLiteTemplates.Evaluation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using JetBrains.Annotations;

    using XtraLiteTemplates.Compilation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Parsing;

    /// <summary>
    /// Provides <c>lex</c> interpretation facilities. Instances of this class are used to interpret
    /// sequences of <see cref="Lex" /> objects and assemble the final <see cref="CompiledTemplate{TContext}" /> objects.
    /// </summary>
    [PublicAPI]
    public sealed class Interpreter
    {
        [NotNull]
        private static readonly CompiledTemplateFactory CompiledTemplateFactory = new CompiledTemplateFactory();

        [NotNull]
        private readonly List<Directive> _registeredDirectives;

        [NotNull]
        private readonly Lexer _lexer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Interpreter"/> class.
        /// </summary>
        /// <param name="tokenizer">The <c>tokenizer</c> instance.</param>
        /// <param name="expressionFlowSymbols">The expression flow symbols.</param>
        /// <param name="comparer">The keyword and identifier comparer.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="tokenizer"/> or <paramref name="expressionFlowSymbols"/> or <paramref name="comparer"/> is <c>null</c>.</exception>
        public Interpreter(
            [NotNull] ITokenizer tokenizer,
            [NotNull] ExpressionFlowSymbols expressionFlowSymbols,
            [NotNull] IEqualityComparer<string> comparer)
        {
            Expect.NotNull(nameof(tokenizer), tokenizer);
            Expect.NotNull(nameof(comparer), comparer);
            Expect.NotNull(nameof(expressionFlowSymbols), expressionFlowSymbols);

            _lexer = new Lexer(tokenizer, expressionFlowSymbols, comparer);
            _registeredDirectives = new List<Directive>();
        }

        /// <summary>
        /// Gets the comparer used for keyword and identifier comparison.
        /// </summary>
        /// <value>
        /// The keyword and identifier comparer.
        /// </value>
        /// <remarks>
        /// Value of this property is specified by the caller at construction time.
        /// </remarks>
        [NotNull]
        public IEqualityComparer<string> Comparer => _lexer.Comparer;

        /// <summary>
        /// Registers a directive with this interpreter instance.
        /// </summary>
        /// <param name="directive">The directive to register.</param>
        /// <returns>This interpreter instance.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="directive"/> is <c>null</c>.</exception>
        [NotNull]
        public Interpreter RegisterDirective([NotNull] Directive directive)
        {
            Expect.NotNull(nameof(directive), directive);
            Debug.Assert(directive.Tags.Any(), "Directive must have at least one tag defined.");

            if (!_registeredDirectives.Contains(directive))
            {
                foreach (var tag in directive.Tags)
                {
                    _lexer.RegisterTag(tag);
                }

                _registeredDirectives.Add(directive);
            }

            return this;
        }

        /// <summary>
        /// Registers an operator with this interpreter instance.
        /// </summary>
        /// <param name="operator">The operator to register.</param>
        /// <returns>This interpreter instance.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="operator"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The symbol used by <paramref name="operator"/> is already registered with the interpreter.</exception>
        [NotNull]
        public Interpreter RegisterOperator([NotNull] Operator @operator)
        {
            _lexer.RegisterOperator(@operator);
            return this;
        }

        /// <summary>
        /// Registers a special constant with this interpreter.
        /// </summary>
        /// <param name="keyword">The constant name.</param>
        /// <param name="value">The constant value.</param>
        /// <returns>This interpreter instance.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="keyword"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="keyword"/> is empty.</exception>
        [NotNull]
        [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
        public Interpreter RegisterSpecial([NotNull] string keyword, [CanBeNull] object value)
        {
            _lexer.RegisterSpecial(keyword, value);
            return this;
        }

        /// <summary>
        /// Compiles a template supplied by the <see cref="ITokenizer"/> instance offered at construction time.
        /// </summary>
        /// <param name="factory">The factory used to compile the interpreted document into the final <see cref="CompiledTemplate{TContext}"/>.</param>
        /// <typeparam name="TContext">Any class that implements <see cref="IExpressionEvaluationContext"/> interface.</typeparam>
        /// <returns>An evaluable object.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="factory"/> is <c>null</c>.</exception>
        [NotNull]
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public CompiledTemplate<TContext> Compile<TContext>([NotNull] CompiledTemplateFactory<TContext> factory)
            where TContext : IExpressionEvaluationContext
        {
            Expect.NotNull(nameof(factory), factory);

            var document = new TemplateDocument();
            Interpret(document);

            return new CompiledTemplate<TContext>(document, factory.CompileTemplate(document));
        }

        /// <summary>
        /// Compiles the template prepared for the standard <see cref="EvaluationContext"/>-based evaluator.
        /// </summary>
        /// <returns>A compiled template.</returns>
        [NotNull]
        public CompiledTemplate<EvaluationContext> Compile()
        {
            return Compile(CompiledTemplateFactory);
        }

        private void Interpret([NotNull] CompositeNode compositeNode)
        {
            Debug.Assert(compositeNode != null, "compositeNode cannot be null.");
            var matchIndex = 1;

            while (true)
            {
                /* Read the next lex out of the lexer. */
                var lex = _lexer.ReadNext();
                if (lex == null)
                {
                    break;
                }

                if (lex is UnParsedLex unParsedLex)
                {
                    /* This is an un-parsed lex. */
                    compositeNode.AddChild(new UnParsedNode(compositeNode, unParsedLex));
                }
                else
                {
                    /* This can only be a tag lex. */
                    var tagLex = lex as TagLex;
                    Debug.Assert(tagLex != null, "lex must be a tag lex.");

                    if (compositeNode is DirectiveNode directiveCompositeNode
                        && directiveCompositeNode.SelectDirective(matchIndex, tagLex.Tag, Comparer))
                    {
                        /* OK, this is the Nth part of the current directive. */
                        matchIndex++;
                        compositeNode.AddChild(new TagNode(directiveCompositeNode, tagLex));

                        if (directiveCompositeNode.CandidateDirectiveLockedIn)
                        {
                            break;
                        }

                        continue;
                    }

                    /* Match any directive that starts with this tag. */
                    var candidateDirectives = _registeredDirectives.Where(p => p.Tags[0].Equals(tagLex.Tag, Comparer))
                        .ToArray();
                    if (candidateDirectives.Length == 0)
                    {
                        /* No directive found that starts with the supplied tag! Nothing we can do but bail at this point. */
                        ExceptionHelper.UnexpectedTag(tagLex);
                    }

                    var directiveNode = new DirectiveNode(compositeNode, candidateDirectives);
                    compositeNode.AddChild(directiveNode);
                    directiveNode.AddChild(new TagNode(directiveNode, tagLex));

                    /* Select the current directive. */
                    directiveNode.SelectDirective(0, tagLex.Tag, Comparer);

                    if (!directiveNode.CandidateDirectiveLockedIn)
                    {
                        Interpret(directiveNode);

                        if (!directiveNode.CandidateDirectiveLockedIn)
                        {
                            /* Expecting all child directives to have locked in. Otherwise they weren't closed! */
                            ExceptionHelper.UnmatchedDirectiveTag(
                                directiveNode.CandidateDirectives,
                                lex.FirstCharacterIndex);
                        }
                    }
                }
            }
        }
    }
}