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
namespace XtraLiteTemplates
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
            InDirective,
        }

        private ParserState m_parserState;

        public Char DirectiveStartCharacter { get; private set; }

        public Char DirectiveEndCharacter { get; private set; }

        public Char StringStartCharacter { get; private set; }

        public Char StringEndCharacter { get; private set; }

        public Char StringEscapeCharacter { get; private set; }

        public Tokenizer(TextReader reader, 
                         Char directiveStartCharacter, 
                         Char directiveEndCharacter, 
                         Char stringStartCharacter, 
                         Char stringEndCharacter, 
                         Char stringEscapeCharacter)
        {
            Expect.NotNull("reader", reader);
            Expect.NotEqual("directiveStartCharacter", "directiveEndCharacter", directiveStartCharacter, directiveEndCharacter);
            Expect.NotEqual("stringStartCharacter", "directiveStartCharacter", stringStartCharacter, directiveStartCharacter);
            Expect.NotEqual("stringStartCharacter", "directiveEndCharacter", stringStartCharacter, directiveEndCharacter);
            Expect.NotEqual("stringEndCharacter", "directiveStartCharacter", stringEndCharacter, directiveStartCharacter);
            Expect.NotEqual("stringEndCharacter", "directiveEndCharacter", stringEndCharacter, directiveEndCharacter);
            Expect.NotEqual("stringEscapeCharacter", "stringStartCharacter", stringEscapeCharacter, stringStartCharacter);
            Expect.NotEqual("stringEscapeCharacter", "stringEndCharacter", stringEscapeCharacter, stringEndCharacter);

            /* Validate allow character set. */
            Char[] all = new char[]
            { 
                directiveStartCharacter, directiveEndCharacter, stringStartCharacter,
                stringEndCharacter, stringEscapeCharacter
            };

            var allowedCharacterSet = !all.Any(c => Char.IsLetterOrDigit(c) || c == '_' || c == '.');
            Expect.IsTrue("allowed set of characters", allowedCharacterSet);

            this.m_textReader = reader;
            this.DirectiveStartCharacter = directiveStartCharacter;
            this.DirectiveEndCharacter = directiveEndCharacter;

            this.StringStartCharacter = stringStartCharacter;
            this.StringEndCharacter = stringEndCharacter;
            this.StringEscapeCharacter = stringEscapeCharacter;

            this.m_parserState = ParserState.InText;

            this.m_currentCharacterIndex = 0;
            this.LoadCharacter();
        }

        public Tokenizer(TextReader reader)
            : this(reader, '{', '}', '"', '"', '\\')
        {
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
                ParseException.UnexpectedEndOfStream(this.m_currentCharacterIndex);
            }

            this.LoadCharacter();
            this.m_currentCharacterIndex++;

            if (this.m_isEndOfStream && required)
            {
                ParseException.UnexpectedEndOfStream(this.m_currentCharacterIndex);
            }

            return !this.m_isEndOfStream;
        }

        public Token ReadNext()
        {
            if (this.m_isEndOfStream && this.m_parserState != ParserState.InDirective)
            {
                return null;
            }

            StringBuilder tokenValue = new StringBuilder();
            Int32 tokenStartIndex = this.m_currentCharacterIndex;

            if (this.m_parserState == ParserState.InDirective && this.m_currentCharacter == this.DirectiveEndCharacter)
            {
                /* Directiove end character. Need to switch to text mode. */
                this.m_parserState = ParserState.InText;

                if (!this.NextCharacter(false))
                {
                    return null;
                }
            }
            else if (this.m_parserState == ParserState.InDirective &&
                     this.m_currentCharacter == this.DirectiveStartCharacter)
            {
                ParseException.UnexpectedCharacter(this.m_currentCharacterIndex, this.DirectiveStartCharacter);
            }

            if (this.m_parserState == ParserState.InText)
            {
                while (!this.m_isEndOfStream)
                {
                    /* Parsing free-form text until the start directive character is found. */
                    while (!this.m_isEndOfStream && this.m_currentCharacter != this.DirectiveStartCharacter)
                    {
                        tokenValue.Append(this.m_currentCharacter);
                        this.NextCharacter(false);
                    }

                    if (!this.m_isEndOfStream)
                    {
                        /* Found the { character. Read as many as we can and decide which are escapes. */
                        Int32 escapeSequenceStartIndex = this.m_currentCharacterIndex;
                        while (this.m_currentCharacter == this.DirectiveStartCharacter)
                        {
                            this.NextCharacter(true);
                        }

                        Int32 escapeLength = this.m_currentCharacterIndex - escapeSequenceStartIndex;
                        if (escapeLength > 1)
                        {
                            /* Append the escape value to the token. */
                            tokenValue.Append(this.DirectiveStartCharacter, escapeLength / 2);
                            escapeLength = escapeLength % 2;
                        }

                        if (escapeLength == 1)
                        {
                            /* Directive start. */
                            this.m_parserState = ParserState.InDirective;
                            break;
                        }
                    }
                }

                if (tokenValue.Length > 0)
                {
                    /* Check for extra in-directive character. */
                    var delta = this.m_parserState == ParserState.InDirective ? 1 : 0;

                    return new Token(
                        Token.TokenType.Unparsed, 
                        tokenValue.ToString(), 
                        tokenStartIndex, 
                        this.m_currentCharacterIndex - tokenStartIndex - delta);
                }
                else
                {
                    tokenStartIndex = this.m_currentCharacterIndex;
                }

                /* No text read only can happen when the first character is a start directive. */
                Debug.Assert(this.m_isEndOfStream || this.m_parserState == ParserState.InDirective);
            }

            if (this.m_parserState == ParserState.InDirective)
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
                                        ParseException.InvalidEscapeCharacter(this.m_currentCharacterIndex, this.m_currentCharacter);
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
                        Token.TokenType.Identifier, 
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
                            ParseException.UnexpectedCharacter(this.m_currentCharacterIndex, this.m_currentCharacter);
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
                else if (this.m_currentCharacter != DirectiveStartCharacter && this.m_currentCharacter != DirectiveEndCharacter)
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
