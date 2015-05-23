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

namespace XtraLiteTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Parsing;

    internal static class ExceptionHelper
    {
        internal static void ArgumentIsNull(String argumentName)
        {
            Debug.Assert(!String.IsNullOrEmpty(argumentName));

            throw new ArgumentNullException(argumentName, String.Format("Argument \"{0}\" cannot be null.", argumentName));
        }

        internal static void ArgumentIsEmpty(String argumentName)
        {
            Debug.Assert(!String.IsNullOrEmpty(argumentName));

            throw new ArgumentException(String.Format("Argument \"{0}\" cannot be empty.", argumentName), argumentName);
        }

        internal static void ArgumentIsNotValidIdentifier(String argumentName)
        {
            Debug.Assert(!String.IsNullOrEmpty(argumentName));

            throw new ArgumentException(String.Format("Argument \"{0}\" does not represent a valid identifer.", argumentName), argumentName);
        }

        internal static void ArgumentsAreEqual(String argumentName1, String argumentName2)
        {
            Debug.Assert(!String.IsNullOrEmpty(argumentName1));
            Debug.Assert(!String.IsNullOrEmpty(argumentName2));

            throw new ArgumentException(String.Format("Arguments \"{0}\" and \"{1}\" cannot be equal.", argumentName1, argumentName2), argumentName1);
        }


        internal static void ArgumentNotGreaterThan(String argumentName, String comparand)
        {
            Debug.Assert(!String.IsNullOrEmpty(argumentName));
            Debug.Assert(!String.IsNullOrEmpty(comparand));

            throw new ArgumentOutOfRangeException(argumentName, String.Format("Argument \"{0}\" is expected to be greater than {1}.", argumentName, comparand));
        }

        internal static void ArgumentNotGreaterThanOrEqual(String argumentName, String comparand)
        {
            Debug.Assert(!String.IsNullOrEmpty(argumentName));
            Debug.Assert(!String.IsNullOrEmpty(comparand));

            throw new ArgumentOutOfRangeException(argumentName, String.Format("Argument \"{0}\" is expected to be greater than or equal to {1}.", argumentName, comparand));
        }

        internal static void ArgumentNotLessThan(String argumentName, String comparand)
        {
            Debug.Assert(!String.IsNullOrEmpty(argumentName));
            Debug.Assert(!String.IsNullOrEmpty(comparand));

            throw new ArgumentOutOfRangeException(argumentName, String.Format("Argument \"{0}\" is expected to be less than {1}.", argumentName, comparand));
        }

        internal static void ArgumentNotLessThanOrEqual(String argumentName, String comparand)
        {
            Debug.Assert(!String.IsNullOrEmpty(argumentName));
            Debug.Assert(!String.IsNullOrEmpty(comparand));

            throw new ArgumentOutOfRangeException(argumentName, String.Format("Argument \"{0}\" is expected to be less than or equal to {1}.", argumentName, comparand));
        }

        internal static void ConditionFailed(String conditionName)
        {
            Debug.Assert(!String.IsNullOrEmpty(conditionName));

            throw new ArgumentException(String.Format("Argument condition \"{0}\" failed to be validated as true.", conditionName), conditionName);
        }


        internal static void UnexpectedCharacter(Int32 characterIndex, Char character)
        {
            throw new ParseException(characterIndex, "Unexpected character '{0}' found at position {1}.", character, characterIndex);
        }

        internal static void UnexpectedEndOfStream(Int32 characterIndex)
        {
            throw new ParseException(characterIndex, "Unexpected end of stream detected at position {0}.", characterIndex);
        }

        internal static void InvalidEscapeCharacter(Int32 characterIndex, Char character)
        {
            throw new ParseException(characterIndex, "Invalid escape character '{0}' at position {1}.", character, characterIndex);
        }

        internal static void UnexpectedToken(Token token)
        {
            Debug.Assert(token != null);

            throw new ParseException(token.CharacterIndex,
                "Unexpected token '{0}' (type: {1}) found at position {2}.", token.Value, token.Type, token.CharacterIndex);
        }

        internal static void UnexpectedOrInvalidExpressionToken(ExpressionException inner, Token token)
        {
            Debug.Assert(token != null);
            Debug.Assert(inner != null);

            throw new ParseException(token.CharacterIndex,
                "Unexpected or invalid expression token '{0}' (type: {1}) found at position {2}. Error: {3}",
                token.Value, token.Type, token.CharacterIndex, inner.Message);
        }

        internal static void UnexpectedTag(TagLex tagLex)
        {
            Debug.Assert(tagLex != null);

            throw new InterpreterException(new Directive[0], tagLex.FirstCharacterIndex,
                "Unexpected tag {{{0}}} encountered at position {1}.", tagLex.ToString(), tagLex.FirstCharacterIndex);
        }

        internal static void UnexpectedEndOfStreamAfterToken(Token token)
        {
            Debug.Assert(token != null);

            throw new ParseException(token.CharacterIndex,
                "Unexpected end of stream after token '{0}' (type: {1}) at position {2}.", token.Value, token.Type, token.CharacterIndex);
        }

        internal static void CannotEvaluateOperator(Operator @operator, Object constant)
        {
            Debug.Assert(@operator != null);
            throw new ExpressionException("Operator {0} could not be applied to value '{1}'.", @operator, constant);
        }

        internal static void InvalidExpressionTerm(Object term)
        {
            throw new ExpressionException("Invalid expression term: '{0}'.", term);
        }

        internal static void UnexpectedExpressionTerm(Object term)
        {
            throw new ExpressionException("Unexpected expression term: '{0}'.", term);
        }

        internal static void UnexpectedLiteralRequiresOperator(Object term)
        {
            throw new ExpressionException("Unexpected expression literal value: '{0}'. Expected operator.", term);
        }

        internal static void UnexpectedOperator(String @operator)
        {
            Debug.Assert(!String.IsNullOrEmpty(@operator));
            throw new ExpressionException("Unexpected expression operator: '{0}'.", @operator);
        }

        internal static void UnexpectedLiteralRequiresIdentifier(String @operator, Object literal)
        {
            Debug.Assert(!String.IsNullOrEmpty(@operator));
            throw new ExpressionException("Operator '{0}' cannot be applied to literal value: '{1}'. Expected identifier.", @operator, literal);
        }

        internal static void OperatorAlreadyRegistered(Operator @operator)
        {
            Debug.Assert(@operator != null);
            throw new InvalidOperationException(String.Format("Operator '{0}' (or one of its identifying symbols) already registered.", @operator));
        }

        internal static void SpecialCannotBeRegistered(String keyword)
        {
            Debug.Assert(!String.IsNullOrEmpty(keyword));

            throw new InvalidOperationException(String.Format("Special keyword '{0}' cannot be registered as it is currently in use by an operator.", keyword));
        }

        internal static void CannotRegisterOperatorsForStartedExpression()
        {
            throw new InvalidOperationException("Operator registration must be performed before construction.");
        }

        internal static void CannotModifyAConstructedExpression()
        {
            throw new InvalidOperationException("Cannot modify a finalized expression.");
        }

        internal static void CannotConstructExpressionInvalidState()
        {
            throw new ExpressionException("Unbalanced expressions cannot be finalized.");
        }

        internal static void CannotEvaluateUnconstructedExpression()
        {
            throw new InvalidOperationException("Expression has not been finalized.");
        }


        internal static void TagAnyIndentifierCannotFollowExpression()
        {
            throw new InvalidOperationException("Indentifier tag component cannot follow an expression.");
        }

        internal static void TagExpressionCannotFollowExpression()
        {
            throw new InvalidOperationException("Expression tag component cannot follow another expression.");
        }

        internal static void CannotRegisterTagWithNoComponents()
        {
            throw new InvalidOperationException("Cannot register a tag with no defined components.");
        }
        
        internal static void InvalidTagMarkup(String markup)
        {
            throw new FormatException(String.Format("Invalid tag markup: '{0}'", markup));
        }


        internal static void UnmatchedDirectiveTag(Directive[] candidateDirectives, Int32 firstCharaterIndex)
        {
            Debug.Assert(candidateDirectives != null);

            throw new InterpreterException(candidateDirectives, firstCharaterIndex,
                "Directive(s) {0} encountered at position {1}, could not be finalized by matching all component tags.",
                String.Join(" or ", candidateDirectives.AsEnumerable()), firstCharaterIndex);
        }

        internal static void DirectiveEvaluationError(Directive directive, Exception exception)
        {
            Debug.Assert(directive != null);
            Debug.Assert(exception != null);

            throw new EvaluationException(exception, "Evaluation of directive {0} resulted in an error: {1}", directive, exception.Message);
        }


        internal static void InvalidObjectMemberName(String name)
        {
            Debug.Assert(!String.IsNullOrEmpty(name));
            throw new EvaluationException(null, "Property or method '{0}' could not be located in the provided object.", name);
        }

        internal static void ObjectMemberEvaluationError(Exception inner, String name)
        {
            Debug.Assert(inner != null);
            Debug.Assert(!String.IsNullOrEmpty(name));
            throw new EvaluationException(inner, "Evaluation of property or method '{0}' failed. An exception was raised: {1}", name, inner.Message);
        }
    }
}