﻿//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2018, Alexandru Ciobanu (alex+git@ciobanu.org)
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
    using System.Diagnostics;
    using System.Linq;

    using JetBrains.Annotations;

    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Introspection;
    using XtraLiteTemplates.Parsing;

    /// <summary>
    /// The IF - ELSE directive implementation.
    /// </summary>
    [PublicAPI]
    public sealed class IfElseDirective : StandardDirective
    {
        private readonly int _conditionalExpressionComponentIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="IfElseDirective" /> class.
        /// </summary>
        /// <param name="startTagMarkup">The start tag markup.</param>
        /// <param name="midTagMarkup">The mid tag markup.</param>
        /// <param name="endTagMarkup">The end tag markup.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="InvalidOperationException">Argument <paramref name="startTagMarkup" /> does not correspond to the expressed rules.</exception>
        /// <exception cref="System.FormatException">Argument <paramref name="startTagMarkup" /> or <paramref name="midTagMarkup" /> or <paramref name="endTagMarkup" /> cannot be parsed.</exception>
        /// <remarks>
        /// The <paramref name="startTagMarkup" /> is expected to contain exactly one expression components - the conditional expression.
        /// </remarks>
        public IfElseDirective(
            [NotNull] string startTagMarkup, 
            [NotNull] string midTagMarkup, 
            [NotNull] string endTagMarkup, 
            [NotNull] IPrimitiveTypeConverter typeConverter)
            : base(typeConverter, Tag.Parse(startTagMarkup), Tag.Parse(midTagMarkup), Tag.Parse(endTagMarkup))
        {
            Debug.Assert(Tags.Count == 3, "Expected a tag count of 3.");

            /* Find all expressions. */
            var tag = Tags[0];
            var expressionComponents = Enumerable.Range(0, tag.ComponentCount)
                .Where(index => tag.MatchesExpression(index)).Select(index => index).ToArray();

            Expect.IsTrue(nameof(expressionComponents), expressionComponents.Length == 1);

            _conditionalExpressionComponentIndex = expressionComponents[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IfElseDirective" /> class using the standard markup {IF $ THEN}...{ELSE}...{END}.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        public IfElseDirective([NotNull] IPrimitiveTypeConverter typeConverter)
            : this("IF $ THEN", "ELSE", "END", typeConverter)
        {
        }

        /// <summary>
        /// Executes the current directive.
        /// </summary>
        /// <param name="tagIndex">The index of the tag that triggered the execution.</param>
        /// <param name="components">The tag components as provided by the lexical analyzer.</param>
        /// <param name="state">A general-purpose state object. Initially set to <c>null</c>.</param>
        /// <param name="context">The evaluation context.</param>
        /// <param name="text">Always <c>null</c>.</param>
        /// <returns>
        /// A value indicating the next step for the evaluation environment.
        /// </returns>
        /// <remarks>
        /// If the expression evaluates to <c>true</c>, the contents between the first and the middle tag are evaluated, other wise the constants between the middle
        /// and the end tag are evaluated.
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
                    var conditionIsTrue = TypeConverter.ConvertToBoolean(components[_conditionalExpressionComponentIndex]);
                    state = conditionIsTrue;

                    return conditionIsTrue ? FlowDecision.Evaluate : FlowDecision.Skip;
                case 1:
                    Debug.Assert(state is bool, "state object expected to be a boolean.");
                    var conditionWasTrue = (bool)state;

                    if (!conditionWasTrue)
                    {
                        return FlowDecision.Evaluate;
                    }

                    break;
            }
            
            return FlowDecision.Terminate;
        }
    }
}