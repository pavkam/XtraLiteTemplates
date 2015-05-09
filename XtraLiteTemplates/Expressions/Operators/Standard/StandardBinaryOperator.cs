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

namespace XtraLiteTemplates.Expressions.Operators.Standard
{
    using System;

    public abstract class StandardBinaryOperator : BinaryOperator
    {
        protected StandardBinaryOperator(String symbol, Int32 precedence)
            : base(symbol, precedence, Associativity.LeftToRight, false, false)
        {
        }

        public override Boolean Evaluate(Object left, Object right, out Object result)
        {
            result = null;

            if (left != null)
            {
                Int64 _leftInteger;
                Int64 _rightInteger;
                if (TryAsInteger(left, out _leftInteger) && TryAsInteger(right, out _rightInteger))
                    return Evaluate(_leftInteger, _rightInteger, out result);
                Boolean _leftBoolean;
                Boolean _rightBoolean;
                if (TryAsBoolean(left, out _leftBoolean) && TryAsBoolean(right, out _rightBoolean))
                    return Evaluate(_leftBoolean, _rightBoolean, out result);
                Double _leftFloat;
                Double _rightFloat;
                if (TryAsFloat(left, out _leftFloat) && TryAsFloat(right, out _rightFloat))
                    return Evaluate(_leftFloat, _rightFloat, out result);
                String _leftString;
                String _rightString;
                if (TryAsString(left, out _leftString) && TryAsString(right, out _rightString))
                    return Evaluate(_leftString, _rightString, out result);
            }

            return false;
        }

        protected virtual Boolean Evaluate(Int64 left, Int64 right, out Object result)
        {
            result = null;
            return false;
        }

        protected virtual Boolean Evaluate(Double left, Double right, out Object result)
        {
            result = null;
            return false;
        }

        protected virtual Boolean Evaluate(Boolean left, Boolean right, out Object result)
        {
            result = null;
            return false;
        }

        protected virtual Boolean Evaluate(String left, String right, out Object result)
        {
            result = null;
            return false;
        }


        public override Boolean Evaluate(Object arg, out Object result)
        {
            result = null;

            if (arg != null)
            {
                Int64 _integer;
                if (TryAsInteger(arg, out _integer))
                    return EvaluateLeft(_integer, out result);
                Boolean _boolean;
                if (TryAsBoolean(arg, out _boolean))
                    return EvaluateLeft(_boolean, out result);
                Double _float;
                if (TryAsFloat(arg, out _float))
                    return EvaluateLeft(_float, out result);
                String _string;
                if (TryAsString(arg, out _string))
                    return EvaluateLeft(_string, out result);
            }

            return false;
        }

        protected virtual Boolean EvaluateLeft(String left, out Object result)
        {
            result = left;
            return false;
        }

        protected virtual Boolean EvaluateLeft(Int64 left, out Object result)
        {
            result = left;
            return false;
        }

        protected virtual Boolean EvaluateLeft(Double left, out Object result)
        {
            result = left;
            return false;
        }

        protected virtual Boolean EvaluateLeft(Boolean left, out Object result)
        {
            result = left;
            return false;
        }
    }
}

