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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Determines whether the strings represented by a tokens tree must contain
    /// a specified constant.
    /// </summary>
    class MustContainVisitor : ForwardTokensTreeVisitor<IndexInt>
    {
        private readonly KMP constantKmp;
        private readonly bool fixedEnd;
        private bool fail;

        public MustContainVisitor(string constant, bool fixedEnd)
        {
            this.constantKmp = new KMP(constant);
            this.fixedEnd = fixedEnd;
        }


        private bool IsAcceptingState(IndexInt index)
        {
            if (index.IsInfinite)
                return false;
            if (index.IsNegative)
                return true;
            return index == constantKmp.End;
        }
        private IndexInt Next(IndexInt state, char c)
        {
            if (state.IsInfinite || state.IsNegative)
                return state;
            else
            {
                int nextIndex = constantKmp.Next(state.AsInt, c);

                if (!fixedEnd && nextIndex == constantKmp.End)
                    return IndexInt.Negative;

                return IndexInt.For(nextIndex);
            }

        }

        #region ForwardTokensTreeVisitor<IndexInt> overrides
        protected override IndexInt Default()
        {
            return IndexInt.Negative;
        }

        protected override IndexInt Merge(IndexInt oldData, IndexInt newData)
        {
            return IndexUtils.JoinIndices(oldData, newData);
        }


        protected override void VisitInnerNode(InnerNode node)
        {
            if (fail)
                return;

            IndexInt index = Get(node);

            if (node.Accepting && !IsAcceptingState(index))
            {
                fail = true;
            }
            else
            {
                foreach (var c in node.children)
                {
                    Push(c.Value, Next(index, c.Key));
                }
            }
        }
        #endregion

        public bool MustContain(InnerNode root)
        {
            fail = false;
            Push(root, IndexInt.For(0));
            this.Traverse(root);
            return !fail;
        }
    }


}
