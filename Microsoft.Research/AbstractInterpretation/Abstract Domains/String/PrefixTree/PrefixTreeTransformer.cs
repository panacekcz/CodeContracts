using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    public abstract class PrefixTreeTransformer : CachedPrefixTreeVisitor<PrefixTreeNode>
    {
        private readonly TrieShare share = new TrieShare();
        private readonly PrefixTreeMerger merger;

        public PrefixTreeTransformer(PrefixTreeMerger merger)
        {
            this.merger = merger;//?? new PrefixTreeMerger();
        }

        protected PrefixTreeNode Share(PrefixTreeNode tn)
        {
            return share.Share(tn);
        }
        protected PrefixTreeNode Cutoff(PrefixTreeNode tn)
        {
            return merger.Cutoff(tn);
        }
        protected PrefixTreeNode Merge(PrefixTreeNode left, PrefixTreeNode right)
        {
            return merger.Merge(left, right);
        }

        public void Transform(PrefixTreeNode root)
        {

            root = VisitNodeCached(root);

            InnerNode newRoot = (root is RepeatNode) ? PrefixTreeBuilder.Empty() : (InnerNode)root;
            merger.Cutoff(newRoot);
            //return merger.MergeOffcuts(newRoot);
        }

        protected override PrefixTreeNode VisitInnerNode(InnerNode inn)
        {
            InnerNode newNode = null;
            foreach (var kv in inn.children)
            {
                PrefixTreeNode tn = VisitNodeCached(kv.Value);
                if (tn != kv.Value) // Reference comparison
                {
                    if (newNode == null)
                    {
                        newNode = new InnerNode(inn);
                    }
                    newNode.children[kv.Key] = tn;
                }
            }

            return Share(newNode ?? inn);
        }
        protected override PrefixTreeNode VisitRepeatNode(RepeatNode inn)
        {
            return inn;
        }
    }
}
