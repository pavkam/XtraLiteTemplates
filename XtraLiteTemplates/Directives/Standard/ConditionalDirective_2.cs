using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Directives.Standard
{
    public sealed class ConditionalDirective_2 : Directive
    {
        public ConditionalDirective_2() :
            base(new DirectiveDefinition().ExpectKeyword("1", "IF").ExpectVariable("2").ExpectIdentifier("3").ExpectConstant("4"))
        {
        }
    }
}
