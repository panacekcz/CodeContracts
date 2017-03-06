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
    internal static class Normalizer
    {
        private const int depthRestriction = 10;

        private static void MergeWithAncestor(TemporaryNode l, List<TemporaryNode> unexpandedLeafs)
        {
            //4a1
            if (l.TryReplaceBySupersetAnc())
            {
                return;
            }

            //4a2
            TemporaryNode mg = l.SelectAncestor();
            mg.DeleteSubtrees();

            if (mg.label.Kind == NodeKind.Or)
            {
                mg.nd.UnionWith(l.nd);
                mg.nfr.UnionWith(l.nd);
            }
            else
            {
                mg.label = new Label(NodeKind.Or);
                mg.nd.UnionWith(l.nd);
                mg.nfr.Clear();
                mg.nfr.UnionWith(mg.nd);
            }

            unexpandedLeafs.Add(mg);
        }

        private static void NormalizeConcatNode(TemporaryNode l, List<TemporaryNode> unexpandedLeafs)
        {
            if (l.ViolatesDepthRestriction(depthRestriction))
            {
                //4a
                MergeWithAncestor(l, unexpandedLeafs);
            }
            else
            {
                //4b
                if (l.nd.Count == 1)
                {
                    //4b1
                    ConcatNode n = (ConcatNode)l.nd.Single();
                    int arity = l.label.Arity;
                    for (int i = 0; i < arity; ++i)
                    {
                        Node ni = n.children[i];
                        TemporaryNode mi = new TemporaryNode(ni);
                        mi.label = ni.Label;

                        if (mi.label.Kind == NodeKind.Or)
                        {
                            OrNode oni = (OrNode)ni;
                            mi.nfr.UnionWith(oni.children);
                            mi.nd.UnionWith(oni.children);
                        }

                        l.AddChild(mi);
                        unexpandedLeafs.Add(mi);
                    }
                }
                else
                {
                    //4b2

                    int arity = l.label.Arity;
                    for (int i = 0; i < arity; ++i)
                    {
                        TemporaryNode mi = new TemporaryNode();
                        bool isMax = false;
                        foreach (Node nj in l.nd)
                        {
                            Node nji = ((ConcatNode)nj).children[i];
                            if (nji.Label.Kind == NodeKind.Max)
                            {
                                isMax = true;
                            }
                            mi.nd.Add(nji);
                        }
                        if (isMax)
                        {
                            mi.label = new Label(NodeKind.Max);
                        }
                        else
                        {
                            mi.label = new Label(NodeKind.Or);
                            mi.nfr.UnionWith(mi.nd);
                        }

                        l.AddChild(mi);
                        unexpandedLeafs.Add(mi);

                    }
                }
            }

        }

        private static void NormalizeOrNode(TemporaryNode l, List<TemporaryNode> unexpandedLeafs)
        {
            l.FinalizeNFR();

            // 3a Try safe ancestor
            if (l.TryReplaceBySafeAnc())
                return;

            // 3b 

            Dictionary<Label, TemporaryNode> nonOrFronNodes = new Dictionary<Label, TemporaryNode>();

            foreach (Node n in l.nfr)
            {
                if (n is OrNode)
                {
                    //Add or nodes
                    OrNode on = (OrNode)n;
                    TemporaryNode mi = new TemporaryNode(n);
                    mi.label = new Label(NodeKind.Or);
                    mi.nfr.UnionWith(on.children);
                    mi.nd.UnionWith(on.children);

                    l.AddChild(mi);
                    unexpandedLeafs.Add(mi);
                }
                // Else collect labels
                else
                {
                    TemporaryNode mi;
                    if (!nonOrFronNodes.TryGetValue(n.Label, out mi))
                    {
                        mi = new TemporaryNode(n);
                        mi.label = n.Label;
                        nonOrFronNodes[n.Label] = mi;
                    }
                    else
                    {
                        mi.nd.Add(n);
                    }
                }
            }
            // Add nodes for collected labels (could be done in the prev loop)
            foreach (TemporaryNode mi in nonOrFronNodes.Values)
            {
                l.AddChild(mi);
                unexpandedLeafs.Add(mi);
            }
        }

        internal static Node Normalize(Node root)
        {
            // Normal type graph 
            // page 272; [23],224; [24],14(27)
            // children of OR nodes have non-overlapping principal labels

            // Initialization
            TemporaryNode m0 = new TemporaryNode(root);
            m0.label = root.Label;

            if (root.Label.Kind == NodeKind.Or)
            {
                OrNode or = (OrNode)root;
                m0.nfr.UnionWith(or.children);
            }

            m0.nd.UnionWith(m0.nfr);

            List<TemporaryNode> unexpandedLeafs = new List<TemporaryNode> { m0 };

            while (unexpandedLeafs.Count != 0)
            {
                // Take any unexpanded leaf
                TemporaryNode l = unexpandedLeafs[unexpandedLeafs.Count - 1];
                unexpandedLeafs.RemoveAt(unexpandedLeafs.Count - 1);

                //Here also could remove abandoned leafs from 4a2

                //Step 1
                if (l.TryReplaceBySafeAnc())
                {
                    continue;
                }

                //Step 2
                else if (l.IsSimple())
                {
                    continue;
                }

                //Step 3
                else if (l.label.Kind == NodeKind.Or)
                {
                    NormalizeOrNode(l, unexpandedLeafs);
                }

                //Step 4
                else if (l.label.Kind == NodeKind.Concat)
                {
                    NormalizeConcatNode(l, unexpandedLeafs);
                }
            }

            Node result = m0.Build();
            return Compact(result);
        }

        public static Node Compact(Node root)
        {
            CompactVisitor compactVisitor = new CompactVisitor();
            return compactVisitor.Compact(root);
        }
    }
}
