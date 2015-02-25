using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.TOM
{
    public abstract class CompositeNode : TomNode
    {
        private List<TomNode> m_children;

        internal CompositeNode(TomNode parent)
            : base(parent)
        {
            m_children = new List<TomNode>();
        }

        public IReadOnlyList<TomNode> Children
        {
            get
            {
                return m_children;
            }
        }

        internal void AddChild(TomNode child)
        {
            Debug.Assert(child != null);
            Debug.Assert(child.Parent == this);

            m_children.Add(child);
        }
    }
}
