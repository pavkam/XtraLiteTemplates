//
//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2018, Alexandru Ciobanu (alex+git@ciobanu.org)
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

namespace XtraLiteTemplates.Tests
{
    using global::NUnit.Framework;
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Dynamic;
    using System.Linq;
    using Introspection;

    [TestFixture]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public class SimpleDynamicInvokerTests : TestBase
    {
        private string _privateField = nameof(_privateField);
        public string PublicField = nameof(PublicField);
        public string PublicProperty => nameof(PublicProperty);

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        public string NoArgumentMethod()
        {
            return nameof(NoArgumentMethod);
        }

        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public long MatchArgumentMethod(string a1, bool a2)
        {
            return long.Parse(a1) + (a2 ? 1 : 2);
        }

        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public string FullArgumentMethod(string a1, string a2, string a3, string a4, string a5, string a6, string a7, string a8, string a9,
                                         string a10, string a11, string a12, string a13, string a14)
        {
            return $"{a1}-{a2}-{a3}-{a4}-{a5}-{a6}-{a7}-{a8}-{a9}-{a10}-{a11}-{a12}-{a13}-{a14}";
        }


        [SuppressMessage("ReSharper", "UnusedMember.Global")]
        [SuppressMessage("ReSharper", "MemberCanBeMadeStatic.Global")]
        public string ThrowException()
        {
            throw new ArgumentException("Die!");
        }

        [Test]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void GetValueThrowsErrorIfMemberHasInvalidFormat()
        {
            var invoker = new SimpleDynamicInvoker();

            Assert.Throws<ArgumentNullException>(() => invoker.GetValue(this, null));
            Assert.Throws<ArgumentException>(() => invoker.GetValue(this, ""));
            Assert.Throws<ArgumentException>(() => invoker.GetValue(this, " broken "));
        }

        [Test]
        [SuppressMessage("ReSharper", "AssignNullToNotNullAttribute")]
        public void InvokeThrowsErrorIfMemberHasInvalidFormat()
        {
            var invoker = new SimpleDynamicInvoker();

            Assert.Throws<ArgumentNullException>(() => invoker.Invoke(this, null));
            Assert.Throws<ArgumentException>(() => invoker.Invoke(this, ""));
            Assert.Throws<ArgumentException>(() => invoker.Invoke(this, " broken "));
        }

        [Test]
        public void GetValueReturnsNullIfObjectIsNull()
        {
            var invoker = new SimpleDynamicInvoker();

            Assert.IsNull(invoker.GetValue(null, "property"));
        }

        [Test]
        public void InvokeReturnsNullIfObjectIsNull()
        {
            var invoker = new SimpleDynamicInvoker();

            Assert.IsNull(invoker.Invoke(null, "property"));
        }

        [Test]
        public void InvokeThrowsErrorIfArgsCountGreaterThanFourteen()
        {
            var invoker = new SimpleDynamicInvoker();

            var args = Enumerable.Range(0, 15).Cast<object>().ToArray();
            Assert.Throws<ArgumentOutOfRangeException>(() => invoker.Invoke(this, "property", args));
        }

        [Test]
        public void GetValueReturnsNullIfMemberIsNotAccessible()
        {
            var invoker = new SimpleDynamicInvoker();

            var result = invoker.GetValue(this, nameof(_privateField));
            Assert.IsNull(result);
        }

        [Test]
        public void GetValueReturnsTheValueOfField()
        {
            var invoker = new SimpleDynamicInvoker();

            var result = invoker.GetValue(this, nameof(PublicField));
            Assert.AreEqual(nameof(PublicField), result);
        }

        [Test]
        public void GetValueReturnsTheValueOfProperty()
        {
            var invoker = new SimpleDynamicInvoker();

            var result = invoker.GetValue(this, nameof(PublicProperty));
            Assert.AreEqual(nameof(PublicProperty), result);
        }

        [Test]
        public void InvokeCallsTheFullArgumentMethod()
        {
            var invoker = new SimpleDynamicInvoker();
            var args = new object[] { "1", "2", "3", "4", "5", "6", "7", "8", "9", "10", "11", "12", "13", "14" };
            var result = invoker.Invoke(
                this,
                nameof(FullArgumentMethod),
                args);

            Assert.AreEqual(string.Join("-", args), result);
        }

        [Test]
        public void InvokeFailsToMatchWithImpreciseArguments1()
        {
            var invoker = new SimpleDynamicInvoker();

            var result = invoker.Invoke(this, nameof(MatchArgumentMethod), new object[] { "arg1" });
            Assert.IsNull(result);
        }

        [Test]
        public void InvokeFailsToMatchWithImpreciseArguments2()
        {
            var invoker = new SimpleDynamicInvoker();

            var result = invoker.Invoke(this, nameof(MatchArgumentMethod), new object[] { "arg1", 1 });
            Assert.IsNull(result);
        }

        [Test]
        public void InvokeCallsTheMatchingMethod()
        {
            var invoker = new SimpleDynamicInvoker();
            var result = invoker.Invoke(this, nameof(MatchArgumentMethod), new object[] { "100", false });

            Assert.AreEqual(102, result);
        }

        [Test]
        public void InvokeLeaksException()
        {
            var invoker = new SimpleDynamicInvoker();

            Assert.Throws<ArgumentException>(() => invoker.Invoke(this, nameof(ThrowException)));
        }

        [Test]
        public void InvokeReturnsNullForNonExistentMethod()
        {
            var invoker = new SimpleDynamicInvoker();
            var result = invoker.Invoke(this, "Fake");

            Assert.IsNull(result);
        }


        [Test]
        public void GetValueReturnsTrulyDynamicPropertyValue()
        {
            var invoker = new SimpleDynamicInvoker();
            dynamic expando = new ExpandoObject();
            expando.Property = 100;

            var result = invoker.GetValue(expando, nameof(expando.Property));
            Assert.AreEqual(expando.Property, result);
        }
    }
}

