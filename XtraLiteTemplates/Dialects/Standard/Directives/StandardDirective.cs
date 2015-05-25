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
    using XtraLiteTemplates.Parsing;

    /// <summary>
    /// Abstract base class for all standard directives.
    /// </summary>
    public abstract class StandardDirective : Directive
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDirective"/> class.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        /// <param name="tags">The tags that make up this directive.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="tags"/> or <paramref name="typeConverter"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="tags"/> is empty.</exception>
        /// <exception cref="InvalidOperationException">One or more tags have no defined components.</exception>
        public StandardDirective(IPrimitiveTypeConverter typeConverter, params Tag[] tags)
            : base(tags)
        {
            Expect.NotNull("typeConverter", typeConverter);
            this.TypeConverter = typeConverter;
        }

        /// <summary>
        /// Gets the type converter used to convert to primitive types.
        /// </summary>
        /// <remarks>Value of this property is specified by the caller at construction time.</remarks>
        /// <value>
        /// The type converter.
        /// </value>
        public IPrimitiveTypeConverter TypeConverter { get; private set; }
    }
}