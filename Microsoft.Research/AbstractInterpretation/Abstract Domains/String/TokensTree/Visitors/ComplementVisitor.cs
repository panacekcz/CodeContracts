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
    /// For a bounded tokens tree, creates a tokens tree representing the complement language (or possibly more).
    /// </summary> 
    /// <remarks>
    /// In the input, there should be no repeat nodes
    /// otherwise, for each child: 
    /// - if the child is accepting, do not add it
    /// - otherwise add it and do complement on it.
    /// for other characters, add repeat nodes.
    ///</remarks>

    class ComplementVisitor : CachedTokensTreeVisitor<InnerNode>
    {
        private readonly InnerNode root;

        public static InnerNode Complement(InnerNode root)
        {
            ComplementVisitor complementVisitor = new ComplementVisitor(root);
            return complementVisitor.VisitNode(root);
        }

        private ComplementVisitor(InnerNode root)
        {
            this.root = root;
        }


        #region TokensTreeVisitor<InnerNode> implementation
        protected override InnerNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode complementNode = new InnerNode(innerNode == root);

            foreach(var c in innerNode.children)
            {
                if (!((InnerNode)c.Value).Accepting)
                {
                    complementNode.children[c.Key] = VisitNodeCached(c.Value);
                }
            }
            for(int i = char.MinValue; i <= char.MaxValue; ++i)
            {
                if (!innerNode.children.ContainsKey((char)i))
                {
                    complementNode.children[(char)i] = RepeatNode.Repeat;
                }
            }

            return complementNode;
        }

        protected override InnerNode VisitRepeatNode(RepeatNode repeatNode)
        {
            // No repeat nodes are allowed
            throw new InvalidOperationException();
        }
        #endregion
    }
}
