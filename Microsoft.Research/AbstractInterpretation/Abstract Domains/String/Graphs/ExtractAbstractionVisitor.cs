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
  /// Extracts another string abstraction from a string graph.
  /// </summary>
  /// <typeparam name="Abstraction">Type of the target abstraction.</typeparam>
  internal abstract class ExtractAbstractionVisitor<Abstraction> : Visitor<Abstraction, Void>
    where Abstraction : IStringAbstraction<Abstraction, string>
  {
    protected readonly ConstantsVisitor constants;
    protected readonly Abstraction top;

    protected ExtractAbstractionVisitor(Abstraction top)
    {
      constants = new ConstantsVisitor();
      this.top = top;
    }

    public Abstraction Extract(Node node)
    {
      constants.ComputeConstantsFor(node);

      Void unusedData;
      return VisitNode(node, VisitContext.Root, ref unusedData);
    }

    protected override Abstraction Visit(ConcatNode concatNode, VisitContext context, ref Void data)
    {
      return top;
    }

    protected override Abstraction Visit(CharNode charNode, VisitContext context, ref Void data)
    {
      return top.Constant(charNode.Value.ToString());
    }

    protected override Abstraction Visit(MaxNode maxNode, VisitContext context, ref Void data)
    {
      return top;
    }

    protected override Abstraction Visit(OrNode orNode, VisitContext context, ref Void data)
    {
      return top;
    }

    protected override Abstraction VisitChildren(OrNode orNode, Abstraction result, ref Void data)
    {
      result = top.Bottom;

      foreach (Node child in orNode.children)
      {
        Abstraction next = VisitNode(child, VisitContext.Or, ref data);
        result = result.Join(next);
      }
      return result;
    }

    protected override Abstraction Visit(BottomNode bottomNode, VisitContext context, ref Void data)
    {
      return top.Bottom;
    }
  }

}
