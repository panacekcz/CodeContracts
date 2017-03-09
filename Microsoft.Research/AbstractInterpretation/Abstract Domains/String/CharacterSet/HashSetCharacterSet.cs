using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;

namespace Microsoft.Research.AbstractDomains.Strings
{
    public struct HashCharacterSet : ICharacterSet<HashCharacterSet>
    {
        private HashSet<int> hashSet;

        internal HashCharacterSet(HashSet<int> hashSet)
        {
            Contract.Requires(hashSet != null);

            this.hashSet = hashSet;
        }

        public int Count
        {
            get
            {
                return hashSet.Count;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return hashSet.Count == 0;
            }
        }

        public bool IsSingleton
        {
            get
            {
                return hashSet.Count == 1;
            }
        }

        public void Add(int value)
        {
            hashSet.Add(value);
        }

        public bool Contains(int characterClass)
        {
            return hashSet.Contains(characterClass);
        }

        public HashCharacterSet Create(bool full, int size)
        {
            HashSet<int> set;
            if (full)
                set = new HashSet<int>(Enumerable.Range(0, size));
            else
                set = new HashSet<int>();

            return new HashCharacterSet(set);
        }

        public HashCharacterSet Empty()
        {
            return new HashCharacterSet(new HashSet<int>());
        }

        public bool Equals(HashCharacterSet other)
        {
            return HashSet<int>.CreateSetComparer().Equals(hashSet, other.hashSet);
        }

        public HashCharacterSet Except(HashCharacterSet set)
        {
            HashSet<int> newHashSet = new HashSet<int>(hashSet);
            newHashSet.ExceptWith(set.hashSet);
            return new HashCharacterSet(newHashSet);
        }

        public HashCharacterSet Intersection(HashCharacterSet set)
        {
            HashSet<int> newHashSet = new HashSet<int>(hashSet);
            newHashSet.IntersectWith(set.hashSet);
            return new HashCharacterSet(newHashSet);
        }

        public bool Intersects(HashCharacterSet set)
        {
            return hashSet.Overlaps(set.hashSet);
        }

        public void IntersectWith(HashCharacterSet set)
        {
            hashSet.IntersectWith(set.hashSet);
        }
        public void ExceptWith(HashCharacterSet set)
        {
            hashSet.ExceptWith(set.hashSet);
        }

        public void Invert(int size)
        {
            HashSet<int> newHashSet = new HashSet<int>(Enumerable.Range(0, size));
            newHashSet.ExceptWith(hashSet);
            hashSet = newHashSet;
        }

        public HashCharacterSet Inverted(int size)
        {
            HashCharacterSet newSet = Create(true, size);
            newSet.ExceptWith(this);
            return newSet;
        }

        public bool IsFull(int size)
        {
            return hashSet.Count == size;
        }

        public bool IsSubset(HashCharacterSet set)
        {
            return hashSet.IsSubsetOf(set.hashSet);
        }

        public HashCharacterSet MutableClone()
        {
            HashSet<int> newHashSet = new HashSet<int>(hashSet);
            return new HashCharacterSet(newHashSet);
        }

        public void Remove(int value)
        {
            hashSet.Remove(value);
        }

        public HashCharacterSet Union(HashCharacterSet set)
        {
            HashSet<int> newHashSet = new HashSet<int>(hashSet);
            newHashSet.UnionWith(set.hashSet);
            return new HashCharacterSet(newHashSet);
        }

        public void UnionWith(HashCharacterSet set)
        {
            hashSet.UnionWith(set.hashSet);
        }

        public HashCharacterSet With(int value)
        {
            HashSet<int> newHashSet = new HashSet<int>(hashSet);
            newHashSet.Add(value);
            return new HashCharacterSet(newHashSet);
        }

        public HashCharacterSet Without(int value)
        {
            HashSet<int> newHashSet = new HashSet<int>(hashSet);
            newHashSet.Remove(value);
            return new HashCharacterSet(newHashSet);
        }
    }

}
