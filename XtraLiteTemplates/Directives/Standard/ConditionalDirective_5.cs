using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Directives.Standard
{
    public sealed class ConditionalDirective_5 : Directive
    {
        public ConditionalDirective_5() :
            base(new DirectiveDefinition().ExpectKeyword("IF").ExpectVariable("VALUE").ExpectKeyword("NOT").ExpectKeyword("SET"))
        {
        }
    }
}
