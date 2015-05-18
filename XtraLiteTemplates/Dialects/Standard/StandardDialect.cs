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

    public class StandardDialect : IDialect
    {
        public static IDialect InvariantCultureIgnoreCase { get; private set; }
        public static IDialect InvariantCultureUpperCase { get; private set; }
        public static IDialect InvariantCultureLowerCase { get; private set; }

        static StandardDialect()
        {
            InvariantCultureIgnoreCase = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.IgnoreCase);
            InvariantCultureUpperCase = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.UpperCase);
            InvariantCultureLowerCase = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.LowerCase);
        }

        private IPrimitiveTypeConverter m_typeConverter;

        public CultureInfo Culture { get; private set;  }
        public IEqualityComparer<String> IdentifierComparer { get; private set; }
        public IComparer<String> StringLiteralComparer { get; private set; }

        public IReadOnlyCollection<Operator> Operators { get; private set; }
        public IReadOnlyCollection<Directive> Directives { get; private set; }
        public IReadOnlyDictionary<String, Object> SpecialKeywords { get; private set; }

        public StandardDialect(CultureInfo culture, DialectCasing casing)
        {
            Expect.NotNull("culture", culture);

            /* Build culture-aware values.*/
            Culture = culture;
            m_typeConverter = new FlexiblePrimitiveTypeConverter(Culture);
            
            var comparer = StringComparer.Create(culture, 
                casing == DialectCasing.IgnoreCase);

            IdentifierComparer = comparer;
            StringLiteralComparer = comparer;

            Func<String, String> caseModifierFunc = input => input;
            if (casing == DialectCasing.LowerCase)
                caseModifierFunc = input => input.ToLower(culture);

            /* Create all operators */
            Operators = new List<Operator>()
            {
                new RelationalEqualsOperator(StringLiteralComparer, m_typeConverter),
                new RelationalNotEqualsOperator(StringLiteralComparer, m_typeConverter),
                new RelationalGreaterThanOperator(StringLiteralComparer, m_typeConverter),
                new RelationalGreaterThanOrEqualsOperator(StringLiteralComparer, m_typeConverter),
                new RelationalLowerThanOperator(StringLiteralComparer, m_typeConverter),
                new RelationalLowerThanOrEqualsOperator(StringLiteralComparer, m_typeConverter),
                new LogicalAndOperator(m_typeConverter),
                new LogicalOrOperator(m_typeConverter),
                new LogicalNotOperator(m_typeConverter),
                new BitwiseAndOperator(m_typeConverter),
                new BitwiseOrOperator(m_typeConverter),
                new BitwiseXorOperator(m_typeConverter),
                new BitwiseNotOperator(m_typeConverter),
                new BitwiseShiftLeftOperator(m_typeConverter),
                new BitwiseShiftRightOperator(m_typeConverter),
                new ArithmeticDivideOperator(m_typeConverter),
                new ArithmeticModuloOperator(m_typeConverter),
                new ArithmeticMultiplyOperator(m_typeConverter),
                new ArithmeticNegateOperator(m_typeConverter),
                new ArithmeticNeutralOperator(m_typeConverter),
                new ArithmeticSubtractOperator(m_typeConverter),
                new ArithmeticSumOperator(m_typeConverter),
                new MemberAccessOperator(IdentifierComparer),
                new IntegerRangeOperator(m_typeConverter),
                new SubscriptOperator(),
            };

            /* Create all directives. */
            Directives = new List<Directive>()
            {
                new ConditionalInterpolationDirective(caseModifierFunc("$ IF $"), false, m_typeConverter),
                new ForEachDirective(caseModifierFunc("FOR ? IN $"), caseModifierFunc("END"), m_typeConverter),
                new IfDirective(caseModifierFunc("IF $"), caseModifierFunc("END"), m_typeConverter),
                new IfElseDirective(caseModifierFunc("IF $"), caseModifierFunc("ELSE"), caseModifierFunc("END"), m_typeConverter),
                new InterpolationDirective(m_typeConverter),
                new RepeatDirective(caseModifierFunc("REPEAT $"), caseModifierFunc("END"), m_typeConverter),
            };

            /* Special keywords. */
            SpecialKeywords = new Dictionary<String, Object>()
            {
                { caseModifierFunc("TRUE"), true },
                { caseModifierFunc("FALSE"), false },
                { caseModifierFunc("UNDEFINED"), null },
                { caseModifierFunc("NAN"), Double.NaN },
                { caseModifierFunc("INFINITY"), Double.PositiveInfinity },
            };
        }

        public StandardDialect()
            : this(CultureInfo.InvariantCulture, DialectCasing.IgnoreCase)
        {
        }

        public virtual String DecorateUnparsedText(String unparsedText)
        {
            if (String.IsNullOrEmpty(unparsedText))
                return String.Empty;
            else
                return unparsedText.Trim();
        }


        public virtual Char StartTagCharacter
        {
            get
            {
                return '{';
            }
        }

        public virtual Char EndTagCharacter
        {
            get 
            {
                return '}';
            }
        }

        public virtual Char StartStringLiteralCharacter
        {
            get
            {
                return '"';
            }
        }

        public virtual Char EndStringLiteralCharacter
        {
            get
            {
                return '"';
            }
        }

        public virtual Char StringLiteralEscapeCharacter
        {
            get
            {
                return '\\';
            }
        }

        public virtual Char NumberDecimalSeparatorCharacter
        {
            get
            {
                if (Culture.NumberFormat.NumberDecimalSeparator.Length != 1)
                    return '.';
                else
                    return Culture.NumberFormat.NumberDecimalSeparator[0];
            }
        }
    }
}
