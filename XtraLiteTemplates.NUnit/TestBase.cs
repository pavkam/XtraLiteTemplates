using System;
using NUnit.Framework;

namespace XtraLiteTemplates.NUnit
{
    public class TestBase
    {
        protected void AssertArgumentNullException(String argument, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(ArgumentNullException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" cannot be null.", argument), 
                        StringComparison.CurrentCulture));
            }
        }

        protected void AssertArgumentEmptyException(String argument, Action action)
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
                    Assert.IsInstanceOfType(typeof(ArgumentException), e);
                    Assert.IsTrue(e.Message.StartsWith(
                            String.Format("Argument \"{0}\" cannot be empty.", argument), 
                            StringComparison.CurrentCulture));
                }
            }
        }

        protected void AssertArgumentsEqualException(String argument1, String argument2, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(ArgumentException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Arguments \"{0}\" and \"{1}\" cannot be equal.", argument1, argument2), 
                        StringComparison.CurrentCulture));
            }
        }

        protected void AssertArgumentLessThanOrEqualException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be greater than {1}.", argument, than), 
                        StringComparison.CurrentCulture));
            }
        }

        protected void AssertArgumentLessThanException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be greater than or equal to {1}.", argument, than), 
                        StringComparison.CurrentCulture));
            }
        }

        protected void AssertArgumentGreaterThanOrEqualException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be less than {1}.", argument, than), 
                        StringComparison.CurrentCulture));
            }
        }

        protected void AssertArgumentGreaterThanException<T>(String argument, T than, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(ArgumentOutOfRangeException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument \"{0}\" is expected to be less than or equal to {1}.", argument, than), 
                        StringComparison.CurrentCulture));
            }
        }

        protected void AssertArgumentConditionNotTrueException(String condition, Action action)
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Assert.IsInstanceOfType(typeof(ArgumentException), e);
                Assert.IsTrue(e.Message.StartsWith(
                        String.Format("Argument condition \"{0}\" failed to be validated as true.", condition), 
                        StringComparison.CurrentCulture));
            }
        }
    }
}

