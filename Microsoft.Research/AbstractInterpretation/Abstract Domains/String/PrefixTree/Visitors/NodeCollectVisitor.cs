using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    /// <summary>
    /// Collects all nodes in a prefix tree into a set.
    /// </summary>
    class NodeCollectVisitor : PrefixTreeVisitor<Void>
    {
        private HashSet<InnerNode> nodes = new HashSet<InnerNode>();

        public HashSet<InnerNode> Nodes { get { return nodes; } }
        /// <summary>
        /// Add all nodes in tree rooted by <paramref name="node"/> to <see cref="Nodes"/>.
        /// </summary>
        /// <param name="node">Root of the tree to add.</param>
        public void Collect(PrefixTreeNode node)
        {
            VisitNode(node);
        }

        #region PrefixTreeVisitor<Void> overrides
        protected override Void VisitInnerNode(InnerNode innerNode)
        {
            if (!nodes.Contains(innerNode))
            {
                nodes.Add(innerNode);
                foreach (var child in innerNode.children)
                {
                    VisitNode(child.Value);
                }
            }

            return Void.Value;
        }
        protected override Void VisitRepeatNode(RepeatNode repeatNode)
        {
            return Void.Value;
        }
        #endregion
    }
}
