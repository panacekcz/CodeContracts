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

    struct InnerNodeBuilder
    {
        InnerNode newNode;
        InnerNode oldNode;

        public InnerNodeBuilder(InnerNode oldNode)
        {
            this.newNode = null;
            this.oldNode = oldNode;
        }

        public void SetChild(char c, PrefixTreeNode next)
        {


            if(newNode == null)
            {
                newNode = new InnerNode(oldNode);
            }
        }

        public InnerNode Build()
        {
            return newNode ?? oldNode;
        }
    }

    class ReplaceCharVisitor : PrefixTreeTransformer
    {
        private CharInterval from, to;

        public ReplaceCharVisitor(PrefixTreeMerger merger, CharInterval from, CharInterval to)
            : base(merger)
        {
            this.from = from;
            this.to = to;
        }

        public void ReplaceChar(InnerNode root)
        {
            Transform(root);
        }


        protected override PrefixTreeNode VisitRepeatNode(RepeatNode inn)
        {
            return inn;

        }
        protected override PrefixTreeNode VisitInnerNode(InnerNode inn)
        {
              
            InnerNode newInn = null;
            PrefixTreeNode next = PrefixTreeBuilder.Unreached(); //could be optinized

            
            foreach(var child in inn.children)
            {
                PrefixTreeNode newChild = VisitNodeCached(child.Value);

                if (from.Contains(child.Key))
                {
                    if(newInn == null)
                        newInn = new InnerNode(inn);

                    if (from.IsConstant)
                        newInn.children.Remove(child.Key);

                    next = Merge(next, child.Value);
                }
                if (to.Contains(child.Key))
                {
                    //TODO: order is completely wrong
                    next = Merge(next, child.Value);
                    newInn.children[child.Key] = next;
                }

            }


            return newInn ?? inn;

        }
    }

}
