using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates
{
    internal static class ValidationHelper
    {
        public static void AssertArgumentIsNotNull(String argumentName, Object argumentValue)
        {
            if (argumentValue == null)
                throw new ArgumentException(String.Format("Expected a non-null value for argument '{0}'.", argumentName), argumentName);
        }

        public static void AssertArgumentIsNotAnEmptyString(String argumentName, String argumentValue)
        {
            AssertArgumentIsNotNull(argumentName, argumentValue);
            if (argumentValue.Length == 0)
                throw new ArgumentException(String.Format("Expected a non-empty string value for argument '{0}'.", argumentName), argumentName);
        }

        public static void AssertArgumentGreaterThanZero(String argumentName, Int32 argumentValue)
        {
            if (argumentValue <= 0)
                throw new ArgumentOutOfRangeException(String.Format("Expected argument '{0}' to have a value greater than zero (but has a value of {1}).", argumentName, argumentValue), argumentName);
        }
    }
}
