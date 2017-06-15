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
    /// Abstract base class for all <c>lex</c> objects created and passed along during the lexical analysis process.
    /// </summary>
    public abstract class Lex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Lex"/> class.
        /// </summary>
        /// <param name="firstCharacterIndex">The index of the first character in the input template</param>
        /// <param name="originalLength">The combined length of all tokens that make up this <c>lex</c> object.</param>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="firstCharacterIndex" /> is less than zero; or <paramref name="originalLength" /> is less or equal to zero.</exception>
        protected Lex(int firstCharacterIndex, int originalLength)
        {
            Expect.GreaterThanOrEqual("firstCharacterIndex", firstCharacterIndex, 0);
            Expect.GreaterThan("originalLength", originalLength, 0);

            FirstCharacterIndex = firstCharacterIndex;
            OriginalLength = originalLength;
        }

        /// <summary>
        /// Gets index in the input template of the first character of the first token that makes up this <c>lex</c> object.
        /// </summary>
        /// <value>
        /// The index of the first character.
        /// </value>
        /// <remarks>
        /// The value of this property is provided by the caller during the construction process.
        /// </remarks>
        public int FirstCharacterIndex { get; }

        /// <summary>
        /// Gets original length of all tokens combined that make up this <c>lex</c> object.
        /// </summary>
        /// <value>
        /// The total length of all tokens that make up this <c>lex</c>.
        /// </value>
        /// <remarks>
        /// The value of this property is provided by the caller during the construction process.
        /// </remarks>
        public int OriginalLength { get; }
    }
}