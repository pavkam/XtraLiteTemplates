//  Author:
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

namespace XtraLiteTemplates.Dialects.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using JetBrains.Annotations;
    using XtraLiteTemplates.Introspection;

    /// <summary>
    /// The developer version of the <c>self</c> object implementation. Extends the <see cref="StandardSelfObject"/> with developer-focused methods and properties.
    /// </summary>
    [PublicAPI]
    public class CodeMonkeySelfObject : StandardSelfObject
    {
        /// <summary>
        /// Wrapper over <see cref="Environment"/> singleton class.
        /// </summary>
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public struct SystemEnvironment
        {
            /// <summary>
            /// Gets the name of the machine.
            /// </summary>
            /// <value>
            /// The name of the machine.
            /// </value>
            [NotNull]
            public string MachineName => Environment.MachineName;

            /// <summary>
            /// Gets the name of the user domain.
            /// </summary>
            /// <value>
            /// The name of the user domain.
            /// </value>
            [NotNull]
            public string UserDomainName => Environment.UserDomainName;

            /// <summary>
            /// Gets the name of the user.
            /// </summary>
            /// <value>
            /// The name of the user.
            /// </value>
            [NotNull]
            public string UserName => Environment.UserName;

            /// <summary>
            /// Gets the OS version.
            /// </summary>
            /// <value>
            /// The OS version.
            /// </value>
            [NotNull]
            public string OsVersion => Environment.OSVersion.ToString();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeMonkeySelfObject"/> class.
        /// </summary>
        /// <param name="typeConverter">The type converter.</param>
        public CodeMonkeySelfObject([NotNull] IPrimitiveTypeConverter typeConverter): base(typeConverter)
        {
        }

        /// <summary>
        /// Exposes the static <see cref="SystemEnvironment"/> to the scripts.
        /// </summary>
        /// <value>
        /// The system environment details.
        /// </value>
        public SystemEnvironment System { get; }

        /// <summary>
        /// Gets the new line sequence.
        /// </summary>
        /// <value>
        /// The new line sequence.
        /// </value>
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        [NotNull]
        public string NL { get; } = Environment.NewLine;

        /// <summary>
        /// Wrapper method for <see cref="Math.Abs(double)"/>
        /// </summary>
        /// <param name="value">The argument to obtain absolute value for.</param>
        /// <returns>The absolute value.</returns>
        public double Abs(object value)
        {
            return Math.Abs(TypeConverter.ConvertToNumber(value));
        }

        /// <summary>
        /// Wrapper method for <see cref="Math.Floor(double)"/>
        /// </summary>
        /// <param name="value">The argument to obtain floor value for.</param>
        /// <returns>The floor value.</returns>
        public double Floor(object value)
        {
            return Math.Floor(TypeConverter.ConvertToNumber(value));
        }

        /// <summary>
        /// Wrapper method for <see cref="Math.Ceiling(double)"/>
        /// </summary>
        /// <param name="value">The argument to obtain ceiling value for.</param>
        /// <returns>The ceiling value.</returns>
        public double Ceiling(object value)
        {
            return Math.Ceiling(TypeConverter.ConvertToNumber(value));
        }

        /// <summary>
        /// Wrapper method for <see cref="Math.Round(double, int)"/>
        /// </summary>
        /// <param name="value">The value to round.</param>
        /// <param name="decimals">The number of decimals.</param>
        /// <returns>The rounded value.</returns>
        public double Round(object value, object decimals)
        {
            return Math.Round(TypeConverter.ConvertToNumber(value), TypeConverter.ConvertToInteger(decimals));
        }

        /// <summary>
        /// Wrapper method for <see cref="Math.Min(double, double)"/>
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>The minimum of the two values.</returns>
        public double Min(object value1, object value2)
        {
            return Math.Min(TypeConverter.ConvertToNumber(value1), TypeConverter.ConvertToNumber(value2));
        }

        /// <summary>
        /// Wrapper method for <see cref="Math.Max(double, double)"/>
        /// </summary>
        /// <param name="value1">The first value.</param>
        /// <param name="value2">The second value.</param>
        /// <returns>The maximum of the two values.</returns>
        public double Max(object value1, object value2)
        {
            return Math.Max(TypeConverter.ConvertToNumber(value1), TypeConverter.ConvertToNumber(value2));
        }

        /// <summary>
        /// Wrapper method for <see cref="string.Join{T}(string, IEnumerable{T})"/>
        /// </summary>
        /// <param name="separator">The separator string.</param>
        /// <param name="sequence">The sequence to join.</param>
        /// <returns>The resulting string.</returns>
        [CanBeNull]
        public string Join(object separator, object sequence)
        {
            var enumerable = TypeConverter.ConvertToSequence(sequence);
            return enumerable != null ? string.Join(TypeConverter.ConvertToString(separator), enumerable) : null;
        }
    }
}
