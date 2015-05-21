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

    public class CodeMonkeyDialect : StandardDialect
    {
        public static new IDialect DefaultIgnoreCase { get; private set; }
        public static new IDialect Default { get; private set; }

        static CodeMonkeyDialect()
        {
            DefaultIgnoreCase = new CodeMonkeyDialect(DialectCasing.IgnoreCase);
            Default = new CodeMonkeyDialect(DialectCasing.UpperCase);
        }

        protected override Directive[] CreateDirectives(IPrimitiveTypeConverter typeConverter)
        {
            return new Directive[]
            {
                new ConditionalInterpolationDirective(AdjustCasing("$ IF $"), false, typeConverter),
                new ForEachDirective(AdjustCasing("FOR ? IN $"), AdjustCasing("END"), typeConverter),
                new ForDirective(AdjustCasing("FOR $"), AdjustCasing("END"), typeConverter),
                new IfDirective(AdjustCasing("IF $"), AdjustCasing("END"), typeConverter),
                new IfElseDirective(AdjustCasing("IF $"), AdjustCasing("ELSE"), AdjustCasing("END"), typeConverter),
                new InterpolationDirective(typeConverter),
                new PreFormattedUnparsedTextDirective(AdjustCasing("PRE"), AdjustCasing("END"), PreformattedStateObject, typeConverter),
            };
        }

        public CodeMonkeyDialect(DialectCasing casing)
            : base("Code Monkey", CultureInfo.InvariantCulture, casing)
        {
        }

        public CodeMonkeyDialect()
            : this(DialectCasing.IgnoreCase)
        {
        }


        public override Char StartStringLiteralCharacter
        {
            get
            {
                return '\'';
            }
        }

        public override Char EndStringLiteralCharacter
        {
            get
            {
                return '\'';
            }
        }


        public override Boolean Equals(Object obj)
        {
            return base.Equals(obj as CodeMonkeyDialect);
        }

        public override Int32 GetHashCode()
        {
            return base.GetHashCode() ^ GetType().GetHashCode();
        }
    }
}
