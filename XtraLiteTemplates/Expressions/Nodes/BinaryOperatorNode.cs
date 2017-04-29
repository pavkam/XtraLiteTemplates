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
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using XtraLiteTemplates.Expressions.Operators;
    using LinqExpression = System.Linq.Expressions.Expression;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal sealed class BinaryOperatorNode : OperatorNode
    {
        internal BinaryOperatorNode(ExpressionNode parent, BinaryOperator @operator)
            : base(parent, @operator)
        {
        }

        public new BinaryOperator Operator
        {
            get
            {
                return base.Operator as BinaryOperator;
            }
        }

        public ExpressionNode LeftNode { get; internal set; }

        public override PermittedContinuations Continuity 
        { 
            get
            {
                return
                    PermittedContinuations.UnaryOperator |
                    PermittedContinuations.Identifier |
                    PermittedContinuations.Literal |
                    PermittedContinuations.NewGroup;
            }
        }

        public override string ToString(ExpressionFormatStyle style)
        {
            var leftAsString = this.LeftNode != null ? this.LeftNode.ToString(style) : "??";
            var rightAsString = this.RightNode != null ? this.RightNode.ToString(style) : "??";

            string result = null;

            if (style == ExpressionFormatStyle.Arithmetic)
            {
                result = string.Format("{0} {1} {2}", leftAsString, this.Operator, rightAsString);
            }
            else if (style == ExpressionFormatStyle.Polish)
            {
                result = string.Format("{0} {1} {2}", this.Operator, leftAsString, rightAsString);
            }
            else if (style == ExpressionFormatStyle.Canonical)
            {
                result = string.Format("{0}{{{1},{2}}}", this.Operator, leftAsString, rightAsString);
            }

            Debug.Assert(result != null, "resulting string cannot be null.");
            return result;
        }

        protected override bool TryReduce(IExpressionEvaluationContext reduceContext, out object reducedValue)
        {
            Debug.Assert(reduceContext != null, "reduceContext cannot be null.");

            if (this.LeftNode.Reduce(reduceContext))
            {
                if (this.Operator.EvaluateLhs(reduceContext, this.LeftNode.ReducedValue, out reducedValue))
                {
                    return true;
                }
                else if (this.RightNode.Reduce(reduceContext))
                {
                    reducedValue = this.Operator.Evaluate(reduceContext, this.LeftNode.ReducedValue, this.RightNode.ReducedValue);
                    return true;
                }
            }

            reducedValue = null;
            return false;
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Readability is OK in this circumstances.")]
        protected override LinqExpression BuildLinqExpression()
        {
            var leftOperandExpression = this.LeftNode.GetEvaluationLinqExpression();
            var rightOperandExpression = this.RightNode.GetEvaluationLinqExpression();

            var variableLeft = LinqExpression.Variable(typeof(object));
            var variableEvaluatedResult = LinqExpression.Variable(typeof(object));

            return LinqExpression.Block(
                typeof(object),
                new[] { variableLeft, variableEvaluatedResult },
                LinqExpressionHelper.ExpressionCallThrowIfCancellationRequested,
                LinqExpression.Assign(variableLeft, leftOperandExpression),
                LinqExpression.IfThen(
                    LinqExpression.Not(
                        LinqExpression.Call(
                            LinqExpression.Constant(this.Operator), 
                            LinqExpressionHelper.MethodInfoBinaryOperatorEvaluateLhs,
                            LinqExpressionHelper.ExpressionParameterContext, 
                            variableLeft, 
                            variableEvaluatedResult)),
                    LinqExpression.Assign(
                        variableEvaluatedResult, 
                        LinqExpression.Call(
                            LinqExpression.Constant(this.Operator), 
                            LinqExpressionHelper.MethodInfoBinaryOperatorEvaluate, 
                            LinqExpressionHelper.ExpressionParameterContext, 
                            variableLeft, 
                            rightOperandExpression))),
                variableEvaluatedResult);
        }
    }
}
