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
    public abstract class PrefixTreeVisitor<Result>
    {
        protected Result VisitNode(PrefixTreeNode tn)
        {
            if (tn is InnerNode)
                return VisitInnerNode((InnerNode)tn);
            else if (tn is RepeatNode)
            {
                return VisitRepeatNode((RepeatNode)tn);
            }

            throw new NotImplementedException();
        }

        protected abstract Result VisitInnerNode(InnerNode inn);
        protected abstract Result VisitRepeatNode(RepeatNode inn);
    }

    public abstract class CachedPrefixTreeVisitor<Result> : PrefixTreeVisitor<Result>
    {
        private Dictionary<PrefixTreeNode, Result> cache = new Dictionary<PrefixTreeNode, Result>();

        protected Result VisitNodeCached(PrefixTreeNode tn)
        {
            Result r;
            if (!cache.TryGetValue(tn, out r))
            {
                r = VisitNode(tn);
                cache[tn] = r;
            }

            return r;
        }

    }

    internal class TrieShare
    {
        private Dictionary<PrefixTreeNode, PrefixTreeNode> nodes = new Dictionary<PrefixTreeNode, PrefixTreeNode>(PrefixTreeNodeComparer.Comparer);

        public PrefixTreeNode Share(PrefixTreeNode tn)
        {
            PrefixTreeNode tno;
            if (nodes.TryGetValue(tn, out tno))
                return nodes[tn];
            else
            {
                nodes[tn] = tn;
                return tn;
            }
        }

    }

    


    

}
