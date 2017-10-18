// CodeContracts
// 
// Copyright (c) Microsoft Corporation
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

using Microsoft.Research.Regex;
using Microsoft.Research.Regex.Model;
using Microsoft.Research.AbstractDomains.Strings.Graphs;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.AbstractDomains.Strings.Regex;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Extracts a regular expression.
    /// </summary>
    internal class StringGraphRegexVisitor : Graphs.Visitor<Element, Void>
    {
        public static Element RegexForSG(Node root)
        {
            Console.WriteLine(root);

            StringGraphRegexVisitor sgv = new StringGraphRegexVisitor();
            Void v = null;
            
            Element element = sgv.VisitNode(root, VisitContext.Root, ref v);
            Concatenation rootElement = new Concatenation();
            rootElement.Parts.Add(Anchor.Begin);
            rootElement.Parts.Add(element);
            rootElement.Parts.Add(Anchor.End);
            return rootElement;
        }

        protected override Element Visit(MaxNode maxNode, VisitContext context, ref Void data)
        {
            return ModelBuilder.AllStrings();
        }

        protected override Element Visit(BottomNode bottomNode, VisitContext context, ref Void data)
        {
            // Should not happen
            throw new InvalidOperationException();
        }

        protected override Element VisitBackwardEdge(Node graphNode, Element result, VisitContext context, ref Void data)
        {
            throw new InvalidOperationException();
        }

        protected override Element Visit(OrNode orNode, VisitContext context, ref Void data)
        {
            if(orNode.indegree > 1)
            {
                return GetCharacterSet(orNode);
            }
            else
            {
                return new Union();
            }
        }

        protected override Element Visit(CharNode charNode, VisitContext context, ref Void data)
        {
            return new Character(charNode.Value);
        }

        protected override Element Visit(ConcatNode concatNode, VisitContext context, ref Void data)
        {
            if (concatNode.indegree > 1)
            {
                return GetCharacterSet(concatNode);
            }
            else
                return new Concatenation();
        }

        protected override Element VisitChildren(OrNode orNode, Element result, ref Void data)
        {
            if (orNode.indegree > 1)
                return result;
            else
            {
                Union union = new Union();
                foreach (var child in orNode.children)
                {
                    union.Patterns.Add(VisitNode(child, VisitContext.Or, ref data));
                }
                return union;
            }
        }

        protected override Element VisitChildren(ConcatNode concatNode, Element result, ref Void data)
        {
            if (concatNode.indegree > 1)
                return result;
            else
            {
                Concatenation concat = new Concatenation();
                foreach (var child in concatNode.children) {
                    concat.Parts.Add(VisitNode(child, VisitContext.Concat, ref data));
                }
                return concat;
            }
        }

        private Element GetCharacterSet(Node node)
        {
            var charset = CharSetVisitor.GetCharSet(node);
            if (charset == null)
            {
                return ModelBuilder.AllStrings();
            }
            else
            {
                CharRanges ranges = new CharRanges(charset.Select(x => new CharRange(x, x)));
                return new Loop(new Character(ranges, ranges), 0, Loop.Unbounded);
            }
        }
    }

    /// <summary>
    /// Extracts a set of possibly occuring characters.
    /// </summary>
    class CharSetVisitor : Visitor<Void, Void>
    {
        private readonly HashSet<char> chars;
        bool isTop;

        public CharSetVisitor()
        {
            this.chars = new HashSet<char>();
            isTop = false;
        }

        public static HashSet<char> GetCharSet(Node root)
        {
            Void v = null;
            CharSetVisitor csv = new CharSetVisitor();
            csv.VisitNode(root, VisitContext.Root, ref v);
            return csv.isTop ? null : csv.chars;
        }

        protected override Void Visit(ConcatNode concatNode, VisitContext context, ref Void data)
        {
            return data;
        }

        protected override Void Visit(CharNode charNode, VisitContext context, ref Void data)
        {
            chars.Add(charNode.Value);
            return data;
        }

        protected override Void Visit(MaxNode maxNode, VisitContext context, ref Void data)
        {
            isTop = true;
            return data;
        }

        protected override Void Visit(OrNode orNode, VisitContext context, ref Void data)
        {
            return data;
        }
        protected override Void Visit(BottomNode bottomNode, VisitContext context, ref Void data)
        {
            return data;
        }
    }

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

        public IEnumerable<Element> GetRegex()
        {
            return new[] { StringGraphRegexVisitor.RegexForSG(element.root) };
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

            public Node Loop(Node prev, GeneratingLoopState<Node> loop, IndexInt min, IndexInt max)
            {
                if (max == 1)
                {
                    if (min == 1)
                    {
                        // No repetition, simply concatenate
                        return Concat(prev, loop.Last);
                    }
                    else if (min == 0)
                    {
                        // Optional node
                        if (loop.resultClosed)
                        {
                            OrNode orNode = new OrNode();
                            orNode.children.Add(loop.loopClosed);
                            orNode.children.Add(NodeBuilder.CreateEmptyNode());
                            return orNode;
                        }
                        else
                        {
                            return Wrap(prev, false);
                        }
                    }
                }

                if (!underapproximate)
                {
                    return Concat(prev, Wrap(NodeBuilder.CreateLoop(loop.loopClosed), loop.resultClosed));
                }
                else if (min == 0)
                {
                    return Wrap(prev, loop.resultClosed);
                }
                else if (min == 1 && max >= 1)
                {
                    return Concat(prev, loop.Last);
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
