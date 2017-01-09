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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    public class PrefixTreeBuilder
    {
        /// <summary>
        /// Builds a prefix tree representing the empty string.
        /// </summary>
        /// <returns>Root node of a tree representing the empty string.</returns>
        public static InnerNode Empty()
        {
            return new InnerNode(true);
        }

        /// <summary>
        /// Builds a prefix tree representing all strings.
        /// </summary>
        /// <returns>Root node of the tree.</returns>
        public static InnerNode Unknown()
        {
            InnerNode top = new InnerNode(true);
            for (int i = char.MinValue; i <= char.MaxValue; ++i)
            {
                top.children[(char)i] = RepeatNode.Repeat;
            }

            return top;
        }

        /// <summary>
        /// Builds a prefix tree representing no strings.
        /// </summary>
        /// <returns>Root node of the tree.</returns>
        public static InnerNode Unreached()
        {
            return new InnerNode(false);
        }


        private static InnerNode PrependChar(char c, PrefixTreeNode tn)
        {
            InnerNode inn = new InnerNode(false);
            inn.children[c] = tn;
            return inn;
        }

        /// <summary>
        /// Builds a prefix tree representing a single constant.
        /// </summary>
        /// <param name="constant">The string constant.</param>
        /// <returns>Root node of the tree.</returns>
        public static InnerNode FromString(string constant)
        {
            InnerNode tn = Empty();

            for (int i = constant.Length - 1; i >= 0; --i)
            {
                tn = PrependChar(constant[i], tn);
            }

            return tn;
        }

        /// <summary>
        /// Builds a prefix tree representing strings which are repetitions of a single token.
        /// </summary>
        /// <param name="token">The repeated token.</param>
        /// <returns>Root node of the tree</returns>
        public static PrefixTreeNode FromToken(string token)
        {
            if (token == "")
                return Empty();
            PrefixTreeNode tn = RepeatNode.Repeat;

            for (int i = token.Length - 1; i >= 0; --i)
            {
                tn = PrependChar(token[i], tn);
            }

            return tn;
        }

        public static PrefixTreeNode PrependFromCharInterval(CharInterval interval, int repeat, PrefixTreeNode e)
        {
            for (int i = 0; i < repeat; ++i)
            {
                e = CharIntervalNode(interval, e);
            }
            return e;
        }
        public static PrefixTreeNode FromCharInterval(CharInterval interval, int repeat = 1)
        {
            return PrependFromCharInterval(interval, repeat, Empty());
        }
        public static PrefixTreeNode CharIntervalTokens(CharInterval interval)
        {
            return CharIntervalNode(interval, RepeatNode.Repeat);
        }


        public static InnerNode CharIntervalNode(CharInterval interval, PrefixTreeNode next)
        {
            InnerNode node = new InnerNode(true);
            for (int i = interval.LowerBound; i <= interval.UpperBound; ++i)
            {
                node.children[(char)i] = next;
            }

            return node;
        }

        public static InnerNode CharIntervalsNode(IEnumerable<CharInterval> intervals, PrefixTreeNode next)
        {
            InnerNode node = new InnerNode(true);
            foreach (var interval in intervals)
            {
                for (int i = interval.LowerBound; i <= interval.UpperBound; ++i)
                {
                    node.children[(char)i] = next;
                }
            }

            return node;
        }

    }
}
