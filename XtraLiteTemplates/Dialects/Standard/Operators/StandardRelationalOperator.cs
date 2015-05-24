﻿//  Author:
//    Alexandru Ciobanu alex@ciobanu.org
//
//  Copyright (c) 2015, Alexandru Ciobanu (alex@ciobanu.org)
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
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions;

    /// <summary>
    /// The abstract base class for all standard binary relational expression operators.
    /// </summary>
    public abstract class StandardRelationalOperator : StandardBinaryOperator
    {
        /// <summary>
        /// Specifies a <see cref="IComparer{String}"/> object used to compare string literals.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The string literal comparer.
        /// </value>
        public IComparer<String> StringComparer { get; private set; }

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
        public StandardRelationalOperator(String symbol, Int32 precedence, 
            IComparer<String> stringComparer, IPrimitiveTypeConverter typeConverter)
            : base(symbol, precedence, typeConverter)
        {
            Expect.NotNull("stringComparer", stringComparer);
            StringComparer = stringComparer;
        }

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
        public sealed override Object Evaluate(IExpressionEvaluationContext context, Object left, Object right)
        {
            Expect.NotNull("context", context);

            Int32 relation;
            if (TypeConverter.TypeOf(left) == PrimitiveType.String || TypeConverter.TypeOf(right) == PrimitiveType.String)
                relation = StringComparer.Compare(TypeConverter.ConvertToString(left), TypeConverter.ConvertToString(right));
            else
                relation = TypeConverter.ConvertToNumber(left).CompareTo(TypeConverter.ConvertToNumber(right));

            return Evaluate(relation, left, right);
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
        public abstract bool Evaluate(Int32 relation, Object left, Object right);
    }
}

