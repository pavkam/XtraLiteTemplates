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

namespace XtraLiteTemplates.Evaluation
{
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using Expressions;
    using Parsing;

    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    [SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
    internal sealed class TagNode : TemplateNode
    {
        public TagNode(TemplateNode parent, TagLex lex)
            : base(parent)
        {
            Debug.Assert(parent != null, "parent cannot be null.");
            Debug.Assert(lex != null, "lex cannot be null.");

            FirstCharacterIndex = lex.FirstCharacterIndex;
            OriginalLength = lex.OriginalLength;
            Components = lex.Components;
            Tag = lex.Tag;
        }

        public new DirectiveNode Parent => (DirectiveNode)base.Parent;

        public Tag Tag { get; }

        public int FirstCharacterIndex { get; }

        public int OriginalLength { get; }

        public object[] Components { get; }

        public object[] Evaluate(IExpressionEvaluationContext context)
        {
            Debug.Assert(context != null, "context cannot be null.");

            var result = new object[Components.Length];
            for (var i = 0; i < Components.Length; i++)
            {
                var expression = Components[i] as Expression;
                if (expression != null)
                {
                    /* Evaluate the expression. */
                    result[i] = expression.Evaluate(context);
                }
                else
                {
                    result[i] = Components[i];
                }
            }

            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var component in Components)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                sb.Append(component);
            }

            return $"{{{sb}}}";
        }
    }
}