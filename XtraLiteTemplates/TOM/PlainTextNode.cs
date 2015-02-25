using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.TOM
{
    public sealed class PlainTextNode : TomNode
    {
        public String Value { get; private set; }

        public PlainTextNode(TomNode parent, String value)
            : base(parent)
        {
            Debug.Assert(parent != null);
            Debug.Assert(value != null);

            Value = value;
        }
    }
}
