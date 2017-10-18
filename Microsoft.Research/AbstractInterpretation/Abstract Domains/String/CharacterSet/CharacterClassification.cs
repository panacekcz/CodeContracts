// CodeContracts
// 
// Copyright (c) Charles University
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

        /// <summary>
        /// Gets an interval that contains all characters from some class.
        /// </summary>
        /// <param name="bucket">Index of the character class.</param>
        /// <returns>Interval containing all characters from the class. May contain some characters outside of the class.</returns>
        CharInterval ToInterval(int bucket);
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

        public CharInterval ToInterval(int bucket)
        {
            return CharInterval.For((char)bucket);
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

        public CharInterval ToInterval(int bucket)
        {
            if (bucket < 128)
                return CharInterval.For((char)bucket);
            else
                return CharInterval.For((char)128, char.MaxValue);
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


        public CharInterval ToInterval(int bucket)
        {
            if (bucket == (int)System.Globalization.UnicodeCategory.Surrogate)
            {
                // surrogates are exactly U+D800 to U+DFFF
                return CharInterval.For((char)0xd800, (char)0xdfff);
            }
            else if (bucket == (int)System.Globalization.UnicodeCategory.Control)
            {
                //Controls are U+0000 to U+001F and U+007F to U+009F
                return CharInterval.For((char)0x0000, (char)0x009f);
            }
            else
            {
                //Otherwise, return all characters from the first non-control character
                return CharInterval.For((char)0x0020, (char)0xffff);
            }
        }
    }
}
