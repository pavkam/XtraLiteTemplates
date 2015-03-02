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
    public sealed class TemplateDocument : CompositeNode
    {
        public TemplateDocument()
            : base(null)
        {
        }

        public override Int32 Evaluate(TextWriter writer, IEvaluationContext evaluationContext)
        {
            ValidationHelper.AssertArgumentIsNotNull("writer", writer);
            ValidationHelper.AssertArgumentIsNotNull("evaluationContext", evaluationContext);

            Int32 characterCount = 0;
            foreach(var child in Children)
                characterCount += child.Evaluate(writer, evaluationContext);

            return characterCount;
        }
    }
}
