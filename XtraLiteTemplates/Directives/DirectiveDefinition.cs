using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Directives
{
    public sealed class DirectiveDefinition
    {
        private IList<DirectiveComponent> m_components;
        private ISet<String> m_usedKeys;
        
        private void CheckAndRegisterKey(String key)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("key", key);

            if (m_usedKeys.Contains(key))
                throw new ArgumentException(String.Format("A directive component with the key '{0}' already defined.", key));

            m_usedKeys.Add(key);
        }

        public DirectiveDefinition()
        {
            m_components = new List<DirectiveComponent>();
            m_usedKeys = new HashSet<String>();
        }

        internal DirectiveComponent this[Int32 index]
        {
            get
            {
                ValidationHelper.AssertArgumentGreaterThanZero("index", index);
                if (index >= m_components.Count)
                    return null;

                return m_components[index];
            }
        }

        public DirectiveDefinition ExpectKeyword(String keyword)
        {
            ValidationHelper.AssertArgumentIsNotAnEmptyString("keyword", keyword);

            m_components.Add(new DirectiveComponent(null, DirectiveComponent.DirectiveComponentType.Keyword, keyword.ToUpper()));

            return this;
        }

        public DirectiveDefinition ExpectConstant(String key)
        {
            CheckAndRegisterKey(key);

            m_components.Add(new DirectiveComponent(key, DirectiveComponent.DirectiveComponentType.Constant, null));

            return this;
        }

        public DirectiveDefinition ExpectVariable(String key)
        {
            CheckAndRegisterKey(key);

            m_components.Add(new DirectiveComponent(key, DirectiveComponent.DirectiveComponentType.Variable, null));

            return this;
        }

        public DirectiveDefinition ExpectIdentifier(String key)
        {
            CheckAndRegisterKey(key);

            m_components.Add(new DirectiveComponent(key, DirectiveComponent.DirectiveComponentType.Constant, null));

            return this;
        }
    }
}
