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
//     * Neither the name of the [ORGANIZATION] nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
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

namespace XtraLiteTemplates.Parsing
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Text;

    public sealed class Tag
    {
        private const String MarkupExpressionWord = "$";

        private const String MarkupAnyIdentifierWord = "?";

        private const Char MarkupIdentifierGroupStartCharacter = '(';

        private const Char MarkupIdentifierGroupEndCharacter = ')';

        public static String AsIdentifier(String candidate)
        {
            if (candidate != null)
                candidate = candidate.Trim();

            var isValid =
                !String.IsNullOrEmpty(candidate) &&
                (Char.IsLetter(candidate[0]) || candidate[0] == '_') &&
                candidate.All(c => Char.IsLetterOrDigit(c) || c == '_');

            return isValid ? candidate : null;
        }


        private readonly IList<Object> m_components;

        private Boolean LastComponentIsExpression
        {
            get
            {
                return m_components.Count > 0 && m_components[m_components.Count - 1] == null;
            }
        }

        public Tag()
        {
            m_components = new List<Object>();
        }

        public Tag Keyword(String keyword)
        {
            Expect.Identifier("keyword", keyword);

            m_components.Add(keyword);
            return this;
        }

        public Tag Identifier()
        {
            if (m_components.Count > 0 && m_components[m_components.Count - 1] == null)
                ExceptionHelper.TagAnyIndentifierCannotFollowExpression();

            m_components.Add(String.Empty);
            return this;
        }

        public Tag Identifier(params String[] candidates)
        {
            Expect.NotEmpty("candidates", candidates);

            foreach (var arg in candidates)
                Expect.Identifier("candidate", arg);

            m_components.Add(candidates);
            return this;
        }

        public Tag Expression()
        {
            if (m_components.Count > 0 && m_components[m_components.Count - 1] == null)
                ExceptionHelper.TagExpressionCannotFollowExpression();

            m_components.Add(null);

            return this;
        }

        public override String ToString()
        {
            if (m_components.Count == 0)
                return String.Empty;
            else
            {
                StringBuilder sb = new StringBuilder();
                foreach (var component in m_components)
                {
                    if (sb.Length > 0)
                        sb.Append(" ");

                    if (component == null)
                        sb.Append(MarkupExpressionWord);
                    else if (component == (Object)String.Empty)
                        sb.Append(MarkupAnyIdentifierWord);
                    else if (component is String)
                        sb.Append(component);
                    else
                    {
                        sb.AppendFormat("{0}{1}{2}", MarkupIdentifierGroupStartCharacter,
                            String.Join(" ", (component as String[])), MarkupIdentifierGroupEndCharacter);
                    }
                }

                return sb.ToString();
            }
        }

        public static Boolean TryParse(String markup, out Tag result)
        {
            result = null;
            if (String.IsNullOrEmpty(markup))
                return false;

            /* Parse the markup */
            Tag beingBuilt = new Tag();
            Boolean identifierGroupBeingParsed = false;
            StringBuilder currentParsedWord = new StringBuilder();
            HashSet<String> currentIdentifierGroup = new HashSet<String>();
            for (var i = 0; i < markup.Length; i++)
            {
                if (Char.IsWhiteSpace(markup[i]) || markup[i] == MarkupIdentifierGroupStartCharacter || markup[i] == MarkupIdentifierGroupEndCharacter)
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
                                return false;
                            else
                                beingBuilt.Expression();
                        }
                        else if (word == MarkupAnyIdentifierWord)
                        {
                            /* This signifies an expression. */
                            if (beingBuilt.LastComponentIsExpression || identifierGroupBeingParsed)
                                return false;
                            else
                                beingBuilt.Identifier();
                        }
                        else
                        {
                            var identifier = AsIdentifier(word);

                            if (identifier == null)
                                return false;

                            if (identifierGroupBeingParsed)
                                currentIdentifierGroup.Add(identifier);
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
                        return false;
                    else
                        identifierGroupBeingParsed = true;
                }
                else if (markup[i] == MarkupIdentifierGroupEndCharacter)
                {
                    /* This signifies an identifier group being ended. */
                    if (!identifierGroupBeingParsed || currentIdentifierGroup.Count == 0)
                        return false;
                    else
                    {
                        beingBuilt.Identifier(currentIdentifierGroup.ToArray());
                        currentIdentifierGroup.Clear();

                        identifierGroupBeingParsed = false;
                    }
                }
                else if (!Char.IsWhiteSpace(markup[i]))
                {
                    /* Just put the character in the box-ah */
                    currentParsedWord.Append(markup[i]);
                }
            }

            /* Finalize and flush. */
            if (identifierGroupBeingParsed)
                return false;

            if (currentParsedWord.Length > 0)
            {
                var word = currentParsedWord.ToString();
                if (word == MarkupExpressionWord)
                {
                    /* This signifies an expression. */
                    if (beingBuilt.LastComponentIsExpression)
                        return false;
                    else
                        beingBuilt.Expression();
                }
                else if (word == MarkupAnyIdentifierWord)
                {
                    /* This signifies an expression. */
                    if (beingBuilt.LastComponentIsExpression)
                        return false;
                    else
                        beingBuilt.Identifier();
                }
                else
                {
                    var identifier = AsIdentifier(word);
                    if (identifier == null)
                        return false;

                    beingBuilt.Keyword(identifier);
                }
            }

            if (beingBuilt.m_components.Count > 0)
                result = beingBuilt;

            return result != null;
        }

        public static Tag Parse(String markup)
        {
            Tag result;
            if (!TryParse(markup, out result))
                ExceptionHelper.InvalidTagMarkup(markup);
                
            return result;
        }


        public Boolean Equals(Object obj, IEqualityComparer<String> comparer)
        {
            Expect.NotNull("comparer", comparer);

            var tag = obj as Tag;
            if (tag == null || tag.m_components.Count != m_components.Count)
                return false;
            else
            {
                for (var i = 0; i < m_components.Count; i++)
                {
                    /* Compare as expressions. */
                    if (m_components[i] == null && tag.m_components[i] != null)
                        return false;
                    else if (m_components[i] == null && tag.m_components[i] != null)
                        return false;

                    /* Compare as keywords or "any identifiers" */
                    var otherKeyword = tag.m_components[i] as String;
                    var mineKeyword = m_components[i] as String;

                    if (otherKeyword != null && mineKeyword == null)
                        return false;
                    else if (otherKeyword == null && mineKeyword != null)
                        return false;
                    else if (otherKeyword != null && mineKeyword != null)
                    {
                        if (!comparer.Equals(otherKeyword, mineKeyword))
                            return false;
                    }

                    /* Compare as identifier options. */
                    var otherIdents = tag.m_components[i] as String[];
                    var mineIdents = m_components[i] as String[];

                    if (otherIdents != null && mineIdents == null)
                        return false;
                    else if (otherIdents == null && mineIdents != null)
                        return false;
                    else if (otherIdents != null && mineIdents != null)
                    {
                        Debug.Assert(otherIdents.Length > 0);
                        Debug.Assert(mineIdents.Length > 0);

                        var areEqualSets = new HashSet<String>(otherIdents, comparer).SetEquals(mineIdents);
                        if (!areEqualSets)
                            return false;
                    }
                }
            }

            return true;
        }

        public override Boolean Equals(Object obj)
        {
            return Equals(obj, StringComparer.CurrentCulture);
        }

        public Int32 GetHashCode(IEqualityComparer<String> comparer)
        {
            Expect.NotNull("comparer", comparer);

            var hash = 17; /* Just a magic constant */
            unchecked
            {
                foreach (var component in m_components)
                {
                    hash = hash * 23;

                    if (component != null)
                    {
                        var stringComponent = component as String;
                        if (stringComponent != null)
                            hash += comparer.GetHashCode(stringComponent);
                        else
                        {
                            var identArray = component as String[];
                            Debug.Assert(identArray != null);

                            var identsHash = 0;
                            foreach (var ident in identArray)
                                identsHash = identsHash ^ comparer.GetHashCode(ident);

                            hash += identsHash;
                        }
                    }
                }
            }

            return hash;
        }

        public override Int32 GetHashCode()
        {
            return GetHashCode(StringComparer.CurrentCulture);
        }

        public Int32 ComponentCount
        {
            get
            {
                return m_components.Count;
            }
        }

        internal Boolean MatchesKeyword(Int32 index, IEqualityComparer<String> comparer, String keyword)
        {
            Debug.Assert(comparer != null);
            Debug.Assert(!String.IsNullOrEmpty(keyword));

            if (index >= m_components.Count || m_components[index] == null)
                return false;

            String stringComponent = m_components[index] as String;
            return comparer.Equals(stringComponent, keyword);
        }

        internal Boolean MatchesIdentifier(Int32 index, IEqualityComparer<String> comparer, String identifier)
        {
            Debug.Assert(comparer != null);
            Debug.Assert(!String.IsNullOrEmpty(identifier));

            if (index >= m_components.Count || m_components[index] == null)
                return false;

            var stringComponent = m_components[index] as String;
            if (stringComponent != null)
                return stringComponent == String.Empty;

            String[] _identifiers = m_components[index] as String[];
            return _identifiers.Any(i => comparer.Equals(i, identifier));
        }

        internal Boolean MatchesExpression(Int32 index)
        {
            return index < m_components.Count && m_components[index] == null;
        }
    }
}

