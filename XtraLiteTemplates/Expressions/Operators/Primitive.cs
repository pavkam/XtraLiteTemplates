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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;

    public struct Primitive
    {
        public Object Value { get; private set; }

        public Primitive(Object value) : this()
        {
            Value = null;

            /* Identify standard types */
            if (value is Byte)
                Value = (Double)(Byte)value;
            else if (value is SByte)
                Value = (Double)(SByte)value;
            else if (value is Int16)
                Value = (Double)(Int16)value;
            else if (value is UInt16)
                Value = (Double)(UInt16)value;
            else if (value is Int32)
                Value = (Double)(Int32)value;
            else if (value is UInt32)
                Value = (Double)(UInt32)value;
            else if (value is Int64)
                Value = (Double)(Int64)value;
            else if (value is UInt64)
                Value = (Double)(Int64)value;
            else if (value is Single)
                Value = (Double)(Single)value;
            else if (value is Double)
                Value = (Double)(Double)value;
            else if (value is Decimal)
                Value = (Double)(Decimal)value;
            else if (value is Boolean)
                Value = (Boolean)value;
            else if (value is String)
                Value = (String)value;
        }

        public static implicit operator Primitive(Double value)
        {
            return new Primitive
            {
                Value = (Double)value,
            };
        }

        public static explicit operator Primitive(Decimal value)
        {
            return (Double)value;
        }

        public static implicit operator Primitive(Boolean value)
        {
            return new Primitive
            {
                Value = (Boolean)value,
            };
        }

        public static implicit operator Primitive(String value)
        {
            if (value == null)
                return new Primitive(null);
            else
            {
                return new Primitive
                {
                    Value = (String)value,
                };
            }
        }



        public Double AsNumber()
        {
            Double result;
            if (Value == null)
                result = 0;
            else if (Value is Double)
                result = (Double)Value;
            else if (Value is Boolean)
                result = (Boolean)Value ? 1 : 0;
            else if (!(Value is String) || !Double.TryParse((String)Value, out result))
                result = Double.NaN;

            return result;
        }

        public String AsString()
        {
            String result;
            if (Value is String)
                result = (String)Value;
            else if (Value is Double)
                result = ((Double)Value).ToString(CultureInfo.InvariantCulture);
            else if (Value is Boolean)
                result = ((Boolean)Value).ToString(CultureInfo.InvariantCulture);
            else if (Value == null)
                result = "undefined";
            else
                result = Value.ToString();

            return result;
        }

        public Boolean AsBoolean()
        {
            Boolean result;
            if (Value == null)
                result = false;
            else if (Value is Boolean)
                result = (Boolean)Value;
            else if (Value is Double)
                result = (Double)Value != 0;
            else if (Value is String)
                result = ((String)Value).Length > 0;
            else
                result = true;

            return result;
        }


        public static Primitive operator +(Primitive right)
        {
            return right.AsNumber();
        }

        public static Primitive operator -(Primitive right)
        {
            return - right.AsNumber();
        }


        public static Primitive operator +(Primitive left, Primitive right)
        {
            if (left.Value is String || right.Value is String)
                return left.AsString() + right.AsString();
            else if (left.Value is Double || right.Value is Double)
                return left.AsNumber() + right.AsNumber();
            else if (left.Value is Boolean || right.Value is Boolean)
                return left.AsNumber() + right.AsNumber();
            else
                return null;
        }

        public static Primitive operator -(Primitive left, Primitive right)
        {
            return left.AsNumber() - right.AsNumber();
        }

        public static Primitive operator *(Primitive left, Primitive right)
        {
            return left.AsNumber() * right.AsNumber();
        }

        public static Primitive operator /(Primitive left, Primitive right)
        {
            return left.AsNumber() / right.AsNumber();
        }

        public static Primitive operator %(Primitive left, Primitive right)
        {
            return (Int64)left.AsNumber() % (Int64)right.AsNumber();
        }


        public static Primitive operator >>(Primitive left, Int32 right)
        {
            return (Int32)left.AsNumber() >> right;
        }

        public static Primitive operator <<(Primitive left, Int32 right)
        {
            return (Int32)left.AsNumber() << right;
        }

        public static Primitive operator ~(Primitive right)
        {
            return ~(Int32)right.AsNumber();
        }

        public static Primitive operator ^(Primitive left, Primitive right)
        {
            return (Int32)left.AsNumber() ^ (Int32)right.AsNumber();
        }

        public static Primitive operator |(Primitive left, Primitive right)
        {
            return (Int32)left.AsNumber() | (Int32)right.AsNumber();
        }

        public static Primitive operator &(Primitive left, Primitive right)
        {
            return (Int32)left.AsNumber() & (Int32)right.AsNumber();
        }


        public static Primitive operator !(Primitive right)
        {
            return !right.AsBoolean();
        }

        public static Primitive operator >(Primitive left, Primitive right)
        {
            if (left.Value is String || right.Value is String)
                return left.AsString().CompareTo(right.AsString()) > 0;
            else
                return left.AsNumber() > right.AsNumber();
        }

        public static Primitive operator >=(Primitive left, Primitive right)
        {
            if (left.Value is String || right.Value is String)
                return left.AsString().CompareTo(right.AsString()) >= 0;
            else
                return left.AsNumber() >= right.AsNumber();
        }

        public static Primitive operator <(Primitive left, Primitive right)
        {
            if (left.Value is String || right.Value is String)
                return left.AsString().CompareTo(right.AsString()) < 0;
            else
                return left.AsNumber() < right.AsNumber();
        }

        public static Primitive operator <=(Primitive left, Primitive right)
        {
            if (left.Value is String || right.Value is String)
                return left.AsString().CompareTo(right.AsString()) <= 0;
            else
                return left.AsNumber() <= right.AsNumber();
        }

        public static Primitive operator ==(Primitive left, Primitive right)
        {
            if (left.Value is String || right.Value is String)
                return left.AsString().CompareTo(right.AsString()) == 0;
            else
                return left.AsNumber() == right.AsNumber();
        }

        public static Primitive operator !=(Primitive left, Primitive right)
        {
            if (left.Value is String || right.Value is String)
                return left.AsString().CompareTo(right.AsString()) != 0;
            else
                return left.AsNumber() != right.AsNumber();
        }


        public override Boolean Equals(Object obj)
        {
            if (obj is Primitive)
            {
                var primitive = (Primitive)obj;
                return (this == primitive).AsBoolean();
            }
            else
                return false;
        }

        public override Int32 GetHashCode()
        {
            if (Value == null)
                return 0;
            else 
                return Value.GetHashCode();
        }
    }
}

