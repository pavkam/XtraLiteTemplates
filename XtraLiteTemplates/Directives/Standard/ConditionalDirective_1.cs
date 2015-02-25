using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Directives.Standard
{
    public sealed class ConditionalDirective_1 : Directive
    {
        public ConditionalDirective_1() :
            base(new DirectiveDefinition().ExpectKeyword("IF").ExpectVariable("LEFT VALUE").ExpectIdentifier("OPERATOR").ExpectVariable("RIGHT VALUE"))
        {
        }
    }
}
