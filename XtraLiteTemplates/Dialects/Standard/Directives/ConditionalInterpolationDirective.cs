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

namespace XtraLiteTemplates.Dialects.Standard.Directives
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using XtraLiteTemplates.Parsing;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;

    public sealed class ConditionalInterpolationDirective : StandardDirective
    {
        private int m_interpolatedExpressionIndex;
        private int m_conditionalExpressionIndex;

        public ConditionalInterpolationDirective(String markup, bool invertExpressionOrder, IPrimitiveTypeConverter typeConverter)
            : base(typeConverter, Tag.Parse(markup))
        {
            Debug.Assert(Tags.Count == 1);

            /* Find all expressions. */
            var tag = Tags[0];
            var expressionComponents = Enumerable.Range(0, tag.ComponentCount)
                .Where(index => tag.MatchesExpression(index)).Select(index => index).ToArray();

            Expect.IsTrue("two expression components", expressionComponents.Length == 2);

            if (invertExpressionOrder)
            {
                m_interpolatedExpressionIndex = expressionComponents[1];
                m_conditionalExpressionIndex = expressionComponents[0];
            }
            else
            {
                m_interpolatedExpressionIndex = expressionComponents[0];
                m_conditionalExpressionIndex = expressionComponents[1];
            }
        }

        public ConditionalInterpolationDirective(IPrimitiveTypeConverter typeConverter)
            : this("$ IF $", false, typeConverter)
        {
        }

        protected internal override FlowDecision Execute(
            int tagIndex, 
            Object[] components,
            ref Object state, 
            IExpressionEvaluationContext context, 
            out String text)
        {
            /* It is a simple directive. Expecting just one tag here. */
            Debug.Assert(tagIndex == 0);
            Debug.Assert(components != null);
            Debug.Assert(components.Length == Tags[tagIndex].ComponentCount);
            Debug.Assert(context != null);

            if (TypeConverter.ConvertToBoolean(components[m_conditionalExpressionIndex]))
                text = TypeConverter.ConvertToString(components[m_interpolatedExpressionIndex]);
            else
                text = null;

            return FlowDecision.Terminate;
        }
    }
}

