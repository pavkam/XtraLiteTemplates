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
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Threading;
    using Expressions;
    using Introspection;

    /// <summary>
    /// Provides a standard implementation of an evaluation context.
    /// </summary>
    public class EvaluationContext : IExpressionEvaluationContext
    {
        private readonly Stack<Frame> _frames;
        private readonly Dictionary<Type, SimpleTypeDisemboweler> _disembowelers;
        private readonly IEqualityComparer<string> _identifierComparer;
        private readonly IObjectFormatter _objectFormatter;

        private readonly object _selfObject;
        private readonly Func<IExpressionEvaluationContext, string, string> _unParsedTextHandler;

        /// <summary>
        /// Initializes a new instance of the <see cref="EvaluationContext"/> class.
        /// </summary>
        /// <param name="ignoreEvaluationExceptions">If set to <c>true</c>, ignores evaluation exceptions.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <param name="identifierComparer">The identifier comparer.</param>
        /// <param name="objectFormatter">The object formatter.</param>
        /// <param name="selfObject">The <c>self</c> object that exposes all global identifiers.</param>
        /// <param name="unParsedTextHandler">The un-parsed text handler delegate.</param>
        public EvaluationContext(
            bool ignoreEvaluationExceptions,
            CancellationToken cancellationToken,
            IEqualityComparer<string> identifierComparer,
            IObjectFormatter objectFormatter,
            object selfObject,
            Func<IExpressionEvaluationContext, string, string> unParsedTextHandler)
        {
            Expect.NotNull("identifierComparer", identifierComparer);
            Expect.NotNull("objectFormatter", objectFormatter);
            Expect.NotNull("unParsedTextHandler", unParsedTextHandler);

            CancellationToken = cancellationToken;
            _identifierComparer = identifierComparer;
            _objectFormatter = objectFormatter;
            IgnoreEvaluationExceptions = ignoreEvaluationExceptions;
            _unParsedTextHandler = unParsedTextHandler;
            _selfObject = selfObject;

            _disembowelers = new Dictionary<Type, SimpleTypeDisemboweler>();
            _frames = new Stack<Frame>();

            OpenEvaluationFrame();
        }

        /// <summary>
        /// Gets a value indicating whether evaluation exceptions are ignored.
        /// </summary>
        /// <value>
        /// <c>true</c> if evaluation exceptions are ignored; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreEvaluationExceptions { get; }

        /// <summary>
        /// Gets the cancellation token.
        /// </summary>
        /// <value>
        /// The cancellation token that was supplied during construction.
        /// </value>
        public CancellationToken CancellationToken { get; }

        private Frame TopFrame
        {
            get
            {
                Debug.Assert(_frames.Count > 0, "No open frames remaining.");
                return _frames.Peek();
            }
        }

        /// <summary>
        /// Processes the un-parsed text blocks.
        /// </summary>
        /// <param name="value">The un-parsed text value.</param>
        /// <returns>The processed un-parsed text.</returns>
        public string ProcessUnParsedText(string value)
        {
            return _unParsedTextHandler(this, value);
        }

        /// <summary>
        /// Sets the value of context property (variable).
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="property" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="property" /> is not a valid identifier.</exception>
        public void SetProperty(string property, object value)
        {
            Expect.Identifier("property", property);

            var topFrame = TopFrame;
            if (topFrame.Variables == null)
            {
                topFrame.Variables = new Dictionary<string, object>(_identifierComparer);
            }

            topFrame.Variables[property] = value;
        }

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
            Expect.Identifier("property", property);

            object result;
            if (!TryGetProperty(property, out result))
            {
                result = GetProperty(_selfObject, property);
            }

            return result;
        }

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
                return GetDisembowelerForType(type).Invoke(@object, property);
            }

            return null;
        }

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
            Expect.Identifier("method", method);
            
            if (arguments == null || arguments.Length == 0)
            {
                object result;
                if (TryGetProperty(method, out result))
                {
                    return result;
                }
            }

            /* Go to self object. */
            return Invoke(_selfObject, method, arguments);
        }

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
                return GetDisembowelerForType(type).Invoke(@object, method, arguments);
            }

            return null;
        }

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
            Expect.NotNull("state", state);

            var topFrame = TopFrame;
            if (topFrame.StateObjects == null)
            {
                topFrame.StateObjects = new HashSet<object>();
            }

            topFrame.StateObjects.Add(state);
        }

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
            Expect.NotNull("state", state);

            var topFrame = TopFrame;
            topFrame.StateObjects?.Remove(state);
        }

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
            Expect.NotNull("state", state);

            var topFrame = TopFrame;
            return topFrame.StateObjects != null && topFrame.StateObjects.Contains(state);
        }

        internal void OpenEvaluationFrame()
        {
            _frames.Push(new Frame());
        }

        internal void CloseEvaluationFrame()
        {
            Debug.Assert(_frames.Count > 0, "No open frames remaining.");
            _frames.Pop();
        }

        private bool TryGetProperty(string property, out object value)
        {
            /* Obtain the property from the list given to us. */
            foreach (var frame in _frames)
            {
                if (frame.Variables != null && frame.Variables.TryGetValue(property, out value))
                {
                    return true;
                }
            }

            value = null;
            return false;
        }

        private SimpleTypeDisemboweler GetDisembowelerForType(Type type)
        {
            Debug.Assert(type != null, "type cannot be null.");

            SimpleTypeDisemboweler disemboweler;
            if (!_disembowelers.TryGetValue(type, out disemboweler))
            {
                disemboweler = new SimpleTypeDisemboweler(type, _identifierComparer, _objectFormatter);

                _disembowelers.Add(type, disemboweler);
            }

            return disemboweler;
        }

        private sealed class Frame
        {
            public Dictionary<string, object> Variables { get; set; }

            public HashSet<object> StateObjects { get; set; }
        }
    }
}
