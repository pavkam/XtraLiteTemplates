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
    /// The exception type thrown for errors encountered during the lexical analysis.
    /// </summary>
    [PublicAPI]
    public class LexingException : ParseException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LexingException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="token">The token that resulted in the error being thrown.</param>
        /// <param name="format">A format string.</param>
        /// <param name="args">Format arguments.</param>
        [StringFormatMethod("format")]
        internal LexingException(
            [CanBeNull] Exception innerException, 
            [NotNull] Token token, 
            [NotNull] string format, 
            [NotNull] params object[] args)
            : base(innerException, token.CharacterIndex, format, args)
        {
            Token = token;
        }

        /// <summary>
        /// <value>Gets the token that resulted in the exception being thrown.</value>
        /// </summary>
        [NotNull]
        public Token Token { get; }
    }
}