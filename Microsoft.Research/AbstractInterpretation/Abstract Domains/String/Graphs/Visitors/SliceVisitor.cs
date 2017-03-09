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
    /// Extracts a string graph corresponding to substrings.
    /// </summary>
    abstract class SliceVisitor : CopyVisitor<IndexInterval>
    {
        private readonly LengthVisitor lengths;

        protected SliceVisitor(LengthVisitor lengths)
        {
            this.lengths = lengths;
        }

        public Node Slice(Node root, IndexInterval index)
        {
            return VisitNode(root, VisitContext.Root, ref index);
        }

        protected override Node VisitChildren(ConcatNode concatNode, Node result, ref IndexInterval data)
        {
            IndexInterval index = concatNode.indegree > 1 ? IndexInterval.UnknownNonNegative : data;

            foreach (Node child in concatNode.children)
            {
                IndexInterval length = lengths.GetLengthFor(child);

                Node next = VisitNode(child, VisitContext.Concat, ref index);

                ((ConcatNode)result).children.Add(next);

                index = index.AfterOffset(length);
            }

            return result;
        }


        protected override Node Visit(MaxNode maxNode, VisitContext context, ref IndexInterval data)
        {
            return maxNode;
        }

        protected override Node VisitChildren(OrNode orNode, Node result, ref IndexInterval data)
        {
            IndexInterval index = orNode.indegree > 1 ? IndexInterval.UnknownNonNegative : data;

            foreach (Node child in orNode.children)
            {
                Node next = VisitNode(child, VisitContext.Or, ref index);
                ((OrNode)result).children.Add(next);
            }

            return result;
        }

        protected override Node Visit(BottomNode bottomNode, VisitContext context, ref IndexInterval data)
        {
            return bottomNode;
        }
    }
}
