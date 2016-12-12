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

using Microsoft.Research.DataStructures;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
  static class StringAbstractionPredicate
  {
    public static StringAbstractionPredicate<Abstraction, Variable>
      ForTrue<Abstraction, Variable>(Variable variable, Abstraction trueAbstraction)
      where Variable : class, IEquatable<Variable>
      where Abstraction : IStringAbstraction<Abstraction, string>
    {
      return new StringAbstractionPredicate<Abstraction, Variable>(variable, trueAbstraction, trueAbstraction.Top);
    }

    public static StringAbstractionPredicate<Abstraction, Variable>
      For<Abstraction, Variable>(Variable variable, Abstraction trueAbstraction, Abstraction falseAbstraction)
      where Variable : class, IEquatable<Variable>
      where Abstraction : IStringAbstraction<Abstraction, string>
    {
      return new StringAbstractionPredicate<Abstraction, Variable>(variable, trueAbstraction, falseAbstraction);
    }
  }

  class StringAbstractionPredicate<Abstraction, Variable> : IStringPredicate
    where Variable : class, IEquatable<Variable>
    where Abstraction : IStringAbstraction<Abstraction, string>
  {
    private readonly Abstraction trueAbstraction, falseAbstraction;
    private readonly Variable variable;

    internal StringAbstractionPredicate(Variable variable, Abstraction trueAbstraction, Abstraction falseAbstraction)
    {
      System.Diagnostics.Contracts.Contract.Requires(variable != null);
      System.Diagnostics.Contracts.Contract.Requires(trueAbstraction != null);
      System.Diagnostics.Contracts.Contract.Requires(falseAbstraction != null);

      this.variable = variable;
      this.trueAbstraction = trueAbstraction;
      this.falseAbstraction = falseAbstraction;
    }

    public Abstraction AbstractionIf(bool holds)
    {
      return holds ? trueAbstraction : falseAbstraction;
    }

    public bool ContainsValue(bool value)
    {
      return true; //overapproximation
    }
    public Variable DependentVariable
    {
      get
      {
        return variable;
      }
    }

    public bool IsBottom
    {
      get { return false; }//overapproximation
    }

    public bool IsTop
    {
      get { return trueAbstraction.IsTop && falseAbstraction.IsTop; }
    }

    public bool LessEqual(IAbstractDomain a)
    {
      if (a.IsTop)
      {
        return true;
      }
      else if (a is StringAbstractionPredicate<Abstraction, Variable>)
      {
        var other = (StringAbstractionPredicate<Abstraction, Variable>)a;
        return other.variable.Equals(variable) && trueAbstraction.LessThanEqual(other.trueAbstraction) && falseAbstraction.LessThanEqual(other.falseAbstraction);
      }
      else
      {
        return false;
      }
    }

    public IAbstractDomain Bottom
    {
      get { return new FlatPredicate(false, false); }
    }

    public IAbstractDomain Top
    {
      get { return new FlatPredicate(); }
    }

    public IAbstractDomain Join(IAbstractDomain a)
    {
      if (a is FlatPredicate)
      {
        return a.Join(this);
      }
      else if (a is StringAbstractionPredicate<Abstraction, Variable>)
      {
        var ap = (StringAbstractionPredicate<Abstraction, Variable>)a;
        if (!ap.variable.Equals(this.variable))
        {
          return FlatPredicate.Top;
        }
        return new StringAbstractionPredicate<Abstraction, Variable>(variable, trueAbstraction.Join(ap.trueAbstraction), falseAbstraction.Join(ap.falseAbstraction));
      }
      else
      {
        return FlatPredicate.Top;
      }
    }

    public IAbstractDomain Meet(IAbstractDomain a)
    {
      if (a is FlatPredicate)
      {
        return a.Meet(this);
      }
      else if (a is StringAbstractionPredicate<Abstraction, Variable>)
      {
        var ap = (StringAbstractionPredicate<Abstraction, Variable>)a;
        if (!ap.variable.Equals(this.variable))
        {
          return FlatPredicate.Top;
        }
        return new StringAbstractionPredicate<Abstraction, Variable>(variable, trueAbstraction.Meet(ap.trueAbstraction), falseAbstraction.Meet(ap.falseAbstraction));
      }
      else
      {
        return FlatPredicate.Top;
      }
    }

    public IAbstractDomain Widening(IAbstractDomain prev)
    {
      if (prev is FlatPredicate)
      {
        return prev.Join(this);
      }
      else if (prev is StringAbstractionPredicate<Abstraction, Variable>)
      {
        var ap = (StringAbstractionPredicate<Abstraction, Variable>)prev;
        if (!ap.variable.Equals(this.variable))
        {
          return FlatPredicate.Top;
        }
        Abstraction trueWide = (Abstraction)trueAbstraction.Widening(ap.trueAbstraction);
        Abstraction falseWide = (Abstraction)falseAbstraction.Widening(ap.falseAbstraction);

        return new StringAbstractionPredicate<Abstraction, Variable>(variable, trueWide, falseWide);
      }
      else
      {
        return FlatPredicate.Top;
      }
    }

    public T To<T>(IFactory<T> factory)
    {
      return factory.Constant(true);
    }

    public object Clone()
    {
      return new StringAbstractionPredicate<Abstraction, Variable>(variable, trueAbstraction, falseAbstraction);
    }

    public override string ToString()
    {
      return variable.ToString() + "=" + trueAbstraction.ToString() + "/" + falseAbstraction.ToString();
    }

    public IStringPredicate AssignInParallel<Variable1>(Dictionary<Variable1, FList<Variable1>> sourcesToTargets)
    {
      FList<Variable1> list;
      if (sourcesToTargets.TryGetValue((Variable1)(object)variable, out list) && !list.IsEmpty())
      {
        return new StringAbstractionPredicate<Abstraction, Variable>((Variable)(object)list.Head, trueAbstraction, falseAbstraction);
      }
      else
      {
        return FlatPredicate.Top;
      }
    }

    public CodeAnalysis.ProofOutcome ProofOutcome
    {
      get { return CodeAnalysis.ProofOutcome.Top; }//Overapproximation
    }
  }
}
