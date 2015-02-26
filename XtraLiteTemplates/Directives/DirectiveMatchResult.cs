using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Parsing;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Directives
{
    internal enum DirectiveMatchResult
    {
        MatchingFaceDirective,
        MatchingTailDirective,
        NoMatches,
        AmbiguousMatches,
    }
}
