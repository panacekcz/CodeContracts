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
  /// <summary>
  /// Regex visitor evaluating a match from an anchor element.
  /// </summary>
  internal abstract class AnchoredIsMatchVisitor : SimpleRegexVisitor<ProofOutcome, IndexInt>
  {
    private readonly AnchorKind anchorKind;
    private bool reverse;

    protected AnchoredIsMatchVisitor(AnchorKind anchorKind, bool reverse)
    {
      this.anchorKind = anchorKind;
      this.reverse = reverse;
    }

    /// <summary>
    /// Tries to match a single character at the specified position from the anchor.
    /// </summary>
    /// <param name="element">The single character regex element.</param>
    /// <param name="index">The index relative to the anchor.</param>
    /// <returns>Possible results of the match.</returns>
    protected abstract ProofOutcome IsMatchCharacterAt(SingleElement element, IndexInt index);

    protected override ProofOutcome Unsupported(Element regex, ref IndexInt data)
    {
      data = IndexUtils.After;
      return ProofOutcome.Top;
    }
    protected override ProofOutcome Visit(Alternation element, ref IndexInt data)
    {
      IndexInt next = IndexUtils.Before;
      ProofOutcome result = ProofOutcome.Bottom;
      foreach (var part in element.Patterns)
      {
        IndexInt next_offset = data;
        result = ProofOutcomeUtils.Or(result, VisitElement(part, ref next_offset));
        next = IndexUtils.JoinIndices(next, next_offset);
      }
      data = next;

      return result;
    }
    protected override ProofOutcome Visit(Concatenation concat, ref IndexInt data)
    {
      ProofOutcome result = ProofOutcome.Bottom;

      IEnumerable<Element> partSequence = concat.Parts;

      if (reverse)
      {
        partSequence = partSequence.Reverse();
      }

      foreach (var part in partSequence)
      {
        result = ProofOutcomeUtils.And(result, VisitElement(part, ref data));
      }
      return result;
    }

    protected override ProofOutcome Visit(Empty element, ref IndexInt data)
    {
      if (data.IsNegative)
      {
        data = IndexUtils.After;
        return ProofOutcome.Top;
      }
      else
      {
        return ProofOutcome.True;
      }
    }

    protected override ProofOutcome Visit(Loop element, ref IndexInt data)
    {
      data = IndexUtils.After;
      return ProofOutcome.Top;
    }

    protected override ProofOutcome Visit(SingleElement element, ref IndexInt data)
    {
      if (data.IsInfinite || data.IsNegative)
      {
        data = IndexUtils.After;
        return ProofOutcome.Top;
      }
      /* If offset is negative, we could try to find the first element,
       * however, it would not work in concatenation as it is implemented now,
       * so we would need to keep additional state.
       * The code for prefix might look like:
      else if (offset.IsNegative)
      {
        for (int i = 0; i < prefix.Length; ++i)
        {
          char character = prefix[i];
          if (element.CanMatch(character))
          {
            next = IndexInt.For(i + 1);
            return ProofOutcomeUtils.Build(true, !element.MustMatch(character));
          }
        }

        next = IndexUtils.After;
        return ProofOutcome.Top;
      }
      */
      else
      {
        ProofOutcome result = IsMatchCharacterAt(element, data);
        data = IndexInt.For(data.AsInt + 1);
        return result;
      }
    }


    protected override ProofOutcome Visit(Anchor anchor, ref IndexInt data)
    {
      if (anchor.Kind == anchorKind)
      {
        if (data.IsNegative || data == 0)
        {
          data = IndexInt.For(0);
          return ProofOutcome.True;
        }
        else if (data.IsInfinite)
        {
          return ProofOutcome.Top;
        }
        else
        {
          data = IndexInt.Negative;
          return ProofOutcome.False;
        }
      }
      else
      {
        data = IndexUtils.After;
        return ProofOutcome.Top;
      }
    }
  }

  internal class AnchoredExtractVisitor : SimpleRegexVisitor<Void, IndexInt>
  {
    private List<char> builder = new List<char>();
    private readonly Void unusedReturnValue = new Void();
    private readonly AnchorKind anchorKind;
    private bool reverse;

    public AnchoredExtractVisitor(AnchorKind anchorKind, bool reverse)
    {
      this.anchorKind = anchorKind;
      this.reverse = reverse;
    }

    public string GetString()
    {
      if (builder == null)
      {
        return null;
      }
      else
      {
        if (reverse)
        {
          builder.Reverse();
        }

        return new string(builder.ToArray());
      }
    }


    protected override Void Unsupported(Element regex, ref IndexInt data)
    {
      data = IndexUtils.After;
      return unusedReturnValue;
    }

    protected override Void Visit(Alternation element, ref IndexInt data)
    {
      data = IndexUtils.After;
      return unusedReturnValue;
    }

    protected override Void Visit(Anchor element, ref IndexInt data)
    {
      if (element.Kind == anchorKind)
      {
        if (data.IsNegative)
        {
          data = IndexInt.For(0);
        }
        else if (!data.IsInfinite && data.AsInt > 0)
        {
          builder = null;
          data = IndexUtils.After;
        }
        return unusedReturnValue;
      }
      else
      {
        return Unsupported(element, ref data);
      }
    }

    protected override Void Visit(Concatenation element, ref IndexInt data)
    {
      IEnumerable<Element> parts = element.Parts;
      if (reverse)
      {
        parts = Enumerable.Reverse(parts);
      }

      foreach (var part in parts)
      {
        VisitElement(part, ref data);
      }

      return unusedReturnValue;
    }

    protected override Void Visit(Empty element, ref IndexInt data)
    {
      return unusedReturnValue;
    }

    protected override Void Visit(Loop element, ref IndexInt data)
    {
      if (!(element.Min < 1 || data.IsInfinite || data.IsNegative))
      {
        VisitElement(element.Content, ref data);
      }
      data = IndexUtils.After;
      return unusedReturnValue;
    }

    protected override Void Visit(SingleElement element, ref IndexInt data)
    {
      if (data.IsNegative || data.IsInfinite)
      {
        data = IndexUtils.After;
        return unusedReturnValue;
      }

      char singleChar;
      if (element.TryCanMatchSingleChar(out singleChar))
      {
        builder.Add(singleChar);
        data = data + IndexInt.For(1);
      }
      else if (element.IsEmptyCanMatchSet())
      {
        builder = null;
        data = IndexUtils.After;
      }
      else
      {
        data = IndexUtils.After;
      }
      return unusedReturnValue;
    }
  }

  /// <summary>
  /// Utility methods for working with <see cref="IndeInt"/>.
  /// </summary>
  internal static class IndexUtils
  {
    public static readonly IndexInt Before = IndexInt.Negative;
    public static readonly IndexInt After = IndexInt.Infinity;

    public static IndexInt JoinIndices(IndexInt a, IndexInt b)
    {
      if (a.IsNegative || b.IsInfinite)
        return b;
      if (b.IsNegative || a.IsInfinite)
        return a;

      if (a == b)
        return a;
      else
        return IndexInt.Infinity;
    }
  }

}
