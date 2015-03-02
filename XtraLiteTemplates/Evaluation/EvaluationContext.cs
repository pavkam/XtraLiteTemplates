using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Evaluation
{
    public sealed class EvaluationContext : IEvaluationContext
    {
        private VariableCollection m_variables;

        public EvaluationContext(IEqualityComparer<String> comparer)
        {
            ValidationHelper.AssertArgumentIsNotNull("comparer", comparer);
            m_variables = new VariableCollection(comparer);
        }

        public EvaluationContext()
            : this(StringComparer.Ordinal)
        {
        }

        public void RegisterVariable(String name, Object value, Object context)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("name", name);

            m_variables.Set(name, value, context);
        }

        public void RegisterVariable(String name, Object value)
        {
            RegisterVariable(name, value, null);
        }


    }
}
