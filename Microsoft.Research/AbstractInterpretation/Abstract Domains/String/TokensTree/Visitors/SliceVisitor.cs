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
    /// Splits a tokens tree at specified nodes.
    /// </summary>
    internal class SplitVisitor : TokensTreeTransformer
    {
        private readonly HashSet<InnerNode> splitStates;

        public SplitVisitor(TokensTreeMerger merger, HashSet<InnerNode> splitStates) : base(merger) {
            this.splitStates = splitStates;
        }

        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode newInnerNode = (InnerNode)base.VisitInnerNode(innerNode);

            if (splitStates.Contains(innerNode))
                return Cutoff(newInnerNode);
            else
                return newInnerNode;
        }

        public void Split(InnerNode root)
        {
            if (splitStates.Count == 0)
                return;
            // Cut off all split nodes
            Transform(root);
        }
    }

    /// <summary>
    /// Gets a slice of a tokens tree before specified nodes.
    /// </summary>
    internal class SliceBeforeVisitor : TokensTreeTransformer
    {
        
        private readonly HashSet<InnerNode> mayEnd;
        IntervalMarkVisitor imv;
        IndexInt upper;

        public SliceBeforeVisitor(TokensTreeMerger merger, HashSet<InnerNode> mayEnd, IntervalMarkVisitor imv, IndexInt upper) : base(merger)
        {
            this.mayEnd = mayEnd;
            this.imv = imv;
            this.upper = upper;
        }

        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
            if (imv.GetIndexInterval(innerNode).LowerBound >= upper)
            {
                return Share(new InnerNode(true));
            }

            InnerNode newNode = (InnerNode)base.VisitInnerNode(innerNode);

            if (mayEnd.Contains(innerNode) && !newNode.Accepting)
            {
                //set accepting to true
                InnerNode acceptingNode = new InnerNode(newNode);
                acceptingNode.accepting = true;
                return Share(acceptingNode);
            }
            else
            {
                return newNode;
            }
            
        }

        public void SliceBefore(InnerNode root)
        {
            Transform(root);
        }
    }

    /// <summary>
    /// Gets a slice of a tokens tree after specified nodes.
    /// </summary>
    internal class SliceAfterVisitor : TokensTreeTransformer
    {
        private readonly HashSet<InnerNode> splitStates;

        public SliceAfterVisitor(TokensTreeMerger merger, HashSet<InnerNode> splitStates) : base(merger) {
            this.splitStates = splitStates;
        }

        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode newInnerNode = (InnerNode)base.VisitInnerNode(innerNode);

            if (splitStates.Contains(innerNode))
                return Cutoff(newInnerNode);
            else
                return newInnerNode;
        }

        public void Split(InnerNode root, bool bounded)
        {
            if(bounded)
                TransformTree(root);
            else
                Transform(root);
        }
    }
}
