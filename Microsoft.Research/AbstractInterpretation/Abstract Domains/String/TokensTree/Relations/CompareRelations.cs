﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Relation on tokens tree nodes, relating nodes that are less than equal.
    /// </summary>
    class LessThanEqualRelation : TokensTreeRelation
    {
        public static bool CanBeLessEqual(InnerNode leftRoot, InnerNode rightRoot)
        {
            LessThanEqualRelation v = new LessThanEqualRelation(leftRoot, rightRoot);
            return v.Solve();
        }

        public LessThanEqualRelation(InnerNode leftRoot, InnerNode rightRoot) : base(leftRoot, rightRoot)
        {
        }

        public override void Init()
        {
            Request(leftRoot, rightRoot);
        }

        public override bool Next(InnerNode left, InnerNode right)
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
    class LessThanRelation : TokensTreeRelation
    {
        public static bool CanBeLess(InnerNode leftRoot, InnerNode rightRoot)
        {
            LessThanRelation v = new LessThanRelation(leftRoot, rightRoot);
            return v.Solve();
        }

        public LessThanRelation(InnerNode leftRoot, InnerNode rightRoot) : base(leftRoot, rightRoot)
        {
        }

        public override void Init()
        {
            Request(leftRoot, rightRoot);
        }

        public override bool Next(InnerNode left, InnerNode right)
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

        public EqualityRelation(InnerNode leftRoot, InnerNode rightRoot) : base(leftRoot, rightRoot)
        {
        }

        public override void Init()
        {
            Request(leftRoot, rightRoot);
        }

        public override bool Next(InnerNode left, InnerNode right)
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