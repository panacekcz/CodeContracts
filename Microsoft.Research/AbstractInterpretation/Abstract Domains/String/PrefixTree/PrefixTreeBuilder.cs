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
    /// <summary>
    /// Provides helper methods for building prefix trees.
    /// </summary>
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

        /// <summary>
        /// Prepends nodes with edges for all characters in the specified interval, to a existing tree.
        /// </summary>
        /// <param name="interval">The interval of characters.</param>
        /// <param name="repeatCount">How many times a node should be prepended.</param>
        /// <param name="next">The node following after all charactes.</param>
        /// <returns>A tree which starts with <paramref name="repeatCount"/> nodes, which are not accepting and for each character in <paramref name="interval"/>, there
        /// is an edge to the next node, ending with <paramref name="next"/>.</returns>
        public static PrefixTreeNode PrependCharInterval(CharInterval interval, int repeatCount, PrefixTreeNode next)
        {
            for (int i = 0; i < repeatCount; ++i)
            {
                next = PrependCharInterval(interval, next, false);
            }
            return next;
        }

        /// <summary>
        /// Creates a tree for a constant number of characters in the specified interval.
        /// </summary>
        /// <param name="interval">The interval of characters.</param>
        /// <param name="repeat">How many times the characters should be repeated.</param>
        /// <returns>A tree which starts with <paramref name="repeat"/> nodes, which are not accepting and for each character in <paramref name="interval"/>, there
        /// is an edge to the next node, ending with an accepting node.</returns>
        public static PrefixTreeNode FromCharInterval(CharInterval interval, int repeatCount = 1)
        {
            return PrependCharInterval(interval, repeatCount, Empty());
        }

        /// <summary>
        /// Creates a prefix tree which represents a language of unlimited iteration of characters
        /// from an interval.
        /// </summary>
        /// <param name="interval">The interval of allowed characters.</param>
        /// <returns>A tree where the root is accepting and for each character in <paramref name="interval"/>, there is 
        /// an edge to a repeat node.</returns>
        public static PrefixTreeNode CharIntervalTokens(CharInterval interval)
        {
            return PrependCharInterval(interval, RepeatNode.Repeat, true);
        }

        /// <summary>
        /// Prepends a node with edges for a specified character to a existing tree.
        /// </summary>
        /// <param name="c">The character.</param>
        /// <param name="next">The node following after all charactes.</param>
        /// <returns>A tree where the root is not accepting and for each character in <paramref name="intervals"/>, there
        /// is an edge to <paramref name="next"/>.</returns>
        public static InnerNode PrependChar(char c, PrefixTreeNode next)
        {
            InnerNode newNode = new InnerNode(false);
            newNode.children[c] = next;
            return newNode;
        }


        /// <summary>
        /// Prepends a node with edges for all characters in the specified interval, to a existing tree.
        /// </summary>
        /// <param name="interval">The interval of characters.</param>
        /// <param name="next">The node following after all charactes.</param>
        /// <param name="accepting">Whether the node should be accepting.</param>
        /// <returns>A tree where the root accepting flag is <paramref name="accepting"/> and for each character in <paramref name="interval"/>, there
        /// is an edge to <paramref name="next"/>.</returns>
        public static InnerNode PrependCharInterval(CharInterval interval, PrefixTreeNode next, bool accepting)
        {
            InnerNode node = new InnerNode(accepting);
            for (int i = interval.LowerBound; i <= interval.UpperBound; ++i)
            {
                node.children[(char)i] = next;
            }

            return node;
        }

        /// <summary>
        /// Prepends a node with edges for all characters in the specified intervals, to a existing tree.
        /// </summary>
        /// <param name="intervals">The intervals of characters.</param>
        /// <param name="next">The node following after all charactes.</param>
        /// <returns>A tree where the root is not accepting and for each character in <paramref name="intervals"/>, there
        /// is an edge to <paramref name="next"/>.</returns>
        public static InnerNode PrependCharIntervals(IEnumerable<CharInterval> intervals, PrefixTreeNode next)
        {
            InnerNode node = new InnerNode(false);
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
