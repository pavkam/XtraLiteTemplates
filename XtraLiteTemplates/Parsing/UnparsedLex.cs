//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex+git@ciobanu.org)
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

    /// <summary>
    /// A <see cref="Lex"/> object representing an unparsed text block.
    /// </summary>
    public sealed class UnparsedLex : Lex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UnparsedLex"/> class.
        /// </summary>
        /// <param name="unparsedText">The unparsed text.</param>
        /// <param name="firstCharacterIndex">Index of the first character in the input template.</param>
        /// <param name="originalLength">Original length of the unparsed text.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="unparsedText"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="unparsedText"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="firstCharacterIndex" /> is less than zero; or <paramref name="originalLength" /> is less or equal to zero.</exception>
        public UnparsedLex(string unparsedText, int firstCharacterIndex, int originalLength)
            : base(firstCharacterIndex, originalLength)
        {
            Expect.NotEmpty("unparsedText", unparsedText);

            this.UnparsedText = unparsedText;
        }

        /// <summary>
        /// Gets the unparsed text (as read from the input template).
        /// <remarks>The value of this property is provided by the caller during the construction process.</remarks>
        /// </summary>
        /// <value>
        /// The unparsed text.
        /// </value>
        public string UnparsedText { get; private set; }
    }
}