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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    using JetBrains.Annotations;

    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;

    /// <summary>
    /// Class that encapsulates <c>compilation</c> logic. The sole purpose of <see cref="CompiledTemplateFactory{TContext}"/> is to
    /// transform the internal <c>lexed</c> representation of a template to its compiled form (form that can be evaluated).
    /// </summary>
    /// <typeparam name="TContext">Any class that implements <see cref="IExpressionEvaluationContext"/> interface.</typeparam>
    [PublicAPI]
    public abstract class CompiledTemplateFactory<TContext> 
        where TContext : IExpressionEvaluationContext
    {
        [NotNull]
        private static readonly CompiledEvaluationDelegate<TContext> EmptyCompiledEvaluationDelegate = (writer, context) => { };

        internal CompiledEvaluationDelegate<TContext> CompileTemplate([NotNull] TemplateDocument document)
        {
            Debug.Assert(document != null, "Argument document cannot be null.");

            /* Construct the combined node for all the children. */
            return CompileTemplateNodes(document.Children);
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
        [NotNull]
        protected abstract CompiledEvaluationDelegate<TContext> BuildDoubleTagDirectiveEvaluationDelegate(
            [NotNull] Directive directive,
            [NotNull] CompiledEvaluationDelegate<TContext> innerDelegate,
            [NotNull] object[] openComponents,
            [NotNull] object[] closeComponents);

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
        [NotNull]
        protected abstract CompiledEvaluationDelegate<TContext> BuildMultipleTagDirectiveEvaluationDelegate(
            [NotNull] Directive directive,
            [NotNull] CompiledEvaluationDelegate<TContext>[] innerDelegates,
            [NotNull] object[][] components);

        /// <summary>
        /// Constructs an un-parsed text evaluation delegate.
        /// </summary>
        /// <param name="unParsedText">The un-parsed text.</param>
        /// <returns>A new delegate that handles the given text.</returns>
        [NotNull]
        protected abstract CompiledEvaluationDelegate<TContext> BuildUnParsedTextEvaluationDelegate(string unParsedText);

        /// <summary>
        /// Combines a sequence of <see cref="CompiledEvaluationDelegate{TContext}"/> delegates into a single evaluation delegate.
        /// </summary>
        /// <remarks>
        /// The implementer is required to generate a new <see cref="CompiledEvaluationDelegate{TContext}"/> that sequentially calls the delegates
        /// in the provided list.
        /// </remarks>
        /// <param name="delegates">The delegates to be merged.</param>
        /// <returns>A new merged delegate.</returns>
        [NotNull]
        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
        protected virtual CompiledEvaluationDelegate<TContext> BuildMergedEvaluationDelegate([NotNull] CompiledEvaluationDelegate<TContext>[] delegates)
        {
            Debug.Assert(delegates != null, "Argument delegates cannot be null.");
            Debug.Assert(delegates.Length > 0, "Argument delegates cannot be empty.");

            CompiledEvaluationDelegate<TContext> resultingDelegate;
            switch (delegates.Length)
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
                        delegates[2](writer, context);
                    };
                    break;
                default:
                    var arrayOfEvaluationMethods = delegates.ToArray();
                    resultingDelegate = (writer, context) =>
                    {
                        foreach (var method in arrayOfEvaluationMethods)
                        {
                            method(writer, context);
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
        [NotNull]
        protected abstract CompiledEvaluationDelegate<TContext> BuildSingleTagDirectiveEvaluationDelegate(
            [NotNull] Directive directive,
            [NotNull] object[] components);

        [NotNull]
        private CompiledEvaluationDelegate<TContext> CompileUnParsedNode([NotNull] UnParsedNode unParsedNode)
        {
            Debug.Assert(unParsedNode != null, "Argument unParsedNode cannot be null.");

            return BuildUnParsedTextEvaluationDelegate(unParsedNode.UnParsedText);
        }

        [NotNull]
        private CompiledEvaluationDelegate<TContext> CompileDirectiveNode([NotNull] DirectiveNode directiveNode)
        {
            Debug.Assert(directiveNode != null, "Argument directiveNode cannot be null.");
            Debug.Assert(directiveNode.CandidateDirectiveLockedIn, "Argument directiveNode must have a locked in candidate.");

            var directive = directiveNode.CandidateDirectives[0];

            /* Build inter-tag evaluable parts. */
            var innerDelegates = new List<CompiledEvaluationDelegate<TContext>>();
            var innerTagNodes = new List<TemplateNode>();
            for (var index = 0; index < directiveNode.Children.Count; index++)
            {
                if (directiveNode.Children[index] is TagNode)
                {
                    if (innerTagNodes.Count > 0)
                    {
                        innerDelegates.Add(CompileTemplateNodes(innerTagNodes));
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

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (directive.Tags.Count)
            {
                case 1:
                    var tagNode = directiveNode.Children[0] as TagNode;
                    Debug.Assert(tagNode != null, "The single directive node expected to be a tag node.");

                    return BuildSingleTagDirectiveEvaluationDelegate(directive, tagNode.Components);
                case 2:
                    var beginTagNode = directiveNode.Children[0] as TagNode;
                    var endTagNode = directiveNode.Children[directiveNode.Children.Count - 1] as TagNode;

                    Debug.Assert(beginTagNode != null, "The first directive node expected to be a tag node.");
                    Debug.Assert(endTagNode != null, "The last directive node expected to be a tag node.");

                    return BuildDoubleTagDirectiveEvaluationDelegate(directive, innerDelegates[0], beginTagNode.Components, endTagNode.Components);
            }

            var componentArray = directiveNode.Children.Where(p => p is TagNode).Select(s => ((TagNode)s).Components).ToArray();

            return BuildMultipleTagDirectiveEvaluationDelegate(directive, innerDelegates.ToArray(), componentArray);
        }

        [NotNull]
        private CompiledEvaluationDelegate<TContext> CompileTemplateNodes([NotNull] IReadOnlyCollection<TemplateNode> nodes)
        {
            Debug.Assert(nodes != null, "Argument nodes cannot be null.");

            if (nodes.Count == 0)
            {
                /* For empty templates simply return a "null" delegate */
                return EmptyCompiledEvaluationDelegate;
            }

            var delegates = new List<CompiledEvaluationDelegate<TContext>>();
            foreach (var node in nodes)
            {
                var directiveNode = node as DirectiveNode;
                if (directiveNode != null)
                {
                    delegates.Add(CompileDirectiveNode(directiveNode));
                }
                else
                {
                    var unParsedNode = node as UnParsedNode;

                    Debug.Assert(unParsedNode != null, "Node can only be un-parsed at this stage.");
                    delegates.Add(CompileUnParsedNode(unParsedNode));
                }
            }

            Debug.Assert(delegates.Count > 0, "The list of compiled delegates must contain at least one item.");

            /* Merge */
            return BuildMergedEvaluationDelegate(delegates.ToArray());
        }
    }
}