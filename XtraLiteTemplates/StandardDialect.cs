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

namespace XtraLiteTemplates
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;
    using XtraLiteTemplates.Evaluation.Directives;
    using XtraLiteTemplates.Evaluation.Directives.Standard;
    using XtraLiteTemplates.Expressions;
    using XtraLiteTemplates.Expressions.Operators;
    using XtraLiteTemplates.Expressions.Operators.Standard;

    public class StandardDialect : IDialect
    {
        public static StandardDialect CurrentCulture { get; private set; }
        public static StandardDialect CurrentCultureIgnoreCase { get; private set; }
        public static StandardDialect InvariantCulture { get; private set; }
        public static StandardDialect InvariantCultureIgnoreCase { get; private set; }
        public static StandardDialect Ordinal { get; private set; }
        public static StandardDialect OrdinalIgnoreCase { get; private set; }

        static StandardDialect()
        {
            CurrentCulture = new StandardDialect(CultureInfo.CurrentCulture, false);
            CurrentCultureIgnoreCase = new StandardDialect(CultureInfo.CurrentCulture, true);

            InvariantCulture = new StandardDialect(CultureInfo.InvariantCulture, false);
            InvariantCultureIgnoreCase = new StandardDialect(CultureInfo.InvariantCulture, true);

            Ordinal = new StandardDialect(CultureInfo.InvariantCulture, new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture),
                StringComparer.Create(CultureInfo.InvariantCulture, false), StringComparer.Create(CultureInfo.InvariantCulture, false));
            OrdinalIgnoreCase = new StandardDialect(CultureInfo.InvariantCulture, new FlexiblePrimitiveTypeConverter(CultureInfo.InvariantCulture),
                StringComparer.Create(CultureInfo.InvariantCulture, true), StringComparer.Create(CultureInfo.InvariantCulture, false));
        }

        public CultureInfo Culture { get; private set;  }
        public IEqualityComparer<String> IdentifierComparer { get; private set; }
        public IComparer<String> StringLiteralComparer { get; private set; }
        public IReadOnlyCollection<Operator> Operators { get; private set; }
        public IReadOnlyCollection<Directive> Directives { get; private set; }
        public IPrimitiveTypeConverter TypeConverter { get; private set; }
        public IReadOnlyDictionary<String, Object> SpecialKeywords { get; private set; }

        private Object[] m_emptyParameters = new Object[0];
        private Type[] m_emptyTypeParameters = new Type[0];

        public StandardDialect(CultureInfo culture, IPrimitiveTypeConverter typeConverter,
            IEqualityComparer<String> identifierComparer, IComparer<String> stringLiteralComparer)
        {
            Expect.NotNull("culture", culture);
            Expect.NotNull("typeConverter", typeConverter);
            Expect.NotNull("identifierComparer", identifierComparer);
            Expect.NotNull("stringLiteralComparer", stringLiteralComparer);

            Culture = culture;
            TypeConverter = typeConverter;
            IdentifierComparer = identifierComparer;
            StringLiteralComparer = stringLiteralComparer;

            /* Create all operators */
            var operators = new List<Operator>()
            {
                CreateOperator<RelationalEqualsOperator>(),
                CreateOperator<RelationalNotEqualsOperator>(),
                CreateOperator<RelationalGreaterThanOperator>(),
                CreateOperator<RelationalGreaterThanOrEqualsOperator>(),
                CreateOperator<RelationalLowerThanOperator>(),
                CreateOperator<RelationalLowerThanOrEqualsOperator>(),
                CreateOperator<LogicalAndOperator>(),
                CreateOperator<LogicalOrOperator>(),
                CreateOperator<LogicalNotOperator>(),
                CreateOperator<BitwiseAndOperator>(),
                CreateOperator<BitwiseOrOperator>(),
                CreateOperator<BitwiseXorOperator>(),
                CreateOperator<BitwiseNotOperator>(),
                CreateOperator<BitwiseShiftLeftOperator>(),
                CreateOperator<BitwiseShiftRightOperator>(),
                CreateOperator<ArithmeticDivideOperator>(),
                CreateOperator<ArithmeticModuloOperator>(),
                CreateOperator<ArithmeticMultiplyOperator>(),
                CreateOperator<ArithmeticNegateOperator>(),
                CreateOperator<ArithmeticNeutralOperator>(),
                CreateOperator<ArithmeticSubtractOperator>(),
                CreateOperator<ArithmeticSumOperator>(),
                CreateOperator<MemberAccessOperator>(),
                CreateOperator<IntegerRangeOperator>(),
                CreateOperator<SubscriptOperator>(),
            };

            Operators = operators.Where(p => p != null).ToList();

            /* Create all directives. */
            var directives = new List<Directive>()
            {
                CreateDirective<ConditionalInterpolationDirective>(),
                CreateDirective<ForEachDirective>(),
                CreateDirective<IfDirective>(),
                CreateDirective<IfElseDirective>(),
                CreateDirective<InterpolationDirective>(),
                CreateDirective<RepeatDirective>(),
            };

            Directives = directives.Where(p => p != null).ToList();

            /* Special keywords. */
            SpecialKeywords = new Dictionary<String, Object>()
            {
                { "true", true },
                { "false", false },
                { "undefined", null },
                { "NaN", Double.NaN },
                { "Infinity", Double.PositiveInfinity },
                { "PositiveInfinity", Double.PositiveInfinity },
                { "NegativeInfinity", Double.NegativeInfinity },
            };
        }

        public StandardDialect(CultureInfo culture, IEqualityComparer<String> comparer) :
            this(culture, new FlexiblePrimitiveTypeConverter(culture), comparer, StringComparer.Create(culture, false))
        {
        }

        public StandardDialect(CultureInfo culture, Boolean ignoreCase) :
            this(culture, new FlexiblePrimitiveTypeConverter(culture), 
                StringComparer.Create(culture, ignoreCase), 
                StringComparer.Create(culture, false))
        {
        }

        private Operator CreateOperator<T>() where T : Operator
        {
            return CreateOperator(typeof(T));
        }

        private Object ConstructStandardEntity(Type typeOfEntity)
        {
            /* Get all public constructors, prefer the ones with the most parameters. */
            List<Object> callingParams = new List<Object>();
            foreach (var constructor in typeOfEntity.GetConstructors().
                OrderByDescending(c => c.GetParameters().Length))
            {
                if (!constructor.IsPublic)
                    continue;

                callingParams.Clear();
                Boolean perfectConstructor = true;
                foreach(var param in constructor.GetParameters())
                {
                    if (param.ParameterType == typeof(IPrimitiveTypeConverter))
                        callingParams.Add(TypeConverter);
                    else if (param.ParameterType == typeof(IEqualityComparer<String>))
                        callingParams.Add(IdentifierComparer);
                    else if (param.ParameterType == typeof(IComparer<String>))
                        callingParams.Add(StringLiteralComparer);
                    else if (param.ParameterType == typeof(IFormatProvider))
                        callingParams.Add((IFormatProvider)Culture);
                    else if (param.ParameterType == typeof(CultureInfo))
                        callingParams.Add(Culture);
                    else
                    {
                        perfectConstructor = false;
                        break;
                    }
                }

                if (perfectConstructor)
                    return constructor.Invoke(callingParams.ToArray());
            }

            return null;
        }

        protected virtual Operator CreateOperator(Type operatorType)
        {
            Debug.Assert(operatorType != null);
            Debug.Assert(operatorType.IsSubclassOf(typeof(Operator)));

            return (Operator)ConstructStandardEntity(operatorType);
        }

        private Directive CreateDirective<T>() where T : Directive
        {
            return CreateDirective(typeof(T));
        }

        protected virtual Directive CreateDirective(Type directiveType)
        {
            Debug.Assert(directiveType != null);
            Debug.Assert(directiveType.IsSubclassOf(typeof(Directive)));

            return (Directive)ConstructStandardEntity(directiveType);
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
