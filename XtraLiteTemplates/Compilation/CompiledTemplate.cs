//  Author:
//    Alexandru Ciobanu alex@ciobanu.org
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

namespace XtraLiteTemplates.Compilation
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;

    /// <summary>
    /// Class that represents a compiled template. Instances of <see cref="CompiledTemplate"/> created by a call to <see cref="Interpreter.Compile"/>.
    /// </summary>
    public sealed class CompiledTemplate
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private TemplateDocument document;
        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private CompiledEvaluationDelegate evaluationDelegate;

        [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        internal CompiledTemplate(TemplateDocument document, CompiledEvaluationDelegate evaluationDelegate)
        {
            Debug.Assert(document != null, "Argument document cannot be null.");
            Debug.Assert(evaluationDelegate != null, "Argument evaluationDelegate cannot be null.");

            this.document = document;
            this.evaluationDelegate = evaluationDelegate;
        }

        /// <summary>
        /// Evaluates the specified template using a given evaluation context.
        /// </summary>
        /// <param name="writer">The text writer which will serve as the destination of the evaluated text.</param>
        /// <param name="context">The evaluation context providing all required state and variables.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="writer"/> or <paramref name="context"/> is <c>null</c>.</exception>
        public void Evaluate(TextWriter writer, IEvaluationContext context)
        {
            Expect.NotNull("writer", writer);
            Expect.NotNull("context", context);

            this.evaluationDelegate(writer, context);
        }
    }
}
