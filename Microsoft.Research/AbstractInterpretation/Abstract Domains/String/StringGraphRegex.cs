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
using Microsoft.Research.Regex.AST;
using Microsoft.Research.AbstractDomains.Strings.Graphs;
using Microsoft.Research.CodeAnalysis;

namespace Microsoft.Research.AbstractDomains.Strings
{
#if vdfalse
    class StringGraphRegex
    {
        private StringGraph element;

        public StringGraphRegex(StringGraph element)
        {
            this.element = element;
        }

        private class StringGraphRegexVisitor : OpenClosedRegexVisitor<Node, RegexEndsData>
        {
            private bool overapproximate;

            public StringGraphRegexVisitor(bool overapproximate)
            {
                this.overapproximate = overapproximate;
            }

            private Node Wrap(Node inner, RegexEndsData endsData)
            {
                if (endsData.LeftClosed && endsData.RightClosed)
                {
                    return inner;
                }
                else
                {
                    ConcatNode concat = new ConcatNode();
                    if (!endsData.LeftClosed)
                    {
                        concat.children.Add(new MaxNode());
                    }
                    concat.children.Add(inner);
                    if (!endsData.RightClosed)
                    {
                        concat.children.Add(new MaxNode());
                    }
                    return concat;
                }
            }


            protected override Node VisitConcatenation(Concatenation element,
              int startIndex, int endIndex, RegexEndsData ends,
              ref RegexEndsData data)
            {
                ConcatNode concatNode = new ConcatNode();

                for (int index = startIndex; index < endIndex; ++index)
                {
                    RegexEndsData childEnds = ConcatChildEnds(ends, data, startIndex, endIndex, index);
                    concatNode.children.Add(VisitElement(element.Parts[index], ref childEnds));
                }

                return concatNode;
            }

            protected override Node Visit(Alternation element, ref RegexEndsData data)
            {
                OrNode orNode = new OrNode();

                foreach (Element child in element.Patterns)
                {
                    orNode.children.Add(VisitElement(child, ref data));
                }

                return orNode;
            }

            protected override Node Visit(SingleElement element, ref RegexEndsData data)
            {
                var intervals = overapproximate ? element.CanMatchRanges : element.MustMatchRanges;
                var charIntervals = intervals.ToIntervals();
                Node closedNode = NodeBuilder.CreateNodeForIntervals(charIntervals);
                return Wrap(closedNode, data);
            }

            protected override Node Visit(Loop element, ref RegexEndsData data)
            {
                if (element.Max == 1)
                {
                    if (element.Min == 1)
                    {
                        return VisitElement(element.Content, ref data);
                    }
                    else if (element.Min == 0)
                    {
                        OrNode orNode = new OrNode();
                        orNode.children.Add(VisitElement(element.Content, ref data));
                        orNode.children.Add(ForEmpty(data));
                        return orNode;
                    }
                }

                if (overapproximate)
                {
                    RegexEndsData closedEnds = new RegexEndsData(true, true);
                    Node contentNode = VisitElement(element.Content, ref closedEnds);
                    return Wrap(NodeBuilder.CreateLoop(contentNode), data);
                }
                else if (element.Min == 0)
                {
                    return ForEmpty(data);
                }
                else if (element.Min == 1 && element.Max >= 1)
                {
                    return VisitElement(element.Content, ref data);
                }
                else
                {
                    return new BottomNode();
                }
            }

            private Node ForEmpty(RegexEndsData ends)
            {
                if (ends.LeftClosed && ends.RightClosed)
                {
                    return new ConcatNode();
                }
                else
                {
                    return new MaxNode();
                }
            }

            protected override Node Visit(Empty element, ref RegexEndsData data)
            {
                return ForEmpty(data);
            }

            protected override Node Unsupported(Element regex, ref RegexEndsData data)
            {
                return overapproximate ? (Node)new MaxNode() : (Node)new BottomNode();
            }
        }

        public StringGraph StringGraphForRegex(Element regex)
        {
            StringGraphRegexVisitor visitor = new StringGraphRegexVisitor(true);

            RegexEndsData ends = new RegexEndsData();

            Node node = visitor.VisitSimpleRegex(regex, ref ends);

            return new StringGraph(node);
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

            StringGraphRegexVisitor visitor = new StringGraphRegexVisitor(false);

            RegexEndsData ends = new RegexEndsData();
            StringGraph underapproximation = new StringGraph(visitor.VisitSimpleRegex(regex, ref ends));

            bool mustMatch = element.LessThanEqual(underapproximation);

            return ProofOutcomeUtils.Build(!canMatchGraph.IsBottom, !mustMatch);
        }
    }
#endif
}
