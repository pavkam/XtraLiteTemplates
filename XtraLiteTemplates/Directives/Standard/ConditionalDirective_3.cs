using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Directives.Standard
{
    public sealed class ConditionalDirective_3 : Directive
    {
        public ConditionalDirective_3() :
            base(new DirectiveDefinition().ExpectKeyword("1", "IF").ExpectConstant("LEFT VALUE").ExpectIdentifier("OPERATOR").ExpectVariable("RIGHT VALUE"))
        {
        }
    }
}
