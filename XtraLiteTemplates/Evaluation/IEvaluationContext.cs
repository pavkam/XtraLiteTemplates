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

namespace XtraLiteTemplates.Evaluation
{
    using System;
    using System.Globalization;
    using System.IO;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions;

    /// <summary>
    /// Defines all the traits and properties required for template evaluation.
    /// Implementers of <see cref="IEvaluationContext" /> need to provide basic variable and state management.
    /// </summary>
    public interface IEvaluationContext : IExpressionEvaluationContext
    {
        /// <summary>
        /// Gets a value indicating whether evaluation exceptions are silently ignored (or result in <c>null</c> values) during template evaluation.
        /// </summary>
        /// <value>
        /// <c>true</c> if evaluation exceptions are ignored; otherwise, <c>false</c>.
        /// </value>
        bool IgnoreEvaluationExceptions { get; }

        /// <summary>
        /// Processes the unparsed text blocks.
        /// <remarks>
        /// <see cref="ProcessUnparsedText"/> is invoked during the evaluation process for each unparsed text block before it is committed to the result.
        /// </remarks>
        /// </summary>
        /// <param name="value">The unparsed text block to process.</param>
        /// <returns>The processed text.</returns>
        string ProcessUnparsedText(string value);

        /// <summary>
        /// Opens an evaluation frame. An evaluation frame can be considered as a rudimentary stack frame.
        /// <para>
        /// Each variable or state object lives in its own frame. The evaluation environment opens up a frame for each directive it encounters.
        /// </para>
        /// </summary>
        void OpenEvaluationFrame();

        /// <summary>
        /// Closes an evaluation frame.
        /// <para>
        /// The evaluation environment closes up frames after each directive finished its execution. It is expected that any
        /// state objects and variables belonging to the frame are cleared in the process.
        /// </para>
        /// </summary>
        void CloseEvaluationFrame();
    }
}
