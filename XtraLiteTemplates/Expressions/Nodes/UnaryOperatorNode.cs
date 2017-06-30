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

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Does not apply.")]

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System.Diagnostics;

    using Operators;
    using LinqExpression = System.Linq.Expressions.Expression;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal sealed class UnaryOperatorNode : OperatorNode
    {
        public UnaryOperatorNode(ExpressionNode parent, UnaryOperator @operator)
            : base(parent, @operator)
        {
        }

        public new UnaryOperator Operator => base.Operator as UnaryOperator;

        public override PermittedContinuations Continuity => PermittedContinuations.UnaryOperator |
                                                             PermittedContinuations.Literal |
                                                             PermittedContinuations.Identifier |
                                                             PermittedContinuations.NewGroup;

        public override string ToString(ExpressionFormatStyle style)
        {
            var childAsString = RightNode != null ? RightNode.ToString(style) : "??";

            var result = style == ExpressionFormatStyle.Canonical ? $"{Operator}{{{childAsString}}}" : $"{Operator}{childAsString}";

            Debug.Assert(result != null, "resulting string cannot be null.");
            return result;
        }

        protected override bool TryReduce(IExpressionEvaluationContext reduceContext, out object value)
        {
            Debug.Assert(reduceContext != null, "reduceContext cannot be null.");

            if (RightNode.Reduce(reduceContext))
            {
                value = Operator.Evaluate(reduceContext, RightNode.ReducedValue);
                return true;
            }

            value = null;
            return false;
        }

        protected override LinqExpression BuildLinqExpression()
        {
            var operandExpression = RightNode.GetEvaluationLinqExpression();

            return LinqExpression.Block(
                typeof(object),
                LinqExpressionHelper.ExpressionCallThrowIfCancellationRequested,
                LinqExpression.Call(LinqExpression.Constant(Operator), LinqExpressionHelper.MethodInfoUnaryOperatorEvaluate, LinqExpressionHelper.ExpressionParameterContext, operandExpression));
        }
    }
}
