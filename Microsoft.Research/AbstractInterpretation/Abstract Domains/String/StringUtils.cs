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

using System.Diagnostics;

namespace Microsoft.Research.AbstractDomains.Strings
{
    internal static class StringUtils
    {
        /// <summary>
        /// Computes the length of the longest common prefix of two strings.
        /// </summary>
        /// <param name="stringA">The first string.</param>
        /// <param name="stringB">The second string.</param>
        /// <returns>The length of the longest common prefix of <paramref name="stringA"/>
        /// and <paramref name="stringB"/>.</returns>
        public static int LongestCommonPrefixLength(string stringA, string stringB)
        {
            System.Diagnostics.Contracts.Contract.Requires(stringA != null && stringB != null);

            int i = 0;
            while (i < stringA.Length && i < stringB.Length && stringA[i] == stringB[i])
            {
                ++i;
            }
            return i;
        }

        /// <summary>
        /// Computes the longest common prefix of two strings.
        /// </summary>
        /// <param name="stringA">The first string.</param>
        /// <param name="stringB">The second string.</param>
        /// <returns>The longest common prefix of <paramref name="stringA"/>
        /// and <paramref name="stringB"/>.</returns>
        public static string LongestCommonPrefix(string stringA, string stringB)
        {
            System.Diagnostics.Contracts.Contract.Requires(stringA != null && stringB != null);

            return stringA.Substring(0, LongestCommonPrefixLength(stringA, stringB));
        }

        public static int LongestCommonSuffixLength(string a, string b)
        {
            int al = a.Length;
            int bl = b.Length;
            int i = 0;
            while (i < al && i < bl && a[al - 1 - i] == b[bl - 1 - i])
            {
                ++i;
            }
            return i;
        }

        public static string LongestCommonSuffix(string a, string b)
        {
            return a.Substring(a.Length - LongestCommonSuffixLength(a, b));
        }

        public static string LongestConstantPrefix(string a, char p)
        {
            int i = 0;
            while (i < a.Length && a[i] == p)
            {
                ++i;
            }
            return a.Substring(0, i);
        }

        public static string LongestConstantSuffix(string a, char p)
        {
            int i = a.Length;
            while (i > 0 && a[i - 1] == p)
            {
                --i;
            }
            return a.Substring(i);
        }

        public static bool CanBeEqualPrefix(string a, string b)
        {
            //alternative: return a.StartsWith(b, SC.Ordinal) || b.StartsWith(a, SC.Ordinal)
            for (int i = 0; i < a.Length && i < b.Length; ++i)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }
            return true;
        }

    }
}
