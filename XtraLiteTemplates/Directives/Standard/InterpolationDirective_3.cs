using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Directives.Standard
{
    public sealed class InterpolationDirective_3 : Directive
    {
        public InterpolationDirective_3() :
            base(new DirectiveDefinition().ExpectVariable("VALUE").ExpectKeyword("AS").ExpectConstant("FORMAT"))
        {
        }
    }
}
