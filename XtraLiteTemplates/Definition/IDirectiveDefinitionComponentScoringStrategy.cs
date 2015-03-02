using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Definition;
using XtraLiteTemplates.Parsing;

namespace XtraLiteTemplates.Definition
{
    public interface IDirectiveDefinitionComponentScoringStrategy
    {
        Int32 ScoreDirectiveDefinitionComponent(Directive directive, DirectiveDefinitionComponent component, DirectiveLiteral literal);
    }
}
