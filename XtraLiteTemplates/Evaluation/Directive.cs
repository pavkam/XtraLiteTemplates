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

namespace XtraLiteTemplates.Evaluation
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;
    using XtraLiteTemplates.Parsing;
    using XtraLiteTemplates.Expressions;

    /// <summary>
    /// Abstract base class for all supported directives.
    /// </summary>
    public abstract class Directive
    {
        /// <summary>
        /// Defines the directive execution flow.
        /// </summary>
        protected internal enum FlowDecision
        {
            /// <summary>
            /// Terminate execution of diretive immediately after.
            /// </summary>
            Terminate,
            /// <summary>
            /// Restart the execution of the directive immediately by jumping to the first tag.
            /// </summary>
            Restart,
            /// <summary>
            /// Evaluate the all child directives and uparsed text blocks between the current tag and the following one.
            /// </summary>
            Evaluate,
            /// <summary>
            /// Skip directly to the next following tag in the directive.
            /// </summary>
            Skip,
        }

        private readonly List<Tag> m_tags;

        internal IReadOnlyList<Tag> Tags
        {
            get 
            {
                return m_tags;
            }
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="Directive"/> class.
        /// </summary>
        /// <param name="tags">The tags that make up the directive.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="tags"/> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="tags"/> is empty.</exception>
        /// <exception cref="InvalidOperationException">One or more tags have no defined components.</exception>
        public Directive(params Tag[] tags)
        {
            Expect.NotEmpty("tags", tags);

            foreach (var tag in tags)
            {
                Expect.NotNull("tag", tag);

                if (tag.ComponentCount == 0)
                    ExceptionHelper.CannotRegisterTagWithNoComponents();
            }

            m_tags = new List<Tag>(tags);
        }

        /// <summary>
        /// Returns a human-readable representation of the current directive object.
        /// </summary>
        /// <returns>
        /// A string that represents the current directive.
        /// </returns>
        public override String ToString()
        {
            StringBuilder sb = new StringBuilder();
            foreach (var tag in m_tags)
            {
                if (sb.Length > 0)
                    sb.Append("...");

                sb.AppendFormat("{{{0}}}", tag.ToString());
            }

            return sb.ToString();
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object" /> is equal to the current directive object using the provided comparer.
        /// <remarks>
        /// Two directives are equal is all their corresponding tags are equal and in the same order.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to compare with the current directive object.</param>
        /// <param name="comparer">The keyword and identifier comparer.</param>
        /// <returns>
        ///   <c>true</c> if the specified object is equal to the current directive; otherwise, <c>false</c>.
        /// </returns>
        public Boolean Equals(Object obj, IEqualityComparer<String> comparer)
        {
            Expect.NotNull("comparer", comparer);

            var directiveObj = obj as Directive;
            if (directiveObj == null || directiveObj.m_tags.Count != directiveObj.m_tags.Count)
                return false;
            else if (directiveObj == this)
                return true;

            for (var i = 0; i < m_tags.Count; i++)
            {
                if (!m_tags[i].Equals(directiveObj.m_tags[i], comparer))
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object" /> is equal to the current directive object using current culture comparing rules.
        /// <remarks>
        /// Two directives are equal is all their corresponding tags are equal and in the same order.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to compare with the current directive object.</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current directive; otherwise, <c>false</c>.
        /// </returns>
        public override Boolean Equals(Object obj)
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
        public Int32 GetHashCode(IEqualityComparer<String> comparer)
        {
            Expect.NotNull("comparer", comparer);

            var hash = 73; /* Magic constant */
            unchecked
            {
                foreach (var tag in Tags)
                    hash = hash * 51 + tag.GetHashCode(comparer);
            }

            return hash;
        }

        /// <summary>
        /// Calculates the hash of the current directive using the current culture comparing rules.
        /// </summary>
        /// <returns>
        /// A hash code for the current directive object.
        /// </returns>
        public override Int32 GetHashCode()
        {
            return GetHashCode(StringComparer.CurrentCulture);
        }

        /// <summary>
        /// Executes the current directive. This method can be invoken multiple times by the evaluation environment depending on
        /// the particular directive implementation.
        /// </summary>
        /// <param name="tagIndex">The index of the tag that triggered the execution.</param>
        /// <param name="components">The tag components as provided by the lexical analyzer.</param>
        /// <param name="state">A general-purpose state object. Initially set to <c>null</c>.</param>
        /// <param name="context">The evaluation context.</param>
        /// <param name="text">An optional text generated by the directive.</param>
        /// <returns>A value indicating the next step for the evaluation environment.</returns>
        protected internal abstract FlowDecision Execute(Int32 tagIndex, Object[] components, 
            ref Object state, IExpressionEvaluationContext context, out String text);
    }
}

