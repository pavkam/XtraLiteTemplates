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

    public class SubscriptOperator : Operator
    {
        public static SubscriptOperator Standard { get; private set; }

        static SubscriptOperator()
        {
            Standard = new SubscriptOperator("(", ")");
        }

        public String Terminator { get; private set; }

        public SubscriptOperator(String symbol, String terminator)
            : base(symbol, Int32.MaxValue, false)
        {
            Expect.NotEmpty("terminator", terminator);
            Expect.NotEqual("symbol", "terminator", symbol, terminator);

            Terminator = terminator;
        }

        public override Primitive Evaluate(Primitive arg)
        {
            result = arg;
            return true;
        }

        public override String ToString()
        {
            return String.Format("{0}{1}", Symbol, Terminator);
        }

        public override Boolean Equals(Object obj)
        {
            var objc = obj as SubscriptOperator;
            return 
                objc != null && objc.Symbol == Symbol && objc.Terminator == Terminator;
        }

        public override Int32 GetHashCode()
        {
            return Symbol.GetHashCode() ^ Terminator.GetHashCode();
        }
    }
}

