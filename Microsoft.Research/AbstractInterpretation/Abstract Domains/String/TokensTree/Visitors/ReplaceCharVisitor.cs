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

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Replaces char from an interval with another char from another interval.
    /// </summary>
    /// <remarks>
    /// For each node, looks at edges that can be replaced, and constructs a merged node of the children.
    /// Also merges in the children of nodes that can be the replacement character, so that 
    /// it does not construct too many merged nodes.
    /// If the replaced char is known, removes the edge.
    /// For each replacement edge, if it was there before or not, adds the merged ndoe.
    /// </remarks>
    class ReplaceCharVisitor : TokensTreeTransformer
    {
        private CharInterval from, to;

        public ReplaceCharVisitor(TokensTreeMerger merger, CharInterval from, CharInterval to)
            : base(merger)
        {
            this.from = from;
            this.to = to;
        }

        /// <summary>
        /// Replaces the characters in a specified tree.
        /// </summary>
        /// <param name="root">Root node of the tree.</param>
        public void ReplaceChar(InnerNode root)
        {
            Transform(root);
        }

        #region TokensTreeVisitor<InnerNode> overrides
        protected override TokensTreeNode VisitRepeatNode(RepeatNode repeatNode)
        {
            return repeatNode;

        }
        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
              
            InnerNode newInnerNode = null;
            TokensTreeNode next = TokensTreeBuilder.Unreached(); //could be optinized

            bool canReplace = false;
            
            foreach(var child in innerNode.children)
            {
                TokensTreeNode newChild = VisitNodeCached(child.Value);

                if (newChild != child.Value)
                {
                    if (newInnerNode == null)
                        newInnerNode = new InnerNode(innerNode);

                    newInnerNode.children[child.Key] = newChild;
                }

                if (from.Contains(child.Key))
                {
                    canReplace = true;
                    next = Merge(next, newChild);
                }
                else if (to.Contains(child.Key))
                {
                    next = Merge(next, child.Value);
                }
            }

            if (canReplace)
            {
                if (newInnerNode == null)
                    newInnerNode = new InnerNode(innerNode);

                if (from.IsConstant)
                    newInnerNode.children.Remove(from.LowerBound);

                for (int i = to.LowerBound; i <= to.UpperBound; ++i)
                {
                    newInnerNode.children[(char)i] = next;
                }
            }
            
            return newInnerNode ?? innerNode;

        }
        #endregion
    }
}
