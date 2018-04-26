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

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics;
    using LinqExpression = System.Linq.Expressions.Expression;

    internal abstract class ExpressionNode
    {
        protected ExpressionNode(ExpressionNode parent)
        {
            Parent = parent;
        }

        public ExpressionNode Parent { get; set; }

        [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
        public bool IsReduced { get; private set; }

        public object ReducedValue { get; private set; }

        public abstract PermittedContinuations Continuity { get; }

        public bool Reduce(IExpressionEvaluationContext reduceContext)
        {
            Debug.Assert(reduceContext != null, "reduceContext cannot be null.");

            if (!IsReduced)
            {
                if (TryReduce(reduceContext, out var value))
                {
                    IsReduced = true;
                    ReducedValue = value;
                }
            }

            return IsReduced;
        }

        public LinqExpression GetEvaluationLinqExpression()
        {
            return IsReduced ? LinqExpression.Constant(ReducedValue, typeof(object)) : BuildLinqExpression();
        }

        public abstract string ToString(ExpressionFormatStyle style);

        public override string ToString()
        {
            return ToString(ExpressionFormatStyle.Arithmetic);
        }

        protected virtual bool TryReduce(IExpressionEvaluationContext reduceContext, out object reducedValue)
        {
            Debug.Assert(reduceContext != null, "reduceContext cannot be null.");

            reducedValue = null;
            return false;
        }

        protected abstract LinqExpression BuildLinqExpression();
    }
}
