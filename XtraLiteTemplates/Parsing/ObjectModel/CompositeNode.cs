using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Evaluation;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Parsing.ObjectModel
{
    public abstract class CompositeNode : TemplateNode
    {
        private List<TemplateNode> m_children;

        public IReadOnlyCollection<TemplateNode> Children
        {
            get
            {
                return m_children;
            }
        }

        internal CompositeNode(CompositeNode parent)
            : base(parent)
        {
        }

        internal void AddChild(TemplateNode child)
        {
            ValidationHelper.AssertArgumentIsNotNull("child", child);
            ValidationHelper.Assert("child", "not self", child != this);

            m_children.Add(child);
        }
    }
}
