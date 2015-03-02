using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Definition;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Definition
{
    public sealed class MatchedDirective
    {
        public Directive Directive { get; private set; }
        public DirectiveDefinitionPlacement Placement { get; private set; }
        public IReadOnlyCollection<MatchedDirectiveDefinitionComponent> ActualComponents { get; private set; }

        internal MatchedDirective(Directive directive, DirectiveDefinitionPlacement placement, 
            IReadOnlyCollection<MatchedDirectiveDefinitionComponent> actualComponents)
        {
            ValidationHelper.AssertArgumentIsNotNull("directive", directive);
            ValidationHelper.AssertObjectCollectionIsNotEmpty("actualComponents", actualComponents);

            Placement = placement;
            Directive = directive;
            ActualComponents = actualComponents;
        }
    }
}
