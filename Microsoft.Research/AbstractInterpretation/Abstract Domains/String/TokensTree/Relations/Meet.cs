using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Relates nodes of two trees to meet two trees.
    /// </summary>
    internal class MeetRelation : TokensTreeRelation
    {
        public MeetRelation(InnerNode leftRoot, InnerNode rightRoot) :
            base(leftRoot, rightRoot)
        { }

        HashSet<InnerNode> alignedAcceptingNodes = new HashSet<InnerNode>();
        Dictionary<InnerNode, HashSet<char>> usedEdges = new Dictionary<InnerNode, HashSet<char>>();

        public static InnerNode Meet(InnerNode self, InnerNode other)
        {
            if (self == other)
                return self;

            MeetRelation meetRelation = new MeetRelation(self, other);
            meetRelation.Solve();
            TokensTreeMerger merger = new TokensTreeMerger();
            MeetPruneVisitor pruneVisitor = new MeetPruneVisitor(merger, meetRelation);
            pruneVisitor.Prune(self);
            return merger.Build();

        }
        public override void Init()
        {
            Request(leftRoot, rightRoot);
        }
        public override bool Next(InnerNode left, InnerNode right)
        {

            if (right.Accepting)
            {
                alignedAcceptingNodes.Add(left);
            }

            if (!usedEdges.ContainsKey(left))
            {
                usedEdges[left] = new HashSet<char>();
            }

            foreach (var child in left.children)
            {
                TokensTreeNode rightChild;
                if (right.children.TryGetValue(child.Key, out rightChild))
                {
                    usedEdges[left].Add(child.Key);

                    Request(child.Value, rightChild);
                }
            }

            return true;

        }

        /// <summary>
        /// Prunes the tree according to a meet relation.
        /// </summary>
        internal class MeetPruneVisitor : TokensTreeTransformer
        {
            private MeetRelation meetRelation;

            public MeetPruneVisitor(
                TokensTreeMerger merger,
                MeetRelation meetRelation
                ) : base(merger)
            {
                this.meetRelation = meetRelation;
            }

            private bool IsBottom(TokensTreeNode tn)
            {
                if (tn is InnerNode)
                {
                    InnerNode inn = (InnerNode)tn;
                    return !inn.accepting && inn.children.Count == 0;
                }
                else
                {
                    return false;
                }
            }

            protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
            {
                InnerNode newNode = new InnerNode(innerNode.Accepting && meetRelation.alignedAcceptingNodes.Contains(innerNode));

                foreach (var kv in innerNode.children)
                {
                    if (meetRelation.usedEdges[innerNode].Contains(kv.Key))
                    {

                        TokensTreeNode tn = VisitNodeCached(kv.Value);
                        if (!IsBottom(tn))
                            newNode.children[kv.Key] = tn;
                    }

                }

                return Share(newNode);
            }

            public void Prune(InnerNode root)
            {
                Transform(root);
            }
        }

    }
}
