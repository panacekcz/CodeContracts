using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    class  MarkSplitVisitor : PrefixTreeTransformer
    {
        HashSet<InnerNode> markedStates;


        public MarkSplitVisitor(PrefixTreeMerger merger, HashSet<InnerNode> marked) : base(merger) {
            this.markedStates = marked;
        }

        protected override PrefixTreeNode VisitInnerNode(InnerNode inn)
        {
            InnerNode ninn = (InnerNode)base.VisitInnerNode(inn);

            if (markedStates.Contains(ninn))
                return Cutoff(ninn);
            else
                return ninn;

        }

        protected override PrefixTreeNode VisitRepeatNode(RepeatNode inn)
        {
            // nothing
            return inn;
        }
        public void Split(InnerNode root)
        {
            Transform(root);
        }
        /*public void Substrings(InnerNode root, IndexInterval start, IndexInterval length)
        {
            LengthCongruenceVisitor lcv = new LengthCongruenceVisitor();
            //Congruence cong = lcv.GetLengthCommonDivisor(root);

            IntervalMarkVisitor mv = new IntervalMarkVisitor(start);

            Transform(root);
        }*/
    }
    abstract class IntervalVisitor : ForwardVisitor<IndexInterval>
    {

        protected override IndexInterval Default()
        {
            return IndexInterval.Unreached;
        }

        protected override IndexInterval Merge(IndexInterval oldData, IndexInterval newData)
        {
            return oldData.Join(newData);
        }
        protected void PushChildren(InnerNode node, IndexInterval curr)
        {
            foreach (var c in node.children)
            {
                Push(c.Value, curr.Add(1));
            }
        }
}

    class IntervalMarkVisitor : IntervalVisitor
    {
        HashSet<InnerNode> startStates = new HashSet<InnerNode>();
        IndexInterval marking;

        public IntervalMarkVisitor(IndexInterval markIntervaa)
        {
            marking = markIntervaa;
        }

        public HashSet<InnerNode> Nodes
        {
            get
            {
                return startStates;
            }
        }

        public void Collect(InnerNode i)
        {
            Push(i, IndexInterval.For(0));
            Traverse(i);
        }

        protected override void VisitInnerNode(InnerNode node)
        {
            IndexInterval nodeInterval = Get(node);
            if (!nodeInterval.Meet(marking).IsBottom)
            {
                startStates.Add(node);
            }

            PushChildren(node, nodeInterval);
        }
    }
}
