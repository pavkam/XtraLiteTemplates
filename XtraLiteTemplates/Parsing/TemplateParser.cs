using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.TOM;

namespace XtraLiteTemplates.Parsing
{
    public class TemplateParser
    {
        public String Template { get; private set; }
        public IParserProperties Properties { get; private set; }

        public TemplateParser(IParserProperties properties, String template)
        {
            if (properties == null)
                throw new ArgumentNullException("properties");
            if (template == null)
                throw new ArgumentNullException("template");

            Properties = properties;
            Template = template;
        }

        private void ParseError(Char character, Int32 position, Int32 line, Int32 positionInLine, String errorMessage)
        {
            throw new ParsingException(character, position, line, positionInLine, errorMessage);
        }

        private void ParseError(Char character, Int32 position, Int32 line, Int32 positionInLine, String errorMessage)
        {
            throw new ParsingException(character, position, line, positionInLine, errorMessage);
        }

        private void ContinueParsingDirective(String directive)
        {

        }

        public DocumentNode Parse()
        {
            /* Start a cursor to be used when reading the string. */
            StringForwardCursor cursor = new StringForwardCursor(Template);

            Boolean previousCharacterWasDirectiveSectionStart = false;
            Boolean currentlyParsingDirective = false;

            StringBuilder plainTextBuffer = new StringBuilder();
            StringBuilder directiveBuffer = new StringBuilder();

            DocumentNode document = new DocumentNode();
            CompositeNode currentParentNode = document;

            while (!cursor.EndOfString)
            {
                Char current = cursor.ReadNext();

                if (current == Properties.DirectiveSectionStartCharacter)
                {
                    /* Directive start character detected "{". Check if another one before that (then this is an escape sequence). */
                    if (previousCharacterWasDirectiveSectionStart)
                    {
                        /* Yes, this is an escape sequence "{{". Add it as plain text. */
                        plainTextBuffer.Append(current);
                        previousCharacterWasDirectiveSectionStart = false;
                    }
                    else
                    {
                        /* No, this is the first instance of the directive start character "{". Mark it as such. */
                        previousCharacterWasDirectiveSectionStart = true;                        
                    }
                }
                else
                {
                    if (previousCharacterWasDirectiveSectionStart)
                    {
                        /* Create a text TOM node. */
                        PlainTextNode node = new PlainTextNode(currentParentNode, plainTextBuffer.ToString());
                        currentParentNode.AddChild(node);

                        plainTextBuffer.Clear();
                        currentlyParsingDirective = true;
                    }

                    if (current == Properties.DirectiveSectionEndCharacter)
                    {
                        /* Directive end character detected "}". */
                        if (currentlyParsingDirective)
                        {
                            /* A directive was actively parsed. Close it off. */
                            currentlyParsingDirective = false;
                            
                            ContinueParsingDirective(directiveBuffer.ToString());
                            directiveBuffer.Clear();
                        } 
                        else
                        {
                            /* Just a random sitting "}" character. Treat as simple text. */
                            plainTextBuffer.Append(current);
                        }
                    }
                    else if (currentlyParsingDirective)
                    {
                        /* Nothing special over here, but we are parsing a directive, so put all the text into the proper buffer. */
                        directiveBuffer.Append(current);
                    }
                    else
                    {
                        /* Finally, this is just plain-old text. */
                        plainTextBuffer.Append(current);
                    }

                    previousCharacterWasDirectiveSectionStart = false;
                }
            }

            return document;
        }
    }
}
