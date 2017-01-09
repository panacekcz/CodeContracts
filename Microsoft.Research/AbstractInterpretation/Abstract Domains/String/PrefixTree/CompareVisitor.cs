using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    class CompareVisitor : PrefixTreeRelation
    {
        public static bool CanBeLessEqual(InnerNode leftRoot, InnerNode rightRoot)
        {
            CompareVisitor v = new CompareVisitor(leftRoot, rightRoot);
            return v.Solve();
        }

        public CompareVisitor(InnerNode leftRoot, InnerNode rightRoot) : base(leftRoot, rightRoot)
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

    class StrictCompareVisitor : PrefixTreeRelation
    {
        public static bool CanBeLess(InnerNode leftRoot, InnerNode rightRoot)
        {
            StrictCompareVisitor v = new StrictCompareVisitor(leftRoot, rightRoot);
            return v.Solve();
        }

        public StrictCompareVisitor(InnerNode leftRoot, InnerNode rightRoot) : base(leftRoot, rightRoot)
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



    class EqualityVisitor : PrefixTreeRelation
    {
        public static bool CanBeEqual(InnerNode leftRoot, InnerNode rightRoot)
        {
            EqualityVisitor v = new EqualityVisitor(leftRoot, rightRoot);
            return !v.Solve();
        }

        public EqualityVisitor(InnerNode leftRoot, InnerNode rightRoot) : base(leftRoot, rightRoot)
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
                PrefixTreeNode rightChild;
                if (right.children.TryGetValue(child.Key, out rightChild))
                {
                    Request(child.Value, rightChild);
                }
            }
            return true;
        }
    }

}
