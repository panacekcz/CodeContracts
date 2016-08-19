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
  /// Traverses the string graph, copying the nodes.
  /// </summary>
  /// <typeparam name="Data">The data passed along the traversal.</typeparam>
  class CopyVisitor<Data> : Visitor<Node, Data>
  {
    protected override Node Visit(ConcatNode concatNode, VisitContext context, ref Data data)
    {
      return new ConcatNode();
    }

    protected override Node Visit(CharNode charNode, VisitContext context, ref Data data)
    {
      return charNode;
    }

    protected override Node Visit(MaxNode maxNode, VisitContext context, ref Data data)
    {
      return maxNode;
    }

    protected override Node Visit(OrNode orNode, VisitContext context, ref Data data)
    {
      return new OrNode();
    }

    protected override Node VisitBackwardEdge(Node graphNode, Node result, VisitContext context, ref Data data)
    {
      if (result is InnerNode)
      {
        ((InnerNode)result).indegree++;
      }
      return result;
    }

    protected override Node VisitSharedEdge(Node graphNode, Node result, VisitContext context, ref Data data)
    {
      return VisitForwardEdge(graphNode, context, ref data);
    }

    protected override Node VisitNodeChildren(InnerNode innerNode, Node result, VisitContext context, ref Data data)
    {
      InnerNode newInnerNode = (InnerNode)result;
      foreach (Node child in innerNode.children)
      {
        newInnerNode.children.Add(VisitNode(child, context, ref data));
      }
      return result;
    }

    protected override Node Visit(BottomNode bottomNode, VisitContext context, ref Data data)
    {
      return bottomNode;
    }
  }
}
