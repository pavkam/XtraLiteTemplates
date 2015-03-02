using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Definition
{
    public sealed class DirectiveDefinitionComponent
    {
        public DirectiveDefinitionComponentType Type { get; private set; }
        public String Key { get; private set; }

        public DirectiveDefinitionComponent(String key, DirectiveDefinitionComponentType type)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("key", key);

            Type = type;
            Key = key;
        }
    }
}
