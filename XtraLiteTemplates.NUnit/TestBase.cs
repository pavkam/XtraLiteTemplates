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
using NUnit.Framework;

namespace XtraLiteTemplates.NUnit
{
    using System;
    using System.Globalization;
    using System.Linq;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Expressions.Operators.Standard;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Evaluation.Directives;
    using XtraLiteTemplates.Parsing;

    public class TestBase
    {
        protected static IPrimitiveTypeConverter CreateTypeConverter()
        {
            return new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture);
        }

        protected void ExpectArgumentNullException(String argument, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentNullException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" cannot be null.", argument), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentEmptyException(String argument, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException)
                {
                    Assert.IsTrue(e.Message.StartsWith(
                            String.Format("Argument \"{0}\" cannot be null.", argument), 
                            StringComparison.CurrentCulture));
                }
                else
                {
                    Assert.IsInstanceOf(typeof(ArgumentException), e);
                    Assert.IsTrue(e.Message.StartsWith(
                            String.Format("Argument \"{0}\" cannot be empty.", argument), 
                            StringComparison.CurrentCulture));
                }

                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentNotIdentifierException(String argument, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException)
                {
                    Assert.IsTrue(e.Message.StartsWith(
                            String.Format("Argument \"{0}\" cannot be null.", argument),
                            StringComparison.CurrentCulture));
                }
                else 
                {
                    Assert.IsInstanceOf(typeof(ArgumentException), e);

                    var isGoodOne = 
                        e.Message.StartsWith(String.Format("Argument \"{0}\" cannot be empty.", argument), StringComparison.CurrentCulture) ||
                        e.Message.StartsWith(String.Format("Argument \"{0}\" does not represent a valid identifer.", argument), StringComparison.CurrentCulture);

                    Assert.IsTrue(isGoodOne);
                }

                return;
            }

            Assert.Fail();
        }


        protected void ExpectArgumentsEqualException(String argument1, String argument2, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Arguments \"{0}\" and \"{1}\" cannot be equal.", argument1, argument2), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentLessThanOrEqualException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be greater than {1}.", argument, than), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentLessThanException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be greater than or equal to {1}.", argument, than), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentGreaterThanOrEqualException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be less than {1}.", argument, than), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentGreaterThanException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be less than or equal to {1}.", argument, than), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentConditionNotTrueException(String condition, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument condition \"{0}\" failed to be validated as true.", condition), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }


        protected static void ExpectUnexpectedCharacterException(Int32 index, Char character, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Unexpected character '{0}' found at position {1}.", character, index), e.Message);
                if (e is ParseException)
                {
                    Assert.AreEqual(index, (e as ParseException).CharacterIndex);
                }
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectInvalidEscapeCharacterException(Int32 index, Char character, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Invalid escape character '{0}' at position {1}.", character, index), e.Message);
                if (e is ParseException)
                {
                    Assert.AreEqual(index, (e as ParseException).CharacterIndex);
                }
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedEndOfStreamException(Int32 index, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Unexpected end of stream detected at position {0}.", index), e.Message);
                if (e is ParseException)
                {
                    Assert.AreEqual(index, (e as ParseException).CharacterIndex);
                }
                return;
            }

            Assert.Fail();
        }


        protected static void ExpectUnexpectedTokenException(Int32 index, String token, Token.TokenType type, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Unexpected token '{0}' (type: {1}) found at position {2}.", token, type, index), e.Message);
                Assert.AreEqual(index, (e as ParseException).CharacterIndex);

                return;
            }

            Assert.Fail();
        }

        protected static void ExpectCannotRegisterTagWithNoComponentsException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Cannot register a tag with no defined components.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectOperatorAlreadyRegisteredException(String @operator, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual(String.Format("Operator '{0}' (or one of its identifying symbols) already registered.", @operator), e.Message);

                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnbalancedExpressionCannotBeFinalizedException(Int32 index, String token, Token.TokenType type, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Unexpected or invalid expression token '{0}' (type: {1}) found at position {2}. Error: Unbalanced expressions cannot be finalized.",
                    token, type, index), e.Message);

                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedOrInvalidExpressionTokenException(Int32 index, String token, Token.TokenType type, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual(String.Format("Unexpected or invalid expression token '{0}' (type: {1}) found at position {2}. Error: Invalid expression term: '{0}'.",
                    token, type, index), e.Message);

                return;
            }

            Assert.Fail();
        }


        protected static void ExpectTagAnyIndentifierCannotFollowExpressionException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Indentifier tag component cannot follow an expression.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectTagExpressionCannotFollowExpressionException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Expression tag component cannot follow another expression.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectInvalidTagMarkupException(String markup, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(FormatException), e);
                Assert.AreEqual(String.Format("Invalid tag markup: '{0}'", markup), e.Message);
                return;
            }

            Assert.Fail();
        }


        protected static void ExpectCannotModifyAConstructedExpressionException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Cannot modify a finalized expression.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectCannotRegisterOperatorsForStartedExpressionException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Operator registration must be performed before construction.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectCannotEvaluateUnconstructedExpressionException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Expression has not been finalized.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectInvalidExpressionTermException(Object term, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ExpressionException), e);
                Assert.AreEqual(String.Format("Invalid expression term: '{0}'.", term), e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedExpressionTermException(Object term, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ExpressionException), e);
                Assert.AreEqual(String.Format("Unexpected expression term: '{0}'.", term), e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectCannotConstructExpressionInvalidStateException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ExpressionException), e);
                Assert.AreEqual("Unbalanced expressions cannot be finalized.", e.Message);
                return;
            }

            Assert.Fail();
        }


        protected static void ExpectUnmatchedDirectiveTagException(Directive[] directives, Int32 index, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InterpreterException), e);
                Assert.AreEqual(String.Format("Directive(s) {0} encountered at position {1}, could not be finalized by matching all component tags.", 
                    String.Join(" or ", directives.AsEnumerable()), index), e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedTagException(String actualTag, Int32 index, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InterpreterException), e);
                Assert.AreEqual(String.Format("Unexpected tag {{{0}}} encountered at position {1}.", actualTag, index), e.Message);    
                return;
            }

            Assert.Fail();
        }
    }
}

