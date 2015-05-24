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
    /// Abstract base class for all standard dialects supported by this library. Defines a set of common properties and behaviours that concrete
    /// dialect implementations can use out-of-the box.
    /// </summary>
    public abstract class StandardDialectBase : IDialect
    {
        private IPrimitiveTypeConverter m_typeConverter;
        private DialectCasing m_casing;
        private IReadOnlyList<Operator> m_operators;
        private IReadOnlyList<Directive> m_directives;
        private IReadOnlyDictionary<String, Object> m_specials;

        protected abstract Operator[] CreateOperators(IPrimitiveTypeConverter typeConverter);
        protected abstract Directive[] CreateDirectives(IPrimitiveTypeConverter typeConverter);
        protected abstract KeyValuePair<String, Object>[] CreateSpecials();

        protected String AdjustCasing(String markup)
        {
            if (String.IsNullOrEmpty(markup))
                return markup;

            if (m_casing == DialectCasing.LowerCase)
                return markup.ToLower(Culture);
            else if (m_casing == DialectCasing.UpperCase)
                return markup.ToUpper(Culture);
            else
                return markup;
        }

        protected StandardDialectBase(String name, CultureInfo culture, DialectCasing casing)
        {
            Expect.NotEmpty("name", name);
            Expect.NotNull("culture", culture);

            /* Build culture-aware values.*/
            Name = name;
            Culture = culture;
            m_typeConverter = new FlexiblePrimitiveTypeConverter(Culture);
            m_casing = casing;

            var comparer = StringComparer.Create(culture, casing == DialectCasing.IgnoreCase);

            IdentifierComparer = comparer;
            StringLiteralComparer = comparer;
        }

        public CultureInfo Culture { get; private set;  }
        public IEqualityComparer<String> IdentifierComparer { get; private set; }
        public IComparer<String> StringLiteralComparer { get; private set; }
        public String Name { get; private set; }

        public ExpressionFlowSymbols FlowSymbols
        {
            get 
            {
                return ExpressionFlowSymbols.Default;
            }
        }

        public IReadOnlyCollection<Operator> Operators
        { 
            get
            {
                if (m_operators == null)
                    m_operators = new List<Operator>(CreateOperators(m_typeConverter));

                return m_operators;
            }
        }

        public IReadOnlyCollection<Directive> Directives
        { 
            get
            {
                if (m_directives == null)
                    m_directives = new List<Directive>(CreateDirectives(m_typeConverter));

                return m_directives;
            }
        }

        public IReadOnlyDictionary<String, Object> SpecialKeywords
        {
            get
            {
                if (m_specials == null)
                {
                    var specials = new Dictionary<String, Object>(IdentifierComparer);
                    foreach (var kvp in CreateSpecials())
                        specials.Add(kvp.Key, kvp.Value);

                    m_specials = specials;
                }

                return m_specials;
            }
        }


        public virtual String DecorateUnparsedText(IExpressionEvaluationContext context, String unparsedText)
        {
            Expect.NotNull("context", context);

            if (String.IsNullOrEmpty(unparsedText))
                return String.Empty;
            else
            {
                /* Trim all 1+ white spaces to one space character. */
                StringBuilder result = new StringBuilder();
                Boolean putSpace = false;
                foreach (var c in unparsedText)
                {
                    if (Char.IsWhiteSpace(c))
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

                if (result.Length > 0 && Char.IsWhiteSpace(result[result.Length - 1]))
                    result.Length -= 1;

                return result.ToString();
            }
        }

        public abstract Char StartTagCharacter { get; }

        public abstract Char EndTagCharacter { get; }

        public abstract Char StartStringLiteralCharacter { get; }

        public abstract Char EndStringLiteralCharacter { get; }

        public abstract Char StringLiteralEscapeCharacter { get; }

        public abstract Char NumberDecimalSeparatorCharacter { get; }

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

        public override Boolean Equals(Object obj)
        {
            var other = obj as StandardDialect;
            return
                other != null &&
                other.Name.Equals(Name) &&
                other.m_casing.Equals(m_casing) &&
                other.Culture.Equals(Culture);
        }

        public override Int32 GetHashCode()
        {
            return
                Name.GetHashCode() ^
                m_casing.GetHashCode() ^
                Culture.GetHashCode();
        }
    }
}
