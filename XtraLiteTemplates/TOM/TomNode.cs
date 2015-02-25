using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.TOM
{
    public abstract class TomNode
    {
        public TomNode Parent { get; private set; }

        internal TomNode(TomNode parent)
        {
            Parent = parent;
        }
    }
}
