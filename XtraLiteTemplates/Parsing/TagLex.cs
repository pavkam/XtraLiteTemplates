//  Author:
//    Alexandru Ciobanu alex@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex@ciobanu.org)
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
    using System.Text;

    /// <summary>
    /// A <see cref="Lex"/> object representing a matched <see cref="Tag"/>.
    /// </summary>
    public sealed class TagLex : Lex
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TagLex"/> class.
        /// </summary>
        /// <param name="tag">The matched tag.</param>
        /// <param name="components">The matched tag's components.</param>
        /// <param name="firstCharacterIndex">Index of the first character in the input template.</param>
        /// <param name="originalLength">Original length of all the tags combined that make up this <c>lex</c> object.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="tag"/> or <paramref name="components"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="components"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="firstCharacterIndex" /> is less than zero; or <paramref name="originalLength" /> is less or equal to zero.</exception>
        public TagLex(Tag tag, object[] components, int firstCharacterIndex, int originalLength)
            : base(firstCharacterIndex, originalLength)
        {
            Expect.NotEmpty("components", components);
            Expect.NotNull("tag", tag);

            this.Components = components;
            this.Tag = tag;
        }

        /// <summary>
        /// Gets the tag that is matched by this <c>lex</c> object.
        /// <remarks>The value of this property is provided by the caller during the construction process.</remarks>
        /// </summary>
        /// <value>
        /// The tag object
        /// </value>
        public Tag Tag { get; private set; }

        /// <summary>
        /// Gets all the components of the matched tag. The number of items in this property matches the <see cref="XtraLiteTemplates.Parsing.Tag.ComponentCount"/> property.
        /// Each item can either be a <see cref="String"/> or an <see cref="XtraLiteTemplates.Expressions.Expression"/>, depending on which of the tag's component was matched.
        /// <remarks>The value of this property is provided by the caller during the construction process.</remarks>
        /// </summary>
        /// <value>
        /// The matched tag's components.
        /// </value>
        public object[] Components { get; private set; }

        /// <summary>
        /// Returns a human-readable representation of this <c>lex</c> object.
        /// </summary>
        /// <returns>
        /// A string that represents the current <c>lex</c> object.
        /// </returns>
        public override string ToString()
        {
            return string.Join(" ", this.Components);
        }
    }
}