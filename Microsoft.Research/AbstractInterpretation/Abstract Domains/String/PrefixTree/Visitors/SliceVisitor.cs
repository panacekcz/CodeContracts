using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    internal class SplitVisitor : PrefixTreeTransformer
    {
        private readonly HashSet<InnerNode> splitStates;

        public SplitVisitor(PrefixTreeMerger merger, HashSet<InnerNode> splitStates) : base(merger) {
            this.splitStates = splitStates;
        }

        protected override PrefixTreeNode VisitInnerNode(InnerNode innerNode)
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


    internal class SliceBeforeVisitor : PrefixTreeTransformer
    {
        
        private readonly HashSet<InnerNode> mayEnd;
        IntervalMarkVisitor imv;
        IndexInt upper;

        public SliceBeforeVisitor(PrefixTreeMerger merger, HashSet<InnerNode> mayEnd, IntervalMarkVisitor imv, IndexInt upper) : base(merger)
        {
            this.mayEnd = mayEnd;
            this.imv = imv;
            this.upper = upper;
        }

        protected override PrefixTreeNode VisitInnerNode(InnerNode innerNode)
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
    internal class SliceAfterVisitor : PrefixTreeTransformer
    {
        private readonly HashSet<InnerNode> splitStates;

        public SliceAfterVisitor(PrefixTreeMerger merger, HashSet<InnerNode> splitStates) : base(merger) {
            this.splitStates = splitStates;
        }

        protected override PrefixTreeNode VisitInnerNode(InnerNode innerNode)
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
