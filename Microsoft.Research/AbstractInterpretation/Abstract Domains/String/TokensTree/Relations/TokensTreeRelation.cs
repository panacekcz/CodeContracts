using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.DataStructures;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Pair of related nodes.
    /// </summary>
    internal struct InnerNodePair
    {
        public readonly InnerNode left;
        public readonly InnerNode right;

        public InnerNodePair(InnerNode left, InnerNode right)
        {
            this.left = left;
            this.right = right;
        }
    }

    internal abstract class TokensTreeRelation
    {
        internal readonly HashSet<InnerNodePair> knownPairs = new HashSet<InnerNodePair>();
        private readonly WorkList<InnerNodePair> pendingPairs = new WorkList<InnerNodePair>();
        protected readonly InnerNode leftRoot, rightRoot;

        public TokensTreeRelation(InnerNode leftRoot, InnerNode rightRoot)
        {
            this.leftRoot = leftRoot;
            this.rightRoot = rightRoot;
        }

        public abstract void Init();
        public abstract bool Next(InnerNode left, InnerNode right);

        public void Request(TokensTreeNode left, TokensTreeNode right)
        {
            InnerNode innerLeft = (left is RepeatNode) ? leftRoot : (InnerNode)left;
            InnerNode innerRight = (right is RepeatNode) ? rightRoot : (InnerNode)right;
            InnerNodePair innerPair = new InnerNodePair(innerLeft, innerRight);

            if (knownPairs.Add(innerPair))
            {
                pendingPairs.Add(innerPair);
            }
        }

        public bool Solve()
        {
            Init();

            while (!pendingPairs.IsEmpty)
            {
                var pair = pendingPairs.Pull();
                if (!Next(pair.left, pair.right))
                    return false;

            }

            return true;
        }
    }
}
