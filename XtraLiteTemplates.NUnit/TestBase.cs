//
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
//

namespace XtraLiteTemplates.NUnit
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Threading;

    using global::NUnit.Framework;

    using XtraLiteTemplates.Compilation;
    using XtraLiteTemplates.Dialects.Standard.Directives;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Introspection;
    using XtraLiteTemplates.NUnit.Inside;
    using XtraLiteTemplates.Parsing;

    [SuppressMessage("ReSharper", "UnusedParameter.Global")]
    public class TestBase
    {
        protected static readonly IObjectFormatter ObjectFormatter = new TestObjectFormatter(CultureInfo.InvariantCulture);
        protected static readonly IPrimitiveTypeConverter TypeConverter = new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture, ObjectFormatter);

        protected static EvaluationContext CreateContext(IEqualityComparer<string> comparer)
        {
            return new EvaluationContext(true, CancellationToken.None, comparer, ObjectFormatter, null, (context, text) => text);
        }

        protected static string Evaluate(CompiledTemplate<EvaluationContext> compiledTemplate, StringComparer comparer, params KeyValuePair<string, object>[] values)
        {
            var context = CreateContext(comparer);
            foreach (var kvp in values)
                context.SetProperty(kvp.Key, kvp.Value);

            string result;
            using (var sw = new StringWriter())
            {
                compiledTemplate.Evaluate(sw, context);
                result = sw.ToString();
            }

            return result;
        }

        protected string Evaluate(string template, Directive directive, params KeyValuePair<string, object>[] values)
        {
            Debug.Assert(directive != null);

            var evaluable = new Interpreter(new Tokenizer(template), ExpressionFlowSymbols.Default, 
                StringComparer.OrdinalIgnoreCase)
                .RegisterDirective(directive)
                .RegisterDirective(new InterpolationDirective(TypeConverter))
                .Compile();

            return Evaluate(evaluable, StringComparer.OrdinalIgnoreCase, values);
        }

        protected static void ExpectArgumentNullException(string argument, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentNullException), e);
                Assert.IsTrue(e.Message.StartsWith(
                    $"Argument \"{argument}\" cannot be null.", 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectArgumentEmptyException(string argument, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
                Assert.IsTrue(e.Message.StartsWith(
                    $"Argument \"{argument}\" cannot be empty.",
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectArgumentNotIdentifierException(string argument, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
                Assert.IsTrue(e.Message.StartsWith($"Argument \"{argument}\" does not represent a valid identifier.", StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }


        protected static void ExpectArgumentsEqualException(string argument1, string argument2, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
                Assert.IsTrue(e.Message.StartsWith(
                    $"Arguments \"{argument1}\" and \"{argument2}\" cannot be equal.", 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentLessThanOrEqualException<T>(string argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                    $"Argument \"{argument}\" is expected to be greater than {than}.", 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentLessThanException<T>(string argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                    $"Argument \"{argument}\" is expected to be greater than or equal to {than}.", 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentGreaterThanOrEqualException<T>(string argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                    $"Argument \"{argument}\" is expected to be less than {than}.", 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentGreaterThanException<T>(string argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                    $"Argument \"{argument}\" is expected to be less than or equal to {than}.", 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectArgumentConditionNotTrueException(string condition, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
                Assert.IsTrue(e.Message.StartsWith(
                    $"Argument condition \"{condition}\" failed to be validated as true.", 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedCharacterException(int index, char character, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual($"Unexpected character '{character}' found at position {index}.", e.Message);
                var exception = e as ParseException;
                if (exception != null)
                {
                    Assert.AreEqual(index, exception.CharacterIndex);
                }

                return;
            }

            Assert.Fail();
        }

        protected static void ExpectInvalidEscapeCharacterException(int index, char character, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual($"Invalid escape character '{character}' at position {index}.", e.Message);
                var exception = e as ParseException;
                if (exception != null)
                {
                    Assert.AreEqual(index, exception.CharacterIndex);
                }

                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedEndOfStreamException(int index, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual($"Unexpected end of stream detected at position {index}.", e.Message);
                var exception = e as ParseException;
                if (exception != null)
                {
                    Assert.AreEqual(index, exception.CharacterIndex);
                }

                return;
            }

            Assert.Fail();
        }



        protected static void ExpectUnexpectedTokenException(int index, int originalLength, string token, Token.TokenType type, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(LexingException), e);
                Assert.AreEqual($"Unexpected token '{token}' (type: {type}) found at position {index}.", e.Message);

                var exception = (LexingException)e;
                Assert.AreEqual(index, exception.Token.CharacterIndex);
                Assert.AreEqual(originalLength, exception.Token.OriginalLength);
                Assert.AreEqual(token, exception.Token.Value);
                Assert.AreEqual(type, exception.Token.Type);

                return;
            }

            Assert.Fail();
        }

        protected static void ExpectNoMatchingTagsLeftException(object[] components, int index, int originalLength, string token, Token.TokenType type, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(LexingException), e);

                if (components != null && components.Length > 0)
                {
                    Assert.AreEqual(string.Format("No matching tags composed of {{{3}}} found that can be continued with the token '{0}' (type: {1}) found at position {2}.", 
                        token, type, index, string.Join(" ", components)), e.Message);
                }
                else
                {
                    Assert.AreEqual(
                        $"No matching tags found that can be continued with the token '{token}' (type: {type}) found at position {index}.", e.Message);
                }
                var exception = (LexingException)e;

                Assert.AreEqual(index, exception.Token.CharacterIndex);
                Assert.AreEqual(originalLength, exception.Token.OriginalLength);
                Assert.AreEqual(token, exception.Token.Value);
                Assert.AreEqual(type, exception.Token.Type);

                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedOrInvalidExpressionTokenException(int index, int originalLength, string token, Token.TokenType type, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(LexingException), e);
                Assert.AreEqual(string.Format("Unexpected or invalid expression token '{0}' (type: {1}) found at position {2}. Error: Invalid expression term: '{0}'.",
                    token, type, index), e.Message);

                var exception = (LexingException)e;
                Assert.AreEqual(index, exception.Token.CharacterIndex);
                Assert.AreEqual(originalLength, exception.Token.OriginalLength);
                Assert.AreEqual(token, exception.Token.Value);
                Assert.AreEqual(type, exception.Token.Type);

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

        protected static void ExpectOperatorAlreadyRegisteredException(string @operator, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual($"Operator '{@operator}' (or one of its identifying symbols) already registered.", e.Message);

                return;
            }

            Assert.Fail();
        }

        protected static void ExpectSpecialCannotBeRegisteredException(string keyword, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual(
                    $"Special keyword '{keyword}' cannot be registered as it is currently in use by an operator.", e.Message);

                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnbalancedExpressionCannotBeFinalizedException(int index, string token, Token.TokenType type, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ParseException), e);
                Assert.AreEqual(
                    $"Unexpected or invalid expression token '{token}' (type: {type}) found at position {index}. Error: Unbalanced expressions cannot be finalized.", e.Message);

                return;
            }

            Assert.Fail();
        }


        protected static void ExpectTagAnyIdentifierCannotFollowExpressionException(Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InvalidOperationException), e);
                Assert.AreEqual("Identifier tag component cannot follow an expression.", e.Message);
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

        protected static void ExpectInvalidTagMarkupException(string markup, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(FormatException), e);
                Assert.AreEqual($"Invalid tag markup: '{markup}'", e.Message);
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

        protected static void ExpectCannotEvaluateUnConstructedExpressionException(Action action)
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

        protected static void ExpectInvalidExpressionTermException(object term, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ExpressionException), e);
                Assert.AreEqual($"Invalid expression term: '{term}'.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedExpressionTermException(object term, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ExpressionException), e);
                Assert.AreEqual($"Unexpected expression term: '{term}'.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedLiteralRequiresOperatorException(object literal, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ExpressionException), e);
                Assert.AreEqual($"Unexpected expression literal value: '{literal}'. Expected operator.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedExpressionOperatorException(string @operator, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ExpressionException), e);
                Assert.AreEqual($"Unexpected expression operator: '{@operator}'.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedLiteralRequiresIdentifierException(string @operator, object literal, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ExpressionException), e);
                Assert.AreEqual(
                    $"Operator '{@operator}' cannot be applied to literal value: '{literal}'. Expected identifier.", e.Message);

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

        protected static void ExpectUnmatchedDirectiveTagException(IEnumerable<Directive> directives, int index, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InterpreterException), e);
                Assert.AreEqual(
                    $"Directive(s) {string.Join(" or ", directives.AsEnumerable())} encountered at position {index}, could not be finalized by matching all component tags.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectUnexpectedTagException(string actualTag, int index, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(InterpreterException), e);
                Assert.AreEqual($"Unexpected tag {{{actualTag}}} encountered at position {index}.", e.Message);    
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectInvalidObjectMemberNameException(string memberName, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(EvaluationException), e);
                Assert.AreEqual($"Property or method '{memberName}' could not be located in the provided object.", e.Message);
                return;
            }

            Assert.Fail();
        }

        protected static void ExpectObjectMemberEvaluationErrorException(string memberName, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(EvaluationException), e);
                Assert.IsTrue(e.Message.StartsWith(
                    $"Evaluation of property or method '{memberName}' failed. An exception was raised:"));
                return;
            }

            Assert.Fail();
        }
    }
}

