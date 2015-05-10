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
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace XtraLiteTemplates.Parsing
{
    public sealed class Tag
    {
        private readonly IList<String> m_components;

        public Tag()
        {
            m_components = new List<String>();
        }

        public Tag Keyword(String keyword)
        {
            Expect.NotEmpty("keyword", keyword);

            m_components.Add(keyword);
            return this;
        }

        public Tag Expression()
        {
            if (m_components.Count > 0 && m_components[m_components.Count - 1] == null)
                throw new InvalidOperationException("placeholder_expression_after_expression_not_allowed");

            m_components.Add(null);

            return this;
        }


        internal Boolean MatchesKeyword(Int32 index, IEqualityComparer<String> comparer, String keyword)
        {
            Debug.Assert(comparer != null);
            Debug.Assert(!String.IsNullOrEmpty(keyword));

            if (index >= m_components.Count || m_components[index] == null)
                return false;
            
            return comparer.Equals(m_components[index], keyword);
        }

        internal Boolean MatchesExpression(Int32 index)
        {
            return index < m_components.Count && m_components[index] == null;
        }
    }
}

