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

namespace XtraLiteTemplates.Evaluation
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Exception type thrown for any encountered interpretation error.
    /// </summary>
    public class InterpreterException : FormatException
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InterpreterException"/> class.
        /// </summary>
        /// <param name="innerException">The inner exception.</param>
        /// <param name="candidateDirectives">The candidate directives.</param>
        /// <param name="firstCharacterIndex">Index of the first character.</param>
        /// <param name="format">The format string.</param>
        /// <param name="args">Format arguments.</param>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        internal InterpreterException(
            Exception innerException, 
            Directive[] candidateDirectives, 
            int firstCharacterIndex, 
            string format, 
            params object[] args)
            : base(string.Format(format, args), innerException)
        {
            Debug.Assert(firstCharacterIndex >= 0, "firstCharacterIndex cannot be less than zero.");

            FirstCharacterIndex = firstCharacterIndex;
            CandidateDirectives = candidateDirectives;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="InterpreterException"/> class.
        /// </summary>
        /// <param name="candidateDirectives">The candidate directives.</param>
        /// <param name="firstCharacterIndex">Index of the first character.</param>
        /// <param name="format">The format string.</param>
        /// <param name="args">Format arguments.</param>
        internal InterpreterException(Directive[] candidateDirectives, int firstCharacterIndex, string format, params object[] args)
            : this(null, candidateDirectives, firstCharacterIndex, format, args)
        {
        }

        /// <summary>
        /// Gets all candidate directives for which the exception applies.
        /// </summary>
        /// <value>
        /// The candidate directives.
        /// </value>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public Directive[] CandidateDirectives { get; }

        /// <summary>
        /// Gets the index of the first character in the input template that matches the candidate directives.
        /// </summary>
        /// <value>
        /// The index of the first character.
        /// </value>
        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
        public int FirstCharacterIndex { get; }
    }
}