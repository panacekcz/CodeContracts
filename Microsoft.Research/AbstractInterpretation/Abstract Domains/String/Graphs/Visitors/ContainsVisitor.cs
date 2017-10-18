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

using Microsoft.Research.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Graphs
{
    /// <summary>
    /// Searches for a string constant contained in a string graph
    /// </summary>
    class ContainsVisitor : Visitor<bool, Void>
    {
        private string needle;

        protected readonly ConstantsVisitor constants;

        public ContainsVisitor(string needle)
        {
            constants = new ConstantsVisitor();
            this.needle = needle;
        }

        public bool MustContain(Node node)
        {
            if (needle == "")
            {
                return true;
            }
            constants.ComputeConstantsFor(node);

            Void unusedData;
            return VisitNode(node, VisitContext.Root, ref unusedData);
        }

        protected override bool Visit(ConcatNode concatNode, VisitContext context, ref Void data)
        {
            return false;
        }

        protected override bool VisitChildren(ConcatNode concatNode, bool result, ref Void data)
        {
            // Holds a successive constant part
            StringBuilder constantPart = new StringBuilder();

            foreach (Node child in concatNode.children)
            {
                string childConstant = constants.GetConstantFor(child);
                if (childConstant != null)
                {
                    // If the child is constant, add to the constant part
                    constantPart.Append(childConstant);
                }
                else
                {
                    // Check whether the preceding constant part contains needle
                    if (constantPart.ToString().Contains(needle))
                    {
                        return true;
                    }
                    // Start a new constant part
                    constantPart.Clear();
                    // Check inside the non-constant child node
                    if (VisitNode(child, VisitContext.Concat, ref data))
                    {
                        return true;
                    }
                }
            }

            return constantPart.ToString().Contains(needle);
        }

        protected override bool Visit(CharNode charNode, VisitContext context, ref Void data)
        {
            return needle.Length == 1 && needle[0] == charNode.Value;
        }

        protected override bool Visit(MaxNode maxNode, VisitContext context, ref Void data)
        {
            return false;
        }

        protected override bool Visit(OrNode orNode, VisitContext context, ref Void data)
        {
            return true;
        }

        protected override bool VisitChildren(OrNode orNode, bool result, ref Void data)
        {
            foreach (Node child in orNode.children)
            {
                if (!VisitNode(child, VisitContext.Or, ref data))
                {
                    return false;
                }
            }
            return true;
        }

        protected override bool Visit(BottomNode bottomNode, VisitContext context, ref Void data)
        {
            return true;
        }
    }

}
