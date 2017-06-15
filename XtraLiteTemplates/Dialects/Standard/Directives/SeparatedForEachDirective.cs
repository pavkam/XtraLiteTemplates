//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2017, Alexandru Ciobanu (alex+git@ciobanu.org)
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
    using System.Diagnostics;
    using System.Linq;
    using Expressions;
    using Introspection;
    using Parsing;

    /// <summary>
    /// The FOR EACH directive implementation that includes a separator text.
    /// </summary>
    public sealed class SeparatedForEachDirective : StandardDirective
    {
        private readonly int _sequenceExpressionComponentIndex;
        private readonly int _identifierComponentIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="SeparatedForEachDirective" /> class.
        /// </summary>
        /// <param name="startTagMarkup">The start tag markup.</param>
        /// <param name="separatorTagMarkup">The separator tag markup.</param>
        /// <param name="endTagMarkup">The end tag markup.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="InvalidOperationException">Argument <paramref name="startTagMarkup" /> does not correspond to the expressed rules.</exception>
        /// <exception cref="System.FormatException">Argument <paramref name="startTagMarkup" /> or <paramref name="separatorTagMarkup" /> or <paramref name="endTagMarkup" /> cannot be parsed.</exception>
        /// <remarks>
        /// The <paramref name="startTagMarkup" /> is expected to contain exactly one expression components and one "any identifier" component.
        /// The identifier is used to represent each element in the evaluated sequence.
        /// </remarks>
        public SeparatedForEachDirective(string startTagMarkup, string separatorTagMarkup, string endTagMarkup, IPrimitiveTypeConverter typeConverter) :
            base(typeConverter, Tag.Parse(startTagMarkup), Tag.Parse(separatorTagMarkup), Tag.Parse(endTagMarkup))
        {
            Debug.Assert(Tags.Count == 3, "Expected a tag count of 3.");

            /* Find all expressions. */
            var tag = Tags[0];
            var expressionComponents = Enumerable.Range(0, tag.ComponentCount)
                .Where(index => tag.MatchesExpression(index)).Select(index => index).ToArray();
            var identifierComponents = Enumerable.Range(0, tag.ComponentCount)
                .Where(index => tag.MatchesAnyIdentifier(index)).Select(index => index).ToArray();

            Expect.IsTrue("one expression component", expressionComponents.Length == 1);
            Expect.IsTrue("one identifier component", identifierComponents.Length == 1);

            _sequenceExpressionComponentIndex = expressionComponents[0];
            _identifierComponentIndex = identifierComponents[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SeparatedForEachDirective"/> class using the standard markup {FOR EACH ? IN $}...{WITH}...{END}.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        public SeparatedForEachDirective(IPrimitiveTypeConverter typeConverter) :
            this("FOR EACH ? IN $", "WITH", "END", typeConverter)
        {
        }

        /// <summary>
        /// Executes the current directive.
        /// </summary>
        /// <param name="tagIndex">The index of the tag that triggered the execution.</param>
        /// <param name="components">The tag components as provided by the lexical analyzer.</param>
        /// <param name="state">A general-purpose state object. Initially set to <c>null</c>.</param>
        /// <param name="context">The evaluation context.</param>
        /// <param name="text">An optional text generated by the directive.</param>
        /// <returns>
        /// A value indicating the next step for the evaluation environment.
        /// </returns>
        /// <remarks>
        /// If the expression is evaluated to <c>null</c>, the directive does not evaluate. It evaluates once for non-sequences and once for each element if the
        /// expression is a sequence. On each cycle the identifier is set to the value of the enumerated object.
        /// <para>
        /// The contents between the middle tag and the end tag are inserted between the evaluated items, simulating the <see cref="string.Join(string,IEnumerable{string})" /> method.
        /// </para>
        /// </remarks>
        protected internal override FlowDecision Execute(
            int tagIndex, 
            object[] components, 
            ref object state,
            IExpressionEvaluationContext context, 
            out string text)
        {
            Debug.Assert(tagIndex >= 0 && tagIndex <= 2, "tagIndex must be between 0 and 2.");
            Debug.Assert(components != null, "components cannot be null.");
            Debug.Assert(components.Length == Tags[tagIndex].ComponentCount, "component length must match tag component length.");
            Debug.Assert(context != null, "context cannot be null.");

            text = null;
            switch (tagIndex)
            {
                case 0:
                    if (state == null)
                    {
                        var sequence = TypeConverter.ConvertToSequence(components[_sequenceExpressionComponentIndex]);
                        if (sequence == null)
                        {
                            return FlowDecision.Terminate;
                        }

                        var enumerator = sequence.GetEnumerator();
                        if (!enumerator.MoveNext())
                        {
                            return FlowDecision.Terminate;
                        }

                        var propertyName = components[_identifierComponentIndex] as string;
                        context.SetProperty(propertyName, enumerator.Current);

                        state = new State { Enumerator = enumerator, IsLast = !enumerator.MoveNext(), };

                        return FlowDecision.Evaluate;
                    }
                    else
                    {
                        var asState1 = state as State;

                        Debug.Assert(asState1 != null, "state should be a proper object.");
                        Debug.Assert(asState1.Enumerator != null, "state enumerator cannot not be null.");
                        Debug.Assert(!asState1.IsLast, "iteration cannot be the last.");

                        var propertyName = components[_identifierComponentIndex] as string;
                        context.SetProperty(propertyName, asState1.Enumerator.Current);
                        asState1.IsLast = !asState1.Enumerator.MoveNext();

                        return FlowDecision.Evaluate;
                    }
                case 1:
                    var asState2 = state as State;

                    Debug.Assert(asState2 != null, "state should be a proper object.");
                    Debug.Assert(asState2.Enumerator != null, "state enumerator cannot be null.");

                    return asState2.IsLast ? FlowDecision.Terminate : FlowDecision.Evaluate;
            }

            return FlowDecision.Restart;
        }

        private class State
        {
            public IEnumerator<object> Enumerator { get; set; }

            public bool IsLast { get; set; }
        }
    }
}