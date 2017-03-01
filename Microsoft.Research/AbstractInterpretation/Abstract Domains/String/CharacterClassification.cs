// CodeContracts
// 
// Copyright (c) Microsoft Corporation
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

// Created by Vlastimil Dort (2015-2016)
// Master thesis String Analysis for Code Contracts

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Classifies characters by dividing them into equivalence classes.
    /// </summary>
    public interface ICharacterClassification
    {
        /// <summary>
        /// Gets the number of buckets.
        /// </summary>
        int Buckets { get; }
        /// <summary>
        /// Classifies a chacharacter.
        /// </summary>
        /// <param name="index">The character to be classified.</param>
        /// <returns>The bucket, where <paramref name="index"/> belongs.</returns>
        int this[char index]
        {
            get;
        }
        /// <summary>
        /// Tells whether the bucket contains exactly one character.
        /// </summary>
        /// <param name="bucket">Index of the bucket.</param>
        /// <returns>Whether the bucket of contains exactly one character.</returns>
        bool IsSingleton(int bucket);

        string ToString(int bucket);
    }

    /// <summary>
    /// Character classification where each character has its own class.
    /// </summary>
    public class CompleteClassification : ICharacterClassification
    {
        /// <summary>
        /// Gets the number of buckets, which is the range of the <see cref="System.Char"/> type.
        /// </summary>
        public int Buckets
        {
            get { return 65536; }
        }

        /// <summary>
        /// Classifies a chacharacter.
        /// </summary>
        /// <param name="character">The character to be classified.</param>
        /// <returns>The bucket, where <paramref name="character"/> belongs.
        /// Same value as <paramref name="character"/>.</returns>
        public int this[char character]
        {
            get
            {
                return character;
            }
        }

        /// <summary>
        /// Tells whether the bucket contains exactly one character.
        /// </summary>
        /// <param name="bucket">Index of the bucket.</param>
        /// <returns>Always <see langword="true"/>.</returns>
        public bool IsSingleton(int bucket)
        {
            return true;
        }

        public string ToString(int bucket)
        {
            return ((char)bucket).ToString();
        }
    }

    /// <summary>
    /// Character classification where each ASCII character
    /// has its category and all other character are in the same category.
    /// </summary>
    public class ASCIIClassification : ICharacterClassification
    {
        /// <summary>
        /// Gets the number of buckets, which is the range ASCII characters + 1.
        /// </summary>
        public int Buckets
        {
            get { return 129; }
        }

        /// <summary>
        /// Classifies a chacharacter.
        /// </summary>
        /// <param name="character">The character to be classified.</param>
        /// <returns>The bucket, where <paramref name="character"/> belongs.
        /// Same value as <paramref name="character"/> for ASCII characters,
        /// 128 for other characters.
        /// .</returns>
        public int this[char character]
        {
            get
            {
                if (character < 128)
                {
                    return character;
                }
                else
                {
                    return 128;
                }
            }
        }

        /// <summary>
        /// Tells whether the bucket contains exactly one character.
        /// </summary>
        /// <param name="bucket">Index of the bucket.</param>
        /// <returns><see langword="true"/> for ascii characters.</returns>
        public bool IsSingleton(int bucket)
        {
            return bucket != 128;
        }

        public string ToString(int bucket)
        {
            if (IsSingleton(bucket))
                return ((char)bucket).ToString();
            else
                return "(non-ascii)";
        }
    }

    /// <summary>
    /// Character classification according to Unicode categories.
    /// </summary>
    /// <remarks>
    /// This classification is not stable across Unicode versions.
    /// There may be new characters defined and other changes in categories.
    /// </remarks>
    public class CategoryClassification : ICharacterClassification
    {
        public int Buckets
        {
            get
            {
                return 30;
            }
        }

        public bool IsSingleton(int bucket)
        {
            return false;
        }

        public int this[char character]
        {
            get
            {
                return (int)char.GetUnicodeCategory(character);
            }
        }

        public string ToString(int bucket)
        {
            return ((System.Globalization.UnicodeCategory)bucket).ToString();
        }
    }
}
