using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Definition;
using XtraLiteTemplates.Evaluation;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Parsing.ObjectModel
{
    public sealed class CompositeDirectiveNode : CompositeNode
    {
        public Directive Directive { get; private set; }
        public IReadOnlyCollection<MatchedDirectiveDefinitionComponent> Components { get; private set; }

        internal CompositeDirectiveNode(CompositeNode parent, Directive directive, 
            IReadOnlyCollection<MatchedDirectiveDefinitionComponent> components)
            : base(parent)
        {
            ValidationHelper.AssertArgumentIsNotNull("directive", directive);
            ValidationHelper.AssertObjectCollectionIsNotEmpty("components", components);

            Directive = directive;
            Components = components;
        }

        public override Int32 Evaluate(TextWriter writer, IEvaluationContext evaluationContext)
        {
            ValidationHelper.AssertArgumentIsNotNull("writer", writer);
            ValidationHelper.AssertArgumentIsNotNull("evaluationContext", evaluationContext);

            return Directive.Evaluate(writer, Children.Cast<IEvaluable>().ToList(), Components, evaluationContext);
        }
    }
}
