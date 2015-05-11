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

namespace XtraLiteTemplates.ObjectModel
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using System.Text;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Parsing;
    using System.IO;

    public sealed class Interpreter
    {
        private List<Directive> m_directives;
        private Lexer m_lexer;

        public IEqualityComparer<String> Comparer
        {
            get
            {
                return m_lexer.Comparer;
            }
        }

        private void InitializeInterpreter(ITokenizer tokenizer, IEqualityComparer<String> comparer)
        {
            m_lexer = new Lexer(tokenizer, comparer);
            m_directives = new List<Directive>();
        }

        public Interpreter(ITokenizer tokenizer, IEqualityComparer<String> comparer)
        {
            Expect.NotNull("tokenizer", tokenizer);
            Expect.NotNull("comparer", comparer);

            InitializeInterpreter(tokenizer, comparer);
        }

        public Interpreter(TextReader reader, IEqualityComparer<String> comparer)
        {
            Expect.NotNull("reader", reader);
            Expect.NotNull("comparer", comparer);

            InitializeInterpreter(new Tokenizer(reader), comparer);
        }

        public Interpreter(String text, IEqualityComparer<String> comparer)
        {
            Expect.NotNull("text", text);
            Expect.NotNull("comparer", comparer);

            InitializeInterpreter(new Tokenizer(new StringReader(text)), comparer);
        }


        public Interpreter RegisterDirective(Directive directive)
        {
            Expect.NotNull("directive", directive);
            if (directive.Tags.Count == 0)
                ExceptionHelper.CannotRegisterDirectiveWithNoTags();

            if (!m_directives.Contains(directive))
            {
                foreach (var tag in directive.Tags)
                    m_lexer.RegisterTag(tag);

                m_directives.Add(directive);
            }

            return this;
        }

        public Interpreter RegisterOperator(Operator @operator)
        {
            m_lexer.RegisterOperator(@operator);
            return this;
        }


        private void Interpret(CompositeNode compositeNode)
        {
            Debug.Assert(compositeNode != null);
            var matchIndex = 1;

            while (true)
            {
                /* Read the next lex out of the lexer. */
                var lex = this.m_lexer.ReadNext();
                if (lex == null)
                    break;

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
                        directiveCompositeNode.Directive.Tags.Count > matchIndex &&
                        directiveCompositeNode.Directive.Tags[matchIndex] == tagLex.Tag)
                    {
                        /* OK, this is the N'th part of the current directive. */
                        matchIndex++;
                        compositeNode.AddChild(new TagNode(compositeNode as DirectiveNode, tagLex));

                        if (matchIndex == directiveCompositeNode.Directive.Tags.Count)
                            return;
                    }
                    else
                    {
                        /* Nothing matched */
                        var firstMatched = m_directives.FirstOrDefault(p => p.Tags[0] == tagLex.Tag);
                        Debug.Assert(firstMatched != null);

                        var directiveNode = new DirectiveNode(compositeNode, firstMatched);
                        compositeNode.AddChild(directiveNode);
                        directiveNode.AddChild(new TagNode(directiveNode, tagLex));

                        if (firstMatched.Tags.Count > 0)
                            Interpret(directiveNode);
                    }
                }
            }
        }

        public TemplateDocument ConstructDocument()
        {
            var document = new TemplateDocument();
            Interpret(document);

            return document;
        }
    }
}

