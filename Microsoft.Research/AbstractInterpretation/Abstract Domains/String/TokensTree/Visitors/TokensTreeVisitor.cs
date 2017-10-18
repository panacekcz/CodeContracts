// CodeContracts
// 
// Copyright (c) Charles University
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
    /// Base class for visitors that perform an action for ndoes of a tokens tree.
    /// </summary>
    /// <typeparam name="Result">Type of result value from processing a node.</typeparam>
    public abstract class TokensTreeVisitor<Result>
    {
        /// <summary>
        /// Calls <see cref="VisitInnerNode(InnerNode)"/> of <see cref="VisitRepeatNode(RepeatNode)"/>
        /// according to the type of the node.
        /// </summary>
        /// <param name="node">The node to be processed.</param>
        /// <returns>Result returned by the selected method.</returns>
        protected Result VisitNode(TokensTreeNode node)
        {
            if (node is InnerNode)
            {
                return VisitInnerNode((InnerNode)node);
            }
            else if (node is RepeatNode)
            {
                return VisitRepeatNode((RepeatNode)node);
            }

            throw new InvalidOperationException("Invalid node type");
        }
        /// <summary>
        /// Processes an inner node of the tree.
        /// </summary>
        /// <param name="innerNode">The inner node.</param>
        /// <returns>Result of the node processing.</returns>
        protected abstract Result VisitInnerNode(InnerNode innerNode);
        /// <summary>
        /// Processes a repeat node of the tree.
        /// </summary>
        /// <param name="repeatNode">The repeat node.</param>
        /// <returns>Result of the node processing.</returns>
        protected abstract Result VisitRepeatNode(RepeatNode repeatNode);
    }

    /// <summary>
    /// Base class for visitors that perform an action for ndoes of a prefix tree, where the result
    /// is stored an reused for all occurences of the same node within the tree.
    /// </summary>
    /// <typeparam name="Result">Type of result value from processing a node.</typeparam>
    public abstract class CachedTokensTreeVisitor<Result> : TokensTreeVisitor<Result>
    {
        private Dictionary<TokensTreeNode, Result> cache = new Dictionary<TokensTreeNode, Result>();

        /// <summary>
        /// Calls <see cref="VisitInnerNode(InnerNode)"/> of <see cref="VisitRepeatNode(RepeatNode)"/>
        /// according to the type of the node. If the result for <paramref name="node"/> has already been
        /// computed, returns that result.
        /// </summary>
        /// <param name="node">The node to be processed.</param>
        /// <returns>Result returned by the selected method.</returns>
        protected Result VisitNodeCached(TokensTreeNode node)
        {
            Result result;
            if (!cache.TryGetValue(node, out result))
            {
                result = VisitNode(node);
                cache[node] = result;
            }

            return result;
        }

    }

    /// <summary>
    /// Stores shared instances of nodes.
    /// </summary>
    internal class NodeSharing
    {
        private Dictionary<TokensTreeNode, TokensTreeNode> nodes = new Dictionary<TokensTreeNode, TokensTreeNode>(NodeComparer.Comparer);

        /// <summary>
        /// Gets a shared instance for a node.
        /// </summary>
        /// <param name="node">The node to be shared.</param>
        /// <returns>A shared instance equivalent to <paramref name="node"/></returns>
        public TokensTreeNode Share(TokensTreeNode node)
        {
            TokensTreeNode sharedNode;
            if (!nodes.TryGetValue(node, out sharedNode))
            {
                nodes[node] = node;
                sharedNode = node;
            }
            return sharedNode;
        }

    }
}
