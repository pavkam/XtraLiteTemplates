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
using System.Linq;
using System.Diagnostics;

namespace XtraLiteTemplates.Parsing
{
    public class Lexer
    {
        private IList<Tag> m_tags;
        private Token m_currentToken;
        private Boolean m_isEndOfStream;

        public ITokenizer Tokenizer { get; private set; }

        public IEqualityComparer<String> Comparer { get; private set; }

        public Lexer(ITokenizer tokenizer, IEqualityComparer<String> comparer)
        {
            Expect.NotNull("tokenizer", tokenizer);
            Expect.NotNull("comparer", comparer);

            Tokenizer = tokenizer;
            Comparer = comparer;

            m_tags = new List<Tag>();
        }

        public Lexer RegisterTag(Tag tag)
        {
            Expect.NotNull("tag", tag);

            m_tags.Add(tag);

            return this;
        }

        private Boolean NextToken()
        {
            if (this.m_isEndOfStream)
                return false;

            this.m_currentToken = this.Tokenizer.ReadNext();
            if (this.m_currentToken == null)
                this.m_isEndOfStream = true;

            return this.m_isEndOfStream;
        }

        public String ReadNext()
        {
            if (this.m_currentToken == null && !this.m_isEndOfStream)
                this.NextToken();

            if (this.m_isEndOfStream)
                return null;

            /* Load all unparsed tokens and merge them into a big component. */
            var unparsedTokens = new List<Token>();
            while (!this.m_isEndOfStream && this.m_currentToken.Type == Token.TokenType.Unparsed)
            {
                unparsedTokens.Add(this.m_currentToken);
                this.NextToken();
            }

            if (unparsedTokens.Count > 0)
            {
                /* Shit hoser. */
                return null;
            }

            /* This is where a tag is parsed. */
            if (this.m_currentToken.Type != Token.TokenType.StartTag)
                ParseException.UnexpectedToken(this.m_currentToken);

            Int32 tokenIndex = -1;
            var matchingTags = new HashSet<Tag>();
            while (true)
            {
                tokenIndex++;
                var _prev = this.m_currentToken;
                if (!this.NextToken())
                    ParseException.UnexpectedEndOfStreamAfterToken(_prev);
                else if (this.m_currentToken.Type == Token.TokenType.EndTag)
                {
                    if (tokenIndex == 0)
                        ParseException.UnexpectedToken(this.m_currentToken);
                    else
                        break;
                }

                /* We have the new token here. Filter through tags. */
                //   if (this.m_currentToken.Type == Token.TokenType.)
                // matchingTags.RemoveWhere(k => k.MatchesKeyword);
            }

            return null;
        }
    }
}

