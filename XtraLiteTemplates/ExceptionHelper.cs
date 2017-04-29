﻿//  Author:
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

namespace XtraLiteTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Parsing;

    [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
    internal static class ExceptionHelper
    {
        internal static void ArgumentIsNull(string argumentName)
        {
            Debug.Assert(!string.IsNullOrEmpty(argumentName), "argumentName cannot be empty.");

            throw new ArgumentNullException(argumentName, string.Format("Argument \"{0}\" cannot be null.", argumentName));
        }

        internal static void ArgumentIsEmpty(string argumentName)
        {
            Debug.Assert(!string.IsNullOrEmpty(argumentName), "argumentName cannot be empty.");

            throw new ArgumentException(string.Format("Argument \"{0}\" cannot be empty.", argumentName), argumentName);
        }

        internal static void ArgumentIsNotValidIdentifier(string argumentName)
        {
            Debug.Assert(!string.IsNullOrEmpty(argumentName), "argumentName cannot be empty.");

            throw new ArgumentException(string.Format("Argument \"{0}\" does not represent a valid identifer.", argumentName), argumentName);
        }

        internal static void ArgumentsAreEqual(string argumentName1, string argumentName2)
        {
            Debug.Assert(!string.IsNullOrEmpty(argumentName1), "argumentName1 cannot be empty.");
            Debug.Assert(!string.IsNullOrEmpty(argumentName2), "argumentName2 cannot be empty.");

            throw new ArgumentException(string.Format("Arguments \"{0}\" and \"{1}\" cannot be equal.", argumentName1, argumentName2), argumentName1);
        }

        internal static void ArgumentNotGreaterThan(string argumentName, string comparand)
        {
            Debug.Assert(!string.IsNullOrEmpty(argumentName), "argumentName cannot be empty.");
            Debug.Assert(!string.IsNullOrEmpty(comparand), "comparand cannot be empty.");

            throw new ArgumentOutOfRangeException(argumentName, string.Format("Argument \"{0}\" is expected to be greater than {1}.", argumentName, comparand));
        }

        internal static void ArgumentNotGreaterThanOrEqual(string argumentName, string comparand)
        {
            Debug.Assert(!string.IsNullOrEmpty(argumentName), "argumentName cannot be empty.");
            Debug.Assert(!string.IsNullOrEmpty(comparand), "argumentName cannot be comparand.");

            throw new ArgumentOutOfRangeException(argumentName, string.Format("Argument \"{0}\" is expected to be greater than or equal to {1}.", argumentName, comparand));
        }

        internal static void ArgumentNotLessThan(string argumentName, string comparand)
        {
            Debug.Assert(!string.IsNullOrEmpty(argumentName), "argumentName cannot be empty.");
            Debug.Assert(!string.IsNullOrEmpty(comparand), "argumentName cannot be comparand.");

            throw new ArgumentOutOfRangeException(argumentName, string.Format("Argument \"{0}\" is expected to be less than {1}.", argumentName, comparand));
        }

        internal static void ArgumentNotLessThanOrEqual(string argumentName, string comparand)
        {
            Debug.Assert(!string.IsNullOrEmpty(argumentName), "argumentName cannot be empty.");
            Debug.Assert(!string.IsNullOrEmpty(comparand), "argumentName cannot be comparand.");

            throw new ArgumentOutOfRangeException(argumentName, string.Format("Argument \"{0}\" is expected to be less than or equal to {1}.", argumentName, comparand));
        }

        internal static void ConditionFailed(string conditionName)
        {
            Debug.Assert(!string.IsNullOrEmpty(conditionName), "conditionName cannot be empty.");

            throw new ArgumentException(string.Format("Argument condition \"{0}\" failed to be validated as true.", conditionName), conditionName);
        }

        internal static void UnexpectedCharacter(int characterIndex, char character)
        {
            throw new ParseException(null, characterIndex, "Unexpected character '{0}' found at position {1}.", character, characterIndex);
        }

        internal static void UnexpectedEndOfStream(int characterIndex)
        {
            throw new ParseException(null, characterIndex, "Unexpected end of stream detected at position {0}.", characterIndex);
        }

        internal static void InvalidEscapeCharacter(int characterIndex, char character)
        {
            throw new ParseException(null, characterIndex, "Invalid escape character '{0}' at position {1}.", character, characterIndex);
        }

        internal static void UnexpectedToken(Token token)
        {
            Debug.Assert(token != null, "token cannot be null.");

            throw new LexingException(
                null, 
                token,
                "Unexpected token '{0}' (type: {1}) found at position {2}.", 
                token.Value, 
                token.Type, 
                token.CharacterIndex);
        }

        internal static void NoMatchingTagsLeft(IReadOnlyList<object> components, Token token)
        {
            Debug.Assert(token != null, "token cannot be null.");
            Debug.Assert(components != null, "components cannot be null.");

            if (components.Count > 0)
            {
                throw new LexingException(
                    null, 
                    token,
                    "No matching tags composed of {{{3}}} found that can be continued with the token '{0}' (type: {1}) found at position {2}.",
                    token.Value, 
                    token.Type, 
                    token.CharacterIndex,
                    string.Join(" ", components));
            }
            else
            {
                throw new LexingException(
                    null, 
                    token, 
                    "No matching tags found that can be continued with the token '{0}' (type: {1}) found at position {2}.",
                    token.Value, 
                    token.Type, 
                    token.CharacterIndex);
            }
        }

        internal static void UnexpectedOrInvalidExpressionToken(ExpressionException innerException, Token token)
        {
            Debug.Assert(token != null, "token cannot be null.");
            Debug.Assert(innerException != null, "innerException cannot be null.");

            throw new LexingException(
                innerException, 
                token,
                "Unexpected or invalid expression token '{0}' (type: {1}) found at position {2}. Error: {3}",
                token.Value, 
                token.Type, 
                token.CharacterIndex, 
                innerException.Message);
        }

        internal static void UnexpectedTag(TagLex tagLex)
        {
            Debug.Assert(tagLex != null, "tagLex cannot be null.");

            throw new InterpreterException(
                new Directive[0], 
                tagLex.FirstCharacterIndex,
                "Unexpected tag {{{0}}} encountered at position {1}.", 
                tagLex.ToString(), 
                tagLex.FirstCharacterIndex);
        }

        internal static void UnmatchedDirectiveTag(Directive[] candidateDirectives, int firstCharaterIndex)
        {
            Debug.Assert(candidateDirectives != null, "candidateDirectives cannot be null.");

            throw new InterpreterException(
                candidateDirectives, 
                firstCharaterIndex,
                "Directive(s) {0} encountered at position {1}, could not be finalized by matching all component tags.",
                string.Join(" or ", candidateDirectives.AsEnumerable()), 
                firstCharaterIndex);
        }

        internal static void CannotEvaluateOperator(Operator @operator, object constant)
        {
            Debug.Assert(@operator != null, "operator cannot be null.");
            throw new ExpressionException("Operator {0} could not be applied to value '{1}'.", @operator, constant);
        }

        internal static void InvalidExpressionTerm(object term)
        {
            throw new ExpressionException("Invalid expression term: '{0}'.", term);
        }

        internal static void UnexpectedExpressionTerm(object term)
        {
            throw new ExpressionException("Unexpected expression term: '{0}'.", term);
        }

        internal static void UnexpectedLiteralRequiresOperator(object term)
        {
            throw new ExpressionException("Unexpected expression literal value: '{0}'. Expected operator.", term);
        }

        internal static void UnexpectedOperator(string @operator)
        {
            Debug.Assert(!string.IsNullOrEmpty(@operator), "operator cannot be null.");

            throw new ExpressionException("Unexpected expression operator: '{0}'.", @operator);
        }

        internal static void UnexpectedLiteralRequiresIdentifier(string @operator, object literal)
        {
            Debug.Assert(!string.IsNullOrEmpty(@operator), "operator cannot be null.");

            throw new ExpressionException("Operator '{0}' cannot be applied to literal value: '{1}'. Expected identifier.", @operator, literal);
        }

        internal static void OperatorAlreadyRegistered(Operator @operator)
        {
            Debug.Assert(@operator != null, "operator cannot be null.");

            throw new InvalidOperationException(string.Format("Operator '{0}' (or one of its identifying symbols) already registered.", @operator));
        }

        internal static void SpecialCannotBeRegistered(string keyword)
        {
            Debug.Assert(!string.IsNullOrEmpty(keyword), "keyword cannot be empty.");

            throw new InvalidOperationException(string.Format("Special keyword '{0}' cannot be registered as it is currently in use by an operator.", keyword));
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

        internal static void InvalidTagMarkup(string markup)
        {
            throw new FormatException(string.Format("Invalid tag markup: '{0}'", markup));
        }

        internal static void DirectiveEvaluationError(Exception innerException, Directive directive)
        {
            Debug.Assert(directive != null, "directive cannot be null.");
            Debug.Assert(innerException != null, "exception cannot be null.");

            throw new EvaluationException(innerException, "Evaluation of directive {0} resulted in an error: {1}", directive, innerException.Message);
        }

        internal static void InvalidObjectMemberName(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), "name cannot be empty.");

            throw new EvaluationException(null, "Property or method '{0}' could not be located in the provided object.", name);
        }

        internal static void ObjectMemberEvaluationError(Exception innerException, string name)
        {
            Debug.Assert(innerException != null, "innerException cannot be null.");
            Debug.Assert(!string.IsNullOrEmpty(name), "name cannot be empty.");

            throw new EvaluationException(innerException, "Evaluation of property or method '{0}' failed. An exception was raised: {1}", name, innerException.Message);
        }
    }
}