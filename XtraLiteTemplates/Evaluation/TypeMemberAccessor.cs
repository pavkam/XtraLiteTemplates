using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Evaluation
{
    public sealed class TypeMemberAccessor
    {
        private IDictionary<String, Func<Object, Object>> m_readers;

        public TypeMemberAccessor(Type type, IEqualityComparer<String> comparer)
        {
            ValidationHelper.AssertArgumentIsNotNull("type", type);
            ValidationHelper.AssertArgumentIsNotNull("comparer", comparer);

            Comparer = comparer;

            /* Simply scan the type for properties and parameterless methods. */
            var m_readers = new Dictionary<String, Func<Object, Object>>(Comparer);

            foreach (var field in type.GetRuntimeFields())
            {
                if (field.IsPublic)
                    m_readers[field.Name] = target => field.GetValue(target);
            }
            foreach (var property in type.GetRuntimeProperties())
            {
                if (property.CanRead && property.GetIndexParameters().Length == 0)
                    m_readers[property.Name] = target => property.GetValue(target);
            }
            foreach (var method in type.GetRuntimeMethods())
            {
                if (method.IsPublic && method.ReturnParameter != null && method.GetParameters().Length == 0)
                    m_readers[method.Name] = target => method.Invoke(target, null);
            }
        }

        public IEqualityComparer<String> Comparer { get; private set; }

        public Object Read(Object target, String propertyName, Object defaultValue = null)
        {
            Debug.Assert(target != null);
            Debug.Assert(!String.IsNullOrEmpty(propertyName));

            Func<Object, Object> reader;
            if (m_readers.TryGetValue(propertyName, out reader))
                return reader(target);
            else
                return defaultValue;
        }

        public String Read(Object target, String propertyName, String defaultValue = null)
        {
            Object value = Read(target, propertyName, defaultValue);
            if (value != null)
                return value.ToString();
            else
                return null;
        }
    }
}
