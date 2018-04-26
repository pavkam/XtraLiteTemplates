//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2018, Alexandru Ciobanu (alex+git@ciobanu.org)
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

    using JetBrains.Annotations;

    /// <summary>
    /// A <see cref="Lex"/> object representing an un-parsed text block.
    /// </summary>
    [PublicAPI]
    public sealed class UnParsedLex : Lex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnParsedLex"/> class.
        /// </summary>
        /// <param name="unParsedText">The un-parsed text.</param>
        /// <param name="firstCharacterIndex">Index of the first character in the input template.</param>
        /// <param name="originalLength">Original length of the un-parsed text.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="unParsedText"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="unParsedText"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="firstCharacterIndex" /> is less than zero; or <paramref name="originalLength" /> is less or equal to zero.</exception>
        public UnParsedLex(
            [NotNull] string unParsedText, 
            int firstCharacterIndex, 
            int originalLength)
            : base(firstCharacterIndex, originalLength)
        {
            Expect.NotEmpty(nameof(unParsedText), unParsedText);

            UnParsedText = unParsedText;
        }

        /// <summary>
        /// Gets the un-parsed text (as read from the input template).
        /// <remarks>The value of this property is provided by the caller during the construction process.</remarks>
        /// </summary>
        /// <value>
        /// The un-parsed text.
        /// </value>
        [NotNull]
        public string UnParsedText { get; }
    }
}