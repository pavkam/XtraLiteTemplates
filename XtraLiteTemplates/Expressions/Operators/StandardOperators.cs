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
    public static class StandardOperators
    {
        public static UnaryOperator Plus { get; private set; }

        public static UnaryOperator Negate { get; private set; }

        public static BinaryOperator Add { get; private set; }

        public static BinaryOperator Subtract { get; private set; }

        public static BinaryOperator Multiply { get; private set; }

        public static BinaryOperator Divide { get; private set; }


        public static UnaryOperator Not { get; private set; }

        public static BinaryOperator Or { get; private set; }

        public static BinaryOperator And { get; private set; }

        public static BinaryOperator Xor { get; private set; }


        public static BinaryOperator Equal { get; private set; }

        public static BinaryOperator NotEqual { get; private set; }

        public static BinaryOperator GreaterThan { get; private set; }

        public static BinaryOperator GreaterThanOrEqual { get; private set; }

        public static BinaryOperator LowerThan { get; private set; }

        public static BinaryOperator LowerThanOrEqual { get; private set; }

        public static GroupOperator Paranthesis { get; private set; }

        static StandardOperators()
        {
            Paranthesis = new GroupOperator("(", ")");

            Plus = new UnaryOperator("+", UnaryPlusImpl);
            Negate = new UnaryOperator("-", UnaryNegateImpl);
            Add = new BinaryOperator("+", BinaryAddImpl, 4);
            Subtract = new BinaryOperator("-", BinarySubtractImpl, 4);
            Multiply = new BinaryOperator("*", BinaryMultiplyImpl, 3);
            Divide = new BinaryOperator("/", BinaryDivideImpl, 3);

            Not = new UnaryOperator("!", UnaryNotImpl);
            Or = new BinaryOperator("|", BinaryOrImpl, 12);
            And = new BinaryOperator("&", BinaryAndImpl, 11);
            Xor = new BinaryOperator("^", BinaryXorImpl, 12);

            Equal = new BinaryOperator("=", BinaryEqualImpl, 7);
            NotEqual = new BinaryOperator("!=", BinaryNotEqualImpl, 7);
            GreaterThan = new BinaryOperator(">", BinaryGreaterThanImpl, 6);
            GreaterThanOrEqual = new BinaryOperator(">=", BinaryGreaterThanOrEqualImpl, 6);
            LowerThan = new BinaryOperator("<", BinaryLowerThanImpl, 6);
            LowerThanOrEqual = new BinaryOperator("<=", BinaryLowerThanOrEqualImpl, 6);
        }

        private static Operand UnaryPlusImpl(Operand operand)
        {
            if (operand.CanBeNumber)
                return new Operand(operand.AsNumber);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryAddImpl(Operand left, Operand right)
        {
            if (left.IsNumber && right.CanBeNumber)
                return new Operand(left.AsNumber + right.AsNumber);
            else if (left.IsBoolean && right.CanBeBoolean)
                return new Operand(left.AsBoolean | right.AsBoolean);
            else if (left.IsString && right.CanBeString)
                return new Operand(left.AsString + right.AsString);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryOrImpl(Operand left, Operand right)
        {
            if (left.CanBeBoolean && right.CanBeBoolean)
                return new Operand(left.AsBoolean || right.AsBoolean);
            else
                return Operand.Undefined;
        }

        private static Operand UnaryNegateImpl(Operand operand)
        {
            if (operand.CanBeNumber)
                return new Operand(-operand.AsNumber);
            else
                return Operand.Undefined;
        }

        private static Operand UnaryNotImpl(Operand operand)
        {
            if (operand.CanBeBoolean)
                return new Operand(!operand.AsBoolean);
            else
                return Operand.Undefined;
        }

        private static Operand BinarySubtractImpl(Operand left, Operand right)
        {
            if (left.IsNumber && right.CanBeNumber)
                return new Operand(left.AsNumber - right.AsNumber);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryMultiplyImpl(Operand left, Operand right)
        {
            if (left.IsNumber && right.CanBeNumber)
                return new Operand(left.AsNumber * right.AsNumber);
            else if (left.IsBoolean && right.CanBeBoolean)
                return new Operand(left.AsBoolean & right.AsBoolean);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryAndImpl(Operand left, Operand right)
        {
            if (left.CanBeBoolean && right.CanBeBoolean)
                return new Operand(left.AsBoolean && right.AsBoolean);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryDivideImpl(Operand left, Operand right)
        {
            if (left.IsNumber && right.CanBeNumber)
                return new Operand(left.AsNumber / right.AsNumber);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryXorImpl(Operand left, Operand right)
        {
            if (left.CanBeBoolean && right.CanBeBoolean)
                return new Operand(left.AsBoolean ^ right.AsBoolean);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryEqualImpl(Operand left, Operand right)
        {
            if (left.IsNumber && right.CanBeNumber)
                return new Operand(left.AsNumber.CompareTo(right.AsNumber) == 0);
            else if (left.IsBoolean && right.CanBeBoolean)
                return new Operand(left.AsBoolean.CompareTo(right.AsBoolean) == 0);
            else if (left.IsString && right.CanBeString)
                return new Operand(String.Compare(left.AsString, right.AsString, StringComparison.Ordinal) == 0);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryNotEqualImpl(Operand left, Operand right)
        {
            if (left.IsNumber && right.CanBeNumber)
                return new Operand(left.AsNumber.CompareTo(right.AsNumber) != 0);
            else if (left.IsBoolean && right.CanBeBoolean)
                return new Operand(left.AsBoolean.CompareTo(right.AsBoolean) != 0);
            else if (left.IsString && right.CanBeString)
                return new Operand(String.Compare(left.AsString, right.AsString, StringComparison.Ordinal) != 0);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryLowerThanImpl(Operand left, Operand right)
        {
            if (left.IsNumber && right.CanBeNumber)
                return new Operand(left.AsNumber.CompareTo(right.AsNumber) < 0);
            else if (left.IsBoolean && right.CanBeBoolean)
                return new Operand(left.AsBoolean.CompareTo(right.AsBoolean) < 0);
            else if (left.IsString && right.CanBeString)
                return new Operand(String.Compare(left.AsString, right.AsString, StringComparison.Ordinal) < 0);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryLowerThanOrEqualImpl(Operand left, Operand right)
        {
            if (left.IsNumber && right.CanBeNumber)
                return new Operand(left.AsNumber.CompareTo(right.AsNumber) <= 0);
            else if (left.IsBoolean && right.CanBeBoolean)
                return new Operand(left.AsBoolean.CompareTo(right.AsBoolean) <= 0);
            else if (left.IsString && right.CanBeString)
                return new Operand(String.Compare(left.AsString, right.AsString, StringComparison.Ordinal) <= 0);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryGreaterThanImpl(Operand left, Operand right)
        {
            if (left.IsNumber && right.CanBeNumber)
                return new Operand(left.AsNumber.CompareTo(right.AsNumber) > 0);
            else if (left.IsBoolean && right.CanBeBoolean)
                return new Operand(left.AsBoolean.CompareTo(right.AsBoolean) > 0);
            else if (left.IsString && right.CanBeString)
                return new Operand(String.Compare(left.AsString, right.AsString, StringComparison.Ordinal) > 0);
            else
                return Operand.Undefined;
        }

        private static Operand BinaryGreaterThanOrEqualImpl(Operand left, Operand right)
        {
            if (left.IsNumber && right.CanBeNumber)
                return new Operand(left.AsNumber.CompareTo(right.AsNumber) >= 0);
            else if (left.IsBoolean && right.CanBeBoolean)
                return new Operand(left.AsBoolean.CompareTo(right.AsBoolean) >= 0);
            else if (left.IsString && right.CanBeString)
                return new Operand(String.Compare(left.AsString, right.AsString, StringComparison.Ordinal) >= 0);
            else
                return Operand.Undefined;
        }
    }
}

