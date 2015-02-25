using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Tom;

namespace XtraLiteTemplates.Parsing
{
    public class TemplateParser
    {
        private enum TokenType
        {
            Identifier,
            Numerical,
            String,
            Symbolic,
        }

        private struct Token
        {
            public TokenType Type;
            public String Value;
        }


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

        internal List<Token> ParseDirective(StringForwardCursor cursor)
        {
            Char current = cursor.ReadNext();
            while (true)
            {
                if (currentlyParsingStringInADirective)
                {
                    if (current == Properties.StringConstantStartAndEndCharacter)
                    {
                        current = cursor.ReadNext();
                        if (current != Properties.StringConstantStartAndEndCharacter)
                        {
                            directiveTokens.Add(new Token()
                            {
                                Type = TokenType.String,
                                Value = tokenBuffer.ToString(),
                            });

                            tokenBuffer.Clear();
                            currentlyParsingStringInADirective = false;

                            continue;
                        }
                    }

                    tokenBuffer.Append(current);
                    current = cursor.ReadNext();
                }
                else if (currentlyParsingDirective)
                {
                    if (current == Properties.StringConstantStartAndEndCharacter)
                    {
                        /* put the stuff before into the token list... do not know how yet */
                        currentlyParsingStringInADirective = true;

                        current = cursor.ReadNext();
                        continue;
                    }
                    else if (current == Properties.DirectiveSectionEndCharacter)
                    {
                        current = cursor.ReadNext();
                        if (current != Properties.DirectiveSectionEndCharacter)
                        {
                            directiveTokens.Add(new Token()
                            {
                                Type = TokenType.String,
                                Value = tokenBuffer.ToString(),
                            });

                            tokenBuffer.Clear();
                            currentlyParsingStringInADirective = false;

                            continue;
                        }
                    }

                    tokenBuffer.Append(current);
                    current = cursor.ReadNext();
                }
                else if (current == Properties.DirectiveSectionStartCharacter)
                {
                    var next = cursor.ReadNext();
                    if (next != current)
                    {
                        currentlyParsingDirective = true;
                        current = next;

                        continue;
                    }
                }
                else
                {
                    plainTextBuffer.Append(current);

                    if (!cursor.TryReadNext(out current))
                        break;
                }
            }
        }

        internal void Parse(TomDocumentBuilder builder)
        {
            ValidationHelper.AssertArgumentIsNotNull("builder", builder);

            /* Start a cursor to be used when reading the string. */
            StringForwardCursor cursor = new StringForwardCursor(Template);

            Boolean currentlyParsingDirective = false;
            Boolean currentlyParsingStringInADirective = false;

            List<Token> directiveTokens = new List<Token>();
            StringBuilder plainTextBuffer = new StringBuilder();
            StringBuilder tokenBuffer = new StringBuilder();

            Char current = cursor.ReadNext();
            while (true)
            {
                if (currentlyParsingStringInADirective)
                {                    
                    if (current == Properties.StringConstantStartAndEndCharacter)
                    {
                        current = cursor.ReadNext();
                        if (current != Properties.StringConstantStartAndEndCharacter)
                        {
                            directiveTokens.Add(new Token()
                            {
                                Type = TokenType.String,
                                Value = tokenBuffer.ToString(),
                            });

                            tokenBuffer.Clear();
                            currentlyParsingStringInADirective = false;

                            continue;
                        }
                    }

                    tokenBuffer.Append(current);
                    current = cursor.ReadNext();
                }
                else if (currentlyParsingDirective)
                {
                    if (current == Properties.StringConstantStartAndEndCharacter)
                    {
                        /* put the stuff before into the token list... do not know how yet */
                        currentlyParsingStringInADirective = true;

                        current = cursor.ReadNext();
                        continue;
                    } 
                    else if (current == Properties.DirectiveSectionEndCharacter)
                    {
                        current = cursor.ReadNext();
                        if (current != Properties.DirectiveSectionEndCharacter)
                        {
                            directiveTokens.Add(new Token()
                            {
                                Type = TokenType.String,
                                Value = tokenBuffer.ToString(),
                            });

                            tokenBuffer.Clear();
                            currentlyParsingStringInADirective = false;

                            continue;
                        }
                    }

                    tokenBuffer.Append(current);
                    current = cursor.ReadNext();
                }
                else if (current == Properties.DirectiveSectionStartCharacter)
                {
                    var next = cursor.ReadNext();
                    if (next != current)
                    {
                        currentlyParsingDirective = true;
                        current = next;

                        continue;
                    }
                }
                else
                {
                    plainTextBuffer.Append(current);

                    if (!cursor.TryReadNext(out current))
                        break;
                }
            }
        }
    }
}
