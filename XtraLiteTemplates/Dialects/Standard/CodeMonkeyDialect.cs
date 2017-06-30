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

using System.Collections.Generic;

namespace XtraLiteTemplates.Dialects.Standard
{
    using System;
    using System.Globalization;
    using Directives;
    using Evaluation;
    using Introspection;
    using JetBrains.Annotations;

    /// <summary>
    /// A minimalistic, programmer-oriented standard dialect. Contains the full set of supported expression operators, directives and special constants.
    /// See <seealso cref="StandardDialect" /> for a medium verbose dialect.
    /// </summary>
    [PublicAPI]
    public class CodeMonkeyDialect : StandardDialect
    {
        /// <summary>
        /// Initializes static members of the <see cref="CodeMonkeyDialect"/> class.
        /// </summary>
        static CodeMonkeyDialect()
        {
            DefaultIgnoreCase = new CodeMonkeyDialect(DialectCasing.IgnoreCase);
            Default = new CodeMonkeyDialect(DialectCasing.UpperCase);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeMonkeyDialect"/> class.
        /// </summary>
        /// <param name="casing">A <see cref="DialectCasing" /> value that controls the dialect string casing behavior.</param>
        public CodeMonkeyDialect(DialectCasing casing)
            : base("Code Monkey", CultureInfo.InvariantCulture, casing)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CodeMonkeyDialect" /> class. The instance is case-insensitive.
        /// </summary>
        public CodeMonkeyDialect()
            : this(DialectCasing.IgnoreCase)
        {
        }

        /// <summary>
        /// Gets a culture-invariant, case-insensitive instance of <see cref="CodeMonkeyDialect"/> class.
        /// </summary>
        /// <value>
        /// The culture-invariant, case-insensitive instance of <see cref="CodeMonkeyDialect"/> class.
        /// </value>
        [NotNull]
        public static new IDialect DefaultIgnoreCase { get; }

        /// <summary>
        /// Gets a culture-invariant, case-sensitive (upper cased) instance of <see cref="StandardDialect"/> class.
        /// </summary>
        /// <value>
        /// The culture-invariant, case-sensitive instance of <see cref="StandardDialect"/> class.
        /// </value>
        [NotNull]
        public static new IDialect Default { get; }

        /// <summary>
        /// Specifies the string literal start character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal start character.
        /// </value>
        public override char StartStringLiteralCharacter => '\'';

        /// <summary>
        /// Specifies the string literal end character (used by the tokenization process).
        /// </summary>
        /// <value>
        /// The string literal end character.
        /// </value>
        public override char EndStringLiteralCharacter => '\'';

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to the current <see cref="CodeMonkeyDialect" />.
        /// </summary>
        /// <param name="obj">The object to compare with the current dialect class instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified object is equal to the current object; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return base.Equals(obj as CodeMonkeyDialect);
        }

        /// <summary>
        /// Calculates the hash for this dialect class instance.
        /// </summary>
        /// <returns>
        /// A hash code for the current <see cref="CodeMonkeyDialect" />.
        /// </returns>
        public override int GetHashCode()
        {
            return base.GetHashCode() ^ GetType().GetHashCode();
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
            Expect.NotNull(nameof(typeConverter), typeConverter);

            return new Directive[]
            {
                new ConditionalInterpolationDirective(AdjustCasing("$ IF $"), false, typeConverter),
                new UsingDirective(AdjustCasing("? AS $"), AdjustCasing("END"), typeConverter),
                new ForEachDirective(AdjustCasing("FOR ? IN $"), AdjustCasing("END"), typeConverter),
                new SeparatedForEachDirective(AdjustCasing("FOR ? IN $"), AdjustCasing("WITH"), AdjustCasing("END"), typeConverter),
                new ForDirective(AdjustCasing("FOR $"), AdjustCasing("END"), typeConverter),
                new IfDirective(AdjustCasing("IF $"), AdjustCasing("END"), typeConverter),
                new IfElseDirective(AdjustCasing("IF $"), AdjustCasing("ELSE"), AdjustCasing("END"), typeConverter),
                new InterpolationDirective(typeConverter),
                new PreFormattedUnParsedTextDirective(AdjustCasing("PRE"), AdjustCasing("END"), PreformattedStateObject, typeConverter),
            };
        }

        /// <summary>
        /// Override in descendant classes to supply an instance of the <see cref="StandardDialectBase.Self" /> object.
        /// </summary>
        /// <remarks>
        /// This implementation creates an instance of <see cref="CodeMonkeySelfObject"/>.
        /// </remarks>
        /// <param name="typeConverter">The concrete <see cref="IPrimitiveTypeConverter" /> implementation used for type conversions.</param>
        /// <returns>
        /// An instance of the self object.
        /// </returns>
        protected override StandardSelfObject CreateSelfObject(IPrimitiveTypeConverter typeConverter)
        {
            Expect.NotNull(nameof(typeConverter), typeConverter);

            return new CodeMonkeySelfObject(typeConverter);
        }
    }
}