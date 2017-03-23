using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    internal class SplitVisitor : TokensTreeTransformer
    {
        private readonly HashSet<InnerNode> splitStates;

        public SplitVisitor(TokensTreeMerger merger, HashSet<InnerNode> splitStates) : base(merger) {
            this.splitStates = splitStates;
        }

        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode newInnerNode = (InnerNode)base.VisitInnerNode(innerNode);

            if (splitStates.Contains(innerNode))
                return Cutoff(newInnerNode);
            else
                return newInnerNode;
        }

        public void Split(InnerNode root)
        {
            if (splitStates.Count == 0)
                return;
            // Cut off all split nodes
            Transform(root);
        }
    }


    internal class SliceBeforeVisitor : TokensTreeTransformer
    {
        
        private readonly HashSet<InnerNode> mayEnd;
        IntervalMarkVisitor imv;
        IndexInt upper;

        public SliceBeforeVisitor(TokensTreeMerger merger, HashSet<InnerNode> mayEnd, IntervalMarkVisitor imv, IndexInt upper) : base(merger)
        {
            this.mayEnd = mayEnd;
            this.imv = imv;
            this.upper = upper;
        }

        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
            if (imv.GetIndexInterval(innerNode).LowerBound >= upper)
            {
                return Share(new InnerNode(true));
            }

            InnerNode newNode = (InnerNode)base.VisitInnerNode(innerNode);

            if (mayEnd.Contains(innerNode) && !newNode.Accepting)
            {
                //set accepting to true
                InnerNode acceptingNode = new InnerNode(newNode);
                acceptingNode.accepting = true;
                return Share(acceptingNode);
            }
            else
            {
                return newNode;
            }
            
        }

        public void SliceBefore(InnerNode root)
        {
            Transform(root);
        }
    }



    /// <summary>
    /// 
    /// </summary>
    internal class SliceAfterVisitor : TokensTreeTransformer
    {
        private readonly HashSet<InnerNode> splitStates;

        public SliceAfterVisitor(TokensTreeMerger merger, HashSet<InnerNode> splitStates) : base(merger) {
            this.splitStates = splitStates;
        }

        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode newInnerNode = (InnerNode)base.VisitInnerNode(innerNode);

            if (splitStates.Contains(innerNode))
                return Cutoff(newInnerNode);
            else
                return newInnerNode;
        }

        public void Split(InnerNode root, bool bounded)
        {
            if(bounded)
                TransformTree(root);
            else
                Transform(root);
        }
    }
}
