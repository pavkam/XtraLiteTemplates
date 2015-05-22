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

    public class StandardDialect : StandardDialectBase
    {
        public static IDialect DefaultIgnoreCase { get; private set; }
        public static IDialect Default { get; private set; }

        static StandardDialect()
        {
            DefaultIgnoreCase = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.IgnoreCase);
            Default = new StandardDialect(CultureInfo.InvariantCulture, DialectCasing.UpperCase);
        }

        protected Object PreformattedStateObject { get; private set; }

        protected override Operator[] CreateOperators(IPrimitiveTypeConverter typeConverter)
        {
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
                new MemberAccessOperator(IdentifierComparer),
                new IntegerRangeOperator(typeConverter),
                new ValueFormatOperator(Culture, typeConverter),
            };
        }

        protected override Directive[] CreateDirectives(IPrimitiveTypeConverter typeConverter)
        {
            return new Directive[]
            {
                new ConditionalInterpolationDirective(AdjustCasing("$ IF $"), false, typeConverter),
                new ForEachDirective(AdjustCasing("FOR EACH ? IN $"), AdjustCasing("END"), typeConverter),
                new IfDirective(AdjustCasing("IF $ THEN"), AdjustCasing("END"), typeConverter),
                new IfElseDirective(AdjustCasing("IF $ THEN"), AdjustCasing("ELSE"), AdjustCasing("END"), typeConverter),
                new InterpolationDirective(typeConverter),
                new RepeatDirective(AdjustCasing("REPEAT $ TIMES"), AdjustCasing("END"), typeConverter),
                new PreFormattedUnparsedTextDirective(AdjustCasing("PREFORMATTED"), AdjustCasing("END"), PreformattedStateObject, typeConverter),
            };
        }

        protected override KeyValuePair<String, Object>[] CreateSpecials()
        {
            return new KeyValuePair<String, Object>[]
            {
                new KeyValuePair<String, Object>( AdjustCasing("TRUE"), true ),
                new KeyValuePair<String, Object>( AdjustCasing("FALSE"), false ),
                new KeyValuePair<String, Object>( AdjustCasing("UNDEFINED"), null ),
                new KeyValuePair<String, Object>( AdjustCasing("NAN"), Double.NaN ),
                new KeyValuePair<String, Object>( AdjustCasing("INFINITY"), Double.PositiveInfinity ),
            };
        }


        protected StandardDialect(String name, CultureInfo culture, DialectCasing casing)
            : base(name, culture, casing)
        {
            PreformattedStateObject = new Object();
        }

        public StandardDialect(CultureInfo culture, DialectCasing casing)
            : this("Standard", culture, casing)
        {
        }

        public StandardDialect()
            : this(CultureInfo.InvariantCulture, DialectCasing.IgnoreCase)
        {
        }


        public override String DecorateUnparsedText(IExpressionEvaluationContext context, String unparsedText)
        {
            Expect.NotNull("context", context);

            if (PreformattedStateObject != null && context.ContainsStateObject(PreformattedStateObject))
                return unparsedText;
            else
                return base.DecorateUnparsedText(context, unparsedText);
        }

        public override Char StartTagCharacter
        {
            get
            {
                return '{';
            }
        }

        public override Char EndTagCharacter
        {
            get 
            {
                return '}';
            }
        }

        public override Char StartStringLiteralCharacter
        {
            get
            {
                return '"';
            }
        }

        public override Char EndStringLiteralCharacter
        {
            get
            {
                return '"';
            }
        }

        public override Char StringLiteralEscapeCharacter
        {
            get
            {
                return '\\';
            }
        }

        public override Char NumberDecimalSeparatorCharacter
        {
            get
            {
                if (Culture.NumberFormat.NumberDecimalSeparator.Length != 1)
                    return '.';
                else
                    return Culture.NumberFormat.NumberDecimalSeparator[0];
            }
        }

        public override Boolean Equals(Object obj)
        {
            return base.Equals(obj as StandardDialect);
        }

        public override Int32 GetHashCode()
        {
            return base.GetHashCode() ^ GetType().GetHashCode();
        }
    }
}
