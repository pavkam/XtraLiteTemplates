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

namespace XtraLiteTemplates.Compilation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Runtime.InteropServices;
    using Evaluation;
    using Expressions;
    using JetBrains.Annotations;

    /// <summary>
    /// Class that encapsulates <c>compilation</c> logic defined for the standard <see cref="EvaluationContext"/>. The sole purpose of <see cref="CompiledTemplateFactory{TContext}"/> is to
    /// transform the internal <c>lexed</c> representation of a template to its compiled form (form that can be evaluated).
    /// </summary>
    [ComVisible(false)]
    [PublicAPI]
    public class CompiledTemplateFactory : CompiledTemplateFactory<EvaluationContext>
    {
        /// <summary>
        /// Constructs a delegate used in evaluation of a given double-tag <paramref name="directive"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="BuildDoubleTagDirectiveEvaluationDelegate"/> method is used to build a new delegate for directives
        /// that are composed of two tags(e.g. <c>{IF}...{END}</c>. The contents (all inner document nodes) found between the directive's tags is
        /// passed along in <paramref name="innerDelegate"/> parameter. If there are no nodes between its two consecutive tags, a <c>null</c> value
        /// is passed. The <paramref name="openComponents"/> and <paramref name="closeComponents"/> contain the components for the opening and closing tags.
        /// </remarks>
        /// <param name="directive">The directive.</param>
        /// <param name="innerDelegate">A delegate that represents the contents of the inner nodes.</param>
        /// <param name="openComponents">The components associated with the opening tag.</param>
        /// <param name="closeComponents">The components associated with the closing tag.</param>
        /// <returns>A new delegate tasked with evaluating the given directive.</returns>
        protected override CompiledEvaluationDelegate<EvaluationContext> BuildDoubleTagDirectiveEvaluationDelegate(
            Directive directive,
            CompiledEvaluationDelegate<EvaluationContext> innerDelegate,
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
                catch (OperationCanceledException)
                {
                    throw;
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

        /// <summary>
        /// Constructs a delegate used in evaluation of a given multi-tag <paramref name="directive"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="BuildMultipleTagDirectiveEvaluationDelegate"/> method is used to build a new delegate for directives
        /// that are composed of more than two tags. The contents (all inner document nodes) found between the directive's tags is
        /// passed along in <paramref name="innerDelegates"/> parameter. If there are no nodes between two consecutive tags, a <c>null</c> element
        /// is added the to list. The <paramref name="components"/> argument holds a list of component arrays. Each component array matches one
        /// of the directive's tags: for a directive consisting of N tags, <paramref name="components"/> will have a length of N.
        /// </remarks>
        /// <param name="directive">The directive.</param>
        /// <param name="innerDelegates">A list of evaluable delegates that represent all inner nodes.</param>
        /// <param name="components">A list of tag components.</param>
        /// <returns>A new delegate tasked with evaluating the given directive.</returns>
        protected override CompiledEvaluationDelegate<EvaluationContext> BuildMultipleTagDirectiveEvaluationDelegate(
            Directive directive,
            CompiledEvaluationDelegate<EvaluationContext>[] innerDelegates,
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
                catch (OperationCanceledException)
                {
                    throw;
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

        /// <summary>
        /// Constructs an un-parsed text evaluation delegate.
        /// </summary>
        /// <param name="unParsedText">The un-parsed text.</param>
        /// <returns>A new delegate that handles the given text.</returns>
        protected override CompiledEvaluationDelegate<EvaluationContext> BuildUnParsedTextEvaluationDelegate([NotNull] string unParsedText)
        {
            Debug.Assert(!string.IsNullOrEmpty(unParsedText), "Argument unParsedText cannot be empty.");

            return (writer, context) =>
            {
                Debug.Assert(writer != null, "Argument writer cannot be null.");
                Debug.Assert(context != null, "Argument context cannot be null.");

                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                var text = context.ProcessUnParsedText(unParsedText);

                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                if (!string.IsNullOrEmpty(text))
                {
                    writer.Write(text);
                }
            };
        }

        /// <summary>
        /// Constructs a delegate used in evaluation of a given single-tag <paramref name="directive"/>.
        /// </summary>
        /// <param name="directive">The directive.</param>
        /// <param name="components">The components associated with directive's tag.</param>
        /// <returns>A new delegate tasked with evaluating the given directive.</returns>
        protected override CompiledEvaluationDelegate<EvaluationContext> BuildSingleTagDirectiveEvaluationDelegate(
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
                catch (OperationCanceledException)
                {
                    throw;
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

        [NotNull]
        private static object[] EvaluateTag([NotNull] IExpressionEvaluationContext context, [NotNull] IReadOnlyList<object> components)
        {
            var result = new object[components.Count];
            for (var i = 0; i < components.Count; i++)
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
            [NotNull] TextWriter writer,
            [NotNull] IExpressionEvaluationContext context,
            [NotNull] Directive directive,
            [NotNull] IReadOnlyList<object> components)
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
            [NotNull] TextWriter writer,
            [NotNull] EvaluationContext context,
            [NotNull] Directive directive,
            [NotNull] IReadOnlyList<object> beginComponents,
            [NotNull] IReadOnlyList<object> closeComponents,
            [CanBeNull] CompiledEvaluationDelegate<EvaluationContext> beginToCloseEvaluateProc)
        {
            /* Get tag components. */
            var beginTagEvaluatedComponents = EvaluateTag(context, beginComponents);
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

                switch (flowDecision)
                {
                    case Directive.FlowDecision.Restart:
                        continue;
                    case Directive.FlowDecision.Evaluate:
                        beginToCloseEvaluateProc?.Invoke(writer, context);
                        break;
                    case Directive.FlowDecision.Terminate:
                        break;
                    case Directive.FlowDecision.Skip:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException(nameof(flowDecision));
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
            [NotNull] TextWriter writer,
            [NotNull] EvaluationContext context,
            [NotNull] Directive directive,
            [NotNull] IReadOnlyList<object[]> components,
            [NotNull] IReadOnlyList<CompiledEvaluationDelegate<EvaluationContext>> evaluationProcs)
        {
            /* Get tag components. */
            var evaluatedComponents = new object[components.Count][];

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

                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (flowDecision)
                {
                    case Directive.FlowDecision.Terminate:
                        currentTagIndex = -1;
                        break;
                    case Directive.FlowDecision.Restart:
                        currentTagIndex = 0;
                        break;
                    default:
                        currentTagIndex++;
                        if (currentTagIndex == components.Count)
                        {
                            currentTagIndex = 0;
                        }
                        else if (flowDecision == Directive.FlowDecision.Evaluate)
                        {
                            evaluationProcs[currentTagIndex - 1]?.Invoke(writer, context);
                        }
                        break;
                }
            }
        }
    }
}