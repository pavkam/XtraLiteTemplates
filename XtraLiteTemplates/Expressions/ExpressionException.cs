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
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions.Operators;

    public class ExpressionException : InvalidOperationException
    {
        internal ExpressionException(String format, params Object[] args)
            : base(String.Format(format, args))
        {
        }

        internal static void CannotEvaluateOperator(Operator @operator, Object constant)
        {
            Debug.Assert(@operator != null);
            throw new ExpressionException("Operator {0} could not be applied to value '{1}'.", @operator, constant);
        }

        internal static void UnexpectedExpressionNode(ExpressionNode node)
        {
            Debug.Assert(node != null);
            throw new ExpressionException("Unexpected term '{0}' encountered while building expression.", node.ToString());
        }

        internal static void UndefinedOperator(String symbol)
        {
            Debug.Assert(!String.IsNullOrEmpty(symbol));
            throw new ExpressionException("Undefined or un-supported operator '{0}' encountered while parsing expression.", symbol);
        }

        internal static void UnexpectedOperator(String symbol)
        {
            Debug.Assert(!String.IsNullOrEmpty(symbol));
            throw new ExpressionException("Unexpected operator '{0}' encountered while parsing expression.", symbol);
        }

        internal static void UnmatchedGroupOperator(String endSymbol)
        {
            Debug.Assert(!String.IsNullOrEmpty(endSymbol));
            throw new ExpressionException("Unexpected group end symbol '{0}'. No matching group found.", endSymbol);
        }



        internal static void OperatorAlreadyRegistered(Operator @operator)
        {
            Debug.Assert(@operator != null);
            throw new InvalidOperationException(String.Format("Operator identified by symbol '{0}' has already been registered with expression.", @operator));
        }

        internal static void CannotRegisterOperatorsForStartedExpression()
        {
            throw new InvalidOperationException("Operator registration must be performed before construction.");
        }

        internal static void CannotModifyAConstructedExpression()
        {
            throw new InvalidOperationException("Cannot modify a contructed expression.");
        }

        internal static void CannotCloseExpressionInvalidState(Operator @operator)
        {
            Debug.Assert(@operator != null);
            throw new ExpressionException("Expression cannot be finalized, it is not balanced. End operator is '{0}'.", @operator);
        }

        internal static void CannotEvaluateUnconstructedExpression()
        {
            throw new InvalidOperationException("Expression has not been contructed.");
        }
    }
}