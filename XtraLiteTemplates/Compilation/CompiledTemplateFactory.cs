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

    /// <summary>
    /// Class that encapsulates <c>compilation</c> logic. The sole purpose of <see cref="CompiledTemplateFactory"/> is to
    /// transform the internal <c>lexed</c> representation of a template to its compiled form (form that can be evaluated).
    /// </summary>
    public class CompiledTemplateFactory
    {
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        internal CompiledEvaluationDelegate CompileTemplate(TemplateDocument document)
        {
            Debug.Assert(document != null, "Argument document cannot be null.");

            /* Construct the combined node for all the children. */
            return this.CompileTemplateNodes(document.Children);
        }

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
        protected virtual CompiledEvaluationDelegate BuildDoubleTagDirectiveEvaluationDelegate(
            Directive directive,
            CompiledEvaluationDelegate innerDelegate,
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
        protected virtual CompiledEvaluationDelegate BuildMultipleTagDirectiveEvaluationDelegate(
            Directive directive,
            CompiledEvaluationDelegate[] innerDelegates,
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

        /// <summary>
        /// Constructs an unparsed text evaluation delegate.
        /// </summary>
        /// <param name="unparsedText">The unparsed text.</param>
        /// <returns>A new delegate that handles the given text.</returns>
        protected virtual CompiledEvaluationDelegate BuildUnparsedTextEvaluationDelegate(string unparsedText)
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

        /// <summary>
        /// Combines a sequence of <see cref="CompiledEvaluationDelegate"/> delegates into a single evaluation delegate.
        /// </summary>
        /// <remarks>
        /// The implementer is required to generate a new <see cref="CompiledEvaluationDelegate"/> that sequentially calls the delegates
        /// in the provided list.
        /// </remarks>
        /// <param name="delegates">The delegates to be merged.</param>
        /// <returns>A new merged delegate.</returns>
        protected virtual CompiledEvaluationDelegate BuildMergedEvaluationDelegate(IReadOnlyList<CompiledEvaluationDelegate> delegates)
        {
            Debug.Assert(delegates != null, "Argument delegates cannot be null.");
            Debug.Assert(delegates.Count > 0, "Argument delegates cannot be empty.");

            CompiledEvaluationDelegate resultingDelegate;
            switch (delegates.Count)
            {
                case 1:
                    resultingDelegate = delegates[0];
                    break;
                case 2:
                    resultingDelegate = (writer, context) =>
                    {
                        delegates[0](writer, context);
                        delegates[1](writer, context);
                    };
                    break;
                case 3:
                    resultingDelegate = (writer, context) =>
                    {
                        delegates[0](writer, context);
                        delegates[1](writer, context);
                        delegates[3](writer, context);
                    };
                    break;
                default:
                    var arrayOfEvalutionProcs = delegates.ToArray();
                    resultingDelegate = (writer, context) =>
                    {
                        foreach (var proc in arrayOfEvalutionProcs)
                        {
                            proc(writer, context);
                        }
                    };
                    break;
            }

            return resultingDelegate;
        }

        /// <summary>
        /// Constructs a delegate used in evaluation of a given single-tag <paramref name="directive"/>.
        /// </summary>
        /// <param name="directive">The directive.</param>
        /// <param name="components">The components associated with directive's tag.</param>
        /// <returns>A new delegate tasked with evaluating the given directive.</returns>
        protected virtual CompiledEvaluationDelegate BuildSingleTagDirectiveEvaluationDelegate(
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

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private static object[] EvaluateTag(IEvaluationContext context, object[] components)
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

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private static void EvaluateSingleTagDirective(
            TextWriter writer,
            IEvaluationContext context,
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

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private static void EvaluateDoubleTagDirective(
            TextWriter writer,
            IEvaluationContext context,
            Directive directive,
            object[] beginComponents,
            object[] closeComponents,
            CompiledEvaluationDelegate beginToCloseEvaluateProc)
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

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private static void EvaluateTagDirective(
            TextWriter writer,
            IEvaluationContext context,
            Directive directive,
            object[][] components,
            CompiledEvaluationDelegate[] evaluationProcs)
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

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private CompiledEvaluationDelegate CompileUnparsedNode(UnparsedNode unparsedNode)
        {
            Debug.Assert(unparsedNode != null, "Argument unparsedNode cannot be null.");

            return this.BuildUnparsedTextEvaluationDelegate(unparsedNode.UnparsedText);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private CompiledEvaluationDelegate CompileDirectiveNode(DirectiveNode directiveNode)
        {
            Debug.Assert(directiveNode != null, "Argument directiveNode cannot be null.");
            Debug.Assert(directiveNode.CandidateDirectiveLockedIn, "Argument directiveNode must have a locked in candidate.");

            var directive = directiveNode.CandidateDirectives[0];

            /* Build inter-tag evaluables. */
            var innerDelegates = new List<CompiledEvaluationDelegate>();
            var innerTagNodes = new List<TemplateNode>();
            for (var index = 0; index < directiveNode.Children.Count; index++)
            {
                if (directiveNode.Children[index] is TagNode)
                {
                    if (innerTagNodes.Count > 0)
                    {
                        innerDelegates.Add(this.CompileTemplateNodes(innerTagNodes));
                        innerTagNodes.Clear();
                    }
                    else if (index > 0)
                    {
                        innerDelegates.Add(null);
                    }
                }
                else
                {
                    innerTagNodes.Add(directiveNode.Children[index]);
                }
            }

            Debug.Assert(innerTagNodes.Count == 0, "No post-tag node can exist in directive node.");

            if (directive.Tags.Count == 1)
            {
                var tagNode = directiveNode.Children[0] as TagNode;
                Debug.Assert(tagNode != null, "The single directive node expected to be a tag node.");

                return this.BuildSingleTagDirectiveEvaluationDelegate(directive, tagNode.Components);
            }
            else if (directive.Tags.Count == 2)
            {
                var beginTagNode = directiveNode.Children[0] as TagNode;
                var endTagNode = directiveNode.Children[directiveNode.Children.Count - 1] as TagNode;

                Debug.Assert(beginTagNode != null, "The first directive node expected to be a tag node.");
                Debug.Assert(endTagNode != null, "The last directive node expected to be a tag node.");

                return this.BuildDoubleTagDirectiveEvaluationDelegate(directive, innerDelegates[0], beginTagNode.Components, endTagNode.Components);
            }
            else
            {
                var componentArray = directiveNode.Children.Where(p => p is TagNode).Select(s => ((TagNode)s).Components).ToArray();

                return this.BuildMultipleTagDirectiveEvaluationDelegate(directive, innerDelegates.ToArray(), componentArray);
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private CompiledEvaluationDelegate CompileTemplateNodes(IReadOnlyList<TemplateNode> nodes)
        {
            Debug.Assert(nodes != null, "Argument nodes cannot be null.");
            Debug.Assert(nodes.Count > 0, "Argument nodes cannot be empty.");

            List<CompiledEvaluationDelegate> delegates = new List<CompiledEvaluationDelegate>();
            foreach (var node in nodes)
            {
                var directiveNode = node as DirectiveNode;
                if (directiveNode != null)
                {
                    delegates.Add(this.CompileDirectiveNode(directiveNode));
                }
                else
                {
                    var unparsedNode = node as UnparsedNode;

                    Debug.Assert(unparsedNode != null, "Node can only be unparsed at this stage.");
                    delegates.Add(this.CompileUnparsedNode(unparsedNode));
                }
            }

            Debug.Assert(delegates.Count > 0, "The list of compiled delegates must contain at least one item.");

            /* Merge */
            return this.BuildMergedEvaluationDelegate(delegates);
        }
    }
}