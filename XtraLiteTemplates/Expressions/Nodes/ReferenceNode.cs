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

namespace XtraLiteTemplates.Expressions.Nodes
{
    using System;
    using System.CodeDom;
    using System.CodeDom.Compiler;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using XtraLiteTemplates.Expressions.Operators;
    
    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal class ReferenceNode : LeafNode
    {
        public ReferenceNode(ExpressionNode parent, ExpressionNode @object)
            : base(parent)
        {
            Debug.Assert(@object != null, "object cannot be null.");

            this.Object = @object;
        }

        public ReferenceNode(ExpressionNode parent, string identifier)
            : base(parent)
        {
            Debug.Assert(!string.IsNullOrEmpty(identifier), "identifier cannot be empty.");

            this.Identifier = identifier;
        }

        public ExpressionNode Object { get; private set; }

        public string Identifier { get; set; }

        public RootNode Arguments { get; set; }

        public override PermittedContinuations Continuity
        {
            get
            {
                if (this.Identifier == null)
                {
                    return PermittedContinuations.Identifier;
                }
                else
                {
                    return base.Continuity | PermittedContinuations.NewGroup;
                }
            }
        }

        public override string ToString(ExpressionFormatStyle style)
        {
            string arguments = this.Arguments != null ? this.Arguments.ToString(style) : string.Empty;

            if (this.Object != null)
            {
                return string.Format("{0}.{1}{2}", this.Object.ToString(style), this.Identifier, arguments);
            }
            else
            {
                return string.Format("@{0}{1}", this.Identifier, arguments);
            }
        }

        protected override bool TryReduce(IExpressionEvaluationContext reduceContext, out object value)
        {
            Debug.Assert(reduceContext != null, "reduceContext cannot be null.");

            if (this.Object != null)
            {
                this.Object.Reduce(reduceContext);
            }

            if (this.Arguments != null)
            {
                this.Arguments.Reduce(reduceContext);
            }

            value = null;
            return false;
        }

        protected override Func<IExpressionEvaluationContext, object> Build()
        {
            if (this.Arguments != null)
            {
                /* Method invokation. */
                var argumentsFunc = this.Arguments.GetEvaluationFunction();
                
                if (this.Object != null)
                {
                    var objectFunc = this.Object.GetEvaluationFunction();
                    return context =>
                    {
                        var @object = objectFunc(context);
                        var arguments = argumentsFunc(context);
                        var argumentsArray = arguments as object[];
                        if (argumentsArray == null && arguments != null)
                        {
                            argumentsArray = new object[] { arguments };
                        }

                        return context.Invoke(@object, this.Identifier, argumentsArray);
                    };
                }
                else
                {
                    return context =>
                    {
                        var arguments = argumentsFunc(context);
                        var argumentsArray = arguments as object[];
                        if (argumentsArray == null && arguments != null)
                        {
                            argumentsArray = new object[] { arguments };
                        }

                        return context.Invoke(this.Identifier, argumentsArray);
                    };
                }
            }
            else
            {
                /* Property access. */
                if (this.Object != null)
                {
                    var objectFunc = this.Object.GetEvaluationFunction();
                    return context =>
                    {
                        var variable = objectFunc(context);
                        return variable == null ? null : context.GetProperty(variable, this.Identifier);
                    };
                }
                else
                {
                    return context => context.GetProperty(this.Identifier);
                }
            }
        }
    }
}