﻿//
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
//     * Neither the name of the [ORGANIZATION] nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
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

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions.Operators;

    internal sealed class BinaryOperatorNode : OperatorNode
    {
        public new BinaryOperator Operator
        {
            get
            {
                return base.Operator as BinaryOperator;
            }
        }

        public ExpressionNode LeftNode { get; internal set; }

        internal BinaryOperatorNode(ExpressionNode parent, BinaryOperator @operator)
            : base(parent, @operator)
        {
        }

        public override ExpressionNode Reduce()
        {
            /* Left side. */
            LeftNode = LeftNode as LeafNode ?? LeftNode.Reduce();
            RightNode = RightNode as LeafNode ?? RightNode.Reduce();

            var leafLeft = LeftNode as LeafNode;
            if (leafLeft != null && leafLeft.Evaluation == LeafNode.EvaluationType.Literal)
            {
                Object reducedByLeft;
                if (Operator.EvaluateLhs(leafLeft.Operand, out reducedByLeft))
                    return new LeafNode(Parent, reducedByLeft, LeafNode.EvaluationType.Literal);

                /* Right side. */
                var leafRight = RightNode as LeafNode;
                if (leafRight != null && leafRight.Evaluation == LeafNode.EvaluationType.Literal)
                    return new LeafNode(Parent, Operator.Evaluate(leafLeft.Operand, leafRight.Operand), LeafNode.EvaluationType.Literal);
            }

            return this;
        }

        public override Func<IExpressionEvaluationContext, Object> Build()
        {
            var leftFunc = LeftNode.Build();
            var rightFunc = RightNode.Build();

            return (context) =>
            {
                var left = leftFunc(context);
                Object evaluatedByLeft;

                if (Operator.EvaluateLhs(left, out evaluatedByLeft))
                    return evaluatedByLeft;
                else
                    return Operator.Evaluate(left, rightFunc(context));
            };
        }

        public override String ToString(ExpressionFormatStyle style)
        {
            var leftAsString = LeftNode != null ? LeftNode.ToString(style) : "??";
            var rightAsString = RightNode != null ? RightNode.ToString(style) : "??";

            String result = null;

            if (style == ExpressionFormatStyle.Arithmetic)
                result = String.Format("{0} {1} {2}", leftAsString, Operator, rightAsString);
            else if (style == ExpressionFormatStyle.Polish)
                result = String.Format("{0} {1} {2}", Operator, leftAsString, rightAsString);
            else if (style == ExpressionFormatStyle.Canonical)
                result = String.Format("{0}{{{1},{2}}}", Operator, leftAsString, rightAsString);

            Debug.Assert(result != null);
            return result;
        }
    }
}

