// CodeContracts
// 
// Copyright (c) Charles University
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

// Created by Vlastimil Dort (2016)

using System;
using System.Diagnostics.Contracts;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Computes congruence of possible lengths of strings represented by a token tree.
    /// </summary>
    internal class LengthCongruenceVisitor : CachedTokensTreeVisitor<CongruencePair>
    {
        /// <summary>
        /// Get common divisor of lengths of the repeated part.
        /// </summary>
        /// <param name="tree">Root of the tokens tree.</param>
        /// <returns>Common divisor of lengths from <paramref name="tree"/> to all repeat nodes.</returns>
        public int GetRepeatCommonDivisor(InnerNode tree)
        {
            CongruencePair cp = VisitNodeCached(tree);
            if (cp.Repeat.IsBottom)
                return 0;
            return cp.Repeat.CommonDivisor;
        }
        /// <summary>
        /// Get common divisor of lengths of the strings.
        /// </summary>
        /// <param name="tree">Root of the tokens tree.</param>
        /// <returns>Common divisor of lengths of strings represented by <paramref name="tree"/>.</returns>
        public int GetLengthCommonDivisor(InnerNode tree)
        {
            CongruencePair cp = VisitNodeCached(tree);
            Congruence total = cp.Total;
            return total.CommonDivisor;
        }

        #region TokensTreeVisitor<CongruencePair>
        protected override CongruencePair VisitInnerNode(InnerNode innerNode)
        {
            CongruencePair cp = new CongruencePair(Congruence.Unreached, innerNode.Accepting ? Congruence.For(0) : Congruence.Unreached);

            foreach (var child in innerNode.children)
            {
                CongruencePair innerCp = VisitNodeCached(child.Value);
                cp = cp.Join(innerCp.Add(1));
            }

            return cp;
        }
        protected override CongruencePair VisitRepeatNode(RepeatNode repeatNode)
        {
            return new CongruencePair(Congruence.For(0), Congruence.Unreached);
        }
        #endregion
    }


    /// <summary>
    /// Computes interval of possible lengths of strings represented by a token tree.
    /// </summary>
    internal class LengthIntervalVisitor : CachedTokensTreeVisitor<IndexInterval>
    {
        /// <summary>
        /// Gets the interval of lengths of string represented by a token tree.
        /// </summary>
        /// <param name="tree">Root node of the token tree.</param>
        /// <returns>Interval containing lengths of all strings represented by <paramref name="tree"/>.</returns>
        public IndexInterval GetLengthInterval(InnerNode tree)
        {
            return VisitNodeCached(tree);
        }

        #region TokensTreeVisitor<IndexInterval>
        protected override IndexInterval VisitInnerNode(InnerNode innerNode)
        {
            IndexInterval interval = innerNode.Accepting ? IndexInterval.For(0) : IndexInterval.Unreached;

            foreach (var child in innerNode.children)
            {
                IndexInterval childInterval = VisitNodeCached(child.Value);
                interval = interval.Join(childInterval.Add(1));
            }

            return interval;
        }
        protected override IndexInterval VisitRepeatNode(RepeatNode repeatNode)
        {
            // Occurence of a repeat node means that the upper bound is infinity
            // because the part can be repeated any number of times

            // The lower bound is also infinity, because the minimum length is only dependent on
            // accepting inner nodes.
            return IndexInterval.Infinity;
        }
        #endregion
    }

}
