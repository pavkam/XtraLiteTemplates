//  Author:
//    Alexandru Ciobanu alex@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex@ciobanu.org)
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

namespace XtraLiteTemplates.Dialects.Standard.Operators
{
    using System;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Introspection;

    /// <summary>
    /// Implements the standard object formatting (':') operation.
    /// </summary>
    public sealed class FormatOperator : StandardBinaryOperator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FormatOperator" /> class.
        /// </summary>
        /// <param name="symbol">The operator's symbol.</param>
        /// <param name="formatProvider">The culture-specific formatting provider.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="symbol" /> or <paramref name="typeConverter" /> or <paramref name="formatProvider" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="symbol" /> is empty.</exception>
        public FormatOperator(string symbol, IFormatProvider formatProvider, IPrimitiveTypeConverter typeConverter)
            : base(symbol, 2, typeConverter)
        {
            Expect.NotNull("formatProvider", formatProvider);

            this.FormatProvider = formatProvider;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FormatOperator" /> class using the standard ':' symbol.
        /// </summary>
        /// <param name="formatProvider">The culture-specific formatting provider.</param>
        /// <param name="typeConverter">The type converter.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> or <paramref name="formatProvider" /> is <c>null</c>.</exception>
        public FormatOperator(IFormatProvider formatProvider, IPrimitiveTypeConverter typeConverter)
            : this(":", formatProvider, typeConverter)
        {
        }

        /// <summary>
        /// Gets the culture-specific format options used by this operation.
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The culture-specific formatting options.
        /// </value>
        public IFormatProvider FormatProvider { get; private set; }

        /// <summary>
        /// Evaluates the result of formatting operation for <paramref name="left" /> and <paramref name="right" /> operands.
        /// The <paramref name="right" /> operand is expected to be the format string.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <param name="left">The left operand.</param>
        /// <param name="right">The right operand.</param>
        /// <returns>
        /// The evaluated object.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context" /> is <c>null</c>.</exception>
        public override object Evaluate(IExpressionEvaluationContext context, object left, object right)
        {
            Expect.NotNull("context", context);

            var formattable = left as IFormattable;
            if (formattable != null)
            {
                try
                {
                    return formattable.ToString(this.TypeConverter.ConvertToString(right), this.FormatProvider);
                }
                catch
                {
                }
            }

            return null;
        }
    }
}