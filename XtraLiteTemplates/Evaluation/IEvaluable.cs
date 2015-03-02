using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Evaluation;

namespace XtraLiteTemplates.Parsing.ObjectModel
{
    public interface IEvaluable
    {
        Int32 Evaluate(TextWriter writer, IEvaluationContext evaluationContext);
    }
}
