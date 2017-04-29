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

namespace XtraLiteTemplates.Dialects.Standard.Operators
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Introspection;

    /// <summary>
    /// The abstract base class for all standard binary relational expression operators.
    /// </summary>
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
        public StandardRelationalOperator(
            string symbol, 
            int precedence, 
            IComparer<string> stringComparer, 
            IPrimitiveTypeConverter typeConverter)
            : base(symbol, precedence, typeConverter)
        {
            Expect.NotNull("stringComparer", stringComparer);

            this.StringComparer = stringComparer;
        }

        /// <summary>
        /// Gets the <see cref="IComparer{String}"/> object used to compare string literals.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The string literal comparer.
        /// </value>
        public IComparer<string> StringComparer { get; private set; }

        /// <summary>
        /// Evaluates the current operator for a given <paramref name="left" /> and <paramref name="right" /> operands. This method calls <see cref="Evaluate(Int32,Object,Object)"/>, which
        /// is expected to be implemented in descendant classes.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// A <seealso cref="Boolean"/> value indicating the result of the comparison.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context"/> is <c>null</c>.</exception>
        public sealed override object Evaluate(IExpressionEvaluationContext context, object left, object right)
        {
            Expect.NotNull("context", context);

            int relation;
            if (this.TypeConverter.TypeOf(left) == PrimitiveType.String || this.TypeConverter.TypeOf(right) == PrimitiveType.String)
            {
                relation = this.StringComparer.Compare(this.TypeConverter.ConvertToString(left), this.TypeConverter.ConvertToString(right));
            }
            else
            {
                relation = this.TypeConverter.ConvertToNumber(left).CompareTo(this.TypeConverter.ConvertToNumber(right));
            }

            return this.Evaluate(relation, left, right);
        }

        /// <summary>
        /// Validates that the specified <paramref name="relation"/> is valid for the specified operator.
        /// </summary>
        /// <param name="relation">The evaluated relation between the two operands.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// A <see cref="Boolean"/> value indicating the result of the validation.
        /// </returns>
        public abstract bool Evaluate(int relation, object left, object right);
    }
}