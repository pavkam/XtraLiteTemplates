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

namespace XtraLiteTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;

    public class Expression
    {
        public enum FormattingStyle
        {
            Arithmetic,
            Polish,
            Canonical,
        }

        private ExpressionNode m_current;
        private ExpressionNode m_root;
        private Dictionary<String, UnaryOperator> m_unaryOperators;
        private Dictionary<String, BinaryOperator> m_binaryOperators;
        private Dictionary<String, GroupOperator> m_startGroupOperators;
        private Dictionary<String, GroupOperator> m_endGroupOperators;

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


        public void RegisterOperator(BinaryOperator @operator)
        {
            Expect.NotNull("operator", @operator);

            m_binaryOperators.Add(@operator.Symbol, @operator);
        }

        public void RegisterOperator(UnaryOperator @operator)
        {
            Expect.NotNull("operator", @operator);

            m_unaryOperators.Add(@operator.Symbol, @operator);
        }

        public void RegisterOperator(GroupOperator @operator)
        {
            Expect.NotNull("operator", @operator);

            m_startGroupOperators.Add(@operator.Symbol, @operator);
            m_endGroupOperators.Add(@operator.Terminator, @operator);
        }

        public void FeedConstant(Object constant)
        {
            if (m_current == null)
            {
                /* This is the first node ever. */
                m_current = new ConstantExpressionNode(null, constant);
            }
            else
            {
                /* Check that the current node is not a previous constant. */
                if (m_current is ConstantExpressionNode)
                    ExpressionException.UnexpectedConstant(constant);

                var unary = m_current as UnaryOperatorExpressionNode;
                if (unary != null)
                {
                    Debug.Assert(unary.OperandNode == null);
                    unary.OperandNode = new ConstantExpressionNode(unary, constant);

                    m_current = unary.OperandNode;
                }

                var group = m_current as GroupOperatorExpressionNode;
                if (group != null && group.FirstOperandNode == null)
                {
                    group.FirstOperandNode = new ConstantExpressionNode(group, constant);
                    m_current = group.FirstOperandNode;
                }

                var binary = m_current as BinaryOperatorExpressionNode;
                if (binary != null)
                {
                    Debug.Assert(binary.LeftOperandNode != null);
                    Debug.Assert(binary.RightOperandNode == null);
                    binary.RightOperandNode = new ConstantExpressionNode(binary, constant);

                    m_current = binary.RightOperandNode;
                }
            }
        }

        public void FeedSymbol(String symbol)
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

                Debug.Assert(unary == null || unary.OperandNode == null);
                Debug.Assert(binary == null || binary.RightOperandNode == null);
                Debug.Assert(binary == null || binary.LeftOperandNode != null);

                if (group != null && group.FirstOperandNode != null)
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
                    unary.OperandNode = m_current;
                else if (group != null)
                    group.FirstOperandNode = m_current;
                else if (binary != null)
                    binary.RightOperandNode = m_current;
                else
                    Debug.Assert(false);
            }
            else if (m_current is ConstantExpressionNode || m_current is GroupOperatorExpressionNode)
            {
                if (m_current is GroupOperatorExpressionNode && ((GroupOperatorExpressionNode)m_current).FirstOperandNode == null)
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

                        group.FirstOperandNode = root;

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
                        LeftOperandNode = left,
                    };
                
                    if (left.Parent is BinaryOperatorExpressionNode)
                        (left.Parent as BinaryOperatorExpressionNode).RightOperandNode = m_current;
                    left.Parent = m_current;
                }
            }
            else
                Debug.Assert(false);
        }

        public void Close()
        {
            m_root = m_current;
            while (m_root != null && m_root.Parent != null)
                m_root = m_root.Parent;
        }

        private String ToString(ExpressionNode node, FormattingStyle style)
        {
            Debug.Assert(node != null);

            var unary = node as UnaryOperatorExpressionNode;
            if (unary != null)
                return String.Format("{0}{1}", unary.Operator, ToString(unary.OperandNode, style));
            
            var binary = node as BinaryOperatorExpressionNode;
            if (binary != null)
            {
                if (style == FormattingStyle.Arithmetic)
                {
                    return String.Format("{0} {1} {2}", ToString(binary.LeftOperandNode, style),
                        binary.Operator.Symbol, ToString(binary.RightOperandNode, style));
                } 
                else if (style == FormattingStyle.Polish)
                {
                    return String.Format("{0} {1} {2}",
                        binary.Operator, ToString(binary.LeftOperandNode, style), ToString(binary.RightOperandNode, style));
                } 
                else
                {
                    return String.Format("{0}{{{1},{2}}}",
                        binary.Operator, ToString(binary.LeftOperandNode, style), ToString(binary.RightOperandNode, style));
                }
            }

            var group = node as GroupOperatorExpressionNode;
            if (group != null)
            {
                if (style == FormattingStyle.Arithmetic)
                {
                    return String.Format("{0} {1} {2}", group.Operator.Symbol,
                        ToString(group.FirstOperandNode, style), group.Operator.Terminator);
                }
                else if (style == FormattingStyle.Polish)
                {
                    return String.Format("{0}{1}{2}", group.Operator.Symbol,
                        ToString(group.FirstOperandNode, style), group.Operator.Terminator);
                }
                else
                {
                    return String.Format("{0}{{{1}}}", group.Operator, ToString(group.FirstOperandNode, style));
                }
            }

            var constant = node as ConstantExpressionNode;
            if (constant != null)
            {
                return constant.Operand.ToString();
            }

            return null;
        }

        public override String ToString()
        {
            return ToString(FormattingStyle.Arithmetic);
        }

        public String ToString(FormattingStyle style)
        {
            if (m_root != null)
                return ToString(m_root, style);
            else
                return null;
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

