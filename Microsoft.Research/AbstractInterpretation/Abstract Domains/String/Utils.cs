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

namespace Microsoft.Research.AbstractDomains.Strings
{
  /// <summary>
  /// Povides methods to manipulate <see cref="ProofOutcome"/> values.
  /// </summary>
  internal static class ProofOutcomeUtils
  {
    /// <summary>
    /// Builds a <see cref="ProofOutcome"/> enumeration from the possible truth values.
    /// </summary>
    /// <param name="canBeTrue">Whether the outcome can be true.</param>
    /// <param name="canBeFalse">Whether the outcome can be false.</param>
    /// <returns>The proof outcome corresponding to the possible values
    /// <paramref name="canBeTrue"/> and <paramref name="canBeFalse"/>. </returns>
    public static ProofOutcome Build(bool canBeTrue, bool canBeFalse)
    {
      return canBeFalse ? (canBeTrue ? ProofOutcome.Top : ProofOutcome.False) : (canBeTrue ? ProofOutcome.True : ProofOutcome.Bottom);
    }

    /// <summary>
    /// Computes a disjunction of proof outcomes.
    /// </summary>
    /// <param name="a">The first outcome.</param>
    /// <param name="b">The second outcome.</param>
    /// <returns>Disjunction of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static ProofOutcome Or(ProofOutcome a, ProofOutcome b)
    {
      if (a == ProofOutcome.Bottom || b == ProofOutcome.True)
      {
        return b;
      }
      else if (b == ProofOutcome.Bottom || a == ProofOutcome.True)
      {
        return a;
      }
      else if (a == ProofOutcome.Top || b == ProofOutcome.Top)
      {
        return ProofOutcome.Top;
      }
      else
      {
        return ProofOutcome.False;
      }
    }

    /// <summary>
    /// Computes a conjunction of proof outcomes.
    /// </summary>
    /// <param name="a">The first outcome.</param>
    /// <param name="b">The second outcome.</param>
    /// <returns>Conjunction of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static ProofOutcome And(ProofOutcome a, ProofOutcome b)
    {
      if (a == ProofOutcome.Bottom || b == ProofOutcome.False)
      {
        return b;
      }
      else if (b == ProofOutcome.Bottom || a == ProofOutcome.False)
      {
        return a;
      }
      else if (a == ProofOutcome.Top || b == ProofOutcome.Top)
      {
        return ProofOutcome.Top;
      }
      else
      {
        return ProofOutcome.True;
      }
    }
    public static bool CanBeTrue(ProofOutcome outcome)
    {
      return outcome == ProofOutcome.Top || outcome == ProofOutcome.True;
    }
    public static bool CanBeFalse(ProofOutcome outcome)
    {
      return outcome == ProofOutcome.Top || outcome == ProofOutcome.False;
    }
  }
}