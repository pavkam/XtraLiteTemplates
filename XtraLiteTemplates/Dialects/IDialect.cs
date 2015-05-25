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

namespace XtraLiteTemplates.Dialects
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;

    /// <summary>
    /// Defines all the common properties and behaviors specific to a template language dialect.
    /// A dialect exposes any number of expression operators and directives; specifies all control characters and how
    /// text is parsed.
    /// <para>Check out the <seealso cref="XtraLiteTemplates.Dialects.Standard.StandardDialect"/> and <seealso cref="XtraLiteTemplates.Dialects.Standard.CodeMonkeyDialect"/> for specific implementations of this interface.</para>
    /// </summary>
    public interface IDialect
    {
        /// <summary>
        /// Gets the <see cref="CultureInfo" /> object that drives the formatting and collation behavior of the dialect.
        /// </summary>
        /// <value>
        /// The culture-specific properties.
        /// </value>
        CultureInfo Culture { get; }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{String}" /> object used to compare keywords and identifiers.
        /// </summary>
        /// <value>
        /// The identifier comparer.
        /// </value>
        IEqualityComparer<string> IdentifierComparer { get; }

        /// <summary>
        /// Gets the expression flow symbols used by expressions of this dialect.
        /// </summary>
        /// <value>
        /// The flow symbols.
        /// </value>
        ExpressionFlowSymbols FlowSymbols { get; }

        /// <summary>
        /// Gets all dialect supported expression operators.
        /// </summary>
        /// <value>
        /// The operators.
        /// </value>
        IReadOnlyCollection<Operator> Operators { get; }

        /// <summary>
        /// Gets all dialect supported directives.
        /// </summary>
        /// <value>
        /// The directives.
        /// </value>
        IReadOnlyCollection<Directive> Directives { get; }

        /// <summary>
        /// Gets all dialect supported special constants.
        /// </summary>
        /// <value>
        /// The special keywords.
        /// </value>
        IReadOnlyDictionary<string, object> SpecialKeywords { get; }

        /// <summary>
        /// Processes all unparsed text blocks read from the original template.
        /// </summary>
        /// <param name="context">The <see cref="IExpressionEvaluationContext" /> instance containing the current evaluation state.</param>
        /// <param name="unparsedText">The text block being processed.</param>
        /// <returns>
        /// The processed text value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context" /> is <c>null</c>.</exception>
        string DecorateUnparsedText(IExpressionEvaluationContext context, string unparsedText);

        /// <summary>
        /// Gets the tag start character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The tag start character.
        /// </value>
        char StartTagCharacter { get; }

        /// <summary>
        /// Gets the tag end character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The tag end character.
        /// </value>
        char EndTagCharacter { get; }

        /// <summary>
        /// Gets the string literal start character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal start character.
        /// </value>
        char StartStringLiteralCharacter { get; }

        /// <summary>
        /// Gets the string literal end character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal end character.
        /// </value>
        char EndStringLiteralCharacter { get; }

        /// <summary>
        /// Gets the string literal escape character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal escape character.
        /// </value>
        char StringLiteralEscapeCharacter { get; }

        /// <summary>
        /// Gets the number literal decimal separator character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The number literal decimal separator character.
        /// </value>
        char NumberDecimalSeparatorCharacter { get; }
    }
}
