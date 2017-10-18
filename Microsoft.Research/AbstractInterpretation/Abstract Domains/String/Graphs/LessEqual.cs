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

// Created by Vlastimil Dort (2016-2017)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Graphs
{
  

    internal struct NodePair
    {
        internal Node left, right;
        public NodePair(Node l, Node r)
        {
            left = l;
            right = r;
        }
           
    }

    internal class LessEqual 
    {
        private readonly Dictionary<NodePair, bool> pairs = new Dictionary<NodePair, bool>();
        private readonly Dictionary<Node, int> leftDepths = new Dictionary<Node, int>(), rightDepths = new Dictionary<Node, int>();
        private int currentLeftDepth = 0, currentRightDepth = 0;
        //LengthVisitor lv = new LengthVisitor();

        public bool IsLessEqual(Node left, Node right)
        {
            //lv.ComputeLengthsFor(right);
            //lv.ComputeLengthsFor(left);
            return CheckIsNodeLessEqual(left, right);
        }

        private List<Node> ChildrenOfAsOr(Node orNode, out bool isRealChild)
        {
            if (orNode is OrNode)
            {
                isRealChild = true;
                return ((OrNode)orNode).children;
            }
            else
            {
                isRealChild = false;
                return new List<Node> { orNode };
            }
        }

        private List<Node> ChildrenOfAsConcat(Node concatNode, out bool isRealChild)
        {
            if (concatNode is ConcatNode)
            {
                isRealChild = true;
                return ((ConcatNode)concatNode).children;
            }
            else
            {
                isRealChild = false;
                return new List<Node> { concatNode };
            }
        }

        private bool IsChildLessEqual(Node left, Node right, bool isLeftChild, bool isRightChild)
        {
            var pair = new NodePair(left, right);

            bool value;
            if (!pairs.TryGetValue(pair, out value)) {
                value = CheckIsChildLessEqual(left, right, isLeftChild, isRightChild);
                pairs[pair] = value;
            }
            return value;
        }


        private bool CheckIsChildLessEqual(Node left, Node right, bool isLeftChild, bool isRightChild)
        {

            int leftDepth = 0, rightDepth = 0;

            bool isLeftBack = isLeftChild && leftDepths.TryGetValue(left, out leftDepth);
            bool isRightBack = isRightChild && rightDepths.TryGetValue(right, out rightDepth);

            if (isLeftBack != isRightBack)
                return false;

            if (isLeftBack)
            {
                // the left is a back edge
                return currentLeftDepth - leftDepth == currentRightDepth - rightDepth;
            }

            if (isLeftChild)
            {
                ++currentLeftDepth;
                leftDepths[left] = currentLeftDepth;
            }
            if (isRightChild)
            {
                rightDepths[right] = currentRightDepth;
                ++currentRightDepth;
            }
            bool value = CheckIsNodeLessEqual(left, right);
            if (isLeftChild)
            {
                leftDepths.Remove(left);
                --currentLeftDepth;
            }
            if (isRightChild)
            {
                rightDepths.Remove(right);
                --currentRightDepth;
            }

            return value;
        }

        private bool CheckIsNodeLessEqual(Node left, Node right)
        {
            var leftKind = left.Label.Kind;
            var rightKind = right.Label.Kind;

            if (leftKind == NodeKind.Bottom || rightKind == NodeKind.Max)
                return true;
            if (leftKind == NodeKind.Max || rightKind == NodeKind.Bottom)
                return false;

            if(left.Label.Kind == NodeKind.Or || rightKind == NodeKind.Or)
            {
                bool isLeftChild, isRightChild;
                foreach(var leftChild in ChildrenOfAsOr(left, out isLeftChild))
                {
                    bool lessEqFound = false;
                    foreach(var rightChild in ChildrenOfAsOr(right, out isRightChild))
                    {
                        if(IsChildLessEqual(leftChild, rightChild, isLeftChild, isRightChild))
                        {
                            lessEqFound = true;
                            break;
                        }
                    }

                    if (!lessEqFound)
                        return false;
                }

                return true;
            }
            if (leftKind == NodeKind.Concat || rightKind == NodeKind.Concat)
            {
                bool isLeftChild, isRightChild;
                var leftChildren = ChildrenOfAsConcat(left, out isLeftChild);
                var rightChildren = ChildrenOfAsConcat(right, out isRightChild);
                if (leftChildren.Count != rightChildren.Count)
                    return false;

                for(int i = 0; i < leftChildren.Count; ++i)
                {
                    if (!IsChildLessEqual(leftChildren[i], rightChildren[i], isLeftChild, isRightChild))
                        return false;
                }
                return true;

            }
            if (leftKind == NodeKind.Char || rightKind == NodeKind.Char)
            {
                
                return ((CharNode)left).Value == ((CharNode)right).Value;
            }

            throw new InvalidOperationException();
        }

    }
}
