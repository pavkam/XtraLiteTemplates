﻿//  Author:
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

    using JetBrains.Annotations;

    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Introspection;

    /// <summary>
    /// Implements the standard floating point sum ('+') operation, with an extension for <see cref="string"/> and <see cref="bool"/> types.
    /// </summary>
    [PublicAPI]
    public sealed class ArithmeticSumOperator : StandardBinaryOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArithmeticSumOperator" /> class.
        /// </summary>
        /// <param name="symbol">The operator's symbol.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="symbol" /> or <paramref name="typeConverter" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="symbol" /> is empty.</exception>
        public ArithmeticSumOperator([NotNull] string symbol, [NotNull] IPrimitiveTypeConverter typeConverter)
            : base(symbol, 4, typeConverter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ArithmeticSumOperator" /> class using the standard '+' symbol.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> is <c>null</c>.</exception>
        public ArithmeticSumOperator([NotNull] IPrimitiveTypeConverter typeConverter)
            : this("+", typeConverter)
        {
        }

        /// <summary>
        /// Evaluates the result of sum operation for <paramref name="left" /> and <paramref name="right" /> operands.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The evaluated object.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context" /> is <c>null</c>.</exception>
        public override object Evaluate(IExpressionEvaluationContext context, object left, object right)
        {
            Expect.NotNull(nameof(context), context);

            var leftType = TypeConverter.TypeOf(left);
            var rightType = TypeConverter.TypeOf(right);

            if (leftType == PrimitiveType.String || rightType == PrimitiveType.String)
            {
                return TypeConverter.ConvertToString(left) + TypeConverter.ConvertToString(right);
            }

            if (leftType == PrimitiveType.Number || rightType == PrimitiveType.Number)
            {
                return TypeConverter.ConvertToNumber(left) + TypeConverter.ConvertToNumber(right);
            }

            if (leftType == PrimitiveType.Boolean || rightType == PrimitiveType.Boolean)
            {
                return TypeConverter.ConvertToNumber(left) + TypeConverter.ConvertToNumber(right);
            }

            return null;
        }
    }
}