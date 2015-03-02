using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Definition;
using XtraLiteTemplates.Parsing.ObjectModel;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Parsing
{
    public class TemplateParser
    {
        public String Template { get; private set; }
        public ParserProperties Properties { get; private set; }
        public IDirectiveSelectionStrategy DirectiveSelector { get; private set; }
        public Boolean StrictDirectiveSelection { get; private set; }

        public TemplateParser(ParserProperties properties, 
            IDirectiveSelectionStrategy directiveSelector,
            Boolean strictDirectiveSelection, String template)
        {
            ValidationHelper.AssertArgumentIsNotNull("properties", properties);
            ValidationHelper.AssertArgumentIsNotNull("template", template);
            ValidationHelper.AssertArgumentIsNotNull("directiveSelector", directiveSelector);

            Properties = properties;
            Template = template;
            DirectiveSelector = directiveSelector;
            StrictDirectiveSelection = strictDirectiveSelection;
        }

        private CompositeNode CreateDirectiveNode(IDirectiveSelectionStrategy selectionStrategy, CompositeNode parent, 
            IReadOnlyList<DirectiveLiteral> literals)
        {
            /* Select the directive. */
            var matchingDirectives = selectionStrategy.SelectDirective(literals);

            MatchedDirective directive = null;
            if (matchingDirectives.Count == 0)
                throw new InvalidOperationException("no matching node ffs.");
            else if (StrictDirectiveSelection && matchingDirectives.Count > 1)
                throw new InvalidOperationException("too many matching nodes ffs.");
            else
                directive = matchingDirectives.First();

            /* Process the directive. */
            
            if (directive.Placement == DirectiveDefinitionPlacement.Above)
            {
                if (directive.Directive.IsComposite)
                {
                    var node = new CompositeDirectiveNode(parent, directive.Directive);
                    parent.AddChild(node);
                    parent = node;
                }
                else
                {
                    var node = new SimpleDirectiveNode(parent, directive.Directive);
                    parent.AddChild(node);
                }
            }
            else
            {
                var hopefullyMatchingOpening = (parent as CompositeDirectiveNode);
                if (hopefullyMatchingOpening != null && hopefullyMatchingOpening.Directive == directive.Directive)
                {
                    parent = parent.Parent;
                }
                else
                    throw new InvalidOperationException("Closing an unrelated node. Not matching the current open one.");
            }

            return parent;
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


        public TemplateDocument Parse()
        {
            /* Start a cursor to be used when reading the string. */
            StringForwardCursor cursor = new StringForwardCursor(Template);

            List<DirectiveLiteral> directiveLiterals = new List<DirectiveLiteral>();

            StringBuilder plainTextBuffer = new StringBuilder();
            StringBuilder tokenBuffer = new StringBuilder();
            Nullable<DirectiveLiteralType> directiveLiteralType = null;

            Boolean parsingDirective = false;

            /* Prepare document Object Model. */
            TemplateDocument document = new TemplateDocument();
            CompositeNode currentNode = document;

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
                    if (directiveLiteralType == DirectiveLiteralType.StringConstant)
                    {
                        if (current == Properties.StringConstantEndCharacter && !isEscapedChar)
                        {
                            /* Finalize the string. */
                            directiveLiterals.Add(new DirectiveLiteral(directiveLiteralType.Value, tokenBuffer.ToString()));

                            directiveLiteralType = null;
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

                        if (directiveLiteralType != null)
                            directiveLiterals.Add(new DirectiveLiteral(directiveLiteralType.Value, tokenBuffer.ToString()));

                        currentNode = CreateDirectiveNode(directiveSelector, currentNode, directiveLiterals);
                        directiveLiterals.Clear();
                    }
                    else if (current == Properties.StringConstantStartCharacter && !isEscapedChar)
                    {
                        /* Start string. */
                        if (directiveLiteralType != null)
                        {
                            directiveLiterals.Add(new DirectiveLiteral(directiveLiteralType.Value, tokenBuffer.ToString()));
                            tokenBuffer.Clear();
                        }

                        tokenBuffer.Append(current);
                        directiveLiteralType = DirectiveLiteralType.StringConstant;
                    }
                    else
                    {
                        /* Check for token boundaries and such. */
                        if (Char.IsWhiteSpace(current))
                        {
                            if (directiveLiteralType != null)
                            {
                                directiveLiterals.Add(new DirectiveLiteral(directiveLiteralType.Value, tokenBuffer.ToString()));
                                directiveLiteralType = null;
                            }
                        }
                        else if (Char.IsLetter(current))
                        {
                            if (directiveLiteralType == DirectiveLiteralType.NormalIdentifier)
                                tokenBuffer.Append(current);
                            else if (directiveLiteralType == DirectiveLiteralType.NumericalConstant)
                            {
                                directiveLiteralType = DirectiveLiteralType.NormalIdentifier;
                                tokenBuffer.Append(current);
                            }
                            else
                            {
                                if (directiveLiteralType != null)
                                {
                                    directiveLiterals.Add(new DirectiveLiteral(directiveLiteralType.Value, tokenBuffer.ToString()));
                                    tokenBuffer.Clear();
                                }

                                tokenBuffer.Append(current);
                                directiveLiteralType = DirectiveLiteralType.NormalIdentifier;
                            }
                        }
                        else if (Char.IsDigit(current)) //* NEGATIVE OR POSITIVE NUMBERS! *//
                        {
                            if (directiveLiteralType == DirectiveLiteralType.NormalIdentifier || directiveLiteralType == DirectiveLiteralType.NumericalConstant)
                                tokenBuffer.Append(current);
                            else
                            {
                                if (directiveLiteralType != null)
                                {
                                    directiveLiterals.Add(new DirectiveLiteral(directiveLiteralType.Value, tokenBuffer.ToString()));
                                    tokenBuffer.Clear();
                                }

                                tokenBuffer.Append(current);
                                directiveLiteralType = DirectiveLiteralType.NumericalConstant;
                            }
                        }
                        else if (Char.IsPunctuation(current) || Char.IsSeparator(current) || Char.IsSymbol(current))
                        {
                            if (directiveLiteralType == DirectiveLiteralType.SymbologicalIdentifier)
                                tokenBuffer.Append(current);
                            else
                            {
                                if (directiveLiteralType != null)
                                {
                                    directiveLiterals.Add(new DirectiveLiteral(directiveLiteralType.Value, tokenBuffer.ToString()));
                                    tokenBuffer.Clear();
                                }

                                tokenBuffer.Append(current);
                                directiveLiteralType = DirectiveLiteralType.SymbologicalIdentifier;
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
                    if (plainTextBuffer.Length > 0)
                    {
                        currentNode.AddChild(new TextNode(currentNode, plainTextBuffer.ToString()));
                        plainTextBuffer.Clear();
                    }

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
