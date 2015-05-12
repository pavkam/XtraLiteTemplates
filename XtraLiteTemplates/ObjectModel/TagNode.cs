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

namespace XtraLiteTemplates.ObjectModel
{
    using System;
    using System.Diagnostics;
    using System.Text;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Parsing;

    public sealed class TagNode : TemplateNode
    {
        public new DirectiveNode Parent
        {
            get
            {
                return (DirectiveNode)base.Parent;
            }
        }

        public Int32 FirstCharacterIndex { get; private set; }

        public Int32 OriginalLength { get; private set; }

        public Object[] Components { get; private set; }

        public Tag Tag { get; private set; }

        internal TagNode(DirectiveNode parent, TagLex lex)
            : base(parent)
        {
            Debug.Assert(parent != null);
            Debug.Assert(lex != null);

            FirstCharacterIndex = lex.FirstCharacterIndex;
            OriginalLength = lex.OriginalLength;
            Components = lex.Components;
            Tag = lex.Tag;
        }

        internal Object[] Evaluate(IExpressionEvaluationContext context)
        {
            Debug.Assert(context != null);
            Object[] result = new Object[Components.Length];
            for (var i = 0; i < Components.Length; i++)
            {
                var expression = Components[i] as Expression;
                if (expression != null)
                {
                    /* Evaluate the expression. */
                    result[i] = expression.Evaluate(context);
                }
                else
                    result[i] = Components[i];
            }

            return result;
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var component in Components)
            {                
                if (sb.Length > 0)
                    sb.Append(" ");

                sb.Append(component.ToString());
            }

            return String.Format("{{{0}}}", sb.ToString());
        }
    }
}

