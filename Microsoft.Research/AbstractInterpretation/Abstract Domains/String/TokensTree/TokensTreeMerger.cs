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
    /// Merges multiple trees into one by cutting off some branches.
    /// </summary>
    public class TokensTreeMerger
    {
        private readonly List<InnerNode> rootsToMerge = new List<InnerNode>();

        public RepeatNode Cutoff(TokensTreeNode tn)
        {
            if (tn is RepeatNode)
                return (RepeatNode)tn;

            rootsToMerge.Add((InnerNode)tn);
            return RepeatNode.Repeat;
        }

        internal InnerNode MergeInnerNodes(InnerNode oldNode, InnerNode newNode)
        {
            InnerNode merged = new InnerNode(oldNode.Accepting || newNode.Accepting);


            foreach (var kv in oldNode.children)
            {
                TokensTreeNode otherChild;
                if (newNode.children.TryGetValue(kv.Key, out otherChild))
                    merged.children.Add(kv.Key, Merge(kv.Value, otherChild));
                else
                    merged.children.Add(kv.Key, kv.Value);
            }

            foreach (var kv in newNode.children)
            {
                if (!oldNode.children.ContainsKey(kv.Key))
                {
                    merged.children.Add(kv.Key, kv.Value);
                }
            }

            return merged;
        }

        internal virtual TokensTreeNode Merge(TokensTreeNode oldNode, TokensTreeNode newNode)
        {
            // Tree merging
            // - if any of the nodes is repeating, cut the other off
            // - if both nodes are not accepting and not repeating, merge them (defined for inner nodes, but may work for dead-end nodes too).

            if (oldNode is RepeatNode)
            {
                if (newNode is RepeatNode)
                    // Both are repeat nodes, return repeat node
                    return oldNode;
                else
                    // One of them is inner, cut it off
                    return Cutoff(newNode);
            }
            else if (newNode is RepeatNode)
            {
                // One of them is inner, cut it off
                return Cutoff(oldNode);
            }
            else
            {
                // Both are inner nodes
                return MergeInnerNodes((InnerNode)oldNode, (InnerNode)newNode);
            }
        }

        private InnerNode PopRootToMerge()
        {
            InnerNode root = rootsToMerge[rootsToMerge.Count - 1];
            rootsToMerge.RemoveAt(rootsToMerge.Count - 1);
            return root;
        }

        public InnerNode Build()
        {
            if (rootsToMerge.Count == 0)
                throw new InvalidOperationException("No nodes");

            InnerNode root = PopRootToMerge();
            while (rootsToMerge.Count > 0)
            {
                InnerNode merged = PopRootToMerge();
                root = MergeInnerNodes(root, merged);
            }

            return root;
        }

    }

    /// <summary>
    /// A variant of merger, which does not allow the result to represent more strings than
    /// union of the input languages. Branches extending over repeat nodes are ignored.
    /// </summary>
    internal class UnderapproximatingMerger : TokensTreeMerger
    {

        internal override TokensTreeNode Merge(TokensTreeNode oldNode, TokensTreeNode newNode)
        {
            // If any of the nodes is repeating, ignore the other one
            if (oldNode is RepeatNode)
            {
                return oldNode;
            }
            else if (newNode is RepeatNode)
            {
                return newNode;
            }
            else
            {
                // Both are inner nodes
                return MergeInnerNodes((InnerNode)oldNode, (InnerNode)newNode);
            }
        }
    }

    /// <summary>
    /// A variant of merger, which does not allow adding new branches to the tree,
    /// instead it cuts them off to the root.
    /// </summary>
    internal class WideningMerger : TokensTreeMerger
    {
        
        public InnerNode Widening(InnerNode oldRoot, InnerNode newRoot)
        {
            Cutoff(newRoot);
            //The oldRoot goes on top of the stack, so that it is popped first
            Cutoff(oldRoot);
            return Build();
        }


        internal override TokensTreeNode Merge(TokensTreeNode oldNode, TokensTreeNode newNode)
        {
            // Tree merging
            // - if any of the nodes is repeating, cut the other off
            // - if both nodes are not accepting and not repeating, merge them (defined for inner nodes, but may work for dead-end nodes too).

            if (oldNode is RepeatNode)
            {
                if (newNode is RepeatNode)
                    // Both are repeat nodes, return repeat node
                    return oldNode;
                else
                    // One of them is inner, cut it off
                    return Cutoff(newNode);
            }
            else if (newNode is RepeatNode)
            {
                // One of them is inner, cut it off
                return Cutoff(oldNode);
            }
            else
            {
                // Both are inner nodes
                return WideningMergeInnerNodes((InnerNode)oldNode, (InnerNode)newNode);
            }
        }

        internal TokensTreeNode WideningMergeInnerNodes(InnerNode oldNode, InnerNode newNode)
        {
            //The widening is achieved by cutting nodes, where newNode adds something new.

            if(!oldNode.Accepting && newNode.Accepting)
            {
                Cutoff(newNode);
                return Cutoff(oldNode);
            }

            foreach (var kv in newNode.children)
            {
                if (!oldNode.children.ContainsKey(kv.Key))
                {
                    Cutoff(newNode);
                    return Cutoff(oldNode);
                }
            }


            InnerNode merged = new InnerNode(oldNode.Accepting);
            foreach (var kv in oldNode.children)
            {
                TokensTreeNode otherChild;
                if (newNode.children.TryGetValue(kv.Key, out otherChild))
                    merged.children.Add(kv.Key, Merge(kv.Value, otherChild));
                else
                    merged.children.Add(kv.Key, kv.Value);
            }
            return merged;
        }
    }
}
