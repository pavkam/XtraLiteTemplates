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
    using System.Collections.Generic;
    using JetBrains.Annotations;

    /// <summary>
    /// Common interface used by the standard dialects to facilitate type-conversion. Standard operators and directives
    /// use this interface to obtain the data type required for their correct operation.
    /// </summary>
    [PublicAPI]
    public interface IPrimitiveTypeConverter
    {
        /// <summary>
        /// Detects the primitive type of the supplied object. Standard operators and directives can use this
        /// method to detect the data types involved in the operation and decide on the appropriate evaluation techniques.
        /// </summary>
        /// <param name="obj">The object to check the type for.</param>
        /// <returns>
        /// A <see cref="PrimitiveType" /> value.
        /// </returns>
        PrimitiveType TypeOf([CanBeNull] object obj);

        /// <summary>
        /// Converts an object to a 32-bit integer.
        /// <remarks>
        /// This method is guaranteed to always return a value and may never fail.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>
        /// A <see cref="int" /> value.
        /// </returns>
        int ConvertToInteger([CanBeNull] object obj);

        /// <summary>
        /// Converts an object to a double-precision floating point number.
        /// <remarks>
        /// This method is guaranteed to always return a value and may never fail.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>
        /// A <see cref="double" /> value.
        /// </returns>
        double ConvertToNumber([CanBeNull] object obj);

        /// <summary>
        /// Converts an object to its string representation.
        /// <remarks>
        /// This method is guaranteed to always return a value and may never fail.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>
        /// A <see cref="string" /> value.
        /// </returns>
        [CanBeNull]
        string ConvertToString([CanBeNull] object obj);

        /// <summary>
        /// Converts an object to a boolean.
        /// <remarks>
        /// This method is guaranteed to always return a value and may never fail.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>
        /// A <see cref="bool" /> value.
        /// </returns>
        bool ConvertToBoolean([CanBeNull] object obj);

        /// <summary>
        /// Converts an object to a sequence of objects.
        /// <remarks>
        /// This method is guaranteed to always return a value and may never fail.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>
        /// A <see cref="IEnumerable{Object}" /> value.
        /// </returns>
        [CanBeNull]
        IEnumerable<object> ConvertToSequence([CanBeNull] object obj);
    }
}
