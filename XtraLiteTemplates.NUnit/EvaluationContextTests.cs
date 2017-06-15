//
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
//

namespace XtraLiteTemplates.NUnit
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Threading;
    using Evaluation;
    using Introspection;
    using global::NUnit.Framework;

    [TestFixture]
    [SuppressMessage("ReSharper", "ObjectCreationAsStatement")]
    [SuppressMessage("ReSharper", "StringLiteralTypo")]
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
        public void ConstructorUsingNullIdentifierComparerRaisesException()
        {
            new EvaluationContext(false, CancellationToken.None, null, ObjectFormatter, this, (context, text) => text);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorUsingNullObjectFormatterRaisesException()
        {
            new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCulture, null, this, (context, text) => text);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_UsingNullUnParsedTextHandler_RaisesException()
        {
            new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCulture, ObjectFormatter, this, null);
        }

        [Test]
        public void ConstructorPassingTrueForIgnoreEvaluationExceptionsStoresTheValue()
        {
            var evaluationContext = new EvaluationContext(true, CancellationToken.None, StringComparer.InvariantCulture, ObjectFormatter, this, (context, text) => text);
            Assert.AreEqual(true, evaluationContext.IgnoreEvaluationExceptions);
        }

        [Test]
        public void ConstructorPassingFalseForIgnoreEvaluationExceptionsStoresTheValue()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCulture, ObjectFormatter, this, (context, text) => text);
            Assert.AreEqual(false, evaluationContext.IgnoreEvaluationExceptions);
        }

        [Test]
        public void ConstructorForCancellationTokenStoresTheValue()
        {
            var source = new CancellationTokenSource();
            var evaluationContext = new EvaluationContext(false, source.Token, StringComparer.InvariantCulture, ObjectFormatter, this, (context, text) => text);

            Assert.AreEqual(source.Token, evaluationContext.CancellationToken);
        }

        [Test]
        public void ConstructorForInvariantCultureStringComparerStoresTheValue()
        {
            _evaluationContext.SetProperty("caseMatters", 1);

            Assert.IsNull(_evaluationContext.GetProperty("Casematters"));
        }

        [Test]
        public void ConstructorForSelfObjectStoresTheValue()
        {
            Assert.AreEqual(GetType(), _evaluationContext.GetProperty("GetType"));
        }

        [Test]
        public void Constructor_ForUnParsedTextHandler_StoresTheValue()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCultureIgnoreCase, ObjectFormatter, this, (context, text) => text.ToUpper());

            Assert.AreEqual("LOWERCASE", evaluationContext.ProcessUnParsedText("lowercase"));
        }

        [Test]
        public void ProcessUnParsedTextUsingNonNullTextExecutesDelegate()
        {
            var evaluationContext = new EvaluationContext(false, CancellationToken.None, StringComparer.InvariantCultureIgnoreCase,
                ObjectFormatter, this, (context, text) =>
                {
                    Assert.AreEqual("text", text);
                    return "EXECUTED";
                });

            Assert.AreEqual("EXECUTED", evaluationContext.ProcessUnParsedText("text"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void SetPropertyUsingNullPropertyNameRaisesException()
        {
            _evaluationContext.SetProperty(null, 1);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void SetPropertyUsingInvalidPropertyNameRaisesException()
        {
            _evaluationContext.SetProperty("123", 1);
        }

        [Test]
        public void SetPropertyForCorrectPropertyNameBehavesAsExpected()
        {
            _evaluationContext.SetProperty("property", 1);

            Assert.AreEqual(1, _evaluationContext.GetProperty("property"));
        }

        [Test]
        public void SetPropertyForCaseInsensitiveContextBehavesAsExpected()
        {
            _evaluationContextIgnoreCase.SetProperty("property", 1);
            _evaluationContextIgnoreCase.SetProperty("PROPERTY", 2);

            Assert.AreEqual(2, _evaluationContextIgnoreCase.GetProperty("Property"));
        }

        [Test]
        public void SetPropertyForPropertyPartOfSelfObjectOverridesIt()
        {
            _evaluationContext.SetProperty("ToString", "Override!");

            Assert.AreEqual("Override!", _evaluationContext.GetProperty("ToString"));
        }

        [Test]
        public void SetPropertyForCaseInsensitiveContextAndPropertySimilarWithOneInSelfObjectOverridesIt()
        {
            _evaluationContextIgnoreCase.SetProperty("TOSTRING", "Override!");

            Assert.AreEqual("Override!", _evaluationContextIgnoreCase.GetProperty("ToString"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetPropertyUsingNullPropertyNameRaisesException()
        {
            _evaluationContext.GetProperty(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetPropertyUsingInvalidPropertyNameRaisesException()
        {
            _evaluationContext.GetProperty("123");
        }

        [Test]
        public void GetPropertyForCaseInsensitiveContextIgnoresCase()
        {
            _evaluationContextIgnoreCase.SetProperty("property", 1);

            Assert.AreEqual(1, _evaluationContextIgnoreCase.GetProperty("PROPERTY"));
        }

        [Test]
        public void GetPropertyForPropertyPartOfSelfObjectPassesTheControlToSelfObject()
        {
            Assert.AreEqual(ToString(), _evaluationContext.GetProperty("ToString"));
        }

        [Test]
        public void GetPropertyForUnknownPropertyReturnsNull()
        {
            Assert.IsNull(_evaluationContext.GetProperty("any_property"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void GetPropertyWithObjectUsingNullPropertyNameRaisesException()
        {
            _evaluationContext.GetProperty(this, null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void GetPropertyWithObjectUsingInvalidPropertyNameRaisesException()
        {
            _evaluationContext.GetProperty(this, "123");
        }

        [Test]
        public void GetPropertyWithObjectForCaseInsensitiveContextIgnoresCase()
        {
            Assert.AreEqual(ToString(), _evaluationContextIgnoreCase.GetProperty(this, "TOSTRING"));
        }

        [Test]
        public void GetPropertyWithObjectForNullObjectReturnsNull()
        {
            Assert.IsNull(_evaluationContext.GetProperty(null, "ToString"));
        }

        [Test]
        public void GetPropertyWithObjectForUnknownPropertyReturnsNull()
        {
            Assert.IsNull(_evaluationContext.GetProperty(this, "any_property"));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddStateObjectUsingNullStateObjectRaisesException()
        {
            _evaluationContext.AddStateObject(null);
        }

        [Test]
        public void AddStateObjectInNormalCasesBehavesAsExpected()
        {
            _evaluationContext.AddStateObject(this);
            Assert.IsTrue(_evaluationContext.ContainsStateObject(this));
        }

        [Test]
        public void AddStateObjectForTheSameObjectMultipleTimesKeepsOnlyOneCopy()
        {
            _evaluationContext.AddStateObject(this);
            _evaluationContext.AddStateObject(this);
            _evaluationContext.RemoveStateObject(this);

            Assert.IsFalse(_evaluationContext.ContainsStateObject(this));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void RemoveStateObjectUsingNullStateObjectRaisesException()
        {
            _evaluationContext.RemoveStateObject(null);
        }

        [Test]
        public void RemoveStateObjectForUnknownObjectDoesNothing()
        {
            _evaluationContext.AddStateObject(1);
            _evaluationContext.RemoveStateObject(2);

            Assert.IsTrue(_evaluationContext.ContainsStateObject(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ContainsStateObjectUsingNullStateObjectRaisesException()
        {
            _evaluationContext.ContainsStateObject(null);
        }

        [Test]
        public void ContainsStateObjectForUnknownObjectReturnsFalse()
        {
            Assert.IsFalse(_evaluationContext.ContainsStateObject(1));
        }

        [Test]
        public void ContainsStateObjectForKnownObjectReturnsTrue()
        {
            _evaluationContext.AddStateObject(1);
            Assert.IsTrue(_evaluationContext.ContainsStateObject(1));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvokeUsingNullMethodNameRaisesException()
        {
            _evaluationContext.Invoke(null, new object[] { });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void InvokeUsingInvalidMethodNameRaisesException()
        {
            _evaluationContext.Invoke("123", new object[] { });
        }

        [Test]
        public void InvokeForPropertyDefinedInContextAndNoArgumentsReturnsThePropertyValue()
        {
            _evaluationContext.SetProperty("property", 100);
            Assert.AreEqual(100, _evaluationContext.Invoke("property", null));
        }

        [Test]
        public void InvokeForPropertyDefinedInContextAndArgumentsReturnsNull()
        {
            _evaluationContext.SetProperty("property", 100);
            Assert.IsNull(_evaluationContext.Invoke("property", new object[] { 1 }));
        }

        [Test]
        public void InvokeForMethodDefinedInSelfObjectReturnsItsValue()
        {
            Assert.AreEqual(ToString(), _evaluationContext.Invoke("ToString", null));
        }

        [Test]
        public void InvokeUsingNullArgumentsArrayIsSimilarToEmptyArray()
        {
            Assert.AreEqual(_evaluationContext.Invoke("ToString", new object[] { }), _evaluationContext.Invoke("ToString", null));
        }

        [Test]
        public void InvokeForUnknownMethodOrPropertyReturnsNull()
        {
            Assert.IsNull(_evaluationContext.Invoke("DoesNotExist", null));
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void InvokeWithObjectUsingNullMethodNameRaisesException()
        {
            _evaluationContext.Invoke(this, null, new object[] { });
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void InvokeWithObjectUsingInvalidMethodNameRaisesException()
        {
            _evaluationContext.Invoke(this, "123", new object[] { });
        }

        [Test]
        public void InvokeWithObjectForPropertyDefinedInContextAndNoArgumentsReturnsNull()
        {
            _evaluationContext.SetProperty("property", 100);
            Assert.IsNull(_evaluationContext.Invoke(this, "property", null));
        }

        [Test]
        public void InvokeWithObjectForToStringForAnIntegerReturnsItsValue()
        {
            Assert.AreEqual("100", _evaluationContext.Invoke(100, "ToString", null));
        }

        [Test]
        public void InvokeWithObjectUsingNullArgumentsArrayIsSimilarToEmptyArray()
        {
            Assert.AreEqual(_evaluationContext.Invoke(this, "ToString", new object[] { }), _evaluationContext.Invoke(this, "ToString", null));
        }

        [Test]
        public void InvokeWithObjectForUnknownMethodOrPropertyReturnsNull()
        {
            Assert.IsNull(_evaluationContext.Invoke(this, "DoesNotExist", null));
        }

        [Test]
        public void GetPropertyHonorsStaticTypes()
        {
            var result = _evaluationContext.GetProperty(new Static(typeof(Math)), "PI");
            Assert.AreEqual(Math.PI, result);
        }

        [Test]
        public void InvokeHonorsStaticTypes()
        {
            var result = _evaluationContext.Invoke(new Static(typeof(Math)), "Abs", new object[] {-1} );
            Assert.AreEqual(1, result);
        }
    }
}

