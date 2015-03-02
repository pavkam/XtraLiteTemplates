using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Evaluation
{
    public interface IEvaluable
    {
        Boolean Evaluate(TextWriter writer, IEvaluationContext context, ICollection<IEvaluable> children);
    }
}
