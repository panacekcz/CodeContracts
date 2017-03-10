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

    using SGGeneratingState = Node;

    public class StringGraphRegex
    {
        private StringGraph element;

        public StringGraphRegex(StringGraph element)
        {
            this.element = element;
        }

        /*private struct SGGeneratingState
        {
            public Node sgNode;
            public SGGeneratingState(Node nd)
            {
                sgNode = nd;
            }
        }*/

        private class StringGraphGeneratingOperations : IGeneratingOperationsForRegex<SGGeneratingState>
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

            public SGGeneratingState Join(SGGeneratingState prev, SGGeneratingState next, bool widen)
            {
                List<Node> nxDif = new List<SGGeneratingState>(), prDif = new List<SGGeneratingState>();
                if (prev is ConcatNode)
                    prDif.AddRange(((ConcatNode)prev).children);
                else
                    prDif.Add(prev);

                if (next is ConcatNode)
                    nxDif.AddRange(((ConcatNode)next).children);
                else
                    nxDif.Add(next);

                int common = 0;
                while (common < nxDif.Count && common < prDif.Count && nxDif[common] == prDif[common])
                    ++common;

                ConcatNode cn = new ConcatNode();

                cn.children.AddRange(nxDif.Take(common));

                ConcatNode c1 = new ConcatNode();
                c1.children.AddRange(nxDif.Skip(common));
                ConcatNode c2 = new ConcatNode();
                c2.children.AddRange(nxDif.Skip(common));

                OrNode orNode = new OrNode(new[] { c1.Compact(), c2.Compact() });
                cn.children.Add(orNode);

                return cn.Compact();
            }

            public SGGeneratingState AddChar(SGGeneratingState prev, CharRanges ranges, bool closed)
            {
                var charIntervals = ranges.ToIntervals();
                Node closedNode = NodeBuilder.CreateNodeForIntervals(charIntervals);

                ConcatNode concatNode = new ConcatNode(new[] { prev, closedNode });

                return Wrap(concatNode, closed);
            }

            public SGGeneratingState Loop(SGGeneratingState prev, SGGeneratingState loop, SGGeneratingState last, IndexInt min, IndexInt max)
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
                //We dont know
                //TODO: VD: what if underapproximating
                return true;
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
            GeneratingInterpretation<SGGeneratingState> interpretation = new GeneratingInterpretation<SGGeneratingState>(operations);
            ForwardRegexInterpreter<GeneratingState<SGGeneratingState>> interpreter = new ForwardRegexInterpreter<GeneratingState<SGGeneratingState>>(interpretation);


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
            GeneratingInterpretation<SGGeneratingState> interpretation = new GeneratingInterpretation<SGGeneratingState>(operations);
            ForwardRegexInterpreter<GeneratingState<SGGeneratingState>> interpreter = new ForwardRegexInterpreter<GeneratingState<SGGeneratingState>>(interpretation);

            var r = interpreter.Interpret(regex);


            StringGraph underapproximation = new StringGraph(r.Open);

            bool mustMatch = element.LessThanEqual(underapproximation);

            return ProofOutcomeUtils.Build(!canMatchGraph.IsBottom, !mustMatch);
        }

    }
}
