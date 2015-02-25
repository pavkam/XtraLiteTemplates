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
            : this('{', '}', '\'')
        {
        }

        public StandardParserProperties(
            Char directiveSectionStartCharacter, Char directiveSectionEndCharacter, Char stringConstantStartAndEndCharacter)
        {
            DirectiveSectionStartCharacter = directiveSectionStartCharacter;
            DirectiveSectionEndCharacter = directiveSectionEndCharacter;
            StringConstantStartAndEndCharacter = stringConstantStartAndEndCharacter;
        }

        public Char DirectiveSectionStartCharacter { get; private set; }
        public Char DirectiveSectionEndCharacter { get; private set; }
        public Char StringConstantStartAndEndCharacter { get; private set; }
    }
}
