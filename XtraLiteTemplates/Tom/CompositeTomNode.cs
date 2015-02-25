using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Tom
{
    public abstract class CompositeTomNode : TomNode
    {
        private List<TomNode> m_children;

        public CompositeTomNode(TomNode parent)
            : base(parent)
        {
        }

        internal void AddChild(TomNode child)
        {
            ValidationHelper.AssertArgumentIsNotNull("child", child);
            ValidationHelper.Assert("child", "not self", child != this);

            m_children.Add(child);
        }
    }
}
