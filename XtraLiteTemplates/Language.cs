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

namespace XtraLiteTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Expressions.Operators.Standard;

    public abstract class Language
    {
        public enum CoreSyntaxElement
        {
            StartTagCharacter,
            EndTagCharacter,
            StartStringCharacter,
            EndStringCharacter,
            StringEscapeCharacter,
        }

        public enum StandardRelationalOperator
        {
            RelationalEqualsOperatorSymbol,
            RelationalNotEqualsOperator,
            RelationalGreaterThanOperator,
            RelationalGreaterThanOrEqualsOperator,
            RelationalLowerThanOperator,
            RelationalLowerThanOrEqualsOperator,
        }

        public enum StandardLogicalOperator
        {
            LogicalAndOperator,
            LogicalOrOperator,
            LogicalNotOperator,
        }

        public enum StandardBitwiseOperator
        {
            BitwiseAndOperator,
            BitwiseOrOperator,
            BitwiseXorOperator,
            BitwiseNotOperator,
            BitwiseShiftLeftOperator,
            BitwiseShiftRightOperator,
        }

        public enum StandardArithmeticOperator
        {
            ArithmeticDivideOperator,
            ArithmeticModuloOperator,
            ArithmeticMultiplyOperator,
            ArithmeticNegateOperator,
            ArithmeticNeutralOperator,
            ArithmeticSubtractOperator,
            ArithmeticSumOperator,
        }

        public enum SpecialOperator
        {
            MemberAccessOperator,
            IntegerRangeOperator,
        }


        public abstract IPrimitiveTypeConverter TypeConverter { get; }

        public virtual Char GetCoreSyntaxElementCharacter(CoreSyntaxElement element)
        {
            switch (element)
            {
                case CoreSyntaxElement.EndStringCharacter:
                    return '"';
                case CoreSyntaxElement.EndTagCharacter:
                    return '}';
                case CoreSyntaxElement.StartStringCharacter:
                    return '"';
                case CoreSyntaxElement.StartTagCharacter:
                    return '{';
                case CoreSyntaxElement.StringEscapeCharacter:
                    return '\\';
                default:
                    Debug.Assert(false, "Cannot ever happen!");
                    return '\0';
            }
        }


        public virtual String GetStandardRelationalOperatorSymbol(StandardRelationalOperator @operator)
        {
            switch (@operator)
            {
                case StandardRelationalOperator.RelationalEqualsOperatorSymbol:
                case StandardRelationalOperator.RelationalNotEqualsOperator:
                case StandardRelationalOperator.RelationalGreaterThanOperator:
                case StandardRelationalOperator.RelationalGreaterThanOrEqualsOperator:
                case StandardRelationalOperator.RelationalLowerThanOperator:
                case StandardRelationalOperator.RelationalLowerThanOrEqualsOperator:
                default:
                    Debug.Assert(false, "Cannot ever happen!");
                    return null;
            }
        }

        public virtual String GetStandardLogicalOperatorSymbol(StandardLogicalOperator @operator)
        {
            switch (@operator)
            {
                case StandardLogicalOperator.LogicalAndOperator:
                case StandardLogicalOperator.LogicalOrOperator:
                case StandardLogicalOperator.LogicalNotOperator:
                default:
                    Debug.Assert(false, "Cannot ever happen!");
                    return null;
            }
        }

        public virtual String GetStandardBitwiseOperatorSymbol(StandardBitwiseOperator @operator)
        {
            switch (@operator)
            {
                case StandardBitwiseOperator.BitwiseAndOperator:
                case StandardBitwiseOperator.BitwiseOrOperator:
                case StandardBitwiseOperator.BitwiseXorOperator:
                case StandardBitwiseOperator.BitwiseNotOperator:
                case StandardBitwiseOperator.BitwiseShiftLeftOperator:
                case StandardBitwiseOperator.BitwiseShiftRightOperator:
                default:
                    Debug.Assert(false, "Cannot ever happen!");
                    return null;
            }
        }

        public virtual String GetStandardArithmeticOperatorSymbol(StandardArithmeticOperator @operator)
        {
            switch (@operator)
            {
                case StandardArithmeticOperator.ArithmeticDivideOperator:
                case StandardArithmeticOperator.ArithmeticModuloOperator:
                case StandardArithmeticOperator.ArithmeticMultiplyOperator:
                case StandardArithmeticOperator.ArithmeticNegateOperator:
                case StandardArithmeticOperator.ArithmeticNeutralOperator:
                case StandardArithmeticOperator.ArithmeticSubtractOperator:
                case StandardArithmeticOperator.ArithmeticSumOperator:
                default:
                    Debug.Assert(false, "Cannot ever happen!");
                    return null;
            }
        }

        public virtual String GetSpecialOperatorSymbol(SpecialOperator @operator)
        {
            switch (@operator)
            {
                case SpecialOperator.MemberAccessOperator:
                case SpecialOperator.IntegerRangeOperator:
                default:
                    Debug.Assert(false, "Cannot ever happen!");
                    return null;
            }
        }

        public virtual String GetSpecialOperatorSymbol(SpecialOperator @operator)
        {
            switch (@operator)
            {
                case SpecialOperator.MemberAccessOperator:
                case SpecialOperator.IntegerRangeOperator:
                default:
                    Debug.Assert(false, "Cannot ever happen!");
                    return null;
            }
        }

        public virtual Boolean GetSubscriptOperatorSymbol(out String symbol, out String terminator)
        {
        }


    }
}
