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
namespace XtraLiteTemplates.Expressions
{
    using System;

    /// <summary>
    /// Defines a standard set of behaviors that have to be implemented by an expression evaluation context.
    /// </summary>
    public interface IExpressionEvaluationContext
    {
        /// <summary>
        /// Sets the value of a variable.
        /// </summary>
        /// <param name="identifier">The variable name.</param>
        /// <param name="value">The variable value.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="identifier"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="identifier"/> is not a valid identifier.</exception>
        void SetVariable(string identifier, object value);

        /// <summary>
        /// Gets the value of a variable.
        /// </summary>
        /// <param name="identifier">The variable name.</param>
        /// <returns>The variable value.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="identifier" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="identifier" /> is not a valid identifier.</exception>
        object GetVariable(string identifier);

        /// <summary>
        /// Gets the value of an object's property.
        /// </summary>
        /// <param name="variable">The variable.</param>
        /// <param name="memberName">Name of the property to read.</param>
        /// <returns>The value of the property.</returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="memberName" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="memberName" /> is not a valid identifier.</exception>
        object GetProperty(object variable, string memberName);

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