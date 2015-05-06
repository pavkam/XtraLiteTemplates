//
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
//     * Neither the name of the [ORGANIZATION] nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
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
//

namespace XtraLiteTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    internal static class Expect
    {
        public static void NotNull(String name, Object value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(
                    name, 
                    String.Format("Argument \"{0}\" cannot be null.", name));
            }
        }

        public static void NotEmpty<T>(String name, IEnumerable<T> value)
        {
            NotNull(name, value);
            if (!value.Any())
            {
                throw new ArgumentException(
                    String.Format("Argument \"{0}\" cannot be empty.", name), 
                    name);
            }
        }

        public static void NotEqual<T>(String name1, String name2, T value1, T value2)
        {
            if (EqualityComparer<T>.Default.Equals(value1, value2))
            {
                throw new ArgumentException(String.Format("Arguments \"{0}\" and \"{1}\" cannot be equal.", name1, name2), 
                    String.Format("{0}, {1}", name1, name2));
            }
        }

        public static void GreaterThan<T>(String name, T value, T than)
        {
            if (Comparer<T>.Default.Compare(value, than) <= 0)
            {
                throw new ArgumentOutOfRangeException(
                    name, 
                    String.Format("Argument \"{0}\" is expected to be greater than {1}.", name, than));
            }
        }

        public static void GreaterThanOrEqual<T>(String name, T value, T than)
        {
            if (Comparer<T>.Default.Compare(value, than) < 0)
            {
                throw new ArgumentOutOfRangeException(
                    name, 
                    String.Format("Argument \"{0}\" is expected to be greater than or equal to {1}.", name, than));
            }
        }

        public static void LessThan<T>(String name, T value, T than)
        {
            if (Comparer<T>.Default.Compare(value, than) >= 0)
            {
                throw new ArgumentOutOfRangeException(
                    name, 
                    String.Format("Argument \"{0}\" is expected to be less than {1}.", name, than));
            }
        }

        public static void LessThanOrEqual<T>(String name, T value, T than)
        {
            if (Comparer<T>.Default.Compare(value, than) > 0)
            {
                throw new ArgumentOutOfRangeException(
                    name, 
                    String.Format("Argument \"{0}\" is expected to be less than or equal to {1}.", name, than));
            }
        }

        public static void IsTrue(String name, Boolean condition)
        {
            if (!condition)
            {
                throw new ArgumentException(
                    String.Format("Argument condition \"{0}\" failed to be validated as true.", name), 
                    name);
            }
        }
    }
}