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

    public abstract class StandardUnaryOperator : UnaryOperator
    {
        protected StandardUnaryOperator(String symbol)
            : base(symbol)
        {
        }

        public override Boolean Evaluate(Object arg, out Object result)
        {
            /* Try normal operators. */
            Int64 arg_i;
            if (TryAsInteger(arg, out arg_i))
                return Evaluate(arg_i, out result);
            Double arg_f;
            if (TryAsFloat(arg, out arg_f))
                return Evaluate(arg_f, out result);
            Boolean arg_b;
            if (TryAsBoolean(arg, out arg_b))
                return Evaluate(arg_f, out result);
            String arg_s;
            if (TryAsString(arg, out arg_s))
                return Evaluate(arg_f, out result);

            /* Default to Undefined. */
            result = null;
            return false;
        }

        public virtual Boolean Evaluate(String arg, out Object result)
        {
            result = null;
            return false;
        }

        public virtual Boolean Evaluate(Int64 arg, out Object result)
        {
            result = null;
            return false;
        }

        public virtual Boolean Evaluate(Double arg, out Object result)
        {
            result = null;
            return false;
        }

        public virtual Boolean Evaluate(Boolean arg, out Object result)
        {
            result = null;
            return false;
        }
    }
}

