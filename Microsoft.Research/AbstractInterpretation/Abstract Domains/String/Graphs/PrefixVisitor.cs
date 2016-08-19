﻿// CodeContracts
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
  /// Extracts constant prefix from a string graph.
  /// </summary>
  class PrefixVisitor : ExtractAbstractionVisitor<Prefix>
  {
    public PrefixVisitor() :
      base(new Prefix(""))
    {
    }


    protected override Prefix VisitChildren(ConcatNode concatNode, Prefix result, ref Void data)
    {
      StringBuilder builder = new StringBuilder();
      foreach (Node child in concatNode.children)
      {
        string childConstant = constants.GetConstantFor(child);
        if (childConstant != null)
        {
          builder.Append(childConstant);
        }
        else
        {
          Prefix childPrefix = VisitNode(child, VisitContext.Concat, ref data);
          if (childPrefix.IsBottom)
          {
            return childPrefix;
          }
          builder.Append(childPrefix.prefix);
          break;
        }
      }

      return new Prefix(builder.ToString());
    }


  }
}
