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

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions.Operators;

    internal sealed class UnaryOperatorNode : OperatorNode
    {
        public new UnaryOperator Operator
        {
            get
            {
                return base.Operator as UnaryOperator;
            }
        }

        public UnaryOperatorNode(ExpressionNode parent, UnaryOperator @operator)
            : base(parent, @operator)
        {
        }

        protected override Func<IExpressionEvaluationContext, Object> Build()
        {
            var childFunc = RightNode.GetEvaluationFunction();
            return (context) => Operator.Evaluate(context, childFunc(context));
        }

        protected override Boolean TryReduce(IExpressionEvaluationContext reduceContext, out Object value)
        {
            Debug.Assert(reduceContext != null);

            if (RightNode.Reduce(reduceContext))
            {
                value = Operator.Evaluate(reduceContext, RightNode.ReducedValue);
                return true;
            }
            else
            {
                value = null;
                return false;
            }
        }

        public override String ToString(ExpressionFormatStyle style)
        {
            var childAsString = RightNode != null ? RightNode.ToString(style) : "??";

            String result = null;

            if (style == ExpressionFormatStyle.Canonical)
                result = String.Format("{0}{{{1}}}", Operator, childAsString);
            else
                result = String.Format("{0}{1}", Operator, childAsString);

            Debug.Assert(result != null);
            return result;
        }
    }
}

