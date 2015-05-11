﻿//
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

namespace XtraLiteTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Parsing;

    internal static class ExceptionHelper
    {
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

        internal static void OperatorAlreadyRegistered(Operator @operator)
        {
            Debug.Assert(@operator != null);
            throw new InvalidOperationException(String.Format("Operator '{0}' (or one of its identifying symbols) already registered.", @operator));
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

        internal static void CannotRegisterDirectiveWithNoTags()
        {
            throw new InvalidOperationException("Cannot register a directive with no defined tags.");
        }
    }
}