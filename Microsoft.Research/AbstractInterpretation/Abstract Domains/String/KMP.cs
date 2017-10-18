// CodeContracts
// 
// Copyright (c) Microsoft Corporation
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
    /// Implementation of the KMP (Knuth - Morris - Pratt) algorithm.
    /// </summary>
    internal class KMP
    {
        private readonly int[] back;
        private readonly string needle;

        internal int Next(int current, char c)
        {
            while (current >= 0 && needle[current] != c)
                current = back[current];
            return current + 1;
        }

        internal int End
        {
            get { return needle.Length; }
        }

        /// <summary>
        /// Initializes the structure to find the specified string.
        /// </summary>
        /// <param name="needle">The string to be searched for.</param>
        public KMP(string needle)
        {
            this.needle = needle;
            back = new int[needle.Length + 1];
            back[0] = -1;
            for (int i = 0; i < needle.Length; ++i)
            {
                back[i + 1] = Next(back[i], needle[i]);
            }
        }
        /// <summary>
        /// Gets the longest known prefix of a string created by replacing needle
        /// by replacement in a haystack of which only a prefix is known.
        /// </summary>
        /// <param name="haystack">Prefix of the haystack.</param>
        /// <param name="replacement">The string substituted for needle.</param>
        /// <returns>The longest known prefix of the result.</returns>
        public string PrefixOfReplace(string haystack, string replacement)
        {
            int current = 0;
            StringBuilder sb = new StringBuilder();

            int start = 0;
            for (int i = 0; i < haystack.Length; ++i)
            {
                System.Diagnostics.Debug.Assert(start + current == i);

                int next = Next(current, haystack[i]);

                sb.Append(haystack.Substring(start, current + 1 - next));
                start += current + 1 - next;
                current = next;

                if (current == End)
                {
                    current = 0;
                    start += needle.Length;
                    sb.Append(replacement);
                }
            }

            System.Diagnostics.Debug.Assert(start + current == haystack.Length);
            int lcp = StringUtils.LongestCommonPrefixLength(needle, replacement);
            int common = Math.Min(lcp, current);
            sb.Append(haystack.Substring(start, common));

            return sb.ToString();
        }

        /// <summary>
        /// Determines, whether the needle can occur inside a string which has 
        /// the specified prefix, such that the needle overlaps the prefix.
        /// </summary>
        /// <param name="prefix">Prefix of the haystack.</param>
        /// <returns><see langword="true"/>, if the haystack can contain needle
        /// that overlaps the specified prefix.</returns>
        public bool CanOverlap(string prefix)
        {
            int current = 0;

            for (int i = 0; i < prefix.Length; ++i)
            {
                current = Next(current, prefix[i]);
                if (current == End)
                {
                    return true;
                }
            }

            return current > 0;
        }
    }

}
