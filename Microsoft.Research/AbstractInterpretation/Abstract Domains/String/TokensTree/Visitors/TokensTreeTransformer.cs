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
    /// Base class for visitors that take a tokens tree and transforms it into another tokens tree.
    /// </summary>
    public abstract class TokensTreeTransformer : CachedTokensTreeVisitor<TokensTreeNode>
    {
        private readonly NodeSharing sharing = new NodeSharing();
        private readonly TokensTreeMerger merger;

        public TokensTreeTransformer(TokensTreeMerger merger)
        {
            this.merger = merger;
        }

        protected TokensTreeNode Share(TokensTreeNode tn)
        {
            return sharing.Share(tn);
        }
        protected TokensTreeNode Cutoff(TokensTreeNode tn)
        {
            return merger.Cutoff(tn);
        }
        protected TokensTreeNode Merge(TokensTreeNode left, TokensTreeNode right)
        {
            return merger.Merge(left, right);
        }

        protected InnerNode TransformTree(TokensTreeNode root)
        {
            root = VisitNodeCached(root);
            return (root is RepeatNode) ? TokensTreeBuilder.Empty() : (InnerNode)root;
        }

        protected void Transform(TokensTreeNode root)
        {
            merger.Cutoff(TransformTree(root));
        }

        protected override TokensTreeNode VisitInnerNode(InnerNode innerNode)
        {
            InnerNode newNode = null;
            foreach (var kv in innerNode.children)
            {
                TokensTreeNode tn = VisitNodeCached(kv.Value);
                if (tn != kv.Value) // Reference comparison
                {
                    if (newNode == null)
                    {
                        newNode = new InnerNode(innerNode);
                    }
                    newNode.children[kv.Key] = tn;
                }
            }

            return Share(newNode ?? innerNode);
        }
        protected override TokensTreeNode VisitRepeatNode(RepeatNode repeatNode)
        {
            return repeatNode;
        }
    }
}
