//  Author:
//    Alexandru Ciobanu alex+git@ciobanu.org
//
//  Copyright (c) 2015-2016, Alexandru Ciobanu (alex+git@ciobanu.org)
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
    using System.Collections;
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
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
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
        /// The event handler invoked by this class for each candidate type member. The registered delegates
        /// are responsible with deciding if a member will be accepted or rejected during candidate selection.
        /// </summary>
        public event EventHandler<MemberValidationEventArgs> ValidateMember;

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
            return this.Invoke(@object, member, null);
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
            {
                return null;
            }

            /* Create the signature of the member. */
            StringBuilder signature = new StringBuilder();
            signature.Append(member);

            if (arguments != null && arguments.Length > 0)
            {
                foreach (var a in arguments)
                {
                    signature.Append('#');
                    if (a == null)
                    {
                        signature.Append('?');
                    }
                    else
                    {
                        signature.Append(a.GetType().Name);
                    }
                }
            }

            /* Find a cached member from a previous lookup. */
            Func<object, object[], object> getterMethod;
            if (!this.cachedMemberMap.TryGetValue(signature.ToString(), out getterMethod))
            {
                /* No cache made for this call structure. Do it now and cache the invoke candidate. */
                getterMethod = this.LocateSuitableInvokeCandidate(@object, member, arguments);
                this.cachedMemberMap[signature.ToString()] = getterMethod;
            }

            /* Invoke the member if getter was created. */
            if (getterMethod != null)
            {
                return getterMethod(@object, arguments);
            }
            else
            { 
                return null;
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private bool ValidateCandidate(MemberInfo memberInfo)
        {
            Debug.Assert(memberInfo != null, "Argument memberInfo cannot be null.");

            var candidateAccepted = true;
            if (this.ValidateMember != null)
            {
                var eventArgs = new MemberValidationEventArgs(memberInfo);
                this.ValidateMember(this, eventArgs);

                candidateAccepted = eventArgs.Accepted;
            }

            return candidateAccepted;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private Func<object, object[], object> LocateSuitableInvokeCandidate(object @object, string member, object[] arguments)
        {
            if (arguments == null || arguments.Length == 0)
            {
                /* Scan the type for properties. */
                foreach (var property in Type.GetProperties())
                {
                    if (!this.Comparer.Equals(property.Name, member) || 
                        !property.CanRead || 
                        property.GetIndexParameters().Length > 0 ||
                        !this.ValidateCandidate(property))
                    {
                        continue;
                    }

                    /* Found a matching property! */
                    return (i, a) => property.GetValue(i);
                }

                /* Now scan for fields. */
                foreach (var field in this.Type.GetFields())
                {
                    if (!this.Comparer.Equals(field.Name, member) || 
                        field.IsPrivate ||
                        !this.ValidateCandidate(field))
                    {
                        continue;
                    }

                    /* Found a matching property! */
                    return (i, a) => field.GetValue(i);
                }
            }

            /* Try to find a suitable method candidate. */
            return this.LocateSuitableMethodCandidate(@object, member, arguments);
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private Func<object, object[], object> LocateSuitableMethodCandidate(object @object, string member, object[] arguments)
        {
            Debug.Assert(@object != null, "object cannot be null.");
            Debug.Assert(!string.IsNullOrEmpty(member), "member cannot be null or empty.");

            MethodInfo bestMatchingMethod = null;
            Func<object, object>[] argumentAdapters = null;
            var bestMatchingScore = .0;
            var indexOfParamsArrayInMethod = -1;
            Type argsArrayElementType = null;

            /* Scan for methods! */
            foreach (var method in Type.GetMethods())
            {
                if (!this.Comparer.Equals(method.Name, member) || 
                    method.IsAbstract || 
                    method.IsConstructor || 
                    method.IsPrivate ||
                    !this.ValidateCandidate(method))
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

                var methodScore = .0;
                var indexOfParamsArray = -1;

                List<Func<object, object>> argumentAdapterFuncs = new List<Func<object, object>>();
                for (var parameterIndex = 0; parameterIndex < methodParameters.Length; parameterIndex++)
                {
                    var parameter = methodParameters[parameterIndex];

                    Func<object, object> adapterFunc = null;

                    if (arguments == null || arguments.Length <= parameterIndex)
                    {
                        adapterFunc = this.ReconcileMissingArgument(parameter, ref methodScore);
                    }
                    else
                    {
                        var parameterType = parameter.ParameterType;
                        if (parameterIndex == methodParameters.Length - 1 && parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
                        {
                            /* This is the last parameter and it is also a params array[]. Treat it differently. 
                             */
                            Debug.Assert(parameterType.IsArray, "parameterType must be an array.");
                            var elementType = parameterType.GetElementType();
                            indexOfParamsArray = parameterIndex;
                            for (var argsIndex = parameterIndex; argsIndex < arguments.Length; argsIndex++)
                            {
                                adapterFunc = this.ReconcileArgument(elementType, arguments[argsIndex], ref methodScore);
                                if (adapterFunc != null)
                                {
                                    methodScore -= 0.05; /* Remove a bit of score since it's in a params array. */
                                    argumentAdapterFuncs.Add(adapterFunc);
                                }
                                else
                                {
                                    /* Whoops, fail to reconcile. */
                                    break;
                                }
                            }

                            if (adapterFunc != null)
                            {
                                /* Very special case here. */
                                argsArrayElementType = elementType;
                                continue;
                            }
                        }
                        else
                        {
                            adapterFunc = this.ReconcileArgument(parameterType, arguments[parameterIndex], ref methodScore);
                        }
                    }

                    /* Die if not matching. */
                    if (adapterFunc == null)
                    {
                        break;
                    }
                    else
                    {
                        argumentAdapterFuncs.Add(adapterFunc);
                    }
                }

                /* Skip incomplete adaptations. */
                if (argumentAdapterFuncs.Count < methodParameters.Length)
                {
                    continue;
                }

                /* Decide on the matching. */
                var argumentCount = arguments != null ? arguments.Length : 0;
                if (methodParameters.Length > argumentCount)
                {
                    methodScore /= methodParameters.Length;
                }
                else
                {
                    methodScore /= argumentCount;
                }

                if (double.IsNaN(methodScore) || methodScore == 1.00)
                {
                    indexOfParamsArrayInMethod = indexOfParamsArray;
                    argumentAdapters = argumentAdapterFuncs.ToArray();
                    bestMatchingMethod = method;
                    break;
                }
                else if (methodScore >= bestMatchingScore)
                {
                    indexOfParamsArrayInMethod = indexOfParamsArray;
                    argumentAdapters = argumentAdapterFuncs.ToArray();
                    bestMatchingScore = methodScore;
                    bestMatchingMethod = method;
                }
            }

            if (bestMatchingMethod != null)
            {
                /* Yay, we got one that might do what we want it to do. */
                if (indexOfParamsArrayInMethod >= 0)
                {
                    /* We have a params array at the end. Special treatment is a must here. */
                    Debug.Assert(indexOfParamsArrayInMethod < argumentAdapters.Length, "params argument is never out of the index bounds.");

                    return (i, a) =>
                    {
                        /* Normal argument [0...params) */
                        var adaptedArguments = new object[indexOfParamsArrayInMethod + 1];
                        for (var x = 0; x < indexOfParamsArrayInMethod; x++)
                        {
                            adaptedArguments[x] = argumentAdapters[x](a[x]);
                        }

                        /* Params argument [params...end] */
                        IList paramsArgument = (IList)Array.CreateInstance(argsArrayElementType, argumentAdapters.Length - indexOfParamsArrayInMethod);
                        for (var x = indexOfParamsArrayInMethod; x < argumentAdapters.Length; x++)
                        {
                            paramsArgument[x - indexOfParamsArrayInMethod] = argumentAdapters[x](a[x]);
                        }

                        adaptedArguments[indexOfParamsArrayInMethod] = paramsArgument;

                        return bestMatchingMethod.Invoke(i, adaptedArguments);
                    };
                }
                else
                {
                    /* Standard function mapping. */
                    return (i, a) =>
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
            }
            else
            {
                /* Found nothing :( return a big fat NULL. */
                return null;
            }
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private Func<object, object> ReconcileMissingArgument(ParameterInfo parameter, ref double reconciliationScore)
        {
            Debug.Assert(parameter != null, "parameter cannot be null.");

            object defaultValue = null;

            /* No more input arguments left in the input list. We'll need to default to something. */
            if (parameter.HasDefaultValue)
            {
                /* OK, we have a default value! Use it. */
                reconciliationScore += 1.00;
                defaultValue = parameter.DefaultValue;
            }
            else if (parameter.GetCustomAttribute<ParamArrayAttribute>() != null)
            {
                /* It's a "param" array. Use a default elementless one. */
                reconciliationScore += 1.00;
                defaultValue = Array.CreateInstance(parameter.ParameterType.GetElementType(), 0);
            }
            else
            {
                /* Not matching anything. */
                if (parameter.ParameterType.IsValueType)
                {
                    reconciliationScore += 0.10;
                    defaultValue = Activator.CreateInstance(parameter.ParameterType);
                }
                else if (parameter.ParameterType == typeof(string))
                {
                    reconciliationScore += 0.20;
                }
                else if (parameter.ParameterType == typeof(object))
                {
                    reconciliationScore += 0.25;
                }
                else
                {
                    reconciliationScore += 0.15;
                }
            }

            return a => defaultValue;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private Func<object, object> ReconcileArgument(Type parameterType, object argument, ref double reconciliationScore)
        {
            Debug.Assert(parameterType != null, "parameterType cannot be null.");

            /* Now the real work begins. Trying to reconcile the passed-in argument type with the expected parameter type. */
            var argumentType = argument != null ? argument.GetType() : null;

            if (parameterType == argumentType)
            {
                /* Perfect match. */
                reconciliationScore += 1.00;
                return a => a;
            }
            else if (parameterType == typeof(string))
            {
                /* Expected is string. We'll convert. */
                reconciliationScore += 0.60;
                return a => this.ObjectFormatter.ToString(a);
            }
            else if (parameterType == typeof(object))
            {
                /* Expected is an untyped object. Anything can be accepted in this case. */
                reconciliationScore += 0.70;
                return a => a;
            }
            else
            {
                var parameterTypeCode = Type.GetTypeCode(parameterType);
                var argumentTypeCode = Type.GetTypeCode(argumentType);

                if (argumentTypeCode >= TypeCode.SByte && argumentTypeCode <= TypeCode.Decimal &&
                    parameterTypeCode >= TypeCode.SByte && parameterTypeCode <= TypeCode.Decimal)
                {
                    /* Number to number */
                    if (parameterTypeCode >= argumentTypeCode)
                    {
                        reconciliationScore += 1.00;
                    }
                    else
                    {
                        reconciliationScore += 0.80;
                    }

                    return a => Convert.ChangeType(a, parameterType);
                }
            }

            /* Impossible case. Bail! */
            return null;
        }
    }
}
