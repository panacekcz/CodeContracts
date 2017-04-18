// CodeContracts
// 
// Copyright 2016-2017 Charles University
// 
// All rights reserved. 
// 
// MIT License
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// Created by Vlastimil Dort (2016-2017)
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Base for forward visitors which compute an interval of indices for each node.
    /// </summary>
    internal abstract class IntervalVisitor : ForwardTokensTreeVisitor<IndexInterval>
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
    internal abstract class CongruenceVisitor : ForwardTokensTreeVisitor<Congruence>
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
