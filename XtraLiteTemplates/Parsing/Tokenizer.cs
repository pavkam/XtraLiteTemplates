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
using System.Collections.Generic;

namespace XtraLiteTemplates.Parsing
{
    using System;
    using System.Text;
    using System.Linq;
    using System.Diagnostics;
    using System.IO;

    public sealed class Tokenizer : ITokenizer, IDisposable
    {
        private enum ParserState
        {
            InText,
            InTag,
        }

        private ParserState m_parserState;

        public Char TagStartCharacter { get; private set; }

        public Char TagEndCharacter { get; private set; }

        public Char StringStartCharacter { get; private set; }

        public Char StringEndCharacter { get; private set; }

        public Char StringEscapeCharacter { get; private set; }

        private void InitializeTokenizer(TextReader reader, Char tagStartCharacter, Char tagEndCharacter, 
            Char stringStartCharacter, Char stringEndCharacter, Char stringEscapeCharacter)
        {
            Expect.NotNull("reader", reader);
            Expect.NotEqual("tagStartCharacter", "tagEndCharacter", tagStartCharacter, tagEndCharacter);
            Expect.NotEqual("stringStartCharacter", "tagStartCharacter", stringStartCharacter, tagStartCharacter);
            Expect.NotEqual("stringStartCharacter", "tagEndCharacter", stringStartCharacter, tagEndCharacter);
            Expect.NotEqual("stringEndCharacter", "tagStartCharacter", stringEndCharacter, tagStartCharacter);
            Expect.NotEqual("stringEndCharacter", "tagEndCharacter", stringEndCharacter, tagEndCharacter);
            Expect.NotEqual("stringEscapeCharacter", "stringStartCharacter", stringEscapeCharacter, stringStartCharacter);
            Expect.NotEqual("stringEscapeCharacter", "stringEndCharacter", stringEscapeCharacter, stringEndCharacter);

            /* Validate allow character set. */
            Char[] all = new char[]
            { 
                tagStartCharacter, tagEndCharacter, stringStartCharacter,
                stringEndCharacter, stringEscapeCharacter
            };

            var allowedCharacterSet = !all.Any(c => Char.IsWhiteSpace(c) || Char.IsLetterOrDigit(c) || c == '_' || c == '.');
            Expect.IsTrue("allowed set of characters", allowedCharacterSet);

            this.m_textReader = reader;
            this.TagStartCharacter = tagStartCharacter;
            this.TagEndCharacter = tagEndCharacter;

            this.StringStartCharacter = stringStartCharacter;
            this.StringEndCharacter = stringEndCharacter;
            this.StringEscapeCharacter = stringEscapeCharacter;

            this.m_parserState = ParserState.InText;

            this.m_currentCharacterIndex = 0;
            this.LoadCharacter();
        }

        public Tokenizer(TextReader reader, Char tagStartCharacter, Char tagEndCharacter, Char stringStartCharacter, 
            Char stringEndCharacter, Char stringEscapeCharacter)
        {
            InitializeTokenizer(reader, tagStartCharacter, tagEndCharacter, stringStartCharacter, stringEndCharacter, stringEscapeCharacter);
        }

        public Tokenizer(TextReader reader)            
        {
            InitializeTokenizer(reader, '{', '}', '"', '"', '\\');
        }

        public Tokenizer(String text)
            : this(new StringReader(text ?? String.Empty))
        {
        }

        private void LoadCharacter()
        {
            var c = this.m_textReader.Read();
            if (c == -1)
            {
                this.m_isEndOfStream = true;
            }
            else
            {
                this.m_currentCharacter = (Char)c;
            }
        }

        private TextReader m_textReader;
        private Int32 m_currentCharacterIndex;
        private Boolean m_isEndOfStream;
        private Char m_currentCharacter;

        private Boolean NextCharacter(Boolean required)
        {
            if (this.m_isEndOfStream)
            {
                ExceptionHelper.UnexpectedEndOfStream(this.m_currentCharacterIndex);
            }

            this.LoadCharacter();
            this.m_currentCharacterIndex++;

            if (this.m_isEndOfStream && required)
            {
                ExceptionHelper.UnexpectedEndOfStream(this.m_currentCharacterIndex);
            }

            return !this.m_isEndOfStream;
        }

