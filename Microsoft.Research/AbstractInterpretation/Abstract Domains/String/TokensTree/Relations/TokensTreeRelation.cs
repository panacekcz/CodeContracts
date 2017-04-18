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
using Microsoft.Research.DataStructures;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Pair of related nodes.
    /// </summary>
    internal struct InnerNodePair
    {
        public readonly InnerNode left;
        public readonly InnerNode right;

        public InnerNodePair(InnerNode left, InnerNode right)
        {
            this.left = left;
            this.right = right;
        }
    }

    /// <summary>
    /// Computes a relation between nodes of token trees.
    /// </summary>
    internal abstract class TokensTreeRelation
    {
        internal readonly HashSet<InnerNodePair> knownPairs = new HashSet<InnerNodePair>();
        private readonly WorkList<InnerNodePair> pendingPairs = new WorkList<InnerNodePair>();
        protected readonly InnerNode leftRoot, rightRoot;

        /// <summary>
        /// Creates an empty relation of trees.
        /// </summary>
        /// <param name="leftRoot">Root of the tree of nodes at the left side.</param>
        /// <param name="rightRoot">Root of the tree of nodes at the right side.</param>
        protected TokensTreeRelation(InnerNode leftRoot, InnerNode rightRoot)
        {
            this.leftRoot = leftRoot;
            this.rightRoot = rightRoot;
        }

        /// <summary>
        /// Adds the initial node pairs to the relation.
        /// </summary>
        protected abstract void Init();
        /// <summary>
        /// Adds node pairs to the relation based on a pair of related nodes.
        /// </summary>
        /// <param name="left">A related node on the left side.</param>
        /// <param name="right">A related node on the right side.</param>
        /// <returns>False, if contradiction is found.</returns>
        protected abstract bool Next(InnerNode left, InnerNode right);

        /// <summary>
        /// Requests a pair of nodes to be related.
        /// </summary>
        /// <param name="left">A node on the left side.</param>
        /// <param name="right">A node on the right side.</param>
        public void Request(TokensTreeNode left, TokensTreeNode right)
        {
            InnerNode innerLeft = (left is RepeatNode) ? leftRoot : (InnerNode)left;
            InnerNode innerRight = (right is RepeatNode) ? rightRoot : (InnerNode)right;
            InnerNodePair innerPair = new InnerNodePair(innerLeft, innerRight);

            if (knownPairs.Add(innerPair))
            {
                pendingPairs.Add(innerPair);
            }
        }

        /// <summary>
        /// Computes the relation.
        /// </summary>
        /// <returns>False, if contradiction is found.</returns>
        public bool Solve()
        {
            Init();

            while (!pendingPairs.IsEmpty)
            {
                var pair = pendingPairs.Pull();
                if (!Next(pair.left, pair.right))
                    return false;

            }

            return true;
        }
    }
}
