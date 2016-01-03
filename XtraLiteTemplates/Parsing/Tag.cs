//  Author:
//    Alexandru Ciobanu alex@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex@ciobanu.org)
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

namespace XtraLiteTemplates.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// The basic building block of <see cref="XtraLiteTemplates.Evaluation.Directive"/> classes. The tag defines a set of keywords, identifier and expression
    /// match rules that need to be satisfied during the lexical analysis.
    /// </summary>
    public sealed class Tag
    {
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private const string MarkupExpressionWord = "$";

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private const string MarkupAnyIdentifierWord = "?";

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private const char MarkupIdentifierGroupStartCharacter = '(';

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private const char MarkupIdentifierGroupEndCharacter = ')';

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private readonly IList<object> components;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        public Tag()
        {
            this.components = new List<object>();
        }

        /// <summary>
        /// <value>Gets the number of components this tag has defined.</value>
        /// </summary>
        public int ComponentCount
        {
            get
            {
                return this.components.Count;
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private bool LastComponentIsExpression
        {
            get
            {
                return this.components.Count > 0 && this.components[this.components.Count - 1] == null;
            }
        }

        /// <summary>
        /// Tries to parse a given tag markup string. This is an easy way to defined tags without needing to resort to calling individual component methods.
        /// </summary>
        /// <param name="markup">The markup string.</param>
        /// <param name="result">The built tag object, if the parsing succeeded.</param>
        /// <returns><c>true</c> if the parsing was successful; or <c>false</c> otherwise.</returns>
        public static bool TryParse(string markup, out Tag result)
        {
            result = null;
            if (string.IsNullOrEmpty(markup))
            {
                return false;
            }

            /* Parse the markup */
            var beingBuilt = new Tag();
            var identifierGroupBeingParsed = false;
            var currentParsedWord = new StringBuilder();
            var currentIdentifierGroup = new HashSet<string>();
            for (var i = 0; i < markup.Length; i++)
            {
                if (char.IsWhiteSpace(markup[i]) || markup[i] == MarkupIdentifierGroupStartCharacter || markup[i] == MarkupIdentifierGroupEndCharacter)
                {
                    /* A term has ended (maybe) */
                    if (currentParsedWord.Length > 0)
                    {
                        var word = currentParsedWord.ToString();
                        currentParsedWord.Clear();

                        if (word == MarkupExpressionWord)
                        {
                            /* This signifies an expression. */
                            if (beingBuilt.LastComponentIsExpression || identifierGroupBeingParsed)
                            {
                                return false;
                            }
                            else
                            {
                                beingBuilt.Expression();
                            }
                        }
                        else if (word == MarkupAnyIdentifierWord)
                        {
                            /* This signifies an expression. */
                            if (beingBuilt.LastComponentIsExpression || identifierGroupBeingParsed)
                            {
                                return false;
                            }
                            else
                            {
                                beingBuilt.Identifier();
                            }
                        }
                        else
                        {
                            var identifier = AsIdentifier(word);

                            if (identifier == null)
                            {
                                return false;
                            }

                            if (identifierGroupBeingParsed)
                            {
                                currentIdentifierGroup.Add(identifier);
                            }
                            else
                            {
                                /* Standalone keyword */
                                beingBuilt.Keyword(identifier);
                            }
                        }
                    }
                }

                if (markup[i] == MarkupIdentifierGroupStartCharacter)
                {
                    /* This signifies an identifier group being specified. */
                    if (identifierGroupBeingParsed)
                    {
                        return false;
                    }
                    else
                    {
                        identifierGroupBeingParsed = true;
                    }
                }
                else if (markup[i] == MarkupIdentifierGroupEndCharacter)
                {
                    /* This signifies an identifier group being ended. */
                    if (!identifierGroupBeingParsed || currentIdentifierGroup.Count == 0)
                    {
                        return false;
                    }
                    else
                    {
                        beingBuilt.Identifier(currentIdentifierGroup.ToArray());
                        currentIdentifierGroup.Clear();

                        identifierGroupBeingParsed = false;
                    }
                }
                else if (!char.IsWhiteSpace(markup[i]))
                {
                    /* Just put the character in the box-ah */
                    currentParsedWord.Append(markup[i]);
                }
            }

            /* Finalize and flush. */
            if (identifierGroupBeingParsed)
            {
                return false;
            }

            if (currentParsedWord.Length > 0)
            {
                var word = currentParsedWord.ToString();
                if (word == MarkupExpressionWord)
                {
                    /* This signifies an expression. */
                    if (beingBuilt.LastComponentIsExpression)
                    {
                        return false;
                    }
                    else
                    {
                        beingBuilt.Expression();
                    }
                }
                else if (word == MarkupAnyIdentifierWord)
                {
                    /* This signifies an expression. */
                    if (beingBuilt.LastComponentIsExpression)
                    {
                        return false;
                    }
                    else
                    {
                        beingBuilt.Identifier();
                    }
                }
                else
                {
                    var identifier = AsIdentifier(word);
                    if (identifier == null)
                    {
                        return false;
                    }

                    beingBuilt.Keyword(identifier);
                }
            }

            if (beingBuilt.components.Count > 0)
            {
                result = beingBuilt;
            }

            return result != null;
        }

        /// <summary>
        /// Parses a given tag markup string. This method uses <see cref="TryParse"/>, but will throw an exception if the parsing fails.
        /// </summary>
        /// <param name="markup">The markup string.</param>
        /// <returns>The built tag object.</returns>
        /// <exception cref="System.FormatException">If <paramref name="markup"/> cannot be parsed.</exception>
        public static Tag Parse(string markup)
        {
            Tag result;
            if (!TryParse(markup, out result))
            {
                ExceptionHelper.InvalidTagMarkup(markup);
            }

            return result;
        }

        /// <summary>
        /// Appends a keyword requirement component to the tag's matching pattern.
        /// </summary>
        /// <param name="keyword">The keyword to match.</param>
        /// <returns>This tag instance.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="keyword"/> is <c>null</c>.</exception>
        /// <exception cref="System.ArgumentException"><paramref name="keyword"/> is not a valid identifier.</exception>
        public Tag Keyword(string keyword)
        {
            Expect.Identifier("keyword", keyword);

            this.components.Add(keyword);
            return this;
        }

        /// <summary>
        /// Appends any identifier requirement component to the tag's matching pattern. Any valid identifier will match this component.
        /// </summary>
        /// <returns>This tag instance.</returns>
        /// <exception cref="System.InvalidOperationException">If the previous requirement component was an expression.</exception>
        public Tag Identifier()
        {
            if (this.components.Count > 0 && this.components[this.components.Count - 1] == null)
            {
                ExceptionHelper.TagAnyIndentifierCannotFollowExpression();
            }

            this.components.Add(string.Empty);
            return this;
        }

        /// <summary>
        /// Appends identifier set requirement component to the tag's matching pattern. Any identifier contained in the provided set of options will match this component.
        /// </summary>
        /// <param name="candidates">The candidate identifier set.</param>
        /// <returns>
        /// This tag instance.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="candidates" /> or any of its elements are <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="candidates" /> is <c>empty</c> or any of its elements is not a valid identifier.</exception>
        public Tag Identifier(params string[] candidates)
        {
            Expect.NotEmpty("candidates", candidates);

            foreach (var arg in candidates)
            {
                Expect.Identifier("candidate", arg);
            }

            this.components.Add(candidates);
            return this;
        }

        /// <summary>
        /// Appends an expression requirement component to the tag's matching pattern.
        /// </summary>
        /// <returns>
        /// This tag instance.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">If the previous requirement component was another expression.</exception>
        public Tag Expression()
        {
            if (this.components.Count > 0 && this.components[this.components.Count - 1] == null)
            {
                ExceptionHelper.TagExpressionCannotFollowExpression();
            }

            this.components.Add(null);

            return this;
        }

        /// <summary>
        /// A human-readable representation of the tag's structure. The representation uses the same markup language used to create the tag.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            if (this.components.Count == 0)
            {
                return string.Empty;
            }
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var component in this.components)
                {
                    if (sb.Length > 0)
                    {
                        sb.Append(" ");
                    }

                    if (component == null)
                    {
                        sb.Append(MarkupExpressionWord);
                    }
                    else if (component == (object)string.Empty)
                    {
                        sb.Append(MarkupAnyIdentifierWord);
                    }
                    else if (component is string)
                    {
                        sb.Append(component);
                    }
                    else
                    {
                        sb.AppendFormat(
                            "{0}{1}{2}",
                            MarkupIdentifierGroupStartCharacter,
                            string.Join(" ", component as string[]),
                            MarkupIdentifierGroupEndCharacter);
                    }
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Checks whether this tag object is semantically equal with another tag object using a given <paramref name="comparer"/> for keyword and identifier
        /// comparisons.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <param name="comparer">An instance of <see cref="System.Collections.Generic.IEqualityComparer{String}"/> used for comparison.</param>
        /// <returns><c>true</c> if the tags are equal; <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="comparer"/> is <c>null</c>.</exception>
        public bool Equals(object obj, IEqualityComparer<string> comparer)
        {
            Expect.NotNull("comparer", comparer);

            var tag = obj as Tag;
            if (tag == null || tag.components.Count != this.components.Count)
            {
                return false;
            }
            else
            {
                for (var i = 0; i < this.components.Count; i++)
                {
                    /* Compare as expressions. */
                    if (this.components[i] == null && tag.components[i] != null)
                    {
                        return false;
                    }
                    else if (this.components[i] == null && tag.components[i] != null)
                    {
                        return false;
                    }

                    /* Compare as keywords or "any identifiers" */
                    var otherKeyword = tag.components[i] as string;
                    var mineKeyword = this.components[i] as string;

                    if (otherKeyword != null && mineKeyword == null)
                    {
                        return false;
                    }
                    else if (otherKeyword == null && mineKeyword != null)
                    {
                        return false;
                    }
                    else if (otherKeyword != null && mineKeyword != null)
                    {
                        if (!comparer.Equals(otherKeyword, mineKeyword))
                        {
                            return false;
                        }
                    }

                    /* Compare as identifier options. */
                    var otherIdents = tag.components[i] as string[];
                    var mineIdents = this.components[i] as string[];

                    if (otherIdents != null && mineIdents == null)
                    {
                        return false;
                    }
                    else if (otherIdents == null && mineIdents != null)
                    {
                        return false;
                    }
                    else if (otherIdents != null && mineIdents != null)
                    {
                        Debug.Assert(otherIdents.Length > 0, "other is expected to heve more one or more identifiers.");
                        Debug.Assert(mineIdents.Length > 0, "this is expected to heve more one or more identifiers.");

                        var areEqualSets = new HashSet<string>(otherIdents, comparer).SetEquals(mineIdents);
                        if (!areEqualSets)
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Checks whether this tag object is semantically equal with another tag object using the current culture comparison rules.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <returns><c>true</c> if the tags are equal; <c>false</c> otherwise.</returns>
        public override bool Equals(object obj)
        {
            return this.Equals(obj, StringComparer.CurrentCulture);
        }

        /// <summary>
        /// Calculates the hash code for this tag object using a given <paramref name="comparer"/> object.
        /// </summary>
        /// <param name="comparer">An instance of <see cref="System.Collections.Generic.IEqualityComparer{String}"/> used for hash code generation.</param>
        /// <returns>A <see cref="System.Int32"/> value representing the hash code.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="comparer"/> is <c>null</c>.</exception>
        public int GetHashCode(IEqualityComparer<string> comparer)
        {
            Expect.NotNull("comparer", comparer);

            var hash = 17; /* Just a magic constant */
            unchecked
            {
                foreach (var component in this.components)
                {
                    hash = hash * 23;

                    if (component != null)
                    {
                        var stringComponent = component as string;
                        if (stringComponent != null)
                        {
                            hash += comparer.GetHashCode(stringComponent);
                        }
                        else
                        {
                            var identArray = component as string[];
                            Debug.Assert(identArray != null, "identifier set component cannot be null.");

                            var identsHash = 0;
                            foreach (var ident in identArray)
                            {
                                identsHash = identsHash ^ comparer.GetHashCode(ident);
                            }

                            hash += identsHash;
                        }
                    }
                }
            }

            return hash;
        }

        /// <summary>
        /// Calculates the hash code for this tag object using the current culture string comparison rules.
        /// </summary>
        /// <returns>A <see cref="System.Int32"/> value representing the hash code.</returns>
        public override int GetHashCode()
        {
            return this.GetHashCode(StringComparer.CurrentCulture);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        internal bool MatchesKeyword(int index, IEqualityComparer<string> comparer, string keyword)
        {
            Debug.Assert(comparer != null, "comparer cannot be null.");
            Debug.Assert(!string.IsNullOrEmpty(keyword), "keyword cannot be empty.");

            if (index < 0 || index >= this.components.Count || this.components[index] == null)
            {
                return false;
            }

            var stringComponent = this.components[index] as string;
            return comparer.Equals(stringComponent, keyword);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        internal bool MatchesIdentifier(int index, IEqualityComparer<string> comparer, string identifier)
        {
            Debug.Assert(comparer != null, "comparer cannot be null.");
            Debug.Assert(!string.IsNullOrEmpty(identifier), "identifier cannot be empty.");

            if (index < 0 || index >= this.components.Count || this.components[index] == null)
            {
                return false;
            }

            var stringComponent = this.components[index] as string;
            if (stringComponent != null)
            {
                return stringComponent == string.Empty;
            }

            var anyIdentifierComponent = this.components[index] as string[];
            return anyIdentifierComponent.Any(i => comparer.Equals(i, identifier));
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        internal bool MatchesAnyIdentifier(int index)
        {
            if (index < 0 || index >= this.components.Count || this.components[index] == null)
            {
                return false;
            }

            var stringComponent = this.components[index] as string;
            return stringComponent == string.Empty;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        internal bool MatchesExpression(int index)
        {
            return index >= 0 && index < this.components.Count && this.components[index] == null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private static string AsIdentifier(string candidate)
        {
            if (candidate != null)
            {
                candidate = candidate.Trim();
            }

            var validIdentifier =
                !string.IsNullOrEmpty(candidate) &&
                (char.IsLetter(candidate[0]) || candidate[0] == '_') &&
                candidate.All(c => char.IsLetterOrDigit(c) || c == '_');

            return validIdentifier ? candidate : null;
        }
    }
}
