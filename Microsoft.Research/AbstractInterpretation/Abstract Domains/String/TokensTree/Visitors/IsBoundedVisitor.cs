using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Determines whether a tokens tree is bounded (contains no repeat nodes).
    /// </summary>
    internal class IsBoundedVisitor : CachedTokensTreeVisitor<bool>
    {
        public bool IsBounded(TokensTreeNode node)
        {
            return VisitNode(node);
        }
        #region TokensTreeVisitor<bool> overrides
        protected override bool VisitInnerNode(InnerNode innerNode)
        {
            foreach (var child in innerNode.children)
            {
                if (!VisitNode(child.Value))
                    return false;
            }
            return true;
        }
        protected override bool VisitRepeatNode(RepeatNode repeatNode)
        {
            return false;
        }
        #endregion
    }
}
