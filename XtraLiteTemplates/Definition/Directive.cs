using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Evaluation;
using XtraLiteTemplates.Parsing.ObjectModel;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Definition
{
    public abstract class Directive
    {
        protected internal IList<DirectiveDefinitionComponent> AboveDefinition { get; private set; }
        protected internal IList<DirectiveDefinitionComponent> BelowDefinition { get; private set; }

        public IEqualityComparer<String> Comparer { get; private set; }

        public DirectiveDefinitionComponent GetComponent(DirectiveDefinitionPlacement placement, Int32 index)
        {
            var definition = AboveDefinition;
            if (placement == DirectiveDefinitionPlacement.Below)
                definition = BelowDefinition;

            if (definition.Count <= index)
                return null;
            else
                return definition[index];
        }

        public virtual Boolean AcceptsConstant(DirectiveDefinitionComponent component, String constant)
        {
            return true;
        }

        public virtual Boolean AcceptsIdentifier(DirectiveDefinitionComponent component, String identifier)
        {
            return true;
        }

        public Directive(IEqualityComparer<String> comparer)
        {
            ValidationHelper.AssertArgumentIsNotNull("comparer", comparer);

            /* Initialize the start and end definitions. */
            AboveDefinition = new List<DirectiveDefinitionComponent>();
            BelowDefinition = new List<DirectiveDefinitionComponent>();
        }

        public Directive() : 
            this(StringComparer.Ordinal)
        {
        }

        public Boolean IsComposite
        {
            get
            {
                return BelowDefinition.Count > 0;
            }
        }


        internal Int32 Evaluate(TextWriter writer, TemplateNode node)
        {
        }
    }
}
