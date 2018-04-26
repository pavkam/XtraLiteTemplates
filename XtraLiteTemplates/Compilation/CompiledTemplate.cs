//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//  Copyright (c) 2015-2018, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.Compilation
{
    using System;
    using System.Diagnostics;
    using System.IO;

    using JetBrains.Annotations;

    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;

    /// <summary>
    /// Class that represents a compiled template. Instances of <see cref="CompiledTemplate{TContext}"/> created by a call to <see cref="Interpreter.Compile"/>.
    /// </summary>
    /// <typeparam name="TContext">Any class that implements <see cref="IExpressionEvaluationContext"/> interface.</typeparam>
    [PublicAPI]
    public sealed class CompiledTemplate<TContext>
        where TContext : IExpressionEvaluationContext
    {
        [NotNull]
        private readonly TemplateDocument _document;

        [NotNull]
        private readonly CompiledEvaluationDelegate<TContext> _evaluationDelegate;

        internal CompiledTemplate([NotNull] TemplateDocument document, [NotNull] CompiledEvaluationDelegate<TContext> evaluationDelegate)
        {
            Debug.Assert(document != null, "Argument document cannot be null.");
            Debug.Assert(evaluationDelegate != null, "Argument evaluationDelegate cannot be null.");

            _document = document;
            _evaluationDelegate = evaluationDelegate;
        }

        /// <summary>
        /// Evaluates the specified template using a given evaluation context.
        /// </summary>
        /// <param name="writer">The text writer which will serve as the destination of the evaluated text.</param>
        /// <param name="context">The evaluation context providing all required state and variables.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="writer"/> or <paramref name="context"/> is <c>null</c>.</exception>
        public void Evaluate([NotNull] TextWriter writer, [NotNull] TContext context)
        {
            Expect.NotNull(nameof(writer), writer);
            Expect.NotNull(nameof(context), context);

            _evaluationDelegate(writer, context);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents the initial form of the template.
        /// </returns>
        public override string ToString()
        {
            return _document.ToString();
        }
    }
}