﻿//  Author:
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
namespace XtraLiteTemplates.Evaluation
{
    using System;
    using System.IO;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using System.Text;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Parsing;

    /// <summary>
    /// Provides <c>lex</c> interpretation facilities. Instances of this class are used to interpret
    /// sequences of <see cref="Lex" /> objects and assemble the final <see cref="IEvaluable" /> objects.
    /// </summary>
    public sealed class Interpreter
    {
        private List<Directive> m_directives;
        private Lexer m_lexer;

        private void Interpret(CompositeNode compositeNode)
        {
            Debug.Assert(compositeNode != null);
            var matchIndex = 1;

            while (true)
            {
                /* Read the next lex out of the lexer. */
                var lex = this.m_lexer.ReadNext();
                if (lex == null)
                {
                    break;
                }

                var unparsedLex = lex as UnparsedLex;
                if (unparsedLex != null)
                {
                    /* This is an unparsed lex. */
                    compositeNode.AddChild(new UnparsedNode(compositeNode, unparsedLex));
                }
                else
                {
                    /* This can only be a tag lex. */
                    var tagLex = lex as TagLex;
                    Debug.Assert(tagLex != null);

                    var directiveCompositeNode = compositeNode as DirectiveNode;
                    if (directiveCompositeNode != null &&
                        directiveCompositeNode.SelectDirective(matchIndex, tagLex.Tag, Comparer))
                    {
                        /* OK, this is the N'th part of the current directive. */
                        matchIndex++;
                        compositeNode.AddChild(new TagNode(directiveCompositeNode, tagLex));

                        if (directiveCompositeNode.CandidateDirectiveLockedIn)
                        {
                            break;
                        }
                        else
                        {
                            continue;
                        }
                    }

                    /* Match any directive that starts with this tag. */
                    var candidateDirectives = m_directives.Where(p => p.Tags[0].Equals(tagLex.Tag, Comparer)).ToArray();
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
                            ExceptionHelper.UnmatchedDirectiveTag(directiveNode.CandidateDirectives, lex.FirstCharacterIndex);
                        }
                    }
                }
            }
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
        public IEqualityComparer<string> Comparer
        {
            get
            {
                return m_lexer.Comparer;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Interpreter"/> class.
        /// </summary>
        /// <param name="tokenizer">The <c>tokenizer</c> instance.</param>
        /// <param name="expressionFlowSymbols">The expression flow symbols.</param>
        /// <param name="comparer">The keyword and identifier comparer.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="tokenizer"/> or <paramref name="expressionFlowSymbols"/> or <paramref name="comparer"/> is <c>null</c>.</exception>
        public Interpreter(ITokenizer tokenizer, ExpressionFlowSymbols expressionFlowSymbols, IEqualityComparer<string> comparer)
        {
            Expect.NotNull("tokenizer", tokenizer);
            Expect.NotNull("comparer", comparer);
            Expect.NotNull("expressionFlowSymbols", expressionFlowSymbols);

            m_lexer = new Lexer(tokenizer, expressionFlowSymbols, comparer);
            m_directives = new List<Directive>();
        }

        /// <summary>
        /// Registers a directive with this interpreter instance.
        /// </summary>
        /// <param name="directive">The directive to register.</param>
        /// <returns>This interpreter instance.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="directive"/> is <c>null</c>.</exception>
        public Interpreter RegisterDirective(Directive directive)
        {
            Expect.NotNull("directive", directive);
            Debug.Assert(directive.Tags.Any());

            if (!m_directives.Contains(directive))
            {
                foreach (var tag in directive.Tags)
                {
                    m_lexer.RegisterTag(tag);
                }

                m_directives.Add(directive);
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
        public Interpreter RegisterOperator(Operator @operator)
        {
            m_lexer.RegisterOperator(@operator);
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
        public Interpreter RegisterSpecial(string keyword, object value)
        {
            m_lexer.RegisterSpecial(keyword, value);
            return this;
        }

        /// <summary>
        /// Compiles a template supplied by the <see cref="ITokenizer"/> instance offered at construction time.
        /// </summary>
        /// <returns>An evaluable object.</returns>
        public IEvaluable Construct()
        {
            var document = new TemplateDocument();
            Interpret(document);

            return document;
        }
    }
}