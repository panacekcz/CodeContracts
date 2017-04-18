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
    /// Deep comparison of prefix tree subtrees.
    /// </summary>
    public class NodeComparer : IEqualityComparer<TokensTreeNode>
    {
        public static readonly NodeComparer Comparer = new NodeComparer();

        private NodeComparer() { }

        #region IEqualityComparer<PrefixTreeNode> implementation
        public bool Equals(TokensTreeNode leftNode, TokensTreeNode rightNode)
        {
            if (leftNode == rightNode)
                return true;
            if (!(leftNode is InnerNode && rightNode is InnerNode))
                return false;

            InnerNode leftInner = (InnerNode)leftNode;
            InnerNode rightInner = (InnerNode)rightNode;

            if (leftInner.children.Count != rightInner.children.Count)
                return false;

            foreach (var leftChild in leftInner.children)
            {
                TokensTreeNode rightChild;
                if (!rightInner.children.TryGetValue(leftChild.Key, out rightChild))
                    return false;
                if (!Equals(leftChild.Value, rightChild))
                    return false;
            }

            return true;
        }

        public int GetHashCode(TokensTreeNode obj)
        {
            if (obj is InnerNode)
            {
                InnerNode innerNode = (InnerNode)obj;
                int hashCode = innerNode.Accepting ? 111 : 222;

                foreach (var x in innerNode.children)
                {
                    hashCode += x.Key * x.Value.GetHashCode();
                }

                return hashCode;
            }
            else if (obj != null)
                return obj.GetHashCode();
            else
                return 0;
        }
        #endregion
    }
}
