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

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Does not apply.")]

namespace XtraLiteTemplates.Evaluation
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Threading;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Introspection;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal sealed class EvaluationContextImpl : IExpressionEvaluationContext
    {
        private Stack<Frame> frames;
        private Dictionary<Type, SimpleTypeDisemboweler> disembowelers;
        private IEqualityComparer<string> identifierComparer;
        private IObjectFormatter objectFormatter;
        private bool ignoreEvaluationExceptions;
        private CancellationToken cancellationToken;
        private object selfObject;
        private Func<IExpressionEvaluationContext, string, string> unparsedTextHandler;

        public EvaluationContextImpl(
            bool ignoreEvaluationExceptions,
            CancellationToken cancellationToken,
            IEqualityComparer<string> identifierComparer,
            IObjectFormatter objectFormatter,
            object selfObject,
            Func<IExpressionEvaluationContext, string, string> unparsedTextHandler)
        {
            Debug.Assert(identifierComparer != null, "identifierComparer cannot be null.");
            Debug.Assert(objectFormatter != null, "objectFormatter cannot be null.");
            Debug.Assert(unparsedTextHandler != null, "unparsedTextHandler cannot be null.");

            this.cancellationToken = cancellationToken;
            this.identifierComparer = identifierComparer;
            this.objectFormatter = objectFormatter;
            this.ignoreEvaluationExceptions = ignoreEvaluationExceptions;
            this.unparsedTextHandler = unparsedTextHandler;
            this.selfObject = selfObject;

            this.disembowelers = new Dictionary<Type, SimpleTypeDisemboweler>();
            this.frames = new Stack<Frame>();
        }

        public bool IgnoreEvaluationExceptions
        {
            get
            {
                return this.ignoreEvaluationExceptions;
            }
        }

        public CancellationToken CancellationToken
        {
            get
            {
                return this.cancellationToken;
            }
        }

        internal Frame TopFrame
        {
            get
            {
                Debug.Assert(this.frames.Count > 0, "No open frames remaining.");
                return this.frames.Peek();
            }
        }

        public string ProcessUnparsedText(string value)
        {
            return this.unparsedTextHandler(this, value);
        }

        public void OpenEvaluationFrame()
        {
            this.frames.Push(new Frame());
        }

        public void CloseEvaluationFrame()
        {
            Debug.Assert(this.frames.Count > 0, "No open frames remaining.");
            this.frames.Pop();
        }

        public void SetProperty(string property, object value)
        {
            var topFrame = this.TopFrame;
            if (topFrame.Variables == null)
            {
                topFrame.Variables = new Dictionary<string, object>(this.identifierComparer);
            }

            topFrame.Variables[property] = value;
        }

        public object GetProperty(string property)
        {
            /* Obtain the propety from the list given to us. */
            foreach (var frame in this.frames)
            {
                object result;
                if (frame.Variables != null && frame.Variables.TryGetValue(property, out result))
                {
                    return result;
                }
            }

            /* Not there. Let's go to the self object now. */
            return this.GetProperty(this.selfObject, property);
        }

        public object GetProperty(object @object, string property)
        {
            Expect.Identifier("property", property);

            if (@object != null)
            {
                var type = @object.GetType();
                return this.GetDisembowelerForType(type).Invoke(@object, property);
            }
            else
            {
                return null;
            }
        }

        public object Invoke(string method, object[] arguments)
        {
            /* Go to self object. */
            return this.Invoke(this.selfObject, method, arguments);
        }

        public object Invoke(object @object, string method, object[] arguments)
        {
            Expect.Identifier("method", method);

            if (@object != null)
            {
                var type = @object.GetType();
                return this.GetDisembowelerForType(type).Invoke(@object, method, arguments);
            }
            else
            {
                return null;
            }
        }

        public void AddStateObject(object state)
        {
            var topFrame = this.TopFrame;
            if (topFrame.StateObjects == null)
            {
                topFrame.StateObjects = new HashSet<object>();
            }

            topFrame.StateObjects.Add(state);
        }

        public void RemoveStateObject(object state)
        {
            var topFrame = this.TopFrame;
            if (topFrame.StateObjects != null)
            {
                topFrame.StateObjects.Remove(state);
            }
        }

        public bool ContainsStateObject(object state)
        {
            var topFrame = this.TopFrame;
            return topFrame.StateObjects != null && topFrame.StateObjects.Contains(state);
        }

        private SimpleTypeDisemboweler GetDisembowelerForType(Type type)
        {
            Debug.Assert(type != null, "type cannot be null.");

            SimpleTypeDisemboweler disemboweler;
            if (!this.disembowelers.TryGetValue(type, out disemboweler))
            {
                disemboweler = new SimpleTypeDisemboweler(type, this.identifierComparer, this.objectFormatter);

                this.disembowelers.Add(type, disemboweler);
            }

            return disemboweler;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        public sealed class Frame
        {
            public Dictionary<string, object> Variables { get; set; }

            public HashSet<object> StateObjects { get; set; }
        }
    }
}
