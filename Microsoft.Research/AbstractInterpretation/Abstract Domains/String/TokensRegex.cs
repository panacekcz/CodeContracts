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

// Created by Vlastimil Dort (2016)

using Microsoft.Research.AbstractDomains.Strings.PrefixTree;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
/*  class TokensRegexMatch : SimpleRegexVisitor<ProofOutcome, InnerNode>
  {

    private InnerNode root;

    protected override ProofOutcome Unsupported(Element regex, ref InnerNode data)
    {
      data = null;
      return ProofOutcome.Top;
    }

    protected override ProofOutcome Visit(Empty element, ref InnerNode data)
    {
      return ProofOutcome.True;
    }

    protected override ProofOutcome Visit(SingleElement element, ref InnerNode data)
    {
      if (data == null)
      {
        // We do not know the position in the trie
        return ProofOutcome.Top;
      }
      else
      {
        PrefixTreeNode next;
        if (!data.children.TryGetValue(cl, out next))
        {
          return ProofOutcome.False;
        }
        else if (next is RepeatNode)
          data = root;
        else
          data = (InnerNode)next;
      }
    }

    protected override ProofOutcome Visit(Loop element, ref InnerNode data)
    {
      throw new NotImplementedException();
    }

    protected override ProofOutcome Visit(Concatenation element, ref InnerNode data)
    {
      ProofOutcome o = ProofOutcome.True;
      foreach (Element e in element.Parts)
      {
        VisitElement(e, ref data);
      }

      return o;
    }

    protected override ProofOutcome Visit(Anchor element, ref InnerNode data)
    {
      if (element.IsStartAnchor())
      {
        data = root;
        return ProofOutcome.True;
      }
      else
      {
        data = root;
        return ProofOutcome.Top;
      }
    }

    protected override ProofOutcome Visit(Alternation element, ref InnerNode data)
    {
      InnerNode advanced = null;
      foreach (Element e in element.Patterns)
      {
        InnerNode next = data;
        VisitElement(e, ref next);
      }

      data = advanced;

      throw new NotImplementedException();
    }
  }


  class TokensFromRegex : SimpleRegexVisitor<InnerNode, Void>
  {
    protected override InnerNode Unsupported(Element regex, ref Void data)
    {
      return PrefixTreeBuilder.Unknown();
    }

    protected override InnerNode Visit(Loop element, ref Void data)
    {
      throw new NotImplementedException();
    }

    protected override InnerNode Visit(Concatenation element, ref Void data)
    {
      throw new NotImplementedException();
    }

    protected override InnerNode Visit(Anchor element, ref Void data)
    {
      throw new NotImplementedException();
    }

    protected override InnerNode Visit(SingleElement element, ref Void data)
    {
      throw new NotImplementedException();
    }

    protected override InnerNode Visit(Empty element, ref Void data)
    {
      return PrefixTreeBuilder.Empty();
    }

    protected override InnerNode Visit(Alternation element, ref Void data)
    {
      InnerNode bot = PrefixTreeBuilder.Unreached();

      PrefixTreeJoiner joiner = new PrefixTreeJoiner();

      foreach (var e in element.Patterns)
      {
        joiner.Add(VisitElement(e, ref data));
      }

      return joiner.Result();
    }


  }
  */
}
