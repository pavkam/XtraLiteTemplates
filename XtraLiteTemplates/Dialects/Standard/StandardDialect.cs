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


namespace XtraLiteTemplates.Dialects.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Directives;
    using Evaluation;
    using Expressions;
    using Expressions.Operators;
    using Introspection;
    using JetBrains.Annotations;
    using Operators;

    /// <summary>
    /// The standard dialect. Contains the full set of supported expression operators, directives and special constants.
    /// This is the default, medium-verbosity dialect exposed by this library.
    /// See <seealso cref="CodeMonkeyDialect" /> for a less verbose, programmer-oriented dialect.
    /// </summary>
    [PublicAPI]
    public class StandardDialect : StandardDialectBase
    {
        /// <summary>
        /// Initializes static members of the <see cref="StandardDialect"/> class.
        /// </summary>
        static StandardDialect()
        {
            DefaultIgnoreCase = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.IgnoreCase);
            Default = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.UpperCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDialect"/> class.
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo" /> object that drives the formatting and collation behavior of the dialect.</param>
        /// <param name="casing">A <see cref="DialectCasing" /> value that controls the dialect string casing behavior.</param>
        /// <exception cref="ArgumentNullException">Argument <paramref name="culture" /> is <c>null</c>.</exception>
        public StandardDialect([NotNull]CultureInfo culture, DialectCasing casing)
            : this("Standard", culture, casing)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDialect" /> class. The instance is culture-invariant and case-insensitive.
        /// </summary>
        public StandardDialect()
            : this(CultureInfo.InvariantCulture, DialectCasing.IgnoreCase)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDialect"/> class.
        /// </summary>
        /// <param name="name">A human-readable name for the dialect.</param>
        /// <param name="culture">A <see cref="CultureInfo" /> object that drives the formatting and collation behavior of the dialect.</param>
        /// <param name="casing">A <see cref="DialectCasing" /> value that controls the dialect string casing behavior.</param>
        /// <exception cref="ArgumentNullException">Either argument <paramref name="name" /> or <paramref name="culture" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="name" /> is an empty string.</exception>
        protected StandardDialect([NotNull] string name, [NotNull] CultureInfo culture, DialectCasing casing)
            : base(name, culture, casing)
        {
            PreformattedStateObject = new object();
        }

        /// <summary>
        /// Gets a culture-invariant, case-insensitive instance of <see cref="StandardDialect"/> class.
        /// </summary>
        /// <value>
        /// The culture-invariant, case-insensitive instance of <see cref="StandardDialect"/> class.
        /// </value>
        [NotNull]
        public static IDialect DefaultIgnoreCase { get; }

        /// <summary>
        /// Gets a culture-invariant, case-sensitive (upper cased) instance of <see cref="StandardDialect"/> class.
        /// </summary>
        /// <value>
        /// The culture-invariant, case-sensitive instance of <see cref="StandardDialect"/> class.
        /// </value>
        [NotNull]
        public static IDialect Default { get; }

        /// <summary>
        /// Specifies the tag start character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The tag start character.
        /// </value>
        public override char StartTagCharacter => '{';

        /// <summary>
        /// Specifies the tag end character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The tag end character.
        /// </value>
        public override char EndTagCharacter => '}';

        /// <summary>
        /// Specifies the string literal start character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal start character.
        /// </value>
        public override char StartStringLiteralCharacter => '"';

        /// <summary>
        /// Specifies the string literal end character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal end character.
        /// </value>
        public override char EndStringLiteralCharacter => '"';

        /// <summary>
        /// Specifies the string literal escape character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal escape character.
        /// </value>
        public override char StringLiteralEscapeCharacter => '\\';

        /// <summary>
        /// Specifies the number literal decimal separator character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The number literal decimal separator character.
        /// </value>
        public override char NumberDecimalSeparatorCharacter => 
            Culture.NumberFormat.NumberDecimalSeparator.Length != 1 ? '.' : Culture.NumberFormat.NumberDecimalSeparator[0];

        /// <summary>
        /// Gets the state object that can be used by directive to disable the unhandled text trimming behavior.
        /// </summary>
        /// <value>
        /// The preformatted state object.
        /// </value>
        [CanBeNull]
        protected object PreformattedStateObject { get; }

        /// <summary>
        /// Processes all un-parsed text blocks read from the original template. The method current implementation
        /// trims all white-spaces and new line characters and replaces them with a single white-space character (emulates how HTML trimming works).
        /// A preformatted directive is available to allow disabling this default behavior and preserving all original formatting.
        /// </summary>
        /// <param name="context">The <see cref="IExpressionEvaluationContext" /> instance containing the current evaluation state.</param>
        /// <param name="unParsedText">The text block being processed.</param>
        /// <returns>
        /// The processed text value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context" /> is <c>null</c>.</exception>
        public override string DecorateUnParsedText(IExpressionEvaluationContext context, string unParsedText)
        {
            Expect.NotNull(nameof(context), context);

            if (PreformattedStateObject != null && context.ContainsStateObject(PreformattedStateObject))
            {
                return unParsedText;
            }

            return base.DecorateUnParsedText(context, unParsedText);
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to the current <see cref="StandardDialect" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current dialect class instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj as StandardDialect);
        }

        /// <summary>
        /// Calculates the hash for this dialect class instance.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="StandardDialect" />.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ GetType().GetHashCode();
        }

        /// <summary>
        /// Override in descendant classes to supply all dialect supported operators.
        /// </summary>
        /// <param name="typeConverter">The concrete <see cref="IPrimitiveTypeConverter" /> implementation used for type conversions.</param>
        /// <returns>
        /// An array of all supported operators.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> is <c>null</c>.</exception>
        protected override IEnumerable<Operator> CreateOperators(IPrimitiveTypeConverter typeConverter)
        {
            Debug.Assert(typeConverter != null);

            return new Operator[]
            {
                new RelationalEqualsOperator(StringLiteralComparer, typeConverter),
                new RelationalNotEqualsOperator(StringLiteralComparer, typeConverter),
                new RelationalGreaterThanOperator(StringLiteralComparer, typeConverter),
                new RelationalGreaterThanOrEqualsOperator(StringLiteralComparer, typeConverter),
                new RelationalLowerThanOperator(StringLiteralComparer, typeConverter),
                new RelationalLowerThanOrEqualsOperator(StringLiteralComparer, typeConverter),
                new LogicalAndOperator(typeConverter),
                new LogicalOrOperator(typeConverter),
                new LogicalNotOperator(typeConverter),
                new BitwiseAndOperator(typeConverter),
                new BitwiseOrOperator(typeConverter),
                new BitwiseXorOperator(typeConverter),
                new BitwiseNotOperator(typeConverter),
                new BitwiseShiftLeftOperator(typeConverter),
                new BitwiseShiftRightOperator(typeConverter),
                new ArithmeticDivideOperator(typeConverter),
                new ArithmeticModuloOperator(typeConverter),
                new ArithmeticMultiplyOperator(typeConverter),
                new ArithmeticNegateOperator(typeConverter),
                new ArithmeticNeutralOperator(typeConverter),
                new ArithmeticSubtractOperator(typeConverter),
                new ArithmeticSumOperator(typeConverter),
                new SequenceOperator(typeConverter),
                new FormatOperator(Culture, typeConverter),
            };
        }

        /// <summary>
        /// Override in descendant classes to supply all dialect supported directives.
        /// </summary>
        /// <param name="typeConverter">The concrete <see cref="IPrimitiveTypeConverter" /> implementation used for type conversions.</param>
        /// <returns>
        /// An array of all supported directives.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> is <c>null</c>.</exception>
        protected override IEnumerable<Directive> CreateDirectives(IPrimitiveTypeConverter typeConverter)
        {
            Debug.Assert(typeConverter != null);

            return new Directive[]
            {
                new ConditionalInterpolationDirective(AdjustCasing("$ IF $"), false, typeConverter),
                new ForEachDirective(AdjustCasing("FOR EACH ? IN $"), AdjustCasing("END"), typeConverter),
                new IfDirective(AdjustCasing("IF $ THEN"), AdjustCasing("END"), typeConverter),
                new IfElseDirective(AdjustCasing("IF $ THEN"), AdjustCasing("ELSE"), AdjustCasing("END"), typeConverter),
                new InterpolationDirective(typeConverter),
                new RepeatDirective(AdjustCasing("REPEAT $ TIMES"), AdjustCasing("END"), typeConverter),
                new PreFormattedUnParsedTextDirective(AdjustCasing("PREFORMATTED"), AdjustCasing("END"), PreformattedStateObject, typeConverter),
            };
        }

        /// <summary>
        /// Override in descendant classes to supply all dialect supported special constants.
        /// </summary>
        /// <returns>
        /// An array of all supported special constants.
        /// </returns>
        protected override IEnumerable<KeyValuePair<string, object>> CreateSpecials()
        {
            return new[]
            {
                new KeyValuePair<string, object>(AdjustCasing("True"), true),
                new KeyValuePair<string, object>(AdjustCasing("False"), false),
                new KeyValuePair<string, object>(AdjustCasing("Undefined"), null),
                new KeyValuePair<string, object>(AdjustCasing("NaN"), double.NaN),
                new KeyValuePair<string, object>(AdjustCasing("Infinity"), double.PositiveInfinity),
            };
        }
    }
}