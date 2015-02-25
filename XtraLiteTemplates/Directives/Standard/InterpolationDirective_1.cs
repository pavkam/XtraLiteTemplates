using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Directives.Standard
{
    public sealed class InterpolationDirective_1 : Directive
    {
        public InterpolationDirective_1() :
            base(new DirectiveDefinition().ExpectVariable("VALUE"))
        {
        }
    }
}
