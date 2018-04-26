//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2018, Alexandru Ciobanu (alex+git@ciobanu.org)
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

using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Does not apply.")]

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System.Diagnostics;

    using LinqExpression = System.Linq.Expressions.Expression;
    
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal class ReferenceNode : LeafNode
    {
        public ReferenceNode(ExpressionNode parent, ExpressionNode @object)
            : base(parent)
        {
            Debug.Assert(@object != null, "object cannot be null.");

            Object = @object;
        }

        public ReferenceNode(ExpressionNode parent, string identifier)
            : base(parent)
        {
            Debug.Assert(!string.IsNullOrEmpty(identifier), "identifier cannot be empty.");

            Identifier = identifier;
        }

        public ExpressionNode Object { get; }

        public string Identifier { get; set; }

        public RootNode Arguments { get; set; }

        public override PermittedContinuations Continuity
        {
            get
            {
                if (Identifier == null)
                {
                    return PermittedContinuations.Identifier;
                }

                return base.Continuity | PermittedContinuations.NewGroup;
            }
        }

        public override string ToString(ExpressionFormatStyle style)
        {
            var arguments = Arguments != null ? Arguments.ToString(style) : string.Empty;

            if (Object != null)
            {
                return $"{Object.ToString(style)}.{Identifier}{arguments}";
            }

            return $"@{Identifier}{arguments}";
        }

        protected override bool TryReduce(IExpressionEvaluationContext reduceContext, out object value)
        {
            Debug.Assert(reduceContext != null, "reduceContext cannot be null.");

            if (Object != null)
            {
                Object.Reduce(reduceContext);
            }

            if (Arguments != null)
            {
                Arguments.Reduce(reduceContext);
            }

            value = null;
            return false;
        }

        [SuppressMessage("StyleCop.CSharp.ReadabilityRules", "SA1118:ParameterMustNotSpanMultipleLines", Justification = "Readability is OK in this circumstances.")]
        protected override LinqExpression BuildLinqExpression()
        {
            if (Arguments != null)
            {
                /* Method invokation. */
                var argumentsExpression = Arguments.GetEvaluationLinqExpression();
                var evaluatedArgumentsVariable = LinqExpression.Variable(typeof(object));

                if (Object != null)
                {
                    var objectExpression = Object.GetEvaluationLinqExpression();

                    return LinqExpression.Block(
                        typeof(object),
                        new[] { evaluatedArgumentsVariable },
                        LinqExpressionHelper.ExpressionCallThrowIfCancellationRequested,
                        LinqExpression.Assign(evaluatedArgumentsVariable, argumentsExpression),
                        LinqExpression.IfThen(
                            LinqExpression.AndAlso(
                                LinqExpression.Not(LinqExpression.TypeIs(evaluatedArgumentsVariable, typeof(object[]))),
                                LinqExpression.NotEqual(evaluatedArgumentsVariable, LinqExpression.Constant(null))),
                            LinqExpression.Assign(
                                evaluatedArgumentsVariable,
                                LinqExpression.NewArrayInit(typeof(object), evaluatedArgumentsVariable))),
                        LinqExpression.Call(
                            LinqExpressionHelper.ExpressionParameterContext,
                            LinqExpressionHelper.MethodInfoExpressionEvaluationContextInvokeObject,
                            objectExpression,
                            LinqExpression.Constant(Identifier),
                            LinqExpression.TypeAs(evaluatedArgumentsVariable, typeof(object[]))));
                }

                return LinqExpression.Block(
                    typeof(object),
                    new[] { evaluatedArgumentsVariable },
                    LinqExpressionHelper.ExpressionCallThrowIfCancellationRequested,
                    LinqExpression.Assign(evaluatedArgumentsVariable, argumentsExpression),
                    LinqExpression.IfThen(
                        LinqExpression.AndAlso(
                            LinqExpression.Not(LinqExpression.TypeIs(evaluatedArgumentsVariable, typeof(object[]))),
                            LinqExpression.NotEqual(evaluatedArgumentsVariable, LinqExpression.Constant(null))),
                        LinqExpression.Assign(
                            evaluatedArgumentsVariable,
                            LinqExpression.NewArrayInit(typeof(object), evaluatedArgumentsVariable))),
                    LinqExpression.Call(
                        LinqExpressionHelper.ExpressionParameterContext,
                        LinqExpressionHelper.MethodInfoExpressionEvaluationContextInvoke,
                        LinqExpression.Constant(Identifier),
                        LinqExpression.TypeAs(evaluatedArgumentsVariable, typeof(object[]))));
            }

            /* Property access. */
            if (Object != null)
            {
                var objectExpression = Object.GetEvaluationLinqExpression();

                return LinqExpression.Block(
                    typeof(object),
                    LinqExpressionHelper.ExpressionCallThrowIfCancellationRequested,
                    LinqExpression.Call(
                        LinqExpressionHelper.ExpressionParameterContext,
                        LinqExpressionHelper.MethodInfoExpressionEvaluationContextGetPropertyObject,
                        objectExpression,
                        LinqExpression.Constant(Identifier)));
            }

            return LinqExpression.Block(
                typeof(object),
                LinqExpressionHelper.ExpressionCallThrowIfCancellationRequested,
                LinqExpression.Call(
                    LinqExpressionHelper.ExpressionParameterContext,
                    LinqExpressionHelper.MethodInfoExpressionEvaluationContextGetProperty,
                    LinqExpression.Constant(Identifier)));
        }
    }
}