using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Definition;
using XtraLiteTemplates.Parsing;

namespace XtraLiteTemplates.Definition
{
    public interface IDirectiveSelectionStrategy
    {
        IReadOnlyCollection<MatchedDirective> SelectDirective(IReadOnlyList<DirectiveLiteral> literals);
    }
}
