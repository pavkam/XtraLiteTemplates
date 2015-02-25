using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Directives.Standard
{
    public sealed class ForeachDirective : Directive
    {
        public ForeachDirective() :
            base(new DirectiveDefinition().ExpectKeyword("FOR").ExpectIdentifier("ELEMENT").ExpectKeyword("IN").ExpectVariable("COLLECTION"))
        {
        }
    }
}
