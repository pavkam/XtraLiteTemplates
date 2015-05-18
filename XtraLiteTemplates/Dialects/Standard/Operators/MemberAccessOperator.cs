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
//     * Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
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
    using System.Reflection;
    using System.Collections.Generic;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions.Operators;

    public sealed class MemberAccessOperator : BinaryOperator
    {
        public IEqualityComparer<String> Comparer { get; private set; }

        public MemberAccessOperator(String symbol, IEqualityComparer<String> comparer)
            : base(symbol, 0, Associativity.LeftToRight, false, true)
        {
            Expect.NotNull("comparer", comparer);
            Comparer = comparer;
        }

        public MemberAccessOperator(IEqualityComparer<String> comparer)
            : this(".", comparer)
        {
        }

        public override Object Evaluate(Object left, Object right)
        {
            var memberName = right as String;
            Debug.Assert(memberName != null);

            if (left != null)
            {
                foreach (var property in left.GetType().GetProperties())
                {
                    if (!property.CanRead || property.GetIndexParameters().Length > 0 || !Comparer.Equals(property.Name, memberName))
                        continue;
                    
                    return property.GetValue(left);
                }
            }

            return null;
        }
    }
}

