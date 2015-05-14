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

namespace XtraLiteTemplates.Evaluation.Directives
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using XtraLiteTemplates.Parsing;

    public abstract class Directive
    {
        protected internal enum FlowDecision
        {
            Terminate,
            Restart,
            Evaluate,
            Skip,
        }

        private readonly List<Tag> m_tags;

        internal IReadOnlyList<Tag> Tags
        {
            get 
            {
                return m_tags;
            }
        }

        public Directive(params Tag[] tags)
        {
            Expect.NotEmpty("tags", tags);

            foreach (var tag in tags)
            {
                Expect.NotNull("tag", tag);

                if (tag.ComponentCount == 0)
                    ExceptionHelper.CannotRegisterTagWithNoComponents();
            }

            m_tags = new List<Tag>(tags);
        }

        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var tag in m_tags)
            {
                if (sb.Length > 0)
                    sb.Append("...");

                sb.AppendFormat("{{{0}}}", tag.ToString());
            }

            return sb.ToString();
        }


        public Boolean Equals(Object obj, IEqualityComparer<String> comparer)
        {
            Expect.NotNull("comparer", comparer);

            var directiveObj = obj as Directive;
            if (directiveObj == null || directiveObj.m_tags.Count != directiveObj.m_tags.Count)
                return false;
            else if (directiveObj == this)
                return true;

            for (var i = 0; i < m_tags.Count; i++)
            {
                if (!m_tags[i].Equals(directiveObj.m_tags[i], comparer))
                    return false;
            }

            return true;
        }

        public override Boolean Equals(Object obj)
        {
            return Equals(obj, StringComparer.CurrentCulture);
        }

        public Int32 GetHashCode(IEqualityComparer<String> comparer)
        {
            Expect.NotNull("comparer", comparer);

            var hash = 73; /* Magic constant */
            unchecked
            {
                foreach (var tag in Tags)
                    hash = hash * 51 + tag.GetHashCode(comparer);
            }

            return hash;
        }

        public override Int32 GetHashCode()
        {
            return GetHashCode(StringComparer.CurrentCulture);
        }


        protected internal abstract FlowDecision Execute(Int32 tagIndex, Object[] components, ref Object state, IVariableContext context, out String text);
    }
}

