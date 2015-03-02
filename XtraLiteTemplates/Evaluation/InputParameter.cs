using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Evaluation
{
    public sealed class InputParameter
    {
        public InputParameterType Type { get; private set; }
        public String Key { get; private set; }
        public String Value { get; private set; }

        internal InputParameter(InputParameterType type, String key, String value)
        {
            Key = key;
            Type = type;
            Value = value;
        }
    }
}
