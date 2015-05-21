﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Dialects.Standard
{
    public interface IPrimitiveTypeConverter
    {
        PrimitiveType TypeOf(Object obj);

        Int32 ConvertToInteger(Object obj);
        Double ConvertToNumber(Object obj);
        String ConvertToString(Object obj);
        Boolean ConvertToBoolean(Object obj);
        IEnumerable<Object> ConvertToSequence(Object obj);
    }
}
