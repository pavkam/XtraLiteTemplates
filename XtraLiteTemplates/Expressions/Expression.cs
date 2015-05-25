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

namespace XtraLiteTemplates.Expressions
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using XtraLiteTemplates.Expressions.Nodes;
    using XtraLiteTemplates.Expressions.Operators;

    /// <summary>
    /// Provides core expression construction and evaluation functionality.
    /// </summary>
    public sealed class Expression
    {
        private ExpressionNode m_current;
        private RootNode m_root;
        private List<Operator> m_supportedOperators;
        private Func<IExpressionEvaluationContext, object> m_function;
        private Dictionary<string, UnaryOperator> m_unaryOperatorSymbols;
        private Dictionary<string, BinaryOperator> m_binaryOperatorSymbols;

        /// <summary>
        /// Gets a value indicating whether this <see cref="Expression"/> is constructed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if constructed; otherwise, <c>false</c>.
        /// </value>
        public bool Constructed
        {
            get
            {
                return this.m_function != null;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the construction of this <see cref="Expression"/> has started.
        /// </summary>
        /// <value>
        ///   <c>true</c> if started; otherwise, <c>false</c>.
        /// </value>
        public bool Started
        {
            get
            {
                return this.m_current != null;
            }
        }

        /// <summary>
        /// Determines whether the specified <paramref name="symbol" /> is a supported operator.
        /// </summary>
        /// <param name="symbol">The symbol to verify.</param>
        /// <returns>
        ///   <c>true</c> if the symbol is supported; otherwise, <c>false</c>.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="symbol"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="symbol"/> is empty.</exception>
        public bool IsSupportedOperator(string symbol)
        {
            Expect.NotEmpty("symbol", symbol);

            return
                this.m_unaryOperatorSymbols.ContainsKey(symbol) || this.m_binaryOperatorSymbols.ContainsKey(symbol);
        }

        /// <summary>
        /// Gets all supported operators.
        /// </summary>
        /// <value>
        /// The supported operators.
        /// </value>
        public IReadOnlyList<Operator> SupportedOperators
        {
            get
            {
                return this.m_supportedOperators;
            }
        }

        /// <summary>
        /// Gets the flow symbols.
        /// </summary>
        /// <value>
        /// The flow symbols.
        /// </value>
        public ExpressionFlowSymbols FlowSymbols { get; private set; }

        /// <summary>
        /// Gets the comparer used to compare symbols and identifiers.
        /// </summary>
        /// <value>
        /// The symbol and identifier comparer.
        /// </value>
        public IEqualityComparer<string> Comparer { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class.
        /// </summary>
        /// <param name="flowSymbols">The flow symbols.</param>
        /// <param name="comparer">The symbol and identifier comparer.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="flowSymbols"/> or <paramref name="comparer"/> is <c>null</c>.</exception>
        public Expression(ExpressionFlowSymbols flowSymbols, IEqualityComparer<string> comparer)
        {
            Expect.NotNull("comparer", comparer);
            Expect.NotNull("flowSymbols", flowSymbols);

            this.FlowSymbols = flowSymbols;
            this.m_unaryOperatorSymbols = new Dictionary<String, UnaryOperator>(comparer);
            this.m_binaryOperatorSymbols = new Dictionary<String, BinaryOperator>(comparer);
            this.m_supportedOperators = new List<Operator>();
            this.Comparer = comparer;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Expression"/> class using the default flow symbols and a culture-invariant, case-insensitive comparer.
        /// </summary>
        public Expression()
            : this(ExpressionFlowSymbols.Default, StringComparer.OrdinalIgnoreCase)
        {
        }

        /// <summary>
        /// Registers an operator with this expression.
        /// </summary>
        /// <param name="operator">The operator to register.</param>
        /// <returns>This expression instance.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="operator"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Expression under constrcution or the symbol of <paramref name="operator"/> already registered.</exception>
        public Expression RegisterOperator(Operator @operator)
        {
            Expect.NotNull("operator", @operator);

            if (Started)
            {
                ExceptionHelper.CannotRegisterOperatorsForStartedExpression();
            }

            Debug.Assert(!Constructed);

            /* Standards. */
            if (@operator.Symbol == this.FlowSymbols.Separator ||
                @operator.Symbol == this.FlowSymbols.GroupClose ||
                @operator.Symbol == this.FlowSymbols.GroupOpen ||
                @operator.Symbol == this.FlowSymbols.MemberAccess)
            {
                ExceptionHelper.OperatorAlreadyRegistered(@operator);
            }

            if (@operator is UnaryOperator)
            {
                var unaryOperator = (UnaryOperator)@operator;
                if (this.m_unaryOperatorSymbols.ContainsKey(unaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(unaryOperator);
                }

                this.m_unaryOperatorSymbols.Add(@operator.Symbol, unaryOperator);
            }
            else if (@operator is BinaryOperator)
            {
                var binaryOperator = (BinaryOperator)@operator;
                if (this.m_binaryOperatorSymbols.ContainsKey(binaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(binaryOperator);
                }

                this.m_binaryOperatorSymbols.Add(@operator.Symbol, binaryOperator);
            }
            else
            {
                Debug.Fail("Unsupported operator type.");
            }

            this.m_supportedOperators.Add(@operator);
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
        public Expression FeedLiteral(object literal)
        {
            if (this.Constructed)
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
        public Expression FeedSymbol(string symbol)
        {
            Expect.NotEmpty("symbol", symbol);

            if (this.Constructed)
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
            if (!this.Started)
            {
                ExceptionHelper.CannotConstructExpressionInvalidState();
            }

            if (!this.Constructed)
            {
                bool fail = false;
                if (this.m_root.Parent != null)
                {
                    fail = true;
                }
                else
                {
                    var currentRootNode = this.m_current as RootNode;
                    if (currentRootNode != null)
                    {
                        fail = !currentRootNode.Closed;
                    }
                    else
                    {
                        var currentDisembowelerNode = this.m_current as DisembowelerNode;
                        if (currentDisembowelerNode != null)
                        {
                            fail = currentDisembowelerNode.MemberName == null;
                        }
                        else
                        {
                            fail = !(this.m_current is LeafNode);
                        }
                    }
                }

                if (fail)
                {
                    ExceptionHelper.CannotConstructExpressionInvalidState();
                }

                /* Reduce the expression if so was desired. */
                this.m_root.Reduce(ReduceExpressionEvaluationContext.Instance);
                this.m_function = this.m_root.GetEvaluationFunction();
            }
        }

        /// <summary>
        /// Evaluates this expression using an evaluation context.
        /// </summary>
        /// <param name="context">The evaluation context.</param>
        /// <returns>The result of expression evaluation.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">Expression not constructed.</exception>
        public object Evaluate(IExpressionEvaluationContext context)
        {
            Expect.NotNull("context", context);

            if (!Constructed)
            {
                ExceptionHelper.CannotEvaluateUnconstructedExpression();
            }

            return this.m_function(context);
        }

        /// <summary>
        /// Returns a human-readable representation this expression instance using the <see cref="ExpressionFormatStyle.Arithmetic"/> formatting.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
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
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public string ToString(ExpressionFormatStyle style)
        {
            if (this.m_root == null)
            {
                return "??";
            }
            else
            {
                return this.m_root.ToString(style);
            }
        }

        private void OpenNewGroup()
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.NewGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(this.FlowSymbols.GroupOpen);
            }

            if (m_current is OperatorNode)
            {
                var currentOperatorNode = (OperatorNode)this.m_current;
                Debug.Assert(currentOperatorNode.RightNode == null);

                /* Flip to the new root */
                this.m_root = new RootNode(currentOperatorNode);
                currentOperatorNode.RightNode = this.m_root;
                this.m_current = this.m_root;
            }
            else if (this.m_current is RootNode)
            {
                var currentRootNode = (RootNode)this.m_current;

                Debug.Assert(currentRootNode == this.m_root);
                Debug.Assert(!currentRootNode.Closed);

                /* Flip to the new root */
                this.m_root = new RootNode(currentRootNode);
                currentRootNode.AddChild(m_root);
                this.m_current = m_root;
            }
        }

        private void CloseExistingGroup()
        {
            /* Special case just here! */
            if (this.m_root.Parent == null)
            {
                ExceptionHelper.InvalidExpressionTerm(this.FlowSymbols.GroupClose);
            }

            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.CloseGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(this.FlowSymbols.GroupClose);
            }

            this.m_root.Close();
            this.m_current = m_root;

            /* Find the actual root now. */
            var rootNode = this.m_root.Parent;
            while (!(rootNode is RootNode))
            {
                rootNode = rootNode.Parent;
            }

            this.m_root = (RootNode)rootNode;
        }

        private void ContinueExistingGroup()
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.ContinueGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(FlowSymbols.Separator);
            }

            this.m_current = this.m_root;
        }

        private void StartUnary(UnaryOperator unaryOperator)
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.UnaryOperator))
            {
                ExceptionHelper.UnexpectedOperator(unaryOperator.Symbol);
            }

            if (this.m_current is OperatorNode)
            {
                var currentOperatorNode = this.m_current as OperatorNode;
                Debug.Assert(currentOperatorNode.RightNode == null);

                currentOperatorNode.RightNode = new UnaryOperatorNode(currentOperatorNode, unaryOperator);
                this.m_current = currentOperatorNode.RightNode;
            }
            else if (m_current is RootNode)
            {
                var currentRootNode = this.m_current as RootNode;
                Debug.Assert(!currentRootNode.Closed);

                var newNode = new UnaryOperatorNode(currentRootNode, unaryOperator);
                currentRootNode.AddChild(newNode);
                this.m_current = newNode;
            }
        }

        private void StartBinary(BinaryOperator binaryOperator)
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
            {
                ExceptionHelper.UnexpectedOperator(binaryOperator.Symbol);
            }

            var leftNode = this.m_current;
            var comparand = binaryOperator.Associativity == Associativity.LeftToRight ? 0 : -1;

            /* Go up the tree while the precedence allows. */
            while (leftNode.Parent is OperatorNode &&
                   ((OperatorNode)leftNode.Parent).Operator.Precedence.CompareTo(binaryOperator.Precedence) <= comparand)
            {
                leftNode = leftNode.Parent;
            }

            var leftNodeParentOperatorNode = leftNode.Parent as OperatorNode;

            this.m_current = new BinaryOperatorNode(leftNode.Parent, binaryOperator)
            {
                LeftNode = leftNode,
            };

            /* Re-jig the tree. */
            if (leftNodeParentOperatorNode != null)
            {
                leftNodeParentOperatorNode.RightNode = this.m_current;
            }

            leftNode.Parent = m_current;
            if (this.m_root.LastChild == leftNode)
            {
                this.m_root.LastChild = m_current;
            }

            return;
        }

        private void CompleteWithSymbol(string symbol)
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.Identifier))
            {
                ExceptionHelper.InvalidExpressionTerm(symbol);
            }

            var newNode = new ReferenceNode(this.m_current, symbol);

            if (this.m_current is OperatorNode)
            {
                var currentOperatorNode = this.m_current as OperatorNode;
                Debug.Assert(currentOperatorNode.RightNode == null);

                currentOperatorNode.RightNode = newNode;
                this.m_current = newNode;
            }
            else if (this.m_current is RootNode)
            {
                var currentRootNode = this.m_current as RootNode;
                Debug.Assert(!currentRootNode.Closed);

                currentRootNode.AddChild(newNode);
                this.m_current = newNode;
            }
            else if (this.m_current is DisembowelerNode && ((DisembowelerNode)this.m_current).MemberName == null)
            {
                var currentDisembowelerNode = this.m_current as DisembowelerNode;
                Debug.Assert(currentDisembowelerNode.ObjectNode != null);

                currentDisembowelerNode.MemberName = symbol;
            }
        }

        private void CompleteWithLiteral(object literal)
        {
            if (!this.m_current.Continuity.HasFlag(PermittedContinuations.Literal))
            {
                if (this.m_current is DisembowelerNode && ((DisembowelerNode)this.m_current).MemberName == null)
                {
                    ExceptionHelper.UnexpectedLiteralRequiresIdentifier(FlowSymbols.MemberAccess, literal);
                }
                else
                {
                    ExceptionHelper.UnexpectedLiteralRequiresOperator(literal);
                }
            }

            var newNode = new LiteralNode(this.m_current, literal);

            if (this.m_current is OperatorNode)
            {
                var currentOperatorNode = this.m_current as OperatorNode;

                Debug.Assert(currentOperatorNode.RightNode == null);

                currentOperatorNode.RightNode = newNode;
                this.m_current = newNode;
            }
            else if (this.m_current is RootNode)
            {
                var currentRootNode = this.m_current as RootNode;
                Debug.Assert(!currentRootNode.Closed);

                currentRootNode.AddChild(newNode);
                this.m_current = newNode;
            }
        }

        private void ContinueWithMemberAccess()
        {
            if (!m_current.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
            {
                ExceptionHelper.UnexpectedOperator(this.FlowSymbols.MemberAccess);
            }

            /* Left side now becomes the "object" of disembowlement and the right side will be the member name */
            var newNode = new DisembowelerNode(this.m_current.Parent, this.m_current);
            var parentOperatorNode = this.m_current.Parent as OperatorNode;
            if (parentOperatorNode != null)
            {
                Debug.Assert(parentOperatorNode.RightNode == this.m_current);
                parentOperatorNode.RightNode = newNode;
            }
            else if (this.m_current.Parent == m_root)
            {
                Debug.Assert(this.m_root.LastChild == this.m_current);
                this.m_root.LastChild = newNode;
            }

            this.m_current = newNode;
        }

        private void FeedTerm(object term, bool isLiteral)
        {
            Debug.Assert(isLiteral || term is string);

            if (this.m_root == null)
            {
                /* Init! */
                this.m_root = new RootNode(null);
                this.m_current = m_root;
            }

            if (Constructed)
            {
                ExceptionHelper.CannotModifyAConstructedExpression();
            }

            if (!isLiteral)
            {
                var symbol = (string)term;

                if (symbol == this.FlowSymbols.MemberAccess)
                {
                    this.ContinueWithMemberAccess();
                }
                else if (symbol == this.FlowSymbols.GroupOpen)
                {
                    this.OpenNewGroup();
                }
                else if (symbol == this.FlowSymbols.GroupClose)
                {
                    this.CloseExistingGroup();
                }
                else if (symbol == this.FlowSymbols.Separator)
                {
                    this.ContinueExistingGroup();
                }
                else
                {
                    UnaryOperator unaryOperator;
                    if (this.m_unaryOperatorSymbols.TryGetValue(symbol, out unaryOperator))
                    {
                        if (this.m_current.Continuity.HasFlag(PermittedContinuations.UnaryOperator))
                        {
                            StartUnary(unaryOperator);
                            return;
                        }
                    }

                    BinaryOperator binaryOperator;
                    if (this.m_binaryOperatorSymbols.TryGetValue(symbol, out binaryOperator))
                    {
                        if (this.m_current.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
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
                        this.CompleteWithSymbol(symbol);
                    }
                }
            }
            else
            {
                this.CompleteWithLiteral(term);
            }
        }
    }
}
