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

namespace XtraLiteTemplates
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;

    using JetBrains.Annotations;

    internal static class Expect
    {
        [ContractAnnotation("halt <= value: null")]
        public static void NotNull([NotNull] [InvokerParameterName] string argument, object value)
        {
            Debug.Assert(!string.IsNullOrEmpty(argument), "Argument name cannot be empty.");

            if (value == null)
            {
                ExceptionHelper.ArgumentIsNull(argument);
            }
        }

        [ContractAnnotation("halt <= value: false")]
        public static void NotEmpty<T>([NotNull] [InvokerParameterName] string argument, IEnumerable<T> value)
        {
            NotNull(argument, value);

            if (!value.Any())
            {
                ExceptionHelper.ArgumentIsEmpty(argument);
            }
        }

        [ContractAnnotation("halt <= value: false")]
        public static void Identifier([NotNull] [InvokerParameterName] string argument, string value)
        {
            NotEmpty(argument, value);

            var identifierValid = (char.IsLetter(value[0]) || value[0] == '_')
                                  && value.All(c => char.IsLetterOrDigit(c) || c == '_');

            if (!identifierValid)
            {
                ExceptionHelper.ArgumentIsNotValidIdentifier(argument);
            }
        }

        public static void NotEqual<T>(
            [NotNull] [InvokerParameterName] string argument1,
            [NotNull] [InvokerParameterName] string argument2,
            T value1,
            T value2)
        {
            Debug.Assert(!string.IsNullOrEmpty(argument1));
            Debug.Assert(!string.IsNullOrEmpty(argument2));

            if (EqualityComparer<T>.Default.Equals(value1, value2))
            {
                ExceptionHelper.ArgumentsAreEqual(argument1, argument2);
            }
        }

        public static void GreaterThan<T>([NotNull] [InvokerParameterName] string argument, T value, T than) where T : struct
        {
            if (Comparer<T>.Default.Compare(value, than) <= 0)
            {
                ExceptionHelper.ArgumentNotGreaterThan(argument, than.ToString());
            }
        }

        public static void GreaterThanOrEqual<T>([NotNull] [InvokerParameterName] string argument, T value, T than) where T : struct 
        {
            if (Comparer<T>.Default.Compare(value, than) < 0)
            {
                ExceptionHelper.ArgumentNotGreaterThanOrEqual(argument, than.ToString());
            }
        }

        [ContractAnnotation("halt <= condition: false")]
        public static void IsTrue([NotNull] [InvokerParameterName] string argument, bool condition)
        {
            if (!condition)
            {
                ExceptionHelper.ConditionFailed(argument);
            }
        }
    }
}