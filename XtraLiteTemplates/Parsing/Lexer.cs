//
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
//

namespace XtraLiteTemplates.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Diagnostics;
    using System.Text;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Parsing;
    using System.IO;
    using System.Globalization;

    /// <summary>
    /// The lexical analyzer class. Requires an instance of <see cref="XtraLiteTemplates.Parsing.ITokenizer"/> to obtain all the tokens from. Based
    /// on said tokens, the <see cref="XtraLiteTemplates.Parsing.Lexer"/> identifies the correct lexical structures and generates <see cref="XtraLiteTemplates.Parsing.Lex"/> objects
    /// for the <see cref="XtraLiteTemplates.Evaluation.Interpreter"/>.
    /// </summary>
    public sealed class Lexer
    {
        private ExpressionFlowSymbols m_expressionFlowSymbols;
        private List<Operator> m_expressionOperators;
        private HashSet<String> m_unaryExpressionOperators;
        private HashSet<String> m_binaryExpressionOperators;
        private Dictionary<String, Object> m_specials;
        private List<Tag> m_tags;

        private Token m_currentToken;
        private Boolean m_isEndOfStream;

        private Boolean NextToken()
        {
            if (this.m_isEndOfStream)
                return false;

            this.m_currentToken = this.Tokenizer.ReadNext();
            if (this.m_currentToken == null)
                this.m_isEndOfStream = true;

            return !this.m_isEndOfStream;
        }

        private Expression CreateExpression()
        {
            var expression = new Expression(m_expressionFlowSymbols, Comparer);

            foreach (var @operator in m_expressionOperators)
                expression.RegisterOperator(@operator);

            return expression;
        }

        private Boolean KnownSymbol(String symbol)
        {
            Debug.Assert(!String.IsNullOrEmpty(symbol));

            if (Comparer.Equals(symbol, m_expressionFlowSymbols.GroupClose) ||
                Comparer.Equals(symbol, m_expressionFlowSymbols.GroupOpen) ||
                Comparer.Equals(symbol, m_expressionFlowSymbols.Separator) ||
                Comparer.Equals(symbol, m_expressionFlowSymbols.MemberAccess))
                return true;

            foreach (var @operator in m_expressionOperators)
            {
                if (Comparer.Equals(@operator.Symbol, symbol))
                    return true;
            }

            return false;
        }

        private void InterpretSymbolChainToken(Expression expression, Token token)
        {
            String _symbols = token.Value;

            while (_symbols.Length > 0)
            {
                Boolean found = false;

                for (var i = _symbols.Length; i > 0; i--)
                {
                    var potentialOperator = _symbols.Substring(0, i);
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
                        _symbols = _symbols.Substring(i);
                        break;
                    }
                }

                if (!found)
                    ExceptionHelper.UnexpectedToken(token);
            }
        }

        /// <summary>
        /// <value>The <see cref="XtraLiteTemplates.Parsing.ITokenizer"/> object used to read the tokens from the input template.</value>
        /// <remarks>The value of this property is provided by the caller during the construction process.</remarks>
        /// </summary>
        public ITokenizer Tokenizer { get; private set; }

        /// <summary>
        /// <value>The <see cref="System.Collections.Generic.IEqualityComparer{String}"/> object used to match keywords and identifiers.</value>
        /// <remarks>The value of this property is provided by the caller during the construction process.</remarks>
        /// </summary>
        public IEqualityComparer<String> Comparer { get; private set; }

        /// <summary>
        /// <value>A collection of <see cref="XtraLiteTemplates.Parsing.Tag"/> objects registered using <see cref="RegisterTag"/> method.</value>
        /// <remarks>The caller is responsible with loading up the known tag objects into the lexer before attempting to reading the first lex object.</remarks>
        /// </summary>
        public IReadOnlyCollection<Tag> Tags
        {
            get 
            {
                return m_tags;
            }
        }

        /// <summary>
        /// Creates an instance of <see cref="XtraLiteTemplates.Parsing.Lexer"/> class.
        /// </summary>
        /// <param name="tokenizer">The <see cref="XtraLiteTemplates.Parsing.ITokenizer"/> object used to read the tokens from the input template.</param>
        /// <param name="expressionFlowSymbols">The <see cref="XtraLiteTemplates.Expressions.ExpressionFlowSymbols"/> object containing the standard expression flow control symbols.</param>
        /// <param name="comparer">The <see cref="System.Collections.Generic.IEqualityComparer{String}"/> object used to match keywords and identifiers.</param>
        /// <exception cref="System.ArgumentNullException">Either <paramref name="tokenizer"/>, <paramref name="expressionFlowSymbols"/> or <paramref name="expressionFlowSymbols"/> are <c>null</c>.</exception>
        public Lexer(ITokenizer tokenizer, ExpressionFlowSymbols expressionFlowSymbols, IEqualityComparer<String> comparer)
        {
            Expect.NotNull("tokenizer", tokenizer);
            Expect.NotNull("comparer", comparer);
            Expect.NotNull("expressionFlowSymbols", expressionFlowSymbols);

            Tokenizer = tokenizer;
            Comparer = comparer;

            m_tags = new List<Tag>();
            m_expressionOperators = new List<Operator>();
            m_unaryExpressionOperators = new HashSet<String>(comparer);
            m_binaryExpressionOperators = new HashSet<String>(comparer);
            m_specials = new Dictionary<String, Object>();

            m_expressionFlowSymbols = expressionFlowSymbols;

            /* Register the flow symbols in */
            m_binaryExpressionOperators.Add(m_expressionFlowSymbols.Separator);
            m_binaryExpressionOperators.Add(m_expressionFlowSymbols.MemberAccess);
            m_unaryExpressionOperators.Add(m_expressionFlowSymbols.GroupOpen);
            m_binaryExpressionOperators.Add(m_expressionFlowSymbols.GroupClose);
        }

        /// <summary>
        /// Registers a know tag with this lexer instance. All registered tags will take part in the matching process during the analysis of the
        /// incoming tokens.
        /// </summary>
        /// <param name="tag">A <see cref="XtraLiteTemplates.Parsing.Tag"/> object to register.</param>
        /// <returns>This lexer instance.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="tag"/> is <c>null</c>.</exception>
        public Lexer RegisterTag(Tag tag)
        {
            Expect.NotNull("tag", tag);
            if (tag.ComponentCount == 0)
                ExceptionHelper.CannotRegisterTagWithNoComponents();

            /* Check for an equivalent tag in the list */
            foreach (var ot in m_tags)
            {
                if (ot.Equals(tag, Comparer))
                    return this;
            }

            m_tags.Add(tag);
            return this;
        }

        /// <summary>
        /// Registers a know operator with this lexer instance. All registered operators will take part in the matching process 
        /// during the expression analysis of the incoming tokens.
        /// </summary>
        /// <param name="@operator">A <see cref="XtraLiteTemplates.Expressions.Operators.Operator"/> object to register.</param>
        /// <returns>This lexer instance.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="@operator"/> is <c>null</c>.</exception>
        public Lexer RegisterOperator(Operator @operator)
        {
            Expect.NotNull("operator", @operator);

            var unaryOperator = @operator as UnaryOperator;
            if (unaryOperator != null)
            {
                if (m_unaryExpressionOperators.Contains(unaryOperator.Symbol) || 
                    m_specials.ContainsKey(unaryOperator.Symbol))
                    ExceptionHelper.OperatorAlreadyRegistered(@operator);
                else
                    m_unaryExpressionOperators.Add(unaryOperator.Symbol);
            }

            var binaryOperator = @operator as BinaryOperator;
            if (binaryOperator != null)
            {
                if (m_binaryExpressionOperators.Contains(binaryOperator.Symbol) ||
                    m_specials.ContainsKey(binaryOperator.Symbol))
                    ExceptionHelper.OperatorAlreadyRegistered(@operator);
                else
                    m_binaryExpressionOperators.Add(binaryOperator.Symbol);
            }

            m_expressionOperators.Add(@operator);

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
        /// <returns>This lexer instance.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="keyword"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="keyword"/> is not a valid identifier.</exception>
        /// <exception cref="System.InvalidOperationException"><paramref name="keyword"/> is already in use by an operator.</exception>
        public Lexer RegisterSpecial(String keyword, Object value)
        {
            Expect.Identifier("keyword", keyword);
            
            if (m_unaryExpressionOperators.Contains(keyword) ||
                m_binaryExpressionOperators.Contains(keyword))
            {
                ExceptionHelper.SpecialCannotBeRegistered(keyword);
            }

            m_specials[keyword] = value;
            return this;
        }

        /// <summary>
        /// Analizes a batch of tokens and generates a lex object. If the tokenizer reached the end of the stream, this method will
        /// return a <c>null</c> value. All subsequent calls to this method will also return <c>null</c>.
        /// </summary>
        /// <returns>An analyzed <see cref="XtraLiteTemplates.Parsing.Lex"/> object.</returns>
        /// <exception cref="XtraLiteTemplates.Parsing.ParseException">A parsing error encountered.</exception>
        /// <exception cref="XtraLiteTemplates.Expressions.ExpressionException">An expression contruction error encountered.</exception>
        public Lex ReadNext()
        {
            if (this.m_currentToken == null && !this.m_isEndOfStream)
                this.NextToken();

            if (this.m_isEndOfStream)
                return null;

            /* Load all unparsed tokens and merge them into a big component. */
            if (this.m_currentToken.Type == Token.TokenType.Unparsed)
            {
                var _unparsedTokens = new List<Token>();
                while (!this.m_isEndOfStream && this.m_currentToken.Type == Token.TokenType.Unparsed)
                {
                    _unparsedTokens.Add(this.m_currentToken);
                    this.NextToken();
                }

                return new UnparsedLex(String.Join(String.Empty, _unparsedTokens.Select(s => s.Value)), 
                    _unparsedTokens[0].CharacterIndex, _unparsedTokens.Sum(s => s.OriginalLength));
            }

            /* This is where a tag is parsed. */
            if (this.m_currentToken.Type != Token.TokenType.StartTag)
                ExceptionHelper.UnexpectedToken(this.m_currentToken);

            Int32 tokenIndex = -1;

            var matchingTags = new HashSet<Tag>(m_tags);
            List<Token> _allTokens = new List<Token>() { this.m_currentToken }; 
            List<Object> _components = new List<Object>();

            Expression currentExpression = null;
            while (true)
            {
                tokenIndex++;
                var _previousToken = this.m_currentToken;
                if (!this.NextToken())
                    ExceptionHelper.UnexpectedEndOfStreamAfterToken(_previousToken);

                _allTokens.Add(this.m_currentToken);

                if (this.m_currentToken.Type == Token.TokenType.EndTag)
                {
                    var matchingTag = matchingTags.Where(p => p.ComponentCount == _components.Count).FirstOrDefault();

                    if (matchingTag == null)
                        ExceptionHelper.NoMatchingTagsLeft(_components, this.m_currentToken);
                    else
                    {
                        if (matchingTag.MatchesExpression(_components.Count - 1))
                        {
                            Debug.Assert(currentExpression != null);
                            try
                            {
                                currentExpression.Construct();
                            }
                            catch (ExpressionException constructException)
                            {
                                ExceptionHelper.UnexpectedOrInvalidExpressionToken(constructException, m_currentToken);
                            }
                        }

                        Object[] actualComponents = new Object[_components.Count];
                        for (var i = 0; i < _components.Count; i++)
                        {
                            var component = _components[i];

                            var tuple = component as Tuple<String, Expression>;
                            if (tuple != null)
                            {
                                if (matchingTag.MatchesExpression(i))
                                {
                                    Debug.Assert(tuple.Item2.Constructed);
                                    actualComponents[i] = tuple.Item2;
                                }
                                else
                                    actualComponents[i] = tuple.Item1;
                            }
                            else
                            {
                                Debug.Assert(component is String || (component is Expression && ((Expression)component).Constructed));
                                actualComponents[i] = component;
                            }
                        }

                        NextToken();
                        return new TagLex(matchingTag, actualComponents, _allTokens[0].CharacterIndex, _allTokens.Sum(s => s.OriginalLength));
                    }
                }

                if (m_currentToken.Type == Token.TokenType.Word && 
                    !m_specials.ContainsKey(m_currentToken.Value))
                {
                    if (_previousToken.Type != Token.TokenType.StartTag &&
                        _previousToken.Type != Token.TokenType.Symbol && 
                        _previousToken.Type != Token.TokenType.Whitespace)
                        ExceptionHelper.UnexpectedToken(m_currentToken);

                    /* This is either a keyword or part of an expression. Reflect that. */
                    var matchesByKeyword = matchingTags.Where(p => p.MatchesKeyword(_components.Count, Comparer, this.m_currentToken.Value)).ToArray();
                    if (matchesByKeyword.Length > 0)
                    {
                        /* Keyword match. All the rest is now history. */
                        if (matchesByKeyword.Length > 0)
                            matchingTags = new HashSet<Tag>(matchesByKeyword);

                        var previousComponentWasExpression = matchingTags.Any(p => p.MatchesExpression(_components.Count - 1));
                        if (previousComponentWasExpression)
                        {
                            Debug.Assert(currentExpression != null);
                            try
                            {
                                currentExpression.Construct();
                            }
                            catch (ExpressionException constructException)
                            {
                                ExceptionHelper.UnexpectedOrInvalidExpressionToken(constructException, m_currentToken);
                            }
                        }

                        currentExpression = null;
                        _components.Add(this.m_currentToken.Value);
                        continue;
                    }
                    else
                    {
                        var matchesByIdentifier = matchingTags.Where(p => p.MatchesIdentifier(_components.Count, Comparer, this.m_currentToken.Value)).ToArray();
                        if (matchesByIdentifier.Length > 0)
                        {
                            if (currentExpression == null)
                            {
                                var matchesByExpression = matchingTags.Where(p => p.MatchesExpression(_components.Count)).ToArray();
                                if (matchesByExpression.Length > 0)
                                {
                                    currentExpression = CreateExpression();
                                    try
                                    {
                                        currentExpression.FeedSymbol(this.m_currentToken.Value);
                                    }
                                    catch (ExpressionException feedException)
                                    {
                                        ExceptionHelper.UnexpectedOrInvalidExpressionToken(feedException, this.m_currentToken);
                                    }

                                    _components.Add(Tuple.Create(this.m_currentToken.Value, currentExpression));
                                }
                                else
                                    _components.Add(this.m_currentToken.Value);

                                matchingTags = new HashSet<Tag>(matchesByIdentifier.Concat(matchesByExpression));
                            }
                            else
                            {
                                var previousComponentWasExpression = matchesByIdentifier.Any(p => p.MatchesExpression(_components.Count - 1));
                                if (previousComponentWasExpression)
                                {
                                    Debug.Assert(currentExpression != null);
                                    try
                                    {
                                        currentExpression.Construct();
                                        currentExpression = null;
                                    }
                                    catch (ExpressionException constructException)
                                    {
                                        ExceptionHelper.UnexpectedOrInvalidExpressionToken(constructException, m_currentToken);
                                    }
                                }

                                _components.Add(this.m_currentToken.Value);
                            }

                            continue;
                        }
                    }
                }

                if (m_currentToken.Type == Token.TokenType.Word ||
                    m_currentToken.Type == Token.TokenType.Number ||
                    m_currentToken.Type == Token.TokenType.String)
                {
                    if (_previousToken.Type != Token.TokenType.StartTag &&
                        _previousToken.Type != Token.TokenType.Symbol && 
                        _previousToken.Type != Token.TokenType.Whitespace)
                        ExceptionHelper.UnexpectedToken(m_currentToken);

                    if (currentExpression == null)
                    {
                        /* Starting an expression. */
                        matchingTags.RemoveWhere(p => !p.MatchesExpression(_components.Count));
                        if (matchingTags.Count == 0)
                            ExceptionHelper.NoMatchingTagsLeft(_components, this.m_currentToken);

                        currentExpression = CreateExpression();
                        _components.Add(currentExpression);
                    }

                    try
                    {
                        if (this.m_currentToken.Type == Token.TokenType.Number)
                        {
                            Double _float;
                            if (Double.TryParse(this.m_currentToken.Value, NumberStyles.Number, CultureInfo.InvariantCulture, out _float))
                                currentExpression.FeedLiteral(_float);
                            else
                                ExceptionHelper.UnexpectedToken(this.m_currentToken);
                        }
                        else if (this.m_currentToken.Type == Token.TokenType.String)
                        {
                            currentExpression.FeedLiteral(this.m_currentToken.Value);
                        }
                        else
                        {
                            Object keywordedLiteral;
                            if (m_specials.TryGetValue(this.m_currentToken.Value, out keywordedLiteral))
                                currentExpression.FeedLiteral(keywordedLiteral);
                            else
                                currentExpression.FeedSymbol(this.m_currentToken.Value);
                        }
                    }
                    catch (ExpressionException feedException)
                    {
                        ExceptionHelper.UnexpectedOrInvalidExpressionToken(feedException, this.m_currentToken);
                    }
                }
                else if (this.m_currentToken.Type == Token.TokenType.Symbol)
                {
                    /* This must be part of an expression. */
                    if (currentExpression == null)
                    {
                        /* Starting an expression. */
                        matchingTags.RemoveWhere(p => !p.MatchesExpression(_components.Count));
                        if (matchingTags.Count == 0)
                            ExceptionHelper.NoMatchingTagsLeft(_components, this.m_currentToken);

                        currentExpression = CreateExpression();
                        _components.Add(currentExpression);
                    }

                    InterpretSymbolChainToken(currentExpression, this.m_currentToken);
                }
            }
        }
    }
}

