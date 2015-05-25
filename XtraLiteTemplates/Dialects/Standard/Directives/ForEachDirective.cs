﻿//  Author:
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

    /// <summary>
    /// The FOR EACH directive implementation.
    /// </summary>
    public sealed class ForEachDirective : StandardDirective
    {
        private int m_expressionIndex;
        private int m_identifierIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="ForEachDirective"/> class.
        /// </summary>
        /// <remarks>
        /// The <paramref name="startTagMarkup"/> is expected to contain exactly one expression components and one "any identifier" component.
        /// The identifier is used to represent each element in the evaluated sequence.
        /// </remarks>
        /// <param name="startTagMarkup">The start tag markup.</param>
        /// <param name="endTagMarkup">The end tag markup.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="InvalidOperationException">Argument <paramref name="startTagMarkup"/> or <paramref name="endTagMarkup"/>  does not correspond to the expressed rules.</exception>
        /// <exception cref="System.FormatException">Argument <paramref name="startTagMarkup"/> or <paramref name="endTagMarkup"/> cannot be parsed.</exception>
        public ForEachDirective(string startTagMarkup, string endTagMarkup, IPrimitiveTypeConverter typeConverter) :
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

        /// <summary>
        /// Initializes a new instance of the <see cref="ForEachDirective"/> class using the standard markup {FOR EACH ? IN $}...{END}.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        public ForEachDirective(IPrimitiveTypeConverter typeConverter) :
            this("FOR EACH ? IN $", "END", typeConverter)
        {
        }

        /// <summary>
        /// Executes the current directive. 
        /// <remarks>
        /// If the expression is evaluated to <c>null</c>, the directive does not evaluate. It evaluates once for non-sequences and once for each element if the 
        /// expression is a sequence. On each cycle the indentifer is set to the value of teh enumerated object.
        /// </remarks>
        /// </summary>
        /// <param name="tagIndex">The index of the tag that triggered the execution.</param>
        /// <param name="components">The tag components as provided by the lexical analyzer.</param>
        /// <param name="state">A general-purpose state object. Initially set to <c>null</c>.</param>
        /// <param name="context">The evaluation context.</param>
        /// <param name="text">An optional text generated by the directive.</param>
        /// <returns>
        /// A value indicating the next step for the evaluation environment.
        /// </returns>
        protected internal override FlowDecision Execute(
            int tagIndex,
            object[] components,
            ref object state,
            IExpressionEvaluationContext context,
            out string text)
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

                var sequence = TypeConverter.ConvertToSequence(components[m_expressionIndex]);
                if (sequence == null)
                {
                    return FlowDecision.Terminate;
                }

                enumerator = sequence.GetEnumerator();
                state = enumerator;
            }
            else if (tagIndex == 0)
            {
                enumerator = state as IEnumerator<object>;
                Debug.Assert(enumerator != null);
            }
            else
            {
                return FlowDecision.Restart;
            }

            if (!enumerator.MoveNext())
            {
                return FlowDecision.Terminate;
            }
            else
            {
                var variableName = components[m_identifierIndex] as string;
                Debug.Assert(variableName != null);
                context.SetVariable(variableName, enumerator.Current);
                return FlowDecision.Evaluate;
            }
        }
    }
}