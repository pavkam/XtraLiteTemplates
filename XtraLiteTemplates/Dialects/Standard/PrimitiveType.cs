using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Dialects.Standard
{
    /// <summary>
    /// Defines one of the types the standard dialects support and can operate upon.
    /// </summary>
    public enum PrimitiveType
    {
        /// <summary>
        /// Undefined data type. Normally maps to the <c>null</c> object values.
        /// </summary>
        Undefined = 0,
        /// <summary>
        /// Number data type. Represented internally as a double-precision floating point number.
        /// </summary>
        Number,
        /// <summary>
        /// The string data type. There is no <c>null</c> string as that is equivalent to a <see cref="Undefined"/> value.
        /// </summary>
        String,
        /// <summary>
        /// The boolean data type.
        /// </summary>
        Boolean,
        /// <summary>
        /// Any object that does not map directly to the other four types.
        /// </summary>
        Object,
    }
}
