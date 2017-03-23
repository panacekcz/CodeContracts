using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    internal class PreorderRelation : TokensTreeRelation
    {
        public PreorderRelation(InnerNode leftRoot, InnerNode rightRoot) :
            base(leftRoot, rightRoot)
        { }
        public static bool LessEqual(InnerNode le, InnerNode ge)
        {
            if (le == ge)
                return true;

            PreorderRelation preorder = new PreorderRelation(le, ge);
            return preorder.Solve();

        }
        public override void Init()
        {
            Request(leftRoot, rightRoot);
        }
        public override bool Next(InnerNode left, InnerNode right)
        {

            if (left.Accepting && !right.Accepting)
                return false;

            foreach (var child in left.children)
            {
                TokensTreeNode rightChild;
                if (!right.children.TryGetValue(child.Key, out rightChild))
                    return false;

                Request(child.Value, rightChild);
            }

            return true;

        }
    }

}
