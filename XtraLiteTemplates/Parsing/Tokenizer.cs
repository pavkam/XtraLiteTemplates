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
//
using System.Collections.Generic;

namespace XtraLiteTemplates.Parsing
{
    using System;
    using System.Text;
    using System.Linq;
    using System.Diagnostics;
    using System.IO;
using System.Globalization;

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

        public Char StringLiteralStartCharacter { get; private set; }

        public Char StringLiteralEndCharacter { get; private set; }

        public Char StringLiteralEscapeCharacter { get; private set; }

        public Char NumberLiteralDecimalSeparatorCharacter { get; private set; }

        private void InitializeTokenizer(TextReader reader, Char tagStartCharacter, Char tagEndCharacter,
            Char stringStartCharacter, Char stringEndCharacter, Char stringEscapeCharacter, Char numberDecimalSeparatorCharacter)
        {
            Expect.NotNull("reader", reader);
            Expect.NotEqual("tagStartCharacter", "tagEndCharacter", tagStartCharacter, tagEndCharacter);
            Expect.NotEqual("stringStartCharacter", "tagStartCharacter", stringStartCharacter, tagStartCharacter);
            Expect.NotEqual("stringStartCharacter", "tagEndCharacter", stringStartCharacter, tagEndCharacter);
            Expect.NotEqual("stringEndCharacter", "tagStartCharacter", stringEndCharacter, tagStartCharacter);
            Expect.NotEqual("stringEndCharacter", "tagEndCharacter", stringEndCharacter, tagEndCharacter);
            Expect.NotEqual("stringEscapeCharacter", "stringStartCharacter", stringEscapeCharacter, stringStartCharacter);
            Expect.NotEqual("stringEscapeCharacter", "stringEndCharacter", stringEscapeCharacter, stringEndCharacter);
            Expect.NotEqual("tagStartCharacter", "numberDecimalSeparatorCharacter", tagStartCharacter, numberDecimalSeparatorCharacter);
            Expect.NotEqual("tagEndCharacter", "numberDecimalSeparatorCharacter", tagEndCharacter, numberDecimalSeparatorCharacter);
            Expect.NotEqual("stringStartCharacter", "numberDecimalSeparatorCharacter", stringStartCharacter, numberDecimalSeparatorCharacter);
            Expect.NotEqual("stringEndCharacter", "numberDecimalSeparatorCharacter", stringEndCharacter, numberDecimalSeparatorCharacter);

            /* Validate allow character set. */
            Char[] all = new char[]
            { 
                tagStartCharacter, tagEndCharacter, stringStartCharacter,
                stringEndCharacter, stringEscapeCharacter, numberDecimalSeparatorCharacter
            };

            var allowedCharacterSet = !all.Any(c => Char.IsWhiteSpace(c) || Char.IsLetterOrDigit(c) || c == '_');
            Expect.IsTrue("allowed set of characters", allowedCharacterSet);

            this.m_textReader = reader;
            this.TagStartCharacter = tagStartCharacter;
            this.TagEndCharacter = tagEndCharacter;

            this.StringLiteralStartCharacter = stringStartCharacter;
            this.StringLiteralEndCharacter = stringEndCharacter;
            this.StringLiteralEscapeCharacter = stringEscapeCharacter;

            this.NumberLiteralDecimalSeparatorCharacter = numberDecimalSeparatorCharacter;

            this.m_parserState = ParserState.InText;
            this.m_currentCharacterIndex = -1;
        }

        public Tokenizer(TextReader reader, Char tagStartCharacter, Char tagEndCharacter, Char stringStartCharacter, 
            Char stringEndCharacter, Char stringEscapeCharacter, Char numberDecimalSeparatorCharacter)
        {
            InitializeTokenizer(reader, tagStartCharacter, tagEndCharacter,
                stringStartCharacter, stringEndCharacter, stringEscapeCharacter, numberDecimalSeparatorCharacter);
        }

        public Tokenizer(TextReader reader)            
        {
            InitializeTokenizer(reader, '{', '}', '"', '"', '\\', '.');
        }

        public Tokenizer(String text)
            : this(new StringReader(text ?? String.Empty))
        {
        }

        private TextReader m_textReader;
        private Int32 m_currentCharacterIndex;
        private Boolean m_isEndOfStream;
        private Char m_currentCharacter;

        private Char PeekCharacter()
        {
            if (this.m_isEndOfStream)
                ExceptionHelper.UnexpectedEndOfStream(this.m_currentCharacterIndex);

            var peekedCharacter = this.m_textReader.Peek();
            if (peekedCharacter == -1)
            {
                this.m_isEndOfStream = true;
                ExceptionHelper.UnexpectedEndOfStream(this.m_currentCharacterIndex + 1);
            }
            else
                return (Char)peekedCharacter;

            Debug.Fail("PeekCharacter()");
            return '\0';
        }

