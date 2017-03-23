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
using Microsoft.Research.DataStructures;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Extension methods for tokens tree nodes.
    /// </summary>
    internal static class TokensTreeExtensions
    {
        /// <summary>
        /// Checks whether the tokens tree is bounded (contains no repeat nodes).
        /// </summary>
        /// <param name="root">Root node of the tree.</param>
        /// <returns>True, if the tree <paramref name="root"/> does not contain any repeat nodes.</returns>
        public static bool IsBounded(this InnerNode root)
        {
            IsBoundedVisitor isBoundedVisitor = new IsBoundedVisitor();
            return isBoundedVisitor.IsBounded(root);
        }
        /// <summary>
        /// Checks whether the tokens tree represents no strings.
        /// </summary>
        /// <param name="root">Root node of the tree.</param>
        /// <returns>True, if the node <paramref name="root"/> is not accepting and has no children.</returns>
        public static bool IsBottom(this InnerNode root)
        {
            return !root.Accepting && root.children.Count == 0;
        }
        /// <summary>
        /// Checks whether the tokens tree represents just an empty string.
        /// </summary>
        /// <param name="node">Root node of the tree.</param>
        /// <returns>True, if the node <paramref name="root"/> is accepting and has no children.</returns>
        public static bool IsEmpty(this InnerNode node)
        {
            return node.Accepting && node.children.Count == 0;
        }
    }
}
