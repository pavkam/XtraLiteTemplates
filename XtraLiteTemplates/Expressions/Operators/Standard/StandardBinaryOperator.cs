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
using System;

namespace XtraLiteTemplates
{
    using System;

    public abstract class StandardBinaryOperator : BinaryOperator
    {
        protected StandardBinaryOperator(String symbol, Int32 precedence)
            : base(symbol, precedence)
        {
        }

        public override Boolean Evaluate(Object left, Object right, out Object result)
        {
            /* Try normal operators. */
            Int64 left_i, right_i;
            Boolean left_is_i = TryAsInteger(left, out left_i);
            Boolean right_is_i = TryAsInteger(right, out right_i);

            if (left_is_i && right_is_i)
                return Evaluate(left_i, right_i, out result);

            Double left_f, right_f;
            Boolean left_is_f = TryAsFloat(left, out left_f);
            Boolean right_is_f = TryAsFloat(right, out right_f);

            if (left_is_f && right_is_f)
                return Evaluate(left_f, right_f, out result);

            Boolean left_b, right_b;
            Boolean left_is_b = TryAsBoolean(left, out left_b);
            Boolean right_is_b = TryAsBoolean(right, out right_b);

            if (left_is_b && right_is_b)
                return Evaluate(left_b, right_b, out result);

            String left_s, right_s;
            Boolean left_is_s = TryAsString(left, out left_s);
            Boolean right_is_s = TryAsString(right, out right_s);

            if (left_is_s && right_is_s)
                return Evaluate(left_s, right_s, out result);

            /* Default to Undefined. */
            result = null;
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
    }
}

