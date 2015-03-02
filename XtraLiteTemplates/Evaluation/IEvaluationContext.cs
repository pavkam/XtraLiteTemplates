using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Evaluation
{
    public interface IEvaluationContext
    {
        void InitiateNewVariableScope();
        void CloseCurrentVariableScope();

        void AssignVariable(String name, Object value);
    }
}
