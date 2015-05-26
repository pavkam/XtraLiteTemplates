﻿//  Author:
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

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using XtraLiteTemplates.Expressions.Operators;

    /// <summary>
    /// Defines a set of permitted continuations that each expression node can expose to the expression builder.
    /// Depending on the state of the expression and the position of the node these can differ.
    /// </summary>
    [Flags]
    internal enum PermittedContinuations
    {
        /// <summary>
        /// No expression term allowed next.
        /// </summary>
        None = 0x00,

        /// <summary>
        /// Any literal is allowed next.
        /// </summary>
        Literal = 0x01,

        /// <summary>
        /// Any identifier is allowed next.
        /// </summary>
        Identifier = 0x02,

        /// <summary>
        /// An unary operator can follow.
        /// </summary>
        UnaryOperator = 0x04,

        /// <summary>
        /// A binary operator can follow.
        /// </summary>
        BinaryOperator = 0x08,

        /// <summary>
        /// A new group can be opened next.
        /// </summary>
        NewGroup = 0x10,

        /// <summary>
        /// The current group can be closed next.
        /// </summary>
        CloseGroup = 0x20,

        /// <summary>
        /// A group separator symbol can follow next.
        /// </summary>
        ContinueGroup = CloseGroup,
    }
}