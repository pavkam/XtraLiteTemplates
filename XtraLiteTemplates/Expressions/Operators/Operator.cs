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
    using System.Linq;
    using System.Collections.Generic;
    using XtraLiteTemplates.Expressions.Operators.Standard;

    public abstract class Operator
    {
        public String Symbol { get; private set; }

        public Int32 Precedence { get; private set; }

        public Boolean ExpectRhsIdentifier { get; private set; }

        protected Operator(String symbol, Int32 precedence, Boolean expectRhsIdentifier)
        {
            Expect.NotEmpty("symbol", symbol);
            Expect.GreaterThanOrEqual("precedence", precedence, 0);

            ExpectRhsIdentifier = expectRhsIdentifier;
            Symbol = symbol;
            Precedence = precedence;
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


        public static IReadOnlyCollection<Operator> CreateStandardRelationalOperators(
            IComparer<String> stringComparer, IPrimitiveTypeConverter typeConverter)
        {
            var result = new List<Operator>()
            {
                new RelationalEqualsOperator(stringComparer, typeConverter),
                new RelationalNotEqualsOperator(stringComparer, typeConverter),
                new RelationalGreaterThanOperator(stringComparer, typeConverter),
                new RelationalGreaterThanOrEqualsOperator(stringComparer, typeConverter),
                new RelationalLowerThanOperator(stringComparer, typeConverter),
                new RelationalLowerThanOrEqualsOperator(stringComparer, typeConverter),
            };

            return result;
        }

        public static IReadOnlyCollection<Operator> CreateStandardLogicalOperators(IPrimitiveTypeConverter typeConverter)
        {
            var result = new List<Operator>()
            {
                new LogicalAndOperator(typeConverter),
                new LogicalOrOperator(typeConverter),
                new LogicalNotOperator(typeConverter),
            };

            return result;
        }

        public static IReadOnlyCollection<Operator> CreateStandardBitwiseOperators(IPrimitiveTypeConverter typeConverter)
        {
            var result = new List<Operator>()
            {
                new BitwiseAndOperator(typeConverter),
                new BitwiseNotOperator(typeConverter),
                new BitwiseOrOperator(typeConverter),
                new BitwiseXorOperator(typeConverter),
                new BitwiseShiftLeftOperator(typeConverter),
                new BitwiseShiftRightOperator(typeConverter),
            };

            return result;
        }

        public static IReadOnlyCollection<Operator> CreateStandardArithmeticOperators(IPrimitiveTypeConverter typeConverter)
        {
            var result = new List<Operator>()
            {
                new ArithmeticDivideOperator(typeConverter),
                new ArithmeticModuloOperator(typeConverter),
                new ArithmeticMultiplyOperator(typeConverter),
                new ArithmeticNegateOperator(typeConverter),
                new ArithmeticNeutralOperator(typeConverter),
                new ArithmeticSubtractOperator(typeConverter),
                new ArithmeticSumOperator(typeConverter),
            };

            return result;
        }


        public static IReadOnlyCollection<Operator> CreateStandardOperators(
            IComparer<String> stringComparer, IEqualityComparer<String> identifierComparer, IPrimitiveTypeConverter typeConverter)
        {
            var allOperators =
                CreateStandardRelationalOperators(stringComparer, typeConverter)
                .Concat(CreateStandardLogicalOperators(typeConverter))
                .Concat(CreateStandardBitwiseOperators(typeConverter))
                .Concat(CreateStandardArithmeticOperators(typeConverter)).ToList();

            allOperators.Add(new SubscriptOperator());
            allOperators.Add(new MemberAccessOperator(identifierComparer));

            return allOperators;
        }
    }
}

