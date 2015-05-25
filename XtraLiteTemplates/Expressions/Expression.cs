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

namespace XtraLiteTemplates.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions.Nodes;
    using XtraLiteTemplates.Expressions.Operators;

    public sealed class Expression
    {
        private ExpressionNode m_current;
        private RootNode m_root;
        private List<Operator> m_supportedOperators;
        private Func<IExpressionEvaluationContext, object> m_function;
        private Dictionary<string, UnaryOperator> m_unaryOperatorSymbols;
        private Dictionary<string, BinaryOperator> m_binaryOperatorSymbols;

        public bool Constructed
        {
            get
            {
                return this.m_function != null;
            }
        }

        public bool Started
        {
            get
            {
                return this.m_current != null;
            }
        }

        public bool IsSupportedOperator(string symbol)
        {
            Expect.NotEmpty("symbol", symbol);

            return
                this.m_unaryOperatorSymbols.ContainsKey(symbol) || this.m_binaryOperatorSymbols.ContainsKey(symbol);
        }

        public IReadOnlyList<Operator> SupportedOperators
        {
            get
            {
                return this.m_supportedOperators;
            }
        }

        public ExpressionFlowSymbols FlowSymbols { get; private set; }

        public IEqualityComparer<string> Comparer { get; private set; }

        public Expression(ExpressionFlowSymbols flowSymbols, IEqualityComparer<string> comparer)
        {
            Expect.NotNull("comparer", comparer);
            Expect.NotNull("flowSymbols", flowSymbols);

            this.FlowSymbols = flowSymbols;
            this.m_unaryOperatorSymbols = new Dictionary<String, UnaryOperator>(comparer);
            this.m_binaryOperatorSymbols = new Dictionary<String, BinaryOperator>(comparer);
            this.m_supportedOperators = new List<Operator>();
            this.Comparer = comparer;
        }

        public Expression()
            : this(ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
        {
        }

        public Expression RegisterOperator(Operator @operator)
        {
            Expect.NotNull("operator", @operator);

            if (Started)
            {
                ExceptionHelper.CannotRegisterOperatorsForStartedExpression();
            }

            Debug.Assert(!Constructed);

            /* Standards. */
            if (@operator.Symbol == this.FlowSymbols.Separator ||
                @operator.Symbol == this.FlowSymbols.GroupClose ||
                @operator.Symbol == this.FlowSymbols.GroupOpen ||
                @operator.Symbol == this.FlowSymbols.MemberAccess)
                ExceptionHelper.OperatorAlreadyRegistered(@operator);

            if (@operator is UnaryOperator)
            {
                var unaryOperator = (UnaryOperator)@operator;
                if (this.m_unaryOperatorSymbols.ContainsKey(unaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(unaryOperator);
                }

                this.m_unaryOperatorSymbols.Add(@operator.Symbol, unaryOperator);
            }
            else if (@operator is BinaryOperator)
            {
                var binaryOperator = (BinaryOperator)@operator;
                if (this.m_binaryOperatorSymbols.ContainsKey(binaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(binaryOperator);
                }

                this.m_binaryOperatorSymbols.Add(@operator.Symbol, binaryOperator);
            }
            else
            {
                Debug.Fail("Unsupported operator type.");
            }

            this.m_supportedOperators.Add(@operator);
            return this;
        }

        private void OpenNewGroup()
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.NewGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(this.FlowSymbols.GroupOpen);
            }

            if (m_current is OperatorNode)
            {
                var _current = (OperatorNode)this.m_current;
                Debug.Assert(_current.RightNode == null);

                /* Flip to the new root */
                this.m_root = new RootNode(_current);
                _current.RightNode = this.m_root;
                this.m_current = this.m_root;
            }
            else if (this.m_current is RootNode)
            {
                var _current = (RootNode)this.m_current;

                Debug.Assert(_current == this.m_root);
                Debug.Assert(!_current.Closed);

                /* Flip to the new root */
                this.m_root = new RootNode(_current);
                _current.AddChild(m_root);
                this.m_current = m_root;
            }
        }

        private void CloseExistingGroup()
        {
            /* Special case just here! */
            if (this.m_root.Parent == null)
            {
                ExceptionHelper.InvalidExpressionTerm(this.FlowSymbols.GroupClose);
            }

            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.CloseGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(this.FlowSymbols.GroupClose);
            }

            this.m_root.Close();
            this.m_current = m_root;

            /* Find the actual root now. */
            var _root = this.m_root.Parent;
            while (!(_root is RootNode))
            {
                _root = _root.Parent;
            }

            this.m_root = (RootNode)_root;
        }

        private void ContinueExistingGroup()
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.ContinueGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(FlowSymbols.Separator);
            }

            this.m_current = this.m_root;
        }

        private void StartUnary(UnaryOperator unaryOperator)
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.UnaryOperator))
            {
                ExceptionHelper.UnexpectedOperator(unaryOperator.Symbol);
            }

            if (this.m_current is OperatorNode)
            {
                var _current = this.m_current as OperatorNode;
                Debug.Assert(_current.RightNode == null);

                _current.RightNode = new UnaryOperatorNode(_current, unaryOperator);
                this.m_current = _current.RightNode;
            }
            else if (m_current is RootNode)
            {
                var _current = this.m_current as RootNode;
                Debug.Assert(!_current.Closed);

                var newNode = new UnaryOperatorNode(_current, unaryOperator);
                _current.AddChild(newNode);
                this.m_current = newNode;
            }
        }

        private void StartBinary(BinaryOperator binaryOperator)
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
                ExceptionHelper.UnexpectedOperator(binaryOperator.Symbol);

            var leftNode = this.m_current;
            var comparand = binaryOperator.Associativity == Associativity.LeftToRight ? 0 : -1;

            /* Go up the tree while the precedence allows. */
            while (leftNode.Parent is OperatorNode &&
                   ((OperatorNode)leftNode.Parent).Operator.Precedence.CompareTo(binaryOperator.Precedence) <= comparand)
            {
                leftNode = leftNode.Parent;
            }

            var leftNodeParentOperatorNode = leftNode.Parent as OperatorNode;

            this.m_current = new BinaryOperatorNode(leftNode.Parent, binaryOperator)
            {
                LeftNode = leftNode,
            };

            /* Re-jig the tree. */
            if (leftNodeParentOperatorNode != null)
            {
                leftNodeParentOperatorNode.RightNode = this.m_current;
            }

            leftNode.Parent = m_current;
            if (this.m_root.LastChild == leftNode)
            {
                this.m_root.LastChild = m_current;
            }

            return;
        }

        private void CompleteWithSymbol(string symbol)
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.Identifier))
                ExceptionHelper.InvalidExpressionTerm(symbol);

            var newNode = new ReferenceNode(this.m_current, symbol);

            if (this.m_current is OperatorNode)
            {
                var _current = this.m_current as OperatorNode;
                Debug.Assert(_current.RightNode == null);

                _current.RightNode = newNode;
                this.m_current = newNode;
            }
            else if (this.m_current is RootNode)
            {
                var _current = this.m_current as RootNode;
                Debug.Assert(!_current.Closed);

                _current.AddChild(newNode);
                this.m_current = newNode;
            }
            else if (this.m_current is DisembowelerNode && ((DisembowelerNode)this.m_current).MemberName == null)
            {
                var _current = this.m_current as DisembowelerNode;
                Debug.Assert(_current.ObjectNode != null);

                _current.MemberName = symbol;
            }
        }

        private void CompleteWithLiteral(object literal)
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.Literal))
            {
                if (this.m_current is DisembowelerNode && ((DisembowelerNode)this.m_current).MemberName == null)
                    ExceptionHelper.UnexpectedLiteralRequiresIdentifier(FlowSymbols.MemberAccess, literal);
                else
                    ExceptionHelper.UnexpectedLiteralRequiresOperator(literal);
            }

            var newNode = new LiteralNode(this.m_current, literal);

            if (this.m_current is OperatorNode)
            {
                var _current = this.m_current as OperatorNode;

                Debug.Assert(_current.RightNode == null);

                _current.RightNode = newNode;
                this.m_current = newNode;
            }
            else if (this.m_current is RootNode)
            {
                var _current = this.m_current as RootNode;
                Debug.Assert(!_current.Closed);

                _current.AddChild(newNode);
                this.m_current = newNode;
            }
        }

        private void ContinueWithMemberAccess()
        {
            if (!m_current.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
                ExceptionHelper.UnexpectedOperator(this.FlowSymbols.MemberAccess);

            /* Left side now becomes the "object" of disembowlement and the right side will be the member name */
            var newNode = new DisembowelerNode(this.m_current.Parent, this.m_current);
            var parentOperatorNode = this.m_current.Parent as OperatorNode;
            if (parentOperatorNode != null)
            {
                Debug.Assert(parentOperatorNode.RightNode == this.m_current);
                parentOperatorNode.RightNode = newNode;
            }
            else if (this.m_current.Parent == m_root)
            {
                Debug.Assert(this.m_root.LastChild == this.m_current);
                this.m_root.LastChild = newNode;
            }

            this.m_current = newNode;
        }

        private void FeedTerm(object term, bool isLiteral)
        {
            Debug.Assert(isLiteral || term is string);

            if (this.m_root == null)
            {
                /* Init! */
                this.m_root = new RootNode(null);
                this.m_current = m_root;
            }

            if (Constructed)
            {
                ExceptionHelper.CannotModifyAConstructedExpression();
            }

            if (!isLiteral)
            {
                var symbol = (string)term;

                if (symbol == this.FlowSymbols.MemberAccess)
                {
                    this.ContinueWithMemberAccess();
                }
                else if (symbol == this.FlowSymbols.GroupOpen)
                {
                    this.OpenNewGroup();
                }
                else if (symbol == this.FlowSymbols.GroupClose)
                {
                    this.CloseExistingGroup();
                }
                else if (symbol == this.FlowSymbols.Separator)
                {
                    this.ContinueExistingGroup();
                }
                else
                {
                    UnaryOperator unaryOperator;
                    if (this.m_unaryOperatorSymbols.TryGetValue(symbol, out unaryOperator))
                    {
                        if (this.m_current.Continuity.HasFlag(PermittedContinuations.UnaryOperator))
                        {
                            StartUnary(unaryOperator);
                            return;
                        }
                    }

                    BinaryOperator binaryOperator;
                    if (this.m_binaryOperatorSymbols.TryGetValue(symbol, out binaryOperator))
                    {
                        if (this.m_current.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
                        {
                            StartBinary(binaryOperator);
                            return;
                        }
                    }

                    if (unaryOperator != null || binaryOperator != null)
                    {
                        ExceptionHelper.UnexpectedOperator(symbol);
                    }
                    else
                    {
                        this.CompleteWithSymbol(symbol);
                    }
                }
            }
            else
                this.CompleteWithLiteral(term);
        }

        public Expression FeedLiteral(object literal)
        {
            if (this.Constructed)
                ExceptionHelper.CannotModifyAConstructedExpression();

            FeedTerm(literal, true);
            return this;
        }

        public Expression FeedSymbol(string symbol)
        {
            Expect.NotEmpty("symbol", symbol);

            if (this.Constructed)
            {
                ExceptionHelper.CannotModifyAConstructedExpression();
            }

            FeedTerm(symbol, false);
            return this;
        }

        public void Construct()
        {
            if (!this.Started)
                ExceptionHelper.CannotConstructExpressionInvalidState();

            if (!this.Constructed)
            {
                bool fail = false;
                if (this.m_root.Parent != null)
                    fail = true;
                else
                {
                    var _currentAsRoot = this.m_current as RootNode;
                    if (_currentAsRoot != null)
                    {
                        fail = !_currentAsRoot.Closed;
                    }
                    else
                    {
                        var _currentAsDisem = this.m_current as DisembowelerNode;
                        if (_currentAsDisem != null)
                        {
                            fail = _currentAsDisem.MemberName == null;
                        }
                        else
                        {
                            fail = !(this.m_current is LeafNode);
                        }
                    }
                }

                if (fail)
                {
                    ExceptionHelper.CannotConstructExpressionInvalidState();
                }

                /* Reduce the expression if so was desired. */
                this.m_root.Reduce(ReduceExpressionEvaluationContext.Instance);
                this.m_function = this.m_root.GetEvaluationFunction();
            }
        }

        public object Evaluate(IExpressionEvaluationContext context)
        {
            Expect.NotNull("context", context);

            if (!Constructed)
            {
                ExceptionHelper.CannotEvaluateUnconstructedExpression();
            }

            return this.m_function(context);
        }

        public override string ToString()
        {
            return ToString(ExpressionFormatStyle.Arithmetic);
        }

        public string ToString(ExpressionFormatStyle style)
        {
            if (this.m_root == null)
            {
                return "??";
            }
            else
            {
                return this.m_root.ToString(style);
            }
        }
    }
}
