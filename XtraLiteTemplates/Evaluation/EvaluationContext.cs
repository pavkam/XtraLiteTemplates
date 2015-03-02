using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Tom;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Evaluation
{
    public class EvaluationContext : IEvaluationContext
    {
        private BranchableVariableContext m_variableContext;
        public TomNode m_current { get; private set; }

        public DocumentTomNode Document { get; private set; }
        public Boolean CaseSensitive { get; private set; }

        public EvaluationContext(DocumentTomNode document, Boolean caseSensitive)
        {
            ValidationHelper.AssertArgumentIsNotNull("document", document);

            CaseSensitive = caseSensitive;
            Document = document;
            m_current = document;

            m_variableContext = new BranchableVariableContext(caseSensitive);
        }

        public void AssignVariable(String name, Object value)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("name", name);

            m_variableContext[name] = value;
        }

        public void EvaluateChildren(TextWriter writer)
        {
            ValidationHelper.AssertArgumentIsNotNull("writer", writer);

            /* Iterate on all the children of the "current" node */
            foreach (var child in m_current.Children)
            {
                if (child is PlainTextTomNode)
                    writer.Write((child as PlainTextTomNode).PlainText);
                else
                {
                    /* Save the actual */
                    var actual = m_current;
                    m_current = child;

                    /* Recurse. */
                    EvaluateChildren(writer);

                    /* Restore actual. */
                    m_current = actual;
                }
            }
        }
    }
}
