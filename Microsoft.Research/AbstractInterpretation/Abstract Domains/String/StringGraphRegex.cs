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

using Microsoft.Research.Regex;
using Microsoft.Research.Regex.Model;
using Microsoft.Research.AbstractDomains.Strings.Graphs;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.AbstractDomains.Strings.Regex;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Provides regex-related functionality to <see cref="StringGraph"/>.
    /// </summary>
    public class StringGraphRegex
    {
        private StringGraph element;

        public StringGraphRegex(StringGraph element)
        {
            this.element = element;
        }

        private class StringGraphGeneratingOperations : IGeneratingOperationsForRegex<Node>
        {
            private bool underapproximate;

            public StringGraphGeneratingOperations(bool underapproximate)
            {
                this.underapproximate = underapproximate;
            }

            public bool IsUnderapproximating
            {
                get
                {
                    return underapproximate;
                }
            }

            private Node Wrap(Node inner, bool closed)
            {
                if (closed)
                {
                    return inner;
                }
                else
                {
                    return Concat(inner, new MaxNode());
                }
            }

            private void AddNodeToConcatChildren(List<Node> concatChildren, Node node)
            {
                if (node is ConcatNode)
                    concatChildren.AddRange(((ConcatNode)node).children);
                else
                    concatChildren.Add(node);
            }

            private void AddNodeToOrChildren(List<Node> orChildren, Node node)
            {
                if (node is OrNode)
                    orChildren.AddRange(((OrNode)node).children);
                else
                    orChildren.Add(node);
            }

            public Node Join(Node prev, Node next, bool widen)
            {
                // The join does not by itself introduce approximation
                if (prev is BottomNode)
                    return next;
                if (next is BottomNode)
                    return prev;

                List<Node> nextParts = new List<Node>(), prevParts = new List<Node>();
                AddNodeToConcatChildren(prevParts, prev);
                AddNodeToConcatChildren(nextParts, next);

                int commonPartCount = 0;
                while (commonPartCount < nextParts.Count && commonPartCount < prevParts.Count && nextParts[commonPartCount] == prevParts[commonPartCount])
                    ++commonPartCount;

                ConcatNode topConcat = new ConcatNode(nextParts.Take(commonPartCount));
                ConcatNode prevConcat = new ConcatNode(prevParts.Skip(commonPartCount));
                ConcatNode nextConcat = new ConcatNode(nextParts.Skip(commonPartCount));

                OrNode orNode = new OrNode();

                Node prevPartsNode = prevConcat.Compact();
                AddNodeToOrChildren(orNode.children, prevPartsNode);
                Node nextPartsNode = nextConcat.Compact();
                AddNodeToOrChildren(orNode.children, nextPartsNode);

                topConcat.children.Add(orNode);

                return topConcat.Compact();
            }

            public Node AddChar(Node prev, CharRanges ranges, bool closed)
            {
                var charIntervals = ranges.ToIntervals();
                Node closedNode = NodeBuilder.CreateNodeForIntervals(charIntervals);

                ConcatNode concatNode = new ConcatNode();
                AddNodeToConcatChildren(concatNode.children, prev);
                concatNode.children.Add(closedNode);

                return Wrap(concatNode.Compact(), closed);
            }

            public Node Loop(Node prev, Node loop, Node last, IndexInt min, IndexInt max)
            {
                if (max == 1)
                {
                    if (min == 1)
                    {
                        return Concat(prev, last);
                    }
                    else if (min == 0)
                    {
                        OrNode orNode = new OrNode();
                        orNode.children.Add(last);
                        //TODO: VD: closed/open
                        orNode.children.Add(NodeBuilder.CreateEmptyNode());
                        return orNode;
                    }
                }

                if (!underapproximate)
                {
                    return Wrap(NodeBuilder.CreateLoop(loop), (loop == last));
                }
                else if (min == 0)
                {
                    //TODO: VD: here we do not consider open/closed
                    return prev;
                }
                else if (min == 1 && max >= 1)
                {
                    return Concat(prev, last);
                }
                else
                {
                    return new BottomNode();
                }
            }
            private Node Concat(Node left, Node right)
            {

                return new ConcatNode(new[] { left, right }).Compact();
            }

            public bool CanBeEmpty(Node node)
            {
                //TODO: VD: underap?
                LengthVisitor lengths = new LengthVisitor();
                lengths.ComputeLengthsFor(node);
                IndexInterval length = lengths.GetLengthFor(node);
                return length.LowerBound == 0;
            }
            public Node Empty
            {
                get
                {
                    return new ConcatNode();
                }
            }
            public Node Top
            {
                get
                {
                    return new MaxNode();
                }
            }

            public Node Bottom
            {
                get
                {
                    return new BottomNode();
                }
            }
        }


        public StringGraph StringGraphForRegex(Element regex)
        {
            StringGraphGeneratingOperations operations = new StringGraphGeneratingOperations(false);
            GeneratingInterpretation<Node> interpretation = new GeneratingInterpretation<Node>(operations);
            ForwardRegexInterpreter<GeneratingState<Node>> interpreter = new ForwardRegexInterpreter<GeneratingState<Node>>(interpretation);

            Node node = interpreter.Interpret(regex).Open;

            CompactVisitor compacter = new CompactVisitor();
            return new StringGraph(compacter.Compact(node));
        }

        /// <summary>
        /// Verifies whether the string graph matches the specified regex expression.
        /// </summary>
        /// <param name="regex">AST of the regex.</param>
        /// <returns>Proven result of the match.</returns>
        public ProofOutcome IsMatch(Element regex)
        {
            StringGraph overapproximation = StringGraphForRegex(regex);
            StringGraph canMatchGraph = element.Meet(overapproximation);

            StringGraphGeneratingOperations operations = new StringGraphGeneratingOperations(true);
            GeneratingInterpretation<Node> interpretation = new GeneratingInterpretation<Node>(operations);
            ForwardRegexInterpreter<GeneratingState<Node>> interpreter = new ForwardRegexInterpreter<GeneratingState<Node>>(interpretation);

            var result = interpreter.Interpret(regex);

            StringGraph underapproximation = new StringGraph(result.Open);

            bool mustMatch = element.LessThanEqual(underapproximation);

            return ProofOutcomeUtils.Build(!canMatchGraph.IsBottom, !mustMatch);
        }

    }
}
