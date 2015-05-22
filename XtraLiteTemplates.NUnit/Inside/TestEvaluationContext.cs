﻿//
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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;

    public class TestEvaluationContext : IEvaluationContext
    {
        private sealed class Frame
        {
            public Dictionary<String, Object> Variables;
            public HashSet<Object> StateObjects;
        }

        private Stack<Frame> m_frames;
        private Dictionary<Type, SimpleTypeDisemboweler> m_disembowelers;
        private IEqualityComparer<String> m_identifierComparer;
        private Boolean m_ignoreEvaluationExceptions;

        public TestEvaluationContext(IEqualityComparer<String> identifierComparer)
        {
            Debug.Assert(identifierComparer != null);

            m_identifierComparer = identifierComparer;
            m_frames = new Stack<Frame>();
            m_disembowelers = new Dictionary<Type, SimpleTypeDisemboweler>();
        }

        public String ProcessUnparsedText(String value)
        {
            return value;
        }

        private Frame TopFrame
        {
            get
            {
                Assert.Greater(m_frames.Count, 0);

                return m_frames.Peek();
            }
        }

        public void OpenEvaluationFrame()
        {
            m_frames.Push(new Frame());
        }

        public void CloseEvaluationFrame()
        {
            Assert.Greater(m_frames.Count, 0);

            m_frames.Pop();
        }

        public void SetVariable(String identifier, Object value)
        {
            Assert.IsNotEmpty(identifier);

            var topFrame = TopFrame;
            if (topFrame.Variables == null)
                topFrame.Variables = new Dictionary<String, Object>(m_identifierComparer);

            topFrame.Variables[identifier] = value;
        }

        public Object GetVariable(String identifier)
        {
            Assert.IsNotEmpty(identifier);

            foreach (var frame in m_frames)
            {
                Object result;
                if (frame.Variables != null && frame.Variables.TryGetValue(identifier, out result))
                    return result;
            }

            return null;
        }

        public bool IgnoreEvaluationExceptions
        {
            get
            {
                return true;
            }
        }

        public void AddStateObject(Object state)
        {
            Assert.NotNull(state);

            var topFrame = TopFrame;
            if (topFrame.StateObjects == null)
                topFrame.StateObjects = new HashSet<Object>();

            topFrame.StateObjects.Add(state);
        }

        public void RemoveStateObject(Object state)
        {
            Assert.NotNull(state);

            var topFrame = TopFrame;
            if (topFrame.StateObjects != null)
                topFrame.StateObjects.Remove(state);
        }

        public Boolean ContainsStateObject(Object state)
        {
            Assert.NotNull(state);

            var topFrame = TopFrame;
            return topFrame.StateObjects != null && topFrame.StateObjects.Contains(state);
        }


        public Object GetProperty(Object variable, String memberName)
        {
            Assert.IsNotEmpty(memberName);

            if (variable != null)
            {
                var type = variable.GetType();

                SimpleTypeDisemboweler disemboweler;
                if (!m_disembowelers.TryGetValue(type, out disemboweler))
                {
                    disemboweler = new SimpleTypeDisemboweler(type,
                        SimpleTypeDisemboweler.EvaluationOptions.TreatAllErrorsAsNull |
                        SimpleTypeDisemboweler.EvaluationOptions.TreatParameterlessFunctionsAsProperties, m_identifierComparer);

                    m_disembowelers.Add(type, disemboweler);
                }

                return disemboweler.Read(memberName, variable);
            }
            else
                return null;
        }
    }
}
