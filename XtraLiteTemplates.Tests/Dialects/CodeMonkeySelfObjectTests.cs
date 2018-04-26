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

namespace XtraLiteTemplates.Tests.Dialects
{
    using System;

    using global::NUnit.Framework;

    using XtraLiteTemplates.Dialects.Standard;
    using XtraLiteTemplates.Introspection;

    [TestFixture]
    public class CodeMonkeySelfObjectTests: StandardSelfObjectTests
    {
        private CodeMonkeySelfObject _selfObject;

        protected override StandardSelfObject CreateSelfObject(IPrimitiveTypeConverter typeConverter)
        {
            return new CodeMonkeySelfObject(typeConverter);
        }

        [SetUp]
        public void SetUp()
        {
            _selfObject = (CodeMonkeySelfObject)CreateSelfObject(TypeConverter);
        }

        [TestCase(-1)]
        [TestCase(2)]
        public void TestCaseAbs(double input)
        {
            Assert.AreEqual(Math.Abs(input), _selfObject.Abs(input));
        }

        [TestCase(-1)]
        [TestCase(-1.4)]
        [TestCase(-1.5)]
        [TestCase(-1.6)]
        [TestCase(2)]
        [TestCase(2.4)]
        [TestCase(2.5)]
        [TestCase(2.6)]
        public void TestCaseCeiling(double input)
        {
            Assert.AreEqual(Math.Ceiling(input), _selfObject.Ceiling(input));
        }

        [TestCase(-1)]
        [TestCase(-1.4)]
        [TestCase(-1.5)]
        [TestCase(-1.6)]
        [TestCase(2)]
        [TestCase(2.4)]
        [TestCase(2.5)]
        [TestCase(2.6)]
        public void TestCaseFloor(double input)
        {
            Assert.AreEqual(Math.Floor(input), _selfObject.Floor(input));
        }

        [TestCase(-1, -1)]
        [TestCase(-1, 1)]
        [TestCase(1, -1)]
        [TestCase(double.PositiveInfinity, double.NegativeInfinity)]
        public void TestCaseMin(double v1, double v2)
        {
            Assert.AreEqual(Math.Min(v1, v2), _selfObject.Min(v1, v2));
        }

        [TestCase(-1, -1)]
        [TestCase(-1, 1)]
        [TestCase(1, -1)]
        [TestCase(double.PositiveInfinity, double.NegativeInfinity)]
        public void TestCaseMax(double v1, double v2)
        {
            Assert.AreEqual(Math.Max(v1, v2), _selfObject.Max(v1, v2));
        }

        [TestCase(1.12345, 1)]
        [TestCase(1.12345, 2)]
        [TestCase(-1.12345, 1)]
        [TestCase(-1.12345, 2)]
        public void TestCaseRound(double v, int d)
        {
            Assert.AreEqual(Math.Round(v, d), _selfObject.Round(v, d));
        }

        [Test]
        public void TestCaseJoin()
        {
            var result = _selfObject.Join(",", new[] { "1", "2", "3" });

            Assert.AreEqual("1,2,3", result);
        }

        [Test]
        public void TestCaseJoinOnNullSequence()
        {
            var result = _selfObject.Join(",", null);

            Assert.IsNull(result);
        }

        [Test]
        public void TestCaseNl()
        {
            Assert.AreEqual(Environment.NewLine, _selfObject.NL);
        }

        public struct SystemEnvironment
        {
            /// <summary>
            /// Gets the name of the machine.
            /// </summary>
            /// <value>
            /// The name of the machine.
            /// </value>
            public string MachineName => Environment.MachineName;

            /// <summary>
            /// Gets the name of the user domain.
            /// </summary>
            /// <value>
            /// The name of the user domain.
            /// </value>
            public string UserDomainName => Environment.UserDomainName;

            /// <summary>
            /// Gets the name of the user.
            /// </summary>
            /// <value>
            /// The name of the user.
            /// </value>
            public string UserName => Environment.UserName;

            /// <summary>
            /// Gets the OS version.
            /// </summary>
            /// <value>
            /// The OS version.
            /// </value>
            public string OsVersion => Environment.OSVersion.ToString();
        }

        [Test]
        public void TestCaseSystemMachineName()
        {
            Assert.AreEqual(Environment.MachineName, _selfObject.System.MachineName);
        }

        [Test]
        public void TestCaseSystemOsVersion()
        {
            Assert.AreEqual(Environment.OSVersion.ToString(), _selfObject.System.OsVersion);
        }

        [Test]
        public void TestCaseSystemUserDomainName()
        {
            Assert.AreEqual(Environment.UserDomainName, _selfObject.System.UserDomainName);
        }

        [Test]
        public void TestCaseSystemUserName()
        {
            Assert.AreEqual(Environment.UserName, _selfObject.System.UserName);
        }
    }
}

