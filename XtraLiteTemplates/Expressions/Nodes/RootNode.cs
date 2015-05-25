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

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.IO;
    using System.Diagnostics;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    internal class RootNode : ExpressionNode
    {
        private List<ExpressionNode> m_children;

        public IReadOnlyList<ExpressionNode> Children { get; private set; }

        public ExpressionNode LastChild
        {
            get
            {
                Debug.Assert(m_children.Count > 0);
                return m_children[m_children.Count - 1];
            }
            set
            {
                Debug.Assert(m_children.Count > 0);
                Debug.Assert(value != null);

                m_children[m_children.Count - 1] = value;
            }
        }

        public bool Closed { get; private set; }

        public RootNode(ExpressionNode parent)
            : base(parent)
        {
            m_children = new List<ExpressionNode>();
        }

        public void AddChild(ExpressionNode child)
        {
            m_children.Add(child);
        }

        public void Close()
        {
            Debug.Assert(!Closed);
            Closed = true;
        }

        public override PermittedContinuations Continuity
        {
            get
            {
                if (Closed)
                {
                    return
                        PermittedContinuations.BinaryOperator |
                        PermittedContinuations.CloseGroup;
                }
                else
                {
                    return
                        PermittedContinuations.UnaryOperator |
                        PermittedContinuations.Literal |
                        PermittedContinuations.Identifier |
                        PermittedContinuations.NewGroup;
                }
            }
        }

        public override string ToString(ExpressionFormatStyle style)
        {
            var result = string.Join(" , ", m_children.Select(s => s.ToString(style)));

            if (Parent != null)
            {
                if (style == ExpressionFormatStyle.Canonical)
                    result = string.Format("(){{{0}}}", result);
                else if (style == ExpressionFormatStyle.Arithmetic)
                    result = string.Format("( {0} )", result);
                else if (style == ExpressionFormatStyle.Polish)
                    result = string.Format("({0})", result);
            }

            return result;
        }

        protected override bool TryReduce(IExpressionEvaluationContext reduceContext, out object reducedValue)
        {
            Debug.Assert(reduceContext != null);

            if (m_children.Count == 1)
            {
                if (m_children[0].Reduce(reduceContext))
                {
                    reducedValue = m_children[0].ReducedValue;
                    return true;
                }
            }
            else
            {
                var allReduced = true;
                foreach (var child in m_children)
                    allReduced &= child.Reduce(reduceContext);

                if (allReduced)
                {
                    reducedValue = m_children.Select(s => s.ReducedValue).ToArray();
                    return true;
                }
            }

            reducedValue = null;
            return false;
        }

        protected override Func<IExpressionEvaluationContext, object> Build()
        {
            var childFuncs = m_children.Select(s => s.GetEvaluationFunction()).ToArray();

            if (childFuncs.Length == 1)
            {
                /* We're transparent */
                var childFunc = childFuncs[0];
                return context => childFunc(context);
            }
            else
            {
                return context =>
                {
                    object[] array = new object[childFuncs.Length];
                    for (var i = 0; i < childFuncs.Length; i++)
                        array[i] = childFuncs[i](context);

                    return array;
                };
            }
        }
    }
}

