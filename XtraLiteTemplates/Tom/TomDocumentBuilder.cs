﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Directives;
using XtraLiteTemplates.Parsing;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Tom
{
    internal sealed class TomDocumentBuilder
    {
        private CompositeTomNode m_currentComposite;

        internal Arbiter Arbiter { get; private set; }

        internal TomDocumentBuilder(Arbiter arbiter)
        {
            Debug.Assert(arbiter != null);

            Arbiter = arbiter;
            m_currentComposite = new DocumentTomNode();
        }

        internal PlainTextTomNode AddPlainText(String plainText)
        {
            Debug.Assert(!String.IsNullOrEmpty(plainText));

            var child = new PlainTextTomNode(m_currentComposite, plainText);
            m_currentComposite.AddChild(child);

            return child;
        }

        internal DirectiveTomNode AddDirective(IReadOnlyList<DirectiveToken> directiveTokens)
        {
            Debug.Assert(directiveTokens != null);
            Debug.Assert(directiveTokens.Count > 0);
            Debug.Assert(directiveTokens.All(s => s != null));

            Directive selectedDirective;
            var matchResult = Arbiter.TryMatch(directiveTokens, out selectedDirective);

            if (matchResult == DirectiveMatchResult.NoMatches)
                throw new InvalidOperationException("Could not select directive?");
            else if (matchResult == DirectiveMatchResult.AmbiguousMatches)
                throw new InvalidOperationException("Could not select directive - too many?"); 
            else                
            {
                DirectiveTomNode node;
                if (matchResult == DirectiveMatchResult.MatchingFaceDirective)
                {
                    node = new DirectiveTomNode(m_currentComposite, selectedDirective);
                    if (node.Directive.IsComposite)
                        m_currentComposite = node;
                }
                else
                {
                    Debug.Assert(m_currentComposite is DirectiveTomNode);

                    node = (m_currentComposite as DirectiveTomNode);
                    var directive = node.Directive;
                    if (directive == selectedDirective)
                    {
                        m_currentComposite = m_currentComposite.Parent as CompositeTomNode;
                    }
                    else
                        throw new InvalidOperationException("Closing an unrelated node. Not matching the current open one.");
                }

                return node;
            }
        }

        internal DocumentTomNode GetDocument()
        {
            if (m_currentComposite is DocumentTomNode)
                return m_currentComposite as DocumentTomNode;
            else
                throw new InvalidOperationException("Not all blocks closed. Fix me,");
        }
    }
}
