using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Graphs
{
    internal class NFANode
    {
        internal Dictionary<char, List<NFANode>> next;
        internal List<NFANode> nextEmpty;
        
        internal Dictionary<char, List<NFANode>> prev;
        internal List<NFANode> prevEmpty;

        internal bool isLoop;

        public IEnumerable<NFANode> GetNext(char c)
        {
            List<NFANode> list;
            if (next.TryGetValue(c, out list))
                return list;
            else
                return Enumerable.Empty<NFANode>();
        }
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

        public void Solve()
        {
            while (!pending.IsEmpty)
            {
                NFAPair pair = pending.Pull();
                // for all children, add all pairs

                foreach(var nextLeft in pair.left.nextEmpty)
                {
                    Add(nextLeft, pair.right);
                }

                foreach(var nextRight in pair.right.nextEmpty)
                {
                    Add(pair.left, nextRight);
                }

                foreach(var kv in pair.left.next)
                {
                    foreach(var nextLeft in kv.Value)
                    {
                        foreach(var nextRight in pair.right.GetNext(kv.Key))
                        {
                            Add(nextLeft, nextRight);
                        }
                    }
                }

                if (pair.left.isLoop)
                {
                    foreach (var kv in pair.right.next)
                    {
                        foreach (var nextRight in kv.Value)
                        {
                            Add(pair.left, nextRight);
                        }
                    }
                }

                if (pair.right.isLoop)
                {
                    foreach (var kv in pair.left.next)
                    {
                        foreach (var nextLeft in kv.Value)
                        {
                            Add(nextLeft, pair.right);
                        }
                    }
                }
            }
        }

        public void Add(NFANode left, NFANode right)
        {
            var pair = new NFAPair(left, right);

            if (pairs.Add(pair))
            {
                pending.Add(pair);
            }
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
            List<NFANode> nodeList;
            if(!nodes.left.next.TryGetValue(val, out nodeList))
            {
                nodeList = new List<NFANode>();
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
        public MarkVisitor(NFAVisitor mapping, NFAMeetRelation relation)
        {
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
            throw new NotImplementedException();
        }

        protected override bool Visit(MaxNode maxNode, VisitContext context, ref Void data)
        {
            throw new NotImplementedException();
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
            NFAMeetRelation relation = new NFAMeetRelation();
            relation.Add(leftNfa.left, rightNfa.left);
            relation.Solve();
            //Pruning
            MarkVisitor mv = new MarkVisitor(nfv, relation);
            bool isBottom = !mv.Mark(left);

            if (isBottom)
                return new BottomNode();

            PruneVisitor pv = new PruneVisitor(mv);

            return pv.Prune(left);
        }

    }

}
