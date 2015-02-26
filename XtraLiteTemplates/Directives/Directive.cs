using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Directives
{
    public abstract class Directive
    {
        public Boolean IsComposite
        {
            get
            {
                return EndDefinition.ComponentCount > 0;
            }
        }

        public Directive()
        {
            /* Initialize the start and end definitions. */
            StartDefinition = new DirectiveDefinition();
            EndDefinition = new DirectiveDefinition();
        }

        private DirectiveComponent GetFacingComponent(DirectiveFacing facing, Int32 index)
        {
            var definition = StartDefinition;
            if (facing == DirectiveFacing.Tail)
                definition = EndDefinition;

            return definition[index];
        }

        protected DirectiveDefinition StartDefinition { get; private set; }
        protected DirectiveDefinition EndDefinition { get; private set; }

        protected virtual Boolean AcceptsConstant(String componentKey, String constant)
        {
            return true;
        }

        protected virtual Boolean AcceptsIdentifier(String componentKey, String identifier)
        {
            return true;
        }


        internal Boolean MatchesKeywordComponent(DirectiveFacing facing, Int32 index, String keyword)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("keyword", keyword);

            /* Find the component in the definition. */
            var component = GetFacingComponent(facing, index);

            if (component == null)
                return false;
            else if (component.Type != DirectiveComponent.DirectiveComponentType.Keyword)
                return false;
            else
                return component.Keyword.Equals(component.Keyword);
        }

        internal Boolean MatchesVariableComponent(DirectiveFacing facing, Int32 index)
        {
            /* Find the component in the definition. */
            var component = GetFacingComponent(facing, index);

            if (component == null)
                return false;
            else if (component.Type != DirectiveComponent.DirectiveComponentType.Variable)
                return false;
            else
                return true;
        }

        internal Boolean MatchesConstantComponent(DirectiveFacing facing, Int32 index, String constantValue)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("constantValue", constantValue);

            /* Find the component in the definition. */
            var component = GetFacingComponent(facing, index);

            if (component == null)
                return false;
            else if (component.Type != DirectiveComponent.DirectiveComponentType.Constant)
                return false;
            else
                return AcceptsConstant(component.Key, constantValue);
        }

        internal Boolean MatchesIdentifierComponent(DirectiveFacing facing, Int32 index, String identifier)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("identifier", identifier);

            /* Find the component in the definition. */
            var component = GetFacingComponent(facing, index);

            if (component == null)
                return false;
            else if (component.Type != DirectiveComponent.DirectiveComponentType.Constant)
                return false;
            else 
                return AcceptsIdentifier(component.Key, identifier);
        }
    }
}
