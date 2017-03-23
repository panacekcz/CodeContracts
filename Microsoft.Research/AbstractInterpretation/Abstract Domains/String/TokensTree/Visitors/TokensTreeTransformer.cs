using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    public abstract class TokensTreeTransformer : CachedTokensTreeVisitor<TokensTreeNode>
    {
        private readonly NodeSharing sharing = new NodeSharing();
        private readonly TokensTreeMerger merger;

        public TokensTreeTransformer(TokensTreeMerger merger)
        {
            this.merger = merger;
        }

        protected TokensTreeNode Share(TokensTreeNode tn)
        {
            return sharing.Share(tn);
        }
        protected TokensTreeNode Cutoff(TokensTreeNode tn)
        {
            return merger.Cutoff(tn);
        }
        protected TokensTreeNode Merge(TokensTreeNode left, TokensTreeNode right)
        {
            return merger.Merge(left, right);
        }

        protected InnerNode TransformTree(TokensTreeNode root)
        {
            root = VisitNodeCached(root);
            return (root is RepeatNode) ? TokensTreeBuilder.Empty() : (InnerNode)root;
        }

        protected void Transform(TokensTreeNode root)
        {
            merger.Cutoff(TransformTree(root));
        }

        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode newNode = null;
            foreach (var kv in innerNode.children)
            {
                TokensTreeNode tn = VisitNodeCached(kv.Value);
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
        protected override TokensTreeNode VisitRepeatNode(RepeatNode repeatNode)
        {
            return repeatNode;
        }
    }
}
