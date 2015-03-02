using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Evaluation
{
    public interface IEvaluationContext
    {
        void RegisterVariable(String name, Object value, Object context);
        void RegisterVariable(String name, Object value);

        void OpenVariableScope();
        void CloseVariableScope();
    }
}
