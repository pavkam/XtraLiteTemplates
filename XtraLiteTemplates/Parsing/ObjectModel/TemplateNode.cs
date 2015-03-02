using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Evaluation;

namespace XtraLiteTemplates.Parsing.ObjectModel
{
    public abstract class TemplateNode : IEvaluable
    {
        public CompositeNode Parent { get; private set; }

        internal TemplateNode(CompositeNode parent)
        {
            Parent = parent;
        }

        public abstract Int32 Evaluate(TextWriter writer, IEvaluationContext evaluationContext);
    }
}
