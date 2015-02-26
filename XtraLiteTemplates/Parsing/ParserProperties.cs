using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Parsing
{
    public sealed class ParserProperties
    {
        public ParserProperties()
            : this('\\', '{', '}', '\'', '\'')
        {
        }

        public ParserProperties(
            Char escapeCharacter, 
            Char directiveSectionStartCharacter, Char directiveSectionEndCharacter,
            Char stringConstantStartCharacter, Char stringConstantEndCharacter)
        {
            /* Validate the character selection. */
            ValidationHelper.Assert("escapeCharacter", "escapeCharacter must not match directiveSectionStartCharacter", 
                escapeCharacter != directiveSectionStartCharacter);
            ValidationHelper.Assert("escapeCharacter", "escapeCharacter must not match directiveSectionEndCharacter", 
                escapeCharacter != directiveSectionEndCharacter);
            ValidationHelper.Assert("escapeCharacter", "escapeCharacter must not match stringConstantStartCharacter", 
                escapeCharacter != stringConstantStartCharacter);
            ValidationHelper.Assert("escapeCharacter", "escapeCharacter must not match stringConstantEndCharacter", 
                escapeCharacter != stringConstantEndCharacter);
            ValidationHelper.Assert("stringConstantStartCharacter", "stringConstantStartCharacter must not match directiveSectionStartCharacter", 
                stringConstantStartCharacter != directiveSectionStartCharacter);
            ValidationHelper.Assert("stringConstantStartCharacter", "stringConstantStartCharacter must not match directiveSectionEndCharacter", 
                stringConstantStartCharacter != directiveSectionEndCharacter);
            ValidationHelper.Assert("stringConstantEndCharacter", "stringConstantEndCharacter must not match directiveSectionStartCharacter", 
                stringConstantEndCharacter != directiveSectionStartCharacter);
            ValidationHelper.Assert("stringConstantEndCharacter", "stringConstantEndCharacter must not match directiveSectionEndCharacter", 
                stringConstantEndCharacter != directiveSectionEndCharacter);

            /* Assign */
            EscapeCharacter = escapeCharacter;
            DirectiveSectionStartCharacter = directiveSectionStartCharacter;
            DirectiveSectionEndCharacter = directiveSectionEndCharacter;
            StringConstantStartCharacter = stringConstantStartCharacter;
            StringConstantEndCharacter = stringConstantEndCharacter;
        }

        public Char EscapeCharacter { get; private set; }
        public Char DirectiveSectionStartCharacter { get; private set; }
        public Char DirectiveSectionEndCharacter { get; private set; }
        public Char StringConstantStartCharacter { get; private set; }
        public Char StringConstantEndCharacter { get; private set; }
    }
}
