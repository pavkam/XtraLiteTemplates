//
//  Author:
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
//

namespace XtraLiteTemplates.Dialects.Standard.Operators
{
    using System;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions;

    /// <summary>
    /// Implements the standard logical or ('||') operation.
    /// </summary>
    public sealed class LogicalOrOperator : StandardBinaryOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalOrOperator" /> class.
        /// </summary>
        /// <param name="symbol">The operator's symbol.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="symbol" /> or <paramref name="typeConverter" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="symbol" /> is empty.</exception>
        public LogicalOrOperator(String symbol, IPrimitiveTypeConverter typeConverter)
            : base(symbol, 12, typeConverter)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LogicalOrOperator" /> class using the standard '||' symbol.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> is <c>null</c>.</exception>
        public LogicalOrOperator(IPrimitiveTypeConverter typeConverter)
            : this("||", typeConverter)
        {
        }

        /// <summary>
        /// Evaluates the result of logical or operation for <paramref name="left" /> and <paramref name="right" /> operands.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The evaluated object.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context" /> is <c>null</c>.</exception>
        public override Object Evaluate(IExpressionEvaluationContext context, Object left, Object right)
        {
            Expect.NotNull("context", context);

            return TypeConverter.ConvertToBoolean(left) || TypeConverter.ConvertToBoolean(right);
        }

        /// <summary>
        /// Tries to evaluate the result only using the left-hand-side operand.
        /// <remarks>This method is used by operators that support left-hand-side short-circuit logic. The current implementation always return <c>false</c>.</remarks>
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="result">The result of the evaluation if the return value is <c>true</c>.</param>
        /// <returns>
        ///   <c>true</c> if the operation is supported; <c>false</c> otherwise.
        /// </returns>
        public override Boolean EvaluateLhs(IExpressionEvaluationContext context, Object left, out Object result)
        {
            Expect.NotNull("context", context);

            result = true;
            return (TypeConverter.TypeOf(left) == PrimitiveType.Boolean && TypeConverter.ConvertToBoolean(left) == true);
        }
    }
}

