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

namespace XtraLiteTemplates.Parsing
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    public sealed class Tag
    {
        private readonly IList<Object> m_components;

        public Tag()
        {
            m_components = new List<Object>();
        }

        public Tag Keyword(String keyword)
        {
            Expect.Identifier("keyword", keyword);

            m_components.Add(keyword);
            return this;
        }

        public Tag Identifier()
        {
            if (m_components.Count > 0 && m_components[m_components.Count - 1] == null)
                ExceptionHelper.TagAnyIndentifierCannotFollowExpression();

            m_components.Add(String.Empty);
            return this;
        }

        public Tag Identifier(params String[] candidates)
        {
            Expect.NotEmpty("candidates", candidates);

            foreach (var arg in candidates)
                Expect.Identifier("candidate", arg);

            m_components.Add(candidates);
            return this;
        }

        public Tag Expression()
        {
            if (m_components.Count > 0 && m_components[m_components.Count - 1] == null)
                ExceptionHelper.TagExpressionCannotFollowExpression();

            m_components.Add(null);

            return this;
        }

        public override String ToString()
        {
            if (m_components.Count == 0)
                return String.Empty;
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var component in m_components)
                {
                    if (sb.Length > 0)
                        sb.Append(" ");

                    if (component == null)
                        sb.Append("(EXPRESSION)");
                    else if (component == (Object)String.Empty)
                        sb.Append("(ANY IDENTIFIER)");
                    else if (component is String)
                        sb.Append(component);
                    else
                        sb.AppendFormat("(ONE OF {0})", String.Join("|", (component as String[])));
                }

                return sb.ToString();
            }
        }


        internal Int32 ComponentCount
        {
            get
            {
                return m_components.Count;
            }
        }

        internal Boolean MatchesKeyword(Int32 index, IEqualityComparer<String> comparer, String keyword)
        {
            Debug.Assert(comparer != null);
            Debug.Assert(!String.IsNullOrEmpty(keyword));

            if (index >= m_components.Count || m_components[index] == null)
                return false;

            String stringComponent = m_components[index] as String;
            return comparer.Equals(stringComponent, keyword);
        }

        internal Boolean MatchesIdentifier(Int32 index, IEqualityComparer<String> comparer, String identifier)
        {
            Debug.Assert(comparer != null);
            Debug.Assert(!String.IsNullOrEmpty(identifier));

            if (index >= m_components.Count || m_components[index] == null)
                return false;

            var stringComponent = m_components[index] as String;
            if (stringComponent != null)
                return stringComponent == String.Empty;

            String[] _identifiers = m_components[index] as String[];
            return _identifiers.Any(i => comparer.Equals(i, identifier));
        }

        internal Boolean MatchesExpression(Int32 index)
        {
            return index < m_components.Count && m_components[index] == null;
        }
    }
}

