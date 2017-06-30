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

namespace XtraLiteTemplates.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using JetBrains.Annotations;
    using Nodes;
    using Operators;
    using LinqExpression = System.Linq.Expressions.Expression;

    /// <summary>
    /// Provides core expression construction and evaluation functionality.
    /// </summary>
    [PublicAPI]
    public sealed class Expression
    {
        [NotNull]
        [ItemNotNull]
        private readonly List<Operator> _registeredOperators;
        [NotNull]
        private readonly Dictionary<string, UnaryOperator> _unaryOperatorSymbols;
        [NotNull]
        private readonly Dictionary<string, BinaryOperator> _binaryOperatorSymbols;
        [CanBeNull]
        private ExpressionNode _currentNode;
        [CanBeNull]
        private RootNode _currentGroupRootNode;
        [CanBeNull]
        private Func<IExpressionEvaluationContext, object> _evaluationFunction;

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// </summary>
        /// <param name="flowSymbols">The flow symbols.</param>
        /// <param name="comparer">The symbol and identifier comparer.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="flowSymbols"/> or <paramref name="comparer"/> is <c>null</c>.</exception>
        public Expression([NotNull] ExpressionFlowSymbols flowSymbols, [NotNull] IEqualityComparer<string> comparer)
        {
            Expect.NotNull(nameof(comparer), comparer);
            Expect.NotNull(nameof(flowSymbols), flowSymbols);

            FlowSymbols = flowSymbols;
            _unaryOperatorSymbols = new Dictionary<string, UnaryOperator>(comparer);
            _binaryOperatorSymbols = new Dictionary<string, BinaryOperator>(comparer);
            _registeredOperators = new List<Operator>();
            Comparer = comparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class using the default flow symbols and a culture-invariant, case-insensitive comparer.
        /// </summary>
        public Expression()
            : this(ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Gets all supported operators.
        /// </summary>
        /// <value>
        /// The supported operators.
        /// </value>
        [NotNull]
        public IReadOnlyList<Operator> SupportedOperators => _registeredOperators;

        /// <summary>
        /// Gets the flow symbols.
        /// </summary>
        /// <value>
        /// The flow symbols.
        /// </value>
        [NotNull]
        public ExpressionFlowSymbols FlowSymbols { get; }

        /// <summary>
        /// Gets the comparer used to compare symbols and identifiers.
        /// </summary>
        /// <value>
        /// The symbol and identifier comparer.
        /// </value>
        [NotNull]
        public IEqualityComparer<string> Comparer { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="Expression"/> is constructed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if constructed; otherwise, <c>false</c>.
        /// </value>
        public bool Constructed => _evaluationFunction != null;

        /// <summary>
        /// Gets a value indicating whether the construction of this <see cref="Expression"/> has started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if started; otherwise, <c>false</c>.
        /// </value>
        public bool Started => _currentNode != null;

        /// <summary>
        /// Determines whether the specified <paramref name="symbol" /> is a supported operator.
        /// </summary>
        /// <param name="symbol">The symbol to verify.</param>
        /// <returns>
        ///   <c>true</c> if the symbol is supported; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="symbol"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="symbol"/> is empty.</exception>
        public bool IsSupportedOperator([NotNull] string symbol)
        {
            Expect.NotEmpty(nameof(symbol), symbol);

            return _unaryOperatorSymbols.ContainsKey(symbol) || _binaryOperatorSymbols.ContainsKey(symbol);
        }

        /// <summary>
        /// Registers an operator with this expression.
        /// </summary>
        /// <param name="operator">The operator to register.</param>
        /// <returns>This expression instance.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="operator"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Expression under construction or the symbol of <paramref name="operator"/> already registered.</exception>
        [NotNull]
        public Expression RegisterOperator([NotNull] Operator @operator)
        {
            Expect.NotNull(nameof(@operator), @operator);

            if (Started)
            {
                ExceptionHelper.CannotRegisterOperatorsForStartedExpression();
            }

            Debug.Assert(!Constructed, "must not be constructed.");

            /* Standards. */
            if (@operator.Symbol == FlowSymbols.Separator ||
                @operator.Symbol == FlowSymbols.GroupClose ||
                @operator.Symbol == FlowSymbols.GroupOpen ||
                @operator.Symbol == FlowSymbols.MemberAccess)
            {
                ExceptionHelper.OperatorAlreadyRegistered(@operator);
            }

            var unaryOperator = @operator as UnaryOperator;
            if (unaryOperator != null)
            {
                if (_unaryOperatorSymbols.ContainsKey(unaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(unaryOperator);
                }

                _unaryOperatorSymbols.Add(unaryOperator.Symbol, unaryOperator);
            }
            else if (@operator is BinaryOperator)
            {
                var binaryOperator = (BinaryOperator)@operator;
                if (_binaryOperatorSymbols.ContainsKey(binaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(binaryOperator);
                }

                _binaryOperatorSymbols.Add(@operator.Symbol, binaryOperator);
            }
            else
            {
                Debug.Fail("Unsupported operator type.");
            }

            _registeredOperators.Add(@operator);
            return this;
        }

        /// <summary>
        /// Feeds a literal value into the expression.
        /// </summary>
        /// <param name="literal">The literal value.</param>
        /// <returns>
        /// This expression instance.
        /// </returns>
        /// <exception cref="ExpressionException">An expression construction error detected.</exception>
        /// <exception cref="InvalidOperationException">Expression construction finalized.</exception>
        [NotNull]
        public Expression FeedLiteral([CanBeNull] object literal)
        {
            if (Constructed)
            {
                ExceptionHelper.CannotModifyAConstructedExpression();
            }

            FeedTerm(literal, true);
            return this;
        }

        /// <summary>
        /// Feeds a symbol into the expression.
        /// </summary>
        /// <param name="symbol">The symbol value.</param>
        /// <returns>
        /// This expression instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="symbol"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="symbol"/> is empty.</exception>
        /// <exception cref="ExpressionException">An expression construction error detected.</exception>
        /// <exception cref="InvalidOperationException">Expression construction finalized.</exception>
        [NotNull]
        public Expression FeedSymbol([NotNull] string symbol)
        {
            Expect.NotEmpty(nameof(symbol), symbol);

            if (Constructed)
            {
                ExceptionHelper.CannotModifyAConstructedExpression();
            }

            FeedTerm(symbol, false);
            return this;
        }

        /// <summary>
        /// Finalize the construction of the expression.
        /// </summary>
        /// <exception cref="InvalidOperationException">Expression construction not started.</exception>
        /// <exception cref="ExpressionException">Expression in unbalanced state.</exception>
        public void Construct()
        {
            if (!Started)
            {
                ExceptionHelper.CannotConstructExpressionInvalidState();
            }

            if (!Constructed)
            {
                bool fail;
                if (_currentGroupRootNode.Parent != null)
                {
                    fail = true;
                }
                else
                {
                    var currentRootNode = _currentNode as RootNode;
                    if (currentRootNode != null)
                    {
                        fail = !currentRootNode.Closed;
                    }
                    else
                    {
                        var currentReferenceNode = _currentNode as ReferenceNode;
                        if (currentReferenceNode != null)
                        {
                            fail = currentReferenceNode.Identifier == null;
                        }
                        else
                        {
                            fail = !(_currentNode is LeafNode);
                        }
                    }
                }

                if (fail)
                {
                    ExceptionHelper.CannotConstructExpressionInvalidState();
                }

                /* Reduce expression.*/
                _currentGroupRootNode.Reduce(ReduceExpressionEvaluationContext.Instance);

                /* Obtain the LINQ expression and generate a lambda out of it (for consumption). */
                var linqExpression = _currentGroupRootNode.GetEvaluationLinqExpression();
                _evaluationFunction = LinqExpression.Lambda<Func<IExpressionEvaluationContext, object>>(
                    linqExpression, LinqExpressionHelper.ExpressionParameterContext).Compile();
            }
        }

        /// <summary>
        /// Evaluates this expression using an evaluation context.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <returns>The result of expression evaluation.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Expression not constructed.</exception>
        [CanBeNull]
        public object Evaluate([NotNull] IExpressionEvaluationContext context)
        {
            Expect.NotNull(nameof(context), context);

            if (!Constructed)
            {
                ExceptionHelper.CannotEvaluateUnConstructedExpression();
            }

            return _evaluationFunction(context);
        }

        /// <summary>
        /// Returns a human-readable representation this expression instance using the <see cref="ExpressionFormatStyle.Arithmetic"/> formatting.
        /// </summary>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return ToString(ExpressionFormatStyle.Arithmetic);
        }

        /// <summary>
        /// Returns a human-readable representation this expression instance using the specified formatting.
        /// </summary>
        /// <param name="style">The formatting style to use.</param>
        /// <returns>
        /// A <see cref="string" /> that represents this instance.
        /// </returns>
        public string ToString(ExpressionFormatStyle style)
        {
            return _currentGroupRootNode == null ? "??" : _currentGroupRootNode.ToString(style);
        }

        private void OpenNewGroup()
        {
            if (!_currentNode.Continuity.HasFlag(PermittedContinuations.NewGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(FlowSymbols.GroupOpen);
            }

            var currentReferenceNode = _currentNode as ReferenceNode;
            if (currentReferenceNode != null)
            {
                Debug.Assert(currentReferenceNode.Identifier != null, "current reference node's identifier must not be null.");

                /* Flip to the new root */
                _currentGroupRootNode = new RootNode(currentReferenceNode, true);
                currentReferenceNode.Arguments = _currentGroupRootNode;
                _currentNode = _currentGroupRootNode;
            }
            else if (_currentNode is OperatorNode)
            {
                var currentOperatorNode = (OperatorNode)_currentNode;
                Debug.Assert(currentOperatorNode.RightNode == null, "current operator node's right node must be null.");

                /* Flip to the new root */
                _currentGroupRootNode = new RootNode(currentOperatorNode, false);
                currentOperatorNode.RightNode = _currentGroupRootNode;
                _currentNode = _currentGroupRootNode;
            }
            else if (_currentNode is RootNode)
            {
                var currentRootNode = (RootNode)_currentNode;

                Debug.Assert(currentRootNode == _currentGroupRootNode, "current operator node must be the current root node.");
                Debug.Assert(!currentRootNode.Closed, "current root node cannot be closed.");

                /* Flip to the new root */
                _currentGroupRootNode = new RootNode(currentRootNode, false);
                currentRootNode.AddChild(_currentGroupRootNode);
                _currentNode = _currentGroupRootNode;
            }
        }

        private void CloseExistingGroup()
        {
            /* Special case just here! */
            if (_currentGroupRootNode.Parent == null)
            {
                ExceptionHelper.InvalidExpressionTerm(FlowSymbols.GroupClose);
            }

            if (!_currentNode.Continuity.HasFlag(PermittedContinuations.CloseGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(FlowSymbols.GroupClose);
            }

            _currentGroupRootNode.Close();

            if (_currentGroupRootNode.Parent is ReferenceNode)
            {
                _currentNode = _currentGroupRootNode.Parent;
            }
            else
            {
                _currentNode = _currentGroupRootNode;
            }

            /* Find the actual root now. */
            var rootNode = _currentNode.Parent;
            while (!(rootNode is RootNode))
            {
                rootNode = rootNode.Parent;
            }

            _currentGroupRootNode = (RootNode)rootNode;
        }

        private void ContinueExistingGroup()
        {
            if (!_currentNode.Continuity.HasFlag(PermittedContinuations.ContinueGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(FlowSymbols.Separator);
            }

            _currentNode = _currentGroupRootNode;
        }

        private void StartUnary([NotNull] UnaryOperator unaryOperator)
        {
            Debug.Assert(unaryOperator != null);

            if (!_currentNode.Continuity.HasFlag(PermittedContinuations.UnaryOperator))
            {
                ExceptionHelper.UnexpectedOperator(unaryOperator.Symbol);
            }

            var currentOperatorNode = _currentNode as OperatorNode;
            if (currentOperatorNode != null)
            {
                Debug.Assert(currentOperatorNode.RightNode == null, "current operator node's right node must be null.");

                currentOperatorNode.RightNode = new UnaryOperatorNode(currentOperatorNode, unaryOperator);
                _currentNode = currentOperatorNode.RightNode;
            }
            else if (_currentNode is RootNode)
            {
                var currentRootNode = (RootNode)_currentNode;
                Debug.Assert(!currentRootNode.Closed, "current root node cannot be closed.");

                var newNode = new UnaryOperatorNode(currentRootNode, unaryOperator);
                currentRootNode.AddChild(newNode);
                _currentNode = newNode;
            }
        }

        private void StartBinary([NotNull] BinaryOperator binaryOperator)
        {
            Debug.Assert(binaryOperator != null);

            if (!_currentNode.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
            {
                ExceptionHelper.UnexpectedOperator(binaryOperator.Symbol);
            }

            var leftNode = _currentNode;
            var comparand = binaryOperator.Associativity == Associativity.LeftToRight ? 0 : -1;

            /* Go up the tree while the precedence allows. */
            while (leftNode.Parent is OperatorNode &&
                   ((OperatorNode)leftNode.Parent).Operator.Precedence.CompareTo(binaryOperator.Precedence) <= comparand)
            {
                leftNode = leftNode.Parent;
            }

            var leftNodeParentOperatorNode = leftNode.Parent as OperatorNode;

            _currentNode = new BinaryOperatorNode(leftNode.Parent, binaryOperator)
            {
                LeftNode = leftNode,
            };

            /* Re-jig the tree. */
            if (leftNodeParentOperatorNode != null)
            {
                leftNodeParentOperatorNode.RightNode = _currentNode;
            }

            leftNode.Parent = _currentNode;
            if (_currentGroupRootNode.LastChild == leftNode)
            {
                _currentGroupRootNode.LastChild = _currentNode;
            }
        }

        private void CompleteWithSymbol([NotNull] string symbol)
        {
            Debug.Assert(!string.IsNullOrEmpty(symbol));
            if (!_currentNode.Continuity.HasFlag(PermittedContinuations.Identifier))
            {
                ExceptionHelper.InvalidExpressionTerm(symbol);
            }

            var currentDisembowelerNode = _currentNode as ReferenceNode;
            if (currentDisembowelerNode != null)
            {
                Debug.Assert(currentDisembowelerNode.Object != null, "current reference node's object must not be null.");
                Debug.Assert(currentDisembowelerNode.Identifier == null, "current reference node's identifier must be null.");

                currentDisembowelerNode.Identifier = symbol;
            }
            else
            {
                var newNode = new ReferenceNode(_currentNode, symbol);

                var currentOperatorNode = _currentNode as OperatorNode;
                if (currentOperatorNode != null)
                {
                    Debug.Assert(currentOperatorNode.RightNode == null, "current operator node's right node must be null.");

                    currentOperatorNode.RightNode = newNode;
                    _currentNode = newNode;
                }
                else if (_currentNode is RootNode)
                {
                    var currentRootNode = (RootNode)_currentNode;
                    Debug.Assert(!currentRootNode.Closed, "current root node cannot be closed.");

                    currentRootNode.AddChild(newNode);
                    _currentNode = newNode;
                }
            }
        }

        private void CompleteWithLiteral([CanBeNull] object literal)
        {
            if (!_currentNode.Continuity.HasFlag(PermittedContinuations.Literal))
            {
                var currentReferenceNode = _currentNode as ReferenceNode;
                if (currentReferenceNode != null && currentReferenceNode.Identifier == null)
                {
                    ExceptionHelper.UnexpectedLiteralRequiresIdentifier(FlowSymbols.MemberAccess, literal);
                }
                else
                {
                    ExceptionHelper.UnexpectedLiteralRequiresOperator(literal);
                }
            }

            var newNode = new LiteralNode(_currentNode, literal);

            var currentOperatorNode = _currentNode as OperatorNode;
            if (currentOperatorNode != null)
            {
                Debug.Assert(currentOperatorNode.RightNode == null, "current operator node's right node must be null.");

                currentOperatorNode.RightNode = newNode;
                _currentNode = newNode;
            }
            else if (_currentNode is RootNode)
            {
                var currentRootNode = (RootNode)_currentNode;
                Debug.Assert(!currentRootNode.Closed, "current root node cannot be closed.");

                currentRootNode.AddChild(newNode);
                _currentNode = newNode;
            }
        }

        private void ContinueWithMemberAccess()
        {
            if (!_currentNode.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
            {
                ExceptionHelper.UnexpectedOperator(FlowSymbols.MemberAccess);
            }

            /* Left side now becomes the "object" of disembowelment and the right side will be the member name */
            var newNode = new ReferenceNode(_currentNode.Parent, _currentNode);
            var parentOperatorNode = _currentNode.Parent as OperatorNode;
            if (parentOperatorNode != null)
            {
                Debug.Assert(parentOperatorNode.RightNode == _currentNode, "parent operator node's right node must be the current node.");
                parentOperatorNode.RightNode = newNode;
            }
            else if (_currentNode.Parent == _currentGroupRootNode)
            {
                Debug.Assert(_currentGroupRootNode.LastChild == _currentNode, "the last child of the current root node must be the current node.");
                _currentGroupRootNode.LastChild = newNode;
            }

            _currentNode = newNode;
        }

        private void FeedTerm([CanBeNull] object term, bool literalTerm)
        {
            Debug.Assert(literalTerm || term is string, "the term expected to be a string for non-literals.");

            if (_currentGroupRootNode == null)
            {
                /* Init! */
                _currentGroupRootNode = new RootNode(null, false);
                _currentNode = _currentGroupRootNode;
            }

            if (Constructed)
            {
                ExceptionHelper.CannotModifyAConstructedExpression();
            }

            if (!literalTerm)
            {
                var symbol = (string)term;

                if (symbol == FlowSymbols.MemberAccess)
                {
                    ContinueWithMemberAccess();
                }
                else if (symbol == FlowSymbols.GroupOpen)
                {
                    OpenNewGroup();
                }
                else if (symbol == FlowSymbols.GroupClose)
                {
                    CloseExistingGroup();
                }
                else if (symbol == FlowSymbols.Separator)
                {
                    ContinueExistingGroup();
                }
                else
                {
                    UnaryOperator unaryOperator;
                    if (_unaryOperatorSymbols.TryGetValue(symbol, out unaryOperator))
                    {
                        if (_currentNode.Continuity.HasFlag(PermittedContinuations.UnaryOperator))
                        {
                            StartUnary(unaryOperator);
                            return;
                        }
                    }

                    BinaryOperator binaryOperator;
                    if (_binaryOperatorSymbols.TryGetValue(symbol, out binaryOperator))
                    {
                        if (_currentNode.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
                        {
                            StartBinary(binaryOperator);
                            return;
                        }
                    }

                    if (unaryOperator != null || binaryOperator != null)
                    {
                        ExceptionHelper.UnexpectedOperator(symbol);
                    }
                    else
                    {
                        CompleteWithSymbol(symbol);
                    }
                }
            }
            else
            {
                CompleteWithLiteral(term);
            }
        }
    }
}
