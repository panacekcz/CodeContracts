using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    public interface ICharacterSetFactory<CharacterSet>
          where CharacterSet : ICharacterSet<CharacterSet>
    {
        CharacterSet Create(bool value, int size);
        CharacterSet CreateSingleton(char value, int size);
    }

    public class BitArrayCharacterSetFactory : ICharacterSetFactory<BitArrayCharacterSet>
    {
        public BitArrayCharacterSet Create(bool value, int size)
        {
            return new BitArrayCharacterSet(value, size);
        }

        public BitArrayCharacterSet CreateSingleton(char value, int size)
        {
            throw new NotImplementedException();
        }
    }

    public interface ICharacterSet<CharacterSet> : IEquatable<CharacterSet>
        where CharacterSet : ICharacterSet<CharacterSet>
    {
        bool IsEmpty { get; }
        bool IsFull(int size);
        int Count { get; }
        bool IsSingleton { get; }

        bool Contains(int characterClass);
        bool Intersects(CharacterSet set);
        bool IsSubset(CharacterSet set);

        CharacterSet Union(CharacterSet set);
        CharacterSet Intersection(CharacterSet set);
        CharacterSet Except(CharacterSet set);

        CharacterSet With(int value);
        CharacterSet Without(int value);

        CharacterSet Empty();
        CharacterSet Create(bool full, int size);
        CharacterSet MutableClone();
        CharacterSet Inverted(int size);

        void Add(int value);
        void Remove(int value);
        void IntersectWith(CharacterSet set);
        void UnionWith(CharacterSet set);
        void Invert(int size);

    }

    public struct HashCharacterSet : ICharacterSet<HashCharacterSet>
    {
        private HashSet<int> hashSet;

        internal HashCharacterSet(HashSet<int> hashSet)
        {
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

    public struct BitArrayCharacterSet : ICharacterSet<BitArrayCharacterSet>
    {
        private readonly BitArray array;

        internal BitArrayCharacterSet(bool value, int size)
        {
            this.array = new BitArray(size, value);
        }

        private BitArrayCharacterSet(BitArray array)
        {
            this.array = array;
        }

        #region ICharacterSet implementation

        public bool IsSingleton { get { return CountBits(array) == 1; } }

        public int Count
        {
            get
            {
                return CountBits(array);
            }
        }
        public bool IsEmpty
        {
            get
            {
                for(int i = 0; i < array.Length; ++i)
                {
                    if (array[i])
                        return false;
                }
                return true;
            }
        }
        public bool IsFull(int total)
        {
            for (int i = 0; i < array.Length; ++i)
            {
                if (!array[i])
                    return false;
            }
            return true;
        }


        public bool Contains(int characterClass)
        {
            return array[characterClass];
        }
        public bool Intersects(BitArrayCharacterSet set)
        {
            return Intersects(array, set.array);
        }
        public bool IsSubset(BitArrayCharacterSet set)
        {
            return Subset(array, set.array);
        }
        public bool Equals(BitArrayCharacterSet set)
        {
            return Equal(array, set.array);
        }
        public BitArrayCharacterSet Intersection(BitArrayCharacterSet set)
        {
            return new BitArrayCharacterSet(And(array, set.array));
        }
        public BitArrayCharacterSet Union(BitArrayCharacterSet set)
        {
            return new BitArrayCharacterSet(Or(array, set.array));
        }

        public BitArrayCharacterSet Except(BitArrayCharacterSet set)
        {
            BitArray inverted = new BitArray(set.array).Not();
            inverted.And(array);
            return new BitArrayCharacterSet(inverted);
        }

        public BitArrayCharacterSet With(int value)
        {
            BitArray newArray = new BitArray(array);
            newArray[value] = true;
            return new BitArrayCharacterSet(newArray);
        }

        public BitArrayCharacterSet Create(bool full, int size)
        {
            return new BitArrayCharacterSet(new BitArray(size, full));
        }
        public BitArrayCharacterSet Empty()
        {
            return new BitArrayCharacterSet(new BitArray(array.Length));
        }
        public BitArrayCharacterSet Without(int value)
        {
            BitArray newArray = new BitArray(array);
            newArray[value] = false;
            return new BitArrayCharacterSet(newArray);
        }

        public BitArrayCharacterSet MutableClone()
        {
            BitArray newArray = new BitArray(array);
            return new BitArrayCharacterSet(newArray);
        }

        public BitArrayCharacterSet Inverted(int size)
        {
            BitArray newArray = new BitArray(array);
            newArray.Not();
            return new BitArrayCharacterSet(newArray);
        }

        public void Add(int value)
        {
            array[value] = true;
        }

        public void Remove(int value)
        {
            array[value] = false;
        }

        public void IntersectWith(BitArrayCharacterSet set)
        {
            array.And(set.array);
        }

        public void UnionWith(BitArrayCharacterSet set)
        {
            array.Or(set.array);
        }

        public void Invert(int size)
        {
            array.Not();
        }

        #endregion


        #region Internal helper methods

        /// <summary>
        /// Compares two bit arrays of equal length.
        /// </summary>
        /// <param name="arrayA">The first bit array.</param>
        /// <param name="arrayB">The second bit array.</param>
        /// <returns><see langword="true"/>, if the array have the same bits set.</returns>
        internal static bool Equal(BitArray arrayA, BitArray arrayB)
        {
            Contract.Requires(arrayA != null && arrayB != null);
            Contract.Requires(arrayA.Length == arrayB.Length);

            for (int i = 0; i < arrayA.Length; ++i)
            {
                if (arrayA[i] != arrayB[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether a bit array represents a subset
        /// of another bit array with the same length.
        /// </summary>
        /// <param name="arrayA">The first bit array.</param>
        /// <param name="arrayB">The second bit array.</param>
        /// <returns><see langword="true"/>, if all bits in <paramref name="arrayA"/>
        /// are also set in <paramref name="arrayB"/>.</returns>
        internal static bool Subset(BitArray arrayA, BitArray arrayB)
        {
            Contract.Requires(arrayA.Length == arrayB.Length);

            for (int i = 0; i < arrayA.Length; ++i)
            {
                if (arrayA[i] && !arrayB[i])
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Determines whether two bit array intersect.
        /// </summary>
        /// <param name="arrayA">The first bit array.</param>
        /// <param name="arrayB">The second bit array.</param>
        /// <returns><see langword="true"/> if there exists a bit
        /// that is set both in
        /// <paramref name="arrayA"/> and <paramref name="arrayB"/>.</returns>
        internal static bool Intersects(BitArray arrayA, BitArray arrayB)
        {
            Contract.Requires(arrayA.Length == arrayB.Length);

            for (int i = 0; i < arrayA.Length; ++i)
            {
                if (arrayA[i] && arrayB[i])
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Counts the number of bits set in the array.
        /// </summary>
        /// <param name="array">An array of bits.</param>
        /// <returns>Number of set bits in <paramref name="array"/>.</returns>
        internal static int CountBits(BitArray array)
        {
            int bits = 0;
            for (int i = 0; i < array.Length; ++i)
            {
                if (array[i])
                {
                    bits++;
                }
            }
            return bits;
        }
        /// <summary>
        /// Finds the index of the first bit set in an array.
        /// </summary>
        /// <param name="array">An array of bits.</param>
        /// <returns>The lowest index of a bit set in <paramref name="array"/>,
        /// or -1 if no bit is set.</returns>
        internal static int Min(BitArray array)
        {
            Contract.Requires(array != null);

            for (int i = 0; i < array.Length; ++i)
            {
                if (array[i])
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Finds the index of the last bit set in an array.
        /// </summary>
        /// <param name="array">An array of bits.</param>
        /// <returns>The highest index of a bit set in <paramref name="array"/>,
        /// or -1 if no bit is set.</returns>
        internal static int Max(BitArray array)
        {
            Contract.Requires(array != null);

            for (int i = array.Length - 1; i >= 0; --i)
            {
                if (array[i])
                {
                    return i;
                }
            }
            return -1;
        }
        /// <summary>
        /// Creates a new bit array by AND operation of two arrays.
        /// </summary>
        /// <param name="arrayA">The first bit array.</param>
        /// <param name="arrayB">The second bit array.</param>
        /// <returns>A new instance of bit array representing the conjunction of 
        /// <paramref name="arrayA"/> and <paramref name="arrayB"/>.
        /// </returns>
        internal static BitArray And(BitArray arrayA, BitArray arrayB)
        {
            BitArray newArray = new BitArray(arrayA);
            return newArray.And(arrayB);
        }
        /// <summary>
        /// Creates a new bit array by OR operation of two arrays.
        /// </summary>
        /// <param name="arrayA">The first bit array.</param>
        /// <param name="arrayB">The second bit array.</param>
        /// <returns>A new instance of bit array representing the disjunction of 
        /// <paramref name="arrayA"/> and <paramref name="arrayB"/>.
        /// </returns>
        internal static BitArray Or(BitArray arrayA, BitArray arrayB)
        {
            BitArray newArray = new BitArray(arrayA);
            return newArray.Or(arrayB);
        }

  

        #endregion

    }

}
