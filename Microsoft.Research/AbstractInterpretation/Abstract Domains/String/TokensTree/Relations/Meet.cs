using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{

    //TODO: VD: cleanup code!
    internal class MeetVisitor : TokensTreeTransformer
    {

        HashSet<InnerNode> acc;
        Dictionary<InnerNode, HashSet<char>> used;

        public MeetVisitor(TokensTreeMerger merger, HashSet<InnerNode> acc,
        Dictionary<InnerNode, HashSet<char>> used) : base(merger)
        {
            this.acc = acc;
            this.used = used;
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
            InnerNode newNode = new InnerNode(innerNode.Accepting && acc.Contains(innerNode));

            foreach (var kv in innerNode.children)
            {
                if (used[innerNode].Contains(kv.Key))
                {

                    TokensTreeNode tn = VisitNodeCached(kv.Value);
                    if (!IsBottom(tn))
                        newNode.children[kv.Key] = tn;
                }

            }

            return Share(newNode);
        }

        public void Mee(InnerNode root)
        {
            Transform(root);
        }
    }

    internal class PrefixTreeMeet : TokensTreeRelation
    {
        public PrefixTreeMeet(InnerNode leftRoot, InnerNode rightRoot) :
            base(leftRoot, rightRoot)
        { }

        HashSet<InnerNode> acc = new HashSet<InnerNode>();
        Dictionary<InnerNode, HashSet<char>> used = new Dictionary<InnerNode, HashSet<char>>();

        public static InnerNode Meet(InnerNode le, InnerNode ge)
        {
            if (le == ge)
                return le;

            PrefixTreeMeet preorder = new PrefixTreeMeet(le, ge);
            preorder.Solve();
            TokensTreeMerger ptm = new TokensTreeMerger();
            MeetVisitor mv = new MeetVisitor(ptm, preorder.acc, preorder.used);
            mv.Mee(le);
            return ptm.Build();

        }
        public override void Init()
        {
            Request(leftRoot, rightRoot);
        }
        public override bool Next(InnerNode left, InnerNode right)
        {

            if (right.Accepting)
            {
                acc.Add(left);
            }

            if (!used.ContainsKey(left))
            {
                used[left] = new HashSet<char>();
            }

            foreach (var child in left.children)
            {
                TokensTreeNode rightChild;
                if (right.children.TryGetValue(child.Key, out rightChild))
                {
                    used[left].Add(child.Key);

                    Request(child.Value, rightChild);
                }
            }

            return true;

        }
    }
}
