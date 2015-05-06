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
        private Dictionary<String, UnaryOperator> m_unaryOperators;
        private Dictionary<String, BinaryOperator> m_binaryOperators;
        private Dictionary<String, GroupOperator> m_startGroupOperators;
        private Dictionary<String, GroupOperator> m_endGroupOperators;

        private Boolean ConstantOrReferenceAcceptedNext
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

        private void AppendChild(Object value, Boolean isReference)
        {
            Debug.Assert(!isReference || value is String);

            ExpressionNode node = isReference ?
                (ExpressionNode)(new ReferenceExpressionNode(m_current, (String)value)) :
                new ConstantExpressionNode(m_current, value);

            if (!ConstantOrReferenceAcceptedNext)
                ExpressionException.UnexpectedExpressionNode(node);

            if (m_current != null)
            {
                var unaryOperator = m_current as UnaryOperatorExpressionNode;
                if (unaryOperator != null)
                {
                    Debug.Assert(unaryOperator.Child == null);
                    unaryOperator.Child = node;
                }
                else
                {
                    var groupOperator = m_current as GroupOperatorExpressionNode;
                    if (groupOperator != null)
                        groupOperator.Child = node;
                    else
                    {
                        var binaryOperator = m_current as BinaryOperatorExpressionNode;
                        if (binaryOperator != null)
                        {
                            Debug.Assert(binaryOperator.LeftNode != null);
                            Debug.Assert(binaryOperator.RightNode == null);

                            binaryOperator.RightNode = node;
                        }
                    }
                }
            }

            m_current = node;
        }

        private ExpressionNode Reduce(UnaryOperatorExpressionNode node)
        {
            Debug.Assert(node != null);

            node.Child = Reduce(node.Child);

            var childNode = node.Child as ConstantExpressionNode;
            if (childNode != null)
            {
                Object result;
                if (!node.Operator.Evaluate(childNode.Operand, out result))
                    ExpressionException.CannotEvaluateOperator(node.Operator, childNode.Operand);
                else
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
                if (!node.Operator.Evaluate(childNode.Operand, out result))
                    ExpressionException.CannotEvaluateOperator(node.Operator, childNode.Operand);
                else
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
                if (!node.Operator.Evaluate(leftNode.Operand, rightNode.Operand, out result))
                    ExpressionException.CannotEvaluateOperator(node.Operator, leftNode.Operand);
                else
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

        public Boolean Closed
        {
            get
            {
                return m_root != null;
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

        public IEqualityComparer<String> Comparer { get; private set; }

        public Expression(IEqualityComparer<String> comparer)
        {
            Expect.NotNull("comparer", comparer);

            m_unaryOperators = new Dictionary<String, UnaryOperator>(comparer);
            m_binaryOperators = new Dictionary<String, BinaryOperator>(comparer);
            m_startGroupOperators = new Dictionary<String, GroupOperator>();
            m_endGroupOperators = new Dictionary<String, GroupOperator>();

            Comparer = comparer;
        }

        public Expression()
            : this(StringComparer.CurrentCultureIgnoreCase)
        {
        }


        public Expression RegisterOperator(Operator @operator)
        {
            Expect.NotNull("operator", @operator);

            if (Started)
                ExpressionException.CannotAddMoreOperatorsExpressionStarted();

            Debug.Assert(!Closed);

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

            return this;
        }



        public Expression FeedConstant(Object constant)
        {
            if (Closed)
                ExpressionException.CannotFeedMoreToExpressionClosed();

            AppendChild(constant, false);

            return this;
        }

        public Expression FeedSymbol(String symbol)
        {
            Expect.NotEmpty("symbol", symbol);

            if (Closed)
                ExpressionException.CannotFeedMoreToExpressionClosed();

            if (!IsSupportedOperator(symbol))
            {
                /* Consider this to be a reference. */
                AppendChild(symbol, true);
            }
            else
            {
                /* Identify the previous node first, and based on that decide what type of operator to expect. */
                if (m_current == null ||
                    m_current is UnaryOperatorExpressionNode ||
                    m_current is BinaryOperatorExpressionNode ||
                    m_current is GroupOperatorExpressionNode)
                {
                    var unary = m_current as UnaryOperatorExpressionNode;
                    var binary = m_current as BinaryOperatorExpressionNode;
                    var group = m_current as GroupOperatorExpressionNode;

                    Debug.Assert(unary == null || unary.Child == null);
                    Debug.Assert(binary == null || binary.RightNode == null);
                    Debug.Assert(binary == null || binary.LeftNode != null);

                    if (group != null && group.Child != null)
                        ExpressionException.UnexpectedOperator(symbol);

                    /* Binary operations only can be applied here. */
                    GroupOperator matchingGroup;
                    if (m_startGroupOperators.TryGetValue(symbol, out matchingGroup))
                    {
                        /* This be a group-start symbol. Find the matching group. */
                        m_current = new GroupOperatorExpressionNode(m_current, matchingGroup);
                    }
                    else
                    {
                        /* Only unary operators are allowed. */
                        UnaryOperator matchingUnary;
                        if (!m_unaryOperators.TryGetValue(symbol, out matchingUnary))
                            ExpressionException.UndefinedOperator(symbol);

                        m_current = new UnaryOperatorExpressionNode(unary, matchingUnary);
                    }

                    /* Attach the node. */
                    if (unary != null)
                        unary.Child = m_current;
                    else if (group != null)
                        group.Child = m_current;
                    else if (binary != null)
                        binary.RightNode = m_current;
                    else
                        Debug.Assert(false);
                }
                else if (m_current is ConstantExpressionNode || m_current is ReferenceExpressionNode || m_current is GroupOperatorExpressionNode)
                {
                    if (m_current is GroupOperatorExpressionNode && ((GroupOperatorExpressionNode)m_current).Child == null)
                        ExpressionException.UnexpectedOperator(symbol);

                    /* Binary operations only can be applied here. */
                    GroupOperator matchingGroup;
                    if (m_endGroupOperators.TryGetValue(symbol, out matchingGroup))
                    {
                        /* This be a group-end symbol. Find the matching group. */
                        GroupOperatorExpressionNode group = null;
                        var _current = m_current;
                        while (_current.Parent != null)
                        {
                            _current = _current.Parent;
                            group = _current as GroupOperatorExpressionNode;
                            if (group != null)
                                break;
                        }

                        if (group == null || group.Operator.Terminator != symbol)
                            ExpressionException.UnmatchedGroupOperator(symbol);
                        else
                        {
                            /* Find root of the group. */
                            var root = m_current;
                            while (root.Parent != group)
                                root = root.Parent;

                            group.Child = root;

                            m_current = group;
                        }
                    }
                    else
                    {
                        /* Binary operations only can be applied here. */
                        BinaryOperator matchingBinary;
                        if (!m_binaryOperators.TryGetValue(symbol, out matchingBinary))
                            ExpressionException.UndefinedOperator(symbol);

                        /* Apply precendence levels here. Go up the tree while the precendence is greater or equal. */
                        ExpressionNode left = m_current, parent = m_current.Parent;
                        while (left.Parent != null)
                        {
                            Int32 prec = ((OperatorExpressionNode)left.Parent).Operator.Precedence;
                            if (prec > matchingBinary.Precedence)
                                break;

                            left = left.Parent;
                            parent = left.Parent;
                        }

                        m_current = new BinaryOperatorExpressionNode(parent, matchingBinary)
                        {
                            LeftNode = left,
                        };

                        if (left.Parent is BinaryOperatorExpressionNode)
                            (left.Parent as BinaryOperatorExpressionNode).RightNode = m_current;
                        left.Parent = m_current;
                    }
                }
                else
                    Debug.Assert(false);
            }

            return this;
        }

        public Expression Close(Boolean reduce)
        {
            if (!Started)
                ExpressionException.CannotCloseExpressionNotYetStarted();
            if (!Closed)
            {
                if (m_current is OperatorExpressionNode)
                {
                    var groupNode = m_current as GroupOperatorExpressionNode;
                    if (groupNode == null || groupNode.Child == null)
                        ExpressionException.CannotCloseExpressionInvalidState(((OperatorExpressionNode)m_current).Operator);
                }

                /* The building process has ended. Find the root. */
                m_root = m_current;
                while (m_root.Parent != null)
                    m_root = m_root.Parent;

                /* Reduce the expression if so was desired. */
                if (reduce)
                    m_root = Reduce(m_root);
            }

            return this;
        }

        public Expression Close()
        {
            return Close(true);
        }


        public override String ToString()
        {
            return ToString(ExpressionFormatStyle.Arithmetic);
        }

        public String ToString(ExpressionFormatStyle style)
        {
            if (!Closed)
                ExpressionException.CannotUseExpressionNotClosed();

            Debug.Assert(Started);

            return m_root.ToString(style);
        }



        public static Expression CreateStandardCStyle()
        {
            return CreateStandardCStyle(StringComparer.InvariantCultureIgnoreCase);
        }

        public static Expression CreateStandardCStyle(IEqualityComparer<String> comparer)
        {
            Expect.NotNull("comparer", comparer);

            var expression = new Expression(comparer);

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
            return CreateStandardPascalStyle(StringComparer.InvariantCultureIgnoreCase);
        }

        public static Expression CreateStandardPascalStyle(IEqualityComparer<String> comparer)
        {
            Expect.NotNull("comparer", comparer);

            var expression = new Expression(comparer);

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

