using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    public class PrefixTreeNodeComparer : IEqualityComparer<PrefixTreeNode>
    {
        public static readonly PrefixTreeNodeComparer Comparer = new PrefixTreeNodeComparer();

        private PrefixTreeNodeComparer() { }

        public bool Equals(PrefixTreeNode x, PrefixTreeNode y)
        {
            if (x == y)
                return true;
            if (!(x is InnerNode && y is InnerNode))
                return false;

            InnerNode xinn = (InnerNode)x;
            InnerNode yinn = (InnerNode)y;

            if (xinn.children.Count != yinn.children.Count)
                return false;

            foreach (var xchild in xinn.children)
            {
                PrefixTreeNode ychild;
                if (!yinn.children.TryGetValue(xchild.Key, out ychild))
                    return false;
                if (!Equals(xchild.Value, ychild))
                    return false;
            }

            return true;
        }

        public int GetHashCode(PrefixTreeNode obj)
        {
            if (obj is InnerNode)
            {
                InnerNode inn = (InnerNode)obj;
                int hc = inn.Accepting ? 111 : 222;
                foreach (var x in inn.children)
                {
                    hc += x.Key * x.Value.GetHashCode();
                }

                return hc;
            }
            else if (obj != null)
                return obj.GetHashCode();
            else
                return 0;
        }



    }
}
