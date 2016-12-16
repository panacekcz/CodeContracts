using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    public class PrefixTreeNodeComparer : IEqualityComparer<PrefixTreeNode>
    {
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
                if (!Equals(xchild.Value, yinn.children[xchild.Key]))
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
