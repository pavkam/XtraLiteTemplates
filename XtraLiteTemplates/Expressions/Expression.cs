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
    using System.Diagnostics.CodeAnalysis;
    using XtraLiteTemplates.Expressions.Nodes;
    using XtraLiteTemplates.Expressions.Operators;

    /// <summary>
    /// Provides core expression construction and evaluation functionality.
    /// </summary>
    public sealed class Expression
    {
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private ExpressionNode currentNode;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private RootNode currentGroupRootNode;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private List<Operator> registeredOperators;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private Func<IExpressionEvaluationContext, object> evaluationFunction;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private Dictionary<string, UnaryOperator> unaryOperatorSymbols;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private Dictionary<string, BinaryOperator> binaryOperatorSymbols;

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
            this.unaryOperatorSymbols = new Dictionary<string, UnaryOperator>(comparer);
            this.binaryOperatorSymbols = new Dictionary<string, BinaryOperator>(comparer);
            this.registeredOperators = new List<Operator>();
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
        /// Gets all supported operators.
        /// </summary>
        /// <value>
        /// The supported operators.
        /// </value>
        public IReadOnlyList<Operator> SupportedOperators
        {
            get
            {
                return this.registeredOperators;
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
        /// Gets a value indicating whether this <see cref="Expression"/> is constructed.
        /// </summary>
        /// <value>
        ///   <c>true</c> if constructed; otherwise, <c>false</c>.
        /// </value>
        public bool Constructed
        {
            get
            {
                return this.evaluationFunction != null;
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
                return this.currentNode != null;
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
                this.unaryOperatorSymbols.ContainsKey(symbol) || this.binaryOperatorSymbols.ContainsKey(symbol);
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

            if (this.Started)
            {
                ExceptionHelper.CannotRegisterOperatorsForStartedExpression();
            }

            Debug.Assert(!this.Constructed, "must not be constructed.");

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
                if (this.unaryOperatorSymbols.ContainsKey(unaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(unaryOperator);
                }

                this.unaryOperatorSymbols.Add(@operator.Symbol, unaryOperator);
            }
            else if (@operator is BinaryOperator)
            {
                var binaryOperator = (BinaryOperator)@operator;
                if (this.binaryOperatorSymbols.ContainsKey(binaryOperator.Symbol))
                {
                    ExceptionHelper.OperatorAlreadyRegistered(binaryOperator);
                }

                this.binaryOperatorSymbols.Add(@operator.Symbol, binaryOperator);
            }
            else
            {
                Debug.Fail("Unsupported operator type.");
            }

            this.registeredOperators.Add(@operator);
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

            this.FeedTerm(literal, true);
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

            this.FeedTerm(symbol, false);
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
                if (this.currentGroupRootNode.Parent != null)
                {
                    fail = true;
                }
                else
                {
                    var currentRootNode = this.currentNode as RootNode;
                    if (currentRootNode != null)
                    {
                        fail = !currentRootNode.Closed;
                    }
                    else
                    {
                        var currentReferenceNode = this.currentNode as ReferenceNode;
                        if (currentReferenceNode != null)
                        {
                            fail = currentReferenceNode.Identifier == null;
                        }
                        else
                        {
                            fail = !(this.currentNode is LeafNode);
                        }
                    }
                }

                if (fail)
                {
                    ExceptionHelper.CannotConstructExpressionInvalidState();
                }

                /* Reduce the expression if so was desired. */
                this.currentGroupRootNode.Reduce(ReduceExpressionEvaluationContext.Instance);
                this.evaluationFunction = this.currentGroupRootNode.GetEvaluationFunction();
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

            if (!this.Constructed)
            {
                ExceptionHelper.CannotEvaluateUnconstructedExpression();
            }

            return this.evaluationFunction(context);
        }

        /// <summary>
        /// Returns a human-readable representation this expression instance using the <see cref="ExpressionFormatStyle.Arithmetic"/> formatting.
        /// </summary>
        /// <returns>
        /// A <see cref="System.String" /> that represents this instance.
        /// </returns>
        public override string ToString()
        {
            return this.ToString(ExpressionFormatStyle.Arithmetic);
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
            if (this.currentGroupRootNode == null)
            {
                return "??";
            }
            else
            {
                return this.currentGroupRootNode.ToString(style);
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private void OpenNewGroup()
        {
            if (!this.currentNode.Continuity.HasFlag(PermittedContinuations.NewGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(this.FlowSymbols.GroupOpen);
            }

            if (this.currentNode is ReferenceNode)
            {
                var currentReferenceNode = (ReferenceNode)this.currentNode;
                Debug.Assert(currentReferenceNode.Identifier != null, "current reference node's identifier must not be null.");

                /* Flip to the new root */
                this.currentGroupRootNode = new RootNode(currentReferenceNode);
                currentReferenceNode.Arguments = currentGroupRootNode;
                this.currentNode = this.currentGroupRootNode;
            }
            else if (this.currentNode is OperatorNode)
            {
                var currentOperatorNode = (OperatorNode)this.currentNode;
                Debug.Assert(currentOperatorNode.RightNode == null, "current operator node's right node must be null.");

                /* Flip to the new root */
                this.currentGroupRootNode = new RootNode(currentOperatorNode);
                currentOperatorNode.RightNode = this.currentGroupRootNode;
                this.currentNode = this.currentGroupRootNode;
            }
            else if (this.currentNode is RootNode)
            {
                var currentRootNode = (RootNode)this.currentNode;

                Debug.Assert(currentRootNode == this.currentGroupRootNode, "current operator node must be the current root node.");
                Debug.Assert(!currentRootNode.Closed, "current root node cannot be closed.");

                /* Flip to the new root */
                this.currentGroupRootNode = new RootNode(currentRootNode);
                currentRootNode.AddChild(this.currentGroupRootNode);
                this.currentNode = this.currentGroupRootNode;
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private void CloseExistingGroup()
        {
            /* Special case just here! */
            if (this.currentGroupRootNode.Parent == null)
            {
                ExceptionHelper.InvalidExpressionTerm(this.FlowSymbols.GroupClose);
            }

            if (!this.currentNode.Continuity.HasFlag(PermittedContinuations.CloseGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(this.FlowSymbols.GroupClose);
            }

            this.currentGroupRootNode.Close();

            if (this.currentGroupRootNode.Parent is ReferenceNode)
            {
                this.currentNode = this.currentGroupRootNode.Parent;
            }
            else
            {
                this.currentNode = this.currentGroupRootNode;
            }

            /* Find the actual root now. */
            var rootNode = this.currentNode.Parent;
            while (!(rootNode is RootNode))
            {
                rootNode = rootNode.Parent;
            }

            this.currentGroupRootNode = (RootNode)rootNode;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private void ContinueExistingGroup()
        {
            if (!this.currentNode.Continuity.HasFlag(PermittedContinuations.ContinueGroup))
            {
                ExceptionHelper.InvalidExpressionTerm(this.FlowSymbols.Separator);
            }

            this.currentNode = this.currentGroupRootNode;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private void StartUnary(UnaryOperator unaryOperator)
        {
            if (!this.currentNode.Continuity.HasFlag(PermittedContinuations.UnaryOperator))
            {
                ExceptionHelper.UnexpectedOperator(unaryOperator.Symbol);
            }

            if (this.currentNode is OperatorNode)
            {
                var currentOperatorNode = this.currentNode as OperatorNode;
                Debug.Assert(currentOperatorNode.RightNode == null, "current operator node's right node must be null.");

                currentOperatorNode.RightNode = new UnaryOperatorNode(currentOperatorNode, unaryOperator);
                this.currentNode = currentOperatorNode.RightNode;
            }
            else if (this.currentNode is RootNode)
            {
                var currentRootNode = this.currentNode as RootNode;
                Debug.Assert(!currentRootNode.Closed, "current root node cannot be closed.");

                var newNode = new UnaryOperatorNode(currentRootNode, unaryOperator);
                currentRootNode.AddChild(newNode);
                this.currentNode = newNode;
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private void StartBinary(BinaryOperator binaryOperator)
        {
            if (!this.currentNode.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
            {
                ExceptionHelper.UnexpectedOperator(binaryOperator.Symbol);
            }

            var leftNode = this.currentNode;
            var comparand = binaryOperator.Associativity == Associativity.LeftToRight ? 0 : -1;

            /* Go up the tree while the precedence allows. */
            while (leftNode.Parent is OperatorNode &&
                   ((OperatorNode)leftNode.Parent).Operator.Precedence.CompareTo(binaryOperator.Precedence) <= comparand)
            {
                leftNode = leftNode.Parent;
            }

            var leftNodeParentOperatorNode = leftNode.Parent as OperatorNode;

            this.currentNode = new BinaryOperatorNode(leftNode.Parent, binaryOperator)
            {
                LeftNode = leftNode,
            };

            /* Re-jig the tree. */
            if (leftNodeParentOperatorNode != null)
            {
                leftNodeParentOperatorNode.RightNode = this.currentNode;
            }

            leftNode.Parent = this.currentNode;
            if (this.currentGroupRootNode.LastChild == leftNode)
            {
                this.currentGroupRootNode.LastChild = this.currentNode;
            }

            return;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private void CompleteWithSymbol(string symbol)
        {
            if (!this.currentNode.Continuity.HasFlag(PermittedContinuations.Identifier))
            {
                ExceptionHelper.InvalidExpressionTerm(symbol);
            }

            if (this.currentNode is ReferenceNode)
            {
                var currentDisembowelerNode = this.currentNode as ReferenceNode;
                Debug.Assert(currentDisembowelerNode.Object != null, "current reference node's object must not be null.");
                Debug.Assert(currentDisembowelerNode.Identifier == null, "current reference node's identifier must be null.");

                currentDisembowelerNode.Identifier = symbol;
            }
            else
            {
                var newNode = new ReferenceNode(this.currentNode, symbol);

                if (this.currentNode is OperatorNode)
                {
                    var currentOperatorNode = this.currentNode as OperatorNode;
                    Debug.Assert(currentOperatorNode.RightNode == null, "current operator node's right node must be null.");

                    currentOperatorNode.RightNode = newNode;
                    this.currentNode = newNode;
                }
                else if (this.currentNode is RootNode)
                {
                    var currentRootNode = this.currentNode as RootNode;
                    Debug.Assert(!currentRootNode.Closed, "current root node cannot be closed.");

                    currentRootNode.AddChild(newNode);
                    this.currentNode = newNode;
                }
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private void CompleteWithLiteral(object literal)
        {
            if (!this.currentNode.Continuity.HasFlag(PermittedContinuations.Literal))
            {
                var currentReferenceNode = this.currentNode as ReferenceNode;
                if (currentReferenceNode != null && currentReferenceNode.Identifier == null)
                {
                    ExceptionHelper.UnexpectedLiteralRequiresIdentifier(this.FlowSymbols.MemberAccess, literal);
                }
                else
                {
                    ExceptionHelper.UnexpectedLiteralRequiresOperator(literal);
                }
            }

            var newNode = new LiteralNode(this.currentNode, literal);

            if (this.currentNode is OperatorNode)
            {
                var currentOperatorNode = this.currentNode as OperatorNode;

                Debug.Assert(currentOperatorNode.RightNode == null, "current operator node's right node must be null.");

                currentOperatorNode.RightNode = newNode;
                this.currentNode = newNode;
            }
            else if (this.currentNode is RootNode)
            {
                var currentRootNode = this.currentNode as RootNode;
                Debug.Assert(!currentRootNode.Closed, "current root node cannot be closed.");

                currentRootNode.AddChild(newNode);
                this.currentNode = newNode;
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private void ContinueWithMemberAccess()
        {
            if (!this.currentNode.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
            {
                ExceptionHelper.UnexpectedOperator(this.FlowSymbols.MemberAccess);
            }

            /* Left side now becomes the "object" of disembowlement and the right side will be the member name */
            var newNode = new ReferenceNode(this.currentNode.Parent, this.currentNode);
            var parentOperatorNode = this.currentNode.Parent as OperatorNode;
            if (parentOperatorNode != null)
            {
                Debug.Assert(parentOperatorNode.RightNode == this.currentNode, "parent operator node's right node must be the current node.");
                parentOperatorNode.RightNode = newNode;
            }
            else if (this.currentNode.Parent == this.currentGroupRootNode)
            {
                Debug.Assert(this.currentGroupRootNode.LastChild == this.currentNode, "the last child of the current root node must be the current node.");
                this.currentGroupRootNode.LastChild = newNode;
            }

            this.currentNode = newNode;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private void FeedTerm(object term, bool isLiteral)
        {
            Debug.Assert(isLiteral || term is string, "the term expected to be a string for non-literals.");

            if (this.currentGroupRootNode == null)
            {
                /* Init! */
                this.currentGroupRootNode = new RootNode(null);
                this.currentNode = this.currentGroupRootNode;
            }

            if (this.Constructed)
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
                    if (this.unaryOperatorSymbols.TryGetValue(symbol, out unaryOperator))
                    {
                        if (this.currentNode.Continuity.HasFlag(PermittedContinuations.UnaryOperator))
                        {
                            this.StartUnary(unaryOperator);
                            return;
                        }
                    }

                    BinaryOperator binaryOperator;
                    if (this.binaryOperatorSymbols.TryGetValue(symbol, out binaryOperator))
                    {
                        if (this.currentNode.Continuity.HasFlag(PermittedContinuations.BinaryOperator))
                        {
                            this.StartBinary(binaryOperator);
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
