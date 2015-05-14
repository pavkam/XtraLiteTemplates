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

namespace XtraLiteTemplates.Evaluation
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using XtraLiteTemplates.Expressions.Operators.Standard;

    public class StandardEvaluationContext : IEvaluationContext, IPrimitiveTypeConverter
    {
        private Stack<Dictionary<String, Object>> m_frames;

        public IFormatProvider PrimitiveFormatProvider { get; private set; }
        public IEqualityComparer<String> IdentifierComparer { get; private set; }
        public Boolean IgnoreEvaluationExceptions { get; private set;  }

        public StandardEvaluationContext(Boolean ignoreEvaluationExceptions, 
            IEqualityComparer<String> identifierComparer, IFormatProvider primitiveFormatPrivider)
        {
            Expect.NotNull("identifierComparer", identifierComparer);
            Expect.NotNull("primitiveFormatPrivider", primitiveFormatPrivider);

            IdentifierComparer = identifierComparer;
            IgnoreEvaluationExceptions = ignoreEvaluationExceptions;
            PrimitiveFormatProvider = primitiveFormatPrivider;

            m_frames = new Stack<Dictionary<String, Object>>();
        }
        
        public virtual String ProcessUnparsedText(String value)
        {
            return value;
        }


        public void OpenEvaluationFrame()
        {
            m_frames.Push(new Dictionary<String, Object>(IdentifierComparer));
        }

        public void CloseEvaluationFrame()
        {
            if (m_frames.Count == 0)
                ExceptionHelper.DepletedVariableFrames();

            m_frames.Pop();
        }

        public void SetVariable(String identifier, Object value)
        {
            if (m_frames.Count == 0)
                ExceptionHelper.DepletedVariableFrames();

            var topFrame = m_frames.Peek();
            topFrame[identifier] = value;
        }

        public Object GetVariable(String identifier)
        {
            if (m_frames.Count == 0)
                ExceptionHelper.DepletedVariableFrames();

            var topFrame = m_frames.Peek();

            Object result;
            if (!topFrame.TryGetValue(identifier, out result))
            {
                result = null;
                if (!IgnoreEvaluationExceptions)
                    ExceptionHelper.UndefinedVariable(identifier);
            }

            return result;
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

        public virtual Int32 ConvertToInteger(Object obj)
        {
            return (Int32)ConvertToNumber(obj);
        }

        public virtual Double ConvertToNumber(Object obj)
        {
            obj = ReduceObject(obj);

            Double result;

            if (obj == null)
                result = 0;
            else if (obj is Double)
                result = (Double)obj;
            else if (obj is Boolean)
                result = (Boolean)obj ? 1 : 0;
            else if (!(obj is String) || !Double.TryParse((String)obj, System.Globalization.NumberStyles.Float, PrimitiveFormatProvider, out result))
                result = Double.NaN;

            return result;
        }

        public virtual String ConvertToString(Object obj)
        {
            obj = ReduceObject(obj);

            String result;

            if (obj is String)
                result = (String)obj;
            else if (obj is Double)
                result = ((Double)obj).ToString(PrimitiveFormatProvider);
            else if (obj is Boolean)
                result = ((Boolean)obj).ToString(PrimitiveFormatProvider);
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
