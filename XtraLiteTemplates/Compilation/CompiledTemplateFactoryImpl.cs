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

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Does not apply.")]

namespace XtraLiteTemplates.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]

    internal sealed class CompiledTemplateFactoryImpl : CompiledTemplateFactory<EvaluationContextImpl>
    {
        protected override CompiledEvaluationDelegate<EvaluationContextImpl> FinalizeEvaluationDelegate(CompiledEvaluationDelegate<EvaluationContextImpl> finalDelegate)
        {
            Debug.Assert(finalDelegate != null, "Argument finalDelegate cannot be null.");

            return (writer, context) =>
            {
                try
                {
                    finalDelegate(writer, context);
                }
                catch (OperationCanceledException)
                {
                    /* Just eat it. The whole evaluation was cancelled using the cancellation token. */
                }
            };
        }

        protected override CompiledEvaluationDelegate<EvaluationContextImpl> BuildDoubleTagDirectiveEvaluationDelegate(
            Directive directive,
            CompiledEvaluationDelegate<EvaluationContextImpl> innerDelegate,
            object[] openComponents, 
            object[] closeComponents)
        {
            Debug.Assert(directive != null, "Argument directive cannot be null.");
            Debug.Assert(openComponents != null, "Argument openComponents cannot be null.");
            Debug.Assert(openComponents.Length > 0, "Argument openComponents cannot be empty.");
            Debug.Assert(closeComponents != null, "Argument closeComponents cannot be null.");
            Debug.Assert(closeComponents.Length > 0, "Argument closeComponents cannot be empty.");

            return (writer, context) =>
            {
                context.OpenEvaluationFrame();
                try
                {
                    EvaluateDoubleTagDirective(
                        writer,
                        context,
                        directive,
                        openComponents,
                        closeComponents,
                        innerDelegate);
                }
                catch (OperationCanceledException cancelException)
                {
                    throw cancelException;
                }
                catch (Exception exception)
                {
                    if (!context.IgnoreEvaluationExceptions)
                    {
                        ExceptionHelper.DirectiveEvaluationError(exception, directive);
                    }
                }
                finally
                {
                    context.CloseEvaluationFrame();
                }
            };
        }

        protected override CompiledEvaluationDelegate<EvaluationContextImpl> BuildMultipleTagDirectiveEvaluationDelegate(
            Directive directive,
            CompiledEvaluationDelegate<EvaluationContextImpl>[] innerDelegates,
            object[][] components)
        {
            Debug.Assert(directive != null, "Argument directive cannot be null.");
            Debug.Assert(components != null, "Argument components cannot be null.");
            Debug.Assert(components.Length == directive.Tags.Count, "Argument components must have the same length as the directive tags.");

            return (writer, context) =>
            {
                context.OpenEvaluationFrame();
                try
                {
                    EvaluateTagDirective(
                        writer,
                        context,
                        directive,
                        components,
                        innerDelegates);
                }
                catch (OperationCanceledException cancelException)
                {
                    throw cancelException;
                }
                catch (Exception exception)
                {
                    if (!context.IgnoreEvaluationExceptions)
                    {
                        ExceptionHelper.DirectiveEvaluationError(exception, directive);
                    }
                }
                finally
                {
                    context.CloseEvaluationFrame();
                }
            };
        }

        protected override CompiledEvaluationDelegate<EvaluationContextImpl> BuildUnparsedTextEvaluationDelegate(string unparsedText)
        {
            Debug.Assert(!string.IsNullOrEmpty(unparsedText), "Argument unparsedText cannot be empty.");

            return (writer, context) =>
            {
                Debug.Assert(writer != null, "Argument writer cannot be null.");
                Debug.Assert(context != null, "Argument context cannot be null.");

                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                var text = context.ProcessUnparsedText(unparsedText);

                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                if (!string.IsNullOrEmpty(text))
                {
                    writer.Write(text);
                }
            };
        }

        protected override CompiledEvaluationDelegate<EvaluationContextImpl> BuildSingleTagDirectiveEvaluationDelegate(
            Directive directive,
            object[] components)
        {
            Debug.Assert(directive != null, "Argument directive cannot be null.");
            Debug.Assert(components != null, "Argument components cannot be null.");
            Debug.Assert(components.Length > 0, "Argument components cannot be empty.");

            return (writer, context) =>
            {
                try
                {
                    EvaluateSingleTagDirective(writer, context, directive, components);
                }
                catch (OperationCanceledException cancelException)
                {
                    throw cancelException;
                }
                catch (Exception exception)
                {
                    if (!context.IgnoreEvaluationExceptions)
                    {
                        ExceptionHelper.DirectiveEvaluationError(exception, directive);
                    }
                }
            };
        }

        private static object[] EvaluateTag(EvaluationContextImpl context, object[] components)
        {
            object[] result = new object[components.Length];
            for (var i = 0; i < components.Length; i++)
            {
                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                var expression = components[i] as Expression;
                if (expression != null)
                {
                    /* Evaluate the expression. */
                    result[i] = expression.Evaluate(context);
                }
                else
                {
                    /* No evaluation required. */
                    result[i] = components[i];
                }
            }

            return result;
        }

        private static void EvaluateSingleTagDirective(
            TextWriter writer,
            EvaluationContextImpl context,
            Directive directive,
            object[] components)
        {
            /* Pre-evaluate the tag's components, as these  */
            var tagEvaluatedComponents = EvaluateTag(context, components);

            /* Evaluate tag. */
            object state = null;
            var flowDecision = Directive.FlowDecision.Evaluate;
            while (flowDecision != Directive.FlowDecision.Terminate)
            {
                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                string directiveText;
                flowDecision = directive.Execute(0, tagEvaluatedComponents, ref state, context, out directiveText);

                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                if (directiveText != null)
                {
                    writer.Write(directiveText);
                }
            }
        }

        private static void EvaluateDoubleTagDirective(
            TextWriter writer,
            EvaluationContextImpl context,
            Directive directive,
            object[] beginComponents,
            object[] closeComponents,
            CompiledEvaluationDelegate<EvaluationContextImpl> beginToCloseEvaluateProc)
        {
            /* Get tag components. */
            object[] beginTagEvaluatedComponents = EvaluateTag(context, beginComponents);
            object[] closeTagEvaluatedComponents = null;

            /* Evaluate tag. */
            object state = null;
            while (true)
            {
                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                string directiveText;
                var flowDecision = directive.Execute(0, beginTagEvaluatedComponents, ref state, context, out directiveText);

                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                if (directiveText != null)
                {
                    writer.Write(directiveText);
                }

                if (flowDecision == Directive.FlowDecision.Terminate)
                {
                    break;
                }
                else if (flowDecision == Directive.FlowDecision.Restart)
                {
                    continue;
                }
                else if (flowDecision == Directive.FlowDecision.Evaluate && beginToCloseEvaluateProc != null)
                {
                    beginToCloseEvaluateProc(writer, context);
                }

                if (closeTagEvaluatedComponents == null)
                {
                    closeTagEvaluatedComponents = EvaluateTag(context, closeComponents);
                }

                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                flowDecision = directive.Execute(1, closeTagEvaluatedComponents, ref state, context, out directiveText);

                if (directiveText != null)
                {
                    writer.Write(directiveText);
                }

                if (flowDecision == Directive.FlowDecision.Terminate)
                {
                    break;
                }
            }
        }

        private static void EvaluateTagDirective(
            TextWriter writer,
            EvaluationContextImpl context,
            Directive directive,
            object[][] components,
            CompiledEvaluationDelegate<EvaluationContextImpl>[] evaluationProcs)
        {
            /* Get tag components. */
            object[][] evaluatedComponents = new object[components.Length][];

            /* Evaluate tag. */
            object state = null;
            var currentTagIndex = 0;
            while (currentTagIndex >= 0)
            {
                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                if (evaluatedComponents[currentTagIndex] == null)
                {
                    evaluatedComponents[currentTagIndex] = EvaluateTag(context, components[currentTagIndex]);
                }

                string directiveText;
                var flowDecision = directive.Execute(currentTagIndex, evaluatedComponents[currentTagIndex], ref state, context, out directiveText);

                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                if (directiveText != null)
                {
                    writer.Write(directiveText);
                }

                if (flowDecision == Directive.FlowDecision.Terminate)
                {
                    currentTagIndex = -1;
                }
                else if (flowDecision == Directive.FlowDecision.Restart)
                {
                    currentTagIndex = 0;
                }
                else
                {
                    currentTagIndex++;
                    if (currentTagIndex == components.Length)
                    {
                        currentTagIndex = 0;
                    }
                    else if (flowDecision == Directive.FlowDecision.Evaluate)
                    {
                        var evaluationProc = evaluationProcs[currentTagIndex - 1];

                        if (evaluationProc != null)
                        {
                            evaluationProc(writer, context);
                        }
                    }
                }
            }
        }
    }
}