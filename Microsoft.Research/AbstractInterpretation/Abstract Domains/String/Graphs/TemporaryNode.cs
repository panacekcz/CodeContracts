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

// Created by Vlastimil Dort (2015-2016)
// Master thesis String Analysis for Code Contracts

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Graphs
{
    /// <summary>
    /// Temporary node used for building the graph in the normalization
    /// algorithm.
    /// </summary>
    internal class TemporaryNode
    {
        static IEqualityComparer<HashSet<Node>> comparer = HashSet<Node>.CreateSetComparer();

        internal Label label;
        internal TemporaryNode parent;
        internal int index;

        internal readonly List<TemporaryNode> children = new List<TemporaryNode>();
        internal readonly HashSet<Node> nd = new HashSet<Node>();
        internal readonly HashSet<Node> nfr = new HashSet<Node>();

        private Node builtNode;

        public TemporaryNode()
        {
        }
        public TemporaryNode(Node source)
        {
            nd.Add(source);
        }

        public void AddChild(TemporaryNode child)
        {
            child.parent = this;
            child.index = children.Count;
            children.Add(child);
        }
        public Node Build()
        {
            if (builtNode != null)
                return builtNode;

            switch (label.Kind)
            {
                case NodeKind.Or:
                    OrNode on = new OrNode();
                    builtNode = on;
                    on.children.AddRange(children.Select(ch => ch.Build()));
                    break;
                case NodeKind.Concat:
                    ConcatNode cn = new ConcatNode();
                    builtNode = cn;
                    cn.children.AddRange(children.Select(ch => ch.Build()));
                    break;
                case NodeKind.Char:
                    builtNode = new CharNode(label.Character);
                    break;
                case NodeKind.Max:
                    builtNode = new MaxNode();
                    break;
                case NodeKind.Bottom:
                    builtNode = new BottomNode();
                    break;
            }

            return builtNode;
        }

        public bool IsSimple()
        {
            return label.Kind != NodeKind.Or && label.Kind != NodeKind.Concat;
        }

        public void ReplaceBy(TemporaryNode node)
        {
            System.Diagnostics.Debug.Assert(parent != null);
            System.Diagnostics.Debug.Assert(node != null);

            parent.children[index] = node;
        }


        public bool TryReplaceBySafeAnc()
        {
            bool seenNonOrNode = false;
            for (TemporaryNode p = parent; p != null; p = p.parent)
            {
                if (p.label.Kind != NodeKind.Or)
                {
                    seenNonOrNode = true;
                }

                if (seenNonOrNode && comparer.Equals(nd, p.nd))
                {
                    ReplaceBy(p);
                    return true;
                }
            }
            return false;
        }

        public bool TryReplaceBySupersetAnc()
        {
            for (TemporaryNode p = parent; p != null; p = p.parent)
            {
                if (p.label == label && nd.IsSubsetOf(p.nd))
                {
                    ReplaceBy(p);
                    return true;
                }
            }
            return false;
        }



        public void FinalizeNFR()
        {
            LabelOverlapSolver ol = new LabelOverlapSolver();
            ol.Solve(nfr);
        }


        public void DeleteSubtrees()
        {
            //Deletes subtrees, because nd has changed
            children.Clear();
        }
        public bool ViolatesDepthRestriction(int depthRestriction)
        {
            int ancestorCount = 0;
            for (TemporaryNode p = parent; p != null; p = p.parent)
            {
                if (p.label == label)
                {
                    ancestorCount++;
                }
            }
            return ancestorCount >= depthRestriction;
        }

        /// <summary>
        /// Determine whether the <paramref name="label"/> is in the PRLB of the node.
        /// </summary>
        /// <param name="label"></param>
        /// <returns></returns>
        public bool HasLabel(Label label)
        {
            if (label == this.label)
            {
                return true;
            }
            else if (label.Kind == NodeKind.Or)
            {
                foreach (TemporaryNode nn in children)
                {
                    if (nn.HasLabel(label))
                    {
                        return true;
                    }
                }
                return false;
            }
            else
            {
                return false;
            }
        }
        /// <summary>
        /// Determine the size of overlap between this node's nd ad the other node's nd.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int NdOverlapSize(TemporaryNode other)
        {
            HashSet<Node> overlaps = new HashSet<Node>(nd);
            overlaps.IntersectWith(other.nd);
            return overlaps.Count;
        }

        public TemporaryNode SelectAncestor()
        {
            bool seenNonOrNode = false;

            TemporaryNode selectedAncestor = null;
            int bestOverlap = 0;

            for (TemporaryNode p = parent; p != null; p = p.parent)
            {
                if (p.label.Kind != NodeKind.Or)
                    seenNonOrNode = true;

                if (seenNonOrNode && p.HasLabel(label))
                {
                    int overlap = NdOverlapSize(p);
                    if (selectedAncestor == null || overlap > bestOverlap)
                    {
                        selectedAncestor = p;
                        bestOverlap = overlap;
                    }
                }
            }

            System.Diagnostics.Debug.Assert(selectedAncestor != null);
            return selectedAncestor;
        }
    }
}
