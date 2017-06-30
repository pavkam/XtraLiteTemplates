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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using JetBrains.Annotations;

    internal abstract class CompositeNode : TemplateNode
    {
        [NotNull]
        private readonly List<TemplateNode> _childNodes;

        protected CompositeNode([CanBeNull] TemplateNode parent)
            : base(parent)
        {
            _childNodes = new List<TemplateNode>();
        }

        [NotNull]
        public IReadOnlyList<TemplateNode> Children => _childNodes;

        public void AddChild([NotNull] TemplateNode child)
        {
            Debug.Assert(child != null, "child cannot be null.");
            Debug.Assert(child.Parent == this, "child's Parent must be this.");

            if (!_childNodes.Contains(child))
            {
                _childNodes.Add(child);
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var child in Children)
            {
                sb.Append(child);
            }

            return $"({sb})";
        }
    }
}