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
    using System.Diagnostics;
    using System.Reflection;

    using JetBrains.Annotations;

    /// <summary>
    /// Event arguments class used by <see cref="SimpleTypeDisemboweler"/> class to decide whether a type member is accepted or rejected.
    /// </summary>
    [PublicAPI]
    public sealed class MemberValidationEventArgs : EventArgs
    {
        internal MemberValidationEventArgs([NotNull] MemberInfo memberInfo)
        {
            Debug.Assert(memberInfo != null, "Argument memberInfo cannot be null.");

            Member = memberInfo;
            Accepted = true;
        }

        /// <summary>
        /// Gets the type member for which the acceptance decision is to be taken.
        /// </summary>
        /// <value>
        /// The type member.
        /// </value>
        [NotNull]
        public MemberInfo Member { get; }

        /// <summary>
        /// Gets or sets a value indicating whether type member is accepted as a suitable candidate.
        /// <remarks>By default all members are accepted. In order to reject a type member, set this property to <c>false</c>.</remarks>
        /// </summary>
        /// <value>
        /// The acceptance of a type member.
        /// </value>
        public bool Accepted { get; set; }
    }
}
