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

namespace XtraLiteTemplates.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Expressions.Operators.Standard;

    public sealed class Expression
    {
        private ExpressionNode m_current;
        private ExpressionNode m_root;
        private List<Operator> m_supportedOperators;
        private Func<IEvaluationContext, Object> m_function;
        private Stack<GroupOperatorExpressionNode> m_openGroups;
        private Dictionary<String, UnaryOperator> m_unaryOperators;
        private Dictionary<String, BinaryOperator> m_binaryOperators;
        private Dictionary<String, GroupOperator> m_startGroupOperators;
        private Dictionary<String, GroupOperator> m_endGroupOperators;


        private ExpressionNode Reduce(UnaryOperatorExpressionNode node)
        {
            Debug.Assert(node != null);

            node.Child = Reduce(node.Child);

            var childNode = node.Child as ConstantExpressionNode;
            if (childNode != null)
            {
                Object result;
                if (node.Operator.Evaluate(childNode.Operand, out result))
                    return new ConstantExpressionNode(node.Parent, result);
            }

            return node;
        }

        private ExpressionNode Reduce(GroupOperatorExpressionNode node)
        {
            Debug.Assert(node != null);

            /* Cannot be redured of reference detected. */
            node.Child = Reduce(node.Child);

            var childNode = node.Child as ConstantExpressionNode;
            if (childNode != null)
            {
                Object result;
                if (node.Operator.Evaluate(childNode.Operand, out result))
                    return new ConstantExpressionNode(node.Parent, result);
            }

            return node;
        }

        private ExpressionNode Reduce(BinaryOperatorExpressionNode node)
        {
            Debug.Assert(node != null);

            node.LeftNode = Reduce(node.LeftNode);
            node.RightNode = Reduce(node.RightNode);

            var leftNode = node.LeftNode as ConstantExpressionNode;
            var rightNode = node.RightNode as ConstantExpressionNode;
            if (leftNode != null && rightNode == null)
            {
                /* Left node has a value. Try to apply short-circuit logic here. Maybe skip the whole right side evaluation entirely. */
                Object result;
                if (node.Operator.EvaluateLeft(leftNode.Operand, out result))
                    return new ConstantExpressionNode(node.Parent, result);
            }
            
            /* Normal evaluation ensues. */
            if (leftNode != null && rightNode != null)
            {
                Object result;
                if (node.Operator.Evaluate(leftNode.Operand, rightNode.Operand, out result))
                    return new ConstantExpressionNode(node.Parent, result);
            } 
            

            return node;
        }

        private ExpressionNode Reduce(ExpressionNode node)
        {
            Debug.Assert(node != null);
            if (node is ConstantExpressionNode || node is ReferenceExpressionNode)
                return node;
            else
            {
                var unaryNode = node as UnaryOperatorExpressionNode;
                if (unaryNode != null)
                    return Reduce(unaryNode);
                else
                {
                    var binaryNode = node as BinaryOperatorExpressionNode;
                    if (binaryNode != null)
                        return Reduce(binaryNode);
                    else
                    {
                        var groupNode = node as GroupOperatorExpressionNode;
                        if (groupNode != null)
                            return Reduce(groupNode);
                    }
                }
            }

            return node;
        }

        private Func<IEvaluationContext, Object> Build(ExpressionNode node)
        {
            Debug.Assert(node != null);

            /* Build each func. */
            var constantNode = node as ConstantExpressionNode;
            if (constantNode != null)
            {
                var operand = constantNode.Operand; 
                return (context) => operand;
            }

            var referenceNode = node as ReferenceExpressionNode;
            if (referenceNode != null)
            {
                var identifier = referenceNode.Identifier;
                return (context) => context.GetVariable(identifier);
            }

            var unaryOperatorNode = node as UnaryOperatorExpressionNode;
            if (unaryOperatorNode != null)
            {
                var childFunc = Build(unaryOperatorNode.Child);
                var @operator = unaryOperatorNode.Operator;
                return (context) =>
                {
                    Object operand = childFunc(context);
                    Object result;
                    if (!@operator.Evaluate(operand, out result))
                        result = context.HandleEvaluationError(@operator, operand);
                    return result;
                };
            }

            var groupOperatorNode = node as GroupOperatorExpressionNode;
            if (groupOperatorNode != null)
            {
                var childFunc = Build(groupOperatorNode.Child);
                var @operator = groupOperatorNode.Operator;
                return (context) =>
                {
                    Object operand = childFunc(context);
                    Object result;
                    if (!@operator.Evaluate(operand, out result))
                        result = context.HandleEvaluationError(@operator, operand);
                    return result;
                };
            }

            var binaryOperatorNode = node as BinaryOperatorExpressionNode;
            if (binaryOperatorNode != null)
            {
                var leftFunc = Build(binaryOperatorNode.LeftNode);
                var rightFunc = Build(binaryOperatorNode.RightNode);
                var @operator = binaryOperatorNode.Operator;
                return (context) =>
                {
                    Object leftOperand = leftFunc(context);
                    Object rightOperand = rightFunc(context);
                    Object result;
                    if (!@operator.Evaluate(leftOperand, rightOperand, out result))
                        result = context.HandleEvaluationError(@operator, leftOperand, rightOperand);
                    return result;
                };
            }

            Debug.Assert(false, "Unreachable location.");
            return null;
        }

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
                m_unaryOperators.ContainsKey(symbol) ||
            m_binaryOperators.ContainsKey(symbol) ||
            m_startGroupOperators.ContainsKey(symbol) ||
            m_endGroupOperators.ContainsKey(symbol);
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

            m_unaryOperators = new Dictionary<String, UnaryOperator>(comparer);
            m_binaryOperators = new Dictionary<String, BinaryOperator>(comparer);
            m_startGroupOperators = new Dictionary<String, GroupOperator>();
            m_endGroupOperators = new Dictionary<String, GroupOperator>();
            m_supportedOperators = new List<Operator>();
            m_openGroups = new Stack<GroupOperatorExpressionNode>();

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
                ExpressionException.CannotRegisterOperatorsForStartedExpression();

            Debug.Assert(!Constructed);

            if (@operator is UnaryOperator)
            {
                var unaryOperator = (UnaryOperator)@operator;
                if (m_unaryOperators.ContainsKey(unaryOperator.Symbol) ||
                    m_startGroupOperators.ContainsKey(unaryOperator.Symbol) ||
                    m_endGroupOperators.ContainsKey(unaryOperator.Symbol))
                {
                    ExpressionException.OperatorAlreadyRegistered(unaryOperator);
                }

                m_unaryOperators.Add(@operator.Symbol, unaryOperator);
            }
            else if (@operator is BinaryOperator)
            {
                var binaryOperator = (BinaryOperator)@operator;
                if (m_binaryOperators.ContainsKey(binaryOperator.Symbol) ||
                    m_startGroupOperators.ContainsKey(binaryOperator.Symbol) ||
                    m_endGroupOperators.ContainsKey(binaryOperator.Symbol))
                {
                    ExpressionException.OperatorAlreadyRegistered(binaryOperator);
                }


                m_binaryOperators.Add(@operator.Symbol, binaryOperator);
            }
            else if (@operator is GroupOperator)
            {
                var groupOperator = (GroupOperator)@operator;
                if (m_unaryOperators.ContainsKey(groupOperator.Symbol) ||
                    m_unaryOperators.ContainsKey(groupOperator.Terminator) ||
                    m_binaryOperators.ContainsKey(groupOperator.Symbol) ||
                    m_binaryOperators.ContainsKey(groupOperator.Terminator) ||
                    m_startGroupOperators.ContainsKey(groupOperator.Symbol) ||
                    m_startGroupOperators.ContainsKey(groupOperator.Terminator) ||
                    m_endGroupOperators.ContainsKey(groupOperator.Symbol) ||
                    m_endGroupOperators.ContainsKey(groupOperator.Terminator))
                {
                    ExpressionException.OperatorAlreadyRegistered(@operator);
                }

                m_startGroupOperators.Add(groupOperator.Symbol, groupOperator);
                m_endGroupOperators.Add(groupOperator.Terminator, groupOperator);
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
                    (m_current is UnaryOperatorExpressionNode) ||
                    (m_current is BinaryOperatorExpressionNode) ||
                    (m_current is GroupOperatorExpressionNode && ((GroupOperatorExpressionNode)m_current).Child == null);
            }
        }

        private Boolean GroupEndOrBinaryOperatorExpected
        {
            get
            {
                return
                    (m_current is ConstantExpressionNode) ||
                    (m_current is ReferenceExpressionNode) ||
                    (m_current is GroupOperatorExpressionNode && ((GroupOperatorExpressionNode)m_current).Child != null);
            }
        }

        private void FeedTerm(Object term, Boolean isLiteral)
        {
            Debug.Assert(isLiteral || term is String);

            if (Constructed)
                ExpressionException.CannotModifyAConstructedExpression();

            if (GroupStartOrUnaryOperatorOrLiteralExpected)
            {
                ExpressionNode continuationNode = null;
                if (isLiteral)
                    continuationNode = new ConstantExpressionNode(m_current, term);
                else
                {
                    String _symbol = (String)term;

                    /* Good cases. */
                    UnaryOperator _unaryOperator;
                    if (m_unaryOperators.TryGetValue(_symbol, out _unaryOperator))
                        continuationNode = new UnaryOperatorExpressionNode(m_current, _unaryOperator);

                    GroupOperator _groupOperator;
                    if (m_startGroupOperators.TryGetValue(_symbol, out _groupOperator))
                    {
                        continuationNode = new GroupOperatorExpressionNode(m_current, _groupOperator);
                        m_openGroups.Push((GroupOperatorExpressionNode)continuationNode);
                    }

                    /* Invalid cases. */
                    if (continuationNode == null && !m_endGroupOperators.ContainsKey(_symbol) && !m_binaryOperators.ContainsKey(_symbol))
                        continuationNode = new ReferenceExpressionNode(m_current, _symbol);
                }

                if (continuationNode != null)
                {
                    var _unaryOperatorNode = m_current as UnaryOperatorExpressionNode;
                    if (_unaryOperatorNode != null)
                    {
                        Debug.Assert(_unaryOperatorNode.Child == null);
                        _unaryOperatorNode.Child = continuationNode;
                    }

                    var _binaryOperatorNode = m_current as BinaryOperatorExpressionNode;
                    if (_binaryOperatorNode != null)
                    {
                        Debug.Assert(_binaryOperatorNode.RightNode == null);
                        Debug.Assert(_binaryOperatorNode.LeftNode != null);

                        _binaryOperatorNode.RightNode = continuationNode;
                    }

                    var _groupOperatorNode = m_current as GroupOperatorExpressionNode;
                    if (_groupOperatorNode != null)
                    {
                        Debug.Assert(_groupOperatorNode.Child == null);
                        _groupOperatorNode.Child = continuationNode;
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
                    String _symbol = (String)term;

                    GroupOperator _groupOperator;
                    if (m_endGroupOperators.TryGetValue(_symbol, out _groupOperator))
                    {
                        var _currentlyOpenGroupNode = m_openGroups.Count > 0 ? m_openGroups.Pop() : null;
                        if (_currentlyOpenGroupNode != null && _currentlyOpenGroupNode.Operator == _groupOperator)
                        {
                            m_current = _currentlyOpenGroupNode;
                            return;
                        }
                    }

                    BinaryOperator _binaryOperator;
                    if (m_binaryOperators.TryGetValue(_symbol, out _binaryOperator))
                    {
                        var leftNode = m_current;

                        /* Go up the tree while the precedence allows. */
                        while (leftNode.Parent != null &&
                            ((OperatorExpressionNode)leftNode.Parent).Operator.Precedence <= _binaryOperator.Precedence)
                            leftNode = leftNode.Parent;

                        m_current = new BinaryOperatorExpressionNode(leftNode.Parent, _binaryOperator)
                        {
                            LeftNode = leftNode,
                        };

                        /* Re-jig the tree. */
                        var _binaryNode = leftNode.Parent as BinaryOperatorExpressionNode;
                        if (_binaryNode != null)
                            _binaryNode.RightNode = m_current;

                        var _groupNode = leftNode.Parent as GroupOperatorExpressionNode;
                        if (_groupNode != null)
                            _groupNode.Child = m_current;

                        var _unaryNode = leftNode.Parent as UnaryOperatorExpressionNode;
                        if (_unaryNode != null)
                            _unaryNode.Child = m_current;

                        leftNode.Parent = m_current;
                        if (m_root == leftNode)
                            m_root = m_current;
                        if (m_openGroups.Count > 0 && m_openGroups.Peek().Child == leftNode)
                            m_openGroups.Peek().Child = m_current;

                        return;
                    }
                }
            }

            ExpressionException.InvalidExpressionTerm(term);
        }

        public Expression FeedLiteral(Object literal)
        {
            if (Constructed)
                ExpressionException.CannotModifyAConstructedExpression();

            FeedTerm(literal, true);
            return this;
        }

        public Expression FeedSymbol(String symbol)
        {
            Expect.NotEmpty("symbol", symbol);

            if (Constructed)
                ExpressionException.CannotModifyAConstructedExpression();

            FeedTerm(symbol, false);
            return this;
        }

        public void Construct()
        {
            if (!Constructed)
            {
                if (GroupStartOrUnaryOperatorOrLiteralExpected || m_openGroups.Count > 0)
                    ExpressionException.CannotConstructExpressionInvalidState();

                /* Reduce the expression if so was desired. */
                m_root = Reduce(m_root);
                m_function = Build(m_root);
            }
        }

        public Object Evaluate(IEvaluationContext context)
        {
            Expect.NotNull("context", context);

            if (!Constructed)
                ExpressionException.CannotEvaluateUnconstructedExpression();

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

        public static Expression CreateStandardCStyle()
        {
            var expression = new Expression(StringComparer.Ordinal);

            expression.RegisterOperator(SubscriptOperator.CStyle);

            expression.RegisterOperator(OrOperator.CStyle);
            expression.RegisterOperator(AndOperator.CStyle);
            expression.RegisterOperator(NotOperator.CStyle);
            expression.RegisterOperator(ShiftLeftOperator.CStyle);
            expression.RegisterOperator(ShiftRightOperator.CStyle);
            expression.RegisterOperator(XorOperator.CStyle);

            expression.RegisterOperator(EqualsOperator.CStyle);
            expression.RegisterOperator(NotEqualsOperator.CStyle);
            expression.RegisterOperator(GreaterThanOperator.CStyle);
            expression.RegisterOperator(GreaterThanOrEqualsOperator.CStyle);
            expression.RegisterOperator(LowerThanOperator.CStyle);
            expression.RegisterOperator(LowerThanOrEqualsOperator.CStyle);

            expression.RegisterOperator(NeutralOperator.CStyle);
            expression.RegisterOperator(NegateOperator.CStyle);
            expression.RegisterOperator(ModuloOperator.CStyle);
            expression.RegisterOperator(DivideOperator.CStyle);
            expression.RegisterOperator(MultiplyOperator.CStyle);
            expression.RegisterOperator(SubtractOperator.CStyle);
            expression.RegisterOperator(SumOperator.CStyle);

            return expression;
        }

        public static Expression CreateStandardPascalStyle()
        {
            var expression = new Expression(StringComparer.OrdinalIgnoreCase);

            expression.RegisterOperator(SubscriptOperator.PascalStyle);

            expression.RegisterOperator(OrOperator.PascalStyle);
            expression.RegisterOperator(AndOperator.PascalStyle);
            expression.RegisterOperator(NotOperator.PascalStyle);
            expression.RegisterOperator(ShiftLeftOperator.PascalStyle);
            expression.RegisterOperator(ShiftRightOperator.PascalStyle);
            expression.RegisterOperator(XorOperator.PascalStyle);

            expression.RegisterOperator(EqualsOperator.PascalStyle);
            expression.RegisterOperator(NotEqualsOperator.PascalStyle);
            expression.RegisterOperator(GreaterThanOperator.PascalStyle);
            expression.RegisterOperator(GreaterThanOrEqualsOperator.PascalStyle);
            expression.RegisterOperator(LowerThanOperator.PascalStyle);
            expression.RegisterOperator(LowerThanOrEqualsOperator.PascalStyle);

            expression.RegisterOperator(NeutralOperator.PascalStyle);
            expression.RegisterOperator(NegateOperator.PascalStyle);
            expression.RegisterOperator(ModuloOperator.PascalStyle);
            expression.RegisterOperator(DivideOperator.PascalStyle);
            expression.RegisterOperator(MultiplyOperator.PascalStyle);
            expression.RegisterOperator(SubtractOperator.PascalStyle);
            expression.RegisterOperator(SumOperator.PascalStyle);

            return expression;
        }
    }
}

