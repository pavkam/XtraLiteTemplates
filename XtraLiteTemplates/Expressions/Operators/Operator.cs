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

namespace XtraLiteTemplates.Expressions.Operators
{
    using System;

    public abstract class Operator
    {
        public String Symbol { get; private set; }

        public Int32 Precedence { get; private set; }

        public OperatorApplicability Applicability { get; private set; }

        protected Operator(String symbol, Int32 precedence, OperatorApplicability applicability)
        {
            Expect.NotEmpty("symbol", symbol);
            Expect.GreaterThanOrEqual("precedence", precedence, 0);
            Expect.GreaterThan("applicability", (int)applicability, 0);

            Applicability = applicability;
            Symbol = symbol;
            Precedence = precedence;
        }

        protected Operator(String symbol, Int32 precedence)
            : this(symbol, precedence, OperatorApplicability.Anything)
        {
        }

        protected static Boolean TryAsInteger(Object obj, out Int64 result)
        {
            var success = true;
            result = default(Int64);

            if (obj is Byte)
                result = (Byte)obj;
            else if (obj is SByte)
                result = (SByte)obj;
            else if (obj is Int16)
                result = (Int16)obj;
            else if (obj is UInt16)
                result = (UInt16)obj;
            else if (obj is Int32)
                result = (Int32)obj;
            else if (obj is UInt32)
                result = (UInt32)obj;
            else if (obj is Int64)
                result = (Int64)obj;
            else if (obj is UInt64)
                result = (Int64)obj;
            else
                success = false;

            return success;
        }

        protected static Boolean TryAsFloat(Object obj, out Double result)
        {
            var success = true;
            result = default(Double);

            if (obj is Byte)
                result = (Byte)obj;
            else if (obj is SByte)
                result = (SByte)obj;
            else if (obj is Int16)
                result = (Int16)obj;
            else if (obj is UInt16)
                result = (UInt16)obj;
            else if (obj is Int32)
                result = (Int32)obj;
            else if (obj is UInt32)
                result = (UInt32)obj;
            else if (obj is Int64)
                result = (Int64)obj;
            else if (obj is UInt64)
                result = (Int64)obj;
            else if (obj is Single)
                result = (Single)obj;
            else if (obj is Double)
                result = (Double)obj;
            else if (obj is Decimal)
                result = Convert.ToDouble((Decimal)obj);
            else
                success = false;

            return success;
        }

        protected static Boolean TryAsBoolean(Object obj, out Boolean result)
        {
            var success = true;
            result = default(Boolean);

            if (obj is Boolean)
                result = (Boolean)obj;
            else
                success = false;

            return success;
        }

        protected static Boolean TryAsString(Object obj, out String result)
        {
            var success = true;
            result = String.Empty;

            if (obj is String)
                result = (String)obj;
            else 
                success = false;

            return success;
        }

        public override String ToString()
        {
            return Symbol;
        }

        public override Boolean Equals(Object obj)
        {
            var objc = obj as Operator;
            return 
                objc != null && objc.Symbol == Symbol;
        }

        public override Int32 GetHashCode()
        {
            return Symbol.GetHashCode();
        }
    }
}

