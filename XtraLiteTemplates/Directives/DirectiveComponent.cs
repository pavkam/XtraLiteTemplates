using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Directives
{
    internal sealed class DirectiveComponent
    {
        internal enum DirectiveComponentType
        {
            Keyword = 1,
            Variable = 2,
            Constant = 3,
            Indentifier = 4,
        }

        public DirectiveComponentType Type { get; private set; }
        public String Key { get; private set; }
        public String Keyword { get; private set; }

        public DirectiveComponent(String key, DirectiveComponentType type, String keyword)
        {
            if (type == DirectiveComponentType.Keyword)
                ValidationHelper.AssertArgumentIsNotAnEmptyString("keyword", keyword);
            else
                ValidationHelper.AssertArgumentIsNotAnEmptyString("key", key);

            Type = type;
            Key = key;
            Keyword = keyword;
        }
    }
}
