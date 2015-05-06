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
using System.Diagnostics;


namespace XtraLiteTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class ExpressionBuilder
    {
        private readonly ISet<String> m_unaryOperators;
        private readonly IDictionary<String, Int32> m_binaryOperators;
        private readonly IDictionary<String, String> m_startGroupOperators;
        private readonly IDictionary<String, String> m_endGroupOperators;

        private ExpressionNode m_currentNode;

        public ExpressionBuilder()
        {
            m_unaryOperators = new HashSet<String>();
            m_binaryOperators = new Dictionary<String, Int32>();
            m_startGroupOperators = new Dictionary<String, String>();
            m_endGroupOperators = new Dictionary<String, String>();
        }

        public void RegisterUnaryOperator(String symbol)
        {
            Expect.NotEmpty("symbol", symbol);
            Expect.IsTrue("symbol charcter set", symbol.All(c => !char.IsNumber(c) && !char.IsWhiteSpace(c)));

            Expect.IsTrue("symbol not in use", 
                !m_unaryOperators.Contains(symbol) &&
                !m_binaryOperators.ContainsKey(symbol) &&
                !m_startGroupOperators.ContainsKey(symbol) &&
                !m_endGroupOperators.ContainsKey(symbol));

            m_unaryOperators.Add(symbol);
        }

        public void RegisterBinaryOperator(String symbol, Int16 precedence)
        {
            Expect.NotEmpty("symbol", symbol);
            Expect.IsTrue("symbol charcter set", symbol.All(c => !char.IsNumber(c) && !char.IsWhiteSpace(c)));

            Expect.IsTrue("symbol not in use", 
                !m_unaryOperators.Contains(symbol) &&
                !m_binaryOperators.ContainsKey(symbol) &&
                !m_startGroupOperators.ContainsKey(symbol) &&
                !m_endGroupOperators.ContainsKey(symbol));
            
            m_binaryOperators.Add(symbol, precedence);
        }

        public void RegisterGroupOperator(String startSymbol, String endSymbol)
        {
            Expect.NotEmpty("startSymbol", startSymbol);
            Expect.IsTrue("startSymbol charcter set", startSymbol.All(c => !char.IsNumber(c) && !char.IsWhiteSpace(c)));
            Expect.NotEmpty("endSymbol", startSymbol);
            Expect.IsTrue("endSymbol charcter set", endSymbol.All(c => !char.IsNumber(c) && !char.IsWhiteSpace(c)));

            Expect.IsTrue("startSymbol not in use", 
                !m_unaryOperators.Contains(startSymbol) &&
                !m_binaryOperators.ContainsKey(startSymbol) &&
                !m_startGroupOperators.ContainsKey(startSymbol) &&
                !m_endGroupOperators.ContainsKey(startSymbol));

            Expect.IsTrue("endSymbol not in use", 
                !m_unaryOperators.Contains(endSymbol) &&
                !m_binaryOperators.ContainsKey(endSymbol) &&
                !m_startGroupOperators.ContainsKey(endSymbol) &&
                !m_endGroupOperators.ContainsKey(endSymbol));
            
            m_startGroupOperators.Add(startSymbol, endSymbol);
            m_endGroupOperators.Add(endSymbol, startSymbol);
        }


        public void BeginExpression()
        {
            /* Reset the current node. */
            m_currentNode = null;
        }

        public void FeedInteger(Int64 value)
        {
            if (m_currentNode == null)
            {
                /* This is the first node ever. */
                m_currentNode = new IntegerExpressionNode(null, value);
            }
            else
            {
                var unaryOperator = m_currentNode as UnaryOperatorExpressionNode;
                var binaryOperator = m_currentNode as BinaryOperatorExpressionNode;

                if (unaryOperator != null)
                {
                    if (unaryOperator.Operand != null)
                        throw new InvalidOperationException("Unexpected interger constant. The unary operator already filled.");
                    else
                        unaryOperator.Operand = new IntegerExpressionNode(unaryOperator, value);
                }
                else if (binaryOperator)
                {
                    Debug.Assert(binaryOperator.LeftOperand != null);

                    if (binaryOperator.RightOperand != null)
                        throw new InvalidOperationException("Unexpected interger constant. The binary operator already filled.");
                    else
                        unaryOperator.Operand = new IntegerExpressionNode(unaryOperator, value);
                }
                else
                {
                    throw new InvalidOperationException("Unexpected interger constant.");
                }
            }
        }

        public void FeedFloat(Double value)
        {
        }

        public void FeedString(String value)
        {
        }

        public void Feed(String something)
        {
            
        }
    }
}

