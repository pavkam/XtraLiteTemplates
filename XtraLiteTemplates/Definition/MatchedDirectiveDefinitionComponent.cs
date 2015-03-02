using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Definition;
using XtraLiteTemplates.Parsing;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Definition
{
    public sealed class MatchedDirectiveDefinitionComponent
    {
        public DirectiveDefinitionComponent Component { get; private set; }
        public DirectiveLiteral Literal { get; private set; }

        internal MatchedDirectiveDefinitionComponent(DirectiveDefinitionComponent component, DirectiveLiteral literal)
        {
            ValidationHelper.AssertArgumentIsNotNull("component", component);
            ValidationHelper.AssertArgumentIsNotNull("literal", literal);

            Component = component;
            Literal = literal;
        }
    }
}
