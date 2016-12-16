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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
  public abstract class PrefixTreeVisitor<Result>
  {
    protected Result VisitNode(PrefixTreeNode tn)
    {
      if (tn is InnerNode)
        return VisitInnerNode((InnerNode)tn);
      else if (tn is RepeatNode)
      {
        return VisitRepeatNode((RepeatNode)tn);
      }

      throw new NotImplementedException();
    }

    protected abstract Result VisitInnerNode(InnerNode inn);
    protected abstract Result VisitRepeatNode(RepeatNode inn);
  }

  public abstract class CachedPrefixTreeVisitor<Result> : PrefixTreeVisitor<Result>
  {
    private Dictionary<PrefixTreeNode, Result> cache = new Dictionary<PrefixTreeNode, Result>();

    protected Result VisitNodeCached(PrefixTreeNode tn)
    {
      Result r;
      if(!cache.TryGetValue(tn, out r))
      {
        r = VisitNode(tn);
        cache[tn] = r;
      }
      
      return r;
    }
    
  }

  internal class TrieShare
  {
    private Dictionary<PrefixTreeNode, PrefixTreeNode> nodes = new Dictionary<PrefixTreeNode, PrefixTreeNode>(new PrefixTreeNodeComparer());

    public PrefixTreeNode Share(PrefixTreeNode tn)
    {
      PrefixTreeNode tno;
      if (nodes.TryGetValue(tn, out tno))
        return nodes[tn];
      else
      {
        nodes[tn] = tn;
        return tn;
      }
    }

  }

  public abstract class PrefixTreeTransformer : CachedPrefixTreeVisitor<PrefixTreeNode>
  {
    private readonly TrieShare share = new TrieShare();
    private readonly List<InnerNode> merges = new List<InnerNode>();

    protected PrefixTreeNode Share(PrefixTreeNode tn)
    {
      return share.Share(tn);
    }

    protected RepeatNode Cutoff(PrefixTreeNode tn)
    {
      if (tn is RepeatNode)
        return (RepeatNode)tn;

      merges.Add((InnerNode)tn);
      return RepeatNode.Repeat;
    }

    private InnerNode MergeInnerNodes(InnerNode oldNode, InnerNode newNode)
    {
      InnerNode merged = new InnerNode(oldNode.Accepting || newNode.Accepting);


      foreach (var kv in oldNode.children)
      {
        PrefixTreeNode otherChild;
        if (newNode.children.TryGetValue(kv.Key, out otherChild))
          merged.children.Add(kv.Key, Merge(kv.Value, otherChild));
        else
          merged.children.Add(kv.Key, kv.Value);
      }

      foreach(var kv in newNode.children)
      {
        if (!oldNode.children.ContainsKey(kv.Key))
        {
          merged.children.Add(kv.Key, kv.Value);
        }
      }

      return merged;
    }

    internal PrefixTreeNode Merge(PrefixTreeNode oldNode, PrefixTreeNode newNode)
    {
      // Trie merging
      // - if any of the two nodes is accepting, cut them off and make root accepting
      //   - if we are in the root (oldNode is root), merge them and make root accepting
      // - if any of the nodes is repeating, cut the other off
      // - if both nodes are not accepting and not repeating, merge them (defined for inner nodes, but may work for dead-end nodes too).

      if (oldNode is RepeatNode)
      {
        if (newNode is RepeatNode)
          // Both are repeat nodes, return repeat node
          return oldNode;
        else
          // One of them is inner, cut it off
          return Cutoff(newNode);
      }
      else if (newNode is RepeatNode)
      {
        // One of them is inner, cut it off
        return Cutoff(oldNode);
      }
      else
      {
        // Both are inner nodes
        return MergeInnerNodes((InnerNode)oldNode, (InnerNode)newNode);
      }
    }

    protected InnerNode MergeOffcuts(InnerNode root)
    {
      while (merges.Count > 0)
      {
        InnerNode merged = merges[merges.Count - 1];
        merges.RemoveAt(merges.Count - 1);
        root = MergeInnerNodes(root, merged);
      }

      return root;
    }

    public InnerNode Transform(PrefixTreeNode root)
    {
     
      root = VisitNodeCached(root);

      InnerNode newRoot = (root is RepeatNode) ? PrefixTreeBuilder.Empty() : (InnerNode)root;

      return MergeOffcuts(newRoot);
    }

    protected override PrefixTreeNode VisitInnerNode(InnerNode inn)
    {
      InnerNode newNode = null;
      foreach (var kv in inn.children)
      {
        PrefixTreeNode tn = VisitNodeCached(kv.Value);
        if (tn != kv.Value) // Reference comparison
        {
          if (newNode == null)
          {
            newNode = new InnerNode(inn);
          }
          newNode.children[kv.Key] = tn;
        }
      }

      return Share(newNode ?? inn);
    }
    protected override PrefixTreeNode VisitRepeatNode(RepeatNode inn)
    {
      return inn;
    }
  }

}
