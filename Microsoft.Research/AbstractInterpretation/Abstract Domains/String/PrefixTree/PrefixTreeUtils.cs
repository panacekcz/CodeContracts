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

// Created by Vlastimil Dort (2016)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.DataStructures;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    internal struct InnerNodePair
    {
        public readonly InnerNode left;
        public readonly InnerNode right;

        public InnerNodePair(InnerNode left, InnerNode right)
        {
            this.left = left;
            this.right = right;
        }
    }

    public abstract class PrefixTreeRelation
    {
        internal readonly HashSet<InnerNodePair> knownPairs = new HashSet<InnerNodePair>();
        private readonly WorkList<InnerNodePair> pendingPairs = new WorkList<InnerNodePair>();
        protected readonly InnerNode leftRoot, rightRoot;

        public PrefixTreeRelation(InnerNode leftRoot, InnerNode rightRoot)
        {
            this.leftRoot = leftRoot;
            this.rightRoot = rightRoot;
        }

        public abstract void Init();
        public abstract bool Next(InnerNode left, InnerNode right);

        public void Request(PrefixTreeNode left, PrefixTreeNode right)
        {
            InnerNode innerLeft = (left is RepeatNode) ? leftRoot : (InnerNode)left;
            InnerNode innerRight = (right is RepeatNode) ? rightRoot : (InnerNode)right;
            InnerNodePair innerPair = new InnerNodePair(innerLeft, innerRight);

            if (knownPairs.Add(innerPair))
            {
                pendingPairs.Add(innerPair);
            }
        }

        public bool Solve()
        {
            Init();

            while (!pendingPairs.IsEmpty)
            {
                var pair = pendingPairs.Pull();
                if (!Next(pair.left, pair.right))
                    return false;
                
            }

            return true;
        }
    }

    public class PrefixTreePreorder : PrefixTreeRelation
    {
        public PrefixTreePreorder(InnerNode leftRoot, InnerNode rightRoot):
            base(leftRoot, rightRoot)
        { }
        public static bool LessEqual(InnerNode le, InnerNode ge)
        {
            if (le == ge)
                return true;

            PrefixTreePreorder preorder = new PrefixTreePreorder(le, ge);
            return preorder.Solve();

        }
        public override void Init()
        {
            Request(leftRoot, rightRoot);
        }
        public override bool Next(InnerNode left, InnerNode right)
        {

            if (left.Accepting && !right.Accepting)
                return false;

            foreach (var child in left.children)
            {
                PrefixTreeNode rightChild;
                if (!right.children.TryGetValue(child.Key, out rightChild))
                    return false;

                Request(child.Value, rightChild);
            }

            return true;

        }
    }

    //TODO: VD: cleanup code!
    internal class MeetVisitor : PrefixTreeTransformer
    {

        HashSet<InnerNode> acc;
        Dictionary<InnerNode, HashSet<char>> used;

        public MeetVisitor(PrefixTreeMerger merger, HashSet<InnerNode> acc,
        Dictionary<InnerNode, HashSet<char>> used) : base(merger)
        {
            this.acc = acc;
            this.used = used;
        }

        private bool IsBottom(PrefixTreeNode tn)
        {
            if(tn is InnerNode)
            {
                InnerNode inn = (InnerNode)tn;
                return !inn.accepting && inn.children.Count == 0;
            }
            else
            {
                return false;
            }
        }

        protected override PrefixTreeNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode newNode = new InnerNode(innerNode.Accepting && acc.Contains(innerNode));
           
            foreach (var kv in innerNode.children)
            {
                if (used[innerNode].Contains(kv.Key))
                {

                    PrefixTreeNode tn = VisitNodeCached(kv.Value);
                    if(!IsBottom(tn))
                    newNode.children[kv.Key] = tn;
                }
            
            }

            return Share(newNode);
        }

        public void Mee(InnerNode root)
        {
            Transform(root);
        }
    }

    internal class PrefixTreeMeet : PrefixTreeRelation
    {
        public PrefixTreeMeet(InnerNode leftRoot, InnerNode rightRoot) :
            base(leftRoot, rightRoot)
        { }

        HashSet<InnerNode> acc = new HashSet<InnerNode>();
        Dictionary<InnerNode, HashSet<char>> used = new Dictionary<InnerNode, HashSet<char>>();

        public static InnerNode Meet(InnerNode le, InnerNode ge)
        {
            if (le == ge)
                return le;

            PrefixTreeMeet preorder = new PrefixTreeMeet(le, ge);
            preorder.Solve();
            PrefixTreeMerger ptm = new PrefixTreeMerger();
            MeetVisitor mv = new MeetVisitor(ptm, preorder.acc, preorder.used);
            mv.Mee(le);
            return ptm.Build();

        }
        public override void Init()
        {
            Request(leftRoot, rightRoot);
        }
        public override bool Next(InnerNode left, InnerNode right)
        {

            if (right.Accepting)
            {
                acc.Add(left);
            }

            if (!used.ContainsKey(left))
            {
                used[left] = new HashSet<char>();
            }

            foreach (var child in left.children)
            {
                PrefixTreeNode rightChild;
                if (right.children.TryGetValue(child.Key, out rightChild))
                {
                    used[left].Add(child.Key);

                    Request(child.Value, rightChild);
                }
            }

            return true;

        }
    }

    class PrefixTreeBounded : CachedPrefixTreeVisitor<bool>
    {
        public bool IsBounded(PrefixTreeNode node)
        {
            return VisitNode(node);
        }
        protected override bool VisitInnerNode(InnerNode inn)
        {
            foreach(var k in inn.children)
            {
                if (!VisitNode(k.Value))
                    return false;
            }
            return true;
        }
        protected override bool VisitRepeatNode(RepeatNode inn)
        {
            return false;
        }
    }
}
