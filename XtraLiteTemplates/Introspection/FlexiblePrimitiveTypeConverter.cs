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

namespace XtraLiteTemplates.Introspection
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Text;
    using JetBrains.Annotations;

    /// <summary>
    /// An implementation of <see cref="IPrimitiveTypeConverter"/> interface. This class offers
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

        /// <summary>
        /// Tries to convert the value of <paramref name="obj"/> argument to a 32-bit integer.
        /// <remarks>A value of <c>0</c> is returned if <paramref name="obj"/> is not directly convertible to an <c>int</c>.</remarks>
        /// </summary>
        /// <param name="obj">The value object to convert.</param>
        /// <returns>A <see cref="int"/> value.</returns>
        public int ConvertToInteger(object obj)
        {
            var number = ConvertToNumber(obj);
            if (double.IsNaN(number) || double.IsInfinity(number))
            {
                return 0;
            }

            return (int)number;
        }

        /// <summary>
        /// Tries to convert the value of <paramref name="obj"/> argument to a double-precision floating point number.
        /// <remarks>A value of <see cref="double.NaN"/> is returned if <paramref name="obj"/> is not directly convertible to a <c>double</c>.</remarks>
        /// </summary>
        /// <param name="obj">The value object to convert.</param>
        /// <returns>A <see cref="double"/> value.</returns>
        public double ConvertToNumber(object obj)
        {
            if (obj is IEnumerable)
            {
                obj = ConvertToString(obj);
            }
            else
            {
                obj = ReduceObject(obj);
            }

            double result;

            if (obj == null)
            {
                result = double.NaN;
            }
            else if (obj is double)
            {
                result = (double)obj;
            }
            else if (obj is bool)
            {
                result = (bool)obj ? 1 : 0;
            }
            else
            {
                var str = obj as string;
                if (str != null)
                {
                    if (str.Length == 0)
                    {
                        result = 0;
                    }
                    else if (!double.TryParse(str, NumberStyles.Float, FormatProvider, out result))
                    {
                        result = double.NaN;
                    }
                }
                else
                {
                    result = double.NaN;
                }
            }

            return result;
        }

        /// <summary>
        /// Tries to convert the value of <paramref name="obj"/> argument to a string.
        /// <remarks>A value of <c>null</c> is returned if <paramref name="obj"/> is null; otherwise the object is formatted accordingly.</remarks>
        /// </summary>
        /// <param name="obj">The value object to convert.</param>
        /// <returns>A <see cref="string"/> value.</returns>
        public string ConvertToString(object obj)
        {
            var str = obj as string;
            if (str != null)
            {
                return str;
            }

            var enumerable = obj as IEnumerable;
            if (enumerable != null)
            {
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

        /// <summary>
        /// Tries to convert the value of <paramref name="obj"/> argument to a boolean.
        /// <remarks>A value of <c>false</c> is returned if <paramref name="obj"/> is not directly convertible to a <c>bool</c>.</remarks>
        /// </summary>
        /// <param name="obj">The value object to convert.</param>
        /// <returns>A <see cref="bool"/> value.</returns>
        public bool ConvertToBoolean(object obj)
        {
            obj = ReduceObject(obj);

            bool result;
            if (obj == null)
            {
                result = false;
            }
            else if (obj is bool)
            {
                result = (bool)obj;
            }
            else if (obj is double)
            {
                result = Math.Abs((double)obj) > 0.000000001;
            }
            else if (obj is string)
            {
                result = ((string)obj).Length > 0;
            }
            else
            {
                result = true;
            }

            return result;
        }

        /// <summary>
        /// Tries to convert the value of <paramref name="obj"/> argument to a sequence of objects.
        /// <remarks>A value of <c>null</c> is returned if <paramref name="obj"/> is null. If <paramref name="obj"/> is not an enumerable object,
        /// a new enumerable, containing <paramref name="obj"/> is returned.</remarks>
        /// </summary>
        /// <param name="obj">The value object to convert.</param>
        /// <returns>A <see cref="IEnumerable{Object}"/> value.</returns>
        public IEnumerable<object> ConvertToSequence(object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var objEnumerable = obj as IEnumerable<object>;
            if (objEnumerable != null)
            {
                return objEnumerable;
            }

            var enumerable = obj as IEnumerable;
            return enumerable != null ? UpgradeEnumerable(enumerable) : ObjectToSequence(obj);
        }

        /// <summary>
        /// Detects the primitive type of the supplied object.
        /// </summary>
        /// <param name="obj">The object to check the type for.</param>
        /// <returns>A <see cref="PrimitiveType"/> value.</returns>
        public PrimitiveType TypeOf(object obj)
        {
            if (obj == null)
            {
                return PrimitiveType.Undefined;
            }

            obj = ReduceObject(obj);

            if (obj is double)
            {
                return PrimitiveType.Number;
            }

            if (obj is bool)
            {
                return PrimitiveType.Boolean;
            }

            if (obj is string)
            {
                return PrimitiveType.String;
            }

            if (obj is IEnumerable)
            {
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

            /* Identify standard types */
            if (obj is double || obj is bool || obj is string)
            {
                reduced = obj;
            }
            else if (obj is byte)
            {
                reduced = (double)(byte)obj;
            }
            else if (obj is sbyte)
            {
                reduced = (double)(sbyte)obj;
            }
            else if (obj is short)
            {
                reduced = (double)(short)obj;
            }
            else if (obj is ushort)
            {
                reduced = (double)(ushort)obj;
            }
            else if (obj is int)
            {
                reduced = (double)(int)obj;
            }
            else if (obj is uint)
            {
                reduced = (double)(uint)obj;
            }
            else if (obj is long)
            {
                reduced = (double)(long)obj;
            }
            else if (obj is ulong)
            {
                reduced = (double)(ulong)obj;
            }
            else if (obj is float)
            {
                reduced = (double)(float)obj;
            }
            else if (obj is decimal)
            {
                reduced = (double)(decimal)obj;
            }
            else
            {
                reduced = obj;
            }

            return reduced;
        }
    }
}
