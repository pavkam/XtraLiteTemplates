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

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System;
    using System.Diagnostics;
    using System.Threading;
    using XtraLiteTemplates.Expressions.Operators;
    using LinqExpression = System.Linq.Expressions.Expression;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal sealed class UnaryOperatorNode : OperatorNode
    {
        public UnaryOperatorNode(ExpressionNode parent, UnaryOperator @operator)
            : base(parent, @operator)
        {
        }

        public new UnaryOperator Operator
        {
            get
            {
                return base.Operator as UnaryOperator;
            }
        }

        public override PermittedContinuations Continuity
        {
            get
            {
                return
                    PermittedContinuations.UnaryOperator |
                    PermittedContinuations.Literal |
                    PermittedContinuations.Identifier |
                    PermittedContinuations.NewGroup;
            }
        }

        public override string ToString(ExpressionFormatStyle style)
        {
            var childAsString = this.RightNode != null ? this.RightNode.ToString(style) : "??";

            string result = null;

            if (style == ExpressionFormatStyle.Canonical)
            {
                result = string.Format("{0}{{{1}}}", this.Operator, childAsString);
            }
            else
            {
                result = string.Format("{0}{1}", this.Operator, childAsString);
            }

            Debug.Assert(result != null, "resulting string cannot be null.");
            return result;
        }

        protected override bool TryReduce(IExpressionEvaluationContext reduceContext, out object value)
        {
            Debug.Assert(reduceContext != null, "reduceContext cannot be null.");

            if (this.RightNode.Reduce(reduceContext))
            {
                value = this.Operator.Evaluate(reduceContext, this.RightNode.ReducedValue);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        protected override LinqExpression BuildLinqExpression()
        {
            var operandExpression = this.RightNode.GetEvaluationLinqExpression();

            return LinqExpression.Block(
                typeof(object),
                LinqExpressionHelper.ExpressionCallThrowIfCancellationRequested,
                LinqExpression.Call(LinqExpression.Constant(this.Operator), LinqExpressionHelper.MethodInfoUnaryOperatorEvaluate, LinqExpressionHelper.ExpressionParameterContext, operandExpression));
        }
    }
}
