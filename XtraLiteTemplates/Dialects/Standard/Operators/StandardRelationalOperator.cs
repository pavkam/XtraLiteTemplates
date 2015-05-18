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

namespace XtraLiteTemplates.Dialects.Standard.Operators
{
    using System;
    using System.Collections.Generic;

    public abstract class StandardRelationalOperator : StandardBinaryOperator
    {
        public IComparer<String> StringComparer { get; private set; }

        public StandardRelationalOperator(String symbol, Int32 precedence, 
            IComparer<String> stringComparer, IPrimitiveTypeConverter typeConverter)
            : base(symbol, precedence, typeConverter)
        {
            Expect.NotNull("stringComparer", stringComparer);
            StringComparer = stringComparer;
        }

        public sealed override Object Evaluate(Object left, Object right)
        {
            Int32 relation;
            if (TypeConverter.TypeOf(left) == PrimitiveType.String || TypeConverter.TypeOf(right) == PrimitiveType.String)
                relation = StringComparer.Compare(TypeConverter.ConvertToString(left), TypeConverter.ConvertToString(right));
            else
                relation = TypeConverter.ConvertToNumber(left).CompareTo(TypeConverter.ConvertToNumber(right));

            return Evaluate(relation, left, right);
        }

        public abstract Boolean Evaluate(Int32 relation, Object left, Object right);
    }
}
