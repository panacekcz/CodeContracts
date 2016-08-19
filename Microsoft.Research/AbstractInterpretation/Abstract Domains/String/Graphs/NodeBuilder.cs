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
  /// Builds common patterns of string graph nodes.
  /// </summary>
  internal static class NodeBuilder
  {
    /// <summary>
    /// Creates a string graph node for a constant string.
    /// </summary>
    /// <param name="constant">A string constant.</param>
    /// <returns>A node representing <paramref name="constant"/>.</returns>
    public static Node CreateConcatNodeForString(string constant)
    {
      ConcatNode concat = new ConcatNode(constant.Select(ch => new CharNode(ch)));
      return Normalizer.Compact(concat);
    }

    /// <summary>
    /// Adds an interval of character to an existing <see cref="OrNode"/>.
    /// </summary>
    /// <param name="destination">The modified <see cref="OrNode"/>.</param>
    /// <param name="values">Interval of characters that are added to <paramref name="destination"/>.</param>
    private static void AddCharInterval(OrNode destination, CharInterval values)
    {
      for (int character = values.LowerBound; character <= values.UpperBound; ++character)
      {
        destination.children.Add(new CharNode((char)character));
      }
    }

    /// <summary>
    /// Creates a string graph node for a interval of characters.
    /// </summary>
    /// <param name="values">A character interval.</param>
    /// <returns>A node representing <paramref name="values"/>.</returns>
    public static Node CreateNodeForInterval(CharInterval values)
    {
      if (values.IsBottom)
      {
        return new BottomNode();
      }

      if (values.IsConstant)
      {
        return new CharNode(values.LowerBound);
      }

      OrNode or = new OrNode();
      AddCharInterval(or, values);
      return or;
    }

    /// <summary>
    /// Creates a string graph node for multiple intervals of characters.
    /// </summary>
    /// <param name="values">A sequence of character intervals.</param>
    /// <returns>A node representing <paramref name="values"/>.</returns>
    public static Node CreateNodeForIntervals(IEnumerable<CharInterval> intervals)
    {
      OrNode or = new OrNode();
      foreach (CharInterval interval in intervals)
      {
        AddCharInterval(or, interval);
      }
      if (or.children.Count == 0)
      {
        return new BottomNode();
      }
      else if (or.children.Count == 1)
      {
        return or.children[0];
      }
      else
      {
        return or;
      }
    }

    /// <summary>
    /// Creates a node for an empty string.
    /// </summary>
    /// <returns>A node representing an empty string.</returns>
    public static Node CreateEmptyNode()
    {
      return new ConcatNode();
    }

    /// <summary>
    /// Creates a node for an iteration.
    /// </summary>
    /// <param name="loopedNode">The iterated node.</param>
    /// <returns>A node representing the iteration of <paramref name="loopedNode"/>.</returns>
    public static Node CreateLoop(Node loopedNode)
    {
      Node emptyNode = CreateEmptyNode();

      OrNode loopNode = new OrNode();

      ConcatNode nonEmptyNode = new ConcatNode();
      nonEmptyNode.children.Add(loopedNode);
      nonEmptyNode.children.Add(loopNode);

      loopNode.indegree = 2;
      loopNode.children.Add(emptyNode);
      loopNode.children.Add(nonEmptyNode);

      return loopNode;
    }

    /// <summary>
    /// Creates a node for padding.
    /// </summary>
    /// <param name="padding">The interval of possible padding characters.</param>
    /// <param name="length">The target length.</param>
    /// <returns>A node representing the iteration of <paramref name="loopedNode"/>.</returns>
    public static Node CreatePaddingNode(CharInterval padding, IndexInterval length)
    {
      if (padding.IsBottom)
      {
        return new BottomNode();
      }
      else if (padding.UpperBound - padding.LowerBound > 100)
      {
        // Too many characters possible
        return new MaxNode();
      }

      Node paddingCharNode = CreateNodeForInterval(padding);

      return CreateLoop(paddingCharNode);
    }

    /// <summary>
    /// Creates a node for an optional occurence.
    /// </summary>
    /// <param name="innerNode">The node representing the optional part.</param>
    /// <returns>A node representing <paramref name="innerNode"/> and an empty string.</returns>
    public static Node CreateOptionalNode(Node innerNode)
    {
      return new OrNode(new Node[] { innerNode, CreateEmptyNode() });
    }
  }
}
