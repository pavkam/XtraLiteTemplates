using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Expressions.Operators.Standard
{
    public sealed class FlexiblePrimitiveTypeConverter : IPrimitiveTypeConverter
    {
        private IFormatProvider m_formatProvider;

        public FlexiblePrimitiveTypeConverter(IFormatProvider formatProvider)
        {
            Expect.NotNull("formatProvider", formatProvider);

            m_formatProvider = formatProvider;
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
                reduced = (Double)(Int64)obj;
            else if (obj is Single)
                reduced = (Double)(Single)obj;
            else if (obj is Decimal)
                reduced = (Double)(Decimal)obj;
            else
                reduced = obj;

            return reduced;
        }

        public Int32 ConvertToInteger(Object obj)
        {
            return (Int32)ConvertToNumber(obj);
        }

        public Double ConvertToNumber(Object obj)
        {
            obj = ReduceObject(obj);

            Double result;

            if (obj == null)
                result = 0;
            else if (obj is Double)
                result = (Double)obj;
            else if (obj is Boolean)
                result = (Boolean)obj ? 1 : 0;
            else if (!(obj is String) || !Double.TryParse((String)obj, System.Globalization.NumberStyles.Float, m_formatProvider, out result))
                result = Double.NaN;

            return result;
        }

        public String ConvertToString(Object obj)
        {
            obj = ReduceObject(obj);

            String result;

            if (obj is String)
                result = (String)obj;
            else if (obj is Double)
                result = ((Double)obj).ToString(m_formatProvider);
            else if (obj is Boolean)
                result = ((Boolean)obj).ToString(m_formatProvider);
            else if (obj == null)
                result = "undefined";
            else
                result = obj.ToString();

            return result;
        }

        public Boolean ConvertToBoolean(Object obj)
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
