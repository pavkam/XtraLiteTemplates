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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.ObjectModel.Directives;

    internal abstract class CompositeNode : TemplateNode, IEvaluable
    {
        private readonly List<TemplateNode> m_children;

        public IReadOnlyList<TemplateNode> Children
        {
            get
            {
                return m_children;
            }
        }

        protected CompositeNode(TemplateNode parent)
            : base(parent)
        {
            m_children = new List<TemplateNode>();
        }

        public void AddChild(TemplateNode child)
        {
            Debug.Assert(child != null);
            Debug.Assert(child.Parent == this);

            if (!m_children.Contains(child))
                m_children.Add(child);
        }

        public virtual void Evaluate(TextWriter writer, IDirectiveEvaluationContext nodeContext, IExpressionEvaluationContext expressionContext)
        {
            Expect.NotNull("writer", writer);
            Expect.NotNull("nodeContext", nodeContext);
            Expect.NotNull("expressionContext", expressionContext);

            foreach (var child in Children)
            {
                var evaluable = child as IEvaluable;
                if (evaluable != null)
                    evaluable.Evaluate(writer, nodeContext, expressionContext);
            }
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var child in Children)
                sb.Append(child.ToString());

            return String.Format("({0})", sb.ToString());
        }
    }
}

