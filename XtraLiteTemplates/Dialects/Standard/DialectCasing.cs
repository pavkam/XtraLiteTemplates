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

namespace XtraLiteTemplates.Dialects.Standard
{
    using JetBrains.Annotations;

    /// <summary>
    /// Defines the the casing of a standard dialect. This type controls how the keywords, variables and properties are matched.
    /// </summary>
    [PublicAPI]
    public enum DialectCasing
    {
        /// <summary>
        /// The dialect is case-insensitive. All keywords, variables and properties will be considered equals regardless of
        /// their case.
        /// </summary>
        IgnoreCase,

        /// <summary>
        /// The dialect is upper-cased. All keywords are required to be upper case to match. The variables and properties are
        /// case-sensitive.
        /// </summary>
        UpperCase,

        /// <summary>
        /// The dialect is upper-cased. All keywords are required to be lower case to match. The variables and properties are
        /// case-sensitive.
        /// </summary>
        LowerCase,
    }
}