        private Boolean NextCharacter(Boolean required)
        {
            if (this.m_isEndOfStream)
                ExceptionHelper.UnexpectedEndOfStream(this.m_currentCharacterIndex);

            var readCharacter = this.m_textReader.Read();
            this.m_currentCharacterIndex++;
            if (readCharacter == -1)
            {
                this.m_isEndOfStream = true;
                if (required)
                    ExceptionHelper.UnexpectedEndOfStream(this.m_currentCharacterIndex);
            }
            else
                this.m_currentCharacter = (Char)readCharacter;

            return !this.m_isEndOfStream;
        }


        private Boolean IsTagSpecialCharacter(Char c)
        {
            return
                c == TagStartCharacter || c == TagEndCharacter || 
                c == StringLiteralStartCharacter || c == StringLiteralEndCharacter;
        }

        private Boolean IsStandardSymbol(Char c)
        {
            if (IsTagSpecialCharacter(c))
                return false;
            else
            {
                var unicodeCategory = Char.GetUnicodeCategory(c);
                return
                    (unicodeCategory == UnicodeCategory.OtherPunctuation ||
                     unicodeCategory == UnicodeCategory.CurrencySymbol ||
                     unicodeCategory == UnicodeCategory.DashPunctuation ||
                     unicodeCategory == UnicodeCategory.ModifierSymbol ||
                     unicodeCategory == UnicodeCategory.MathSymbol);
            }
        }

        private Token ReadNextInternal()
        {
            if (m_currentCharacterIndex == -1)
                NextCharacter(false);

            if (this.m_isEndOfStream && this.m_parserState != ParserState.InTag)
                return null;

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

                if (this.m_currentCharacter == this.StringLiteralStartCharacter)
                {
                    /* String constant start character. Read all the way until the matching one is found (or escape!) */
                    while (true)
                    {
                        this.NextCharacter(true);

                        if (this.m_currentCharacter == this.StringLiteralEndCharacter)
                        {
                            /* End of string. */
                            this.NextCharacter(true);
                            break;
                        }

                        if (this.m_currentCharacter == this.StringLiteralEscapeCharacter)
                        {
                            /* Escape character. */
                            this.NextCharacter(true);

                            if (m_currentCharacter == this.StringLiteralEscapeCharacter)
                            {
                                tokenValue.Append(this.StringLiteralEscapeCharacter);
                            }
                            else if (m_currentCharacter == this.StringLiteralEndCharacter)
                            {
                                tokenValue.Append(this.StringLiteralEndCharacter);
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

                    if (this.m_currentCharacter == this.NumberLiteralDecimalSeparatorCharacter && Char.IsDigit(PeekCharacter()))
                    {
                        /* This is a Decimal point. Read the remaining bits. */
                        tokenValue.Append(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                        this.NextCharacter(true);

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
                else if (this.m_currentCharacter == this.NumberLiteralDecimalSeparatorCharacter)
                {
                    /* Dot-prefixed number may have started. */
                    var dot = this.m_currentCharacter;
                    this.NextCharacter(true);

                    if (Char.IsDigit(this.m_currentCharacter))
                    {
                        tokenValue.Append(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);

                        /* Yes, this is a number! */
                        while (Char.IsDigit(this.m_currentCharacter))
                        {
                            tokenValue.Append(this.m_currentCharacter);
                            this.NextCharacter(true);
                        }

                        return new Token(
                            Token.TokenType.Number,
                            tokenValue.ToString(),
                            tokenStartIndex,
                            this.m_currentCharacterIndex - tokenStartIndex);
                    }
                    else
                    {
                        tokenValue.Append(dot);

                        /* Lump toghether all "standard symbols" (if this one was a standard one). */
                        while (this.IsStandardSymbol(this.m_currentCharacter))
                        {
                            tokenValue.Append(this.m_currentCharacter);
                            this.NextCharacter(true);
                        }

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
                else if (!this.IsTagSpecialCharacter(this.m_currentCharacter))
                {
                    /* Check if this is a "standard symbol" */
                    var isStandardSymbol = this.IsStandardSymbol(this.m_currentCharacter);

                    /* Add the symbol to the token value. */
                    tokenValue.Append(this.m_currentCharacter);
                    this.NextCharacter(true);

                    if (isStandardSymbol)
                    {
                        /* Lump toghether all "standard symbols" (if this one was a standard one). */
                        while (this.IsStandardSymbol(this.m_currentCharacter))
                        {
                            if (this.m_currentCharacter == NumberLiteralDecimalSeparatorCharacter)
                            {
                                /* Decimal separator. Special case. Peek the next char to see if its a digit. */
                                if (Char.IsDigit(PeekCharacter()))
                                    break;
                            }

                            tokenValue.Append(this.m_currentCharacter);
                            this.NextCharacter(true);
                        }
                    }

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
