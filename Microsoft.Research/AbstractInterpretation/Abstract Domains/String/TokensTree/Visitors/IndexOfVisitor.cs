using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Computes interval of indices of specified nodes in a tokens tree.
    /// </summary>
    internal class IndexOfVisitor : IntervalVisitor
    {
        private IndexInterval index = IndexInterval.Unreached;
        private HashSet<InnerNode> nodes;

        /// <summary>
        /// Gets the computed interval.
        /// </summary>
        public IndexInterval Interval { get { return index; } }

        public IndexOfVisitor(HashSet<InnerNode> nodes)
        {
            this.nodes = nodes;
        }

        #region IntervalVisitor overrides
        protected override void VisitInnerNode(InnerNode node)
        {
            IndexInterval ii = Get(node);

            if (nodes.Contains(node))
                index = index.Join(ii);

            PushChildren(node, ii);
        }
        #endregion
    }
}
