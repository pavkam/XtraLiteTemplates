using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Tom
{
    internal sealed class TomDocumentBuilder
    {
        public DocumentTomNode Document { get; private set; }
        private CompositeTomNode m_currentComposite;

        public TomDocumentBuilder()
        {
            Document = new DocumentTomNode();
            m_currentComposite = Document;
        }

        public TomNode AddPlainText(String plainText)
        {
            var child = new PlainTextTomNode(m_currentComposite, plainText);
            m_currentComposite.AddChild(child);

            return child;
        }
    }
}
