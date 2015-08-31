//  Author:
//    Alexandru Ciobanu alex@ciobanu.org
//
//  Copyright (c) 2015, Alexandru Ciobanu (alex@ciobanu.org)
//
//  All rights reserved.
//
//  Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
//
//     * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in
//       the documentation and/or other materials provided with the distribution.
//     * Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
//
//  THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS
//  "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT
//  LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR
//  A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
//  CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL,
//  EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO,
//  PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
//  PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
//  LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
//  NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
//  SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Does not apply.")]

namespace XtraLiteTemplates.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Parsing;

    /// <summary>
    /// The lexical analyzer class. Requires an instance of <see cref="XtraLiteTemplates.Parsing.ITokenizer"/> to obtain all the tokens from. Based
    /// on said tokens, the <see cref="XtraLiteTemplates.Parsing.Lexer"/> identifies the correct lexical structures and generates <see cref="XtraLiteTemplates.Parsing.Lex"/> objects
    /// for the <see cref="XtraLiteTemplates.Evaluation.Interpreter"/>.
    /// </summary>
    public sealed class Lexer
    {
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private ExpressionFlowSymbols expressionFlowSymbols;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private List<Operator> expressionOperators;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private HashSet<string> unaryExpressionOperators;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private HashSet<string> binaryExpressionOperators;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private Dictionary<string, object> specialConstants;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private List<Tag> registeredTags;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private Token currentToken;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private bool endOfStream;

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class.
        /// </summary>
        /// <param name="tokenizer">The <see cref="XtraLiteTemplates.Parsing.ITokenizer" /> object used to read the tokens from the input template.</param>
        /// <param name="expressionFlowSymbols">The <see cref="XtraLiteTemplates.Expressions.ExpressionFlowSymbols" /> object containing the standard expression flow control symbols.</param>
        /// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{String}" /> object used to match keywords and identifiers.</param>
        /// <exception cref="System.ArgumentNullException">Either <paramref name="tokenizer" />, <paramref name="expressionFlowSymbols" /> or <paramref name="expressionFlowSymbols" /> are <c>null</c>.</exception>
        public Lexer(ITokenizer tokenizer, ExpressionFlowSymbols expressionFlowSymbols, IEqualityComparer<string> comparer)
        {
            Expect.NotNull("tokenizer", tokenizer);
            Expect.NotNull("comparer", comparer);
            Expect.NotNull("expressionFlowSymbols", expressionFlowSymbols);

            this.Tokenizer = tokenizer;
            this.Comparer = comparer;

            this.registeredTags = new List<Tag>();
            this.expressionOperators = new List<Operator>();
            this.unaryExpressionOperators = new HashSet<string>(comparer);
            this.binaryExpressionOperators = new HashSet<string>(comparer);
            this.specialConstants = new Dictionary<string, object>(comparer);

            this.expressionFlowSymbols = expressionFlowSymbols;

            /* Register the flow symbols in */
            this.binaryExpressionOperators.Add(expressionFlowSymbols.Separator);
            this.binaryExpressionOperators.Add(expressionFlowSymbols.MemberAccess);
            this.unaryExpressionOperators.Add(expressionFlowSymbols.GroupOpen);
            this.binaryExpressionOperators.Add(expressionFlowSymbols.GroupClose);
        }

        /// <summary>
        /// Gets the <see cref="XtraLiteTemplates.Parsing.ITokenizer" /> object used to read the tokens from the input template.
        /// </summary>
        /// <value>
        /// The input template <c>tokenizer</c>.
        /// </value>
        /// <remarks>
        /// The value of this property is provided by the caller during the construction process.
        /// </remarks>
        public ITokenizer Tokenizer { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{String}" /> object used to match keywords and identifiers.
        /// </summary>
        /// <value>
        /// The identifier comparer.
        /// </value>
        /// <remarks>
        /// The value of this property is provided by the caller during the construction process.
        /// </remarks>
        public IEqualityComparer<string> Comparer { get; private set; }

        /// <summary>
        /// Gets all the registered <see cref="XtraLiteTemplates.Parsing.Tag" /> objects.
        /// </summary>
        /// <value>
        /// The registered tags.
        /// </value>
        /// <remarks>
        /// The caller is responsible with loading up the known tag objects into the <c>lexer</c> before attempting to reading the first <c>lex</c> object.
        /// </remarks>
        public IReadOnlyCollection<Tag> Tags
        {
            get
            {
                return this.registeredTags;
            }
        }

        /// <summary>
        /// Registers a know tag with this <c>lexer</c> instance. All registered tags will take part in the matching process during the analysis of the
        /// incoming tokens.
        /// </summary>
        /// <param name="tag">A <see cref="XtraLiteTemplates.Parsing.Tag"/> object to register.</param>
        /// <returns>This <c>lexer</c> instance.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="tag"/> is <c>null</c>.</exception>
        public Lexer RegisterTag(Tag tag)
        {
            Expect.NotNull("tag", tag);
            if (tag.ComponentCount == 0)
            {
                ExceptionHelper.CannotRegisterTagWithNoComponents();
            }

            /* Check for an equivalent tag in the list */
            foreach (var ot in this.registeredTags)
            {
                if (ot.Equals(tag, this.Comparer))
                {
                    return this;
                }
            }

            this.registeredTags.Add(tag);
            return this;
        }

        /// <summary>
        /// Registers a know operator with this <c>lexer</c> instance. All registered operators will take part in the matching process 
        /// during the expression analysis of the incoming tokens.
        /// </summary>
        /// <param name="operator">A <see cref="XtraLiteTemplates.Expressions.Operators.Operator"/> object to register.</param>
        /// <returns>This <c>lexer</c> instance.</returns>
        /// <exception cref="System.ArgumentNullException">Argument <paramref name="operator"/> is <c>null</c>.</exception>
        public Lexer RegisterOperator(Operator @operator)
        {
            Expect.NotNull("operator", @operator);

            var unaryOperator = @operator as UnaryOperator;
            if (unaryOperator != null)
            {
                if (this.unaryExpressionOperators.Contains(unaryOperator.Symbol) ||
                    this.specialConstants.ContainsKey(unaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(@operator);
                }
                else
                {
                    this.unaryExpressionOperators.Add(unaryOperator.Symbol);
                }
            }

            var binaryOperator = @operator as BinaryOperator;
            if (binaryOperator != null)
            {
                if (this.binaryExpressionOperators.Contains(binaryOperator.Symbol) ||
                    this.specialConstants.ContainsKey(binaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(@operator);
                }
                else
                {
                    this.binaryExpressionOperators.Add(binaryOperator.Symbol);
                }
            }

            this.expressionOperators.Add(@operator);

            return this;
        }

        /// <summary>
        /// Registers a know special constant. Special constants can be looked at as keywords such as <c>true</c> or <c>Infinity</c>. These identifiers will
        /// be replaced by their literal value during the analysis process and will not be identified as anything else.
        /// <remarks>
        /// <para>Calling this method multiple times for the same <paramref name="keyword"/> will result in the last value being used.</para>
        /// </remarks>
        /// </summary>
        /// <param name="keyword">The name of the constant.</param>
        /// <param name="value">The value of the constant.</param>
        /// <returns>This <c>lexer</c> instance.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="keyword"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="keyword"/> is not a valid identifier.</exception>
        /// <exception cref="System.InvalidOperationException"><paramref name="keyword"/> is already in use by an operator.</exception>
        public Lexer RegisterSpecial(string keyword, object value)
        {
            Expect.Identifier("keyword", keyword);

            if (this.unaryExpressionOperators.Contains(keyword) ||
                this.binaryExpressionOperators.Contains(keyword))
            {
                ExceptionHelper.SpecialCannotBeRegistered(keyword);
            }

            this.specialConstants[keyword] = value;
            return this;
        }

        /// <summary>
        /// Analyzes a batch of tokens and generates a <c>lex</c> object. If the <c>tokenizer</c> reached the end of the stream, this method will
        /// return a <c>null</c> value. All subsequent calls to this method will also return <c>null</c>.
        /// </summary>
        /// <returns>An analyzed <see cref="XtraLiteTemplates.Parsing.Lex"/> object.</returns>
        /// <exception cref="XtraLiteTemplates.Parsing.ParseException">A parsing error encountered.</exception>
        /// <exception cref="XtraLiteTemplates.Expressions.ExpressionException">An expression contruction error encountered.</exception>
        public Lex ReadNext()
        {
            if (this.currentToken == null && !this.endOfStream)
            {
                this.NextToken();
            }

            if (this.endOfStream)
            {
                return null;
            }

            /* Load all unparsed tokens and merge them into a big component. */
            if (this.currentToken.Type == Token.TokenType.Unparsed)
            {
                var allUnparsedTokens = new List<Token>();
                while (!this.endOfStream && this.currentToken.Type == Token.TokenType.Unparsed)
                {
                    allUnparsedTokens.Add(this.currentToken);
                    this.NextToken();
                }

                return new UnparsedLex(
                    string.Join(string.Empty, allUnparsedTokens.Select(s => s.Value)), 
                    allUnparsedTokens[0].CharacterIndex, 
                    allUnparsedTokens.Sum(s => s.OriginalLength));
            }

            /* This is where a tag is parsed. */
            if (this.currentToken.Type != Token.TokenType.StartTag)
            {
                ExceptionHelper.UnexpectedToken(this.currentToken);
            }

            var matchingTags = new HashSet<Tag>(this.registeredTags);
            List<Token> allTagTokens = new List<Token>() { this.currentToken };
            List<object> currentComponents = new List<object>();

            Expression currentExpression = null;
            while (true)
            {
                var previousReadToken = this.currentToken;
                if (!this.NextToken())
                {
                    ExceptionHelper.NoMatchingTagsLeft(currentComponents, previousReadToken);
                }

                allTagTokens.Add(this.currentToken);

                if (this.currentToken.Type == Token.TokenType.EndTag)
                {
                    var matchingTag = matchingTags.Where(p => p.ComponentCount == currentComponents.Count).FirstOrDefault();

                    if (matchingTag == null)
                    {
                        ExceptionHelper.NoMatchingTagsLeft(currentComponents, this.currentToken);
                    }
                    else
                    {
                        if (matchingTag.MatchesExpression(currentComponents.Count - 1))
                        {
                            Debug.Assert(currentExpression != null, "currentExpression cannot be null.");
                            try
                            {
                                currentExpression.Construct();
                            }
                            catch (ExpressionException constructException)
                            {
                                ExceptionHelper.UnexpectedOrInvalidExpressionToken(constructException, this.currentToken);
                            }
                        }

                        object[] actualComponents = new object[currentComponents.Count];
                        for (var i = 0; i < currentComponents.Count; i++)
                        {
                            var component = currentComponents[i];

                            var tuple = component as Tuple<string, Expression>;
                            if (tuple != null)
                            {
                                if (matchingTag.MatchesExpression(i))
                                {
                                    Debug.Assert(tuple.Item2.Constructed, "expression variant expected to be fully constructed.");
                                    actualComponents[i] = tuple.Item2;
                                }
                                else
                                {
                                    actualComponents[i] = tuple.Item1;
                                }
                            }
                            else
                            {
                                Debug.Assert(
                                    component is string || (component is Expression && ((Expression)component).Constructed),
                                    "expression component expected to be fully constructed.");

                                actualComponents[i] = component;
                            }
                        }

                        this.NextToken();
                        return new TagLex(matchingTag, actualComponents, allTagTokens[0].CharacterIndex, allTagTokens.Sum(s => s.OriginalLength));
                    }
                }

                if (this.currentToken.Type == Token.TokenType.Word &&
                    !this.specialConstants.ContainsKey(this.currentToken.Value))
                {
                    if (previousReadToken.Type != Token.TokenType.StartTag &&
                        previousReadToken.Type != Token.TokenType.Symbol &&
                        previousReadToken.Type != Token.TokenType.Whitespace)
                    {
                        ExceptionHelper.UnexpectedToken(this.currentToken);
                    }

                    /* This is either a keyword or part of an expression. Reflect that. */
                    var matchesByKeyword = matchingTags.Where(p => p.MatchesKeyword(currentComponents.Count, this.Comparer, this.currentToken.Value)).ToArray();
                    if (matchesByKeyword.Length > 0)
                    {
                        /* Keyword match. All the rest is now history. */
                        if (matchesByKeyword.Length > 0)
                        {
                            matchingTags = new HashSet<Tag>(matchesByKeyword);
                        }

                        var previousComponentWasExpression = matchingTags.Any(p => p.MatchesExpression(currentComponents.Count - 1));
                        if (previousComponentWasExpression)
                        {
                            Debug.Assert(currentExpression != null, "currentExpression cannot be null.");
                            try
                            {
                                currentExpression.Construct();
                            }
                            catch (ExpressionException constructException)
                            {
                                ExceptionHelper.UnexpectedOrInvalidExpressionToken(constructException, this.currentToken);
                            }
                        }

                        currentExpression = null;
                        currentComponents.Add(this.currentToken.Value);
                        continue;
                    }
                    else
                    {
                        var matchesByIdentifier = matchingTags.Where(p => p.MatchesIdentifier(currentComponents.Count, this.Comparer, this.currentToken.Value)).ToArray();
                        if (matchesByIdentifier.Length > 0)
                        {
                            if (currentExpression == null)
                            {
                                var matchesByExpression = matchingTags.Where(p => p.MatchesExpression(currentComponents.Count)).ToArray();
                                if (matchesByExpression.Length > 0)
                                {
                                    currentExpression = this.CreateExpression();
                                    try
                                    {
                                        currentExpression.FeedSymbol(this.currentToken.Value);
                                    }
                                    catch (ExpressionException feedException)
                                    {
                                        ExceptionHelper.UnexpectedOrInvalidExpressionToken(feedException, this.currentToken);
                                    }

                                    currentComponents.Add(Tuple.Create(this.currentToken.Value, currentExpression));
                                }
                                else
                                {
                                    currentComponents.Add(this.currentToken.Value);
                                }

                                matchingTags = new HashSet<Tag>(matchesByIdentifier.Concat(matchesByExpression));
                            }
                            else
                            {
                                var previousComponentWasExpression = matchesByIdentifier.Any(p => p.MatchesExpression(currentComponents.Count - 1));
                                if (previousComponentWasExpression)
                                {
                                    Debug.Assert(currentExpression != null, "currentExpression cannot be null.");
                                    try
                                    {
                                        currentExpression.Construct();
                                        currentExpression = null;
                                    }
                                    catch (ExpressionException constructException)
                                    {
                                        ExceptionHelper.UnexpectedOrInvalidExpressionToken(constructException, this.currentToken);
                                    }
                                }

                                currentComponents.Add(this.currentToken.Value);
                            }

                            continue;
                        }
                    }
                }

                if (this.currentToken.Type == Token.TokenType.Word ||
                    this.currentToken.Type == Token.TokenType.Number ||
                    this.currentToken.Type == Token.TokenType.String)
                {
                    if (previousReadToken.Type != Token.TokenType.StartTag &&
                        previousReadToken.Type != Token.TokenType.Symbol &&
                        previousReadToken.Type != Token.TokenType.Whitespace)
                    {
                        ExceptionHelper.UnexpectedToken(this.currentToken);
                    }

                    if (currentExpression == null)
                    {
                        /* Starting an expression. */
                        matchingTags.RemoveWhere(p => !p.MatchesExpression(currentComponents.Count));
                        if (matchingTags.Count == 0)
                        {
                            ExceptionHelper.NoMatchingTagsLeft(currentComponents, this.currentToken);
                        }

                        currentExpression = this.CreateExpression();
                        currentComponents.Add(currentExpression);
                    }

                    try
                    {
                        if (this.currentToken.Type == Token.TokenType.Number)
                        {
                            double parsedDouble;
                            if (double.TryParse(this.currentToken.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedDouble))
                            {
                                currentExpression.FeedLiteral(parsedDouble);
                            }
                            else
                            {
                                ExceptionHelper.UnexpectedToken(this.currentToken);
                            }
                        }
                        else if (this.currentToken.Type == Token.TokenType.String)
                        {
                            currentExpression.FeedLiteral(this.currentToken.Value);
                        }
                        else
                        {
                            object keywordedLiteral;
                            if (this.specialConstants.TryGetValue(this.currentToken.Value, out keywordedLiteral))
                            {
                                currentExpression.FeedLiteral(keywordedLiteral);
                            }
                            else
                            {
                                currentExpression.FeedSymbol(this.currentToken.Value);
                            }
                        }
                    }
                    catch (ExpressionException feedException)
                    {
                        ExceptionHelper.UnexpectedOrInvalidExpressionToken(feedException, this.currentToken);
                    }
                }
                else if (this.currentToken.Type == Token.TokenType.Symbol)
                {
                    /* This must be part of an expression. */
                    if (currentExpression == null)
                    {
                        /* Starting an expression. */
                        matchingTags.RemoveWhere(p => !p.MatchesExpression(currentComponents.Count));
                        if (matchingTags.Count == 0)
                        {
                            ExceptionHelper.NoMatchingTagsLeft(currentComponents, this.currentToken);
                        }

                        currentExpression = this.CreateExpression();
                        currentComponents.Add(currentExpression);
                    }

                    this.InterpretSymbolChainToken(currentExpression, this.currentToken);
                }
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private bool NextToken()
        {
            if (this.endOfStream)
            {
                return false;
            }

            this.currentToken = this.Tokenizer.ReadNext();
            if (this.currentToken == null)
            {
                this.endOfStream = true;
            }

            return !this.endOfStream;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private Expression CreateExpression()
        {
            var expression = new Expression(this.expressionFlowSymbols, this.Comparer);

            foreach (var @operator in this.expressionOperators)
            {
                expression.RegisterOperator(@operator);
            }

            return expression;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private bool KnownSymbol(string symbol)
        {
            Debug.Assert(!string.IsNullOrEmpty(symbol), "symbol cannot be empty.");

            if (this.Comparer.Equals(symbol, this.expressionFlowSymbols.GroupClose) ||
                this.Comparer.Equals(symbol, this.expressionFlowSymbols.GroupOpen) ||
                this.Comparer.Equals(symbol, this.expressionFlowSymbols.Separator) ||
                this.Comparer.Equals(symbol, this.expressionFlowSymbols.MemberAccess))
            {
                return true;
            }

            foreach (var @operator in this.expressionOperators)
            {
                if (this.Comparer.Equals(@operator.Symbol, symbol))
                {
                    return true;
                }
            }

            return false;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private void InterpretSymbolChainToken(Expression expression, Token token)
        {
            var remainingSymbols = token.Value;

            while (remainingSymbols.Length > 0)
            {
                var found = false;

                for (var i = remainingSymbols.Length; i > 0; i--)
                {
                    var potentialOperator = remainingSymbols.Substring(0, i);
                    if (this.KnownSymbol(potentialOperator))
                    {
                        try
                        {
                            expression.FeedSymbol(potentialOperator);
                        }
                        catch (ExpressionException feedException)
                        {
                            ExceptionHelper.UnexpectedOrInvalidExpressionToken(feedException, token);
                        }

                        found = true;
                        remainingSymbols = remainingSymbols.Substring(i);
                        break;
                    }
                }

                if (!found)
                {
                    ExceptionHelper.UnexpectedToken(token);
                }
            }
        }
    }
}