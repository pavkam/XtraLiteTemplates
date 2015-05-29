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
    using System.Text;
    using System.Threading.Tasks;

    /// <summary>
    /// Utility class that allows for easy access to a type's properties and methods.
    /// This class is used internally to implement the member access operator.
    /// </summary>
    public sealed class SimpleTypeDisemboweler
    {
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private IReadOnlyDictionary<string, Func<object, object>> propertyMap;

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private IReadOnlyDictionary<string, Func<object, object[], object>> methodMap;

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTypeDisemboweler"/> class.
        /// </summary>
        /// <param name="type">The <see cref="System.Type" /> to inspect.</param>
        /// <param name="options">A set of <see cref="EvaluationOptions" /> options.</param>
        /// <param name="memberComparer">An instance of <see cref="IEqualityComparer{String}" /> used when looking up properties in the inspected type.</param>
        /// <exception cref="ArgumentNullException">Either <paramref name="type" /> or <paramref name="memberComparer" /> parameters are <c>null</c>.</exception>
        public SimpleTypeDisemboweler(Type type, EvaluationOptions options, IEqualityComparer<string> memberComparer)
        {
            Expect.NotNull("type", type);
            Expect.NotNull("memberComparer", memberComparer);

            this.Type = type;
            this.Comparer = memberComparer;
            this.Options = options;

            this.propertyMap = this.BuildPropertyMapping();
            this.methodMap = this.BuildMethodMapping();
        }

        /// <summary>
        /// Defines a set of options that guides the <see cref="SimpleTypeDisemboweler"/>.
        /// </summary>
        [Flags]
        public enum EvaluationOptions
        {
            /// <summary>
            /// No special options.
            /// </summary>
            None = 0,

            /// <summary>
            /// Instructs the <see cref="SimpleTypeDisemboweler"/> to treat parameterless, methods to be treated as properties.
            /// The <c>void</c> return type will be treated as <c>null</c>.
            /// </summary>
            [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1650:ElementDocumentationMustBeSpelledCorrectly", Justification = "Not an issue.")]
            TreatParameterlessFunctionsAsProperties = 1,

            /// <summary>
            /// Instructs the <see cref="SimpleTypeDisemboweler"/> to silently return <c>null</c> for any property that raised an exception
            /// during evaluation.
            /// </summary>
            TreatAllErrorsAsNull = 2,
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
        /// Gets the set of <see cref="EvaluationOptions" /> options that modifies the behavior of this
        /// <see cref="SimpleTypeDisemboweler" /> instance.&gt;
        /// </summary>
        /// <value>
        /// The options.
        /// </value>
        /// <remarks>
        /// Value provided by the caller during construction.
        /// </remarks>
        public EvaluationOptions Options { get; private set; }

        /// <summary>
        /// Reads the <paramref name="property" /> of <paramref name="object" />.
        /// </summary>
        /// <param name="object">The object whose property is being read.</param>
        /// <param name="property">The property name.</param>
        /// <returns>The value of the read property.</returns>
        /// <exception cref="ArgumentNullException"><paramref name="property" /> is <c>null</c>. The <paramref name="object" /> is <c>null</c> and this instance of <see cref="SimpleTypeDisemboweler" />
        /// is not instructed to ignore evaluation errors.</exception>
        /// <exception cref="ArgumentException"><paramref name="property" /> is not a valid identifier.</exception>
        /// <exception cref="XtraLiteTemplates.Evaluation.EvaluationException">Any error while reading the property value.</exception>
        public object Read(object @object, string property)
        {
            Expect.Identifier("property", property);

            var ignoreErrors = this.Options.HasFlag(EvaluationOptions.TreatAllErrorsAsNull);

            if (!ignoreErrors)
            {
                Expect.NotNull("object", @object);
            }
            
            if (@object != null)
            {
                Func<object, object> reader;
                if (this.propertyMap.TryGetValue(property, out reader))
                {
                    try
                    {
                        return reader(@object);
                    }
                    catch (Exception e)
                    {
                        if (!ignoreErrors)
                        {
                            ExceptionHelper.ObjectMemberEvaluationError(e, property);
                        }
                    }
                }
                else
                {
                    if (!ignoreErrors)
                    {
                        ExceptionHelper.InvalidObjectMemberName(property);
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Invokes a <paramref name="method" /> of <paramref name="object" /> and returns its value.
        /// </summary>
        /// <param name="object">The object whose method is being invoked.</param>
        /// <param name="method">The invoked method.</param>
        /// <param name="arguments">The arguments.</param>
        /// <returns>
        /// The return value of the method.
        /// </returns>
        /// <exception cref="ArgumentNullException"><paramref name="method" /> is <c>null</c>. The <paramref name="object" /> is <c>null</c> and this instance of <see cref="SimpleTypeDisemboweler" />
        /// is not instructed to ignore evaluation errors.</exception>
        /// <exception cref="ArgumentException"><paramref name="method" /> is not a valid identifier.</exception>
        /// <exception cref="XtraLiteTemplates.Evaluation.EvaluationException">Any error while invoking method.</exception>
        public object Invoke(object @object, string method, object[] arguments)
        {
            Expect.Identifier("method", method);

            var ignoreErrors = this.Options.HasFlag(EvaluationOptions.TreatAllErrorsAsNull);

            if (!ignoreErrors)
            {
                Expect.NotNull("object", @object);
            }

            if (@object != null)
            {
                Func<object, object[], object> reader;
                if (this.methodMap.TryGetValue(method, out reader))
                {
                    try
                    {
                        return reader(@object, arguments);
                    }
                    catch (Exception e)
                    {
                        if (!ignoreErrors)
                        {
                            ExceptionHelper.ObjectMemberEvaluationError(e, method);
                        }
                    }
                }
                else
                {
                    if (!ignoreErrors)
                    {
                        ExceptionHelper.InvalidObjectMemberName(method);
                    }
                }
            }

            return null;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private object[] ReconcileArguments(Type[] expectedArgumentTypes, object[] actualArguments)
        {
            Debug.Assert(expectedArgumentTypes != null, "expectedArgumentTypes cannot be null.");
            object[] result = new object[expectedArgumentTypes.Length];

            for (var argumentIndex = 0; argumentIndex < expectedArgumentTypes.Length; argumentIndex++)
            {
                Type expectedType = expectedArgumentTypes[argumentIndex];
                if (actualArguments.Length <= argumentIndex)
                {
                    /* No actual argument at that position. Default to NULL. */
                    result[argumentIndex] = null;
                }
                else if (expectedType == typeof(object) || actualArguments[argumentIndex] == null)
                {
                    result[argumentIndex] = actualArguments[argumentIndex];
                }
                else
                {
                    try
                    {
                        result[argumentIndex] = Convert.ChangeType(actualArguments[argumentIndex], expectedType);
                    }
                    catch (Exception e)
                    {
                        if (this.Options.HasFlag(EvaluationOptions.TreatAllErrorsAsNull))
                        {
                            result[argumentIndex] = null;
                        }
                        else
                        {
                            throw e;
                        }
                    }
                }
            }

            return result;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private IReadOnlyDictionary<string, Func<object, object>> BuildPropertyMapping()
        {
            var mapping = new Dictionary<string, Func<object, object>>(this.Comparer);

            /* Load properties in. */
            foreach (var property in Type.GetProperties())
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                {
                    continue;
                }

                mapping[property.Name] = property.GetValue;
            }

            if (this.Options.HasFlag(EvaluationOptions.TreatParameterlessFunctionsAsProperties))
            {
                /* Load methods in. */
                foreach (var method in Type.GetMethods())
                {
                    if (method.GetParameters().Length > 0 || method.IsAbstract || method.IsConstructor ||
                        method.IsPrivate)
                    {
                        continue;
                    }

                    if (!mapping.ContainsKey(method.Name))
                    {
                        mapping[method.Name] = instance => method.Invoke(instance, null);
                    }
                }
            }

            return mapping;
        }

        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1600:ElementsMustBeDocumented", Justification = "Not documenting internal entities.")]
        private IReadOnlyDictionary<string, Func<object, object[], object>> BuildMethodMapping()
        {
            var mapping = new Dictionary<string, Func<object, object[], object>>(this.Comparer);
            var scoreMapping = new Dictionary<string, double>(this.Comparer);

            /* Load methods in. */
            foreach (var method in Type.GetMethods())
            {
                if (method.IsAbstract || method.IsConstructor || method.IsPrivate)
                {
                    continue;
                }

                var methodParameters = method.GetParameters();
                foreach (var parameter in methodParameters)
                {
                    /* Scan for unsupported method parameter types. */
                    if (parameter.IsRetval || parameter.IsOut)
                    {
                        continue;
                    }
                }

                /* Get the method arguments and calculate the matching score. */
                double score = 0;
                var methodArgumentTypes = methodParameters.Select(s => s.ParameterType).ToArray();
                if (methodArgumentTypes.Length == 0)
                {
                    score = 1;
                }
                else
                {
                    foreach (var argumentType in methodArgumentTypes)
                    {
                        if (argumentType == typeof(object))
                        {
                            score += 1.00;
                        }
                        else if (argumentType == typeof(string))
                        {
                            score += 0.75;
                        }
                        else if (argumentType == typeof(double))
                        {
                            score += 0.50;
                        }
                    }

                    score = score / methodArgumentTypes.Length;
                }

                double previousScore;
                if (!scoreMapping.TryGetValue(method.Name, out previousScore) || previousScore < score)
                { 
                    Func<object, object[], object> invokationFunc = (@object, arguments) =>
                    {
                        var actualArguments = ReconcileArguments(methodArgumentTypes, arguments);
                        return method.Invoke(@object, actualArguments);
                    };

                    mapping[method.Name] = invokationFunc;
                    scoreMapping[method.Name] = score;
                }
            }

            return mapping;
        }
    }
}
