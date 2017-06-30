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
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Text;
    using Evaluation;
    using Expressions;
    using Expressions.Operators;
    using Introspection;
    using JetBrains.Annotations;

    /// <summary>
    /// Abstract base class for all standard dialects supported by this library. Defines a set of common properties and behaviors that concrete
    /// dialect implementations can use out-of-the box.
    /// </summary>
    [PublicAPI]
    public abstract class StandardDialectBase : IDialect, IObjectFormatter
    {
        [NotNull]
        private readonly IPrimitiveTypeConverter _typeConverter;
        private readonly DialectCasing _dialectCasing;
        [CanBeNull]
        private List<Operator> _dialectOperators;
        [CanBeNull]
        private List<Directive> _dialectDirectives;
        [CanBeNull]
        private Dictionary<string, object> _dialectSpecialConstants;
        [CanBeNull]
        private Dictionary<object, string> _dialectSpecialConstantIdentifiers;
        [NotNull]
        private string _dialectUndefinedSpecialIdentifier;
        [CanBeNull]
        private object _selfObject;

        /// <summary>
        /// Initializes a new instance of the <see cref="StandardDialectBase" /> class.
        /// <remarks>
        /// This constructor is not actually called directly.
        /// </remarks>
        /// </summary>
        /// <param name="name">A human-readable name for the dialect.</param>
        /// <param name="culture">A <see cref="CultureInfo" /> object that drives the formatting and collation behavior of the dialect.</param>
        /// <param name="casing">A <see cref="DialectCasing" /> value that controls the dialect string casing behavior.</param>
        /// <exception cref="ArgumentNullException">Either argument <paramref name="name" /> or <paramref name="culture" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="name" /> is an empty string.</exception>
        protected StandardDialectBase([NotNull] string name, [NotNull] CultureInfo culture, DialectCasing casing)
        {
            Expect.NotEmpty(nameof(name), name);
            Expect.NotNull(nameof(culture), culture);

            /* Build culture-aware values.*/
            Name = name;
            Culture = culture;
            _typeConverter = new FlexiblePrimitiveTypeConverter(Culture, this);
            _dialectCasing = casing;

            var casedUndefined = AdjustCasing("Undefined");
            Debug.Assert(casedUndefined != null);
            _dialectUndefinedSpecialIdentifier = casedUndefined;

            var comparer = StringComparer.Create(culture, casing == DialectCasing.IgnoreCase);

            IdentifierComparer = comparer;
            StringLiteralComparer = comparer;
        }

        /// <summary>
        /// Gets the <see cref="CultureInfo" /> object that drives the formatting and collation behavior of the dialect.
        /// <remarks>The value of this property is supplied by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The culture.
        /// </value>
        public CultureInfo Culture { get; }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{String}" /> object used to compare keywords and identifiers.
        /// <remarks>The value of this property is synthesized at construction time from the provided <see cref="CultureInfo" /> and
        /// <see cref="DialectCasing" /> properties.</remarks>
        /// </summary>
        /// <value>
        /// The identifier comparer.
        /// </value>
        public IEqualityComparer<string> IdentifierComparer { get; }

        /// <summary>
        /// Gets the <see cref="IObjectFormatter" /> object used obtain string representation of objects.
        /// </summary>
        /// <value>
        /// The object formatter.
        /// </value>
        public IObjectFormatter ObjectFormatter => this;

        /// <summary>
        /// Gets the <see cref="IComparer{String}" /> object used to compare string literals when evaluating expressions.
        /// <remarks>The value of this property is synthesized at construction time from the provided <see cref="CultureInfo" /> object.</remarks>
        /// </summary>
        /// <value>
        /// The string literal comparer.
        /// </value>
        [NotNull]
        public IComparer<string> StringLiteralComparer { get; }

        /// <summary>
        /// Gets the dialect's human-readable name.
        /// <remarks>The value of this property is supplied at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The name of the dialect.
        /// </value>
        [NotNull]
        public string Name { get; }

        /// <summary>
        /// Gets the expression flow symbols used in expressions of this dialect.
        /// <remarks>The standard set of flow symbols, specified by <see cref="ExpressionFlowSymbols.Default" /> property are returned.</remarks>
        /// </summary>
        /// <value>
        /// The flow symbols.
        /// </value>
        public ExpressionFlowSymbols FlowSymbols => ExpressionFlowSymbols.Default;

        /// <summary>
        /// Gets all dialect supported expression operators.
        /// </summary>
        /// <value>
        /// The operators.
        /// </value>
        public IReadOnlyCollection<Operator> Operators =>
            _dialectOperators ?? (_dialectOperators = new List<Operator>(CreateOperators(_typeConverter)));

        /// <summary>
        /// Gets all dialect supported directives.
        /// </summary>
        /// <value>
        /// The directives.
        /// </value>
        public IReadOnlyCollection<Directive> Directives => 
            _dialectDirectives ?? (_dialectDirectives = new List<Directive>(CreateDirectives(_typeConverter)));

        /// <summary>
        /// Gets all dialect supported special constants.
        /// </summary>
        /// <value>
        /// The special keywords.
        /// </value>
        public IReadOnlyDictionary<string, object> SpecialKeywords
        {
            get
            {
                if (_dialectSpecialConstants == null)
                {
                    _dialectSpecialConstants = new Dictionary<string, object>(IdentifierComparer);
                    _dialectSpecialConstantIdentifiers = new Dictionary<object, string>();

                    foreach (var kvp in CreateSpecials())
                    {
                        Debug.Assert(_dialectSpecialConstants != null);
                        _dialectSpecialConstants.Add(kvp.Key, kvp.Value);
                        if (kvp.Value == null)
                        {
                            _dialectUndefinedSpecialIdentifier = kvp.Key;
                        }
                        else
                        {
                            Debug.Assert(_dialectSpecialConstantIdentifiers != null);
                            _dialectSpecialConstantIdentifiers[kvp.Value] = kvp.Key;
                        }
                    }
                }

                Debug.Assert(_dialectSpecialConstants != null);
                return _dialectSpecialConstants;
            }
        }

        /// <summary>
        /// Gets the tag start character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The tag start character.
        /// </value>
        public abstract char StartTagCharacter { get; }

        /// <summary>
        /// Gets the tag end character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The tag end character.
        /// </value>
        public abstract char EndTagCharacter { get; }

        /// <summary>
        /// Gets the string literal start character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal start character.
        /// </value>
        public abstract char StartStringLiteralCharacter { get; }

        /// <summary>
        /// Gets the string literal end character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal end character.
        /// </value>
        public abstract char EndStringLiteralCharacter { get; }

        /// <summary>
        /// Gets the string literal escape character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal escape character.
        /// </value>
        public abstract char StringLiteralEscapeCharacter { get; }

        /// <summary>
        /// Gets the number literal decimal separator character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The number literal decimal separator character.
        /// </value>
        public abstract char NumberDecimalSeparatorCharacter { get; }

        /// <summary>
        /// Gets the <c>self</c> object. The <c>self</c> object is used to expose global properties and methods
        /// to the evaluation engine.
        /// </summary>
        /// <value>
        /// The self object.
        /// </value>
        public virtual object Self => _selfObject ?? (_selfObject = CreateSelfObject(_typeConverter));

        /// <summary>
        /// Processes all un-parsed text blocks read from the original template. The method current implementation
        /// trims all white-spaces and new line characters and replaces them with a single white-space character (emulates how HTML trimming works).
        /// <remarks>Descendant classes can override this method and modify this behavior.</remarks>
        /// </summary>
        /// <param name="context">The <see cref="IExpressionEvaluationContext" /> instance containing the current evaluation state.</param>
        /// <param name="unParsedText">The text block being processed.</param>
        /// <returns>
        /// The processed text value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context" /> is <c>null</c>.</exception>
        public virtual string DecorateUnParsedText(IExpressionEvaluationContext context, string unParsedText)
        {
            Expect.NotNull(nameof(context), context);

            if (string.IsNullOrEmpty(unParsedText))
            {
                return string.Empty;
            }

            /* Trim all 1+ white spaces to one space character. */
            var result = new StringBuilder();
            var putSpace = false;
            foreach (var c in unParsedText)
            {
                if (char.IsWhiteSpace(c))
                {
                    if (putSpace)
                    {
                        putSpace = false;
                        result.Append(' ');
                    }
                }
                else
                {
                    result.Append(c);
                    putSpace = true;
                }
            }

            if (result.Length > 0 && char.IsWhiteSpace(result[result.Length - 1]))
            {
                result.Length -= 1;
            }

            return result.ToString();
        }

        /// <summary>
        /// Returns a human-readable representation of the dialect instance.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override string ToString()
        {
            string caseDescription = null;

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_dialectCasing)
            {
                case DialectCasing.IgnoreCase:
                    caseDescription = "Ignore Case";
                    break;
                case DialectCasing.LowerCase:
                    caseDescription = "Lower Case";
                    break;
                case DialectCasing.UpperCase:
                    caseDescription = "Upper Case";
                    break;
            }

            return string.IsNullOrEmpty(Culture.Name) ? 
                $"{Name} ({caseDescription})" : 
                $"{Name} ({Culture.Name}, {caseDescription})";
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to the current <see cref="StandardDialectBase" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current dialect class instance.</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            var other = obj as StandardDialect;
            return
                other != null &&
                other.Name.Equals(Name) &&
                other._dialectCasing.Equals(_dialectCasing) &&
                other.Culture.Equals(Culture);
        }

        /// <summary>
        /// Calculates the hash for this dialect class instance.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="StandardDialectBase" />.
        /// </returns>
        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ _dialectCasing.GetHashCode() ^ Culture.GetHashCode();
        }

        /// <summary>
        /// Gets the string representation of an <see cref="object" /> using the given <paramref name="formatProvider"/>.
        /// </summary>
        /// <param name="obj">The object to obtain the string representation for.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// The string representation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="formatProvider"/> is <c>null</c>.</exception>
        [NotNull]
        string IObjectFormatter.ToString([CanBeNull] object obj, [NotNull] IFormatProvider formatProvider)
        {
            string result;

            if (obj == null)
            {
                return _dialectUndefinedSpecialIdentifier;
            }

            if (!_dialectSpecialConstantIdentifiers.TryGetValue(obj, out result))
            {
                var s = obj as string;
                if (s != null)
                {
                    result = s;
                }
                else if (obj is IFormattable)
                {
                    result = ((IFormattable) obj).ToString(null, formatProvider);
                }
                else
                {
                    result = obj.ToString();
                }
            }

            return result;
        }

        /// <summary>
        /// Gets the string representation of an <see cref="object"/>.
        /// </summary>
        /// <param name="obj">The object to obtain the string representation for.</param>
        /// <returns>The string representation.</returns>
        string IObjectFormatter.ToString([CanBeNull] object obj)
        {
            return ((IObjectFormatter)this).ToString(obj, Culture);
        }

        /// <summary>
        /// Override in descendant classes to supply an instance of the <see cref="Self"/> object.
        /// </summary>
        /// <param name="typeConverter">The concrete <see cref="IPrimitiveTypeConverter" /> implementation used for type conversions.</param>
        /// <returns>
        /// An instance of the self object.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> is <c>null</c>.</exception>
        [NotNull]
        [SuppressMessage("ReSharper", "VirtualMemberNeverOverridden.Global")]
        protected virtual StandardSelfObject CreateSelfObject([NotNull] IPrimitiveTypeConverter typeConverter)
        {
            Expect.NotNull(nameof(typeConverter), typeConverter);

            return new StandardSelfObject(typeConverter);
        }

        /// <summary>
        /// Override in descendant classes to supply all dialect supported operators.
        /// </summary>
        /// <param name="typeConverter">The concrete <see cref="IPrimitiveTypeConverter" /> implementation used for type conversions.</param>
        /// <returns>
        /// An array of all supported operators.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> is <c>null</c>.</exception>
        [NotNull]
        protected abstract IEnumerable<Operator> CreateOperators([NotNull] IPrimitiveTypeConverter typeConverter);

        /// <summary>
        /// Override in descendant classes to supply all dialect supported directives.
        /// </summary>
        /// <param name="typeConverter">The concrete <see cref="T:XtraLiteTemplates.Dialects.Standard.IPrimitiveTypeConverter" /> implementation used for type conversions.</param>
        /// <returns>
        /// An array of all supported directives.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> is <c>null</c>.</exception>
        [NotNull]
        protected abstract IEnumerable<Directive> CreateDirectives([NotNull] IPrimitiveTypeConverter typeConverter);

        /// <summary>
        /// Override in descendant classes to supply all dialect supported special constants.
        /// </summary>
        /// <returns>
        /// An array of all supported special constants.
        /// </returns>
        [NotNull]
        protected abstract IEnumerable<KeyValuePair<string, object>> CreateSpecials();

        /// <summary>
        /// Adjusts the case of a string based on the <see cref="DialectCasing" /> supplied during construction.
        /// </summary>
        /// <param name="markup">The string to adjust the case for.</param>
        /// <returns>
        /// Case-adjusted string.
        /// </returns>
        /// <remarks>
        /// Descendant classes need to call this method when creating directives and operators to adjust the case accordingly.
        /// </remarks>
        [CanBeNull]
        protected string AdjustCasing([CanBeNull] string markup)
        {
            if (string.IsNullOrEmpty(markup))
            {
                return markup;
            }

            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (_dialectCasing)
            {
                case DialectCasing.LowerCase:
                    return markup.ToLower(Culture);
                case DialectCasing.UpperCase:
                    return markup.ToUpper(Culture);
                default:
                    return markup;
            }
        }
    }
}
