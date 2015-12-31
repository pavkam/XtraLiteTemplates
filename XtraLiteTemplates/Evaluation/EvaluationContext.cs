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

    /// <summary>
    /// Provides a standard implementation of an evaluation context.
    /// </summary>
    public class EvaluationContext : IExpressionEvaluationContext
    {
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting private entities.")]
        private Stack<Frame> frames;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting private entities.")]
        private Dictionary<Type, SimpleTypeDisemboweler> disembowelers;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting private entities.")]
        private IEqualityComparer<string> identifierComparer;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting private entities.")]
        private IObjectFormatter objectFormatter;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting private entities.")]
        private bool ignoreEvaluationExceptions;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting private entities.")]
        private CancellationToken cancellationToken;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting private entities.")]
        private object selfObject;
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting private entities.")]
        private Func<IExpressionEvaluationContext, string, string> unparsedTextHandler;

        // TODO: TEST
        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationContext"/> class.
        /// </summary>
        /// <param name="ignoreEvaluationExceptions">If set to <c>true</c>, ignores evaluation exceptions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="identifierComparer">The identifier comparer.</param>
        /// <param name="objectFormatter">The object formatter.</param>
        /// <param name="selfObject">The <c>self</c> object that exposes all global identifiers.</param>
        /// <param name="unparsedTextHandler">The unparsed text handler delegate.</param>
        public EvaluationContext(
            bool ignoreEvaluationExceptions,
            CancellationToken cancellationToken,
            IEqualityComparer<string> identifierComparer,
            IObjectFormatter objectFormatter,
            object selfObject,
            Func<IExpressionEvaluationContext, string, string> unparsedTextHandler)
        {
            Expect.NotNull("identifierComparer", identifierComparer);
            Expect.NotNull("objectFormatter", objectFormatter);
            Expect.NotNull("unparsedTextHandler", unparsedTextHandler);

            this.cancellationToken = cancellationToken;
            this.identifierComparer = identifierComparer;
            this.objectFormatter = objectFormatter;
            this.ignoreEvaluationExceptions = ignoreEvaluationExceptions;
            this.unparsedTextHandler = unparsedTextHandler;
            this.selfObject = selfObject;

            this.disembowelers = new Dictionary<Type, SimpleTypeDisemboweler>();
            this.frames = new Stack<Frame>();

            this.OpenEvaluationFrame();
        }

        // TODO: TEST
        /// <summary>
        /// Gets a value indicating whether evaluation exceptions are ignored.
        /// </summary>
        /// <value>
        /// <c>true</c> if evaluation exceptions are ignored; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreEvaluationExceptions
        {
            get
            {
                return this.ignoreEvaluationExceptions;
            }
        }

        // TODO: TEST
        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        /// <value>
        /// The cancellation token that was supplied during construction.
        /// </value>
        public CancellationToken CancellationToken
        {
            get
            {
                return this.cancellationToken;
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting private entities.")]
        private Frame TopFrame
        {
            get
            {
                Debug.Assert(this.frames.Count > 0, "No open frames remaining.");
                return this.frames.Peek();
            }
        }

        // TODO: TEST
        /// <summary>
        /// Processes the unparsed text blocks.
        /// </summary>
        /// <param name="value">The unparsed text value.</param>
        /// <returns>The processed unparsed text.</returns>
        public string ProcessUnparsedText(string value)
        {
            return this.unparsedTextHandler(this, value);
        }

        // TODO: TEST
        /// <summary>
        /// Sets the value of context property (variable).
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="property" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="property" /> is not a valid identifier.</exception>
        public void SetProperty(string property, object value)
        {
            var topFrame = this.TopFrame;
            if (topFrame.Variables == null)
            {
                topFrame.Variables = new Dictionary<string, object>(this.identifierComparer);
            }

            topFrame.Variables[property] = value;
        }

        // TODO: TEST
        /// <summary>
        /// Gets the value of context property (variable).
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <returns>
        /// The property value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="property" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="property" /> is not a valid identifier.</exception>
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

        // TODO: TEST
        /// <summary>
        /// Gets the value of an object's property.
        /// </summary>
        /// <param name="object">The object to get the property for.</param>
        /// <param name="property">The property name.</param>
        /// <returns>
        /// The property value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="property" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="property" /> is not a valid identifier.</exception>
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

        // TODO: TEST
        /// <summary>
        /// Invokes a context method (global).
        /// </summary>
        /// <param name="method">The method name.</param>
        /// <param name="arguments">The arguments to be passed to the method. <c>null</c> value means no arguments.</param>
        /// <returns>
        /// The return value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="method" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="method" /> is not a valid identifier.</exception>
        public object Invoke(string method, object[] arguments)
        {
            /* Go to self object. */
            return this.Invoke(this.selfObject, method, arguments);
        }

        // TODO: TEST
        /// <summary>
        /// Invokes an object's method.
        /// </summary>
        /// <param name="object">The object.</param>
        /// <param name="method">The method name.</param>
        /// <param name="arguments">The arguments to be passed to the method. <c>null</c> value means no arguments.</param>
        /// <returns>
        /// The return value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="method" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="method" /> is not a valid identifier.</exception>
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

        // TODO: TEST
        /// <summary>
        /// Adds a state object.
        /// </summary>
        /// <param name="state">The state object to add.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="state" /> is <c>null</c>.</exception>
        /// <remarks>
        /// State objects can represent anything and are a simple way of storing state information for special operators
        /// or directives.
        /// </remarks>
        public void AddStateObject(object state)
        {
            var topFrame = this.TopFrame;
            if (topFrame.StateObjects == null)
            {
                topFrame.StateObjects = new HashSet<object>();
            }

            topFrame.StateObjects.Add(state);
        }

        // TODO: TEST
        /// <summary>
        /// Removes a state object.
        /// </summary>
        /// <param name="state">The state object to remove.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="state" /> is <c>null</c>.</exception>
        /// <remarks>
        /// State objects can represent anything and are a simple way of storing state information for special operators
        /// or directives.
        /// </remarks>
        public void RemoveStateObject(object state)
        {
            var topFrame = this.TopFrame;
            if (topFrame.StateObjects != null)
            {
                topFrame.StateObjects.Remove(state);
            }
        }

        // TODO: TEST
        /// <summary>
        /// Determines whether a given state object was registered in this context.
        /// </summary>
        /// <param name="state">The state object to check for.</param>
        /// <returns>
        ///   <c>true</c> if the state object was added; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="state" /> is <c>null</c>.</exception>
        public bool ContainsStateObject(object state)
        {
            var topFrame = this.TopFrame;
            return topFrame.StateObjects != null && topFrame.StateObjects.Contains(state);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        internal void OpenEvaluationFrame()
        {
            this.frames.Push(new Frame());
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        internal void CloseEvaluationFrame()
        {
            Debug.Assert(this.frames.Count > 0, "No open frames remaining.");
            this.frames.Pop();
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting private entities.")]
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

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting private entities.")]
        private sealed class Frame
        {
            public Dictionary<string, object> Variables { get; set; }

            public HashSet<object> StateObjects { get; set; }
        }
    }
}
