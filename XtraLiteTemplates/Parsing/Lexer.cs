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
//     * Neither the name of the [ORGANIZATION] nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
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

    public sealed class Lexer
    {
        private List<Operator> m_expressionOperators;
        private HashSet<String> m_unaryExpressionOperators;
        private HashSet<String> m_binaryExpressionOperators;
        private List<Tag> m_tags;

        private Token m_currentToken;
        private Boolean m_isEndOfStream;

        public ITokenizer Tokenizer { get; private set; }

        public IEqualityComparer<String> Comparer { get; private set; }

        public IReadOnlyCollection<Tag> Tags
        {
            get 
            {
                return m_tags;
            }
        }

        public Lexer(ITokenizer tokenizer, IEqualityComparer<String> comparer)
        {
            Expect.NotNull("tokenizer", tokenizer);
            Expect.NotNull("comparer", comparer);

            Tokenizer = tokenizer;
            Comparer = comparer;

            m_tags = new List<Tag>();
            m_expressionOperators = new List<Operator>();
            m_unaryExpressionOperators = new HashSet<String>(comparer);
            m_binaryExpressionOperators = new HashSet<String>(comparer);
        }

        public Lexer RegisterTag(Tag tag)
        {
            Expect.NotNull("tag", tag);
            if (tag.ComponentCount == 0)
                ExceptionHelper.CannotRegisterTagWithNoComponents();

            if (!m_tags.Contains(tag))
                m_tags.Add(tag);

            return this;
        }

        public Lexer RegisterOperator(Operator @operator)
        {
            Expect.NotNull("operator", @operator);

            var unaryOperator = @operator as UnaryOperator;
            if (unaryOperator != null)
            {
                if (m_unaryExpressionOperators.Contains(unaryOperator.Symbol))
                    ExceptionHelper.OperatorAlreadyRegistered(@operator);
                else
                    m_unaryExpressionOperators.Add(unaryOperator.Symbol);
            }

            var binaryOperator = @operator as BinaryOperator;
            if (binaryOperator != null)
            {
                if (m_binaryExpressionOperators.Contains(binaryOperator.Symbol))
                    ExceptionHelper.OperatorAlreadyRegistered(@operator);
                else
                    m_binaryExpressionOperators.Add(binaryOperator.Symbol);
            }

            var subscriptOperator = @operator as SubscriptOperator;
            if (subscriptOperator != null)
            {
                if (m_unaryExpressionOperators.Contains(subscriptOperator.Symbol) ||
                  m_binaryExpressionOperators.Contains(subscriptOperator.Terminator))
                    ExceptionHelper.OperatorAlreadyRegistered(@operator);
                else
                {
                    m_unaryExpressionOperators.Add(subscriptOperator.Symbol);
                    m_binaryExpressionOperators.Add(subscriptOperator.Terminator);
                }
            }

            m_expressionOperators.Add(@operator);

            return this;
        }

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
            var expression = new Expression(Comparer);
            foreach (var @operator in m_expressionOperators)
                expression.RegisterOperator(@operator);

            return expression;
        }

        private Boolean KnownSymbol(String symbolChain)
        {
            Debug.Assert(!String.IsNullOrEmpty(symbolChain));

            foreach (var @operator in m_expressionOperators)
            {
                if (Comparer.Equals(@operator.Symbol, symbolChain))
                    return true;

                var subscriptOperator = @operator as SubscriptOperator;
                if (subscriptOperator != null && Comparer.Equals(subscriptOperator.Terminator, symbolChain))
                    return true;
            }

            return false;
        }

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
            List<Token> _symbolChain = new List<Token>();
            List<Object> _components = new List<Object>();

            Expression currentExpression = null;
            while (true)
            {
                tokenIndex++;
                var _previousToken = this.m_currentToken;
                if (!this.NextToken())
                    ExceptionHelper.UnexpectedEndOfStreamAfterToken(_previousToken);

                _allTokens.Add(this.m_currentToken);

                if (m_currentToken.Type != Token.TokenType.Symbol && _symbolChain.Count > 0)
                {
                    /* We have a chain of symbols before this token camne to life. Need to interpret them only as potential operators in the expression. */
                    while (_symbolChain.Count > 0)
                    {
                        Boolean found = false;
                        for (var i = _symbolChain.Count; i > 0; i++)
                        {
                            var potentialOperator = String.Join(String.Empty, _symbolChain.Take(i).Select(s => s.Value));
                            if (KnownSymbol(potentialOperator))
                            {
                                try
                                {
                                    currentExpression.FeedSymbol(potentialOperator);
                                }
                                catch (ExpressionException feedException)
                                {
                                    var mergedToken = new Token(Token.TokenType.Symbol,
                                        potentialOperator, _symbolChain[0].CharacterIndex, potentialOperator.Length);
                                    ExceptionHelper.UnexpectedOrInvalidExpressionToken(feedException, mergedToken);
                                }

                                found = true;
                                _symbolChain.RemoveRange(0, i);
                                break;
                            }
                        }

                        if (!found)
                            ExceptionHelper.UnexpectedToken(_symbolChain[0]);
                    }
                }

                if (this.m_currentToken.Type == Token.TokenType.EndTag)
                {
                    var matchingTag = matchingTags.Where(p => p.ComponentCount == _components.Count).FirstOrDefault();

                    if (matchingTag == null)
                        ExceptionHelper.UnexpectedToken(this.m_currentToken);
                    else
                    {
                        if (currentExpression != null)
                        {
                            try
                            {
                                currentExpression.Construct();
                            }
                            catch (ExpressionException constructException)
                            {
                                ExceptionHelper.UnexpectedOrInvalidExpressionToken(constructException, m_currentToken);
                            }
                        }

                        NextToken();
                        return new TagLex(matchingTag, _components.ToArray(), _allTokens[0].CharacterIndex, _allTokens.Sum(s => s.OriginalLength));
                    }
                }

                if (m_currentToken.Type == Token.TokenType.Word)
                {
                    if (_previousToken.Type != Token.TokenType.StartTag &&
                        _previousToken.Type != Token.TokenType.Symbol && 
                        _previousToken.Type != Token.TokenType.Whitespace)
                        ExceptionHelper.UnexpectedToken(m_currentToken);

                    /* This is either a keyword or part of an expression. Reflect that. */
                    var matchesByKeyword = matchingTags.Where(p => p.MatchesKeyword(_components.Count, Comparer, this.m_currentToken.Value)).ToList();
                    var matchesByIdentifier = matchingTags.Where(p => p.MatchesIdentifier(_components.Count, Comparer, this.m_currentToken.Value)).ToList();

                    if (matchesByKeyword.Count > 0 || matchesByIdentifier.Count > 0)
                    {
                        /* Keyword or identifier (prioritize the keyword matches) */
                        if (matchesByKeyword.Count > 0)
                            matchingTags = new HashSet<Tag>(matchesByKeyword);
                        else
                            matchingTags = new HashSet<Tag>(matchesByIdentifier);

                        if (currentExpression != null)
                        {
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

                        /* Keyword si the next component. */
                        _components.Add(this.m_currentToken.Value);
                        continue;
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
                            ExceptionHelper.UnexpectedToken(this.m_currentToken);

                        currentExpression = CreateExpression();
                        _components.Add(currentExpression);
                    }

                    try
                    {
                        if (this.m_currentToken.Type == Token.TokenType.Number)
                        {
                            Int64 _integer;
                            if (Int64.TryParse(this.m_currentToken.Value, out _integer))
                                currentExpression.FeedLiteral(_integer);
                            else
                            {
                                Double _float;
                                if (Double.TryParse(this.m_currentToken.Value, out _float))
                                    currentExpression.FeedLiteral(_float);
                                else
                                    ExceptionHelper.UnexpectedToken(this.m_currentToken);
                            }
                        }
                        else if (this.m_currentToken.Type == Token.TokenType.String)
                        {
                            currentExpression.FeedLiteral(this.m_currentToken.Value);
                        }
                        else
                        {
                            currentExpression.FeedSymbol(this.m_currentToken.Value);
                        }
                    }
                    catch (ExpressionException feedException)
                    {
                        ExceptionHelper.UnexpectedOrInvalidExpressionToken(feedException, this.m_currentToken);
                    }
                }
                else if (m_currentToken.Type == Token.TokenType.Symbol)
                {
                    /* This must be part of an expression. */
                    if (currentExpression == null)
                    {
                        /* Starting an expression. */
                        matchingTags.RemoveWhere(p => !p.MatchesExpression(_components.Count));
                        if (matchingTags.Count == 0)
                            ExceptionHelper.UnexpectedToken(this.m_currentToken);

                        currentExpression = CreateExpression();
                        _components.Add(currentExpression);
                    }

                    _symbolChain.Add(m_currentToken);
                }
            }
        }
    }
}

