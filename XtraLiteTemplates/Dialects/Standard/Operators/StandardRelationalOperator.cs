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

namespace XtraLiteTemplates.Dialects.Standard.Operators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;

    using JetBrains.Annotations;

    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Introspection;

    /// <summary>
    /// The abstract base class for all standard binary relational expression operators.
    /// </summary>
    [PublicAPI]
    public abstract class StandardRelationalOperator : StandardBinaryOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandardRelationalOperator" /> class.
        /// </summary>
        /// <param name="symbol">The operator's symbol.</param>
        /// <param name="precedence">The operator's precedence.</param>
        /// <param name="stringComparer">The string literal comparer.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="ArgumentNullException">Arguments <paramref name="symbol" />, <paramref name="typeConverter" /> or <paramref name="stringComparer" /> are <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="symbol" /> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Argument <paramref name="precedence"/> is less than zero.</exception>
        protected StandardRelationalOperator(
            [NotNull] string symbol, 
            int precedence,
            [NotNull] IComparer<string> stringComparer,
            [NotNull] IPrimitiveTypeConverter typeConverter)
            : base(symbol, precedence, typeConverter)
        {
            Expect.NotNull(nameof(stringComparer), stringComparer);

            StringComparer = stringComparer;
        }

        /// <summary>
        /// Gets the <see cref="IComparer{String}"/> object used to compare string literals.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The string literal comparer.
        /// </value>
        [NotNull]
        public IComparer<string> StringComparer { get; }

        /// <summary>
        /// Evaluates the current operator for a given <paramref name="left" /> and <paramref name="right" /> operands. This method calls <see cref="Evaluate(int,object,object)"/>, which
        /// is expected to be implemented in descendant classes.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// A <seealso cref="bool"/> value indicating the result of the comparison.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context"/> is <c>null</c>.</exception>
        public override sealed object Evaluate(IExpressionEvaluationContext context, object left, object right)
        {
            Expect.NotNull(nameof(context), context);

            int relation;
            if (TypeConverter.TypeOf(left) == PrimitiveType.String || TypeConverter.TypeOf(right) == PrimitiveType.String)
            {
                relation = StringComparer.Compare(TypeConverter.ConvertToString(left), TypeConverter.ConvertToString(right));
            }
            else
            {
                relation = TypeConverter.ConvertToNumber(left).CompareTo(TypeConverter.ConvertToNumber(right));
            }

            return Evaluate(relation, left, right);
        }

        /// <summary>
        /// Validates that the specified <paramref name="relation"/> is valid for the specified operator.
        /// </summary>
        /// <param name="relation">The evaluated relation between the two operands.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// A <see cref="bool"/> value indicating the result of the validation.
        /// </returns>
        [SuppressMessage("ReSharper", "UnusedParameter.Global")]
        protected abstract bool Evaluate(int relation, [CanBeNull] object left, [CanBeNull] object right);
    }
}