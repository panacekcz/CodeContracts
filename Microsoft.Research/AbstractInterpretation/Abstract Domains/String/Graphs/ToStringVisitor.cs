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
    struct Void { }

    /// <summary>
    /// Generates a readable string representation of the string graph.
    /// </summary>
    class ToStringVisitor : Visitor<string, Void>
    {
        private readonly StringBuilder builder;
        private readonly NodeLabels nameGenerator;

        public ToStringVisitor(StringBuilder builder)
        {
            this.builder = builder;
            nameGenerator = new NodeLabels();
        }

        public void Generate(Node node)
        {
            Void unusedData;
            VisitNode(node, VisitContext.Root, ref unusedData);
        }

        private string VisitInnerNode(InnerNode innerNode)
        {
            if (innerNode.indegree > 1)
            {
                string name = nameGenerator.GetNextName();
                builder.Append(name);
                builder.Append(':');
                return name;
            }
            else
            {
                return null;
            }
        }

        protected override string Visit(ConcatNode concatNode, VisitContext context, ref Void data)
        {
            return VisitInnerNode(concatNode);
        }

        protected override string Visit(CharNode charNode, VisitContext context, ref Void data)
        {
            builder.AppendFormat("[{0}]", charNode.Value);
            return null;
        }

        protected override string Visit(MaxNode maxNode, VisitContext context, ref Void data)
        {
            builder.Append("T");
            return null;
        }

        protected override string Visit(OrNode orNode, VisitContext context, ref Void data)
        {
            return VisitInnerNode(orNode);
        }

        protected override string Visit(BottomNode bottomNode, VisitContext context, ref Void data)
        {
            builder.Append("_|_");
            return null;
        }

        protected override string VisitChildren(ConcatNode orNode, string result, ref Void data)
        {
            builder.Append('<');
            base.VisitChildren(orNode, result, ref data);
            builder.Append('>');
            return result;
        }

        protected override string VisitChildren(OrNode orNode, string result, ref Void data)
        {
            builder.Append('{');
            base.VisitChildren(orNode, result, ref data);
            builder.Append('}');
            return result;
        }

        protected override string VisitBackwardEdge(Node graphNode, string result, VisitContext context, ref Void data)
        {
            if (result == null)
                VisitForwardEdge(graphNode, context, ref data);
            else
                builder.Append(result);
            return result;
        }
    }
}
