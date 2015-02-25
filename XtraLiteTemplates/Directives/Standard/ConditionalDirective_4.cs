using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Directives.Standard
{
    public sealed class ConditionalDirective_4 : Directive
    {
        public ConditionalDirective_4() :
            base(new DirectiveDefinition().ExpectKeyword("IF").ExpectVariable("VALUE").ExpectKeyword("IS").ExpectKeyword("SET"))
        {
        }
    }
}
