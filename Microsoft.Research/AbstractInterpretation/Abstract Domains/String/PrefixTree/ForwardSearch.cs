using Microsoft.Research.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    class NodeCollectVisitor : PrefixTreeVisitor<Void>
    {
        private HashSet<InnerNode> nodes = new HashSet<InnerNode>();

        public HashSet<InnerNode> Nodes { get { return nodes; } }
        public void Collect(PrefixTreeNode node)
        {
            VisitNode(node);
        }
        protected override Void VisitInnerNode(InnerNode inn)
        {
            if (!nodes.Contains(inn))
            {
                nodes.Add(inn);
                foreach(var child in inn.children)
                {
                    VisitNode(child.Value);
                }
            }

            return Void.Value;
        }

        protected override Void VisitRepeatNode(RepeatNode inn)
        {
            return Void.Value;
        }
    }

    class PrefixTreeBackwardSearch : PrefixTreeForwardSearch
    {
        private Dictionary<InnerNodePair, List<InnerNodePair>> predecesors = new Dictionary<InnerNodePair, List<InnerNodePair>>();
        private HashSet<InnerNodePair> knownBackwardPairs = new HashSet<InnerNodePair>();
        private readonly WorkList<InnerNodePair> pendingBackwardPairs = new WorkList<InnerNodePair>();

        public PrefixTreeBackwardSearch(InnerNode leftRoot, InnerNode rightRoot, bool allStarts) : base(leftRoot, rightRoot, allStarts)
        {
        }

        private void AddPredecesor(InnerNode left, InnerNode right, PrefixTreeNode leftChild, PrefixTreeNode rightChild)
        {
            InnerNodePair fr = new InnerNodePair(left, right);
            InnerNodePair ch = new InnerNodePair(leftChild.ToInner(leftRoot), rightChild.ToInner(rightRoot));

            List<InnerNodePair> lst;
            if(predecesors.TryGetValue(fr, out lst))
            {
                lst.Add(ch);
            }
            else
            {
                predecesors[fr] = new List<InnerNodePair> { ch };
            }
        }

        public override bool Next(InnerNode left, InnerNode right)
        {
            foreach (var child in left.children)
            {
                PrefixTreeNode rightChild;
                if (right.children.TryGetValue(child.Key, out rightChild))
                {
                    AddPredecesor(left, right, child.Value, rightChild);
                    Request(child.Value, rightChild);

                }
            }
            return true;
        }

        private void RequestBackward(InnerNodePair pr)
        {
            if (knownBackwardPairs.Add(pr))
                pendingBackwardPairs.Add(pr);
        }

        public void BackwardStage(bool allEnds)
        {
            //Init
            foreach(var pr in knownPairs)
            {
                if (pr.right.Accepting && (allEnds || pr.left.Accepting))
                {
                    RequestBackward(pr);
                }
            }
            //Solve


            while (!pendingBackwardPairs.IsEmpty)
            {
                var pair = pendingBackwardPairs.Pull();
                foreach(var npr in predecesors[pair])
                {
                    RequestBackward(npr);
                }

            }

        }

        public HashSet<InnerNode> GetStarts()
        {
            HashSet<InnerNode> endpoints = new HashSet<InnerNode>();
            foreach (var pr in knownBackwardPairs)
            {
                if (pr.right == rightRoot)
                    endpoints.Add(pr.left);
            }

            return endpoints;
        }

        public HashSet<InnerNode> GetEndpoints()
        {
            HashSet<InnerNode> endpoints = new HashSet<InnerNode>();
            foreach(var pr in knownPairs)
            {
                if (pr.right.Accepting)
                    endpoints.Add(pr.left);
            }
            foreach(var pr in knownBackwardPairs)
            {
                if (pr.right == rightRoot)
                    endpoints.Add(pr.left);
            }

            return endpoints;
        }

    }

    class PrefixTreeForwardSearch : PrefixTreeRelation
    {
        //Finds all nodes in a graph, where an occurence of a strings from the other graph CAN possibly end
        //by starting that the occurence can start anywhere and then working forward
        //the complexity is quadratic if the size of alphabet is constant

            //That is, for each pair, we do constant amount of work

        bool allStarts;

        public PrefixTreeForwardSearch(InnerNode leftRoot, InnerNode rightRoot, bool allStarts) : base(leftRoot, rightRoot)
        {
            this.allStarts = allStarts;
        }
        public override void Init()
        {
            //For Equals, startswith, align roots
            //For replace, align root with each node of the other one

            if (allStarts)
            {
                //add all nodes from left tree with rightRoot
                NodeCollectVisitor ncv = new NodeCollectVisitor();
                ncv.Collect(leftRoot);
                foreach(var l in ncv.Nodes)
                {
                    Request(l, rightRoot);
                }
            }
            else
            {
                Request(leftRoot, rightRoot);
            }
        }

        public bool AnyEnd()
        {
            foreach(var c in this.knownPairs)
            {
                if (c.right.Accepting)
                    return true;
            }
            return false;
        }

        public bool AlignedEnd()
        {
            foreach (var c in this.knownPairs)
            {
                if (c.right.Accepting && c.left.Accepting)
                    return true;
            }
            return false;
        }

        public override bool Next(InnerNode left, InnerNode right)
        {
            foreach (var child in left.children)
            {
                PrefixTreeNode rightChild;
                if (right.children.TryGetValue(child.Key, out rightChild))
                {
                    Request(child.Value, rightChild);

                }
            }
            return true;
        }
    }



