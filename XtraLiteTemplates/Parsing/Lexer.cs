//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2017, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    using JetBrains.Annotations;

    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;

    /// <summary>
    /// The lexical analyzer class. Requires an instance of <see cref="ITokenizer"/> to obtain all the tokens from. Based
    /// on said tokens, the <see cref="Lexer"/> identifies the correct lexical structures and generates <see cref="Lex"/> objects
    /// for the <see cref="Evaluation.Interpreter"/>.
    /// </summary>
    [PublicAPI]
    public sealed class Lexer
    {
        [NotNull]
        private readonly ExpressionFlowSymbols _expressionFlowSymbols;
        [NotNull]
        [ItemNotNull]
        private readonly List<Operator> _expressionOperators;
        [NotNull]
        private readonly HashSet<string> _unaryExpressionOperators;
        [NotNull]
        private readonly HashSet<string> _binaryExpressionOperators;
        [NotNull]
        private readonly Dictionary<string, object> _specialConstants;
        [NotNull]
        [ItemNotNull]
        private readonly List<Tag> _registeredTags;
        [CanBeNull]
        private Token _currentToken;
        private bool _endOfStream;

        [NotNull]
        private Token CurrentValidToken
        {
            get
            {
                Debug.Assert(_currentToken != null);
                return _currentToken;
            }
        }

        [NotNull]
        private string CurrentValidTokenValue
        {
            get
            {
                var value = CurrentValidToken.Value;
                Debug.Assert(value != null);
                return value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Lexer"/> class.
        /// </summary>
        /// <param name="tokenizer">The <see cref="ITokenizer" /> object used to read the tokens from the input template.</param>
        /// <param name="expressionFlowSymbols">The <see cref="ExpressionFlowSymbols" /> object containing the standard expression flow control symbols.</param>
        /// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{String}" /> object used to match keywords and identifiers.</param>
        /// <exception cref="System.ArgumentNullException">Either <paramref name="tokenizer" />, <paramref name="expressionFlowSymbols" /> or <paramref name="expressionFlowSymbols" /> are <c>null</c>.</exception>
        public Lexer(
            [NotNull] ITokenizer tokenizer,
            [NotNull] ExpressionFlowSymbols expressionFlowSymbols,
            [NotNull] IEqualityComparer<string> comparer)
        {
            Expect.NotNull(nameof(tokenizer), tokenizer);
            Expect.NotNull(nameof(comparer), comparer);
            Expect.NotNull(nameof(expressionFlowSymbols), expressionFlowSymbols);

            Tokenizer = tokenizer;
            Comparer = comparer;

            _registeredTags = new List<Tag>();
            _expressionOperators = new List<Operator>();
            _unaryExpressionOperators = new HashSet<string>(comparer);
            _binaryExpressionOperators = new HashSet<string>(comparer);
            _specialConstants = new Dictionary<string, object>(comparer);

            _expressionFlowSymbols = expressionFlowSymbols;

            /* Register the flow symbols in */
            _binaryExpressionOperators.Add(expressionFlowSymbols.Separator);
            _binaryExpressionOperators.Add(expressionFlowSymbols.MemberAccess);
            _unaryExpressionOperators.Add(expressionFlowSymbols.GroupOpen);
            _binaryExpressionOperators.Add(expressionFlowSymbols.GroupClose);
        }

        /// <summary>
        /// Gets the <see cref="ITokenizer" /> object used to read the tokens from the input template.
        /// </summary>
        /// <value>
        /// The input template <c>tokenizer</c>.
        /// </value>
        /// <remarks>
        /// The value of this property is provided by the caller during the construction process.
        /// </remarks>
        [NotNull]
        public ITokenizer Tokenizer { get; }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{String}" /> object used to match keywords and identifiers.
        /// </summary>
        /// <value>
        /// The identifier comparer.
        /// </value>
        /// <remarks>
        /// The value of this property is provided by the caller during the construction process.
        /// </remarks>
        [NotNull]
        public IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Gets all the registered <see cref="Parsing.Tag" /> objects.
        /// </summary>
        /// <value>
        /// The registered tags.
        /// </value>
        /// <remarks>
        /// The caller is responsible with loading up the known tag objects into the <c>lexer</c> before attempting to reading the first <c>lex</c> object.
        /// </remarks>
        [NotNull]
        [ItemNotNull]
        public IReadOnlyCollection<Tag> Tags => _registeredTags;

        /// <summary>
        /// Registers a know tag with this <c>lexer</c> instance. All registered tags will take part in the matching process during the analysis of the
        /// incoming tokens.
        /// </summary>
        /// <param name="tag">A <see cref="Parsing.Tag"/> object to register.</param>
        /// <returns>This <c>lexer</c> instance.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="tag"/> is <c>null</c>.</exception>
        [NotNull]
        public Lexer RegisterTag([NotNull] Tag tag)
        {
            Expect.NotNull(nameof(tag), tag);
            if (tag.ComponentCount == 0)
            {
                ExceptionHelper.CannotRegisterTagWithNoComponents();
            }

            /* Check for an equivalent tag in the list */
            if (_registeredTags.Any(ot => ot.Equals(tag, Comparer)))
            {
                return this;
            }

            _registeredTags.Add(tag);
            return this;
        }

        /// <summary>
        /// Registers a know operator with this <c>lexer</c> instance. All registered operators will take part in the matching process 
        /// during the expression analysis of the incoming tokens.
        /// </summary>
        /// <param name="operator">A <see cref="XtraLiteTemplates.Expressions.Operators.Operator"/> object to register.</param>
        /// <returns>This <c>lexer</c> instance.</returns>
        /// <exception cref="System.ArgumentNullException">Argument <paramref name="operator"/> is <c>null</c>.</exception>
        [NotNull]
        public Lexer RegisterOperator([NotNull] Operator @operator)
        {
            Expect.NotNull(nameof(@operator), @operator);

            var unaryOperator = @operator as UnaryOperator;
            if (unaryOperator != null)
            {
                if (_unaryExpressionOperators.Contains(unaryOperator.Symbol) || _specialConstants.ContainsKey(unaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(@operator);
                }
                else
                {
                    _unaryExpressionOperators.Add(unaryOperator.Symbol);
                }
            }

            var binaryOperator = @operator as BinaryOperator;
            if (binaryOperator != null)
            {
                if (_binaryExpressionOperators.Contains(binaryOperator.Symbol) || _specialConstants.ContainsKey(binaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(@operator);
                }
                else
                {
                    _binaryExpressionOperators.Add(binaryOperator.Symbol);
                }
            }

            _expressionOperators.Add(@operator);

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
        [NotNull]
        public Lexer RegisterSpecial([NotNull] string keyword, [CanBeNull] object value)
        {
            Expect.Identifier(nameof(keyword), keyword);

            if (_unaryExpressionOperators.Contains(keyword) || _binaryExpressionOperators.Contains(keyword))
            {
                ExceptionHelper.SpecialCannotBeRegistered(keyword);
            }

            _specialConstants[keyword] = value;
            return this;
        }

        /// <summary>
        /// Analyzes a batch of tokens and generates a <c>lex</c> object. If the <c>tokenizer</c> reached the end of the stream, this method will
        /// return a <c>null</c> value. All subsequent calls to this method will also return <c>null</c>.
        /// </summary>
        /// <returns>An analyzed <see cref="Lex"/> object.</returns>
        /// <exception cref="ParseException">A parsing error encountered.</exception>
        /// <exception cref="ExpressionException">An expression construction error encountered.</exception>
        [CanBeNull]
        public Lex ReadNext()
        {
            if (_currentToken == null && !_endOfStream)
            {
                NextToken();
            }

            if (_endOfStream)
            {
                return null;
            }

            /* Load all un-parsed tokens and merge them into a big component. */
            if (CurrentValidToken.Type == Token.TokenType.UnParsed)
            {
                var unParsedTokens = new List<Token>();
                while (!_endOfStream && CurrentValidToken.Type == Token.TokenType.UnParsed)
                {
                    unParsedTokens.Add(_currentToken);
                    NextToken();
                }

                return new UnParsedLex(
                    string.Join(string.Empty, unParsedTokens.Select(s => s.Value)), 
                    unParsedTokens[0].CharacterIndex, 
                    unParsedTokens.Sum(s => s.OriginalLength));
            }

            /* This is where a tag is parsed. */
            if (CurrentValidToken.Type != Token.TokenType.StartTag)
            {
                ExceptionHelper.UnexpectedToken(CurrentValidToken);
            }

            var matchingTags = new HashSet<Tag>(_registeredTags);
            var allTagTokens = new List<Token> { _currentToken };
            var currentComponents = new List<object>();

            Expression currentExpression = null;
            while (true)
            {
                var previousReadToken = CurrentValidToken;
                if (!NextToken())
                {
                    ExceptionHelper.NoMatchingTagsLeft(currentComponents, previousReadToken);
                }

                allTagTokens.Add(_currentToken);

                if (CurrentValidToken.Type == Token.TokenType.EndTag)
                {
                    var matchingTag = matchingTags.FirstOrDefault(p => p.ComponentCount == currentComponents.Count);

                    if (matchingTag == null)
                    {
                        ExceptionHelper.NoMatchingTagsLeft(currentComponents, CurrentValidToken);
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
                                ExceptionHelper.UnexpectedOrInvalidExpressionToken(constructException, CurrentValidToken);
                            }
                        }

                        var actualComponents = new object[currentComponents.Count];
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

                        NextToken();
                        return new TagLex(matchingTag, actualComponents, allTagTokens[0].CharacterIndex, allTagTokens.Sum(s => s.OriginalLength));
                    }
                }

                if (CurrentValidToken.Type == Token.TokenType.Word &&
                    !_specialConstants.ContainsKey(CurrentValidTokenValue))
                {
                    if (previousReadToken.Type != Token.TokenType.StartTag &&
                        previousReadToken.Type != Token.TokenType.Symbol &&
                        previousReadToken.Type != Token.TokenType.Whitespace)
                    {
                        ExceptionHelper.UnexpectedToken(CurrentValidToken);
                    }

                    /* This is either a keyword or part of an expression. Reflect that. */
                    var matchesByKeyword = matchingTags.Where(p => p.MatchesKeyword(currentComponents.Count, Comparer, CurrentValidTokenValue)).ToArray();
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
                                ExceptionHelper.UnexpectedOrInvalidExpressionToken(constructException, CurrentValidToken);
                            }
                        }

                        currentExpression = null;
                        currentComponents.Add(CurrentValidToken.Value);
                        continue;
                    }

                    var matchesByIdentifier = matchingTags.Where(p => p.MatchesIdentifier(currentComponents.Count, Comparer, CurrentValidTokenValue)).ToArray();
                    if (matchesByIdentifier.Length > 0)
                    {
                        if (currentExpression == null)
                        {
                            var matchesByExpression = matchingTags.Where(p => p.MatchesExpression(currentComponents.Count)).ToArray();
                            if (matchesByExpression.Length > 0)
                            {
                                currentExpression = CreateExpression();
                                try
                                {
                                    currentExpression.FeedSymbol(CurrentValidTokenValue);
                                }
                                catch (ExpressionException feedException)
                                {
                                    ExceptionHelper.UnexpectedOrInvalidExpressionToken(feedException, CurrentValidToken);
                                }

                                currentComponents.Add(Tuple.Create(CurrentValidToken.Value, currentExpression));
                            }
                            else
                            {
                                currentComponents.Add(CurrentValidToken.Value);
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
                                    ExceptionHelper.UnexpectedOrInvalidExpressionToken(constructException, CurrentValidToken);
                                }
                            }

                            currentComponents.Add(CurrentValidToken.Value);
                        }

                        continue;
                    }
                }

                switch (CurrentValidToken.Type)
                {
                    case Token.TokenType.Word:
                    case Token.TokenType.Number:
                    case Token.TokenType.String:
                        if (previousReadToken.Type != Token.TokenType.StartTag &&
                            previousReadToken.Type != Token.TokenType.Symbol &&
                            previousReadToken.Type != Token.TokenType.Whitespace)
                        {
                            ExceptionHelper.UnexpectedToken(CurrentValidToken);
                        }

                        if (currentExpression == null)
                        {
                            /* Starting an expression. */
                            matchingTags.RemoveWhere(p => !p.MatchesExpression(currentComponents.Count));
                            if (matchingTags.Count == 0)
                            {
                                ExceptionHelper.NoMatchingTagsLeft(currentComponents, CurrentValidToken);
                            }

                            currentExpression = CreateExpression();
                            currentComponents.Add(currentExpression);
                        }

                        try
                        {
                            switch (CurrentValidToken.Type)
                            {
                                case Token.TokenType.Number:
                                    double parsedDouble;
                                    if (double.TryParse(CurrentValidToken.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out parsedDouble))
                                    {
                                        currentExpression.FeedLiteral(parsedDouble);
                                    }
                                    else
                                    {
                                        ExceptionHelper.UnexpectedToken(CurrentValidToken);
                                    }
                                    break;
                                case Token.TokenType.String:
                                    currentExpression.FeedLiteral(CurrentValidToken.Value);
                                    break;
                                default:
                                    object keyLiteral;
                                    if (_specialConstants.TryGetValue(CurrentValidTokenValue, out keyLiteral))
                                    {
                                        currentExpression.FeedLiteral(keyLiteral);
                                    }
                                    else
                                    {
                                        currentExpression.FeedSymbol(CurrentValidTokenValue);
                                    }
                                    break;
                            }
                        }
                        catch (ExpressionException feedException)
                        {
                            ExceptionHelper.UnexpectedOrInvalidExpressionToken(feedException, CurrentValidToken);
                        }
                        break;
                    case Token.TokenType.Symbol:
                        /* This must be part of an expression. */
                        if (currentExpression == null)
                        {
                            /* Starting an expression. */
                            matchingTags.RemoveWhere(p => !p.MatchesExpression(currentComponents.Count));
                            if (matchingTags.Count == 0)
                            {
                                ExceptionHelper.NoMatchingTagsLeft(currentComponents, CurrentValidToken);
                            }

                            currentExpression = CreateExpression();
                            currentComponents.Add(currentExpression);
                        }

                        InterpretSymbolChainToken(currentExpression, CurrentValidToken);
                        break;
                }
            }
        }

        private bool NextToken()
        {
            if (_endOfStream)
            {
                return false;
            }

            _currentToken = Tokenizer.ReadNext();
            if (_currentToken == null)
            {
                _endOfStream = true;
            }

            return !_endOfStream;
        }

        [NotNull]
        private Expression CreateExpression()
        {
            var expression = new Expression(_expressionFlowSymbols, Comparer);

            foreach (var @operator in _expressionOperators)
            {
                expression.RegisterOperator(@operator);
            }

            return expression;
        }

        private bool KnownSymbol([NotNull] string symbol)
        {
            Debug.Assert(!string.IsNullOrEmpty(symbol), "symbol cannot be empty.");

            if (Comparer.Equals(symbol, _expressionFlowSymbols.GroupClose) || Comparer.Equals(symbol, _expressionFlowSymbols.GroupOpen) || Comparer.Equals(symbol, _expressionFlowSymbols.Separator) || Comparer.Equals(symbol, _expressionFlowSymbols.MemberAccess))
            {
                return true;
            }

            return _expressionOperators.Any(@operator => Comparer.Equals(@operator.Symbol, symbol));
        }

        private void InterpretSymbolChainToken([NotNull] Expression expression, [NotNull] Token token)
        {
            Debug.Assert(expression != null);
            Debug.Assert(token != null);

            var remainingSymbols = token.Value;
            Debug.Assert(remainingSymbols != null);

            while (remainingSymbols.Length > 0)
            {
                var found = false;

                for (var i = remainingSymbols.Length; i > 0; i--)
                {
                    var potentialOperator = remainingSymbols.Substring(0, i);
                    if (KnownSymbol(potentialOperator))
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