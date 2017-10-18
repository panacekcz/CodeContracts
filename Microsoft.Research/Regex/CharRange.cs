// Copyright (c) Charles University
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Created by Vlastimil Dort
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex
{
    /// <summary>
    /// Represents a range of character values.
    /// </summary>
    public struct CharRange : IEnumerable<char>
    {
        private readonly char low, high;

        /// <summary>
        /// Gets the lowest character in the range.
        /// </summary>
        public char Low
        {
            get
            {
                return low;
            }
        }

        /// <summary>
        /// Gets the highest character in the range.
        /// </summary>
        public char High
        {
            get
            {
                return high;
            }
        }
        
        /// <summary>
        /// Constructs a range of characters from the lowest and highest character.
        /// </summary>
        /// <param name="low">The lowest character if the range.</param>
        /// <param name="high">The highest character of the range.</param>
        public CharRange(char low, char high)
        {
            this.low = low;
            this.high = high;
        }
        /// <summary>
        /// Checks whether the range contains a specified character.
        /// </summary>
        /// <param name="value">A character value to test.</param>
        /// <returns>True, if <paramref name="value"/> is in the range.</returns>
        public bool Contains(char value)
        {
            return low <= value && value <= high;
        }

        #region IEnumerable<char> implementation
        CharRangeEnumerator GetEnumerator()
        {
            return new CharRangeEnumerator(low, high);
        }

        IEnumerator<char> IEnumerable<char>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
    /// <summary>
    /// Represents multiple ranges of characters.
    /// </summary>
    public struct CharRanges
    {
        private readonly IEnumerable<CharRange> ranges;

        public IEnumerable<CharRange> Ranges
        {
            get
            {
                return ranges;
            }
        }

        public CharRanges(params CharRange[] ranges) :
            this((IEnumerable<CharRange>)ranges)
        { }

        public CharRanges(IEnumerable<CharRange> ranges)
        {
            this.ranges = ranges;
        }
        /// <summary>
        /// Checks whether the ranges contain a specified character.
        /// </summary>
        /// <param name="value">A character value to test.</param>
        /// <returns>True, if <paramref name="value"/> is in some of the ranges.</returns>
        public bool Contains(char value)
        {
            return ranges.Any(r => r.Contains(value));
        }
    }

    /// <summary>
    /// Enumerates individual characters of a CharRange.
    /// </summary>
    public struct CharRangeEnumerator : IEnumerator<char>
    {
        private readonly char lowerBound, upperBound;
        private int current;

        public CharRangeEnumerator(char lowerBound, char upperBound)
        {
            this.lowerBound = lowerBound;
            this.upperBound = upperBound;
            current = lowerBound - 1;
        }

        public char Current
        {
            get
            {
                return (char)current;
            }
        }

        object IEnumerator.Current
        {
            get
            {
                return (char)current;
            }
        }

        public void Dispose()
        {
        }

        public bool MoveNext()
        {
            ++current;
            return current <= upperBound;
        }

        public void Reset()
        {
            current = lowerBound - 1;
        }
    }
}
