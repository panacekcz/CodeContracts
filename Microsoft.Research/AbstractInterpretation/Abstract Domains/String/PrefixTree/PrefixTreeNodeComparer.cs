using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    /// <summary>
    /// Deep comparison of prefix tree subtrees.
    /// </summary>
    public class PrefixTreeNodeComparer : IEqualityComparer<PrefixTreeNode>
    {
        public static readonly PrefixTreeNodeComparer Comparer = new PrefixTreeNodeComparer();

        private PrefixTreeNodeComparer() { }

        #region IEqualityComparer<PrefixTreeNode> implementation
        public bool Equals(PrefixTreeNode leftNode, PrefixTreeNode rightNode)
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
                PrefixTreeNode rightChild;
                if (!rightInner.children.TryGetValue(leftChild.Key, out rightChild))
                    return false;
                if (!Equals(leftChild.Value, rightChild))
                    return false;
            }

            return true;
        }

        public int GetHashCode(PrefixTreeNode obj)
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
