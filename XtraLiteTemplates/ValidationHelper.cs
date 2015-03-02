using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Utils
{
    internal static class ValidationHelper
    {
        public static void Assert(String argumentName, String condition, Boolean truth)
        {
            if (!truth)
                throw new ArgumentException(String.Format("Expected condition '{0}' to be satisfied for argument '{1}'.", condition, argumentName), argumentName);
        }

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

        public static void AssertCollectionIsNotEmpty<T>(String argumentName, IReadOnlyCollection<T> argumentValue)
        {
            AssertArgumentIsNotNull(argumentName, argumentValue);
            if (argumentValue.Count == 0)
                throw new ArgumentException(String.Format("Expected a non-empty collection: '{0}'.", argumentName), argumentName);
        }

        public static void AssertObjectCollectionIsNotEmpty<T>(String argumentName, IReadOnlyCollection<T> argumentValue)
            where T: class
        {
            AssertCollectionIsNotEmpty(argumentName, argumentValue);
            foreach (var o in argumentValue)
                AssertArgumentIsNotNull(String.Format("element of {0}", argumentName), o);
        }

        public static void AssertArgumentGreaterThanZero(String argumentName, Int32 argumentValue)
        {
            if (argumentValue <= 0)
                throw new ArgumentOutOfRangeException(String.Format("Expected argument '{0}' to have a value greater than zero (but has a value of {1}).", argumentName, argumentValue), argumentName);
        }
    }
}
