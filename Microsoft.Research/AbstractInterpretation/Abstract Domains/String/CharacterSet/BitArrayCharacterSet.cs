// CodeContracts
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
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Implementation of character set using an array of bits.
    /// </summary>
    public struct BitArrayCharacterSet : ICharacterSet<BitArrayCharacterSet>
    {
        private readonly BitArray array;

        internal BitArrayCharacterSet(bool value, int size)
        {
            this.array = new BitArray(size, value);
        }

        private BitArrayCharacterSet(BitArray array)
        {
            Contract.Requires(array != null);

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
                for (int i = 0; i < array.Length; ++i)
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

    /// <summary>
    /// Creates character sets implemented as an array of bits.
    /// </summary>
    public class BitArrayCharacterSetFactory : ICharacterSetFactory<BitArrayCharacterSet>
    {
        public BitArrayCharacterSet Create(bool value, int size)
        {
            return new BitArrayCharacterSet(value, size);
        }

        public BitArrayCharacterSet CreateSingleton(char value, ICharacterClassification classif)
        {
            return Create(false, classif.Buckets).With(classif[value]);
        }

        public BitArrayCharacterSet CreateIntervals(IEnumerable<CharInterval> intervals, ICharacterClassification classif)
        {
            Contract.Requires(intervals != null);

            BitArrayCharacterSet array = Create(false, classif.Buckets);
            foreach (CharInterval interval in intervals)
            {
                for (int character = interval.LowerBound; character <= interval.UpperBound; ++character)
                    array.Add(classif[(char)character]);
            }
            return array;
        }
    }
}
