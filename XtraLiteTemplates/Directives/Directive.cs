using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Directives
{
    public abstract class Directive
    {
        private DirectiveDefinition m_definition;

        public Directive(DirectiveDefinition definition)
        {
            ValidationHelper.AssertArgumentIsNotNull("definition", definition);

            m_definition = definition;
        }

        protected virtual Boolean AcceptsConstant(String componentKey, String constantValue)
        {
            return true;
        }

        protected virtual Boolean AcceptsVariable(String componentKey, String variableName, String variableValue)
        {
            return true;
        }

        protected virtual Boolean AcceptsIdentifier(String componentKey, String identifier)
        {
            return true;
        }

        internal Boolean MatchesKeywordComponent(Int32 index, String keyword)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("keyword", keyword);

            /* Find the component in the definition. */
            var component = m_definition[index];

            if (component == null)
                return false;
            if (component.Type != DirectiveComponent.DirectiveComponentType.Keyword)
                return false;

            return component.Keyword.Equals(component.Keyword);
        }

        internal Boolean MatchesVariableComponent(Int32 index, String variableName, String variableValue)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("variableName", variableName);

            /* Find the component in the definition. */
            var component = m_definition[index];

            if (component == null)
                return false;
            if (component.Type != DirectiveComponent.DirectiveComponentType.Variable)
                return false;

            return AcceptsVariable(component.Key, variableName, variableValue);
        }

        internal Boolean MatchesConstantComponent(Int32 index, String constantValue)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("constantValue", constantValue);

            /* Find the component in the definition. */
            var component = m_definition[index];

            if (component == null)
                return false;
            if (component.Type != DirectiveComponent.DirectiveComponentType.Constant)
                return false;

            return AcceptsConstant(component.Key, constantValue);
        }

        internal Boolean MatchesIdentifierComponent(Int32 index, String identifier)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("identifier", identifier);

            /* Find the component in the definition. */
            var component = m_definition[index];

            if (component == null)
                return false;
            if (component.Type != DirectiveComponent.DirectiveComponentType.Constant)
                return false;

            return AcceptsIdentifier(component.Key, identifier);
        }
    }
}
