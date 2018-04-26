//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2018, Alexandru Ciobanu (alex+git@ciobanu.org)
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
    using System.Dynamic;
    using System.Threading;
    using JetBrains.Annotations;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Introspection;

    /// <inheritdoc />
    /// <summary>
    /// Provides a standard implementation of an evaluation context.
    /// </summary>
    [PublicAPI]
    public class EvaluationContext : IExpressionEvaluationContext
    {
        [NotNull]
        private readonly Stack<Frame> _frames;
        [NotNull]
        private readonly Dictionary<Type, SimpleTypeDisemboweler> _disembowelers;
        [NotNull]
        private readonly SimpleDynamicInvoker _dynamicInvoker;
        [NotNull]
        private readonly IEqualityComparer<string> _identifierComparer;
        [NotNull]
        private readonly IObjectFormatter _objectFormatter;
        [CanBeNull]
        private readonly object _selfObject;
        [NotNull]
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
            [NotNull] IEqualityComparer<string> identifierComparer,
            [NotNull] IObjectFormatter objectFormatter,
            [CanBeNull] object selfObject,
            [NotNull] Func<IExpressionEvaluationContext, string, string> unParsedTextHandler)
        {
            Expect.NotNull(nameof(identifierComparer), identifierComparer);
            Expect.NotNull(nameof(objectFormatter), objectFormatter);
            Expect.NotNull(nameof(unParsedTextHandler), unParsedTextHandler);

            CancellationToken = cancellationToken;
            _identifierComparer = identifierComparer;
            _objectFormatter = objectFormatter;
            IgnoreEvaluationExceptions = ignoreEvaluationExceptions;
            _unParsedTextHandler = unParsedTextHandler;
            _selfObject = selfObject;

            _disembowelers = new Dictionary<Type, SimpleTypeDisemboweler>();
            _frames = new Stack<Frame>();
            _dynamicInvoker = new SimpleDynamicInvoker();

            OpenEvaluationFrame();
        }

        /// <summary>
        /// Gets a value indicating whether evaluation exceptions are ignored.
        /// </summary>
        /// <value>
        /// <c>true</c> if evaluation exceptions are ignored; otherwise, <c>false</c>.
        /// </value>
        public bool IgnoreEvaluationExceptions { get; }

        /// <inheritdoc />
        public CancellationToken CancellationToken { get; }

        [NotNull]
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
        public string ProcessUnParsedText([CanBeNull] string value)
        {
            return _unParsedTextHandler(this, value);
        }

        /// <inheritdoc />
        public void SetProperty(string property, object value)
        {
            Expect.Identifier(nameof(property), property);

            var topFrame = TopFrame;
            if (topFrame.Variables == null)
            {
                topFrame.Variables = new Dictionary<string, object>(_identifierComparer);
            }

            topFrame.Variables[property] = value;
        }

        /// <inheritdoc />
        public object GetProperty(string property)
        {
            Expect.Identifier(nameof(property), property);

            if (!TryGetProperty(property, out var result))
            {
                result = GetProperty(_selfObject, property);
            }

            return result;
        }

        /// <inheritdoc />
        public object GetProperty([CanBeNull] object @object, string property)
        {
            Expect.Identifier(nameof(property), property);

            if (@object == null)
            {
                return null;
            }

            var objectType = GetTypeOfObject(@object);

            return typeof(IDynamicMetaObjectProvider).IsAssignableFrom(objectType) ? 
                       _dynamicInvoker.GetValue(@object, property) : 
                       GetDisembowelerForType(objectType).Invoke(@object, property);
        }

        /// <inheritdoc />
        public object Invoke(string method, [CanBeNull] object[] arguments)
        {
            Expect.Identifier(nameof(method), method);
            
            if (arguments == null || arguments.Length == 0)
            {
                if (TryGetProperty(method, out var result))
                {
                    return result;
                }
            }

            /* Go to self object. */
            return Invoke(_selfObject, method, arguments);
        }

        /// <inheritdoc />
        public object Invoke([CanBeNull] object @object, string method, [CanBeNull] object[] arguments)
        {
            Expect.Identifier(nameof(method), method);

            if (@object == null)
            {
                return null;
            }

            var objectType = GetTypeOfObject(@object);

            return typeof(IDynamicMetaObjectProvider).IsAssignableFrom(objectType)
                       ? _dynamicInvoker.Invoke(@object, method, arguments)
                       : GetDisembowelerForType(objectType).Invoke(@object, method, arguments);
        }

        /// <inheritdoc />
        public void AddStateObject(object state)
        {
            Expect.NotNull(nameof(state), state);

            var topFrame = TopFrame;
            if (topFrame.StateObjects == null)
            {
                topFrame.StateObjects = new HashSet<object>();
            }

            topFrame.StateObjects.Add(state);
        }

        /// <inheritdoc />
        public void RemoveStateObject(object state)
        {
            Expect.NotNull(nameof(state), state);

            var topFrame = TopFrame;
            topFrame.StateObjects?.Remove(state);
        }

        /// <inheritdoc />
        public bool ContainsStateObject(object state)
        {
            Expect.NotNull(nameof(state), state);

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

        private bool TryGetProperty([NotNull] string property, [CanBeNull] out object value)
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

        [NotNull]
        private Type GetTypeOfObject([NotNull] object @object)
        {
            Debug.Assert(@object != null, "Object cannot be null.");

            return @object is Static @static ? @static.ExposedType : @object.GetType();
        }

        [NotNull]
        private SimpleTypeDisemboweler GetDisembowelerForType([NotNull] Type type)
        {
            Debug.Assert(type != null, "type cannot be null.");

            if (!_disembowelers.TryGetValue(type, out var disemboweler))
            {
                disemboweler = new SimpleTypeDisemboweler(type, _identifierComparer, _objectFormatter);

                _disembowelers.Add(type, disemboweler);
            }

            return disemboweler;
        }

        private sealed class Frame
        {
            [CanBeNull]
            public Dictionary<string, object> Variables { get; set; }

            [CanBeNull]
            public HashSet<object> StateObjects { get; set; }
        }
    }
}
