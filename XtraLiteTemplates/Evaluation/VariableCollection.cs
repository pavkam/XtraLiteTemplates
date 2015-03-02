using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using XtraLiteTemplates.Utils;

namespace XtraLiteTemplates.Evaluation
{
    public sealed class VariableCollection
    {
        private VariableCollection m_parentContext;
        private Dictionary<String, Tuple<Object, Object>> m_currentProperties;
        private HashSet<String> m_parentRemovedProperties;

        private VariableCollection(VariableCollection parentContext)
        {
            m_parentContext = parentContext;
        }

        public VariableCollection(IEqualityComparer<String> comparer)
        {
            ValidationHelper.AssertArgumentIsNotNull("comparer", comparer);

            Comparer = comparer;
        }

        public VariableCollection()
            : this(StringComparer.Ordinal)
        {
        }

        public IEqualityComparer<String> Comparer { get; private set; }

        public VariableCollection Branch()
        {
            return new VariableCollection(this);
        }

        public void Set(String name, Object value, Object context)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("name", name);

            /* Simple */
            if (m_parentRemovedProperties != null)
                m_parentRemovedProperties.Remove(name);

            if (m_currentProperties == null)
                m_currentProperties = new Dictionary<String, Tuple<Object, Object>>();

            m_currentProperties[name] = Tuple.Create(value, context);
        }

        public Object Get(String name, out Object context)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("name", name);

            Tuple<Object, Object> result;
            if (m_currentProperties != null && m_currentProperties.TryGetValue(name, out result))
            {
                context = result.Item2;
                return result.Item1;
            }
            else if (m_parentRemovedProperties != null && m_parentRemovedProperties.Contains(name))
                ValidationHelper.Assert("name", "identifies a valid and stored property", false);
            else if (m_parentContext != null && m_parentContext.Contains(name))
            {
                return m_parentContext.Get(name, out context);
            }
            else
                ValidationHelper.Assert("name", "identifies a valid and stored property", false);

            context = null;
            return null;
        }

        public Object Get(String name)
        {
            Object dummy;
            return Get(name, out dummy);
        }

        public Boolean Remove(String name)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("name", name);

            if (m_currentProperties != null && m_currentProperties.Remove(name))
                return true;
            else if (m_parentRemovedProperties != null && m_parentRemovedProperties.Contains(name))
                return false;
            else if (m_parentContext != null && m_parentContext.Contains(name))
            {
                if (m_parentRemovedProperties == null)
                    m_parentRemovedProperties = new HashSet<String>(m_equalityComparer);

                m_parentRemovedProperties.Add(name);
                return true;
            }
            else
                return false;
        }

        public Boolean Contains(String name)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("property", name);

            if (m_currentProperties != null && m_currentProperties.ContainsKey(name))
                return true;
            else if (m_parentRemovedProperties != null && m_parentRemovedProperties.Contains(name))
                return false;
            else if (m_parentContext != null && m_parentContext.Contains(name))
                return true;
            else
                return false;
        }
    }
}
