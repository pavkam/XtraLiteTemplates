using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Dialects.Standard
{
    /// <summary>
    /// Defines a method that allows obtaing the string representation of an untyped <see cref="Object"/>.
    /// </summary>
    public interface IObjectFormatter
    {
        /// <summary>
        /// Gets the string representation of an <see cref="Object"/>.
        /// </summary>
        /// <param name="obj">The object to obtain the string representation for.</param>
        /// <returns>The string representation.</returns>
        String ToString(Object obj);

        /// <summary>
        /// Gets the string representation of an <see cref="Object" /> using the given <paramref name="formatProvider"/>.
        /// </summary>
        /// <param name="obj">The object to obtain the string representation for.</param>
        /// <param name="formatProvider">The format provider.</param>
        /// <returns>
        /// The string representation.
        /// </returns>
        /// <exception cref="ArgumentNullException">Argument <paramref name="formatProvider"/> is <c>null</c>.</exception>
        String ToString(Object obj, IFormatProvider formatProvider);
    }
}
