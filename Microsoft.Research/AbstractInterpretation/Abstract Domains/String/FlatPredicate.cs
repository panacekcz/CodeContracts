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
using Microsoft.Research.DataStructures;

namespace Microsoft.Research.AbstractDomains.Strings
{
  /// <summary>
  /// Represents top, bottom, or a constant predicate.
  /// </summary>
  public class FlatPredicate : IStringPredicate
  {
    private readonly bool canBeTrue, canBeFalse;

    /// <summary>
    /// Gets the bottom (unreached) value.
    /// </summary>
    public static FlatPredicate Bottom
    {
      get
      {
        return new FlatPredicate(false, false);
      }
    }
    /// <summary>
    /// Gets the top (unknown) value.
    /// </summary>
    public static FlatPredicate Top
    {
      get
      {
        return new FlatPredicate();
      }
    }

    /// <summary>
    /// Gets the constant true value.
    /// </summary>
    public static FlatPredicate True
    {
      get
      {
        return new FlatPredicate(true);
      }
    }
    /// <summary>
    /// Gets the constant false value.
    /// </summary>
    public static FlatPredicate False
    {
      get
      {
        return new FlatPredicate(false);
      }
    }
    /// <summary>
    /// Converts the <see cref="ProofOutcome"/> enumeration to a predicate.
    /// </summary>
    /// <param name="outcome">The converted proof outcome (one of 4 values).</param>
    /// <returns>The predicate with the same value as <paramref name="outcome"/>.</returns>
    public static FlatPredicate ForProofOutcome(CodeAnalysis.ProofOutcome outcome)
    {
      bool canBeTrue = outcome == CodeAnalysis.ProofOutcome.Top || outcome == CodeAnalysis.ProofOutcome.True;
      bool canBeFalse = outcome == CodeAnalysis.ProofOutcome.Top || outcome == CodeAnalysis.ProofOutcome.False;
      return new FlatPredicate(canBeTrue, canBeFalse);
    }

    /// <summary>
    /// Creates an abstract predicate with the specifed possibility of values.
    /// </summary>
    /// <param name="canBeTrue">Whether the predicate can be true.</param>
    /// <param name="canBeFalse">Whether the predicate can be false.</param>
    public FlatPredicate(bool canBeTrue, bool canBeFalse)
    {
      this.canBeTrue = canBeTrue;
      this.canBeFalse = canBeFalse;
    }
    /// <summary>
    /// Creates a top predicate.
    /// </summary>
    public FlatPredicate()
      : this(true, true)
    {
    }

    /// <summary>
    /// Creates a constant predicate with the specified value.
    /// </summary>
    /// <param name="boolConstant">The constant boolean value.</param>
    public FlatPredicate(bool boolConstant)
      : this(boolConstant, !boolConstant)
    {

    }


    public bool IsBottom
    {
      get { return !canBeTrue && !canBeFalse; }
    }

    public bool IsTop
    {
      get { return canBeTrue && canBeFalse; }
    }


    IAbstractDomain IAbstractDomain.Bottom
    {
      get { return new FlatPredicate(false, false); }
    }

    IAbstractDomain IAbstractDomain.Top
    {
      get { return new FlatPredicate(); }
    }

    public bool LessEqual(IAbstractDomain a)
    {
      FlatPredicate c = a as FlatPredicate;
      if (c != null)
      {
        return (!canBeTrue | c.canBeTrue) & (!canBeFalse | c.canBeFalse);
      }
      else throw new NotImplementedException();
    }

    public IAbstractDomain Join(IAbstractDomain a)
    {
      FlatPredicate c = a as FlatPredicate;
      if (c != null)
      {
        return new FlatPredicate(canBeTrue | c.canBeTrue, canBeFalse | c.canBeFalse);
      }
      else
      {
        if (IsBottom)
        {
          return a;
        }
        else if (a.IsBottom)
        {
          return this;
        }
        else
        {
          return Top;//Overapproximate
        }
      }
    }

    public IAbstractDomain Meet(IAbstractDomain a)
    {
      FlatPredicate c = a as FlatPredicate;
      if (c != null)
      {
        return new FlatPredicate(canBeTrue & c.canBeTrue, canBeFalse & c.canBeFalse);
      }
      else
      {
        if (IsBottom || a.IsBottom)
        {
          return Bottom;
        }
        else if (IsTop)
        {
          return a;
        }
        else
        {
          return this;//Overapproximate (but only between bottom and const)
        }
      }
    }

    public IAbstractDomain Widening(IAbstractDomain prev)
    {
      return Join(prev);
    }

    public T To<T>(IFactory<T> factory)
    {
      if (IsBottom)
      {
        return factory.IdentityForOr;
      }
      else
      {
        return factory.IdentityForAnd;
      }
    }

    public object Clone()
    {
      return new FlatPredicate(canBeTrue, canBeFalse);
    }

    public bool ContainsValue(bool value)
    {
      return value ? canBeTrue : canBeFalse;
    }
    public override string ToString()
    {
      if (canBeTrue)
      {
        return canBeFalse ? "Top" : "True";
      }
      else
      {
        return canBeFalse ? "False" : "Bottom";
      }
    }

    public override bool Equals(object obj)
    {
      return Equals(obj as FlatPredicate);
    }

    public override int GetHashCode()
    {
      return ProofOutcome.GetHashCode();
    }

    public bool Equals(FlatPredicate other)
    {
      if ((object)other == null)
      {
        return false;
      }
      return other.canBeFalse == canBeFalse && other.canBeTrue == canBeTrue;
    }

    public IStringPredicate AssignInParallel<Variable>(Dictionary<Variable, FList<Variable>> sourcesToTargets)
    {
      return this;
    }

    /// <summary>
    /// Converts the predicate to <see cref="ProofOutcome"/> enumeration.
    /// </summary>
    public ProofOutcome ProofOutcome
    {
      get
      {
        return ProofOutcomeUtils.Build(canBeTrue, canBeFalse);
      }
    }
  }
}
