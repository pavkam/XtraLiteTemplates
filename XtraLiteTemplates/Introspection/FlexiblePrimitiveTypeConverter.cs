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

namespace XtraLiteTemplates.Introspection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using JetBrains.Annotations;

    /// <inheritdoc />
    /// <summary>
    /// An implementation of <see cref="T:XtraLiteTemplates.Introspection.IPrimitiveTypeConverter" /> interface. This class offers
    /// a flexible approach to type conversion. It will select the most appropriate way to convert one type to another primitive type guiding
    /// as much as possible by how JavaScript conversion rules operate.
    /// </summary>
    [PublicAPI]
    public sealed class FlexiblePrimitiveTypeConverter : IPrimitiveTypeConverter
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FlexiblePrimitiveTypeConverter" /> class.
        /// </summary>
        /// <param name="formatProvider">Formatting options used to parse string values. Primarily used when parsing <see cref="double" /> values.</param>
        /// <param name="objectFormatter">The object formatter.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="formatProvider" /> or <paramref name="objectFormatter" /> is <c>null</c>.</exception>
        public FlexiblePrimitiveTypeConverter([NotNull] IFormatProvider formatProvider, [NotNull] IObjectFormatter objectFormatter)
        {
            Expect.NotNull(nameof(formatProvider), formatProvider);
            Expect.NotNull(nameof(objectFormatter), objectFormatter);

            FormatProvider = formatProvider;
            ObjectFormatter = objectFormatter;
        }

        /// <summary>
        /// Gets the formatting options used to parse string values. Primarily used when parsing <see cref="double" /> values.
        /// <remarks>The value of the property is set by the caller during class construction.</remarks>
        /// </summary>
        /// <value>
        /// The format provider.
        /// </value>
        [NotNull]
        public IFormatProvider FormatProvider { get; }

        /// <summary>
        /// Gets the object formatter used to transform objects to their string representations.
        /// <remarks>The value of the property is set by the caller during class construction.</remarks>
        /// </summary>
        /// <value>
        /// The object formatter.
        /// </value>
        [NotNull]
        public IObjectFormatter ObjectFormatter { get; }

        /// <inheritdoc />
        public int ConvertToInteger(object obj)
        {
            var number = ConvertToNumber(obj);
            if (double.IsNaN(number) || double.IsInfinity(number))
            {
                return 0;
            }

            return (int)number;
        }

        /// <inheritdoc />
        public double ConvertToNumber(object obj)
        {
            obj = obj is IEnumerable ? ConvertToString(obj) : ReduceObject(obj);

            double result;

            switch (obj)
            {
                case null:
                    result = double.NaN;
                    break;
                case double _:
                    result = (double)obj;
                    break;
                case bool _:
                    result = (bool)obj ? 1 : 0;
                    break;
                case string str when str.Length == 0:
                    result = 0;
                    break;
                case string str:
                    if (!double.TryParse(str, NumberStyles.Float, FormatProvider, out result))
                    {
                        result = double.NaN;
                    }

                    break;
                default:
                    result = double.NaN;
                    break;
            }

            return result;
        }

        /// <inheritdoc />
        public string ConvertToString(object obj)
        {
            switch (obj)
            {
                case string str:
                    return str;
                case IEnumerable enumerable:
                    var builder = new StringBuilder();
                    foreach (var item in enumerable)
                    {
                        if (builder.Length > 0)
                        {
                            builder.Append(',');
                        }

                        builder.Append(ConvertToString(item));
                    }

                    return builder.ToString();
            }

            return ObjectFormatter.ToString(ReduceObject(obj), FormatProvider);
        }

        /// <inheritdoc />
        public bool ConvertToBoolean(object obj)
        {
            obj = ReduceObject(obj);

            bool result;
            switch (obj)
            {
                case null:
                    result = false;
                    break;
                case bool _:
                    result = (bool)obj;
                    break;
                case double _:
                    result = Math.Abs((double)obj) > 0.000000001;
                    break;
                case string _:
                    result = ((string)obj).Length > 0;
                    break;
                default:
                    result = true;
                    break;
            }

            return result;
        }

        /// <inheritdoc />
        public IEnumerable<object> ConvertToSequence(object obj)
        {
            switch (obj)
            {
                case null:
                    return null;
                case IEnumerable<object> objEnumerable:
                    return objEnumerable;
            }

            return obj is IEnumerable enumerable ? UpgradeEnumerable(enumerable) : ObjectToSequence(obj);
        }

        /// <inheritdoc />
        public PrimitiveType TypeOf(object obj)
        {
            if (obj == null)
            {
                return PrimitiveType.Undefined;
            }

            obj = ReduceObject(obj);

            switch (obj)
            {
                case double _:
                    return PrimitiveType.Number;
                case bool _:
                    return PrimitiveType.Boolean;
                case string _:
                    return PrimitiveType.String;
                case IEnumerable _:
                    return PrimitiveType.Sequence;
            }

            return PrimitiveType.Object;
        }

        [NotNull]
        private IEnumerable<object> UpgradeEnumerable([NotNull] IEnumerable enumerable)
        {
            Debug.Assert(enumerable != null, "enumerable cannot be null.");
            foreach (var o in enumerable)
            {
                yield return o;
            }
        }

        [NotNull]
        private IEnumerable<object> ObjectToSequence([CanBeNull] object obj)
        {
            yield return obj;
        }

        [CanBeNull]
        private object ReduceObject([CanBeNull] object obj)
        {
            object reduced;

            switch (obj)
            {
                /* Identify standard types */
                case double _:
                case bool _:
                case string _:
                    reduced = obj;
                    break;
                case byte _:
                    reduced = (double)(byte)obj;
                    break;
                case sbyte _:
                    reduced = (double)(sbyte)obj;
                    break;
                case short _:
                    reduced = (double)(short)obj;
                    break;
                case ushort _:
                    reduced = (double)(ushort)obj;
                    break;
                case int _:
                    reduced = (double)(int)obj;
                    break;
                case uint _:
                    reduced = (double)(uint)obj;
                    break;
                case long _:
                    reduced = (double)(long)obj;
                    break;
                case ulong _:
                    reduced = (double)(ulong)obj;
                    break;
                case float _:
                    reduced = (double)(float)obj;
                    break;
                case decimal _:
                    reduced = (double)(decimal)obj;
                    break;
                default:
                    reduced = obj;
                    break;
            }

            return reduced;
        }
    }
}
