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
    /// Relation on tokens tree nodes, relating nodes that are less than equal.
    /// </summary>
    internal class LessThanEqualRelation : TokensTreeRelation
    {
        public static bool CanBeLessEqual(InnerNode leftRoot, InnerNode rightRoot)
        {
            LessThanEqualRelation v = new LessThanEqualRelation(leftRoot, rightRoot);
            return v.Solve();
        }

        private LessThanEqualRelation(InnerNode leftRoot, InnerNode rightRoot) :
            base(leftRoot, rightRoot)
        {
        }

        protected override void Init()
        {
            Request(leftRoot, rightRoot);
        }

        protected override bool Next(InnerNode left, InnerNode right)
        {
            char leftMin = left.children.Count == 0 ? char.MaxValue : left.children.Keys.Min();
            char rightMax = right.children.Count == 0 ? char.MinValue : right.children.Keys.Max();

            if (left.Accepting)
                return true;

            if (leftMin < rightMax)
                return true;
            else if (leftMin > rightMax)
                return false;

            Request(left.children[leftMin], right.children[rightMax]);
            return true;

        }
    }

    /// <summary>
    /// Relation on tokens tree nodes, relating nodes that are less than equal.
    /// </summary>
    internal class LessThanRelation : TokensTreeRelation
    {
        public static bool CanBeLess(InnerNode leftRoot, InnerNode rightRoot)
        {
            LessThanRelation v = new LessThanRelation(leftRoot, rightRoot);
            return v.Solve();
        }

        private LessThanRelation(InnerNode leftRoot, InnerNode rightRoot) :
            base(leftRoot, rightRoot)
        {
        }

        protected override void Init()
        {
            Request(leftRoot, rightRoot);
        }

        protected override bool Next(InnerNode left, InnerNode right)
        {
            char leftMin = left.children.Count == 0 ? char.MaxValue : left.children.Keys.Min();
            char rightMax = right.children.Count == 0 ? char.MinValue : right.children.Keys.Max();

            if (left.Accepting && right.children.Count > 0)
                return true;

            if (leftMin < rightMax)
                return true;
            else if (leftMin > rightMax)
                return false;

            Request(left.children[leftMin], right.children[rightMax]);
            return true;

        }
    }

    class EqualityRelation : TokensTreeRelation
    {
        public static bool CanBeEqual(InnerNode leftRoot, InnerNode rightRoot)
        {
            EqualityRelation v = new EqualityRelation(leftRoot, rightRoot);
            return !v.Solve();
        }

        public EqualityRelation(InnerNode leftRoot, InnerNode rightRoot) :
            base(leftRoot, rightRoot)
        {
        }

        protected override void Init()
        {
            Request(leftRoot, rightRoot);
        }

        protected override bool Next(InnerNode left, InnerNode right)
        {
            //Tries to show that left and right can NOT be equal

            if (left.Accepting && right.Accepting)
                return false;

            foreach (var child in left.children)
            {
                TokensTreeNode rightChild;
                if (right.children.TryGetValue(child.Key, out rightChild))
                {
                    Request(child.Value, rightChild);
                }
            }
            return true;
        }
    }

}
