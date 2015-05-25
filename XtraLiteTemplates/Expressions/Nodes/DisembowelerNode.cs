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

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions.Operators;

    internal class DisembowelerNode : ExpressionNode
    {
        public ExpressionNode ObjectNode { get; private set; }

        public string MemberName { get; internal set; }

        internal DisembowelerNode(ExpressionNode parent, ExpressionNode objectNode)
            : base(parent)
        {
            Debug.Assert(objectNode != null);

            ObjectNode = objectNode;
        }

        public override PermittedContinuations Continuity
        {
            get
            {
                if (MemberName == null)
                {
                    return PermittedContinuations.Identifier;
                } 
                else
                {
                    return
                       PermittedContinuations.BinaryOperator |
                       PermittedContinuations.CloseGroup;
                }
            }
        }

        public override String ToString(ExpressionFormatStyle style)
        {
            var memberName = MemberName ?? "??";

            if (style == ExpressionFormatStyle.Arithmetic)
            {
                return string.Format("{0} . {1}", ObjectNode.ToString(style), memberName);
            }
            else if (style == ExpressionFormatStyle.Canonical)
            {
                return string.Format(".{{{0},{1}}}", ObjectNode.ToString(style), memberName);
            }
            else if (style == ExpressionFormatStyle.Polish)
            {
                return string.Format(". {0} {1}", ObjectNode.ToString(style), memberName);
            }

            Debug.Fail("Unreachable code.");
            return null;
        }

        protected override bool TryReduce(IExpressionEvaluationContext reduceContext, out object value)
        {
            Debug.Assert(reduceContext != null);

            ObjectNode.Reduce(reduceContext);

            value = null;
            return false;
        }

        protected override Func<IExpressionEvaluationContext, Object> Build()
        {
            var objectFunc = ObjectNode.GetEvaluationFunction();
            return context =>
            {
                var variable = objectFunc(context);
                return variable == null ? null : context.GetProperty(variable, MemberName);
            };
        }
    }
}
