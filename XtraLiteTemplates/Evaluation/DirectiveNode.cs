﻿//
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
//     * Neither the name of the [ORGANIZATION] nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
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
//

namespace XtraLiteTemplates.Evaluation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Evaluation.Directives;
    using XtraLiteTemplates.Parsing;

    internal sealed class DirectiveNode : CompositeNode, IEvaluable
    {
        private Directive[] m_directives;

        public Directive[] CandidateDirectives
        {
            get
            {
                return m_directives;
            }
        }

        public DirectiveNode(TemplateNode parent, Directive[] candidateDirectives)
            : base(parent)
        {
            Debug.Assert(parent != null);
            Debug.Assert(candidateDirectives != null);
            Debug.Assert(candidateDirectives.Length > 0);

            m_directives = candidateDirectives;
        }

        public override void Evaluate(TextWriter writer, IEvaluationContext context)
        {
            Debug.Assert(writer != null);
            Debug.Assert(context != null);
            Debug.Assert(Children.Count > 0);
            Debug.Assert(m_directives.Length == 1);

            context.OpenEvaluationFrame();

            var directiveComponentIndex = 0;
            var tagIndex = 0;
            var tagNode = Children[directiveComponentIndex] as TagNode;
            Debug.Assert(tagNode != null);

            Object state = null;
            while (tagNode != null)
            {
                var tagComponents = tagNode.Evaluate(context);
                String text;
                Directive.FlowDecision flow;

                try
                {
                    flow = m_directives[0].Execute(tagIndex, tagComponents, ref state, context, out text);
                }
                catch (Exception exception)
                {
                    text = null;
                    flow = Directive.FlowDecision.Terminate;

                    if (!context.IgnoreEvaluationExceptions)
                        ExceptionHelper.DirectiveEvaluationError(m_directives[0], exception);
                }

                /* Return any text that was generated by the edirective itself. */
                if (text != null)
                    writer.Write(text);

                if (flow == Directive.FlowDecision.Terminate)
                    tagNode = null;
                else if (flow == Directive.FlowDecision.Restart)
                {
                    directiveComponentIndex = 0;
                    tagIndex = 0;
                    tagNode = Children[directiveComponentIndex] as TagNode;
                }
                else if (flow == Directive.FlowDecision.Evaluate)
                {
                    /* Evaluate inner nodes */
                    tagNode = null;
                    for (var i = directiveComponentIndex + 1; i < Children.Count; i++)
                    {
                        tagNode = Children[i] as TagNode;
                        if (tagNode != null)
                        {
                            directiveComponentIndex = i;
                            tagIndex++;
                            break;
                        }

                        var evaluable = Children[i] as IEvaluable;
                        Debug.Assert(evaluable != null);

                        evaluable.Evaluate(writer, context);
                    }
                }
                else if (flow == Directive.FlowDecision.Skip)
                {
                    /* Evaluate inner nodes */
                    tagNode = null;
                    for (var i = directiveComponentIndex + 1; i < Children.Count; i++)
                    {
                        tagNode = Children[i] as TagNode;
                        if (tagNode != null)
                        {
                            tagIndex++;
                            directiveComponentIndex = i;
                            break;
                        }
                    }
                }
            }

            context.CloseEvaluationFrame();
        }

        public Boolean SelectDirective(Int32 presenceIndex, Tag tag, IEqualityComparer<String> comparer)
        {
            Debug.Assert(presenceIndex >= 0);
            Debug.Assert(tag != null);
            Debug.Assert(comparer != null);
            Debug.Assert(!CandidateDirectiveLockedIn);

            var options = m_directives.Where(d => d.Tags.Count > presenceIndex && d.Tags[presenceIndex].Equals(tag, comparer)).ToArray();
            if (options.Length > 0)
            {
                var firstFullySelected = options.FirstOrDefault(o => o.Tags.Count == presenceIndex + 1);

                if (firstFullySelected != null)
                {
                    m_directives = new Directive[] { firstFullySelected };
                    CandidateDirectiveLockedIn = true;
                }
                else
                    m_directives = options;
            }

            return options.Length > 0;
        }

        public Boolean CandidateDirectiveLockedIn { get; private set; }
    }
}

