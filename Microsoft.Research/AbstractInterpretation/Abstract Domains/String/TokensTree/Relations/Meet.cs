// CodeContracts
// 
// Copyright 2016-2017 Charles University
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

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Relates nodes of two trees to meet two trees.
    /// </summary>
    internal class MeetRelation : TokensTreeRelation
    {
        public MeetRelation(InnerNode leftRoot, InnerNode rightRoot) :
            base(leftRoot, rightRoot)
        { }

        private HashSet<InnerNode> alignedAcceptingNodes = new HashSet<InnerNode>();
        private Dictionary<InnerNode, HashSet<char>> usedEdges = new Dictionary<InnerNode, HashSet<char>>();

        public static InnerNode Meet(InnerNode self, InnerNode other)
        {
            if (self == other)
                return self;

            MeetRelation meetRelation = new MeetRelation(self, other);
            meetRelation.Solve();
            TokensTreeMerger merger = new TokensTreeMerger();
            MeetPruneVisitor pruneVisitor = new MeetPruneVisitor(merger, meetRelation);
            pruneVisitor.Prune(self);
            return merger.Build();

        }
        protected override void Init()
        {
            Request(leftRoot, rightRoot);
        }
        protected override bool Next(InnerNode left, InnerNode right)
        {

            if (right.Accepting)
            {
                alignedAcceptingNodes.Add(left);
            }

            if (!usedEdges.ContainsKey(left))
            {
                usedEdges[left] = new HashSet<char>();
            }

            foreach (var child in left.children)
            {
                TokensTreeNode rightChild;
                if (right.children.TryGetValue(child.Key, out rightChild))
                {
                    usedEdges[left].Add(child.Key);

                    Request(child.Value, rightChild);
                }
            }

            return true;

        }

        /// <summary>
        /// Prunes the tree according to a meet relation.
        /// </summary>
        internal class MeetPruneVisitor : TokensTreeTransformer
        {
            private MeetRelation meetRelation;

            public MeetPruneVisitor(
                TokensTreeMerger merger,
                MeetRelation meetRelation
                ) : base(merger)
            {
                this.meetRelation = meetRelation;
            }

            private bool IsBottom(TokensTreeNode tn)
            {
                if (tn is InnerNode)
                {
                    InnerNode inn = (InnerNode)tn;
                    return !inn.accepting && inn.children.Count == 0;
                }
                else
                {
                    return false;
                }
            }

            protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
            {
                InnerNode newNode = new InnerNode(innerNode.Accepting && meetRelation.alignedAcceptingNodes.Contains(innerNode));

                foreach (var kv in innerNode.children)
                {
                    if (meetRelation.usedEdges[innerNode].Contains(kv.Key))
                    {

                        TokensTreeNode tn = VisitNodeCached(kv.Value);
                        if (!IsBottom(tn))
                            newNode.children[kv.Key] = tn;
                    }

                }

                return Share(newNode);
            }

            public void Prune(InnerNode root)
            {
                Transform(root);
            }
        }

    }
}
