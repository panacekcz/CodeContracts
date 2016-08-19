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

using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.AST;

namespace Microsoft.Research.AbstractDomains.Strings
{
  /// <summary>
  /// Converts between <see cref="Prefix"/> and regexes.
  /// </summary>
  public class PrefixRegex
  {
    #region Private state
    private readonly Prefix self;
    #endregion

    public PrefixRegex(Prefix self)
    {
      this.self = self;
    }


    // 
    /// <summary>
    /// Computes a prefix which overapproximates all strings matching a regex.
    /// </summary>
    /// <param name="regex">The AST of the regex.</param>
    /// <returns>The prefix overapproximating <paramref name="regex"/>.</returns>
    public Prefix PrefixForRegex(Element regex)
    {
      AnchoredExtractVisitor visitor = new AnchoredExtractVisitor(AnchorKind.LineStart, false);
      IndexInt index = IndexUtils.Before;
      visitor.VisitSimpleRegex(regex, ref index);
      string prefix = visitor.GetString();
      if (prefix == null)
      {
        return self.Bottom;
      }
      else
      {
        return new Prefix(prefix);
      }
    }

    #region Match regex against prefix

    private class IsMatchVisitor : AnchoredIsMatchVisitor
    {
      private readonly string prefix;

      public IsMatchVisitor(string prefix) :
        base(Regex.AST.AnchorKind.LineStart, false)
      {
        this.prefix = prefix;
      }

      protected override ProofOutcome IsMatchCharacterAt(SingleElement element, IndexInt index)
      {
        if (index.AsInt < prefix.Length)
        {
          char character = prefix[index.AsInt];
          return ProofOutcomeUtils.Build(element.CanMatch(character), !element.MustMatch(character));
        }
        else
        {
          return ProofOutcome.Top;
        }
      }
    }

    /// <summary>
    /// Verifies whether the prefix matches the specified regex expression.
    /// </summary>
    /// <param name="regex">AST of the regex.</param>
    /// <returns>Proven result of the match.</returns>
    public ProofOutcome IsMatch(Regex.AST.Element regex)
    {
      IndexInt data = IndexUtils.Before;
      IsMatchVisitor visitor = new IsMatchVisitor(self.prefix);
      return visitor.VisitSimpleRegex(regex, ref data);
    }
    #endregion
  }
}
