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

namespace XtraLiteTemplates.Dialects.Standard
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.IO;
    using XtraLiteTemplates.Dialects.Standard.Operators;

    public class FlexiblePrimitiveTypeConverter : IPrimitiveTypeConverter
    {
        public IFormatProvider FormatProvider { get; private set; }

        public FlexiblePrimitiveTypeConverter(IFormatProvider formatProvider)
        {
            Expect.NotNull("formatProvider", formatProvider);

            FormatProvider = formatProvider;
        }

        private IEnumerable<Object> UpgradeEnumerable(IEnumerable enumerable)
        {
            Debug.Assert(enumerable != null);
            foreach (var o in enumerable)
            {
                yield return o;
            }
        }

        private IEnumerable<Object> ObjectToSequence(Object obj)
        {
            yield return obj;
        }

        private Object ReduceObject(Object obj)
        {
            Object reduced;

            /* Identify standard types */
            if (obj is Double || obj is Boolean || obj is String)
                reduced = obj;
            else if (obj is Byte)
                reduced = (Double)(Byte)obj;
            else if (obj is SByte)
                reduced = (Double)(SByte)obj;
            else if (obj is Int16)
                reduced = (Double)(Int16)obj;
            else if (obj is UInt16)
                reduced = (Double)(UInt16)obj;
            else if (obj is Int32)
                reduced = (Double)(Int32)obj;
            else if (obj is UInt32)
                reduced = (Double)(UInt32)obj;
            else if (obj is Int64)
                reduced = (Double)(Int64)obj;
            else if (obj is UInt64)
                reduced = (Double)(UInt64)obj;
            else if (obj is Single)
                reduced = (Double)(Single)obj;
            else if (obj is Decimal)
                reduced = (Double)(Decimal)obj;
            else
                reduced = obj;

            return reduced;
        }

        public virtual Int32 ConvertToInteger(Object obj)
        {
            var number = ConvertToNumber(obj);
            if (Double.IsNaN(number) || Double.IsInfinity(number))
                return 0;
            else
                return (Int32)number;
        }

        public virtual Double ConvertToNumber(Object obj)
        {
            obj = ReduceObject(obj);

            Double result;

            if (obj == null)
                result = Double.NaN;
            else if (obj is Double)
                result = (Double)obj;
            else if (obj is Boolean)
                result = (Boolean)obj ? 1 : 0;
            else
            {
                var str = obj as String;
                if (str != null)
                {
                    if (str.Length == 0)
                        result = 0;
                    else if (!Double.TryParse(str, System.Globalization.NumberStyles.Number, FormatProvider, out result))
                        result = Double.NaN;
                }
                else
                    result = Double.NaN;
            }

            return result;
        }

        public virtual String ConvertToString(Object obj)
        {
            obj = ReduceObject(obj);

            String result;

            if (obj is String)
                result = (String)obj;
            else if (obj is Double)
                result = ((Double)obj).ToString(FormatProvider);
            else if (obj is Boolean)
                result = ((Boolean)obj).ToString(FormatProvider);
            else if (obj == null)
                result = "undefined";
            else
                result = obj.ToString();

            return result;
        }

        public virtual Boolean ConvertToBoolean(Object obj)
        {
            obj = ReduceObject(obj);

            Boolean result;
            if (obj == null)
                result = false;
            else if (obj is Boolean)
                result = (Boolean)obj;
            else if (obj is Double)
                result = (Double)obj != 0;
            else if (obj is String)
                result = ((String)obj).Length > 0;
            else
                result = true;

            return result;
        }

        public virtual IEnumerable<Object> ConvertToSequence(Object obj)
        {
            if (obj == null)
                return null;
            else if (obj is IEnumerable<Object>)
                return (IEnumerable<Object>)obj;
            else if (obj is IEnumerable)
                return UpgradeEnumerable((IEnumerable)obj);
            else
                return ObjectToSequence(obj);
        }

        public PrimitiveType TypeOf(Object obj)
        {
            if (obj == null)
                return PrimitiveType.Undefined;

            obj = ReduceObject(obj);

            if (obj is Double)
                return PrimitiveType.Number;
            else if (obj is Boolean)
                return PrimitiveType.Boolean;
            else if (obj is String)
                return PrimitiveType.String;
            else
                return PrimitiveType.Object;
        }
    }
}
