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

namespace XtraLiteTemplates.Evaluation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using XtraLiteTemplates.Expressions;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal sealed class ContextualEvaluable : IEvaluable
    {
        private TemplateDocument templateDocument;
        private TemplateNodeEvaluatorDelegate evaluationFunction;

        public ContextualEvaluable(TemplateDocument document)
        {
            Debug.Assert(document != null, "document cannot be null.");

            this.templateDocument = document;
            this.evaluationFunction = ConstructMultiple(document.Children);
        }

        private delegate void TemplateNodeEvaluatorDelegate(TextWriter writer, IEvaluationContext context);

        public void Evaluate(TextWriter writer, IEvaluationContext context)
        {
            this.evaluationFunction(writer, context);
        }

        public override string ToString()
        {
            return this.templateDocument.ToString();
        }

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

        private static void EvaluateDoubleTagDirective(
            TextWriter writer,
            IEvaluationContext context,
            Directive directive,
            object[] beginComponents,
            object[] closeComponents,
            TemplateNodeEvaluatorDelegate beginToCloseEvaluateProc)
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
            IEvaluationContext context,
            Directive directive,
            object[][] components,
            TemplateNodeEvaluatorDelegate[] evaluationProcs)
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

        private static TemplateNodeEvaluatorDelegate ConstructUnparsed(UnparsedNode unparsedNode)
        {
            Debug.Assert(unparsedNode != null, "unparsedNode cannot be null.");

            return (writer, context) =>
            {
                Debug.Assert(writer != null, "writer cannot be null.");
                Debug.Assert(context != null, "context cannot be null.");

                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                var text = context.ProcessUnparsedText(unparsedNode.UnparsedText);

                /* Check for cancellation. */
                context.CancellationToken.ThrowIfCancellationRequested();

                if (!string.IsNullOrEmpty(text))
                {
                    writer.Write(text);
                }
            };
        }

        private static TemplateNodeEvaluatorDelegate ConstructMultiple(IReadOnlyList<TemplateNode> nodes)
        {
            List<TemplateNodeEvaluatorDelegate> eachNodeProc = new List<TemplateNodeEvaluatorDelegate>();
            foreach (var node in nodes)
            {
                var directiveNode = node as DirectiveNode;
                if (directiveNode != null)
                {
                    eachNodeProc.Add(ContructDirective(directiveNode));
                }
                else
                {
                    var unparsedNode = node as UnparsedNode;

                    Debug.Assert(unparsedNode != null, "Must be unparsed.");
                    eachNodeProc.Add(ConstructUnparsed(unparsedNode));
                }
            }

            if (eachNodeProc.Count == 0)
            {
                return null;
            }
            else if (eachNodeProc.Count == 1)
            {
                return eachNodeProc[0];
            }
            else if (eachNodeProc.Count == 2)
            {
                return (writer, context) =>
                {
                    eachNodeProc[0](writer, context);
                    eachNodeProc[1](writer, context);
                };
            }
            else
            {
                var arrayOfEvalutionProcs = eachNodeProc.ToArray();
                return (writer, context) =>
                {
                    foreach (var proc in arrayOfEvalutionProcs)
                    {
                        proc(writer, context);
                    }
                };
            }
        }

        private static TemplateNodeEvaluatorDelegate ContructDirective(DirectiveNode directiveNode)
        {
            Debug.Assert(directiveNode != null, "directiveNode cannot be null.");
            Debug.Assert(directiveNode.CandidateDirectiveLockedIn, "directiveNode must have a locked in candidate.");

            var directive = directiveNode.CandidateDirectives[0];

            /* Build inter-tag evaluables. */
            var interTagEvaluables = new List<TemplateNodeEvaluatorDelegate>();
            var interTagNodes = new List<TemplateNode>();
            for (var index = 0; index < directiveNode.Children.Count; index++)
            {
                if (directiveNode.Children[index] is TagNode)
                {
                    if (interTagNodes.Count > 0)
                    {
                        interTagEvaluables.Add(ConstructMultiple(interTagNodes));
                        interTagNodes.Clear();
                    }
                    else if (index > 0)
                    {
                        interTagEvaluables.Add(null);
                    }
                }
                else
                {
                    interTagNodes.Add(directiveNode.Children[index]);
                }
            }

            Debug.Assert(interTagNodes.Count == 0, "no post-tag node can exist in directive node.");

            if (directive.Tags.Count == 1)
            {
                var tagNode = directiveNode.Children[0] as TagNode;
                Debug.Assert(tagNode != null, "the single directive node expected to be a tag node.");

                return (writer, context) =>
                {
                    try
                    {
                        EvaluateSingleTagDirective(writer, context, directive, tagNode.Components);
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
            else if (directive.Tags.Count == 2)
            {
                var beginTagNode = directiveNode.Children[0] as TagNode;
                var endTagNode = directiveNode.Children[directiveNode.Children.Count - 1] as TagNode;

                Debug.Assert(beginTagNode != null, "the first directive node expected to be a tag node.");
                Debug.Assert(endTagNode != null, "the last directive node expected to be a tag node.");

                var evaluable = interTagEvaluables[0];
                return (writer, context) =>
                {
                    context.OpenEvaluationFrame();
                    try
                    {
                        EvaluateDoubleTagDirective(
                            writer,
                            context,
                            directive,
                            beginTagNode.Components,
                            endTagNode.Components,
                            evaluable);
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
            else
            {
                var componentArray = directiveNode.Children.Where(p => p is TagNode).Select(s => ((TagNode)s).Components).ToArray();

                return (writer, context) =>
                {
                    context.OpenEvaluationFrame();
                    try
                    {
                        EvaluateTagDirective(
                            writer,
                            context,
                            directive,
                            componentArray,
                            interTagEvaluables.ToArray());
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
        }
    }
}