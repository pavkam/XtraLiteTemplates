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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using LinqExpression = System.Linq.Expressions.Expression;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal class RootNode : ExpressionNode
    {
        private readonly List<ExpressionNode> _groupChildrenNodes;
        private readonly bool _allowEmptyGroup;

        public RootNode(ExpressionNode parent, bool allowEmptyGroup)
            : base(parent)
        {
            _groupChildrenNodes = new List<ExpressionNode>();
            _allowEmptyGroup = allowEmptyGroup;
        }

        public IReadOnlyList<ExpressionNode> Children { get; private set; }

        public ExpressionNode LastChild
        {
            get
            {
                Debug.Assert(_groupChildrenNodes.Count > 0, "Must have at least one child node.");
                return _groupChildrenNodes[_groupChildrenNodes.Count - 1];
            }

            set
            {
                Debug.Assert(_groupChildrenNodes.Count > 0, "Must have at least one child node.");
                Debug.Assert(value != null, "value cannot be null.");

                _groupChildrenNodes[_groupChildrenNodes.Count - 1] = value;
            }
        }

        public bool Closed { get; private set; }

        public override PermittedContinuations Continuity
        {
            get
            {
                if (Closed)
                {
                    return
                        PermittedContinuations.BinaryOperator |
                        PermittedContinuations.ContinueGroup |
                        PermittedContinuations.CloseGroup;
                }

                if (_groupChildrenNodes.Count == 0 && _allowEmptyGroup)
                {
                    return
                        PermittedContinuations.UnaryOperator |
                        PermittedContinuations.Literal |
                        PermittedContinuations.Identifier |
                        PermittedContinuations.CloseGroup |
                        PermittedContinuations.NewGroup;
                }

                return
                    PermittedContinuations.UnaryOperator |
                    PermittedContinuations.Literal |
                    PermittedContinuations.Identifier |
                    PermittedContinuations.NewGroup;
            }
        }

        public void AddChild(ExpressionNode child)
        {
            _groupChildrenNodes.Add(child);
        }

        public void Close()
        {
            Debug.Assert(!Closed, "Cannot be closed.");
            Debug.Assert(_groupChildrenNodes.Count > 0 || _allowEmptyGroup, "Children must be defined for [mandatory children] groups.");

            Closed = true;
        }

        public override string ToString(ExpressionFormatStyle style)
        {
            var result = string.Join(" , ", _groupChildrenNodes.Select(s => s.ToString(style)));

            if (Parent != null)
            {
                if (style == ExpressionFormatStyle.Canonical)
                {
                    result = $"(){{{result}}}";
                }
                else if (style == ExpressionFormatStyle.Arithmetic)
                {
                    if (!string.IsNullOrEmpty(result))
                    {
                        result = $"( {result} )";
                    }
                    else
                    {
                        result = "( )";
                    }
                }
                else if (style == ExpressionFormatStyle.Polish)
                {
                    result = $"({result})";
                }
            }

            return result;
        }

        protected override bool TryReduce(IExpressionEvaluationContext reduceContext, out object reducedValue)
        {
            Debug.Assert(reduceContext != null, "reduceContext cannot be null.");

            if (_groupChildrenNodes.Count == 1)
            {
                if (_groupChildrenNodes[0].Reduce(reduceContext))
                {
                    reducedValue = _groupChildrenNodes[0].ReducedValue;
                    return true;
                }
            }
            else if (_groupChildrenNodes.Count > 1)
            {
                var allReduced = true;
                foreach (var child in _groupChildrenNodes)
                {
                    allReduced &= child.Reduce(reduceContext);
                }

                if (allReduced)
                {
                    reducedValue = _groupChildrenNodes.Select(s => s.ReducedValue).ToArray();
                    return true;
                }
            }

            reducedValue = null;
            return false;
        }

        protected override LinqExpression BuildLinqExpression()
        {
            var childExpressions = _groupChildrenNodes.Select(s => s.GetEvaluationLinqExpression()).ToArray();

            if (childExpressions.Length == 0)
            {
                return LinqExpression.Constant(null);
            }

            if (childExpressions.Length == 1)
            {
                return childExpressions[0];
            }

            return LinqExpression.NewArrayInit(typeof(object), childExpressions);
        }
    }
}
