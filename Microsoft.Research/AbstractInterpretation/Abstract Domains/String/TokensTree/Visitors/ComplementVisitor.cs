using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{

    /// <summary>
    /// For a bounded tokens tree, creates a tokens tree representing the complement language (or possibly more).
    /// </summary> 
    /// <remarks>
    /// In the input, there should be no repeat nodes
    /// otherwise, for each child: 
    /// - if the child is accepting, do not add it
    /// - otherwise add it and do complement on it.
    /// for other characters, add repeat nodes.
    ///</remarks>

    class ComplementVisitor : CachedTokensTreeVisitor<InnerNode>
    {
        private readonly InnerNode root;

        public static InnerNode Complement(InnerNode root)
        {
            ComplementVisitor complementVisitor = new ComplementVisitor(root);
            return complementVisitor.VisitNode(root);
        }

        private ComplementVisitor(InnerNode root)
        {
            this.root = root;
        }

        protected override InnerNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode complementNode = new InnerNode(innerNode == root);

            foreach(var c in innerNode.children)
            {
                if (!((InnerNode)c.Value).Accepting)
                {
                    complementNode.children[c.Key] = VisitNodeCached(c.Value);
                }
            }
            for(int i = char.MinValue; i <= char.MaxValue; ++i)
            {
                if (!innerNode.children.ContainsKey((char)i))
                {
                    complementNode.children[(char)i] = RepeatNode.Repeat;
                }
            }

            return complementNode;
        }

        protected override InnerNode VisitRepeatNode(RepeatNode repeatNode)
        {
            // No repeat nodes are allowed
            throw new InvalidOperationException();
        }
    }
}
