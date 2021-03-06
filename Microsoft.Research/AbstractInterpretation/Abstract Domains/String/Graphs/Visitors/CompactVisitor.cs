﻿// CodeContracts
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
    /// Compacts a string graph.
    /// </summary>
    /// <remarks>
    /// Compacting a string graph means:
    /// Concat nodes with one child are replaced by their child.
    /// Concat nodes with all child being max nodes are replaced by a max node.
    /// Concat nodes that are children of a concat node are merged with their parents.
    /// </remarks>
    class CompactVisitor : CopyVisitor<Void>
    {
        public Node Compact(Node root)
        {
            Void unusedData;
            return VisitNode(root, VisitContext.Root, ref unusedData);
        }

        protected override Node Visit(ConcatNode concatNode, VisitContext context, ref Void data)
        {
            if (concatNode.children.Count == 1) //Rule 1
            {
                return VisitNode(concatNode.children[0], VisitContext.Concat, ref data);
            }
            else if (concatNode.children.Count > 1 && concatNode.children.TrueForAll(child => child is MaxNode)) //Rule 2
            {
                return concatNode.children[0];
            }
            else
            {
                return new ConcatNode();
            }
        }

        protected override Node Visit(OrNode orNode, VisitContext context, ref Void data)
        {
            if (orNode.children.Count == 1)
            {
                return VisitNode(orNode.children[0], VisitContext.Or, ref data);
            }
            else
            {
                return new OrNode();
            }
        }

        protected override Node VisitChildren(OrNode orNode, Node result, ref Void data)
        {
            if(orNode.children.Count != 1 && result is OrNode)
            {
                OrNode resultOr = (OrNode)result;

                foreach (Node child in orNode.children)
                {
                    Node next = VisitNode(child, VisitContext.Or, ref data);
                    if (IsOwnedOrNode(next))
                    {
                        resultOr.children.AddRange(((OrNode)next).children);
                    }
                    else
                    {
                        resultOr.children.Add(next);
                    }
                }

                if(orNode.indegree == 1 && resultOr.children.Count == 1)
                {
                    return resultOr.children[0];
                }
            }

            return result;
        }

        private static bool IsOwnedConcatNode(Node graphNode)
        {
            return graphNode is ConcatNode && ((ConcatNode)graphNode).indegree == 1;
        }

        private static bool IsOwnedOrNode(Node graphNode)
        {
        return graphNode is OrNode && ((OrNode)graphNode).indegree == 1;
    }


        protected override Node VisitChildren(ConcatNode concatNode, Node result, ref Void data)
        {
            if (concatNode.children.Count != 1 && result is ConcatNode)
            {
                ConcatNode resultConcat = (ConcatNode)result;

                foreach (Node child in concatNode.children)
                {
                    Node next = VisitNode(child, VisitContext.Concat, ref data);
                    if (IsOwnedConcatNode(next))
                    {
                        resultConcat.children.AddRange(((ConcatNode)next).children);
                    }
                    else
                    {
                        resultConcat.children.Add(next);
                    }
                }

                if (concatNode.indegree == 1 && resultConcat.children.Count == 1)
                {
                    return resultConcat.children[0];
                }

            }

            return result;
        }
    }
}
