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

namespace XtraLiteTemplates.Expressions
{
    using System;

    using JetBrains.Annotations;

    /// <summary>
    /// Supplies a set of standard flow symbols used during the expression construction phase.
    /// </summary>
    [PublicAPI]
    public sealed class ExpressionFlowSymbols
    {
        /// <summary>
        /// Initializes static members of the <see cref="ExpressionFlowSymbols"/> class.
        /// </summary>
        static ExpressionFlowSymbols()
        {
            Default = new ExpressionFlowSymbols(",", ".", "(", ")");
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExpressionFlowSymbols"/> class.
        /// </summary>
        /// <param name="separatorSymbol">The group separator symbol.</param>
        /// <param name="memberAccessSymbol">The member access symbol.</param>
        /// <param name="groupOpenSymbol">The group open symbol.</param>
        /// <param name="groupCloseSymbol">The group close symbol.</param>
        /// <exception cref="ArgumentNullException">One or more symbols are <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Some symbols are equal.</exception>
        public ExpressionFlowSymbols(
            [NotNull] string separatorSymbol,
            [NotNull] string memberAccessSymbol,
            [NotNull] string groupOpenSymbol,
            [NotNull] string groupCloseSymbol)
        {
            Expect.NotEmpty(nameof(separatorSymbol), separatorSymbol);
            Expect.NotEmpty(nameof(memberAccessSymbol), memberAccessSymbol);
            Expect.NotEmpty(nameof(groupOpenSymbol), groupOpenSymbol);
            Expect.NotEmpty(nameof(groupCloseSymbol), groupCloseSymbol);

            Expect.NotEqual(nameof(separatorSymbol), nameof(memberAccessSymbol), separatorSymbol, memberAccessSymbol);
            Expect.NotEqual(nameof(separatorSymbol), nameof(groupOpenSymbol), separatorSymbol, groupOpenSymbol);
            Expect.NotEqual(nameof(separatorSymbol), nameof(groupCloseSymbol), separatorSymbol, groupCloseSymbol);
            Expect.NotEqual(nameof(memberAccessSymbol), nameof(groupOpenSymbol), memberAccessSymbol, groupOpenSymbol);
            Expect.NotEqual(nameof(memberAccessSymbol), nameof(groupCloseSymbol), memberAccessSymbol, groupCloseSymbol);
            Expect.NotEqual(nameof(groupOpenSymbol), nameof(groupCloseSymbol), groupOpenSymbol, groupCloseSymbol);

            Separator = separatorSymbol;
            MemberAccess = memberAccessSymbol;
            GroupOpen = groupOpenSymbol;
            GroupClose = groupCloseSymbol;
        }

        /// <summary>
        /// Gets the default set of expression flow symbols.
        /// </summary>
        /// <value>
        /// The default set of symbols.
        /// </value>
        [NotNull]
        public static ExpressionFlowSymbols Default { get; }

        /// <summary>
        /// Gets the group separator symbol.
        /// </summary>
        /// <value>
        /// The group separator symbol.
        /// </value>
        [NotNull]
        public string Separator { get; }

        /// <summary>
        /// Gets the member access symbol.
        /// </summary>
        /// <value>
        /// The member access symbol.
        /// </value>
        [NotNull]
        public string MemberAccess { get; }

        /// <summary>
        /// Gets the group open symbol.
        /// </summary>
        /// <value>
        /// The group open symbol.
        /// </value>
        [NotNull]
        public string GroupOpen { get; }

        /// <summary>
        /// Gets the group close symbol.
        /// </summary>
        /// <value>
        /// The group close symbol.
        /// </value>
        [NotNull]
        public string GroupClose { get; }
    }
}
