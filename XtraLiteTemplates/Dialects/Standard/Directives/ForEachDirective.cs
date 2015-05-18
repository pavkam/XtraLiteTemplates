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
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using XtraLiteTemplates.Parsing;
    using System.Collections;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Evaluation;

    public sealed class ForEachDirective : StandardDirective
    {
        private Int32 m_expressionIndex;
        private Int32 m_identifierIndex;

        public ForEachDirective(String startTagMarkup, String endTagMarkup, IPrimitiveTypeConverter typeConverter) :
            base(typeConverter, Tag.Parse(startTagMarkup), Tag.Parse(endTagMarkup))
        {
            Debug.Assert(Tags.Count == 2);

            /* Find all expressions. */
            var tag = Tags[0];
            var expressionComponents = Enumerable.Range(0, tag.ComponentCount)
                .Where(index => tag.MatchesExpression(index)).Select(index => index).ToArray();
            var identifierComponents = Enumerable.Range(0, tag.ComponentCount)
                .Where(index => tag.MatchesAnyIdentifier(index)).Select(index => index).ToArray();

            Expect.IsTrue("one expression component", expressionComponents.Length == 1);
            Expect.IsTrue("one identifier component", identifierComponents.Length == 1);

            m_expressionIndex = expressionComponents[0];
            m_identifierIndex = identifierComponents[0];
        }

        public ForEachDirective(IPrimitiveTypeConverter typeConverter) :
            this("FOR EACH ? IN $", "END", typeConverter)
        {
        }

        protected internal override FlowDecision Execute(Int32 tagIndex, Object[] components, ref Object state,
            IVariableContext context, out String text)
        {
            Debug.Assert(tagIndex >= 0 && tagIndex <= 1);
            Debug.Assert(components != null);
            Debug.Assert(components.Length == Tags[tagIndex].ComponentCount);
            Debug.Assert(context != null);

            text = null;
           
            IEnumerator enumerator;
            if (state == null)
            {
                /* Starting up. */
                Debug.Assert(tagIndex == 0);
                

                if (components[m_expressionIndex] == null)
                    return FlowDecision.Terminate;

                var enumerable = components[m_expressionIndex] as IEnumerable;
                if (enumerable == null)
                    enumerable = new Object[] { components[m_expressionIndex] };

                enumerator = enumerable.GetEnumerator();
                state = enumerator;
            }
            else if (tagIndex == 0)
            {
                enumerator = state as IEnumerator;
                Debug.Assert(enumerator != null);
            }
            else
                return FlowDecision.Restart;

            if (!enumerator.MoveNext())
                return FlowDecision.Terminate;
            else
            {
                var variableName = components[m_identifierIndex] as String;
                Debug.Assert(variableName != null);
                context.SetVariable(variableName, enumerator.Current);
                return FlowDecision.Evaluate;
            }
        }
    }
}

