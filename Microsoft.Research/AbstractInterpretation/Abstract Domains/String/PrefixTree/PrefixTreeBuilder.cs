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
    class PrefixTreeBuilder
    {
        public static InnerNode Empty()
        {
            return new InnerNode(true);
        }

        public static InnerNode Unknown()
        {
            InnerNode top = new InnerNode(true);
            for (int i = char.MinValue; i <= char.MaxValue; ++i)
            {
                top.children[(char)i] = RepeatNode.Repeat;
            }

            return top;
        }


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

        public static InnerNode FromString(string c)
        {
            InnerNode tn = Empty();

            for (int i = c.Length - 1; i >= 0; --i)
            {
                tn = PrependChar(c[i], tn);
            }

            return tn;
        }

        public static PrefixTreeNode FromToken(string c)
        {
            if (c == "")
                return Empty();
            PrefixTreeNode tn = RepeatNode.Repeat;

            for (int i = c.Length - 1; i >= 0; --i)
            {
                tn = PrependChar(c[i], tn);
            }

            return tn;
        }

        public static PrefixTreeNode FromCharInterval(CharInterval interval, int repeat = 1)
        {
            PrefixTreeNode e = Empty();
            for(int i = 0; i < repeat; ++i)
            {
                e = CharIntervalNode(interval, e);
            }
            return e;
        }
        public static PrefixTreeNode CharIntervalTokens(CharInterval interval)
        {
            return CharIntervalNode(interval, RepeatNode.Repeat);
        }


        private static InnerNode CharIntervalNode(CharInterval interval, PrefixTreeNode next)
        {
            InnerNode node = new InnerNode(true);
            for (int i = interval.LowerBound; i <= interval.UpperBound; ++i)
            {
                node.children[(char)i] = next;
            }

            return node;
        }

    }
}
