using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Interpretation
{
    public sealed class PropertyContext
    {
        private PropertyContext m_parentContext;
        private Dictionary<String, Object> m_currentProperties;
        private HashSet<String> m_parentRemovedProperties;
        private Dictionary<Type, TypeMemberAccessor> m_accessors;
        private IEqualityComparer<String> m_equalityComparer;

        private TypeMemberAccessor GetMemberAccesor(Type type)
        {
            Debug.Assert(type != null);

            TypeMemberAccessor accessor;
            lock (m_accessors)
            {
                if (!m_accessors.TryGetValue(type, out accessor))
                {
                    accessor = new TypeMemberAccessor(type, CaseSensitive);
                    m_accessors.Add(type, accessor);
                }
            }

            return accessor;
        }

        private TypeMemberAccessor GetMemberAccesor(Type type)
        {
            Debug.Assert(type != null);

            TypeMemberAccessor accessor;
            lock (m_accessors)
            {
                if (!m_accessors.TryGetValue(type, out accessor))
                {
                    accessor = new TypeMemberAccessor(type, CaseSensitive);
                    m_accessors.Add(type, accessor);
                }
            }

            return accessor;
        }

        public Boolean CaseSensitive { get; private set; }


        private void InitializeContext(IEqualityComparer<String> keyComparer)
        {
            ValidationHelper.AssertArgumentIsNotNull("keyComparer", keyComparer);

            m_parentRemovedProperties = new HashSet<String>(keyComparer);
            m_currentProperties = new Dictionary<String, Object>(keyComparer);
            m_accessors = new Dictionary<Type, TypeMemberAccessor>();
        }

        private PropertyContext(PropertyContext parentContext)
        {
            Debug.Assert(parentContext != null);

            m_parentContext = parentContext;
            CaseSensitive = parentContext.CaseSensitive;

            InitializeContext(parentContext.m_currentProperties.Comparer);
        }

        public PropertyContext(Boolean caseSensitive)
        {
            /* Select a string comparer. */
            IEqualityComparer<String> comparer = 
                caseSensitive ? StringComparer.Ordinal : StringComparer.OrdinalIgnoreCase;

            CaseSensitive = caseSensitive;

            InitializeContext(comparer);
        }

        public PropertyContext Branch()
        {
            return new PropertyContext(this);
        }

        public Object this[String property]
        {
            get
            {
                ValidationHelper.AssertArgumentIsNotAnEmptyString("property", property);

                Object result;
                if (m_currentProperties != null && m_currentProperties.TryGetValue(property, out result))
                    return result;
                else if (m_parentRemovedProperties != null && m_parentRemovedProperties.Contains(property))
                    ValidationHelper.Assert("property", "identifies a valid and stored property", false);
                else if (m_parentContext != null && m_parentContext.Contains(property))
                    return m_parentContext[property];
                else
                    return false;
            }
            set
            {
                ValidationHelper.AssertArgumentIsNotAnEmptyString("property", property);

                Object result;
                if (m_currentProperties != null && m_currentProperties.TryGetValue(property, out result))
                    return result;
                else if (m_parentRemovedProperties != null && m_parentRemovedProperties.Contains(property))
                    return false;
                else if (m_parentContext != null && m_parentContext.Contains(property))
                    return true;
                else
                    return false;
            }
        }

        public Boolean Remove(String property)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("property", property);

            if (m_currentProperties != null && m_currentProperties.Remove(property))
                return true;
            else if (m_parentRemovedProperties != null && m_parentRemovedProperties.Contains(property))
                return false;
            else if (m_parentContext != null && m_parentContext.Contains(property))
            {
                if (m_parentRemovedProperties == null)
                    m_parentRemovedProperties = new HashSet<String>(m_equalityComparer);

                m_parentRemovedProperties.Add(property);
                return true;
            }
            else
                return false;
        }

        public Boolean Contains(String property)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("property", property);

            if (m_currentProperties != null && m_currentProperties.ContainsKey(property))
                return true;
            else if (m_parentRemovedProperties != null && m_parentRemovedProperties.Contains(property))
                return false;
            else if (m_parentContext != null && m_parentContext.Contains(property))
                return true;
            else
                return false;
        }
    }
}
