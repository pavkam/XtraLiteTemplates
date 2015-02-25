using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Parsing
{
    public interface IParserProperties
    {
        Char DirectiveSectionStartCharacter { get; }
        Char DirectiveSectionEndCharacter { get; }
    }
}
