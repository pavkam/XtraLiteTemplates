using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Tom;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Interpretation
{
    public class InterpretationContext
    {
        public DocumentTomNode Document { get; private set; }

        public InterpretationContext(DocumentTomNode document)
        {
            ValidationHelper.AssertArgumentIsNotNull("document", document);

            Document = document;
        }
    }
}
