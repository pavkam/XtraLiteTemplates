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

[module: System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1634:FileHeaderMustShowCopyright", Justification = "Does not apply.")]

namespace XtraLiteTemplates.Introspection
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Utility class that allows for easy access to a type's properties and methods.
    /// This class is used internally to implement the member access operator.
    /// </summary>
    public sealed class SimpleTypeDisemboweler
    {
        private IDictionary<string, Func<object, object[], object>> cachedMemberMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTypeDisemboweler" /> class.
        /// </summary>
        /// <param name="type">The <see cref="System.Type" /> to inspect.</param>
        /// <param name="memberComparer">An instance of <see cref="IEqualityComparer{String}" /> used when looking up properties in the inspected type.</param>
        /// <param name="objectFormatter">The object formatter.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="type" /> or <paramref name="memberComparer" /> parameters are <c>null</c>.</exception>
        public SimpleTypeDisemboweler(
            Type type, 
            IEqualityComparer<string> memberComparer, 
            IObjectFormatter objectFormatter)
        {
            Expect.NotNull("type", type);
            Expect.NotNull("memberComparer", memberComparer);
            Expect.NotNull("objectFormatter", objectFormatter);

            this.Type = type;
            this.Comparer = memberComparer;
            this.ObjectFormatter = objectFormatter;

            this.cachedMemberMap = new Dictionary<string, Func<object, object[], object>>();
        }

        /// <summary>
        ///   <value>Gets the <see cref="System.Type" /> that represents the type being inspected.</value>
        /// </summary>
        /// <value>
        /// The inspected type.
        /// </value>
        /// <remarks>
        /// Value provided by the caller during construction.
        /// </remarks>
        public Type Type { get; private set; }

        /// <summary>
        /// Gets the <see cref="IEqualityComparer{String}" /> used by <see cref="SimpleTypeDisemboweler" /> to compare the
        /// names of properties. This property is primarily used to decide the case-sensitivity of this <see cref="SimpleTypeDisemboweler" /> instance.
        /// <remarks>Value provided by the caller during construction.</remarks>
        /// </summary>
        /// <value>
        /// The equality comparer.
        /// </value>
        public IEqualityComparer<string> Comparer { get; private set; }

        /// <summary>
        /// Gets the <see cref="IObjectFormatter" /> used by <see cref="SimpleTypeDisemboweler" /> to convert object instances to their string representation.
        /// <remarks>Value provided by the caller during construction.</remarks>
        /// </summary>
        /// <value>
        /// The object formatter.
        /// </value>
        public IObjectFormatter ObjectFormatter { get; private set; }

        /// <summary>
        /// Invokes the <paramref name="member" /> of <paramref name="object" /> and returns its value.
        /// </summary>
        /// <param name="object">The object whose member is being invoked.</param>
        /// <param name="member">The member name.</param>
        /// <returns>The result of member invoke.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="member" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="member" /> is not a valid identifier.</exception>
        public object Invoke(object @object, string member)
        {
            return Invoke(@object, member, null);
        }

        /// <summary>
        /// Invokes the <paramref name="member" /> of <paramref name="object" /> and returns its value.
        /// </summary>
        /// <param name="object">The object whose member is being invoked.</param>
        /// <param name="member">The member name.</param>
        /// <param name="arguments">The arguments to pass to the invoked member.</param>
        /// <returns>
        /// The result of member invoke.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="member" /> is <c>null</c>.</exception>
        /// <exception cref="ArgumentException"><paramref name="member" /> is not a valid identifier.</exception>
        public object Invoke(object @object, string member, object[] arguments)
        {
            Expect.Identifier("member", member);

            if (@object == null)
                return null;

            /* Create the signature of the member. */
            StringBuilder signature = new StringBuilder();
            signature.Append(member);

            if (arguments != null && arguments.Length > 0)
            {
                foreach (var a in arguments)
                {
                    signature.Append('#');
                    if (a == null)
                        signature.Append('?');
                    else
                        signature.Append(a.GetType().Name);
                }
            }

            /* Find a cached member from a previous lookup. */
            Func<object, object[], object> getterMethod;
            if (this.cachedMemberMap.TryGetValue(signature.ToString(), out getterMethod))
            {
                if (getterMethod != null)
                {
                    /* Invoke the cached member */
                    return getterMethod(@object, arguments);
                }
                else
                {
                    /* Previous lookup returned nothing. No point in repeating it. */
                    return null;
                }
            }

            if (arguments == null || arguments.Length == 0)
            {
                /* Scan the type for properties. */
                foreach (var property in Type.GetProperties())
                {
                    if (!Comparer.Equals(property.Name, member) || !property.CanRead || property.GetIndexParameters().Length > 0)
                    {
                        continue;
                    }

                    /* Found a matching property! */
                    getterMethod = (i, a) => property.GetValue(i);
                    this.cachedMemberMap[signature.ToString()] = getterMethod;
                    break;
                }

                /* Now scan for fields. */
                foreach (var field in Type.GetFields())
                {
                    if (!Comparer.Equals(field.Name, member) || field.IsPrivate)
                    {
                        continue;
                    }

                    /* Found a matching property! */
                    getterMethod = (i, a) => field.GetValue(i);
                    this.cachedMemberMap[signature.ToString()] = getterMethod;
                    break;
                }
            }

            if (getterMethod == null)
            {
                MethodInfo bestMatchingMethod = null;
                Func<object, object>[] argumentAdapters = null;
                double bestMatchingScore = 0;

                /* Scan for methods! */
                foreach (var method in Type.GetMethods())
                {                    
                    if (!Comparer.Equals(method.Name, member) || method.IsAbstract || method.IsConstructor || method.IsPrivate)
                    {
                        continue;
                    }

                    /* Reconcile arguments of the method. */
                    var methodParameters = method.GetParameters();
                    foreach (var parameter in methodParameters)
                    {
                        /* Scan for unsupported method parameter types. */
                        if (parameter.IsRetval || parameter.IsOut)
                        {
                            continue;
                        }
                    }

                    Double methodScore = 0;
                    List<Func<object, object>> argumentAdapterFuncs = new List<Func<object, object>>();
                    for (var parameterIndex = 0; parameterIndex < methodParameters.Length; parameterIndex++)
                    {
                        var parameter = methodParameters[parameterIndex];
                        var parameterType = parameter.ParameterType;
                        var parameterTypeCode = Type.GetTypeCode(parameterType);
                        var argument = (arguments != null && arguments.Length > parameterIndex) ? arguments[parameterIndex] : null;
                        var argumentType = argument != null ? argument.GetType() : null;
                        var argumentTypeCode = argumentType != null ? Type.GetTypeCode(argumentType) : TypeCode.Empty;

                        Func<object, object> adapterFunc = null;

                        /* Check argument comptibility. */
                        if (argumentTypeCode == TypeCode.Empty)
                        {
                            if (parameterTypeCode == TypeCode.Object || parameterTypeCode == TypeCode.String)
                            {
                                methodScore += 0.90;
                                adapterFunc = a => null;
                            }
                            else if (parameterType.IsValueType)
                            {
                                var defaultValue = Activator.CreateInstance(parameterType);
                                adapterFunc = a => defaultValue;
                            }
                            else
                                break;
                        }
                        else if (parameterType == argumentType)
                        {
                            methodScore += 1.00;
                            adapterFunc = a => a;
                        }
                        else if (
                            argumentTypeCode >= TypeCode.SByte && argumentTypeCode <= TypeCode.Decimal &&
                            parameterTypeCode >= TypeCode.SByte && parameterTypeCode <= TypeCode.Decimal)
                        {
                            /* Number to number */
                            if (parameterTypeCode >= argumentTypeCode)
                            {
                                methodScore += 1.00;
                            }
                            else
                            {
                                methodScore += 0.80;
                            }

                            adapterFunc = a => Convert.ChangeType(a, parameterType);
                        }
                        else if (parameterTypeCode == TypeCode.String)
                        {
                            methodScore += 0.60;
                            adapterFunc = a => ObjectFormatter.ToString(a);
                        }
                        else if (parameterType == typeof(Object))
                        {
                            methodScore += 0.70;
                            adapterFunc = a => a;
                        }
                        else
                            break;

                        argumentAdapterFuncs.Add(adapterFunc);
                    }

                    /* Skip incomplete adaptations. */
                    if (argumentAdapterFuncs.Count != methodParameters.Length)
                        continue;

                    /* Decide on the matching. */
                    methodScore = methodScore / Math.Max(methodParameters.Length, arguments == null ? 0 : arguments.Length);
                    if (Double.IsNaN(methodScore) || methodScore == 1.00)
                    {
                        argumentAdapters = argumentAdapterFuncs.ToArray();
                        bestMatchingMethod = method;
                        break;
                    }
                    else if (methodScore > bestMatchingScore)
                    {
                        argumentAdapters = argumentAdapterFuncs.ToArray();
                        bestMatchingScore = methodScore;
                        bestMatchingMethod = method;
                    }
                }

                if (bestMatchingMethod != null)
                {
                    /* Yay, we got one that might do what we want it to do. */
                    getterMethod = (i, a) =>
                    {
                        var adaptedArguments = new object[argumentAdapters.Length];
                        for (var x = 0; x < argumentAdapters.Length; x++)
                        {
                            if (a != null && x < a.Length)
                            {
                                adaptedArguments[x] = argumentAdapters[x](a[x]);
                            } 
                            else
                            {
                                adaptedArguments[x] = argumentAdapters[x](null);
                            }
                        }

                        return bestMatchingMethod.Invoke(i, adaptedArguments);
                    };
                }

                this.cachedMemberMap[signature.ToString()] = getterMethod;
            }

            if (getterMethod != null)
            {
                return getterMethod(@object, arguments);
            }
            else
            { 
                return null;
            }
        }
    }
}
