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
//
using NUnit.Framework;

namespace XtraLiteTemplates.NUnit.Inside
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using XtraLiteTemplates.Parsing;
    using XtraLiteTemplates.Evaluation;

    public sealed class RippedOpenDirective : Directive
    {
        private class FlowState
        {
            public FlowDecision PreviousDecision;
            public Int32 Mode;
            public Tag PreviousTag;
            public Tag ExpectedTag;
        }

        private Tag[] m_myTags;

        public RippedOpenDirective(params Tag[] tags)
            : base(tags)
        {
            m_myTags = tags;
        }

        protected override FlowDecision Execute(Int32 tagIndex, Object[] components, 
            ref Object state, IVariableContext context, out String text)
        {
            Assert.GreaterOrEqual(tagIndex, 0);
            Assert.Less(tagIndex, m_myTags.Length);
            Assert.IsNotNull(components);
            Assert.IsNotNull(context);

            var tag = m_myTags[tagIndex];
            Assert.AreEqual(tag.ComponentCount, components.Length);


            if (state == null)
            {
                text = "-> ";
                text += String.Format("{{{0}}}", tag);

                /* Assuming first execution. */
                Assert.AreEqual(m_myTags[0], tag);

                if (m_myTags.Length > 1)
                {
                    state = new FlowState
                    {
                        Mode = 0,
                        ExpectedTag = m_myTags[1],
                        PreviousDecision = FlowDecision.Evaluate,
                        PreviousTag = tag,
                    };
                } 
                else
                {
                    state = new FlowState
                    {
                        Mode = 1,
                        ExpectedTag = m_myTags[0],
                        PreviousDecision = FlowDecision.Restart,
                        PreviousTag = tag,
                    };
                }

                text += String.Format(" -> {0} -> (", (state as FlowState).PreviousDecision);
                return (state as FlowState).PreviousDecision;
            } 
            else
            {
                text = String.Format(") -> {{{0}}}", tag);

                var stateAsFlow = state as FlowState;

                Assert.NotNull(stateAsFlow);
                Assert.AreSame(tag, stateAsFlow.ExpectedTag);
                Assert.AreNotEqual(FlowDecision.Terminate, stateAsFlow.PreviousDecision);

                if (stateAsFlow.PreviousDecision == FlowDecision.Evaluate || stateAsFlow.PreviousDecision == FlowDecision.Skip)
                {
                    Assert.AreEqual(m_myTags[tagIndex - 1], stateAsFlow.PreviousTag);
                }
                else if (stateAsFlow.PreviousDecision == FlowDecision.Restart)
                {
                    Assert.AreEqual(m_myTags[0], stateAsFlow.ExpectedTag);
                }

                if (m_myTags.Last() == tag)
                {
                    /* This is the last tag. */
                    if (stateAsFlow.Mode == 0)
                    {
                        stateAsFlow.Mode = 1;
                        stateAsFlow.PreviousDecision = FlowDecision.Restart;
                        stateAsFlow.PreviousTag = tag;
                        stateAsFlow.ExpectedTag = m_myTags[0];
                    } 
                    else
                    {
                        stateAsFlow.PreviousDecision = FlowDecision.Terminate;
                    }
                }
                else if (m_myTags.First() == tag)
                {
                    /* This is the last tag. */
                    if (stateAsFlow.Mode == 1)
                    {
                        stateAsFlow.PreviousDecision = FlowDecision.Skip;
                        stateAsFlow.PreviousTag = tag;
                        stateAsFlow.ExpectedTag = m_myTags[1];
                    }
                    else
                    {
                        stateAsFlow.PreviousDecision = FlowDecision.Terminate;
                    }
                }
                else
                {
                    stateAsFlow.PreviousTag = tag;
                    stateAsFlow.ExpectedTag = m_myTags[tagIndex + 1];
                }

                text += String.Format(" -> {0} ->", stateAsFlow.PreviousDecision);
                if (stateAsFlow.PreviousDecision != FlowDecision.Terminate)
                    text += " (";

                return stateAsFlow.PreviousDecision;
            }
        }
    }
}

