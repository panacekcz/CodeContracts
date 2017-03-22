using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    public abstract class PrefixTreeTransformer : CachedPrefixTreeVisitor<PrefixTreeNode>
    {
        private readonly NodeSharing sharing = new NodeSharing();
        private readonly PrefixTreeMerger merger;

        public PrefixTreeTransformer(PrefixTreeMerger merger)
        {
            this.merger = merger;
        }

        protected PrefixTreeNode Share(PrefixTreeNode tn)
        {
            return sharing.Share(tn);
        }
        protected PrefixTreeNode Cutoff(PrefixTreeNode tn)
        {
            return merger.Cutoff(tn);
        }
        protected PrefixTreeNode Merge(PrefixTreeNode left, PrefixTreeNode right)
        {
            return merger.Merge(left, right);
        }

        protected InnerNode TransformTree(PrefixTreeNode root)
        {
            root = VisitNodeCached(root);
            return (root is RepeatNode) ? PrefixTreeBuilder.Empty() : (InnerNode)root;
        }

        protected void Transform(PrefixTreeNode root)
        {
            merger.Cutoff(TransformTree(root));
        }

        protected override PrefixTreeNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode newNode = null;
            foreach (var kv in innerNode.children)
            {
                PrefixTreeNode tn = VisitNodeCached(kv.Value);
                if (tn != kv.Value) // Reference comparison
                {
                    if (newNode == null)
                    {
                        newNode = new InnerNode(innerNode);
                    }
                    newNode.children[kv.Key] = tn;
                }
            }

            return Share(newNode ?? innerNode);
        }
        protected override PrefixTreeNode VisitRepeatNode(RepeatNode repeatNode)
        {
            return repeatNode;
        }
    }
}
