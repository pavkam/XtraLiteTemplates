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
        private Func<IExpressionEvaluationContext, Object> m_function;
        private Dictionary<String, UnaryOperator> m_unaryOperatorSymbols;
        private Dictionary<String, BinaryOperator> m_binaryOperatorSymbols;

        public Boolean Constructed
        {
            get
            {
                return m_function != null;
            }
        }

        public Boolean Started
        {
            get
            {
                return m_current != null;
            }
        }

        public Boolean IsSupportedOperator(String symbol)
        {
            Expect.NotEmpty("symbol", symbol);

            return
                m_unaryOperatorSymbols.ContainsKey(symbol) || m_binaryOperatorSymbols.ContainsKey(symbol);
        }

        public IReadOnlyList<Operator> SupportedOperators
        {
            get
            {
                return m_supportedOperators;
            }
        }

        public ExpressionFlowSymbols FlowSymbols { get; private set; }

        public IEqualityComparer<String> Comparer { get; private set; }

        public Expression(ExpressionFlowSymbols flowSymbols, IEqualityComparer<String> comparer)
        {
            Expect.NotNull("comparer", comparer);
            Expect.NotNull("flowSymbols", flowSymbols);

            FlowSymbols = flowSymbols;

            m_unaryOperatorSymbols = new Dictionary<String, UnaryOperator>(comparer);
            m_binaryOperatorSymbols = new Dictionary<String, BinaryOperator>(comparer);
            m_supportedOperators = new List<Operator>();

            Comparer = comparer;
        }

        public Expression()
            : this(ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
        {
        }

        public Expression RegisterOperator(Operator @operator)
        {
            Expect.NotNull("operator", @operator);

            if (Started)
                ExceptionHelper.CannotRegisterOperatorsForStartedExpression();

            Debug.Assert(!Constructed);

            /* Standards. */
            if (@operator.Symbol == FlowSymbols.Separator || 
                @operator.Symbol == FlowSymbols.GroupClose ||
                @operator.Symbol == FlowSymbols.GroupOpen ||
                @operator.Symbol == FlowSymbols.MemberAccess)
                ExceptionHelper.OperatorAlreadyRegistered(@operator);

            if (@operator is UnaryOperator)
            {
                var unaryOperator = (UnaryOperator)@operator;
                if (m_unaryOperatorSymbols.ContainsKey(unaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(unaryOperator);
                }

                m_unaryOperatorSymbols.Add(@operator.Symbol, unaryOperator);
            }
            else if (@operator is BinaryOperator)
            {
                var binaryOperator = (BinaryOperator)@operator;
                if (m_binaryOperatorSymbols.ContainsKey(binaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(binaryOperator);
                }


                m_binaryOperatorSymbols.Add(@operator.Symbol, binaryOperator);
            }
            else
                Debug.Assert(false, "Unsupported operator type.");

            m_supportedOperators.Add(@operator);
            return this;
        }


        private void OpenNewGroup()
        {
            if (!m_current.Continuity.HasFlag(PermittedContinuations.NewGroup))
                ExceptionHelper.UnexpectedExpressionTerm(FlowSymbols.GroupOpen);

            if (m_current is OperatorNode)
            {
                var _current = (OperatorNode)m_current;
                Debug.Assert(_current.RightNode == null);

                /* Flip to the new root */
                m_root = new RootNode(_current);
                _current.RightNode = m_root;
                m_current = m_root;
            }
            else if (m_current is RootNode)
            {
                var _current = (RootNode)m_current;

                Debug.Assert(_current == m_root);
                Debug.Assert(!_current.Closed);

                /* Flip to the new root */
                m_root = new RootNode(_current);
                _current.AddChild(m_root);
                m_current = m_root;
            }
        }

        private void CloseExistingGroup()
        {
            if (!m_current.Continuity.HasFlag(PermittedContinuations.CloseGroup))
                ExceptionHelper.UnexpectedExpressionTerm(FlowSymbols.GroupClose);

            Debug.Assert(m_current != m_root && m_root.Parent != null);

            m_root.Close();
            m_current = m_root;

            /* Find the actual root now. */
            var _root = m_root.Parent;
            while (!(_root is RootNode))
                _root = _root.Parent;

            m_root = (RootNode)_root;
        }

        private void ContinueExistingGroup()
        {
            if (!m_current.Continuity.HasFlag(PermittedContinuations.ContinueGroup))
                ExceptionHelper.UnexpectedExpressionTerm(FlowSymbols.Separator);

            Debug.Assert(m_current != m_root);

            m_current = m_root;
        }

        private void StartUnary(UnaryOperator unaryOperator)
        {
            if (!m_current.Continuity.HasFlag(PermittedContinuations.UnaryOperator))
                ExceptionHelper.UnexpectedExpressionTerm(unaryOperator.Symbol);

            if (m_current is OperatorNode)
            {
                var _current = m_current as OperatorNode;
                Debug.Assert(_current.RightNode == null);

                _current.RightNode = new UnaryOperatorNode(_current, unaryOperator);
                m_current = _current.RightNode;
            }
            else if (m_current is RootNode)
            {
                var _current = m_current as RootNode;
                if (_current.Closed)
                    ExceptionHelper.UnexpectedExpressionTerm(unaryOperator.Symbol);

                var newNode = new UnaryOperatorNode(_current, unaryOperator);
                _current.AddChild(newNode);
                m_current = newNode;
            }
        }

        private void StartBinary(BinaryOperator binaryOperator)
        {
            if (!m_current.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
                ExceptionHelper.UnexpectedExpressionTerm(binaryOperator.Symbol);

            var leftNode = m_current;
            var comparand = binaryOperator.Associativity == Associativity.LeftToRight ? 0 : -1;

            /* Go up the tree while the precedence allows. */
            while (leftNode.Parent is OperatorNode &&
                   ((OperatorNode)leftNode.Parent).Operator.Precedence.CompareTo(binaryOperator.Precedence) <= comparand)
            {
                leftNode = leftNode.Parent;
            }

            var leftNodeParentOperatorNode = leftNode.Parent as OperatorNode;

            m_current = new BinaryOperatorNode(leftNode.Parent, binaryOperator)
            {
                LeftNode = leftNode,
            };

            /* Re-jig the tree. */
            if (leftNodeParentOperatorNode != null)
                leftNodeParentOperatorNode.RightNode = m_current;

            leftNode.Parent = m_current;
            if (m_root.LastChild == leftNode)
                m_root.LastChild = m_current;

            return;
        }

        private void CompleteWithSymbol(String symbol)
        {
            if (!m_current.Continuity.HasFlag(PermittedContinuations.Identifier))
                ExceptionHelper.UnexpectedExpressionTerm(symbol);

            var newNode = new ReferenceNode(m_current, symbol);

            if (m_current is OperatorNode)
            {
                var _current = m_current as OperatorNode;
                Debug.Assert(_current.RightNode == null);

                _current.RightNode = newNode;
                m_current = newNode;
            }
            else if (m_current is RootNode)
            {
                var _current = m_current as RootNode;
                if (_current.Closed)
                    ExceptionHelper.UnexpectedExpressionTerm(symbol);

                _current.AddChild(newNode);
                m_current = newNode;
            }
            else if (m_current is DisembowelerNode && ((DisembowelerNode)m_current).MemberName == null)
            {
                var _current = m_current as DisembowelerNode;
                Debug.Assert(_current.ObjectNode != null);

                _current.MemberName = symbol;
            }
        }

        private void CompleteWithLiteral(Object literal)
        {
            if (!m_current.Continuity.HasFlag(PermittedContinuations.Literal))
                ExceptionHelper.UnexpectedExpressionTerm(literal);

            var newNode = new LiteralNode(m_current, literal);

            if (m_current is OperatorNode)
            {
                var _current = m_current as OperatorNode;

                Debug.Assert(_current.RightNode == null);

                _current.RightNode = newNode;
                m_current = newNode;
            }
            else if (m_current is RootNode)
            {
                var _current = m_current as RootNode;
                if (_current.Closed)
                    ExceptionHelper.UnexpectedExpressionTerm(literal);

                _current.AddChild(newNode);
                m_current = newNode;
            }
        }

        private void ContinueWithMemberAccess()
        {
            if (!m_current.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
                ExceptionHelper.UnexpectedExpressionTerm(FlowSymbols.MemberAccess);

            /* Left side now becomes the "object" of disembowlement and the right side will be the member name */
            var newNode = new DisembowelerNode(m_current.Parent, m_current);
            var parentOperatorNode = m_current.Parent as OperatorNode;
            if (parentOperatorNode != null)
            {
                Debug.Assert(parentOperatorNode.RightNode == m_current);
                parentOperatorNode.RightNode = newNode;
            }
            else if (m_current.Parent == m_root)
            {
                Debug.Assert(m_root.LastChild == m_current);
                m_root.LastChild = newNode;
            }

            m_current = newNode;
        }


        private void FeedTerm(Object term, Boolean isLiteral)
        {
            Debug.Assert(isLiteral || term is String);

            if (m_root == null)
            {
                /* Init! */
                m_root = new RootNode(null);
                m_current = m_root;
            }

            if (Constructed)
                ExceptionHelper.CannotModifyAConstructedExpression();

            if (!isLiteral)
            {
                String symbol = (String)term;

                if (symbol == FlowSymbols.MemberAccess)
                    ContinueWithMemberAccess();
                else if (symbol == FlowSymbols.GroupOpen)
                    OpenNewGroup();
                else if (symbol == FlowSymbols.GroupClose)
                    CloseExistingGroup();
                else if (symbol == FlowSymbols.Separator)
                    ContinueExistingGroup();
                else
                {
                    UnaryOperator unaryOperator;
                    if (m_unaryOperatorSymbols.TryGetValue(symbol, out unaryOperator))
                    {
                        if (m_current.Continuity.HasFlag(PermittedContinuations.UnaryOperator))
                        {
                            StartUnary(unaryOperator);
                            return;
                        }
                    }

                    BinaryOperator binaryOperator;
                    if (m_binaryOperatorSymbols.TryGetValue(symbol, out binaryOperator))
                    {
                        if (m_current.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
                        {
                            StartBinary(binaryOperator);
                            return;
                        }
                    }

                    if (unaryOperator != null || binaryOperator != null)
                        ExceptionHelper.UnexpectedExpressionTerm(symbol);
                    else
                        CompleteWithSymbol(symbol);
                }
            }
            else
                CompleteWithLiteral(term);
        }

        public Expression FeedLiteral(Object literal)
        {
            if (Constructed)
                ExceptionHelper.CannotModifyAConstructedExpression();

            FeedTerm(literal, true);
            return this;
        }

        public Expression FeedSymbol(String symbol)
        {
            Expect.NotEmpty("symbol", symbol);

            if (Constructed)
                ExceptionHelper.CannotModifyAConstructedExpression();

            FeedTerm(symbol, false);
            return this;
        }

        public void Construct()
        {
            if (!Started)
                ExceptionHelper.CannotConstructExpressionInvalidState();

            if (!Constructed)
            {
                Boolean fail = false;
                if (m_root.Parent != null)
                    fail = true;
                else
                {
                    var _currentAsRoot = m_current as RootNode;
                    if (_currentAsRoot != null)
                        fail = !_currentAsRoot.Closed;
                    else
                    {
                        var _currentAsDisem = m_current as DisembowelerNode;
                        if (_currentAsDisem != null)
                            fail = _currentAsDisem.MemberName == null;
                        else
                        {
                            fail = !(m_current is LeafNode);
                        }
                    }
                }

                if (fail)
                    ExceptionHelper.CannotConstructExpressionInvalidState();

                /* Reduce the expression if so was desired. */
                m_root.Reduce(ReduceExpressionEvaluationContext.Instance);
                m_function = m_root.GetEvaluationFunction();
            }
        }

        public Object Evaluate(IExpressionEvaluationContext context)
        {
            Expect.NotNull("context", context);

            if (!Constructed)
                ExceptionHelper.CannotEvaluateUnconstructedExpression();

            return m_function(context);
        }


        public override String ToString()
        {
            return ToString(ExpressionFormatStyle.Arithmetic);
        }

        public String ToString(ExpressionFormatStyle style)
        {
            if (m_root == null)
                return "??";
            else
                return m_root.ToString(style);
        }
    }
}

