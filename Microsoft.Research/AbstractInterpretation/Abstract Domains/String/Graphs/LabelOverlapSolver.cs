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
    /// Solves principal label set overlaps.
    /// </summary>
    internal class LabelOverlapSolver
    {
        /// <summary>
        /// Set of principal labels
        /// </summary>
        private readonly HashSet<Label> labels = new HashSet<Label>();
        /// <summary>
        /// Set of principal labels that occur more than once
        /// </summary>
        private readonly HashSet<Label> duplicateLabels = new HashSet<Label>();
        private readonly HashSet<Node> overlappingNodes = new HashSet<Node>();
        private readonly HashSet<Node> newNfr = new HashSet<Node>();

        /// <summary>
        /// Adds principal labels of <paramref name="node"/> to the sets of labels.
        /// </summary>
        /// <param name="node">Graph node from the NFR set.</param>
        private void CollectLabels(Node node)
        {
            Label label = node.Label;
            if (label.Kind == NodeKind.Or)
            {
                // For OR nodes, recursively process child nodes
                OrNode on = (OrNode)node;
                foreach (var nn in on.children)
                {
                    CollectLabels(nn);
                }
            }
            else
            {
                if (labels.Contains(label))
                {
                    // If the label was already seen, add it to duplicates
                    duplicateLabels.Add(label);
                }
                else
                {
                    labels.Add(label);
                }
            }
        }
        /// <summary>
        /// Finds all nodes in the subtree of <paramref name="node"/>, which have a duplicate label in PRLB
        /// </summary>
        /// <param name="node">Graph node from the NFR set.</param>
        /// <returns>Whether node is overlapping or not.</returns>
        private bool CollectOverlappingNodes(Node node)
        {
            Label label = node.Label;
            if (label.Kind == NodeKind.Or)
            {
                //An OR node is overlapping, if any of its children is overlapping.
                bool overlappingChild = false;
                OrNode on = (OrNode)node;
                //Recursively find overlapping nodes in the subtrees
                foreach (var nn in on.children)
                {
                    overlappingChild |= CollectOverlappingNodes(nn);
                }
                if (overlappingChild)
                {
                    overlappingNodes.Add(node);
                }
                return overlappingChild;
            }
            else
            {
                //A non-OR node is overlapping, if it has a duplicate label.
                if (duplicateLabels.Contains(label))
                {
                    overlappingNodes.Add(node);
                    return true;
                }
                else
                    return false;
            }
        }

        /// <summary>
        /// Find front nodes, that is the highest possible nodes that are not overlapping OR nodes.
        /// </summary>
        /// <param name="node">A node from the old NFR.</param>
        private void CollectFrontNodes(Node node)
        {
            Label label = node.Label;
            if (label.Kind == NodeKind.Or)
            {
                if (overlappingNodes.Contains(node))
                {
                    OrNode on = (OrNode)node;
                    foreach (var nn in on.children)
                    {
                        CollectFrontNodes(nn);
                    }
                }
                else
                {
                    newNfr.Add(node);
                }
            }
            else
            {
                newNfr.Add(node);
            }
        }

        /// <summary>
        /// Take a NFR set of graph nodes and remove overlapping OR nodes,
        /// taking their descendants instead.
        /// </summary>
        /// <param name="nfr">The NFR set og the nodes.</param>
        public void Solve(HashSet<Node> nfr)
        {
            foreach (Node n in nfr)
            {
                CollectLabels(n);
            }
            foreach (Node n in nfr)
            {
                CollectOverlappingNodes(n);
            }
            foreach (Node n in nfr)
            {
                CollectFrontNodes(n);
            }

            // Replace the content of nfr with the new nfr
            nfr.Clear();
            nfr.UnionWith(newNfr);
        }

    }
}
