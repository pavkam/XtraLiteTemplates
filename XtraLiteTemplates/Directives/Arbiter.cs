using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Parsing;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Directives
{
    internal class Arbiter : ISet<Directive>
    {
        private ISet<Directive> m_directives;

        public Boolean Strict { get; private set; }

        public Arbiter(Boolean strict)
        {
            m_directives = new HashSet<Directive>();
            Strict = strict;
        }

        private Int32 ScoreTokenInDirective(Directive directive, Int32 index, 
            DirectiveToken token, DirectiveFacing facing)
        {
            Int32 score = 0;
            if (token.Type == DirectiveToken.TokenType.Identifier)
            {
                if (directive.MatchesKeywordComponent(facing, index, token.Value.ToUpper()))
                    score = 3;
                else if (directive.MatchesIdentifierComponent(facing, index, token.Value.ToUpper()))
                    score = 2;
                else if (directive.MatchesVariableComponent(facing, index))
                    score = 1;
            }
            else if (token.Type == DirectiveToken.TokenType.Symbolic)
            {
                if (directive.MatchesKeywordComponent(facing, index, token.Value))
                    score = 2;
                else if (directive.MatchesIdentifierComponent(facing, index, token.Value))
                    score = 1;
            }
            else if (token.Type == DirectiveToken.TokenType.Numerical)
            {
                if (directive.MatchesConstantComponent(facing, index, token.Value))
                    score =  2;
                else if (directive.MatchesVariableComponent(facing, index))
                    score = 1;
            }
            else if (token.Type == DirectiveToken.TokenType.String)
            {
                if (directive.MatchesConstantComponent(facing, index, token.Value))
                    score =  2;
                else if (directive.MatchesVariableComponent(facing, index))
                    score = 1;
            }

            return score;
        }

        private Int32 ScoreDirective(Directive directive, IReadOnlyList<DirectiveToken> tokens, DirectiveFacing facing)
        {
            Int32 totalScore = 0;

            for (Int32 tokenIndex = 0; tokenIndex < tokens.Count; tokenIndex++)
            {
                var tokenScore = ScoreTokenInDirective(directive, tokenIndex, tokens[tokenIndex], facing) * (tokens.Count - tokenIndex);
                if (tokenScore == 0)
                    return 0;
                else
                    totalScore += tokenScore;
            }

            return totalScore;
        }

        private IReadOnlyList<Directive> GetMatchList(IReadOnlyList<DirectiveToken> tokens, DirectiveFacing facing)
        {
            /* Walk the tokens! */
            var greatestScored = new List<Directive>();
            Int32 greatestScore = 0;
            foreach (var m in this)
            {
                /* Build the match score */
                Int32 score = ScoreDirective(m, tokens, facing);

                if (score > 0)
                {
                    if (score >= greatestScore)
                    {
                        if (score > greatestScore)
                        {
                            greatestScored.Clear();
                            greatestScore = score;
                        }

                        greatestScored.Add(m);
                    }
                }
            }

            return greatestScored;
        }

        internal DirectiveMatchResult TryMatch(IReadOnlyList<DirectiveToken> tokens, out Directive directive)
        {
            ValidationHelper.AssertObjectCollectionIsNotEmpty("tokens", tokens);

            /* Find all the matching defintions by direction */
            var startList = GetMatchList(tokens, DirectiveFacing.Face);
            var endList = GetMatchList(tokens, DirectiveFacing.Tail);

            /* We now have a list of matching directive at the same score. */
            if (startList.Count == 0 && endList.Count == 0)
            {
                directive = null;
                return DirectiveMatchResult.NoMatches;
            }
            else if (startList.Count == 1)
            {
                directive = startList.Single();
                return DirectiveMatchResult.MatchingFaceDirective;
            }
            else if (endList.Count == 1)
            {
                directive = endList.Single();
                return DirectiveMatchResult.MatchingTailDirective;
            }
            else
            {
                directive = null;
                return DirectiveMatchResult.AmbiguousMatches;
            }
        }

        #region ISet<Directive>

        public Boolean Add(Directive item)
        {
            ValidationHelper.AssertArgumentIsNotNull("item", item);

            return m_directives.Add(item);
        }

        public void ExceptWith(IEnumerable<Directive> other)
        {
            ValidationHelper.AssertArgumentIsNotNull("other", other);
            m_directives.ExceptWith(other);
        }

        public void IntersectWith(IEnumerable<Directive> other)
        {
            ValidationHelper.AssertArgumentIsNotNull("other", other);
            m_directives.IntersectWith(other);
        }

        public Boolean IsProperSubsetOf(IEnumerable<Directive> other)
        {
            ValidationHelper.AssertArgumentIsNotNull("other", other);
            return m_directives.IsProperSubsetOf(other);
        }

        public Boolean IsProperSupersetOf(IEnumerable<Directive> other)
        {
            ValidationHelper.AssertArgumentIsNotNull("other", other);
            return m_directives.IsProperSupersetOf(other);
        }

        public Boolean IsSubsetOf(IEnumerable<Directive> other)
        {
            ValidationHelper.AssertArgumentIsNotNull("other", other);
            return m_directives.IsSubsetOf(other);
        }

        public Boolean IsSupersetOf(IEnumerable<Directive> other)
        {
            ValidationHelper.AssertArgumentIsNotNull("other", other);
            return m_directives.IsSupersetOf(other);
        }

        public Boolean Overlaps(IEnumerable<Directive> other)
        {
            ValidationHelper.AssertArgumentIsNotNull("other", other);
            return m_directives.Overlaps(other);
        }

        public Boolean SetEquals(IEnumerable<Directive> other)
        {
            ValidationHelper.AssertArgumentIsNotNull("other", other);
            return m_directives.SetEquals(other);
        }

        public void SymmetricExceptWith(IEnumerable<Directive> other)
        {
            ValidationHelper.AssertArgumentIsNotNull("other", other);
            m_directives.SymmetricExceptWith(other);
        }

        public void UnionWith(IEnumerable<Directive> other)
        {
            ValidationHelper.AssertArgumentIsNotNull("other", other);
            m_directives.UnionWith(other);
        }

        void ICollection<Directive>.Add(Directive item)
        {
            Add(item);
        }

        public void Clear()
        {
            m_directives.Clear();
        }

        public Boolean Contains(Directive item)
        {
            ValidationHelper.AssertArgumentIsNotNull("item", item);
            return m_directives.Contains(item);
        }

        public void CopyTo(Directive[] array, Int32 arrayIndex)
        {
            ValidationHelper.AssertArgumentIsNotNull("array", array);
            m_directives.CopyTo(array, arrayIndex);
        }

        public Int32 Count
        {
            get
            {
                return m_directives.Count;
            }
        }

        public Boolean IsReadOnly
        {
            get
            {
                return false;
            }
        }

        public Boolean Remove(Directive item)
        {
            ValidationHelper.AssertArgumentIsNotNull("item", item);
            return m_directives.Remove(item);
        }

        public IEnumerator<Directive> GetEnumerator()
        {
            return m_directives.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return m_directives.GetEnumerator();
        } 

        #endregion
    }
}
