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
  enum VisitContext
  {
    Root, Concat, Or
  }

  /// <summary>
  /// Implements a generic visitor pattern for string graphs.
  /// </summary>
  /// <typeparam name="Result">The type returned from visit methods.</typeparam>
  /// <typeparam name="Data">Data passed to the visit methods along the traversal.</typeparam>
  abstract class Visitor<Result, Data>
  {
    protected Dictionary<Node, Result> results = new Dictionary<Node, Result>();

    protected Result VisitNode(Node graphNode, VisitContext context, ref Data data)
    {
      Result result;
      if (!results.TryGetValue(graphNode, out result))
      {
        result = VisitForwardEdge(graphNode, context, ref data);
      }
      else if (graphNode is InnerNode)
      {
        result = VisitBackwardEdge(graphNode, result, context, ref data);
      }
      else if (graphNode is InnerNode)
      {
        result = VisitSharedEdge(graphNode, result, context, ref data);
      }
      return result;
    }

    protected virtual Result VisitForwardEdge(Node graphNode, VisitContext context, ref Data data)
    {
      Result result;
      if (graphNode is CharNode)
      {
        result = Visit((CharNode)graphNode, context, ref data);
      }
      else if (graphNode is ConcatNode)
      {
        result = Visit((ConcatNode)graphNode, context, ref data);
        // Save the immediate result for backreferences
        results[graphNode] = result;
        result = VisitChildren((ConcatNode)graphNode, result, ref data);
      }
      else if (graphNode is OrNode)
      {
        result = Visit((OrNode)graphNode, context, ref data);
        // Save the immediate result for backreferences
        results[graphNode] = result;
        result = VisitChildren((OrNode)graphNode, result, ref data);
      }
      else if (graphNode is MaxNode)
      {
        result = Visit((MaxNode)graphNode, context, ref data);
      }
      else if (graphNode is BottomNode)
      {
        result = Visit((BottomNode)graphNode, context, ref data);
      }
      else
      {
        throw new AbstractInterpretationException("Unsupported string graph node type");
      }

      results[graphNode] = result;
      return result;
    }

    protected virtual Result VisitChildren(OrNode orNode, Result result, ref Data data)
    {
      return VisitNodeChildren(orNode, result, VisitContext.Or, ref data);
    }

    protected virtual Result VisitChildren(ConcatNode concatNode, Result result, ref Data data)
    {
      return VisitNodeChildren(concatNode, result, VisitContext.Concat, ref data);
    }

    protected virtual Result VisitNodeChildren(InnerNode innerNode, Result result, VisitContext context, ref Data data)
    {
      foreach (Node childNode in innerNode.children)
      {
        VisitNode(childNode, context, ref data);
      }
      return result;
    }

    protected virtual Result VisitBackwardEdge(Node graphNode, Result result, VisitContext context, ref Data data)
    {
      return result;
    }
    protected virtual Result VisitSharedEdge(Node graphNode, Result result, VisitContext context, ref Data data)
    {
      return result;
    }

    protected abstract Result Visit(ConcatNode concatNode, VisitContext context, ref Data data);
    protected abstract Result Visit(CharNode charNode, VisitContext context, ref Data data);
    protected abstract Result Visit(MaxNode maxNode, VisitContext context, ref Data data);
    protected abstract Result Visit(OrNode orNode, VisitContext context, ref Data data);
    protected abstract Result Visit(BottomNode bottomNode, VisitContext context, ref Data data);

  }
}
