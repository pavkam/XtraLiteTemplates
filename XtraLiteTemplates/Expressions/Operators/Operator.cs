﻿//  Author:
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

namespace XtraLiteTemplates.Expressions.Operators
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// The abstract base class for all expression operators.
    /// </summary>
    [PublicAPI]
    public abstract class Operator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Operator"/> class.
        /// </summary>
        /// <param name="symbol">The operator's symbol.</param>
        /// <param name="precedence">The operator's precedence.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="symbol"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="symbol"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Argument <paramref name="precedence"/> is less than zero.</exception>
        protected Operator([NotNull] string symbol, int precedence)
        {
            Expect.NotEmpty(nameof(symbol), symbol);
            Expect.GreaterThanOrEqual(nameof(precedence), precedence, 0);

            Symbol = symbol;
            Precedence = precedence;
        }

        /// <summary>
        /// Gets the operator's symbol, used in the expression building process.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The operator's symbol.
        /// </value>
        [NotNull]
        public string Symbol { get; }

        /// <summary>
        /// Gets the operator's precedence, used in the expression building process.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The operator's precedence.
        /// </value>
        public int Precedence { get; }

        /// <summary>
        /// Returns a human-readable representation of the operator. This implementation returns the value of <see cref="Symbol"/> property.
        /// </summary>
        /// <returns>
        /// A string that represents the current operator object.
        /// </returns>
        public override string ToString()
        {
            return Symbol;
        }
    }
}
