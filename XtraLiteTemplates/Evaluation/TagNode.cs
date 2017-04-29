//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.Evaluation
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Parsing;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal sealed class TagNode : TemplateNode
    {
        public TagNode(DirectiveNode parent, TagLex lex)
            : base(parent)
        {
            Debug.Assert(parent != null, "parent cannot be null.");
            Debug.Assert(lex != null, "lex cannot be null.");

            this.FirstCharacterIndex = lex.FirstCharacterIndex;
            this.OriginalLength = lex.OriginalLength;
            this.Components = lex.Components;
            this.Tag = lex.Tag;
        }

        public new DirectiveNode Parent
        {
            get
            {
                return (DirectiveNode)base.Parent;
            }
        }

        public Tag Tag { get; private set; }

        public int FirstCharacterIndex { get; private set; }

        public int OriginalLength { get; private set; }

        public object[] Components { get; private set; }

        public object[] Evaluate(IExpressionEvaluationContext context)
        {
            Debug.Assert(context != null, "context cannot be null.");

            object[] result = new object[this.Components.Length];
            for (var i = 0; i < this.Components.Length; i++)
            {
                var expression = this.Components[i] as Expression;
                if (expression != null)
                {
                    /* Evaluate the expression. */
                    result[i] = expression.Evaluate(context);
                }
                else
                {
                    result[i] = this.Components[i];
                }
            }

            return result;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var component in this.Components)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                sb.Append(component.ToString());
            }

            return string.Format("{{{0}}}", sb.ToString());
        }
    }
}