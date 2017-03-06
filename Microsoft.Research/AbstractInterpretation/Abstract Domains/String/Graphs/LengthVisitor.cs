// CodeContracts
// 
// Copyright (c) Microsoft Corporation
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

// Created by Vlastimil Dort (2015-2016)
// Master thesis String Analysis for Code Contracts

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Graphs
{
    /// <summary>
    /// Computes the minumum and maximum legths for each node
    /// of a string graph.
    /// </summary>
    internal class LengthVisitor : Visitor<IndexInterval, Void>
    {
        public void ComputeLengthsFor(Node root)
        {
            Void unusedData;
            VisitNode(root, VisitContext.Root, ref unusedData);
        }

        public IndexInterval GetLengthFor(Node node)
        {
            IndexInterval result;
            if (results.TryGetValue(node, out result))
            {
                return result;
            }
            else
            {
                return IndexInterval.UnknownNonNegative;
            }
        }

        protected override IndexInterval VisitBackwardEdge(Node graphNode, IndexInterval result, VisitContext context, ref Void data)
        {
            return IndexInterval.Infinity;
        }

        protected override IndexInterval Visit(ConcatNode concatNode, VisitContext context, ref Void data)
        {
            return IndexInterval.For(0);
        }

        protected override IndexInterval VisitChildren(ConcatNode concatNode, IndexInterval result, ref Void data)
        {
            foreach (Node child in concatNode.children)
            {
                IndexInterval next = VisitNode(child, VisitContext.Or, ref data);
                result = IndexInterval.For(result.LowerBound + next.LowerBound, result.UpperBound + next.UpperBound);
            }
            return result;
        }

        protected override IndexInterval Visit(CharNode charNode, VisitContext context, ref Void data)
        {
            return IndexInterval.For(1);
        }

        protected override IndexInterval Visit(MaxNode maxNode, VisitContext context, ref Void data)
        {
            return IndexInterval.UnknownNonNegative;
        }

        protected override IndexInterval Visit(OrNode orNode, VisitContext context, ref Void data)
        {
            return IndexInterval.Unknown.Bottom;
        }

        protected override IndexInterval VisitChildren(OrNode orNode, IndexInterval result, ref Void data)
        {
            foreach (Node child in orNode.children)
            {
                IndexInterval next = VisitNode(child, VisitContext.Or, ref data);
                result = result.Join(next);
            }
            return result;
        }

        protected override IndexInterval Visit(BottomNode bottomNode, VisitContext context, ref Void data)
        {
            return IndexInterval.Unknown.Bottom;
        }
    }
}
