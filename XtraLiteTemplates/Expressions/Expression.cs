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

namespace XtraLiteTemplates.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions.Nodes;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Dialects.Standard.Operators;

    public sealed class Expression
    {
        private ExpressionNode m_current;
        private ExpressionNode m_root;
        private List<Operator> m_supportedOperators;
        private Func<IExpressionEvaluationContext, Object> m_function;
        private Stack<SubscriptNode> m_openGroups;
        private Dictionary<String, UnaryOperator> m_unaryOperatorSymbols;
        private Dictionary<String, BinaryOperator> m_binaryOperatorSymbols;
        private Dictionary<String, SubscriptOperator> m_subscriptOperatorSymbols;
        private Dictionary<String, SubscriptOperator> m_subscriptOperatorsTerminators;

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
                m_unaryOperatorSymbols.ContainsKey(symbol) ||
                m_binaryOperatorSymbols.ContainsKey(symbol) ||
                m_subscriptOperatorSymbols.ContainsKey(symbol) ||
                m_subscriptOperatorsTerminators.ContainsKey(symbol);
        }

        public IReadOnlyList<Operator> SupportedOperators
        {
            get
            {
                return m_supportedOperators;
            }
        }


        public IEqualityComparer<String> Comparer { get; private set; }

        public Expression(IEqualityComparer<String> comparer)
        {
            Expect.NotNull("comparer", comparer);

            m_unaryOperatorSymbols = new Dictionary<String, UnaryOperator>(comparer);
            m_binaryOperatorSymbols = new Dictionary<String, BinaryOperator>(comparer);
            m_subscriptOperatorSymbols = new Dictionary<String, SubscriptOperator>();
            m_subscriptOperatorsTerminators = new Dictionary<String, SubscriptOperator>();
            m_supportedOperators = new List<Operator>();
            m_openGroups = new Stack<SubscriptNode>();

            Comparer = comparer;
        }

        public Expression()
            : this(StringComparer.OrdinalIgnoreCase)
        {
        }

        public Expression RegisterOperator(Operator @operator)
        {
            Expect.NotNull("operator", @operator);

            if (Started)
                ExceptionHelper.CannotRegisterOperatorsForStartedExpression();

            Debug.Assert(!Constructed);

            if (@operator is UnaryOperator)
            {
                var unaryOperator = (UnaryOperator)@operator;
                if (m_unaryOperatorSymbols.ContainsKey(unaryOperator.Symbol) ||
                    m_subscriptOperatorSymbols.ContainsKey(unaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(unaryOperator);
                }

                m_unaryOperatorSymbols.Add(@operator.Symbol, unaryOperator);
            }
            else if (@operator is BinaryOperator)
            {
                var binaryOperator = (BinaryOperator)@operator;
                if (m_binaryOperatorSymbols.ContainsKey(binaryOperator.Symbol) ||
                    m_subscriptOperatorsTerminators.ContainsKey(binaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(binaryOperator);
                }


                m_binaryOperatorSymbols.Add(@operator.Symbol, binaryOperator);
            }
            else if (@operator is SubscriptOperator)
            {
                var groupOperator = (SubscriptOperator)@operator;
                if (m_unaryOperatorSymbols.ContainsKey(groupOperator.Symbol) ||
                    m_binaryOperatorSymbols.ContainsKey(groupOperator.Terminator) ||
                    m_subscriptOperatorSymbols.ContainsKey(groupOperator.Symbol) ||
                    m_subscriptOperatorSymbols.ContainsKey(groupOperator.Terminator) ||
                    m_subscriptOperatorsTerminators.ContainsKey(groupOperator.Symbol) ||
                    m_subscriptOperatorsTerminators.ContainsKey(groupOperator.Terminator))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(@operator);
                }

                m_subscriptOperatorSymbols.Add(groupOperator.Symbol, groupOperator);
                m_subscriptOperatorsTerminators.Add(groupOperator.Terminator, groupOperator);
            }
            else
                Debug.Assert(false, "Unsupported operator type.");

            m_supportedOperators.Add(@operator);
            return this;
        }

        private Boolean GroupStartOrUnaryOperatorOrLiteralExpected
        {
            get
            {
                return
                    (m_current == null) ||
                (m_current is UnaryOperatorNode) ||
                (m_current is BinaryOperatorNode) ||
                (m_current is SubscriptNode && ((SubscriptNode)m_current).RightNode == null);
            }
        }

        private Boolean GroupEndOrBinaryOperatorExpected
        {
            get
            {
                return
                    (m_current is LeafNode) ||
                (m_current is SubscriptNode && ((SubscriptNode)m_current).RightNode != null);
            }
        }


        private void FeedTerm(Object term, Boolean isLiteral)
        {
            Debug.Assert(isLiteral || term is String);

            if (Constructed)
                ExceptionHelper.CannotModifyAConstructedExpression();

            if (GroupStartOrUnaryOperatorOrLiteralExpected)
            {
                ExpressionNode continuationNode = null;
                if (isLiteral)
                    continuationNode = new LeafNode(m_current, term, LeafNode.EvaluationType.Literal);
                else
                {
                    var _symbol = (String)term;

                    /* Good cases. */
                    UnaryOperator _unaryOperator;
                    if (m_unaryOperatorSymbols.TryGetValue(_symbol, out _unaryOperator))
                        continuationNode = new UnaryOperatorNode(m_current, _unaryOperator);

                    SubscriptOperator _groupOperator;
                    if (m_subscriptOperatorSymbols.TryGetValue(_symbol, out _groupOperator))
                    {
                        continuationNode = new SubscriptNode(m_current, _groupOperator);
                        m_openGroups.Push((SubscriptNode)continuationNode);
                    }

                    /* Invalid cases. */
                    if (continuationNode == null && !m_subscriptOperatorsTerminators.ContainsKey(_symbol) && !m_binaryOperatorSymbols.ContainsKey(_symbol))
                    {
                        var evaluationType = m_current != null && ((OperatorNode)m_current).Operator.ExpectRhsIdentifier ? 
                            LeafNode.EvaluationType.Indentifier : LeafNode.EvaluationType.Variable;
                        continuationNode = new LeafNode(m_current, _symbol, evaluationType);
                    }
                }

                if (continuationNode != null)
                {
                    var operatorNode = m_current as OperatorNode;
                    if (operatorNode != null)
                    {
                        Debug.Assert(operatorNode.RightNode == null);

                        if (operatorNode.Operator.ExpectRhsIdentifier && (isLiteral || !(continuationNode is LeafNode)))
                            ExceptionHelper.UnexpectedExpressionTerm(term);
                        
                        operatorNode.RightNode = continuationNode;
                    }

                    m_current = continuationNode;
                    m_root = m_root ?? m_current;

                    return;
                }
            }
            else if (GroupEndOrBinaryOperatorExpected)
            {
                if (!isLiteral)
                {
                    var _symbol = (String)term;

                    SubscriptOperator _groupOperator;
                    if (m_subscriptOperatorsTerminators.TryGetValue(_symbol, out _groupOperator))
                    {
                        var _currentlyOpenGroupNode = m_openGroups.Count > 0 ? m_openGroups.Pop() : null;
                        if (_currentlyOpenGroupNode != null && _currentlyOpenGroupNode.Operator == _groupOperator)
                        {
                            m_current = _currentlyOpenGroupNode;
                            return;
                        }
                    }
                    /*
                    if (m_current is LeafNode && ((LeafNode)m_current).Evaluation == LeafNode.EvaluationType.Variable &&
                        m_startGroupOperators.TryGetValue(_symbol, out _groupOperator) && _groupOperator.Function)
                    {
                        m_openGroups.Push(_groupOperator);

                        m_current = _currentlyOpenGroupNode;
                        return;
                    }
*/
                    BinaryOperator _binaryOperator;
                    if (m_binaryOperatorSymbols.TryGetValue(_symbol, out _binaryOperator))
                    {
                        var leftNode = m_current;

                        var comparand = _binaryOperator.Associativity == Associativity.LeftToRight ? 0 : -1;

                        /* Go up the tree while the precedence allows. */
                        while (leftNode.Parent != null &&
                               ((OperatorNode)leftNode.Parent).Operator.Precedence.CompareTo(_binaryOperator.Precedence) <= comparand)
                        {
                            leftNode = leftNode.Parent;
                        }

                        var leftNodeParentOperatorNode = leftNode.Parent as OperatorNode;

                        /* Check that we do not push the RHS identifier off the parent. */
                        if (leftNodeParentOperatorNode != null && leftNodeParentOperatorNode.Operator.ExpectRhsIdentifier)
                            ExceptionHelper.UnexpectedExpressionTerm(_symbol);

                        if (_binaryOperator.ExpectLhsIdentifier)
                        {
                            var leftLeafNode = leftNode as LeafNode;
                            if (leftLeafNode == null || leftLeafNode.Evaluation == LeafNode.EvaluationType.Literal)
                                ExceptionHelper.UnexpectedExpressionTerm(_symbol);
                            else if (leftLeafNode.Evaluation == LeafNode.EvaluationType.Variable)
                                leftLeafNode.ConvertToIdentifier();
                        }

                        m_current = new BinaryOperatorNode(leftNode.Parent, _binaryOperator)
                        {
                            LeftNode = leftNode,
                        };

                        /* Re-jig the tree. */
                        if (leftNodeParentOperatorNode != null)
                            leftNodeParentOperatorNode.RightNode = m_current;
                       
                        leftNode.Parent = m_current;
                        if (m_root == leftNode)
                            m_root = m_current;
                        if (m_openGroups.Count > 0 && m_openGroups.Peek().RightNode == leftNode)
                            m_openGroups.Peek().RightNode = m_current;

                        return;
                    }
                }
            }

            ExceptionHelper.InvalidExpressionTerm(term);
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
            if (!Constructed)
            {
                if (GroupStartOrUnaryOperatorOrLiteralExpected || m_openGroups.Count > 0)
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

