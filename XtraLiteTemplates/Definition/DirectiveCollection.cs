using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Parsing;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Definition
{
    public sealed class DirectiveCollection : 
        IDirectiveSelectionStrategy, IDirectiveDefinitionComponentScoringStrategy
    {
        private IList<Directive> m_directives = new List<Directive>();

        public DirectiveCollection(IDirectiveDefinitionComponentScoringStrategy scoreProvider)
        {
            ValidationHelper.AssertArgumentIsNotNull("scoreProvider", scoreProvider);

            ScoreProvider = scoreProvider;
        }

        public DirectiveCollection()
        {
            ScoreProvider = this;
        }

        public IDirectiveDefinitionComponentScoringStrategy ScoreProvider { get; private set; }

        public ICollection<Directive> Directives
        {
            get
            {
                return m_directives;
            }
        }


        public Int32 ScoreDirectiveDefinitionComponent(Directive directive, DirectiveDefinitionComponent component, DirectiveLiteral literal)
        {
            ValidationHelper.AssertArgumentIsNotNull("directive", directive);
            ValidationHelper.AssertArgumentIsNotNull("component", component);
            ValidationHelper.AssertArgumentIsNotNull("literal", literal);

            Int32 score = 0;
            if (literal.Type == DirectiveLiteralType.NormalIdentifier)
            {
                if (component.Type == DirectiveDefinitionComponentType.Keyword && 
                    directive.Comparer.Equals(component.Key, literal.Literal))
                {
                    score = 3;
                }
                else if (component.Type == DirectiveDefinitionComponentType.Constant &&
                    directive.AcceptsIdentifier(component, literal.Literal))
                {
                    score = 2;
                }
                else if (component.Type == DirectiveDefinitionComponentType.Variable)
                {
                    score = 1;
                }
            }
            else if (literal.Type == DirectiveLiteralType.SymbologicalIdentifier)
            {
                if (component.Type == DirectiveDefinitionComponentType.Keyword && 
                    directive.Comparer.Equals(component.Key, literal.Literal))
                {
                    score = 2;
                }
                else if (component.Type == DirectiveDefinitionComponentType.Constant && 
                    directive.AcceptsIdentifier(component, literal.Literal))
                {
                    score = 1;
                }
            }
            else if (literal.Type == DirectiveLiteralType.NumericalConstant)
            {
                if (component.Type == DirectiveDefinitionComponentType.Constant &&
                    directive.AcceptsConstant(component, literal.Literal))
                {
                    score = 2;
                }
                else if (component.Type == DirectiveDefinitionComponentType.Variable)
                {
                    score = 1;
                }
            }
            else if (literal.Type == DirectiveLiteralType.StringConstant)
            {
                if (component.Type == DirectiveDefinitionComponentType.Constant && directive.AcceptsConstant(component, literal.Literal))
                {
                    score = 2;
                }
                else if (component.Type == DirectiveDefinitionComponentType.Variable)
                {
                    score = 1;
                }
            }

            return score;
        }

        private IReadOnlyList<MatchedDirective> GetMatchList(IReadOnlyList<DirectiveLiteral> literals, 
            DirectiveDefinitionPlacement placement)
        {
            /* Walk the tokens! */
            var greatestScored = new List<MatchedDirective>();
            Int32 greatestScore = 0;

            List<MatchedDirectiveDefinitionComponent> componentMatches = new List<MatchedDirectiveDefinitionComponent>(literals.Count);
            foreach (var m in m_directives)
            {
                /* Build the match score */
                componentMatches.Clear();
                Int32 score = 0;

                for (Int32 literalIndex = 0; literalIndex < literals.Count; literalIndex++)
                {
                    /* Get the parts to be matched */
                    var literal = literals[literalIndex];
                    var component = m.GetComponent(placement, literalIndex);

                    Int32 componentScore = 0;
                    if (component != null)
                        componentScore = ScoreDirectiveDefinitionComponent(m, component, literal) * (literals.Count - literalIndex);

                    if (componentScore == 0)
                    {
                        score = 0;
                        break;
                    }
                    else
                    {
                        componentMatches.Add(new MatchedDirectiveDefinitionComponent(component, literal));
                        score += componentScore;
                    }
                }

                if (score > 0)
                {
                    if (score >= greatestScore)
                    {
                        if (score > greatestScore)
                        {
                            greatestScored.Clear();
                            greatestScore = score;
                        }

                        greatestScored.Add(new MatchedDirective(m, placement, componentMatches));
                    }
                }
            }

            return greatestScored;
        }

        public IReadOnlyCollection<MatchedDirective> SelectDirective(IReadOnlyList<DirectiveLiteral> literals)
        {
            ValidationHelper.AssertObjectCollectionIsNotEmpty("literals", literals);

            /* Find all the matching defintions by direction */
            var aboveList = GetMatchList(literals, DirectiveDefinitionPlacement.Above);
            var belowList = GetMatchList(literals, DirectiveDefinitionPlacement.Below);

            /* Just fill in the result and go. */
            var result = new List<MatchedDirective>();

            if (aboveList.Count > 0)
                result.AddRange(aboveList);

            if (belowList.Count > 0)
                result.AddRange(belowList);

            return result;
        }
    }
}
