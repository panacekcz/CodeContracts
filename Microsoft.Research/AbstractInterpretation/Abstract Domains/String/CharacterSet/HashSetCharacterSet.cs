﻿// CodeContracts
// 
// Copyright 2016-2017 Charles University
// 
// All rights reserved. 
// 
// MIT License
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// Created by Vlastimil Dort (2016-2017)

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
