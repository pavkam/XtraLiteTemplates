//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.Expressions
{
    using System;
    using System.Threading;

    /// <summary>
    /// Defines a standard set of behaviors that have to be implemented by an expression evaluation context.
    /// </summary>
    public interface IExpressionEvaluationContext
    {
        /// <summary>
        /// Gets the cancellation token associated with this context.
        /// </summary>
        /// <value>
        /// The cancellation token is used to signal the expression evaluation engine to stop execution.
        /// </value>
        CancellationToken CancellationToken { get; }

        /// <summary>
        /// Sets the value of context property (variable).
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <param name="value">The property value.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="property" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="property" /> is not a valid identifier.</exception>
        void SetProperty(string property, object value);

        /// <summary>
        /// Gets the value of context property (variable).
        /// </summary>
        /// <param name="property">The property name.</param>
        /// <returns>
        /// The property value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="property" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="property" /> is not a valid identifier.</exception>
        object GetProperty(string property);

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
        object GetProperty(object @object, string property);

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
        object Invoke(string method, object[] arguments);

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
        object Invoke(object @object, string method, object[] arguments);

        /// <summary>
        /// Adds a state object.
        /// </summary>
        /// <param name="state">The state object to add.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="state" /> is <c>null</c>.</exception>
        /// <remarks>
        /// State objects can represent anything and are a simple way of storing state information for special operators
        /// or directives.
        /// </remarks>
        void AddStateObject(object state);

        /// <summary>
        /// Removes a state object.
        /// </summary>
        /// <param name="state">The state object to remove.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="state" /> is <c>null</c>.</exception>
        /// <remarks>
        /// State objects can represent anything and are a simple way of storing state information for special operators
        /// or directives.
        /// </remarks>
        void RemoveStateObject(object state);

        /// <summary>
        /// Determines whether a given state object was registered in this context.
        /// </summary>
        /// <param name="state">The state object to check for.</param>
        /// <returns>
        ///   <c>true</c> if the state object was added; <c>false</c> otherwise.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="state" /> is <c>null</c>.</exception>
        bool ContainsStateObject(object state);
    }
}