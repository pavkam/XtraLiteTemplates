using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XtraLiteTemplates.Utils
{
    public class BranchedDictionary<TKey, TValue> : 
        IDictionary<TKey, TValue>, IReadOnlyDictionary<TKey, TValue>, IDictionary
    {
        private sealed class KeyCollection : ICollection<TKey>, ICollection
        {
            private BranchedDictionary<TKey, TValue> m_owner;

            public KeyCollection(BranchedDictionary<TKey, TValue> dictionary)
            {
                Debug.Assert(dictionary != null);
                m_owner = dictionary;
            }

            public Int32 Count
            {
                get
                {
                    return m_owner.Count;
                }
            }

            public void CopyTo(TKey[] array, int index)
            {

            }

            public Dictionary<TKey, TValue>.KeyCollection.Enumerator GetEnumerator();
            public struct Enumerator : IEnumerator<TKey>, IEnumerator, IDisposable
            {
                public TKey Current { get; }
                public void Dispose();
                public bool MoveNext();
            }

            public void Add(TKey item)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public bool Contains(TKey item)
            {
                throw new NotImplementedException();
            }

            public bool IsReadOnly
            {
                get { throw new NotImplementedException(); }
            }

            public bool Remove(TKey item)
            {
                throw new NotImplementedException();
            }

            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }
        }













        private BranchedDictionary<TKey, TValue> m_parentDictionary;
        private Dictionary<TKey, TValue> m_dictionary;
        private HashSet<TKey> m_removed;

        private BranchedDictionary(BranchedDictionary<TKey, TValue> parent)
        {
            Debug.Assert(parent != null);

            m_parentDictionary = parent;
            m_dictionary = new Dictionary<TKey, TValue>(parent.Comparer);
            m_removed = new HashSet<TKey>();
        }

        public BranchedDictionary()
            : this(EqualityComparer<TKey>.Default)
        {
        }

        public BranchedDictionary(IEqualityComparer<TKey> comparer)
        {
            ValidationHelper.AssertArgumentIsNotNull("comparer", comparer);

            m_dictionary = new Dictionary<TKey, TValue>(comparer);
            m_removed = new HashSet<TKey>();
        }

        public BranchedDictionary(IReadOnlyDictionary<TKey, TValue> dictionary) : this()
        {
            ValidationHelper.AssertArgumentIsNotNull("dictionary", dictionary);
            foreach (var kvp in dictionary)
                Add(kvp);
        }

        public BranchedDictionary(IReadOnlyDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer)
            : this(comparer)
        {
            ValidationHelper.AssertArgumentIsNotNull("dictionary", dictionary);
            foreach (var kvp in dictionary)
                Add(kvp);
        }

        public IEqualityComparer<TKey> Comparer
        { 
            get
            {
                return m_dictionary.Comparer;
            }
        }

        public BranchedDictionary<TKey, TValue> AddBranch(IReadOnlyDictionary<TKey, TValue> dictionary)
        {
            ValidationHelper.AssertArgumentIsNotNull("dictionary", dictionary);

            var branch = AddBranch();
            foreach (var kvp in dictionary)
                branch.Add(kvp);

            return branch;
        }

        public BranchedDictionary<TKey, TValue> AddBranch()
        {
            return new BranchedDictionary<TKey, TValue>(this);
        }



        public void Add(TKey key, TValue value)
        {
            ValidationHelper.AssertArgumentIsNotNull("key", key);
            ValidationHelper.Assert("key", "Key not present in the dictionary or its parent branch.", !ContainsKey(key));

            m_dictionary.Add(key, value);
            m_removed.Remove(key);
        }

        public Boolean ContainsKey(TKey key)
        {
            ValidationHelper.AssertArgumentIsNotNull("key", key);

            if (m_removed.Contains(key))
                return false;
            else
                return m_dictionary.ContainsKey(key) || m_parentDictionary.ContainsKey(key);
        }

        public Boolean Remove(TKey key)
        {
            ValidationHelper.AssertArgumentIsNotNull("key", key);

            var removed = m_dictionary.Remove(key);
            if (!removed && !m_removed.Contains(key))
            {
                /* No such value in *this* branch. Check the parent branch and mark as deleted if found. */
                removed = m_parentDictionary.ContainsKey(key);
                if (removed)
                    m_removed.Add(key);
            }

            return removed;
        }

        public Boolean TryGetValue(TKey key, out TValue value)
        {
            ValidationHelper.AssertArgumentIsNotNull("key", key);

            if (m_dictionary.TryGetValue(key, out value))
                return true;
            else if (!m_removed.Contains(key) && m_parentDictionary.TryGetValue(key, out value))
                return true;
            else
            {
                value = default(TValue);
                return false;
            }
        }

        public ICollection<TKey> Keys
        {
            get 
            {
                return this.Select(kvp => kvp.Key)
            }
        }

        public ICollection<TValue> Values
        {
            get { throw new NotImplementedException(); }
        }

        public TValue this[TKey key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void Add(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(KeyValuePair<TKey, TValue> item)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        public bool IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        public object SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        public void Add(object key, object value)
        {
            throw new NotImplementedException();
        }

        public bool Contains(object key)
        {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public bool IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        ICollection IDictionary.Keys
        {
            get { throw new NotImplementedException(); }
        }

        public void Remove(object key)
        {
            throw new NotImplementedException();
        }

        ICollection IDictionary.Values
        {
            get { throw new NotImplementedException(); }
        }

        public object this[object key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys
        {
            get { throw new NotImplementedException(); }
        }

        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values
        {
            get { throw new NotImplementedException(); }
        }
    }
}
