using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Dialects.Standard
{
    /// <summary>
    /// Common interface used by the standard dialects to facilitate type-conversion. Standard operators and directives
    /// use this intreface to obtain the data type required for their correct operation.
    /// </summary>
    public interface IPrimitiveTypeConverter
    {
        /// <summary>
        /// Detects the primitive type of the supplied object. Standard operators and directives can use this
        /// method to detect the data types involved in the operation and decide on the appropriate evaluation techniques.
        /// </summary>
        /// <param name="obj">The object to check the type for.</param>
        /// <returns>
        /// A <see cref="PrimitiveType" /> value.
        /// </returns>
        PrimitiveType TypeOf(Object obj);

        /// <summary>
        /// Converts an object to a 32-bit integer.
        /// <remarks>
        /// This method is guaranteed to always return a value and may never fail.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>
        /// A <see cref="Int32" /> value.
        /// </returns>
        Int32 ConvertToInteger(Object obj);

        /// <summary>
        /// Converts an object to a double-precision floating point number.
        /// <remarks>
        /// This method is guaranteed to always return a value and may never fail.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>
        /// A <see cref="Double" /> value.
        /// </returns>
        Double ConvertToNumber(Object obj);

        /// <summary>
        /// Converts an object to its string representation.
        /// <remarks>
        /// This method is guaranteed to always return a value and may never fail.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>
        /// A <see cref="String" /> value.
        /// </returns>
        String ConvertToString(Object obj);

        /// <summary>
        /// Converts an object to a boolean.
        /// <remarks>
        /// This method is guaranteed to always return a value and may never fail.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>
        /// A <see cref="Boolean" /> value.
        /// </returns>
        Boolean ConvertToBoolean(Object obj);

        /// <summary>
        /// Converts an object to a sequence of objects.
        /// <remarks>
        /// This method is guaranteed to always return a value and may never fail.
        /// </remarks>
        /// </summary>
        /// <param name="obj">The object to convert.</param>
        /// <returns>
        /// A <see cref="IEnumerable{Object}" /> value.
        /// </returns>
        IEnumerable<Object> ConvertToSequence(Object obj);
    }
}
