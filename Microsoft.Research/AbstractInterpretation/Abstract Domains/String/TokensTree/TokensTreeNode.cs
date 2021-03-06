﻿// CodeContracts
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
    /// Represents a node of a tokens tree.
    /// </summary>
    public abstract class TokensTreeNode
    {
        /// <summary>
        /// Gets the corresponding inner node in a tokens tree.
        /// </summary>
        /// <param name="root">Root of the tokens tree.</param>
        /// <returns>The inner node corresponding to this in a tree rooted in <paramref name="root"/>.</returns>
        public abstract InnerNode ToInner(InnerNode root);

        public override string ToString()
        {
            ToStringVisitor visitor = new ToStringVisitor();
            return visitor.ToString(this);
        }
    }

    /// <summary>
    /// Represents a non-repeat node of a tokens tree. The node can be accepting and have children labeled by characters.
    /// </summary>
    public class InnerNode : TokensTreeNode
    {
        internal Dictionary<char, TokensTreeNode> children;
        internal bool accepting;

        public bool Accepting { get { return accepting; } }

        public InnerNode(bool accepting)
        {
            this.accepting = accepting;
            children = new Dictionary<char, TokensTreeNode>();
        }
        public InnerNode(InnerNode inn)
        {
            this.accepting = inn.Accepting;
            children = new Dictionary<char, TokensTreeNode>(inn.children);
        }

        public override InnerNode ToInner(InnerNode root)
        {
            return this;
        }
    }

    /// <summary>
    /// Represents a repeat node in a tokens tree. Reaching this node means returning to the root
    /// of the tree.
    /// </summary>
    public class RepeatNode : TokensTreeNode
    {
        public static RepeatNode Repeat = new RepeatNode();

        private RepeatNode()
        {
        }

        public override InnerNode ToInner(InnerNode root)
        {
            return root;
        }
    }
}
