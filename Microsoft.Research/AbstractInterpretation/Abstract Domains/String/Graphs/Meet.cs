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

// Created by Vlastimil Dort (2016-2017)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Graphs
{
    internal class NFANode
    {
        internal Dictionary<char, HashSet<NFANode>> next = new Dictionary<char, HashSet<NFANode>>();
        internal HashSet<NFANode> nextEmpty = new HashSet<NFANode>();
        
        internal Dictionary<char, HashSet<NFANode>> prev = new Dictionary<char, HashSet<NFANode>>();
        internal HashSet<NFANode> prevEmpty = new HashSet<NFANode>();

        internal bool isLoop;

        public Dictionary<char, HashSet<NFANode>> Edges(bool backward)
        {
            return backward ? prev : next;
        }

        public HashSet<NFANode> EmptyTransition(bool backward)
        {
            return backward ? prevEmpty : nextEmpty;
        }

        public IEnumerable<NFANode> Transition(bool back, char c)
        {
            var edges = Edges(back);

            HashSet<NFANode> list;
            edges.TryGetValue(c, out list);
            return list;
        }

        /*public void Clear(bool back)
        {
            if (back)
            {
                prev.Clear();

            }
        }*/
    }

    internal struct NFAPair
    {
        internal NFANode left, right;
        public NFAPair(NFANode l, NFANode r)
        {
            left = l;
            right = r;
        }
        // Equality by default
    }

    internal class NFAMeetRelation
    {
        internal HashSet<NFAPair> pairs = new HashSet<NFAPair>();
        internal DataStructures.WorkList<NFAPair> pending = new DataStructures.WorkList<NFAPair>();
        internal NFAMeetRelation otherDirection;

        public NFAMeetRelation(NFAMeetRelation otherDirection = null)
        {
            this.otherDirection = otherDirection;
        }

        private void EdgeUsed(NFANode from, char value, NFANode to)
        {
            //TODO: implement
        }

        public void Solve(bool backward, bool createOtherDirection)
        {
            while (!pending.IsEmpty)
            {
                NFAPair pair = pending.Pull();
                // for all children, add all pairs

                foreach(var nextLeft in pair.left.EmptyTransition(backward))
                {
                    Add(nextLeft, pair.right);
                }

                foreach(var nextRight in pair.right.EmptyTransition(backward))
                {
                    Add(pair.left, nextRight);
                }

                foreach(var kv in pair.left.Edges(backward))
                {
                    var label = kv.Key;

                    foreach(var nextLeft in kv.Value)
                    {
                        var transition = pair.right.Transition(backward, label);
                        if (transition != null) {

                            EdgeUsed(pair.left, label, nextLeft);

                            foreach (var nextRight in transition)
                            {
                                EdgeUsed(pair.right, label, nextRight);

                                Add(nextLeft, nextRight);
                            }
                        }
                    }
                }

                if (pair.left.isLoop)
                {
                    bool used = false;
                    foreach (var kv in pair.right.Edges(backward))
                    {
                        foreach (var nextRight in kv.Value)
                        {
                            used |= Add(pair.left, nextRight);
                        }
                    }
                    if (used)
                    {
                        //TODO:
                    }

                }

                if (pair.right.isLoop)
                {
                    bool used = false;

                    foreach (var kv in pair.left.Edges(backward))
                    {
                        foreach (var nextLeft in kv.Value)
                        {
                            used |= Add(nextLeft, pair.right);
                        }
                    }
                    if (used)
                    {
                        //TODO:
                    }

                }
            }
        }

        public bool Add(NFANode left, NFANode right)
        {
            var pair = new NFAPair(left, right);

            if(otherDirection != null && !otherDirection.pairs.Contains(pair))
            {
                // The nodes are not related in the other direction, do not add it
                return false;
            }

            if (pairs.Add(pair))
            {
                pending.Add(pair);
            }

            return true;
        }

        public bool IsEdgeAllowed(NFANode from, char by, NFANode to)
        {
            //TODO:
            return true;
        }
    }

    /// <summary>
    /// Builds a NFA over-approximation of a string graph
    /// </summary>
    internal class NFAVisitor : Visitor<NFAPair, NFAPair>
    {
        public Dictionary<Node, NFAPair> Mapping { get { return results; } }

        private void Connect(NFAPair nodes, char val)
        {
            HashSet<NFANode> nodeList;
            if(!nodes.left.next.TryGetValue(val, out nodeList))
            {
                nodeList = new HashSet<NFANode>();
                nodes.left.next[val] = nodeList;
            }
            nodeList.Add(nodes.right);
        }

        private void ConnectEmpty(NFAPair nodes)
        {
            nodes.left.nextEmpty.Add(nodes.right);
        }



        public NFAPair NfaFor(Node root)
        {
            NFANode begin = new NFANode();
            NFANode end = new NFANode();
            NFAPair rootPair = new NFAPair(begin, end);

            VisitNode(root, VisitContext.Root, ref rootPair);

            return rootPair;
        }

        protected override NFAPair Visit(BottomNode bottomNode, VisitContext context, ref NFAPair data)
        {
            // No edges will be connected
            return data;
        }

        protected override NFAPair Visit(OrNode orNode, VisitContext context, ref NFAPair data)
        {
            // Associate the begin and end states with the node
            return data;
        }


        // Visiting children of orNode is handled by the default implementation

        protected override NFAPair VisitChildren(ConcatNode concatNode, NFAPair result, ref NFAPair data)
        {
            NFANode last = null;
            // Creates new nodes for separators
            for(int i=0; i < concatNode.children.Count; ++i)
            {
                NFAPair pair;
                pair.left = i == 0 ? data.left : last;
                if (i == concatNode.children.Count - 1)
                {
                    pair.right = data.right;
                }
                else
                {
                    pair.right = last = new NFANode();
                }
                VisitNode(concatNode.children[i], VisitContext.Concat, ref pair);
            }

            if(concatNode.children.Count == 0)
            {
                ConnectEmpty(data);
            }

            return result;
        }

        protected override NFAPair Visit(MaxNode maxNode, VisitContext context, ref NFAPair data)
        {
            // Connect a node to itself with all chars
            NFANode loopNode = new NFANode();
            loopNode.isLoop = true;
            ConnectEmpty(new NFAPair(data.left, loopNode));
            ConnectEmpty(new NFAPair(loopNode, data.right));
            return data;
        }

        protected override NFAPair Visit(CharNode charNode, VisitContext context, ref NFAPair data)
        {
            // Connect the two nodes by an edge
            Connect(data, charNode.Value);
            return data;
        }

        protected override NFAPair Visit(ConcatNode concatNode, VisitContext context, ref NFAPair data)
        {
            // Associate the begin and end states with the node
            return data;
        }

        protected override NFAPair VisitBackwardEdge(Node graphNode, NFAPair result, VisitContext context, ref NFAPair data)
        {
            return data;
        }
    }
    class MarkVisitor : Visitor<bool, Void>
    {
        private readonly NFAMeetRelation relation;
        private readonly NFAVisitor mapping;

        public MarkVisitor(NFAVisitor mapping, NFAMeetRelation relation)
        {
            this.mapping = mapping;
            this.relation = relation;
        }

        public bool Mark(Node n)
        {
            Void v;
            return VisitNode(n, VisitContext.Root, ref v);
        }

        public bool IsMarkedForPrune(Node node)
        {
            return results[node];
        }

        protected override bool Visit(ConcatNode concatNode, VisitContext context, ref Void data)
        {
            return true;
        }

        protected override bool Visit(CharNode charNode, VisitContext context, ref Void data)
        {
            NFAPair states = mapping.Mapping[charNode];
            return relation.IsEdgeAllowed(states.left, charNode.Value, states.right);
        }

        protected override bool Visit(MaxNode maxNode, VisitContext context, ref Void data)
        {
            return true;
        }

        protected override bool Visit(OrNode orNode, VisitContext context, ref Void data)
        {
            return false;
        }

        protected override bool VisitChildren(OrNode orNode, bool result, ref Void data)
        {
            foreach(var child in orNode.children)
            {
                result |= VisitNode(child, VisitContext.Or, ref data);
            }

            return result;
        }

        protected override bool VisitChildren(ConcatNode concatNode, bool result, ref Void data)
        {
            return base.VisitChildren(concatNode, result, ref data);
        }

        protected override bool VisitBackwardEdge(Node graphNode, bool result, VisitContext context, ref Void data)
        {
            return true;
        }

        protected override bool Visit(BottomNode bottomNode, VisitContext context, ref Void data)
        {
            return false;
        }
    }

    class PruneVisitor : CopyVisitor<Void>
    {
        private readonly MarkVisitor marks;
        public PruneVisitor(MarkVisitor marks)
        {
            this.marks = marks;
        }

        public Node Prune(Node n)
        {
            Void v;
            return VisitNode(n, VisitContext.Root, ref v);
        }

        protected override Node VisitChildren(OrNode orNode, Node result, ref Void data)
        {
            InnerNode newInnerNode = (InnerNode)result;
            foreach (Node child in orNode.children)
            {
                if(!marks.IsMarkedForPrune(child))
                    newInnerNode.children.Add(VisitNode(child, VisitContext.Or, ref data));
            }
            return result;
        }
    }
    class Meet
    {

        internal Node DoMeet(Node left, Node right)
        {
            //Make NFA of both trees
            NFAVisitor nfv = new NFAVisitor();
            NFAPair leftNfa = nfv.NfaFor(left);
            NFAPair rightNfa = nfv.NfaFor(right);
            //Relation
            NFAMeetRelation forwardRelation = new NFAMeetRelation();
            forwardRelation.Add(leftNfa.left, rightNfa.left);
            forwardRelation.Solve(false, true);

            NFAMeetRelation backwardRelation = new NFAMeetRelation(forwardRelation);
            backwardRelation.Add(leftNfa.right, rightNfa.right);
            backwardRelation.Solve(true, false);

            //Pruning
            MarkVisitor mv = new MarkVisitor(nfv, backwardRelation);
            bool isBottom = !mv.Mark(left);

            if (isBottom)
                return new BottomNode();

            PruneVisitor pv = new PruneVisitor(mv);

            return pv.Prune(left);
        }

    }

}
