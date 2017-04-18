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

// Created by Vlastimil Dort (2016)

using Microsoft.Research.Regex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Helper methods for <see cref="CharRange"/> and <see cref="CharRanges"/>.
    /// </summary>
    static class CharRangeExtensions
    {
        /// <summary>
        /// Converts a character range (from regex) to a character interval.
        /// </summary>
        /// <param name="range">Character range from regex.</param>
        /// <returns>Character interval with the same values as <paramref name="range"/>. </returns>
        public static CharInterval ToInterval(this CharRange range)
        {
            return CharInterval.For(range.Low, range.High);
        }

        /// <summary>
        /// Converts multiple character range (from regex) to multiple character interval.
        /// </summary>
        /// <param name="ranges">Character ranges from regex.</param>
        /// <returns>Character intervals with the same values as <paramref name="ranges"/>. </returns>
        public static IEnumerable<CharInterval> ToIntervals(this CharRanges ranges)
        {
            return ranges.Ranges.Select(ToInterval);
        }

        /// <summary>
        /// Tries to extract the first character from character ranges.
        /// </summary>
        /// <param name="ranges">Character ranges from regex.</param>
        /// <param name="first">Set to the first char from <paramref name="ranges"/>.</param>
        /// <returns>True, if <paramref name="ranges"/> contains at least one character.</returns>
        public static bool TryGetFirst(this CharRanges ranges, out char first)
        {
            foreach (var range in ranges.Ranges)
            {
                first = range.Low;
                return true;
            }
            first = default(char);
            return false;
        }

        /// <summary>
        /// Tries to extract a single character from character ranges.
        /// </summary>
        /// <param name="ranges">Character ranges from regex.</param>
        /// <param name="singleton">Set to the single char from <paramref name="ranges"/>.</param>
        /// <returns>True, if <paramref name="ranges"/> contains exactly one character.</returns>
        public static bool TryGetSingleton(this CharRanges ranges, out char singleton)
        {
            bool first = true;
            singleton = default(char);
            foreach(var range in ranges.Ranges)
            {
                if (first)
                {
                    if (range.Low != range.High)
                        return false;

                    first = false;
                    singleton = range.Low;
                    
                }
                else
                {
                    return false;
                }
            }
            return !first;
        }
    }
}
