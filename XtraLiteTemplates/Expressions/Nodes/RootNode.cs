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

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Does not apply.")]

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal class RootNode : ExpressionNode
    {
        private List<ExpressionNode> groupChildrenNodes;
        private bool allowEmptyGroup;

        public RootNode(ExpressionNode parent, bool allowEmptyGroup)
            : base(parent)
        {
            this.groupChildrenNodes = new List<ExpressionNode>();
            this.allowEmptyGroup = allowEmptyGroup;
        }

        public IReadOnlyList<ExpressionNode> Children { get; private set; }

        public ExpressionNode LastChild
        {
            get
            {
                Debug.Assert(this.groupChildrenNodes.Count > 0, "Must have at least one child node.");
                return this.groupChildrenNodes[this.groupChildrenNodes.Count - 1];
            }

            set
            {
                Debug.Assert(this.groupChildrenNodes.Count > 0, "Must have at least one child node.");
                Debug.Assert(value != null, "value cannot be null.");

                this.groupChildrenNodes[this.groupChildrenNodes.Count - 1] = value;
            }
        }

        public bool Closed { get; private set; }

        public override PermittedContinuations Continuity
        {
            get
            {
                if (this.Closed)
                {
                    return
                        PermittedContinuations.BinaryOperator |
                        PermittedContinuations.ContinueGroup |
                        PermittedContinuations.CloseGroup;
                }
                else
                {
                    if (this.groupChildrenNodes.Count == 0 && this.allowEmptyGroup)
                    {
                        return
                            PermittedContinuations.UnaryOperator |
                            PermittedContinuations.Literal |
                            PermittedContinuations.Identifier |
                            PermittedContinuations.CloseGroup |
                            PermittedContinuations.NewGroup;
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
        }

        public void AddChild(ExpressionNode child)
        {
            this.groupChildrenNodes.Add(child);
        }

        public void Close()
        {
            Debug.Assert(!this.Closed, "Cannot be closed.");
            Debug.Assert(this.groupChildrenNodes.Count > 0 || this.allowEmptyGroup, "Children must be defined for [mandatory children] groups.");

            this.Closed = true;
        }

        public override string ToString(ExpressionFormatStyle style)
        {
            var result = string.Join(" , ", this.groupChildrenNodes.Select(s => s.ToString(style)));

            if (this.Parent != null)
            {
                if (style == ExpressionFormatStyle.Canonical)
                {
                    result = string.Format("(){{{0}}}", result);
                }
                else if (style == ExpressionFormatStyle.Arithmetic)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        result = string.Format("( {0} )", result);
                    }
                    else
                    {
                        result = "( )";
                    }
                }
                else if (style == ExpressionFormatStyle.Polish)
                {
                    result = string.Format("({0})", result);
                }
            }

            return result;
        }

        protected override bool TryReduce(IExpressionEvaluationContext reduceContext, out object reducedValue)
        {
            Debug.Assert(reduceContext != null, "reduceContext cannot be null.");

            if (this.groupChildrenNodes.Count == 1)
            {
                if (this.groupChildrenNodes[0].Reduce(reduceContext))
                {
                    reducedValue = this.groupChildrenNodes[0].ReducedValue;
                    return true;
                }
            }
            else if (this.groupChildrenNodes.Count > 1)
            {
                var allReduced = true;
                foreach (var child in this.groupChildrenNodes)
                {
                    allReduced &= child.Reduce(reduceContext);
                }

                if (allReduced)
                {
                    reducedValue = this.groupChildrenNodes.Select(s => s.ReducedValue).ToArray();
                    return true;
                }
            }

            reducedValue = null;
            return false;
        }

        protected override Func<IExpressionEvaluationContext, object> Build()
        {
            var childFuncs = this.groupChildrenNodes.Select(s => s.GetEvaluationFunction()).ToArray();

            if (childFuncs.Length == 0)
            {
                return context => null;
            }
            else if (childFuncs.Length == 1)
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
                    {
                        array[i] = childFuncs[i](context);
                    }

                    return array;
                };
            }
        }
    }
}
