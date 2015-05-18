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
using NUnit.Framework;

namespace XtraLiteTemplates.NUnit
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using XtraLiteTemplates.NUnit.Inside;
    using XtraLiteTemplates.Evaluation;
    using XtraLiteTemplates.Parsing;
    using System.Globalization;
    using XtraLiteTemplates.Dialects.Standard;

    [TestFixture]
    public class XLTemplateTests : TestBase
    {
        [Test]
        public void TestCaseCaseSensitiveInvariantCulture()
        {
            const String text = @"
{REPEAT 20.1 TIMES}={END}{'  '}
    {FOR EACH index IN 0 : 100}
        {IF index % 10.2 == 0 then}
            {index / 10 + ': '}
            {' ' + var1 + ' ' IF INDEX%20 == 0}
            {' ' + VAR2 + ' ' IF index%30 == 0}
            {if (inDex/10) % 2 == 0 THEN}
                {' EVEN'}
            {else}
                {' ODD'}
            {END} -- 

            
        {end}
    {END}
{'  '}{REPEAT 20.19 times}={END}
";
            var variables = new Dictionary<String, Object>()
            {
                { "var1", "<+>" },
                { "var2", ">-<" },
            };

            var template = new XLTemplate(StandardDialect.OrdinalIgnoreCase, text.Replace("'", "\""));
            var result = template.Evaluate(variables);

            Assert.AreEqual("====================  0:  <+>  >-<  EVEN--1:  ODD--2:  <+>  EVEN--3:  >-<  ODD--4:  <+>  EVEN--5:  ODD--6:  <+>  >-<  EVEN--7:  ODD--8:  <+>  EVEN--9:  >-<  ODD--10:  <+>  EVEN--  ====================", result);
        }
    }
}

