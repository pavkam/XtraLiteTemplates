using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

namespace XtraLiteTemplates.Interpretation
{
    internal sealed class SimpleTypeWrapper
    {
        private IReadOnlyDictionary<String, PropertyInfo> m_properties;
        private Boolean m_caseSensitive;

        internal SimpleTypeWrapper(Type type, Boolean caseSensitive)
        {
            Debug.Assert(type != null);

            /* Simply scan the type for properties and parameterless methods. */
            var collectedProperties = new Dictionary<String, PropertyInfo>();
            var properties = type.GetRuntimeProperties();
            foreach (var property in properties)
            {
                if (property.CanRead && property.GetIndexParameters().Length == 0)
                {
                    if (caseSensitive)
                        collectedProperties[property.Name] = property;
                    else
                        collectedProperties[property.Name.ToUpper()] = property;
                }
            }

            m_caseSensitive = caseSensitive;
            m_properties = collectedProperties;
        }

        internal Object Read(Object target, String propertyName, Object defaultValue = null)
        {
            Debug.Assert(target != null);
            Debug.Assert(!String.IsNullOrEmpty(propertyName));

            if (!m_caseSensitive)
                propertyName = propertyName.ToUpper();

            PropertyInfo property;
            if (m_properties.TryGetValue(propertyName, out property))
            {
                return property.GetValue(target);
            }
            else
                return defaultValue;
        }

        internal String Read(Object target, String propertyName, String defaultValue = null)
        {
            Debug.Assert(target != null);
            Debug.Assert(!String.IsNullOrEmpty(propertyName));

            if (!m_caseSensitive)
                propertyName = propertyName.ToUpper();

            PropertyInfo property;
            if (m_properties.TryGetValue(propertyName, out property))
            {
                return property.GetValue(target);
            }
            else
                return defaultValue;
        }
    }
}
