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

namespace XtraLiteTemplates.Dialects.Standard
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Dialects.Standard.Operators;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Dialects.Standard.Directives;

    /// <summary>
    /// Abstract base class for all standard dialects supported by this library. Defines a set of common properties and behaviors that concrete
    /// dialect implementations can use out-of-the box.
    /// </summary>
    public abstract class StandardDialectBase : IDialect, IObjectFormatter
    {
        private IPrimitiveTypeConverter m_typeConverter;
        private DialectCasing m_casing;
        private List<Operator> m_operators;
        private List<Directive> m_directives;
        private Dictionary<String, Object> m_specials;
        private Dictionary<Object, String> m_specialsIdentifiers;
        private String m_undefinedSpecialIdentifier;

        /// <summary>
        /// Override in descendant classes to supply all dialect supported operators.
        /// </summary>
        /// <param name="typeConverter">The concrete <see cref="IPrimitiveTypeConverter" /> implentation used for type conversions.</param>
        /// <returns>
        /// An array of all supported operators.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> is <c>null</c>.</exception>
        protected abstract Operator[] CreateOperators(IPrimitiveTypeConverter typeConverter);

        /// <summary>
        /// Override in descendant classes to supply all dialect supported directives.
        /// </summary>
        /// <param name="typeConverter">The concrete <see cref="T:XtraLiteTemplates.Dialects.Standard.IPrimitiveTypeConverter" /> implementation used for type conversions.</param>
        /// <returns>
        /// An array of all supported directives.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="typeConverter" /> is <c>null</c>.</exception>
        protected abstract Directive[] CreateDirectives(IPrimitiveTypeConverter typeConverter);

        /// <summary>
        /// Override in descendant classes to supply all dialect supported special constants.
        /// </summary>
        /// <returns>
        /// An array of all supported special constants.
        /// </returns>
        protected abstract KeyValuePair<String, Object>[] CreateSpecials();

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
        protected String AdjustCasing(String markup)
        {
            if (String.IsNullOrEmpty(markup))
                return markup;

            if (this.m_casing == DialectCasing.LowerCase)
                return markup.ToLower(this.Culture);
            else if (this.m_casing == DialectCasing.UpperCase)
                return markup.ToUpper(this.Culture);
            else
                return markup;
        }

        /// <summary>
        /// Creates a new instance of <see cref="StandardDialectBase" /> class.
        /// <remarks>
        /// This constructor is not actually called directly.
        /// </remarks>
        /// </summary>
        /// <param name="name">A human-readable name for the dialect.</param>
        /// <param name="culture">A <see cref="CultureInfo" /> object that drives the formatting and collation behaviour of the dialect.</param>
        /// <param name="casing">A <see cref="DialectCasing" /> value that controls the dialect string casing behaviour.</param>
        /// <exception cref="ArgumentNullException">Either argument <paramref name="name" /> or <paramref name="culture" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException">Argument <paramref name="name" /> is an empty string.</exception>
        protected StandardDialectBase(String name, CultureInfo culture, DialectCasing casing)
        {
            Expect.NotEmpty("name", name);
            Expect.NotNull("culture", culture);

            /* Build culture-aware values.*/
            this.Name = name;
            this.Culture = culture;
            this.m_typeConverter = new FlexiblePrimitiveTypeConverter(Culture, this);
            this.m_casing = casing;
            this.m_undefinedSpecialIdentifier = this.AdjustCasing("Undefined");

            var comparer = StringComparer.Create(culture, casing == DialectCasing.IgnoreCase);

            this.IdentifierComparer = comparer;
            this.StringLiteralComparer = comparer;
        }

        /// <summary>
        /// Specifies the <see cref="CultureInfo" /> object that drives the formatting and collation behaviour of the dialect.
        /// <remarks>The value of this property is supplied by the caller at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The culture.
        /// </value>
        public CultureInfo Culture { get; private set;  }

        /// <summary>
        /// Specifies the <see cref="IEqualityComparer{String}" /> object used to compare keywords and identifiers.
        /// <remarks>The value of this property is synthesized at construction time from the provided <see cref="CultureInfo" /> and
        /// <see cref="DialectCasing" /> properties.</remarks>
        /// </summary>
        /// <value>
        /// The identifier comparer.
        /// </value>
        public IEqualityComparer<String> IdentifierComparer { get; private set; }

        /// <summary>
        /// Specifies the <see cref="IComparer{String}" /> object used to compare string literals when evaluating expressions.
        /// <remarks>The value of this property is synthesized at construction time from the provided <see cref="CultureInfo" /> object.</remarks>
        /// </summary>
        /// <value>
        /// The string literal comparer.
        /// </value>
        public IComparer<String> StringLiteralComparer { get; private set; }

        /// <summary>
        /// Specifies the dialect's human-readable name.
        /// <remarks>The value of this property is supplied at construction time.</remarks>
        /// </summary>
        /// <value>
        /// The name of the dialect.
        /// </value>
        public String Name { get; private set; }

        /// <summary>
        /// Specifies the expression flow symbols used in expressions of this dialect.
        /// <remarks>The standard set of flow symbols, specified by <see cref="ExpressionFlowSymbols.Default" /> property are returned.</remarks>
        /// </summary>
        /// <value>
        /// The flow symbols.
        /// </value>
        public ExpressionFlowSymbols FlowSymbols
        {
            get 
            {
                return ExpressionFlowSymbols.Default;
            }
        }

        /// <summary>
        /// Lists all dialect supported expression operators.
        /// </summary>
        /// <value>
        /// The operators.
        /// </value>
        public IReadOnlyCollection<Operator> Operators
        { 
            get
            {
                if (m_operators == null)
                    m_operators = new List<Operator>(CreateOperators(m_typeConverter));

                return m_operators;
            }
        }

        /// <summary>
        /// Lists all dialect supported directives.
        /// </summary>
        /// <value>
        /// The directives.
        /// </value>
        public IReadOnlyCollection<Directive> Directives
        { 
            get
            {
                if (m_directives == null)
                    m_directives = new List<Directive>(CreateDirectives(m_typeConverter));

                return m_directives;
            }
        }

        /// <summary>
        /// Lists all dialect supported special constants.
        /// </summary>
        /// <value>
        /// The special keywords.
        /// </value>
        public IReadOnlyDictionary<String, Object> SpecialKeywords
        {
            get
            {
                if (m_specials == null)
                {
                    m_specials = new Dictionary<String, Object>(IdentifierComparer);
                    m_specialsIdentifiers = new Dictionary<Object, String>();

                    foreach (var kvp in CreateSpecials())
                    {
                        m_specials.Add(kvp.Key, kvp.Value);
                        if (kvp.Value == null)
                            m_undefinedSpecialIdentifier = kvp.Key;
                        else
                            m_specialsIdentifiers[kvp.Value] = kvp.Key;
                    }
                }

                return m_specials;
            }
        }

        /// <summary>
        /// Processes all unparsed text blocks read from the original template. The method current implementation
        /// trims all white-spaces and new line characters and replaces them with a single white-space character (emulates how HTML trimming works).
        /// <remarks>Descendant classes can override this method and modify this behaviour.</remarks>
        /// </summary>
        /// <param name="context">The <see cref="IExpressionEvaluationContext" /> instance containing the current evaluation state.</param>
        /// <param name="unparsedText">The text block being processed.</param>
        /// <returns>
        /// The processed text value.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="context" /> is <c>null</c>.</exception>
        public virtual String DecorateUnparsedText(IExpressionEvaluationContext context, String unparsedText)
        {
            Expect.NotNull("context", context);

            if (String.IsNullOrEmpty(unparsedText))
                return String.Empty;
            else
            {
                /* Trim all 1+ white spaces to one space character. */
                StringBuilder result = new StringBuilder();
                bool putSpace = false;
                foreach (var c in unparsedText)
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
                    result.Length -= 1;

                return result.ToString();
            }
        }

        /// <summary>
        /// Override in descendant classes to specify the tag start character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The tag start character.
        /// </value>
        public abstract char StartTagCharacter { get; }

        /// <summary>
        /// Override in descendant classes to specify the tag end character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The tag end character.
        /// </value>
        public abstract char EndTagCharacter { get; }

        /// <summary>
        /// Override in descendant classes to specify the string literal start character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal start character.
        /// </value>
        public abstract char StartStringLiteralCharacter { get; }

        /// <summary>
        /// Override in descendant classes to specify the string literal end character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal end character.
        /// </value>
        public abstract char EndStringLiteralCharacter { get; }

        /// <summary>
        /// Override in descendant classes to specify the string literal escape character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal escape character.
        /// </value>
        public abstract char StringLiteralEscapeCharacter { get; }

        /// <summary>
        /// Override in descendant classes to specify the number literal decimal separator character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The number literal decimal separator character.
        /// </value>
        public abstract char NumberDecimalSeparatorCharacter { get; }

        /// <summary>
        /// Returns a human-readable representation of the dialect instance.
        /// </summary>
        /// <returns>
        /// A string that represents the current object.
        /// </returns>
        public override String ToString()
        {
            String caseDescr = null;
            switch (m_casing)
            {
                case DialectCasing.IgnoreCase:
                    caseDescr = "Ignore Case";
                    break;
                case DialectCasing.LowerCase:
                    caseDescr = "Lower Case";
                    break;
                case DialectCasing.UpperCase:
                    caseDescr = "Upper Case";
                    break;
            }

            if (String.IsNullOrEmpty(Culture.Name))
                return String.Format("{0} ({1})", Name, caseDescr);
            else
                return String.Format("{0} ({1}, {2})", Name, Culture.Name, caseDescr);
        }

        /// <summary>
        /// Determines whether the specified <see cref="Object" /> is equal to the current <see cref="StandardDialectBase" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current dialect class instance.</param>
        /// <returns>
        /// <c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(Object obj)
        {
            var other = obj as StandardDialect;
            return
                other != null &&
                other.Name.Equals(Name) &&
                other.m_casing.Equals(m_casing) &&
                other.Culture.Equals(Culture);
        }

        /// <summary>
        /// Calculates the hash for this dialect class instance.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="StandardDialectBase" />.
        /// </returns>
        public override Int32 GetHashCode()
        {
            return
                Name.GetHashCode() ^
                m_casing.GetHashCode() ^
                Culture.GetHashCode();
        }

        /// <summary>
        /// Gets the string representation of an <see cref="Object" /> using the given <paramref name="formatProvider"/>.
        /// </summary>
        /// <param name="obj">The object to obtain the string representation for.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// The string representation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="formatProvider"/> is <c>null</c>.</exception>
        String IObjectFormatter.ToString(Object obj, IFormatProvider formatProvider)
        {
            String result;

            if (obj == null)
                return m_undefinedSpecialIdentifier;
            else if (!m_specialsIdentifiers.TryGetValue(obj, out result))
            {
                if (obj is String)
                    result = (String)obj;
                else if (obj is IFormattable)
                    result = (obj as IFormattable).ToString(null, formatProvider);
                else
                    result = obj.ToString();
            }

            return result;
        }

        /// <summary>
        /// Gets the string representation of an <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The object to obtain the string representation for.</param>
        /// <returns>The string representation.</returns>
        String IObjectFormatter.ToString(Object obj)
        {
            return ((IObjectFormatter)this).ToString(obj, Culture);
        }
    }
}
