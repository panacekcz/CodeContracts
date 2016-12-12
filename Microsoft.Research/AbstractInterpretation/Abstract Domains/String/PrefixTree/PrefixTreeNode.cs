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

// Created by Vlastimil Dort (2016)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
  public abstract class PrefixTreeNode
  {
 
  }

  public class InnerNode : PrefixTreeNode
  {
    internal Dictionary<char, PrefixTreeNode> children;
    private readonly bool accepting;

    public bool Accepting { get { return accepting; } }

    public InnerNode(bool accepting)
    {
      this.accepting = accepting;
      children = new Dictionary<char, PrefixTreeNode>();
    }
    public InnerNode(InnerNode inn)
    {
      this.accepting = inn.Accepting;
      children = new Dictionary<char, PrefixTreeNode>(inn.children);
    }

    public override bool Equals(object obj)
    {
      if ((object)obj == this)
        return true;

      InnerNode inn = obj as InnerNode;
      if ((object)inn == null)
        return false;


      if (children.Count != inn.children.Count)
        return false;

      foreach(var x in children)
      {
        if (!x.Value.Equals(inn.children[x.Key]))
          return false;
      }

      return true;
    }

    public override int GetHashCode()
    {
      int hc = Accepting ? 111 : 222;
      foreach(var x in children)
      {
        hc += x.Key * x.Value.GetHashCode();
      }

      return hc;
    }
  }

  public class RepeatNode : PrefixTreeNode
  {
    private RepeatNode()
    {
    }

    public static RepeatNode Repeat = new RepeatNode();
  }
}