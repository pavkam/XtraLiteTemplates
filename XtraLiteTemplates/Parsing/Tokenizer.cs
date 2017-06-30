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

namespace XtraLiteTemplates.Parsing
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;

    using JetBrains.Annotations;

    /// <summary>
    /// Provides the standard tokenization services.
    /// </summary>
    [PublicAPI]
    public sealed class Tokenizer : ITokenizer, IDisposable
    {
        [NotNull]
        private readonly TextReader _inputTextReader;
        private bool _parsingTag;
        private int _currentCharacterIndex;
        private bool _endOfStream;
        private char _currentCharacter;

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
            [NotNull] TextReader reader, 
            char tagStartCharacter = '{', 
            char tagEndCharacter = '}', 
            char stringStartCharacter = '"',
            char stringEndCharacter = '"', 
            char stringEscapeCharacter = '\\', 
            char numberDecimalSeparatorCharacter = '.')
        {
            Expect.NotNull(nameof(reader), reader);
            Expect.NotEqual(nameof(tagStartCharacter), nameof(tagEndCharacter), tagStartCharacter, tagEndCharacter);
            Expect.NotEqual(nameof(stringStartCharacter), nameof(tagStartCharacter), stringStartCharacter, tagStartCharacter);
            Expect.NotEqual(nameof(stringStartCharacter), nameof(tagEndCharacter), stringStartCharacter, tagEndCharacter);
            Expect.NotEqual(nameof(stringEndCharacter), nameof(tagStartCharacter), stringEndCharacter, tagStartCharacter);
            Expect.NotEqual(nameof(stringEndCharacter), nameof(tagEndCharacter), stringEndCharacter, tagEndCharacter);
            Expect.NotEqual(nameof(stringEscapeCharacter), nameof(stringStartCharacter), stringEscapeCharacter, stringStartCharacter);
            Expect.NotEqual(nameof(stringEscapeCharacter), nameof(stringEndCharacter), stringEscapeCharacter, stringEndCharacter);
            Expect.NotEqual(nameof(tagStartCharacter), nameof(numberDecimalSeparatorCharacter), tagStartCharacter, numberDecimalSeparatorCharacter);
            Expect.NotEqual(nameof(tagEndCharacter), nameof(numberDecimalSeparatorCharacter), tagEndCharacter, numberDecimalSeparatorCharacter);
            Expect.NotEqual(nameof(stringStartCharacter), nameof(numberDecimalSeparatorCharacter), stringStartCharacter, numberDecimalSeparatorCharacter);
            Expect.NotEqual(nameof(stringEndCharacter), nameof(numberDecimalSeparatorCharacter), stringEndCharacter, numberDecimalSeparatorCharacter);

            /* Validate allow character set. */
            var all = new[]
            { 
                tagStartCharacter, tagEndCharacter, stringStartCharacter,
                stringEndCharacter, stringEscapeCharacter, numberDecimalSeparatorCharacter
            };

            var allowedCharacterSet = !all.Any(c => char.IsWhiteSpace(c) || char.IsLetterOrDigit(c) || c == '_');
            Expect.IsTrue(nameof(allowedCharacterSet), allowedCharacterSet);

            _inputTextReader = reader;
            TagStartCharacter = tagStartCharacter;
            TagEndCharacter = tagEndCharacter;

            StringLiteralStartCharacter = stringStartCharacter;
            StringLiteralEndCharacter = stringEndCharacter;
            StringLiteralEscapeCharacter = stringEscapeCharacter;

            NumberLiteralDecimalSeparatorCharacter = numberDecimalSeparatorCharacter;

            _parsingTag = false;
            _currentCharacterIndex = -1;
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
        public Tokenizer([CanBeNull] string text)
            : this(new StringReader(text ?? string.Empty))
        {
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="Tokenizer"/> class.
        /// </summary>
        ~Tokenizer()
        {
            Dispose();
        }

        /// <summary>
        /// Gets the tag start character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The tag start character.
        /// </value>
        public char TagStartCharacter { get; }

        /// <summary>
        /// Gets the tag end character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The tag end character.
        /// </value>
        public char TagEndCharacter { get; }

        /// <summary>
        /// Gets the string literal start character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The string literal start character.
        /// </value>
        public char StringLiteralStartCharacter { get; }

        /// <summary>
        /// Gets the string literal end character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The string literal end character.
        /// </value>
        public char StringLiteralEndCharacter { get; }

        /// <summary>
        /// Gets the string literal escape character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The string literal escape character.
        /// </value>
        public char StringLiteralEscapeCharacter { get; }

        /// <summary>
        /// Gets the number literal decimal separator character.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The number literal decimal separator character.
        /// </value>
        public char NumberLiteralDecimalSeparatorCharacter { get; }

        /// <summary>
        /// Reads the next <see cref="Token" /> object from the input template.
        /// </summary>
        /// <returns>
        /// The next read token; or <c>null</c> if the end of template was reached.
        /// </returns>
        public Token ReadNext()
        {
            return ReadNextInternal();
        }

        /// <summary>
        /// Disposes the input template reader.
        /// </summary>
        public void Dispose()
        {
            _inputTextReader.Dispose();

            GC.SuppressFinalize(this);
        }

        private char PeekCharacter()
        {
            if (_endOfStream)
            {
                ExceptionHelper.UnexpectedEndOfStream(_currentCharacterIndex);
            }

            var peekedCharacter = _inputTextReader.Peek();
            if (peekedCharacter == -1)
            {
                _endOfStream = true;
                ExceptionHelper.UnexpectedEndOfStream(_currentCharacterIndex + 1);
            }
            else
            {
                return (char)peekedCharacter;
            }

            return '\0';
        }

        private void NextCharacter(bool required)
        {
            if (_endOfStream)
            {
                ExceptionHelper.UnexpectedEndOfStream(_currentCharacterIndex);
            }

            var readCharacter = _inputTextReader.Read();
            _currentCharacterIndex++;
            if (readCharacter == -1)
            {
                _endOfStream = true;
                if (required)
                {
                    ExceptionHelper.UnexpectedEndOfStream(_currentCharacterIndex);
                }
            }
            else
            {
                _currentCharacter = (char)readCharacter;
            }
        }

        private bool IsTagSpecialCharacter(char c)
        {
            return
                c == TagStartCharacter || c == TagEndCharacter ||
                c == StringLiteralStartCharacter || c == StringLiteralEndCharacter;
        }

        private bool IsStandardSymbol(char c)
        {
            if (IsTagSpecialCharacter(c))
            {
                return false;
            }

            var unicodeCategory = char.GetUnicodeCategory(c);
            return
                unicodeCategory == UnicodeCategory.OtherPunctuation ||
                unicodeCategory == UnicodeCategory.CurrencySymbol ||
                unicodeCategory == UnicodeCategory.DashPunctuation ||
                unicodeCategory == UnicodeCategory.ModifierSymbol ||
                unicodeCategory == UnicodeCategory.MathSymbol;
        }

        [CanBeNull]
        private Token ReadNextInternal()
        {
            if (_currentCharacterIndex == -1)
            {
                NextCharacter(false);
            }

            if (_endOfStream && !_parsingTag)
            {
                return null;
            }

            var tokenValue = new StringBuilder();
            var tokenStartIndex = _currentCharacterIndex;

            if (_parsingTag && _currentCharacter == TagEndCharacter)
            {
                /* Tag end character. Need to switch to text mode. */
                _parsingTag = false;

                var endTagToken = new Token(Token.TokenType.EndTag, _currentCharacter.ToString(), _currentCharacterIndex, 1);

                NextCharacter(false);
                return endTagToken;
            }

            if (_parsingTag && _currentCharacter == TagStartCharacter)
            {
                ExceptionHelper.UnexpectedCharacter(_currentCharacterIndex, TagStartCharacter);
            }

            if (!_parsingTag)
            {
                tokenValue.Append(_currentCharacter);

                if (_currentCharacter == TagStartCharacter)
                {
                    /* Found { character. This might actually be an escape sequence. */
                    NextCharacter(true);
                    if (_currentCharacter == TagStartCharacter)
                    {
                        NextCharacter(false);

                        return new Token(
                            Token.TokenType.UnParsed,
                            tokenValue.ToString(),
                            tokenStartIndex,
                            _currentCharacterIndex - tokenStartIndex);
                    }

                    _parsingTag = true;

                    return new Token(
                        Token.TokenType.StartTag,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        _currentCharacterIndex - tokenStartIndex);
                }

                /* Parsing free-form text until the start directive character is found. */
                NextCharacter(false);

                while (!_endOfStream && _currentCharacter != TagStartCharacter)
                {
                    tokenValue.Append(_currentCharacter);
                    NextCharacter(false);
                }

                return new Token(
                    Token.TokenType.UnParsed,
                    tokenValue.ToString(),
                    tokenStartIndex,
                    _currentCharacterIndex - tokenStartIndex);
            }

            if (_parsingTag)
            {
                /* Directive parsing is a different beast... */
                Debug.Assert(!_endOfStream, "Must not be end-of-stream.");

                if (_currentCharacter == StringLiteralStartCharacter)
                {
                    /* String constant start character. Read all the way until the matching one is found (or escape!) */
                    while (true)
                    {
                        NextCharacter(true);

                        if (_currentCharacter == StringLiteralEndCharacter)
                        {
                            /* End of string. */
                            NextCharacter(true);
                            break;
                        }

                        if (_currentCharacter == StringLiteralEscapeCharacter)
                        {
                            /* Escape character. */
                            NextCharacter(true);

                            if (_currentCharacter == StringLiteralEscapeCharacter)
                            {
                                tokenValue.Append(StringLiteralEscapeCharacter);
                            }
                            else if (_currentCharacter == StringLiteralEndCharacter)
                            {
                                tokenValue.Append(StringLiteralEndCharacter);
                            }
                            else
                            {
                                switch (_currentCharacter)
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
                                        ExceptionHelper.InvalidEscapeCharacter(_currentCharacterIndex, _currentCharacter);
                                        break;
                                }
                            }
                        }
                        else
                        {
                            tokenValue.Append(_currentCharacter);
                        }
                    }

                    return new Token(
                        Token.TokenType.String,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        _currentCharacterIndex - tokenStartIndex);
                }

                if (char.IsLetter(_currentCharacter) || _currentCharacter == '_')
                {
                    /* Identifier/keyword token has started. */
                    while (char.IsLetterOrDigit(_currentCharacter) || _currentCharacter == '_')
                    {
                        tokenValue.Append(_currentCharacter);
                        NextCharacter(true);
                    }

                    Debug.Assert(tokenValue.Length > 0, "Expected non-empty token value.");
                    return new Token(
                        Token.TokenType.Word,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        _currentCharacterIndex - tokenStartIndex);
                }

                if (char.IsDigit(_currentCharacter))
                {
                    /* Number has started. */
                    while (char.IsDigit(_currentCharacter))
                    {
                        tokenValue.Append(_currentCharacter);
                        NextCharacter(true);
                    }

                    if (_currentCharacter == NumberLiteralDecimalSeparatorCharacter && char.IsDigit(PeekCharacter()))
                    {
                        /* This is a Decimal point. Read the remaining bits. */
                        tokenValue.Append(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);
                        NextCharacter(true);

                        while (char.IsDigit(_currentCharacter))
                        {
                            tokenValue.Append(_currentCharacter);
                            NextCharacter(true);
                        }
                    }

                    Debug.Assert(tokenValue.Length > 0, "Expected non-empty token value.");
                    return new Token(
                        Token.TokenType.Number,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        _currentCharacterIndex - tokenStartIndex);
                }

                if (_currentCharacter == NumberLiteralDecimalSeparatorCharacter)
                {
                    /* Dot-prefixed number may have started. */
                    var dot = _currentCharacter;
                    NextCharacter(true);

                    if (char.IsDigit(_currentCharacter))
                    {
                        tokenValue.Append(CultureInfo.InvariantCulture.NumberFormat.NumberDecimalSeparator);

                        /* Yes, this is a number! */
                        while (char.IsDigit(_currentCharacter))
                        {
                            tokenValue.Append(_currentCharacter);
                            NextCharacter(true);
                        }

                        return new Token(
                            Token.TokenType.Number,
                            tokenValue.ToString(),
                            tokenStartIndex,
                            _currentCharacterIndex - tokenStartIndex);
                    }

                    tokenValue.Append(dot);

                    /* Lump together all "standard symbols" (if this one was a standard one). */
                    while (IsStandardSymbol(_currentCharacter))
                    {
                        tokenValue.Append(_currentCharacter);
                        NextCharacter(true);
                    }

                    return new Token(
                        Token.TokenType.Symbol,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        _currentCharacterIndex - tokenStartIndex);
                }

                if (char.IsWhiteSpace(_currentCharacter))
                {
                    /* Whitespace... */
                    while (char.IsWhiteSpace(_currentCharacter))
                    {
                        tokenValue.Append(_currentCharacter);
                        NextCharacter(true);
                    }

                    Debug.Assert(tokenValue.Length > 0, "Expected non-empty token value.");

                    return new Token(
                        Token.TokenType.Whitespace,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        _currentCharacterIndex - tokenStartIndex);
                }

                if (!IsTagSpecialCharacter(_currentCharacter))
                {
                    /* Check if this is a "standard symbol" */
                    var validStandardSymbol = IsStandardSymbol(_currentCharacter);

                    /* Add the symbol to the token value. */
                    tokenValue.Append(_currentCharacter);
                    NextCharacter(true);

                    if (validStandardSymbol)
                    {
                        /* Lump together all "standard symbols" (if this one was a standard one). */
                        while (IsStandardSymbol(_currentCharacter))
                        {
                            if (_currentCharacter == NumberLiteralDecimalSeparatorCharacter)
                            {
                                /* Decimal separator. Special case. Peek the next char to see if its a digit. */
                                if (char.IsDigit(PeekCharacter()))
                                {
                                    break;
                                }
                            }

                            tokenValue.Append(_currentCharacter);
                            NextCharacter(true);
                        }
                    }

                    Debug.Assert(tokenValue.Length > 0, "Expected non-empty token value.");

                    return new Token(
                        Token.TokenType.Symbol,
                        tokenValue.ToString(),
                        tokenStartIndex,
                        _currentCharacterIndex - tokenStartIndex);
                }
            }

            return null;
        }
    }
}