        private Token ReadNextInternal()
        {
            if (this.m_isEndOfStream && this.m_parserState != ParserState.InTag)
            {
                return null;
            }

            var tokenValue = new StringBuilder();
            Int32 tokenStartIndex = this.m_currentCharacterIndex;

            if (this.m_parserState == ParserState.InTag && this.m_currentCharacter == this.TagEndCharacter)
            {
                /* Tag end character. Need to switch to text mode. */
                this.m_parserState = ParserState.InText;

                var _token = new Token(Token.TokenType.EndTag, 
                                 this.m_currentCharacter.ToString(), this.m_currentCharacterIndex, 1);

                this.NextCharacter(false);
                return _token;
            }
            else if (this.m_parserState == ParserState.InTag &&
                     this.m_currentCharacter == this.TagStartCharacter)
            {
                ExceptionHelper.UnexpectedCharacter(this.m_currentCharacterIndex, this.TagStartCharacter);
            }

            if (this.m_parserState == ParserState.InText)
            {
                tokenValue.Append(this.m_currentCharacter);

                if (this.m_currentCharacter == this.TagStartCharacter)
                {
                    /* Found { character. This might actually be an escape sequence. */
                    this.NextCharacter(true);
                    if (this.m_currentCharacter == this.TagStartCharacter)
                    {
                        this.NextCharacter(false);

                        return new Token(
                            Token.TokenType.Unparsed, 
                            tokenValue.ToString(), 
                            tokenStartIndex, 
                            this.m_currentCharacterIndex - tokenStartIndex);
                    }
                    else
                    {
                        this.m_parserState = ParserState.InTag;
                        return new Token(
                            Token.TokenType.StartTag, 
                            tokenValue.ToString(), 
                            tokenStartIndex,
                            this.m_currentCharacterIndex - tokenStartIndex);
                    }
                }
                else
                {
                    /* Parsing free-form text until the start directive character is found. */
                    this.NextCharacter(false);
                
                    while (!this.m_isEndOfStream && this.m_currentCharacter != this.TagStartCharacter)
                    {
                        tokenValue.Append(this.m_currentCharacter);
                        this.NextCharacter(false);
                    }

                    return new Token(
                        Token.TokenType.Unparsed, 
                        tokenValue.ToString(), 
                        tokenStartIndex, 
                        this.m_currentCharacterIndex - tokenStartIndex);
                }
            }

            if (this.m_parserState == ParserState.InTag)
            {
                /* Directive parsing is a diffent beast... */

                Debug.Assert(!this.m_isEndOfStream);

                if (this.m_currentCharacter == this.StringStartCharacter)
                {
                    /* String constant start character. Read all the way until the matching one is found (or escape!) */
                    while (true)
                    {
                        this.NextCharacter(true);

                        if (this.m_currentCharacter == this.StringEndCharacter)
                        {
                            /* End of string. */
                            this.NextCharacter(true);
                            break;
                        }

                        if (this.m_currentCharacter == this.StringEscapeCharacter)
                        {
                            /* Escape character. */
                            this.NextCharacter(true);

                            if (m_currentCharacter == this.StringEscapeCharacter)
                            {
                                tokenValue.Append(this.StringEscapeCharacter);
                            }
                            else if (m_currentCharacter == this.StringEndCharacter)
                            {
                                tokenValue.Append(this.StringEndCharacter);
                            }
                            else
                            {
                                switch (this.m_currentCharacter)
                                {
                                    case 'a':
                                        tokenValue.Append('\a');
                                        break;
                                    case 'b':
                                        tokenValue.Append('\b');
                                        break;
                                    case 'f':
                                        tokenValue.Append('\f');
                                        break;
                                    case 'r':
                                        tokenValue.Append('\r');
                                        break;
                                    case 'n':
                                        tokenValue.Append('\n');
                                        break;
                                    case 't':
                                        tokenValue.Append('\t');
                                        break;
                                    case 'v':
                                        tokenValue.Append('\v');
                                        break;
                                    case '\'':
                                        tokenValue.Append('\'');
                                        break;
                                    case '?':
                                        tokenValue.Append('?');
                                        break;
                                    default:
                                        ExceptionHelper.InvalidEscapeCharacter(this.m_currentCharacterIndex, this.m_currentCharacter);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            tokenValue.Append(this.m_currentCharacter);
                        }
                    }

                    return new Token(
                        Token.TokenType.String, 
                        tokenValue.ToString(), 
                        tokenStartIndex, 
                        this.m_currentCharacterIndex - tokenStartIndex);
                }
                else if (Char.IsLetter(this.m_currentCharacter) || this.m_currentCharacter == '_')
                {
                    /* Identifier/keyword token has started. */

                    while (Char.IsLetterOrDigit(this.m_currentCharacter) || this.m_currentCharacter == '_')
                    {
                        tokenValue.Append(this.m_currentCharacter);
                        this.NextCharacter(true);
                    }

                    Debug.Assert(tokenValue.Length > 0);
                    return new Token(
                        Token.TokenType.Word, 
                        tokenValue.ToString(), 
                        tokenStartIndex, 
                        this.m_currentCharacterIndex - tokenStartIndex);
                }
                else if (Char.IsDigit(this.m_currentCharacter))
                {
                    /* Number has started. */
                    while (Char.IsDigit(this.m_currentCharacter))
                    {
                        tokenValue.Append(this.m_currentCharacter);
                        this.NextCharacter(true);
                    }

                    if (this.m_currentCharacter == '.')
                    {
                        /* This is a Decimal point. Read the remaining bits. */
                        tokenValue.Append(this.m_currentCharacter);
                        this.NextCharacter(true);

                        if (!Char.IsDigit(this.m_currentCharacter))
                        {
                            ExceptionHelper.UnexpectedCharacter(this.m_currentCharacterIndex, this.m_currentCharacter);
                        }

                        while (Char.IsDigit(this.m_currentCharacter))
                        {
                            tokenValue.Append(this.m_currentCharacter);
                            this.NextCharacter(true);
                        }
                    }

                    Debug.Assert(tokenValue.Length > 0);
                    return new Token(
                        Token.TokenType.Number, 
                        tokenValue.ToString(), 
                        tokenStartIndex, 
                        this.m_currentCharacterIndex - tokenStartIndex);
                }
                else if (this.m_currentCharacter == '.')
                {
                    /* dot-prefixed number may have started. */
                    tokenValue.Append(this.m_currentCharacter);
                    this.NextCharacter(true);

                    while (Char.IsDigit(this.m_currentCharacter))
                    {
                        tokenValue.Append(this.m_currentCharacter);
                        this.NextCharacter(true);
                    }

                    Debug.Assert(tokenValue.Length > 0);

                    if (tokenValue.Length > 1)
                    {
                        return new Token(
                            Token.TokenType.Number, 
                            tokenValue.ToString(), 
                            tokenStartIndex, 
                            this.m_currentCharacterIndex - tokenStartIndex);
                    }
                    else
                    {
                        return new Token(
                            Token.TokenType.Symbol, 
                            tokenValue.ToString(), 
                            tokenStartIndex, 
                            this.m_currentCharacterIndex - tokenStartIndex);
                    }
                }
                else if (Char.IsWhiteSpace(this.m_currentCharacter))
                {
                    /* Whitespace... */
                    while (Char.IsWhiteSpace(this.m_currentCharacter))
                    {
                        tokenValue.Append(this.m_currentCharacter);
                        this.NextCharacter(true);
                    }

                    Debug.Assert(tokenValue.Length > 0);

                    return new Token(
                        Token.TokenType.Whitespace, 
                        tokenValue.ToString(), 
                        tokenStartIndex, 
                        this.m_currentCharacterIndex - tokenStartIndex);
                }
                else if (this.m_currentCharacter != TagStartCharacter && this.m_currentCharacter != TagEndCharacter)
                {
                    /* Symbol character (unknown) detected. */
                    tokenValue.Append(this.m_currentCharacter);
                    this.NextCharacter(true);

                    Debug.Assert(tokenValue.Length > 0);

                    return new Token(
                        Token.TokenType.Symbol, 
                        tokenValue.ToString(), 
                        tokenStartIndex, 
                        this.m_currentCharacterIndex - tokenStartIndex);
                }
            }

            return null;
        }

        public Token ReadNext()
        {
            return ReadNextInternal();
        }

        #region IDisposable implementation

        ~Tokenizer ()
        {
            this.Dispose(false);
        }

        public void Dispose(Boolean disposing)
        {
            if (disposing)
            {
                this.m_textReader.Dispose();
            }
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        #endregion
    }
}
