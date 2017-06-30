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
    using System.Linq;
    using System.Text;    
    using Expressions;
    using JetBrains.Annotations;
    using Parsing;

    /// <summary>
    /// Abstract base class for all supported directives.
    /// </summary>
    [PublicAPI]
    public abstract class Directive
    {
        [NotNull]
        [ItemNotNull]
        private readonly List<Tag> _directiveComponentTags;

        /// <summary>
        /// Initializes a new instance of the <see cref="Directive"/> class.
        /// </summary>
        /// <param name="tags">The tags that make up the directive.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="tags"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="tags"/> is empty.</exception>
        /// <exception cref="InvalidOperationException">One or more tags have no defined components.</exception>
        protected Directive([NotNull] [ItemNotNull] params Tag[] tags)
        {
            Expect.NotEmpty(nameof(tags), tags);

            foreach (var tag in tags)
            {
                Expect.NotNull("tag", tag);

                if (tag.ComponentCount == 0)
                {
                    ExceptionHelper.CannotRegisterTagWithNoComponents();
                }
            }

            _directiveComponentTags = new List<Tag>(tags);
        }

        /// <summary>
        /// Defines the directive execution flow.
        /// </summary>
        protected internal enum FlowDecision
        {
            /// <summary>
            /// Terminate execution of directive immediately after.
            /// </summary>
            Terminate = 0,

            /// <summary>
            /// Restart the execution of the directive immediately by jumping to the first tag.
            /// </summary>
            Restart = 1,

            /// <summary>
            /// Evaluate the all child directives and un-parsed text blocks between the current tag and the following one.
            /// </summary>
            Evaluate = 2,

            /// <summary>
            /// Skip directly to the next following tag in the directive.
            /// </summary>
            Skip = 3,
        }

        [NotNull]
        internal IReadOnlyList<Tag> Tags => _directiveComponentTags;

        /// <summary>
        /// Returns a human-readable representation of the current directive object.
        /// </summary>
        /// <returns>
        /// A string that represents the current directive.
        /// </returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var tag in _directiveComponentTags)
            {
                if (sb.Length > 0)
                {
                    sb.Append("...");
                }

                sb.AppendFormat("{{{0}}}", tag);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to the current directive object using the provided comparer.
        /// <remarks>
        /// Two directives are equal if all their corresponding tags are equal and in the same order.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to compare with the current directive object.</param>
        /// <param name="comparer">The keyword and identifier comparer.</param>
        /// <returns>
        ///   <c>true</c> if the specified object is equal to the current directive; otherwise, <c>false</c>.
        /// </returns>
        public bool Equals([CanBeNull] object obj, [NotNull] IEqualityComparer<string> comparer)
        {
            Expect.NotNull(nameof(comparer), comparer);

            if (obj == null || obj.GetType() != GetType())
            {
                return false;
            }

            var directiveObj = (Directive)obj;
            if (ReferenceEquals(directiveObj, this))
            {
                return true;
            }

            if (directiveObj._directiveComponentTags.Count != _directiveComponentTags.Count)
            {
                return false;
            }

            return !_directiveComponentTags.Where((t, i) => !t.Equals(directiveObj._directiveComponentTags[i], comparer)).Any();
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to the current directive object using current culture comparing rules.
        /// <remarks>
        /// Two directives are equal is all their corresponding tags are equal and in the same order.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to compare with the current directive object.</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current directive; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj, StringComparer.CurrentCulture);
        }

        /// <summary>
        /// Calculates the hash of the current directive using the provided comparer.
        /// </summary>
        /// <param name="comparer">The keyword and identifier comparer.</param>
        /// <returns>
        /// A hash code for the current directive object.
        /// </returns>
        public int GetHashCode([NotNull] IEqualityComparer<string> comparer)
        {
            Expect.NotNull(nameof(comparer), comparer);

            var hash = 73; /* Magic constant */
            unchecked
            {
                hash = Tags.Aggregate(hash, (current, tag) => current * 51 + tag.GetHashCode(comparer));
            }

            return hash;
        }

        /// <summary>
        /// Calculates the hash of the current directive using the current culture comparing rules.
        /// </summary>
        /// <returns>
        /// A hash code for the current directive object.
        /// </returns>
        public override int GetHashCode()
        {
            return GetHashCode(StringComparer.CurrentCulture);
        }

        /// <summary>
        /// Executes the current directive. This method can be invoked multiple times by the evaluation environment depending on
        /// the particular directive implementation.
        /// </summary>
        /// <param name="tagIndex">The index of the tag that triggered the execution.</param>
        /// <param name="components">The tag components as provided by the lexical analyzer.</param>
        /// <param name="state">A general-purpose state object. Initially set to <c>null</c>.</param>
        /// <param name="context">The evaluation context.</param>
        /// <param name="text">An optional text generated by the directive.</param>
        /// <returns>A value indicating the next step for the evaluation environment.</returns>
        protected internal abstract FlowDecision Execute(
            int tagIndex,
            [NotNull] object[] components,
            [CanBeNull] ref object state,
            [NotNull] IExpressionEvaluationContext context, 
            [CanBeNull] out string text);
    }
}