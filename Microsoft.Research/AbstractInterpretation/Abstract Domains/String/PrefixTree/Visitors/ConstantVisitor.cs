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
    /// <summary>
    /// Tries to extract a constant string from a tree.
    /// </summary>
    class ConstantVisitor : PrefixTreeVisitor<bool>
    {
        private StringBuilder stringBuilder;

        /// <summary>
        /// Gets the constant string represented by prefix tree
        /// rooted in <paramref name="root"/>, or null.
        /// </summary>
        /// <param name="root">Root of the prefix tree.</param>
        /// <returns>The constant or null.</returns>
        public string GetConstant(PrefixTreeNode root)
        {
            stringBuilder = new StringBuilder();
            if (VisitNode(root))
                return stringBuilder.ToString();
            else
                return null;
        }

        #region PrefixTreeVisitor<bool> overrides
        protected override bool VisitRepeatNode(RepeatNode repeatNode)
        {
            return false;

        }
        protected override bool VisitInnerNode(InnerNode innerNode)
        {
            if (innerNode.Accepting)
            {
                return innerNode.children.Count == 0;
            }
            else
            {
                if (innerNode.children.Count != 1)
                    return false;
                foreach (var child in innerNode.children)
                {
                    stringBuilder.Append(child.Key);
                    return VisitNode(child.Value);
                }
                return false;
            }
        }
        #endregion
    }

}
