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
using NUnit.Framework;

namespace XtraLiteTemplates.NUnit.Inside
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using XtraLiteTemplates.Expressions;

    public sealed class TestExpressionEvaluationContext : IEvaluationContext
    {
        private Dictionary<String, Object> m_variables;

        public Object HandleEvaluationError(Expressions.Operators.Operator @operator, Object operand)
        {
            Assert.NotNull(@operator);
            return null;
        }

        public Object HandleEvaluationError(Expressions.Operators.Operator @operator, Object leftOperand, Object rightOperand)
        {
            Assert.NotNull(@operator);
            return null;
        }

        public Object GetVariable(String identifier)
        {
            Assert.IsNotEmpty(identifier);

            Object result;
            if (m_variables.TryGetValue(identifier, out result))
                return result;
            else
                return null;
        }

        public TestExpressionEvaluationContext(IEqualityComparer<String> comparer, 
            IReadOnlyCollection<KeyValuePair<String, Object>> variables)
        {
            Debug.Assert(comparer != null);
            Debug.Assert(variables != null);

            m_variables = variables.ToDictionary(k => k.Key, v => v.Value);
        }
    }
}
