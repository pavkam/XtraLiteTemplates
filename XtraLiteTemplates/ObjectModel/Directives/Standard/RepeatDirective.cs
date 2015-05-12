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

namespace XtraLiteTemplates.ObjectModel.Directives.Standard
{
    using System;
    using System.Diagnostics;
    using XtraLiteTemplates.Parsing;

    public sealed class RepeatDirective : Directive
    {
        public RepeatDirective() : 
            base(Tag.Parse("REPEAT $"), Tag.Parse("END"))
        {
        }

        protected internal override FlowDecision Execute(Tag tag, Object[] components, ref Object state,
            IDirectiveEvaluationContext context, out String text)
        {
            Debug.Assert(tag != null);
            Debug.Assert(components != null);
            Debug.Assert(context != null);
            Debug.Assert(components.Length == tag.ComponentCount);

            text = null;

            if (tag == Tags[0])
            {
                Int64 remainingIterations = 0;

                if (state != null)
                {
                    Debug.Assert(state is Int64);
                    remainingIterations = (Int64)state;
                }
                else if (components[1] is Int64)
                    remainingIterations = (Int64)components[1];

                state = remainingIterations - 1;

                if (remainingIterations > 0)
                    return FlowDecision.Evaluate;
            }
            else if (tag == Tags[1])
                return FlowDecision.Restart;

            return FlowDecision.Terminate;
        }
    }
}

