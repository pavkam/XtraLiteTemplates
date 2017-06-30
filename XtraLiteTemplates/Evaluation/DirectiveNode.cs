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
    using System.Linq;

    using JetBrains.Annotations;

    using XtraLiteTemplates.Parsing;

    internal sealed class DirectiveNode : CompositeNode
    {
        public DirectiveNode([NotNull] TemplateNode parent, [NotNull] [ItemNotNull] Directive[] candidateDirectives)
            : base(parent)
        {
            Debug.Assert(parent != null, "parent cannot be null.");
            Debug.Assert(candidateDirectives != null, "candidateDirectives cannot be null.");
            Debug.Assert(candidateDirectives.Length > 0, "candidateDirectives cannot be empty");

            CandidateDirectives = candidateDirectives;
        }

        [NotNull]
        [ItemNotNull]
        public Directive[] CandidateDirectives { get; private set; }

        public bool CandidateDirectiveLockedIn { get; private set; }

        public bool SelectDirective(int presenceIndex, [NotNull] Tag tag, [NotNull] IEqualityComparer<string> comparer)
        {
            Debug.Assert(presenceIndex >= 0, "presenceIndex cannot be less than zero.");
            Debug.Assert(tag != null, "tag cannot be null.");
            Debug.Assert(comparer != null, "comparer cannot be null.");
            Debug.Assert(!CandidateDirectiveLockedIn, "Must not have locked in the candidate directive.");

            var options = CandidateDirectives.Where(d => d.Tags.Count > presenceIndex && d.Tags[presenceIndex].Equals(tag, comparer)).ToArray();
            if (options.Length > 0)
            {
                var firstFullySelected = options.FirstOrDefault(o => o.Tags.Count == presenceIndex + 1);

                if (firstFullySelected != null)
                {
                    CandidateDirectives = new[] { firstFullySelected };
                    CandidateDirectiveLockedIn = true;
                }
                else
                {
                    CandidateDirectives = options;
                }
            }

            return options.Length > 0;
        }
    }
}