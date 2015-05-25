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
    using XtraLiteTemplates.Expressions;

    public sealed class SeparatedForEachDirective : StandardDirective
    {
        private class State
        {
            public IEnumerator<object> Enumerator;
            public bool IsLast;
        }

        private int m_expressionIndex;
        private int m_identifierIndex;

        public SeparatedForEachDirective(string startTagMarkup, string separatorTagMarkup, string endTagMarkup, IPrimitiveTypeConverter typeConverter) :
            base(typeConverter, Tag.Parse(startTagMarkup), Tag.Parse(separatorTagMarkup), Tag.Parse(endTagMarkup))
        {
            Debug.Assert(Tags.Count == 3);

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

        public SeparatedForEachDirective(IPrimitiveTypeConverter typeConverter) :
            this("FOR EACH ? IN $", "WITH", "END", typeConverter)
        {
        }

        protected internal override FlowDecision Execute(int tagIndex, object[] components, ref object state,
            IExpressionEvaluationContext context, out string text)
        {
            Debug.Assert(tagIndex >= 0 && tagIndex <= 2);
            Debug.Assert(components != null);
            Debug.Assert(components.Length == Tags[tagIndex].ComponentCount);
            Debug.Assert(context != null);

            text = null;
            if (tagIndex == 0)
            {
                if (state == null)
                {
                    var sequence = TypeConverter.ConvertToSequence(components[m_expressionIndex]);
                    if (sequence == null)
                    {
                        return FlowDecision.Terminate;
                    }

                    var enumerator = sequence.GetEnumerator();
                    if (!enumerator.MoveNext())
                    {
                        return FlowDecision.Terminate;
                    }

                    context.SetVariable(components[m_identifierIndex] as string, enumerator.Current);
                    state = new State
                    {
                        Enumerator = enumerator,
                        IsLast = !enumerator.MoveNext(),
                    };

                    return FlowDecision.Evaluate;
                }
                else
                {
                    var sstate = state as State;

                    Debug.Assert(sstate != null);
                    Debug.Assert(sstate.Enumerator != null);
                    Debug.Assert(!sstate.IsLast);

                    context.SetVariable(components[m_identifierIndex] as string, sstate.Enumerator.Current);
                    sstate.IsLast = !sstate.Enumerator.MoveNext();

                    return FlowDecision.Evaluate;
                }
            }
            else if (tagIndex == 1)
            {
                var sstate = state as State;

                Debug.Assert(sstate != null);
                Debug.Assert(sstate.Enumerator != null);

                return sstate.IsLast ? FlowDecision.Terminate : FlowDecision.Evaluate;
            }
            else
                return FlowDecision.Restart;
        }
    }
}