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

namespace XtraLiteTemplates.Parsing
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    using JetBrains.Annotations;

    /// <summary>
    /// The basic building block of <see cref="Evaluation.Directive"/> classes. The tag defines a set of keywords, identifier and expression
    /// match rules that need to be satisfied during the lexical analysis.
    /// </summary>
    [PublicAPI]
    public sealed class Tag
    {
        [NotNull]
        private const string MarkupExpressionWord = "$";
        [NotNull]
        private const string MarkupAnyIdentifierWord = "?";
        private const char MarkupIdentifierGroupStartCharacter = '(';
        private const char MarkupIdentifierGroupEndCharacter = ')';
        [NotNull]
        private readonly IList<object> _components;

        /// <summary>
        /// Initializes a new instance of the <see cref="Tag"/> class.
        /// </summary>
        public Tag()
        {
            _components = new List<object>();
        }

        /// <summary>
        /// <value>Gets the number of components this tag has defined.</value>
        /// </summary>
        public int ComponentCount => _components.Count;

        private bool LastComponentIsExpression => _components.Count > 0 && _components[_components.Count - 1] == null;

        /// <summary>
        /// Tries to parse a given tag markup string. This is an easy way to defined tags without needing to resort to calling individual component methods.
        /// </summary>
        /// <param name="markup">The markup string.</param>
        /// <param name="result">The built tag object, if the parsing succeeded.</param>
        /// <returns><c>true</c> if the parsing was successful; or <c>false</c> otherwise.</returns>
        [ContractAnnotation("=> false, result : null; => true, result : notnull")]
        public static bool TryParse([CanBeNull] string markup, out Tag result)
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

            foreach (var t in markup)
            {
                if (char.IsWhiteSpace(t) || t == MarkupIdentifierGroupStartCharacter || t == MarkupIdentifierGroupEndCharacter)
                {
                    /* A term has ended (maybe) */
                    if (currentParsedWord.Length > 0)
                    {
                        var word = currentParsedWord.ToString();
                        currentParsedWord.Clear();

                        switch (word)
                        {
                            case MarkupExpressionWord:
                                /* This signifies an expression. */
                                if (beingBuilt.LastComponentIsExpression || identifierGroupBeingParsed)
                                {
                                    return false;
                                }

                                beingBuilt.Expression();
                                break;
                            case MarkupAnyIdentifierWord:
                                /* This signifies an expression. */
                                if (beingBuilt.LastComponentIsExpression || identifierGroupBeingParsed)
                                {
                                    return false;
                                }

                                beingBuilt.Identifier();
                                break;
                            default:
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
                                break;
                        }
                    }
                }

                switch (t)
                {
                    case MarkupIdentifierGroupStartCharacter:
                        /* This signifies an identifier group being specified. */
                        if (identifierGroupBeingParsed)
                        {
                            return false;
                        }

                        identifierGroupBeingParsed = true;
                        break;
                    case MarkupIdentifierGroupEndCharacter:
                        /* This signifies an identifier group being ended. */
                        if (!identifierGroupBeingParsed || currentIdentifierGroup.Count == 0)
                        {
                            return false;
                        }

                        beingBuilt.Identifier(currentIdentifierGroup.ToArray());
                        currentIdentifierGroup.Clear();

                        identifierGroupBeingParsed = false;
                        break;
                    default:
                        if (!char.IsWhiteSpace(t))
                        {
                            /* Just put the character in the box-ah */
                            currentParsedWord.Append(t);
                        }
                        break;
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
                switch (word)
                {
                    case MarkupExpressionWord:
                        /* This signifies an expression. */
                        if (beingBuilt.LastComponentIsExpression)
                        {
                            return false;
                        }

                        beingBuilt.Expression();
                        break;
                    case MarkupAnyIdentifierWord:
                        /* This signifies an expression. */
                        if (beingBuilt.LastComponentIsExpression)
                        {
                            return false;
                        }

                        beingBuilt.Identifier();
                        break;
                    default:
                        var identifier = AsIdentifier(word);
                        if (identifier == null)
                        {
                            return false;
                        }

                        beingBuilt.Keyword(identifier);
                        break;
                }
            }

            if (beingBuilt._components.Count > 0)
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
        [NotNull]
        public static Tag Parse([NotNull] string markup)
        {
            if (!TryParse(markup, out Tag result))
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
        [NotNull]
        public Tag Keyword([NotNull] string keyword)
        {
            Expect.Identifier(nameof(keyword), keyword);

            _components.Add(keyword);
            return this;
        }

        /// <summary>
        /// Appends any identifier requirement component to the tag's matching pattern. Any valid identifier will match this component.
        /// </summary>
        /// <returns>This tag instance.</returns>
        /// <exception cref="System.InvalidOperationException">If the previous requirement component was an expression.</exception>
        [NotNull]
        public Tag Identifier()
        {
            if (_components.Count > 0 && _components[_components.Count - 1] == null)
            {
                ExceptionHelper.TagAnyIdentifierCannotFollowExpression();
            }

            _components.Add(string.Empty);
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
        [NotNull]
        public Tag Identifier([NotNull] [ItemNotNull] params string[] candidates)
        {
            Expect.NotEmpty(nameof(candidates), candidates);

            foreach (var arg in candidates)
            {
                Expect.Identifier(nameof(candidates), arg);
            }

            _components.Add(candidates);
            return this;
        }

        /// <summary>
        /// Appends an expression requirement component to the tag's matching pattern.
        /// </summary>
        /// <returns>
        /// This tag instance.
        /// </returns>
        /// <exception cref="System.InvalidOperationException">If the previous requirement component was another expression.</exception>
        [NotNull]
        public Tag Expression()
        {
            if (_components.Count > 0 && _components[_components.Count - 1] == null)
            {
                ExceptionHelper.TagExpressionCannotFollowExpression();
            }

            _components.Add(null);

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
            if (_components.Count == 0)
            {
                return string.Empty;
            }

            var sb = new StringBuilder();
            foreach (var component in _components)
            {
                if (sb.Length > 0)
                {
                    sb.Append(" ");
                }

                if (component == null)
                {
                    sb.Append(MarkupExpressionWord);
                }
                else if (component.Equals(string.Empty))
                {
                    sb.Append(MarkupAnyIdentifierWord);
                }
                else if (component is string)
                {
                    sb.Append(component);
                }
                else
                {
                    Debug.Assert(component is string[], "invalid type of component");

                    sb.AppendFormat(
                        "{0}{1}{2}",
                        MarkupIdentifierGroupStartCharacter,
                        string.Join(" ", component as string[]),
                        MarkupIdentifierGroupEndCharacter);
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Checks whether this tag object is semantically equal with another tag object using a given <paramref name="comparer"/> for keyword and identifier
        /// comparisons.
        /// </summary>
        /// <param name="obj">The object to compare with.</param>
        /// <param name="comparer">An instance of <see cref="System.Collections.Generic.IEqualityComparer{String}"/> used for comparison.</param>
        /// <returns><c>true</c> if the tags are equal; <c>false</c> otherwise.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="comparer"/> is <c>null</c>.</exception>
        public bool Equals([CanBeNull] object obj, [NotNull] IEqualityComparer<string> comparer)
        {
            Expect.NotNull(nameof(comparer), comparer);

            var tag = obj as Tag;
            if (tag == null || tag._components.Count != _components.Count)
            {
                return false;
            }

            for (var i = 0; i < _components.Count; i++)
            {
                /* Compare as expressions. */
                if (_components[i] == null && tag._components[i] != null)
                {
                    return false;
                }

                if (_components[i] == null && tag._components[i] != null)
                {
                    return false;
                }

                /* Compare as keywords or "any identifiers" */
                var otherKeyword = tag._components[i] as string;
                var mineKeyword = _components[i] as string;

                if (otherKeyword != null && mineKeyword == null)
                {
                    return false;
                }
                if (otherKeyword == null && mineKeyword != null)
                {
                    return false;
                }
                if (otherKeyword != null)
                {
                    if (!comparer.Equals(otherKeyword, mineKeyword))
                    {
                        return false;
                    }
                }

                /* Compare as identifier options. */
                var otherIdentifiers = tag._components[i] as string[];
                var mineIdentifiers = _components[i] as string[];

                if (otherIdentifiers != null && mineIdentifiers == null)
                {
                    return false;
                }

                if (otherIdentifiers == null && mineIdentifiers != null)
                {
                    return false;
                }

                if (otherIdentifiers != null)
                {
                    Debug.Assert(otherIdentifiers.Length > 0, "other is expected to have more one or more identifiers.");
                    Debug.Assert(mineIdentifiers.Length > 0, "this is expected to have more one or more identifiers.");

                    var areEqualSets = new HashSet<string>(otherIdentifiers, comparer).SetEquals(mineIdentifiers);
                    if (!areEqualSets)
                    {
                        return false;
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
            return Equals(obj, StringComparer.CurrentCulture);
        }

        /// <summary>
        /// Calculates the hash code for this tag object using a given <paramref name="comparer"/> object.
        /// </summary>
        /// <param name="comparer">An instance of <see cref="System.Collections.Generic.IEqualityComparer{String}"/> used for hash code generation.</param>
        /// <returns>A <see cref="System.Int32"/> value representing the hash code.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="comparer"/> is <c>null</c>.</exception>
        public int GetHashCode([NotNull] IEqualityComparer<string> comparer)
        {
            Expect.NotNull(nameof(comparer), comparer);

            var hash = 17; /* Just a magic constant */
            unchecked
            {
                foreach (var component in _components)
                {
                    hash = hash * 23;

                    if (component != null)
                    {
                        if (component is string stringComponent)
                        {
                            hash += comparer.GetHashCode(stringComponent);
                        }
                        else
                        {
                            var identifierArray = component as string[];
                            Debug.Assert(identifierArray != null, "identifier set component cannot be null.");

                            var identifierHash = identifierArray.Aggregate(0, (current, identifier) => current ^ comparer.GetHashCode(identifier));

                            hash += identifierHash;
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
            return GetHashCode(StringComparer.CurrentCulture);
        }

        internal bool MatchesKeyword(int index, [NotNull] IEqualityComparer<string> comparer, [NotNull] string keyword)
        {
            Debug.Assert(comparer != null, "comparer cannot be null.");
            Debug.Assert(!string.IsNullOrEmpty(keyword), "keyword cannot be empty.");

            if (index < 0 || index >= _components.Count || _components[index] == null)
            {
                return false;
            }

            var stringComponent = _components[index] as string;
            return comparer.Equals(stringComponent, keyword);
        }

        internal bool MatchesIdentifier(
            int index, 
            [NotNull] IEqualityComparer<string> comparer, 
            [NotNull] string identifier)
        {
            Debug.Assert(comparer != null, "comparer cannot be null.");
            Debug.Assert(!string.IsNullOrEmpty(identifier), "identifier cannot be empty.");

            if (index < 0 || index >= _components.Count || _components[index] == null)
            {
                return false;
            }

            if (_components[index] is string stringComponent)
            {
                return stringComponent == string.Empty;
            }

            var anyIdentifierComponent = _components[index] as string[];
            Debug.Assert(anyIdentifierComponent != null, "invalid component type.");
            return anyIdentifierComponent.Any(i => comparer.Equals(i, identifier));
        }

        internal bool MatchesAnyIdentifier(int index)
        {
            if (index < 0 || index >= _components.Count || _components[index] == null)
            {
                return false;
            }

            var stringComponent = _components[index] as string;
            return stringComponent == string.Empty;
        }

        internal bool MatchesExpression(int index)
        {
            return index >= 0 && index < _components.Count && _components[index] == null;
        }

        private static string AsIdentifier([CanBeNull] string candidate)
        {
            candidate = candidate?.Trim();

            var validIdentifier =
                !string.IsNullOrEmpty(candidate) &&
                (char.IsLetter(candidate[0]) || candidate[0] == '_') &&
                candidate.All(c => char.IsLetterOrDigit(c) || c == '_');

            return validIdentifier ? candidate : null;
        }
    }
}
