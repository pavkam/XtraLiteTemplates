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
using NUnit.Framework;

namespace XtraLiteTemplates.NUnit
{
    using System;
    using System.Linq;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using XtraLiteTemplates.Evaluation;
    using System.Threading;

    [TestFixture]
    public class EvaluationContextTests : TestBase
    {
        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_UsingNullIdentifierComparer_RaisesException()
        {
            new EvaluationContext(false, CancellationToken.None, null, ObjectFormatter, this, (context, text) => text);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_UsingNullObjectFomatter_RaisesException()
        {
            new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCulture, null, this, (context, text) => text);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_UsingNullUnparsedTextHandler_RaisesException()
        {
            new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCulture, ObjectFormatter, this, null);
        }

        [Test]
        public void Constructor_PassingTrueForIgnoreEvaluationExceptions_StoresTheValue()
        {
            var evaluationContext = new EvaluationContext(true, CancellationToken.None, StringComparer.InvariantCulture, ObjectFormatter, this, (context, text) => text);
            Assert.AreEqual(true, evaluationContext.IgnoreEvaluationExceptions);
        }

        [Test]
        public void Constructor_PassingFalseForIgnoreEvaluationExceptions_StoresTheValue()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCulture, ObjectFormatter, this, (context, text) => text);
            Assert.AreEqual(false, evaluationContext.IgnoreEvaluationExceptions);
        }

        [Test]
        public void Constructor_ForCancellationToken_StoresTheValue()
        {
            CancellationTokenSource source = new CancellationTokenSource();
            var evaluationContext = new EvaluationContext(false, source.Token, StringComparer.InvariantCulture, ObjectFormatter, this, (context, text) => text);

            Assert.AreEqual(source.Token, evaluationContext.CancellationToken);
        }

        [Test]
        public void Constructor_ForInvariantCultureStringComparer_StoresTheValue()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCulture, ObjectFormatter, this, (context, text) => text);
            evaluationContext.SetProperty("caseMatters", 1);

            Assert.IsNull(evaluationContext.GetProperty("Casematters"));
        }

        [Test]
        public void Constructor_ForSelfObject_StoresTheValue()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCultureIgnoreCase, ObjectFormatter, this, (context, text) => text);

            Assert.AreEqual(GetType(), evaluationContext.GetProperty("GetType"));
        }

        [Test]
        public void Constructor_ForUnparsedTextHandler_StoresTheValue()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCultureIgnoreCase, ObjectFormatter, this, (context, text) => text.ToUpper());

            Assert.AreEqual("LOWERCASE", evaluationContext.ProcessUnparsedText("lowercase"));
        }

        [Test]
        public void ProcessUnparsedText_UsingNullValue_ReturnsNull()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCultureIgnoreCase,
                ObjectFormatter, this, (context, text) => "EXECUTED");

            Assert.IsNull(evaluationContext.ProcessUnparsedText(null));
        }

        [Test]
        public void ProcessUnparsedText_UsingNonNullText_ExecutesDelegate()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCultureIgnoreCase,
                ObjectFormatter, this, (context, text) =>
                {
                    return "EXECUTED";
                });

            Assert.AreEqual("EXECUTED", evaluationContext.ProcessUnparsedText("text"));
        }
    }
}

