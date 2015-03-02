using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Parsing.ObjectModel
{
    public sealed class TemplateDocument : CompositeNode
    {
        private Boolean m_isFinalized;
        private CompositeNode m_currentParentNode;

        public TemplateDocument()
            : base(null)
        {
            m_currentParentNode = this;
        }

        internal TomDocumentBuilder(Arbiter arbiter)
        {
            Debug.Assert(arbiter != null);

            Arbiter = arbiter;
            m_currentComposite = new TemplateDocument();
        }

        internal PlainTextTomNode AddPlainText(String plainText)
        {
            Debug.Assert(!String.IsNullOrEmpty(plainText));

            var child = new PlainTextTomNode(m_currentComposite, plainText);
            m_currentComposite.AddChild(child);

            return child;
        }

        internal SimpleDirectiveNode AddDirective(IReadOnlyList<DirectiveToken> directiveTokens)
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
                SimpleDirectiveNode node;
                if (matchResult == DirectiveMatchResult.MatchingFaceDirective)
                {
                    node = new SimpleDirectiveNode(m_currentComposite, selectedDirective);
                    if (node.Directive.IsComposite)
                        m_currentComposite = node;
                }
                else
                {
                    Debug.Assert(m_currentComposite is SimpleDirectiveNode);

                    node = (m_currentComposite as SimpleDirectiveNode);
                    var directive = node.Directive;
                    if (directive == selectedDirective)
                    {
                        m_currentComposite = m_currentComposite.Parent as CompositeNode;
                    }
                    else
                        throw new InvalidOperationException("Closing an unrelated node. Not matching the current open one.");
                }

                return node;
            }
        }

        internal TemplateDocument GetDocument()
        {
            if (m_currentComposite is TemplateDocument)
                return m_currentComposite as TemplateDocument;
            else
                throw new InvalidOperationException("Not all blocks closed. Fix me,");
        }

        internal void FinalizeBuilding()
        {
            if (m_isFinalized)
                throw new InvalidOperationException("document already finalized.");

            m_isFinalized = true;
        }
    }
}
