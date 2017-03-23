﻿using Microsoft.Research.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{


    class BidirectionalSearch : ForwardSearch
    {
        private Dictionary<InnerNodePair, List<InnerNodePair>> predecesors = new Dictionary<InnerNodePair, List<InnerNodePair>>();
        private HashSet<InnerNodePair> knownBackwardPairs = new HashSet<InnerNodePair>();
        private readonly WorkList<InnerNodePair> pendingBackwardPairs = new WorkList<InnerNodePair>();

        public BidirectionalSearch(InnerNode leftRoot, InnerNode rightRoot, bool allStarts) : base(leftRoot, rightRoot, allStarts)
        {
        }

        private void AddPredecesor(InnerNode left, InnerNode right, TokensTreeNode leftChild, TokensTreeNode rightChild)
        {
            InnerNodePair fr = new InnerNodePair(left, right);
            InnerNodePair ch = new InnerNodePair(leftChild.ToInner(leftRoot), rightChild.ToInner(rightRoot));

            List<InnerNodePair> lst;
            if(predecesors.TryGetValue(ch, out lst))
            {
                lst.Add(fr);
            }
            else
            {
                predecesors[ch] = new List<InnerNodePair> { fr };
            }
        }

        public override bool Next(InnerNode left, InnerNode right)
        {
            foreach (var child in left.children)
            {
                TokensTreeNode rightChild;
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
                if (predecesors.ContainsKey(pair))
                {
                    foreach (var npr in predecesors[pair])
                    {
                        RequestBackward(npr);
                    }
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

        public HashSet<InnerNode> GetStartsAndEnds()
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
    /// <summary>
    /// Finds all nodes in a graph, where an occurence of a strings from the other graph CAN possibly end.
    /// </summary>
    /// <remarks>
    /// Works by starting that the occurence can start anywhere and then working forward
    /// the complexity is quadratic if the size of alphabet is constant.
    /// That is, for each pair, we do constant amount of work.
    /// </remarks>
    class ForwardSearch : TokensTreeRelation
    {
        private readonly bool allStarts;

        /// <summary>
        /// Initializes search of a language in another language.
        /// </summary>
        /// <param name="leftRoot">Root of the left tree.</param>
        /// <param name="rightRoot">Root of the right tree.</param>
        /// <param name="allStarts">Whether the occurence can start at any node.</param>
        public ForwardSearch(InnerNode leftRoot, InnerNode rightRoot, bool allStarts) : base(leftRoot, rightRoot)
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

        public bool FoundAnyEnd()
        {
            foreach(var c in this.knownPairs)
            {
                if (c.right.Accepting)
                    return true;
            }
            return false;
        }

        public bool FoundAlignedEnd()
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
                TokensTreeNode rightChild;
                if (right.children.TryGetValue(child.Key, out rightChild))
                {
                    Request(child.Value, rightChild);

                }
            }
            return true;
        }
    }
}