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

namespace XtraLiteTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal static class Expect
    {
        public static void NotNull(string name, object value)
        {
            if (value == null)
            {
                ExceptionHelper.ArgumentIsNull(name);
            }
        }

        public static void NotEmpty<T>(string name, IEnumerable<T> value)
        {
            NotNull(name, value);

            if (!value.Any())
            {
                ExceptionHelper.ArgumentIsEmpty(name);
            }
        }

        public static void Identifier(string name, string value)
        {
            NotEmpty(name, value);

            var identifierValid =
                (char.IsLetter(value[0]) || value[0] == '_') &&
                value.All(c => char.IsLetterOrDigit(c) || c == '_');

            if (!identifierValid)
            {
                ExceptionHelper.ArgumentIsNotValidIdentifier(name);
            }
        }

        public static void NotEqual<T>(string name1, string name2, T value1, T value2)
        {
            if (EqualityComparer<T>.Default.Equals(value1, value2))
            {
                ExceptionHelper.ArgumentsAreEqual(name1, name2);
            }
        }

        public static void GreaterThan<T>(string name, T value, T than)
        {
            if (Comparer<T>.Default.Compare(value, than) <= 0)
            {
                ExceptionHelper.ArgumentNotGreaterThan(name, than == null ? null : than.ToString());
            }
        }

        public static void GreaterThanOrEqual<T>(string name, T value, T than)
        {
            if (Comparer<T>.Default.Compare(value, than) < 0)
            {
                ExceptionHelper.ArgumentNotGreaterThanOrEqual(name, than == null ? null : than.ToString());
            }
        }

        public static void LessThan<T>(string name, T value, T than)
        {
            if (Comparer<T>.Default.Compare(value, than) >= 0)
            {
                ExceptionHelper.ArgumentNotLessThan(name, than == null ? null : than.ToString());
            }
        }

        public static void LessThanOrEqual<T>(string name, T value, T than)
        {
            if (Comparer<T>.Default.Compare(value, than) > 0)
            {
                ExceptionHelper.ArgumentNotLessThanOrEqual(name, than == null ? null : than.ToString());
            }
        }

        public static void IsTrue(string name, bool condition)
        {
            if (!condition)
            {
                ExceptionHelper.ConditionFailed(name);
            }
        }
    }
}