#if false
    class AhoCorassick : ForwardVisitor<InnerNode>
    {
        //        private readonly Dictionary<InnerNode, InnerNode> backEdges = new Dictionary<InnerNode, InnerNode>();

        InnerNode root;

        Dictionary<InnerNode, PrefixTreeNode> fixup = new Dictionary<InnerNode, PrefixTreeNode>();
        List<InnerNode> roots = new List<InnerNode>();

        public AhoCorassick(InnerNode root)
        {
            //Traverse the tree
            //root back is root
            this.root = root;
            // for each child, the child back is Next(self.back, c)

            //If the child already has a DIFFERENT back, we need to merge somehow..
            // well basically we need to cut it off which means put a repeat node here and merge with root. While merging, we simply do the same thing for the newly merged edges. This will not affect nodes which already have their back (except for further sharing).
            // Perhaps if we use ForwardVisitor, we can know what are all the back edges - well this will not help us since the problem is how to tell the first parent that something has changed...

        }


        public InnerNode Next(InnerNode n, char c)
        {
            PrefixTreeNode next;
            if (n.children.TryGetValue(c, out next))
            {
                return next.ToInner(root);
            }
            else
            {
                return Next(Get(n), c);
            }

        }

        protected override void VisitInnerNode(InnerNode node)
        {
            //At this point we have the back edge from node,
            // we want to set back edges for children

            InnerNode bk = Get(node);
            if (bk != null)
            {

                foreach (var child in node.children)
                {
                    Push(child.Value, Next(bk, child.Key));
                }
            }
            else
            {
                //Merge with root...
                roots.Add(node);
                fixup.Add(node, RepeatNode.Repeat);
            }
        }

        protected override InnerNode Merge(InnerNode oldData, InnerNode newData)
        {
            if (oldData == newData)
                return oldData;
            else
                return null;
            //throw new NotImplementedException();
        }

        protected override InnerNode Default()
        {
            throw new InvalidOperationException();
        }
    }


    public class AhoCorassickFinder{

        AhoCorassick automaton;
        HashSet<InnerNode> beginnings;
        HashSet<InnerNode> ends;
        Dictionary<InnerNode, InnerNode> assignment;

        public void FindIn(InnerNode haystack)
        {
            // Interprets haystack, possibly modifying the ahoc tree
            // Finds all possible ends and beginings


        }
    }
#endif
}
