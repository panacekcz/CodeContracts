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

  internal struct Congruence
  {
    private readonly int divider, remainder;

    private Congruence(int divider, int remainder)
    {
      this.divider = divider;
      this.remainder = remainder;
    }

    public bool IsBottom { get
      {
        return divider != 0 && remainder >= divider;
      }
    }
    public bool IsConstant
    {
      get
      {
        return divider == 0;
      }
    }



    public Congruence Add(int constant)
    {
      if (IsBottom)
        return this;
      if (IsConstant)
        return For(remainder + 1); //TODO: VD: overflow
      return For(divider, remainder + constant); //TODO: VD: overflow
    }

    public Congruence Join(Congruence other)
    {
      if (IsBottom)
        return other;
      else if (other.IsBottom)
        return this;

      throw new NotImplementedException(); //TODO:
    }

    public static Congruence For(int divider, int remainder)
    {
      if (divider <= 0 || remainder < 0)
        throw new ArgumentOutOfRangeException();

      return new Congruence(divider, remainder % divider);
    }

    public static Congruence For(int constant)
    {
      if (constant < 0)
        throw new ArgumentOutOfRangeException();
      return new Congruence(0, constant);
    }

    public static Congruence Unreached
    {
      get
      {
        return new Congruence(1, 1);
      }
    }
  }

  internal struct CongruencePair
  {
    private readonly Congruence repeat, suffix;

    public CongruencePair(Congruence repeat, Congruence suffix)
    {
      this.repeat = repeat;
      this.suffix = suffix;
    }

    public CongruencePair Add(int offset)
    {
      return new CongruencePair(repeat.Add(offset), suffix.Add(offset));
    }

    public CongruencePair Join(CongruencePair other)
    {
      return new CongruencePair(repeat.Join(other.repeat), suffix.Join(other.suffix));
    }

    public Congruence Total
    {
      get
      {
        Congruence repeated = repeat.Join(Congruence.For(0));
        return repeated.Add(suffix);
      }
    }
  }

  internal class LengthCongruenceVisitor : CachedPrefixTreeVisitor<CongruencePair>
  {  
    
    public IndexInt GetLengthCommonDivisor(InnerNode tree)
    {
      CongruencePair cp = VisitNodeCached(tree);
      Congruence total = cp.Total;
      return total.CommonDivisor;
    }
    protected override CongruencePair VisitInnerNode(InnerNode inn)
    {
      CongruencePair cp = new CongruencePair(Congruence.Unreached, inn.Accepting ? Congruence.For(0) : Congruence.Unreached);

      foreach(var child in inn.children)
      {
        CongruencePair innerCp = VisitNodeCached(child.Value);
        cp = cp.Join(innerCp.Add(1));
      }

      return cp;
    }
    protected override CongruencePair VisitRepeatNode(RepeatNode inn)
    {
      return new CongruencePair(Congruence.For(0), Congruence.Unreached);
    }
  }

  class LengthIntervalVisitor : CachedPrefixTreeVisitor<IndexInterval>
  {
  
    public IndexInterval GetLengthInterval(InnerNode tree)
    {
      return VisitNodeCached(tree);
    }

    protected override IndexInterval VisitInnerNode(InnerNode inn)
    {
      IndexInterval r = inn.Accepting ? IndexInterval.For(0) : IndexInterval.Unreached;

      foreach(var child in inn.children)
      {
        IndexInterval childInterval = VisitNodeCached(child.Value);
        r = r.Join(childInterval.Add(1));
      }

      return r;
    }
    protected override IndexInterval VisitRepeatNode(RepeatNode inn)
    {
      return IndexInterval.For(IndexInt.For(0), IndexInt.Infinity);
    }
  }
  
}
