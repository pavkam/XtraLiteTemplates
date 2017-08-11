//
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
//
namespace XtraLiteTemplates.Tests.Inside
{
    using System.Linq;

    using global::NUnit.Framework;

    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Parsing;

    public sealed class RippedOpenDirective : Directive
    {
        private class FlowState
        {
            public FlowDecision PreviousDecision;
            public int Mode;
            public Tag PreviousTag;
            public Tag ExpectedTag;
        }

        private readonly Tag[] _mMyTags;

        public RippedOpenDirective(params Tag[] tags)
            : base(tags)
        {
            _mMyTags = tags;
        }

        protected override FlowDecision Execute(int tagIndex, object[] components,
            ref object state, IExpressionEvaluationContext context, out string text)
        {
            Assert.GreaterOrEqual(tagIndex, 0);
            Assert.Less(tagIndex, _mMyTags.Length);
            Assert.IsNotNull(components);
            Assert.IsNotNull(context);

            var tag = _mMyTags[tagIndex];
            Assert.AreEqual(tag.ComponentCount, components.Length);


            if (state == null)
            {
                text = "-> ";
                text += $"{{{tag}}}";

                /* Assuming first execution. */
                Assert.AreEqual(_mMyTags[0], tag);

                if (_mMyTags.Length > 1)
                {
                    state = new FlowState
                    {
                        Mode = 0,
                        ExpectedTag = _mMyTags[1],
                        PreviousDecision = FlowDecision.Evaluate,
                        PreviousTag = tag
                    };
                } 
                else
                {
                    state = new FlowState
                    {
                        Mode = 1,
                        ExpectedTag = _mMyTags[0],
                        PreviousDecision = FlowDecision.Restart,
                        PreviousTag = tag
                    };
                }

                text += $" -> {((FlowState)state).PreviousDecision} -> (";
                return ((FlowState)state).PreviousDecision;
            }

            text = $") -> {{{tag}}}";

            var stateAsFlow = state as FlowState;

            Assert.NotNull(stateAsFlow);
            Assert.AreSame(tag, stateAsFlow.ExpectedTag);
            Assert.AreNotEqual(FlowDecision.Terminate, stateAsFlow.PreviousDecision);

            switch (stateAsFlow.PreviousDecision)
            {
                case FlowDecision.Evaluate:
                case FlowDecision.Skip:
                    Assert.AreEqual(_mMyTags[tagIndex - 1], stateAsFlow.PreviousTag);
                    break;
                case FlowDecision.Restart:
                    Assert.AreEqual(_mMyTags[0], stateAsFlow.ExpectedTag);
                    break;
            }

            if (Equals(_mMyTags.Last(), tag))
            {
                /* This is the last tag. */
                if (stateAsFlow.Mode == 0)
                {
                    stateAsFlow.Mode = 1;
                    stateAsFlow.PreviousDecision = FlowDecision.Restart;
                    stateAsFlow.PreviousTag = tag;
                    stateAsFlow.ExpectedTag = _mMyTags[0];
                } 
                else
                {
                    stateAsFlow.PreviousDecision = FlowDecision.Terminate;
                }
            }
            else if (Equals(_mMyTags.First(), tag))
            {
                /* This is the last tag. */
                if (stateAsFlow.Mode == 1)
                {
                    stateAsFlow.PreviousDecision = FlowDecision.Skip;
                    stateAsFlow.PreviousTag = tag;
                    stateAsFlow.ExpectedTag = _mMyTags[1];
                }
                else
                {
                    stateAsFlow.PreviousDecision = FlowDecision.Terminate;
                }
            }
            else
            {
                stateAsFlow.PreviousTag = tag;
                stateAsFlow.ExpectedTag = _mMyTags[tagIndex + 1];
            }

            text += $" -> {stateAsFlow.PreviousDecision} ->";
            if (stateAsFlow.PreviousDecision != FlowDecision.Terminate)
                text += " (";

            return stateAsFlow.PreviousDecision;
        }
    }
}

