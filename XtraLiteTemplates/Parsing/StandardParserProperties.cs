using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Parsing
{
    public class StandardParserProperties : IParserProperties
    {
        public StandardParserProperties()
            : this('{', '}')
        {
        }

        public StandardParserProperties(
            Char directiveSectionStartCharacter, Char directiveSectionEndCharacter)
        {
            DirectiveSectionStartCharacter = directiveSectionStartCharacter;
            DirectiveSectionEndCharacter = directiveSectionEndCharacter;
        }

        public Char DirectiveSectionStartCharacter { get; private set; }
        public Char DirectiveSectionEndCharacter { get; private set; }
    }
}
