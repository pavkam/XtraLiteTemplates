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

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Does not apply.")]

namespace XtraLiteTemplates.Dialects.Standard.Directives
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Parsing;

    /// <summary>
    /// The IF directive implementation.
    /// </summary>
    public sealed class IfDirective : StandardDirective    
    {
        private int m_expressionIndex;

        /// <summary>
        /// Initializes a new instance of the <see cref="IfDirective" /> class.
        /// </summary>
        /// <param name="startTagMarkup">The start tag markup.</param>
        /// <param name="endTagMarkup">The end tag markup.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="InvalidOperationException">Argument <paramref name="startTagMarkup" /> does not correspond to the expressed rules.</exception>
        /// <exception cref="System.FormatException">Argument <paramref name="startTagMarkup" /> or <paramref name="endTagMarkup" /> cannot be parsed.</exception>
        /// <remarks>
        /// The <paramref name="startTagMarkup" /> is expected to contain exactly one expression components - the conditional expression.
        /// </remarks>
        public IfDirective(string startTagMarkup, string endTagMarkup, IPrimitiveTypeConverter typeConverter)
            : base(typeConverter, Tag.Parse(startTagMarkup), Tag.Parse(endTagMarkup))
        {
            Debug.Assert(this.Tags.Count == 2);

            /* Find all expressions. */
            var tag = Tags[0];
            var expressionComponents = Enumerable.Range(0, tag.ComponentCount)
                .Where(index => tag.MatchesExpression(index)).Select(index => index).ToArray();

            Expect.IsTrue("one expression component", expressionComponents.Length == 1);

            this.m_expressionIndex = expressionComponents[0];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IfDirective" /> class using the standard markup {IF $ THEN}...{END}.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        public IfDirective(IPrimitiveTypeConverter typeConverter)
            : this("IF $ THEN", "END", typeConverter)
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
        /// The directive evaluates only if the expression evaluates to <c>true</c>.
        /// </remarks>
        protected internal override FlowDecision Execute(
            int tagIndex, 
            object[] components, 
            ref object state,
            IExpressionEvaluationContext context, 
            out string text)
        {
            Debug.Assert(tagIndex >= 0 && tagIndex <= 1);            
            Debug.Assert(components != null);
            Debug.Assert(components.Length == this.Tags[tagIndex].ComponentCount);
            Debug.Assert(context != null);

            text = null;
            if (tagIndex == 0)
            {
                if (this.TypeConverter.ConvertToBoolean(components[this.m_expressionIndex]) == true)
                {
                    return FlowDecision.Evaluate;
                }
            }
            
            return FlowDecision.Terminate;
        }
    }
}