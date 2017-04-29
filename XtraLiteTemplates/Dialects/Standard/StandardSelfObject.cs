//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.Dialects.Standard
{
    using System;
    using System.Collections.Generic;
    using XtraLiteTemplates.Introspection;

    /// <summary>
    /// The standard <c>self</c> object implementation. Provides a set of standard methods and properties exposed globally to
    /// all templates running under standard dialects.
    /// </summary>
    public class StandardSelfObject
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="StandardSelfObject"/> class.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        public StandardSelfObject(IPrimitiveTypeConverter typeConverter)
        {
            Expect.NotNull("typeConverter", typeConverter);

            this.TypeConverter = typeConverter;
        }

        /// <summary>
        /// Gets the type converter.
        /// </summary>
        /// <value>
        /// The type converter.
        /// </value>
        /// <remarks>The value of this property is supplied by the caller during instance initialization.</remarks>
        public IPrimitiveTypeConverter TypeConverter { get; private set; }

        /// <summary>
        /// Converts an <see cref="System.Object"/> to a <see cref="System.String"/> using the specified <see cref="TypeConverter"/>.
        /// </summary>
        /// <param name="argument">The argument to convert.</param>
        /// <returns>The <c>string</c> representation of the <paramref name="argument"/>.</returns>
        public string String(object argument)
        {
            return this.TypeConverter.ConvertToString(argument);
        }

        /// <summary>
        /// Converts an <see cref="System.Object"/> to a <see cref="System.Double"/> using the specified <see cref="TypeConverter"/>.
        /// </summary>
        /// <param name="argument">The argument to convert.</param>
        /// <returns>The <c>numerical</c> representation of the <paramref name="argument"/>.</returns>
        public double Number(object argument)
        {
            return this.TypeConverter.ConvertToNumber(argument);
        }

        /// <summary>
        /// Converts an <see cref="System.Object"/> to a <see cref="System.Boolean"/> using the specified <see cref="TypeConverter"/>.
        /// </summary>
        /// <param name="argument">The argument to convert.</param>
        /// <returns>The <c>boolean</c> representation of the <paramref name="argument"/>.</returns>
        public bool Boolean(object argument)
        {
            return this.TypeConverter.ConvertToBoolean(argument);
        }
    }
}
