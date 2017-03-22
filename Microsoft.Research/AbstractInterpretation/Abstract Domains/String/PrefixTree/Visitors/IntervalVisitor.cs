using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    /// <summary>
    /// Base for forward visitors which compute an interval of indices for each node.
    /// </summary>
    internal abstract class IntervalVisitor : ForwardVisitor<IndexInterval>
    {
        #region ForwardVisitor overrides
        protected override IndexInterval Default()
        {
            return IndexInterval.Unreached;
        }

        protected override IndexInterval Merge(IndexInterval oldData, IndexInterval newData)
        {
            return oldData.Join(newData);
        }
        protected void PushChildren(InnerNode node, IndexInterval currentInterval)
        {
            foreach (var c in node.children)
            {
                Push(c.Value, currentInterval.Add(1));
            }
        }
        #endregion
    }
    /// <summary>
    /// Base for forward visitors which compute a congruence of indices for each node.
    /// </summary>
    internal abstract class CongruenceVisitor : ForwardVisitor<Congruence>
    {
        #region ForwardVisitor overrides
        protected override Congruence Default()
        {
            return Congruence.Unreached;
        }

        protected override Congruence Merge(Congruence oldData, Congruence newData)
        {
            return oldData.Join(newData);
        }
        protected void PushChildren(InnerNode node, Congruence currentInterval)
        {
            foreach (var c in node.children)
            {
                Push(c.Value, currentInterval.Add(1));
            }
        }
        #endregion
    }

    /// <summary>
    /// 
    /// </summary>
    class MustContainVisitor : ForwardVisitor<IndexInt>
    {
        private readonly KMP constantKmp;
        private readonly bool fixedEnd;
        private bool fail;

        public MustContainVisitor(string constant, bool fixedEnd)
        {
            this.constantKmp = new KMP(constant);
            this.fixedEnd = fixedEnd;
        }


        protected override IndexInt Default()
        {
            return IndexInt.Negative;
        }

        protected override IndexInt Merge(IndexInt oldData, IndexInt newData)
        {
            return IndexUtils.JoinIndices(oldData, newData);
        }

        private bool IsAcceptingState(IndexInt index)
        {
            if (index.IsInfinite)
                return false;
            if (index.IsNegative)
                return true;
            return index == constantKmp.End;
        }
        private IndexInt Next(IndexInt state, char c)
        {
            if (state.IsInfinite || state.IsNegative)
                return state;
            else
            {
                int nextIndex = constantKmp.Next(state.AsInt, c);

                if (!fixedEnd && nextIndex == constantKmp.End)
                    return IndexInt.Negative;

                return IndexInt.For(nextIndex);
            }
            
        }

        protected override void VisitInnerNode(InnerNode node)
        {
            if (fail)
                return;

            IndexInt index = Get(node);

            if (node.Accepting && !IsAcceptingState(index))
            {
                fail = true;
            }
            else
            {
                foreach (var c in node.children)
                {
                    Push(c.Value, Next(index, c.Key));
                }
            }
        }

        public bool MustContain(InnerNode root)
        {
            fail = false;
            Push(root, IndexInt.For(0));
            this.Traverse(root);
            return !fail;
        }
    }


    /// <summary>
    /// Finds all states that can occur in an interval of indices.
    /// </summary>
    internal class CongruenceFilterVisitor : CongruenceVisitor
    {
        private readonly HashSet<InnerNode> candidateNodes;
        private readonly IndexInterval markInterval;
        private readonly int repeatDivisor;
 
        public CongruenceFilterVisitor(HashSet<InnerNode> candidateNodes, IndexInterval markInterval, int repeatDivisor)
        {
            this.candidateNodes = candidateNodes;
            this.markInterval = markInterval;
            this.repeatDivisor = repeatDivisor;
        }

        /// <summary>
        /// Decides, whether a node which has indices satisfying <paramref name="congruence"/>, can be inside
        /// the markInterval.
        /// </summary>
        /// <param name="congruence">Congruence of node indices.</param>
        /// <returns>True, if the node can have index from markInterval.</returns>
        private bool Compatible(Congruence congruence)
        {
            Congruence congruenceWithRepeat = congruence.WithDivisor(repeatDivisor);
            if (markInterval.UpperBound - markInterval.LowerBound >= congruenceWithRepeat.Divisor)
            {
                return true;
            }
            else
            {
                var lowerRemainder = congruenceWithRepeat.RemainderFor(markInterval.LowerBound.AsInt);
                var upperRemainder = congruenceWithRepeat.RemainderFor(markInterval.UpperBound.AsInt);
                if (lowerRemainder < upperRemainder)
                {
                    return congruenceWithRepeat.Remainder >= lowerRemainder && congruenceWithRepeat.Remainder <= upperRemainder;
                }
                else if(lowerRemainder > upperRemainder)
                {
                    return congruenceWithRepeat.Remainder <= upperRemainder || congruenceWithRepeat.Remainder >= lowerRemainder;
                }
                else /*lowerRemainder == upperRemainder*/
                {
                    return congruenceWithRepeat.Remainder == lowerRemainder;
                }
            }
        }

        protected override void VisitInnerNode(InnerNode node)
        {
            Congruence nodeCongruence = Get(node);
            if (!Compatible(nodeCongruence))
            {
                candidateNodes.Remove(node);
            }

            PushChildren(node, nodeCongruence);
        }

        public void FilterCandidateNodes(InnerNode root)
        {
            if(markInterval.IsUpperBoundPlusInfinity || markInterval.UpperBound - markInterval.LowerBound >= repeatDivisor)
            {
                // The interval is large to contain all remainders, no need to compute anything
                return;
            }

            Push(root, Congruence.For(0));
            Traverse(root);
        }
    }



    /// <summary>
    /// Finds all states that can occur in an interval of indices.
    /// </summary>
    internal class IntervalMarkVisitor : IntervalVisitor
    {
        bool bounded;
        private HashSet<InnerNode> startStates = new HashSet<InnerNode>();
        private IndexInterval marking;

        public HashSet<InnerNode> Nodes
        {
            get
            {
                return startStates;
            }
        }

        public IntervalMarkVisitor(IndexInterval markInterval, bool bounded)
        {
            marking = markInterval;
            this.bounded = bounded;
        }

        public IndexInterval GetIndexInterval(InnerNode node)
        {
            return Get(node);
        }

        public void Collect(InnerNode root, int repeatDivisor)
        {
            Push(root, IndexInterval.For(0));
            Traverse(root);

            CongruenceFilterVisitor congruenceFilter = new CongruenceFilterVisitor(startStates, marking, repeatDivisor);
            congruenceFilter.FilterCandidateNodes(root);
        }

        protected override void VisitInnerNode(InnerNode node)
        {
            IndexInterval nodeInterval = Get(node);
            if (nodeInterval.LowerBound <= marking.UpperBound && (!bounded || nodeInterval.UpperBound >= marking.LowerBound))
            {
                startStates.Add(node);
            }
            PushChildren(node, nodeInterval);
        }
    }
}
