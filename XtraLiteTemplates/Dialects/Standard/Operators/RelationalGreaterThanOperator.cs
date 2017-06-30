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

namespace XtraLiteTemplates.Dialects.Standard.Operators
{
    using System;
    using System.Collections.Generic;
    using Introspection;
    using JetBrains.Annotations;

    /// <summary>
    /// Implements the standard relational greater than ('&gt;') operation.
    /// </summary>
    [PublicAPI]
    public sealed class RelationalGreaterThanOperator : StandardRelationalOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalGreaterThanOperator" /> class.
        /// </summary>
        /// <param name="symbol">The operator's symbol.</param>
        /// <param name="stringComparer">The string literal comparer.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="symbol" /> or <paramref name="typeConverter" /> or <paramref name="stringComparer" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="symbol" /> is empty.</exception>
        public RelationalGreaterThanOperator(
            [NotNull] string symbol, 
            [NotNull] IComparer<string> stringComparer, 
            [NotNull] IPrimitiveTypeConverter typeConverter)
            : base(symbol, 6, stringComparer, typeConverter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalGreaterThanOperator" /> class using the standard '&gt;' symbol.
        /// </summary>
        /// <param name="stringComparer">The string literal comparer.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> or <paramref name="stringComparer" /> is <c>null</c>.</exception>
        public RelationalGreaterThanOperator([NotNull] IComparer<string> stringComparer, [NotNull] IPrimitiveTypeConverter typeConverter)
            : this(">", stringComparer, typeConverter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RelationalGreaterThanOperator" /> class using the standard '&gt;' symbol and current culture string comparer.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> is <c>null</c>.</exception>
        public RelationalGreaterThanOperator([NotNull] IPrimitiveTypeConverter typeConverter)
            : this(System.StringComparer.CurrentCulture, typeConverter)
        {
        }

        /// <summary>
        /// Validates that the specified <paramref name="relation" /> is greater than zero.
        /// </summary>
        /// <param name="relation">The evaluated relation between the two operands.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// <c>true</c> if <paramref name="relation"/> is greater than zero; <c>false</c> otherwise.
        /// </returns>
        protected override bool Evaluate(int relation, object left, object right)
        {
            return relation > 0;
        }
    }
}