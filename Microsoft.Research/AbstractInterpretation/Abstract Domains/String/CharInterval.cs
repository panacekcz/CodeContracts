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

namespace Microsoft.Research.AbstractDomains.Strings
{
  /// <summary>
  /// Represents an interval of characters (<see cref="System.Char"/>).
  /// </summary>
  public class CharInterval : Numerical.IntervalBase<CharInterval, char>
  {
    /// <summary>
    /// Creates an interval of characters with the specified bounds.
    /// </summary>
    /// <param name="lowerBound">The lowest represented value.</param>
    /// <param name="upperBound">The highest represented value.</param>
    private CharInterval(char lowerBound, char upperBound)
      : base(lowerBound, upperBound)
    {

    }

    #region Helper methods
    private static char Min(char a, char b)
    {
      return a < b ? a : b;
    }
    private static char Max(char a, char b)
    {
      return a > b ? a : b;
    }
    #endregion

    #region Implementation of IntervalBase

    public override bool IsInt32
    {
      get { return true; }
    }

    public override bool IsInt64
    {
      get { return true; }
    }

    public override bool IsLowerBoundMinusInfinity
    {
      get { return false; }
    }

    public override bool IsUpperBoundPlusInfinity
    {
      get { return false; }
    }

    public override bool IsNormal
    {
      get { return !IsTop && !IsBottom; }
    }

    public override CharInterval ToUnsigned()
    {
      return this;
    }

    public override bool LessEqual(CharInterval a)
    {
      if (IsBottom)
        return true;
      else if (a.IsBottom)
        return false;
      else
        return lowerBound >= a.lowerBound && upperBound <= a.upperBound;
    }

    public override bool IsBottom
    {
      get { return lowerBound > upperBound; }
    }

    public override bool IsTop
    {
      get { return lowerBound == char.MinValue && upperBound == char.MaxValue; }
    }

    public override CharInterval Bottom
    {
      get { return new CharInterval(char.MaxValue, char.MinValue); }
    }

    public override CharInterval Top
    {
      get { return new CharInterval(char.MinValue, char.MaxValue); }
    }

    public override CharInterval Join(CharInterval a)
    {
      return new CharInterval(Min(lowerBound, a.lowerBound), Max(upperBound, a.upperBound));
    }

    public override CharInterval Meet(CharInterval a)
    {
      return new CharInterval(Max(lowerBound, a.lowerBound), Min(upperBound, a.upperBound));
    }

    public override CharInterval Widening(CharInterval a)
    {
      return Join(a);
    }

    public override CharInterval DuplicateMe()
    {
      return new CharInterval(lowerBound, upperBound);
    }
    #endregion

    /// <summary>
    /// Determines whether the interval contains the specified character.
    /// </summary>
    /// <param name="value">A character value.</param>
    /// <returns><see langword="true"/>, if <paramref name="value"/> is in the interval.</returns>
    public bool Contains(char value)
    {
      return value >= lowerBound && value <= upperBound;
    }

    /// <summary>
    /// Determines whether the interval contains exactly one character.
    /// </summary>
    public bool IsConstant
    {
      get
      {
        return lowerBound == upperBound;
      }
    }

    /// <summary>
    /// Gets the interval of all characters (unknown value).
    /// </summary>
    public static CharInterval Unknown
    {
      get
      {
        return new CharInterval(char.MinValue, char.MaxValue);
      }
    }

    /// <summary>
    /// Gets the empty interval for unreached locations.
    /// </summary>
    public static CharInterval Unreached
    {
      get
      {
        return new CharInterval(char.MaxValue, char.MinValue);
      }
    }

    #region Factory methods
    /// <summary>
    /// Constructs a character interval conaining one character.
    /// </summary>
    /// <param name="constant">The single character.</param>
    /// <returns>Character interval containing only <paramref name="constant"/>.</returns>
    public static CharInterval For(char constant)
    {
      return new CharInterval(constant, constant);
    }
    /// <summary>
    /// Constructs a character interval with the specified bounds.
    /// </summary>
    /// <param name="lower">The lower bound character.</param>
    /// <param name="upper">The upper bound character.</param>
    /// <returns>A character interval containing characters between and including <paramref name="lower"/> and <paramref name="upper"/>.</returns>
    public static CharInterval For(char lower, char upper)
    {
      return new CharInterval(lower, upper);
    }

    public static CharInterval For(Numerical.Interval interval)
    {
      int lowerInt = (int)interval.LowerBound.PreviousInt32;
      int upperInt = (int)interval.UpperBound.NextInt32;

      lowerInt = Math.Max(lowerInt, char.MinValue);
      upperInt = Math.Max(upperInt, char.MaxValue);

      return For((char)lowerInt, (char)upperInt);
    }

    #endregion
    /// <summary>
    /// Convert the character interval to a numerical interval.
    /// </summary>
    /// <returns>Numerical interval containing the same values.</returns>
    public Numerical.DisInterval ToDisInterval()
    {
      return Numerical.DisInterval.For((long)lowerBound, (long)upperBound);
    }

    public override bool Equals(object obj)
    {
      CharInterval interval = obj as CharInterval;
      if (interval == null)
      {
        return false;
      }
      else
      {
        return lowerBound == interval.lowerBound && upperBound == interval.upperBound;
      }
    }

    public override int GetHashCode()
    {
      return lowerBound + (upperBound << 16);
    }
  }
}
