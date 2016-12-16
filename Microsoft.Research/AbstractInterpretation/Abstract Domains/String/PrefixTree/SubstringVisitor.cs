using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    class IntervalMarkVisitor : CachedPrefixTreeVisitor<Void>
    {
        HashSet<PrefixTreeNode> markedStates;
        IndexInterval interval;
        int depth;

        public void MarkNodes(InnerNode root, IndexInterval interval, HashSet<PrefixTreeNode> marks)
        {
            depth = 0;
            markedStates = marks;
            this.interval = interval;
            VisitNodeCached(root);

            markedStates = null;
        }

        protected override Void VisitInnerNode(InnerNode inn)
        {
            if (interval.ContainsValue(depth))
                markedStates.Add(inn);

            ++depth;
            foreach(var v in inn.children)
            {
                VisitNodeCached(v.Value);
            }
            --depth;

            return null;
        }

        protected override Void VisitRepeatNode(RepeatNode inn)
        {
            // nothing
            return null;
        }
    }


    class SubstringVisitor
    {
        HashSet<PrefixTreeNode> startStates = new HashSet<PrefixTreeNode>();

        public InnerNode Substrings(InnerNode root, IndexInterval start, IndexInterval length)
        {
            LengthCongruenceVisitor lcv = new LengthCongruenceVisitor();
            //Congruence cong = lcv.GetLengthCommonDivisor(root);

            throw new NotImplementedException();
        }

    }
}
