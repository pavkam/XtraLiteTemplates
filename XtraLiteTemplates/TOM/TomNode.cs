using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Tom
{
    public abstract class TomNode
    {
        public TomNode Parent { get; private set; }

        public TomNode(TomNode parent)
        {
            Parent = parent;
        }
    }
}
