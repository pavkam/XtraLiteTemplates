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

namespace XtraLiteTemplates.Expressions.Operators
{
    using System;

    /// <summary>
    /// The abstract base class for all binary expression operators.
    /// </summary>
    public abstract class BinaryOperator : Operator
    {
        /// <summary>
        /// Specifies the associativity.
        /// </summary>
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// <value>
        /// The operator's associativity.
        /// </value>
        public Associativity Associativity { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BinaryOperator"/> class.
        /// </summary>
        /// <param name="symbol">The operator's symbol.</param>
        /// <param name="precedence">The operator's precedence.</param>
        /// <param name="associativity">The operator's associativity.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="symbol"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="symbol"/> is empty.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Argument <paramref name="precedence"/> is less than zero.</exception>
        protected BinaryOperator(String symbol, Int32 precedence, Associativity associativity)
            : base(symbol, precedence)
        {
            Expect.GreaterThanOrEqual("precedence", precedence, 0);
            Associativity = associativity;
        }

        /// <summary>
        /// Override in descendant classes to evaluate the operation for <paramref name="left"/> and <paramref name="right"/> operands.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>The evaluated object.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context"/> is <c>null</c>.</exception>
        public abstract Object Evaluate(IExpressionEvaluationContext context, Object left, Object right);

        /// <summary>
        /// Tries to evaluate the current operator for a given <paramref name="left"/> operand.
        /// <remarks>This method is used by operators that support left-hand-side short-circuit logic. The current implementation always return <c>false</c>.</remarks>
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="result">The result of the evaluation if the return value is <c>true</c>.</param>
        /// <returns><c>true</c> if the operation is supported; <c>false</c> otherwise.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context"/> is <c>null</c>.</exception>
        public virtual Boolean EvaluateLhs(IExpressionEvaluationContext context, Object left, out Object result)
        {
            Expect.NotNull("context", context);

            result = null;
            return false;
        }
    }
}

