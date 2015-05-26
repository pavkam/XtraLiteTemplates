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

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Does not apply.")]

namespace XtraLiteTemplates.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    
    /// <summary>
    /// Provides the standard tokenization services.
    /// </summary>
    public sealed class Tokenizer : ITokenizer, IDisposable
    {
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private bool parsingTag;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private TextReader inputTextReader;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private int currentCharacterIndex;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private bool endOfStream;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private char currentCharacter;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer"/> class.
        /// </summary>
        /// <param name="reader">The input template reader.</param>
        /// <param name="tagStartCharacter">The tag start character.</param>
        /// <param name="tagEndCharacter">The tag end character.</param>
        /// <param name="stringStartCharacter">The string literal start character.</param>
        /// <param name="stringEndCharacter">The string literal end character.</param>
        /// <param name="stringEscapeCharacter">The string literal escape character.</param>
        /// <param name="numberDecimalSeparatorCharacter">The number literal decimal separator character.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="reader"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">The supplied character combination is not valid.</exception>
        /// <exception cref="InvalidOperationException">One or more control characters is not allowed.</exception>
        public Tokenizer(
            TextReader reader, 
            char tagStartCharacter, 
            char tagEndCharacter, 
            char stringStartCharacter,
            char stringEndCharacter, 
            char stringEscapeCharacter, 
            char numberDecimalSeparatorCharacter)
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
            char[] all = new char[]
            { 
                tagStartCharacter, tagEndCharacter, stringStartCharacter,
                stringEndCharacter, stringEscapeCharacter, numberDecimalSeparatorCharacter
            };

            var allowedCharacterSet = !all.Any(c => char.IsWhiteSpace(c) || char.IsLetterOrDigit(c) || c == '_');
            Expect.IsTrue("allowed set of characters", allowedCharacterSet);

            this.inputTextReader = reader;
            this.TagStartCharacter = tagStartCharacter;
            this.TagEndCharacter = tagEndCharacter;

            this.StringLiteralStartCharacter = stringStartCharacter;
            this.StringLiteralEndCharacter = stringEndCharacter;
            this.StringLiteralEscapeCharacter = stringEscapeCharacter;

            this.NumberLiteralDecimalSeparatorCharacter = numberDecimalSeparatorCharacter;

            this.parsingTag = false;
            this.currentCharacterIndex = -1;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer"/> class.
        /// <remarks>
        /// <para>
        /// Standard character set is used for control characters: '{', '}', '"' and '.'.
        /// </para>
        /// </remarks>
        /// </summary>
        /// <param name="reader">The input template reader.</param>
        public Tokenizer(TextReader reader)
            : this(reader, '{', '}', '"', '"', '\\', '.')         
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Tokenizer"/> class.
        /// <remarks>
        /// <para>
        /// Standard character set is used for control characters: '{', '}', '"' and '.'.
        /// </para>
        /// </remarks>
        /// </summary>
        /// <param name="text">The input template.</param>
        public Tokenizer(string text)
            : this(new StringReader(text ?? string.Empty))
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Tokenizer"/> class.
        /// </summary>
        ~Tokenizer()
        {
            this.Dispose();
        }

        /// <summary>
        /// Gets the tag start character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The tag start character.
        /// </value>
        public char TagStartCharacter { get; private set; }

        /// <summary>
        /// Gets the tag end character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The tag end character.
        /// </value>
        public char TagEndCharacter { get; private set; }

        /// <summary>
        /// Gets the string literal start character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The string literal start character.
        /// </value>
        public char StringLiteralStartCharacter { get; private set; }

        /// <summary>
        /// Gets the string literal end character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The string literal end character.
        /// </value>
        public char StringLiteralEndCharacter { get; private set; }

        /// <summary>
        /// Gets the string literal escape character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The string literal escape character.
        /// </value>
        public char StringLiteralEscapeCharacter { get; private set; }

        /// <summary>
        /// Gets the number literal decimal separator character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The number literal decimal separator character.
        /// </value>
        public char NumberLiteralDecimalSeparatorCharacter { get; private set; }

        /// <summary>
        /// Reads the next <see cref="Token" /> object from the input template.
        /// </summary>
        /// <returns>
        /// The next read token; or <c>null</c> if the end of template was reached.
        /// </returns>
        public Token ReadNext()
        {
            return this.ReadNextInternal();
        }

        /// <summary>
        /// Disposes the input template reader.
        /// </summary>
        public void Dispose()
        {
            if (this.inputTextReader != null)
            {
                this.inputTextReader.Dispose();
            }

            GC.SuppressFinalize(this);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private char PeekCharacter()
        {
            if (this.endOfStream)
            {
                ExceptionHelper.UnexpectedEndOfStream(this.currentCharacterIndex);
            }

            var peekedCharacter = this.inputTextReader.Peek();
            if (peekedCharacter == -1)
            {
                this.endOfStream = true;
                ExceptionHelper.UnexpectedEndOfStream(this.currentCharacterIndex + 1);
            }
            else
            {
                return (char)peekedCharacter;
            }

            Debug.Fail("PeekCharacter()");
            return '\0';
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private bool NextCharacter(bool required)
        {
            if (this.endOfStream)
            {
                ExceptionHelper.UnexpectedEndOfStream(this.currentCharacterIndex);
            }

            var readCharacter = this.inputTextReader.Read();
            this.currentCharacterIndex++;
            if (readCharacter == -1)
            {
                this.endOfStream = true;
                if (required)
                {
                    ExceptionHelper.UnexpectedEndOfStream(this.currentCharacterIndex);
                }
            }
            else
            {
                this.currentCharacter = (char)readCharacter;
            }

            return !this.endOfStream;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private bool IsTagSpecialCharacter(char c)
        {
            return
                c == this.TagStartCharacter || c == this.TagEndCharacter ||
                c == this.StringLiteralStartCharacter || c == this.StringLiteralEndCharacter;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private bool IsStandardSymbol(char c)
        {
            if (this.IsTagSpecialCharacter(c))
            {
                return false;
            }
            else
            {
                var unicodeCategory = char.GetUnicodeCategory(c);
                return
                    unicodeCategory == UnicodeCategory.OtherPunctuation ||
                    unicodeCategory == UnicodeCategory.CurrencySymbol ||
                    unicodeCategory == UnicodeCategory.DashPunctuation ||
                    unicodeCategory == UnicodeCategory.ModifierSymbol ||
                    unicodeCategory == UnicodeCategory.MathSymbol;
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private Token ReadNextInternal()
        {
            if (this.currentCharacterIndex == -1)
            {
                this.NextCharacter(false);
            }

            if (this.endOfStream && !this.parsingTag)
            {
                return null;
            }

            var tokenValue = new StringBuilder();
            var tokenStartIndex = this.currentCharacterIndex;

            if (this.parsingTag && this.currentCharacter == this.TagEndCharacter)
            {
                /* Tag end character. Need to switch to text mode. */
                this.parsingTag = false;

                var endTagToken = new Token(Token.TokenType.EndTag, this.currentCharacter.ToString(), this.currentCharacterIndex, 1);

                this.NextCharacter(false);
                return endTagToken;
            }
            else if (this.parsingTag && this.currentCharacter == this.TagStartCharacter)
            {
                ExceptionHelper.UnexpectedCharacter(this.currentCharacterIndex, this.TagStartCharacter);
            }

            if (!this.parsingTag)
            {
                tokenValue.Append(this.currentCharacter);

                if (this.currentCharacter == this.TagStartCharacter)
                {
                    /* Found { character. This might actually be an escape sequence. */
                    this.NextCharacter(true);
                    if (this.currentCharacter == this.TagStartCharacter)
                    {
                        this.NextCharacter(false);

                        return new Token(
                            Token.TokenType.Unparsed,
                            tokenValue.ToString(),
                            tokenStartIndex,
                            this.currentCharacterIndex - tokenStartIndex);
                    }
                    else
                    {
                        this.parsingTag = true;

                        return new Token(
                            Token.TokenType.StartTag,
                            tokenValue.ToString(),
                            tokenStartIndex,
                            this.currentCharacterIndex - tokenStartIndex);
                    }
                }
                else
                {
                    /* Parsing free-form text until the start directive character is found. */
                    this.NextCharacter(false);

                    while (!this.endOfStream && this.currentCharacter != this.TagStartCharacter)
                    {
                        tokenValue.Append(this.currentCharacter);
                        this.NextCharacter(false);
                    }

                    return new Token(
                        Token.TokenType.Unparsed,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        this.currentCharacterIndex - tokenStartIndex);
                }
            }

            if (this.parsingTag)
            {
                /* Directive parsing is a diffent beast... */

                Debug.Assert(!this.endOfStream, "Must not be end-of-stream.");

                if (this.currentCharacter == this.StringLiteralStartCharacter)
                {
                    /* String constant start character. Read all the way until the matching one is found (or escape!) */
                    while (true)
                    {
                        this.NextCharacter(true);

                        if (this.currentCharacter == this.StringLiteralEndCharacter)
                        {
                            /* End of string. */
                            this.NextCharacter(true);
                            break;
                        }

                        if (this.currentCharacter == this.StringLiteralEscapeCharacter)
                        {
                            /* Escape character. */
                            this.NextCharacter(true);

                            if (this.currentCharacter == this.StringLiteralEscapeCharacter)
                            {
                                tokenValue.Append(this.StringLiteralEscapeCharacter);
                            }
                            else if (this.currentCharacter == this.StringLiteralEndCharacter)
                            {
                                tokenValue.Append(this.StringLiteralEndCharacter);
                            }
                            else
                            {
                                switch (this.currentCharacter)
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
                                        ExceptionHelper.InvalidEscapeCharacter(this.currentCharacterIndex, this.currentCharacter);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            tokenValue.Append(this.currentCharacter);
                        }
                    }

                    return new Token(
                        Token.TokenType.String,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        this.currentCharacterIndex - tokenStartIndex);
                }
                else if (char.IsLetter(this.currentCharacter) || this.currentCharacter == '_')
                {
                    /* Identifier/keyword token has started. */

                    while (char.IsLetterOrDigit(this.currentCharacter) || this.currentCharacter == '_')
                    {
                        tokenValue.Append(this.currentCharacter);
                        this.NextCharacter(true);
                    }

                    Debug.Assert(tokenValue.Length > 0, "Expected non-empty token value.");
                    return new Token(
                        Token.TokenType.Word,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        this.currentCharacterIndex - tokenStartIndex);
                }
                else if (char.IsDigit(this.currentCharacter))
                {
                    /* Number has started. */
                    while (char.IsDigit(this.currentCharacter))
                    {
                        tokenValue.Append(this.currentCharacter);
                        this.NextCharacter(true);
                    }

                    if (this.currentCharacter == this.NumberLiteralDecimalSeparatorCharacter && char.IsDigit(this.PeekCharacter()))
                    {
                        /* This is a Decimal point. Read the remaining bits. */
                        tokenValue.Append(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                        this.NextCharacter(true);

                        while (char.IsDigit(this.currentCharacter))
                        {
                            tokenValue.Append(this.currentCharacter);
                            this.NextCharacter(true);
                        }
                    }

                    Debug.Assert(tokenValue.Length > 0, "Expected non-empty token value.");
                    return new Token(
                        Token.TokenType.Number,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        this.currentCharacterIndex - tokenStartIndex);
                }
                else if (this.currentCharacter == this.NumberLiteralDecimalSeparatorCharacter)
                {
                    /* Dot-prefixed number may have started. */
                    var dot = this.currentCharacter;
                    this.NextCharacter(true);

                    if (char.IsDigit(this.currentCharacter))
                    {
                        tokenValue.Append(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);

                        /* Yes, this is a number! */
                        while (char.IsDigit(this.currentCharacter))
                        {
                            tokenValue.Append(this.currentCharacter);
                            this.NextCharacter(true);
                        }

                        return new Token(
                            Token.TokenType.Number,
                            tokenValue.ToString(),
                            tokenStartIndex,
                            this.currentCharacterIndex - tokenStartIndex);
                    }
                    else
                    {
                        tokenValue.Append(dot);

                        /* Lump toghether all "standard symbols" (if this one was a standard one). */
                        while (this.IsStandardSymbol(this.currentCharacter))
                        {
                            tokenValue.Append(this.currentCharacter);
                            this.NextCharacter(true);
                        }

                        return new Token(
                            Token.TokenType.Symbol,
                            tokenValue.ToString(),
                            tokenStartIndex,
                            this.currentCharacterIndex - tokenStartIndex);
                    }
                }
                else if (char.IsWhiteSpace(this.currentCharacter))
                {
                    /* Whitespace... */
                    while (char.IsWhiteSpace(this.currentCharacter))
                    {
                        tokenValue.Append(this.currentCharacter);
                        this.NextCharacter(true);
                    }

                    Debug.Assert(tokenValue.Length > 0, "Expected non-empty token value.");

                    return new Token(
                        Token.TokenType.Whitespace,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        this.currentCharacterIndex - tokenStartIndex);
                }
                else if (!this.IsTagSpecialCharacter(this.currentCharacter))
                {
                    /* Check if this is a "standard symbol" */
                    var isStandardSymbol = this.IsStandardSymbol(this.currentCharacter);

                    /* Add the symbol to the token value. */
                    tokenValue.Append(this.currentCharacter);
                    this.NextCharacter(true);

                    if (isStandardSymbol)
                    {
                        /* Lump toghether all "standard symbols" (if this one was a standard one). */
                        while (this.IsStandardSymbol(this.currentCharacter))
                        {
                            if (this.currentCharacter == this.NumberLiteralDecimalSeparatorCharacter)
                            {
                                /* Decimal separator. Special case. Peek the next char to see if its a digit. */
                                if (char.IsDigit(this.PeekCharacter()))
                                {
                                    break;
                                }
                            }

                            tokenValue.Append(this.currentCharacter);
                            this.NextCharacter(true);
                        }
                    }

                    Debug.Assert(tokenValue.Length > 0, "Expected non-empty token value.");

                    return new Token(
                        Token.TokenType.Symbol,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        this.currentCharacterIndex - tokenStartIndex);
                }
            }

            return null;
        }
    }
}
