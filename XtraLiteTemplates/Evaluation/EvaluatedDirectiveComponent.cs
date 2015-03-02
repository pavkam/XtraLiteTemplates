using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Directives;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Evaluation
{
    internal sealed class EvaluatedDirectiveComponent
    {
        public DirectiveComponent Component { get; private set; }

        public DirectiveComponent.DirectiveComponentType ActualType { get; private set; }
        public String ActualValue { get; private set; }

        public EvaluatedDirectiveComponent(DirectiveComponent component, 
            DirectiveComponent.DirectiveComponentType actualType, String actualValue)
        {
            ValidationHelper.AssertArgumentIsNotNull("component", component);

            Component = component;
            ActualType = actualType;
            ActualValue = actualValue;
        }
    }
}
