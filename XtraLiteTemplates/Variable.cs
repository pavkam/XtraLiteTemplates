using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates
{
    public sealed class Variable
    {
        public String Name { get; private set; }
        public Object Value { get; private set; }

        public Variable(String name, Object value)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("name", name);

            Name = name;
            Value = value;
        }
    }
}
