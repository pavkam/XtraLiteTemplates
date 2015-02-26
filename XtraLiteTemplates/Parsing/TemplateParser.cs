using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Tom;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Parsing
{
    public class TemplateParser
    {
        public String Template { get; private set; }
        public ParserProperties Properties { get; private set; }

        public TemplateParser(ParserProperties properties, String template)
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
            throw new TemplateParseException(character, position, line, positionInLine, errorMessage);
        }

        private void ParseError(Char character, Int32 position, Int32 line, Int32 positionInLine, String errorMessage)
        {
            throw new TemplateParseException(character, position, line, positionInLine, errorMessage);
        }

        private Char ParseEscapedCharacter(StringForwardCursor cursor)
        {
            var char1 = cursor.ReadNext();

            if (char1 == Properties.DirectiveSectionEndCharacter ||
                char1 == Properties.DirectiveSectionStartCharacter ||
                char1 == Properties.StringConstantEndCharacter ||
                char1 == Properties.StringConstantStartCharacter ||
                char1 == Properties.EscapeCharacter)
            {
                return char1;
            }
            else
            {
                switch (char1)
                {
                    case 'a':
                        return '\a';
                    case 'b':
                        return '\b';
                    case 'f':
                        return '\f';
                    case 'n':
                        return '\n';
                    case 'r':
                        return '\r';
                    case 't':
                        return '\t';
                    case 'v':
                        return '\v';
                    case '\'':
                    case '"':
                    case '?':
                    case '\\':
                        return char1;
                }
            }

            ParseError(char1, cursor.CurrentCharacterPosition, cursor.CurrentCharacterLine, cursor.CurrentCharacterPositionInLine,
                "Unspupported escape sequence.");
        }

        internal void Parse(TomDocumentBuilder builder)
        {
            Debug.Assert(builder != null);

            /* Start a cursor to be used when reading the string. */
            StringForwardCursor cursor = new StringForwardCursor(Template);

            List<DirectiveToken> directiveTokens = new List<DirectiveToken>();

            StringBuilder plainTextBuffer = new StringBuilder();
            StringBuilder tokenBuffer = new StringBuilder();
            Nullable<DirectiveToken.TokenType> tokenType = null;

            Boolean parsingDirective = false;

            Char current = '\0';
            Boolean isEscapedChar = false;
            while (true)
            {
                isEscapedChar = false;
                current = cursor.ReadNext();

                /* 1. Check for escaped sequences */
                if (current == Properties.EscapeCharacter)
                {
                    current = ParseEscapedCharacter(cursor);
                    isEscapedChar = true;
                }

                if (parsingDirective)
                {
                    if (tokenType == DirectiveToken.TokenType.String)
                    {
                        if (current == Properties.StringConstantEndCharacter && !isEscapedChar)
                        {
                            /* Finalize the string. */
                            directiveTokens.Add(new DirectiveToken(tokenType.Value, tokenBuffer.ToString()));

                            tokenType = null;
                        }
                        else
                        {
                            /* All straight in. */
                            tokenBuffer.Append(current);
                        }
                    }
                    else if (current == Properties.DirectiveSectionEndCharacter && !isEscapedChar)
                    {
                        /* Finalize the directive. */
                        parsingDirective = false;

                        if (tokenType != null)
                            directiveTokens.Add(new DirectiveToken(tokenType.Value, tokenBuffer.ToString()));

                        builder.AddDirective(directiveTokens);
                        directiveTokens.Clear();
                    }
                    else if (current == Properties.StringConstantStartCharacter && !isEscapedChar)
                    {
                        /* Start string. */
                        if (tokenType != null)
                        {
                            directiveTokens.Add(new DirectiveToken(tokenType.Value, tokenBuffer.ToString()));
                            tokenBuffer.Clear();
                        }

                        tokenBuffer.Append(current);
                        tokenType = DirectiveToken.TokenType.String;
                    }
                    else
                    {
                        /* Check for token boundaries and such. */
                        if (Char.IsWhiteSpace(current))
                        {
                            if (tokenType != null)
                            {
                                directiveTokens.Add(new DirectiveToken(tokenType.Value, tokenBuffer.ToString()));
                                tokenType = null;
                            }
                        }
                        else if (Char.IsLetter(current))
                        {
                            if (tokenType == DirectiveToken.TokenType.Identifier)
                                tokenBuffer.Append(current);
                            else if (tokenType == DirectiveToken.TokenType.Numerical)
                            {
                                tokenType = DirectiveToken.TokenType.Identifier;
                                tokenBuffer.Append(current);
                            }
                            else
                            {
                                if (tokenType != null)
                                {
                                    directiveTokens.Add(new DirectiveToken(tokenType.Value, tokenBuffer.ToString()));
                                    tokenBuffer.Clear();
                                }

                                tokenBuffer.Append(current);
                                tokenType = DirectiveToken.TokenType.Identifier;
                            }
                        }
                        else if (Char.IsDigit(current)) //* NEGATIVE OR POSITIVE NUMBERS! *//
                        {
                            if (tokenType == DirectiveToken.TokenType.Identifier || tokenType == DirectiveToken.TokenType.Numerical)
                                tokenBuffer.Append(current);
                            else
                            {
                                if (tokenType != null)
                                {
                                    directiveTokens.Add(new DirectiveToken(tokenType.Value, tokenBuffer.ToString()));
                                    tokenBuffer.Clear();
                                }

                                tokenBuffer.Append(current);
                                tokenType = DirectiveToken.TokenType.Numerical;
                            }
                        }
                        else if (Char.IsPunctuation(current) || Char.IsSeparator(current) || Char.IsSymbol(current))
                        {
                            if (tokenType == DirectiveToken.TokenType.Symbolic)
                                tokenBuffer.Append(current);
                            else
                            {
                                if (tokenType != null)
                                {
                                    directiveTokens.Add(new DirectiveToken(tokenType.Value, tokenBuffer.ToString()));
                                    tokenBuffer.Clear();
                                }

                                tokenBuffer.Append(current);
                                tokenType = DirectiveToken.TokenType.Symbolic;
                            }
                        }
                        else
                        {
                            /* We cannot handle these chars in a directive! */
                            ParseError(current, cursor.CurrentCharacterPosition, cursor.CurrentCharacterLine,
                                cursor.CurrentCharacterPositionInLine, "Invalid character detedted in directive.");
                        }
                    }
                }
                else if (current == Properties.DirectiveSectionStartCharacter && !isEscapedChar)
                {
                    /* Starting a directive. */
                    parsingDirective = true;
                }
                else
                {
                    plainTextBuffer.Append(current);

                    /* Break cleanly? */
                    if (cursor.EndOfString)
                        break;
                }
            }
        }
    }
}
