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
    public sealed class SimpleDirectiveNode : TemplateNode
    {
        public Directive Directive { get; private set; }

        internal SimpleDirectiveNode(CompositeNode parent, Directive directive)
            : base(parent)
        {
            ValidationHelper.AssertArgumentIsNotNull("directive", directive);

            Directive = directive;
        }

        public override Boolean Evaluate(TextWriter writer, Object evaluationContext)
        {
            ValidationHelper.AssertArgumentIsNotNull("writer", writer);

            return Directive.EvaluateTomNode(writer, context, this);
        }
    }
}
