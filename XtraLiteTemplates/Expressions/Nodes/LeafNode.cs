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
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Diagnostics;

    internal class LeafNode : ExpressionNode
    {
        public enum EvaluationType
        {
            Literal,
            Variable,
            Indentifier,
        }

        public Object Operand { get; private set; }

        public EvaluationType Evaluation { get; private set; }

        public LeafNode(ExpressionNode parent, Object operand, EvaluationType type)
            : base(parent)
        {
            Debug.Assert(type == EvaluationType.Literal || operand is String);

            Operand = operand;
            Evaluation = type;
        }

        public override String ToString(ExpressionFormatStyle style)
        {
            if (Evaluation == EvaluationType.Indentifier)
                return (String)Operand;
            else if (Evaluation == EvaluationType.Variable)
                return String.Format("@{0}", Operand);
            else if (Operand == null)
                return "undefined";
            else if (Operand is String)
            {
                using (var writer = new StringWriter())
                {
                    using (var provider = CodeDomProvider.CreateProvider("CSharp"))
                    {
                        provider.GenerateCodeFromExpression(new CodePrimitiveExpression(Operand), writer, null);
                        return writer.ToString();
                    }
                }
            }
            else
                return Operand.ToString();
        }

        public void ConvertToIdentifier()
        {
            Debug.Assert(Evaluation == EvaluationType.Variable);
            Evaluation = EvaluationType.Indentifier;
        }

        public override Func<IExpressionEvaluationContext, Object> Build()
        {
            if (Evaluation == EvaluationType.Variable)
                return (IExpressionEvaluationContext context) => context.GetVariable((String)Operand);
            else
                return (IExpressionEvaluationContext context) => Operand;
        }
    }
}

