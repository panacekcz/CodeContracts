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



  class TokensFromRegex : SimpleRegexVisitor<InnerNode, Void>
  {
        private bool underapprox;
        public TokensFromRegex(bool underapprox)
        {
            this.underapprox = underapprox;
        }

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


        public InnerNode Build(Element element)
        {
            Void v = null;
            return VisitSimpleRegex(element, ref v);
        }


  }
  
    internal class TokensRegex
    {
        public static Tokens FromRegex(Element regex, bool underapprox)
        {
            TokensFromRegex tfr = new TokensFromRegex(underapprox);

            return new Tokens(tfr.Build(regex));
        }
    }
}
