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

    /// <summary>
    /// Class used to carry tokens from the tokenization process to the lexical analyzer.
    /// </summary>
    public sealed class Token
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Token"/> class.
        /// </summary>
        /// <param name="type">The type of the token.</param>
        /// <param name="value">The value of the token.</param>
        /// <param name="characterIndex">Index of the first character.</param>
        /// <param name="originalLength">The original length of the token.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="characterIndex" /> is less than zero; or <paramref name="originalLength" /> is less or equal to zero.</exception>
        public Token(TokenType type, string value, int characterIndex, int originalLength)
        {
            Expect.GreaterThanOrEqual("characterIndex", characterIndex, 0);
            Expect.GreaterThan("originalLength", originalLength, 0);

            if (type != TokenType.String)
            {
                Expect.NotEmpty("value", value);
            }

            Type = type;
            Value = value ?? string.Empty;
            CharacterIndex = characterIndex;
            OriginalLength = originalLength;
        }

        /// <summary>
        /// Defines the type of the token.
        /// </summary>
        public enum TokenType
        {
            /// <summary>
            /// An un-parsed text block.
            /// </summary>
            UnParsed,

            /// <summary>
            /// Tag start symbol encountered.
            /// </summary>
            StartTag,

            /// <summary>
            /// Tag end symbol encountered.
            /// </summary>
            EndTag,

            /// <summary>
            /// A word in an open tag.
            /// </summary>
            Word,

            /// <summary>
            /// A number literal in an open tag.
            /// </summary>
            Number,

            /// <summary>
            /// A string literal in an open tag.
            /// </summary>
            String,

            /// <summary>
            /// A sequence of characters that match by type and are not words.
            /// </summary>
            Symbol,

            /// <summary>
            /// Whitespace in an open tag.
            /// </summary>
            Whitespace,
        }

        /// <summary>
        /// Gets the type of the token.
        /// <remarks>The value of this property is provided by the caller during the construction process.</remarks>
        /// </summary>
        /// <value>
        /// The type of the token.
        /// </value>
        public TokenType Type { get; }

        /// <summary>
        /// Gets the value of the token.
        /// <remarks>The value of this property is provided by the caller during the construction process.</remarks>
        /// </summary>
        /// <value>
        /// The value of the token.
        /// </value>
        public string Value { get; }

        /// <summary>
        /// Gets the index of the token's first character 
        /// <remarks>The value of this property is provided by the caller during the construction process.</remarks>
        /// </summary>
        /// <value>
        /// The index the first character.
        /// </value>
        public int CharacterIndex { get; }

        /// <summary>
        /// Gets the original length of the token (as seen in the input template).
        /// The <see cref="OriginalLength"/> might differ from the length of the <see cref="Value"/> property for some tag types.
        /// <remarks>The value of this property is provided by the caller during the construction process.</remarks>
        /// </summary>
        /// <value>
        /// The original length of the token.
        /// </value>
        public int OriginalLength { get; }
    }
}