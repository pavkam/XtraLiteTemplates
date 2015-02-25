using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Tom
{
    internal sealed class PlainTextTomNode : TomNode
    {
        public String PlainText { get; private set; }

        public PlainTextTomNode(TomNode parent, String plainText)
            : base(parent)
        {
            PlainText = plainText;
        }
    }
}
