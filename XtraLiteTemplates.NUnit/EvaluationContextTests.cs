//
//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex+git@ciobanu.org)
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
        private EvaluationContext _evaluationContext;
        private EvaluationContext _evaluationContextIgnoreCase;

        [SetUp]
        public void SetUp()
        {
            _evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCulture, ObjectFormatter, this, (context, text) => text);
            _evaluationContextIgnoreCase = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCultureIgnoreCase, ObjectFormatter, this, (context, text) => text);
        }

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
            _evaluationContext.SetProperty("caseMatters", 1);

            Assert.IsNull(_evaluationContext.GetProperty("Casematters"));
        }

        [Test]
        public void Constructor_ForSelfObject_StoresTheValue()
        {
            Assert.AreEqual(GetType(), _evaluationContext.GetProperty("GetType"));
        }

        [Test]
        public void Constructor_ForUnparsedTextHandler_StoresTheValue()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCultureIgnoreCase, ObjectFormatter, this, (context, text) => text.ToUpper());

            Assert.AreEqual("LOWERCASE", evaluationContext.ProcessUnparsedText("lowercase"));
        }

        [Test]
        public void ProcessUnparsedText_UsingNullValue_ExecutesDelegate()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCultureIgnoreCase,
                ObjectFormatter, this, (context, text) =>
           {
               Assert.IsNull(text);
               return null;
           });
        }

        [Test]
        public void ProcessUnparsedText_UsingNonNullText_ExecutesDelegate()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCultureIgnoreCase,
                ObjectFormatter, this, (context, text) =>
                {
                    Assert.AreEqual("text", text);
                    return "EXECUTED";
                });

            Assert.AreEqual("EXECUTED", evaluationContext.ProcessUnparsedText("text"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetProperty_UsingNullPropertyName_RaisesException()
        {
            _evaluationContext.SetProperty(null, 1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SetProperty_UsingInvalidPropertyName_RaisesException()
        {
            _evaluationContext.SetProperty("123", 1);
        }

        [Test]
        public void SetProperty_ForCorrectPropertyName_BehavesAsExpected()
        {
            _evaluationContext.SetProperty("property", 1);

            Assert.AreEqual(1, _evaluationContext.GetProperty("property"));
        }

        [Test]
        public void SetProperty_ForCaseInsensitiveContext_BehavesAsExpected()
        {
            _evaluationContextIgnoreCase.SetProperty("property", 1);
            _evaluationContextIgnoreCase.SetProperty("PROPERTY", 2);

            Assert.AreEqual(2, _evaluationContextIgnoreCase.GetProperty("Property"));
        }

        [Test]
        public void SetProperty_ForPropertyPartOfSelfObject_OverridesIt()
        {
            _evaluationContext.SetProperty("ToString", "Override!");

            Assert.AreEqual("Override!", _evaluationContext.GetProperty("ToString"));
        }

        [Test]
        public void SetProperty_ForCaseInsensitiveContextAndPropertySimilarWithOneInSelfObject_OverridesIt()
        {
            _evaluationContextIgnoreCase.SetProperty("TOSTRING", "Override!");

            Assert.AreEqual("Override!", _evaluationContextIgnoreCase.GetProperty("ToString"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetProperty_UsingNullPropertyName_RaisesException()
        {
            _evaluationContext.GetProperty(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetProperty_UsingInvalidPropertyName_RaisesException()
        {
            _evaluationContext.GetProperty("123");
        }

        [Test]
        public void GetProperty_ForCaseInsensitiveContext_IgnoresCase()
        {
            _evaluationContextIgnoreCase.SetProperty("property", 1);

            Assert.AreEqual(1, _evaluationContextIgnoreCase.GetProperty("PROPERTY"));
        }

        [Test]
        public void GetProperty_ForPropertyPartOfSelfObject_PassesTheControlToSelfObject()
        {
            Assert.AreEqual(ToString(), _evaluationContext.GetProperty("ToString"));
        }

        [Test]
        public void GetProperty_ForUnknownProperty_ReturnsNull()
        {
            Assert.IsNull(_evaluationContext.GetProperty("any_property"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetProperty_WithObject_UsingNullPropertyName_RaisesException()
        {
            _evaluationContext.GetProperty(this, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetProperty_WithObject_UsingInvalidPropertyName_RaisesException()
        {
            _evaluationContext.GetProperty(this, "123");
        }

        [Test]
        public void GetProperty_WithObject_ForCaseInsensitiveContext_IgnoresCase()
        {
            Assert.AreEqual(ToString(), _evaluationContextIgnoreCase.GetProperty(this, "TOSTRING"));
        }

        [Test]
        public void GetProperty_WithObject_ForNullObject_ReturnsNull()
        {
            Assert.IsNull(_evaluationContext.GetProperty(null, "ToString"));
        }

        [Test]
        public void GetProperty_WithObject_ForUnknownProperty_ReturnsNull()
        {
            Assert.IsNull(_evaluationContext.GetProperty(this, "any_property"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddStateObject_UsingNullStateObject_RaisesException()
        {
            _evaluationContext.AddStateObject(null);
        }

        [Test]
        public void AddStateObject_InNormalCases_BehavesAsExpected()
        {
            _evaluationContext.AddStateObject(this);
            Assert.IsTrue(_evaluationContext.ContainsStateObject(this));
        }

        [Test]
        public void AddStateObject_ForTheSameObjectMultipleTimes_KeepsOnlyOneCopy()
        {
            _evaluationContext.AddStateObject(this);
            _evaluationContext.AddStateObject(this);
            _evaluationContext.RemoveStateObject(this);

            Assert.IsFalse(_evaluationContext.ContainsStateObject(this));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveStateObject_UsingNullStateObject_RaisesException()
        {
            _evaluationContext.RemoveStateObject(null);
        }

        [Test]
        public void RemoveStateObject_ForUnknownObject_DoesNothing()
        {
            _evaluationContext.AddStateObject(1);
            _evaluationContext.RemoveStateObject(2);

            Assert.IsTrue(_evaluationContext.ContainsStateObject(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsStateObject_UsingNullStateObject_RaisesException()
        {
            _evaluationContext.ContainsStateObject(null);
        }

        [Test]
        public void ContainsStateObject_ForUnknownObject_ReturnsFalse()
        {
            Assert.IsFalse(_evaluationContext.ContainsStateObject(1));
        }

        [Test]
        public void ContainsStateObject_ForKnownObject_ReturnsTrue()
        {
            _evaluationContext.AddStateObject(1);
            Assert.IsTrue(_evaluationContext.ContainsStateObject(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoke_UsingNullMethodName_RaisesException()
        {
            _evaluationContext.Invoke(null, new object[] { });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Invoke_UsingInvalidMethodName_RaisesException()
        {
            _evaluationContext.Invoke("123", new object[] { });
        }

        [Test]
        public void Invoke_ForPropertyDefinedInContextAndNoArguments_ReturnsThePropertyValue()
        {
            _evaluationContext.SetProperty("property", 100);
            Assert.AreEqual(100, _evaluationContext.Invoke("property", null));
        }

        [Test]
        public void Invoke_ForPropertyDefinedInContextAndArguments_ReturnsNull()
        {
            _evaluationContext.SetProperty("property", 100);
            Assert.IsNull(_evaluationContext.Invoke("property", new object[] { 1 }));
        }

        [Test]
        public void Invoke_ForMethodDefinedInSelfObject_ReturnsItsValue()
        {
            Assert.AreEqual(ToString(), _evaluationContext.Invoke("ToString", null));
        }

        [Test]
        public void Invoke_UsingNullArgumentsArray_IsSimilarToEmptyArray()
        {
            Assert.AreEqual(_evaluationContext.Invoke("ToString", new object[] { }), _evaluationContext.Invoke("ToString", null));
        }

        [Test]
        public void Invoke_ForUnknownMethodOrProperty_ReturnsNull()
        {
            Assert.IsNull(_evaluationContext.Invoke("DoesNotExist", null));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invoke_WithObject_UsingNullMethodName_RaisesException()
        {
            _evaluationContext.Invoke(this, null, new object[] { });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Invoke_WithObject_UsingInvalidMethodName_RaisesException()
        {
            _evaluationContext.Invoke(this, "123", new object[] { });
        }

        [Test]
        public void Invoke_WithObject_ForPropertyDefinedInContextAndNoArguments_ReturnsNull()
        {
            _evaluationContext.SetProperty("property", 100);
            Assert.IsNull(_evaluationContext.Invoke(this, "property", null));
        }

        [Test]
        public void Invoke_WithObject_ForToStringForAnInteger_ReturnsItsValue()
        {
            Assert.AreEqual("100", _evaluationContext.Invoke(100, "ToString", null));
        }

        [Test]
        public void Invoke_WithObject_UsingNullAgrumentsArray_IsSimilarToEmptyArray()
        {
            Assert.AreEqual(_evaluationContext.Invoke(this, "ToString", new object[] { }), _evaluationContext.Invoke(this, "ToString", null));
        }

        [Test]
        public void Invoke_WithObject_ForUnknownMethodOrProperty_ReturnsNull()
        {
            Assert.IsNull(_evaluationContext.Invoke(this, "DoesNotExist", null));
        }
    }
}

