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

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    internal static class TokensTreeExtensions
    {
        public static bool IsBounded(this InnerNode root)
        {
            IsBoundedVisitor isBoundedVisitor = new IsBoundedVisitor();
            return isBoundedVisitor.IsBounded(root);
        }
        public static bool IsBottom(this InnerNode root)
        {
            return !root.Accepting && root.children.Count == 0;
        }
        public static bool IsEmpty(this InnerNode node)
        {
            return node.Accepting && node.children.Count == 0;
        }
    }


    //TODO: VD: cleanup code!
    internal class MeetVisitor : TokensTreeTransformer
    {

        HashSet<InnerNode> acc;
        Dictionary<InnerNode, HashSet<char>> used;

        public MeetVisitor(TokensTreeMerger merger, HashSet<InnerNode> acc,
        Dictionary<InnerNode, HashSet<char>> used) : base(merger)
        {
            this.acc = acc;
            this.used = used;
        }

        private bool IsBottom(TokensTreeNode tn)
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

        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode newNode = new InnerNode(innerNode.Accepting && acc.Contains(innerNode));
           
            foreach (var kv in innerNode.children)
            {
                if (used[innerNode].Contains(kv.Key))
                {

                    TokensTreeNode tn = VisitNodeCached(kv.Value);
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

    internal class PrefixTreeMeet : TokensTreeRelation
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
            TokensTreeMerger ptm = new TokensTreeMerger();
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
                TokensTreeNode rightChild;
                if (right.children.TryGetValue(child.Key, out rightChild))
                {
                    used[left].Add(child.Key);

                    Request(child.Value, rightChild);
                }
            }

            return true;

        }
    }

}
