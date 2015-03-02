﻿using System;
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

        internal CompositeDirectiveNode(CompositeNode parent, Directive directive)
            : base(parent)
        {
            ValidationHelper.AssertArgumentIsNotNull("directive", directive);

            Directive = directive;
        }

        public override Boolean Evaluate(TextWriter writer, Object evaluationContext)
        {
            ValidationHelper.AssertArgumentIsNotNull("writer", writer);
            ValidationHelper.AssertArgumentIsNotNull("context", context);

            return Directive.EvaluateTomNode(writer, context, this);
        }
    }
}
