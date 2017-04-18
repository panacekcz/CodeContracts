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
    /// Collects all nodes in a prefix tree into a set.
    /// </summary>
    class NodeCollectVisitor : TokensTreeVisitor<Void>
    {
        private HashSet<InnerNode> nodes = new HashSet<InnerNode>();

        public HashSet<InnerNode> Nodes { get { return nodes; } }
        /// <summary>
        /// Add all nodes in tree rooted by <paramref name="node"/> to <see cref="Nodes"/>.
        /// </summary>
        /// <param name="node">Root of the tree to add.</param>
        public void Collect(TokensTreeNode node)
        {
            VisitNode(node);
        }

        #region PrefixTreeVisitor<Void> overrides
        protected override Void VisitInnerNode(InnerNode innerNode)
        {
            if (!nodes.Contains(innerNode))
            {
                nodes.Add(innerNode);
                foreach (var child in innerNode.children)
                {
                    VisitNode(child.Value);
                }
            }

            return Void.Value;
        }
        protected override Void VisitRepeatNode(RepeatNode repeatNode)
        {
            return Void.Value;
        }
        #endregion
    }
}
