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

    public class TestBase
    {
        protected void ExpectArgumentNullException(String argument, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentNullException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" cannot be null.", argument), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentEmptyException(String argument, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                if (e is ArgumentNullException)
                {
                    Assert.IsTrue(e.Message.StartsWith(
                            String.Format("Argument \"{0}\" cannot be null.", argument), 
                            StringComparison.CurrentCulture));
                }
                else
                {
                    Assert.IsInstanceOf(typeof(ArgumentException), e);
                    Assert.IsTrue(e.Message.StartsWith(
                            String.Format("Argument \"{0}\" cannot be empty.", argument), 
                            StringComparison.CurrentCulture));
                }

                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentsEqualException(String argument1, String argument2, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Arguments \"{0}\" and \"{1}\" cannot be equal.", argument1, argument2), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentLessThanOrEqualException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be greater than {1}.", argument, than), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentLessThanException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be greater than or equal to {1}.", argument, than), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentGreaterThanOrEqualException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be less than {1}.", argument, than), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentGreaterThanException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be less than or equal to {1}.", argument, than), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }

        protected void ExpectArgumentConditionNotTrueException(String condition, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOf(typeof(ArgumentException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument condition \"{0}\" failed to be validated as true.", condition), 
                        StringComparison.CurrentCulture));
                return;
            }

            Assert.Fail();
        }
    }
}

