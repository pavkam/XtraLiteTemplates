using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Directives;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Tom
{
    public sealed class DirectiveTomNode : CompositeTomNode
    {
        public Directive Directive { get; private set; }

        public DirectiveTomNode(TomNode parent, Directive directive)
            : base(parent)
        {
            ValidationHelper.AssertArgumentIsNotNull("directive", directive);

            Directive = directive;
        }
    }
}
