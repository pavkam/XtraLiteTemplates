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
using System;
using XtraLiteTemplates.Parsing;
using System.Collections.Generic;

namespace XtraLiteTemplates.Parsing
{
    public class Lexer
    {
        private IList<Tag[]> m_directives;

        public ITokenizer Tokenizer { get; private set; }

        public IEqualityComparer<String> Comparer { get; private set; }

        public Lexer(ITokenizer tokenizer, IEqualityComparer<String> comparer)
        {
            Expect.NotNull("tokenizer", tokenizer);
            Expect.NotNull("comparer", comparer);

            Tokenizer = tokenizer;
            Comparer = comparer;

            m_directives = new List<Tag[]>();
        }

        public Lexer RegisterDirective(params Tag[] tags)
        {
            Expect.NotEmpty("tags", tags);

            m_directives.Add(tags);

            return this;
        }

        public String ReadNext()
        {
            while (true)
            {
                /* Load up the next token and see what it is. */
                var token = Tokenizer.ReadNext();
                if (token == null)
                    break;

                if (token.Type == Token.TokenType.Unparsed)
                {
                    /* Unparsed text block. Pass it straign away */
                }
            }

            return null;
        }
    }
}

