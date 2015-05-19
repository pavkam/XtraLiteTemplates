using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Dialects.Standard
{
    public sealed class SimpleTypeDisemboweler
    {
        [Flags]
        public enum EvaluationOptions
        {
            None = 0,
            TreatParameterlessFunctionsAsProperties = 1,
            TreatAllErrorsAsNull = 2,
        }
        
        private IReadOnlyDictionary<String, Func<Object, Object>> m_mapping;

        public Type Type { get; private set; }
        public IEqualityComparer<String> Comparer { get; private set; }
        public EvaluationOptions Options { get; private set; }

        public SimpleTypeDisemboweler(Type type, EvaluationOptions options, IEqualityComparer<String> memberComparer)
        {
            Expect.NotNull("type", type);
            Expect.NotNull("memberComparer", memberComparer);

            Type = type;
            Comparer = memberComparer;
            Options = options;

            m_mapping = BuildMapping();
        }

        private IReadOnlyDictionary<String, Func<Object, Object>> BuildMapping()
        {
            var mapping = new Dictionary<String, Func<Object, Object>>(Comparer);

            /* Load properties in. */
            foreach (var property in Type.GetProperties())
            {
                if (!property.CanRead || property.GetIndexParameters().Length > 0)
                    continue;

                mapping[property.Name] = property.GetValue;
            }

            if (Options.HasFlag(EvaluationOptions.TreatParameterlessFunctionsAsProperties))
            {
                var zeroParams = new Object[0];

                /* Load methods in. */
                foreach (var method in Type.GetMethods())
                {
                    if (method.GetParameters().Length > 0 || method.IsAbstract || method.IsConstructor || 
                        method.IsPrivate)
                        continue;

                    if (!mapping.ContainsKey(method.Name))
                        mapping[method.Name] = instance => method.Invoke(instance, zeroParams);
                }
            }

            return mapping;
        }

        public Object Read(String property, Object instance)
        {
            Expect.Identifier("property", property);

            var ignoreErrors = Options.HasFlag(EvaluationOptions.TreatAllErrorsAsNull);

            if (!ignoreErrors)
                Expect.NotNull("instance", instance);
            
            if (instance != null)
            {
                Func<Object, Object> reader;
                if (m_mapping.TryGetValue(property, out reader))
                {
                    try
                    {
                        return reader(instance);
                    }
                    catch (Exception e)
                    {
                        if (!ignoreErrors)
                            ExceptionHelper.ObjectMemberEvaluationError(e, property);
                    }
                }
                else
                {
                    if (!ignoreErrors)
                        ExceptionHelper.InvalidObjectMemberName(property);
                }
            }

            return null;
        }
    }
}
