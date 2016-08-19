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
  /// Integer type representing index or length of a string.
  /// </summary>
  public struct IndexInt
  {
    #region Special constants
    /// <summary>
    /// Represents positive infinity.
    /// </summary>
    private const int INFINITY = -2;
    /// <summary>
    /// Represents negative indices.
    /// </summary>
    private const int NEGATIVE = -1;
    #endregion

    /// <summary>
    /// The value of the index. Might be non-negative integer or <see cref="INFINITY"/> or <see cref="NEGATIVE"/>.
    /// </summary>
    private readonly int value;

    private IndexInt(int value)
    {
      this.value = value;
    }

    #region Static properties
    /// <summary>
    /// Gets the special value representing a negative indexes, for example 
    /// the -1 result of IndexOf methods.
    /// </summary>
    public static IndexInt Negative
    {
      get { return new IndexInt(NEGATIVE); }
    }
    /// <summary>
    /// Gets the special value representing infinity or unbounded index.
    /// </summary>
    public static IndexInt Infinity
    {
      get { return new IndexInt(INFINITY); }
    }
    #endregion

    #region Factory methods
    /// <summary>
    /// Gets a <see cref="IndexInt"/> for the specified non-negative integer index.
    /// </summary>
    /// <param name="nonNegativeValue">The integer index.</param>
    /// <returns><see cref="IndexInt"/> with value <paramref name="nonNegativeValue"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// When <paramref name="nonNegativeValue"/> is negative.
    /// </exception>
    public static IndexInt ForNonNegative(int nonNegativeValue)
    {
      if (nonNegativeValue < 0)
      {
        throw new ArgumentOutOfRangeException();
      }

      return new IndexInt(nonNegativeValue);
    }

    /// <summary>
    /// Gets a <see cref="IndexInt"/> for the specified integer index.
    /// </summary>
    /// <param name="intValue">The integer index.</param>
    /// <returns><see cref="IndexInt"/> with value <paramref name="intValue"/>, or 
    /// <see cref="Negative"/> if <paramref name="intValue"/> is negative.</returns>
    public static IndexInt For(int intValue)
    {
      if (intValue < 0)
      {
        return new IndexInt(NEGATIVE);
      }

      return new IndexInt(intValue);
    }
    #endregion

    #region Properties
    /// <summary>
    /// Check whether the index is <see cref="Negative"/>.
    /// </summary>
    public bool IsNegative
    {
      get { return value == NEGATIVE; }
    }
    /// <summary>
    /// Check whether the index is <see cref="Infinity"/>.
    /// </summary>
    public bool IsInfinite
    {
      get { return value == INFINITY; }
    }
    #endregion

    /// <summary>
    /// Gets the index as an integer.
    /// </summary>
    public int AsInt
    {
      get
      {
        if (value < 0)
        {
          throw new InvalidOperationException();
        }
        return value;
      }
    }

    /// <summary>
    /// Converts the index to a rational number.
    /// </summary>
    /// <returns>The index as a rational number.</returns>
    internal Numerical.Rational ToRational()
    {
      if (value == INFINITY)
      {
        return Numerical.Rational.PlusInfinity;
      }
      else
      {
        // For negative, returns -1
        return Numerical.Rational.For(value);
      }
    }

    /// <summary>
    /// Coverts the index to a readable string.
    /// </summary>
    /// <returns>String representation of the index.</returns>
    public override string ToString()
    {
      if (IsInfinite)
      {
        return "Inf";
      }
      else
      {
        return value.ToString();
      }
    }

    public override int GetHashCode()
    {
      return value;
    }

    public override bool Equals(object obj)
    {
      IndexInt? otherIndex = obj as IndexInt?;
      if (otherIndex == null)
      {
        return false;
      }

      return value == otherIndex.Value.value;
    }

    #region Operators

    /// <summary>
    /// Compares two indices for equality.
    /// </summary>
    /// <param name="left">The first index.</param>
    /// <param name="right">The second index.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/>
    /// and <paramref name="right"/> are the same.</returns>
    public static bool operator ==(IndexInt left, IndexInt right)
    {
      return left.value == right.value;
    }

    /// <summary>
    /// Compares two indices for inequality.
    /// </summary>
    /// <param name="left">The first index.</param>
    /// <param name="right">The second index.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/>
    /// and <paramref name="right"/> are not the same.</returns>
    public static bool operator !=(IndexInt left, IndexInt right)
    {
      return left.value != right.value;
    }

    /// <summary>
    /// Compares two indices for less than order.
    /// </summary>
    /// <param name="left">The first index.</param>
    /// <param name="right">The second index.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/>
    /// is lower than <paramref name="right"/>.</returns>
    public static bool operator <(IndexInt left, IndexInt right)
    {
      return !left.IsInfinite && (right.IsInfinite || left.value < right.value);
    }
    /// <summary>
    /// Compares two indices for greater than order.
    /// </summary>
    /// <param name="left">The first index.</param>
    /// <param name="right">The second index.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/>
    /// is greater than <paramref name="right"/>.</returns>
    public static bool operator >(IndexInt left, IndexInt right)
    {
      return !right.IsInfinite && (left.IsInfinite || left.value > right.value);
    }

    /// <summary>
    /// Compares two indices for less than or equal.
    /// </summary>
    /// <param name="left">The first index.</param>
    /// <param name="right">The second index.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/>
    /// is less than or equal to <paramref name="right"/>.</returns>
    public static bool operator <=(IndexInt left, IndexInt right)
    {
      return right.IsInfinite || (!left.IsInfinite && left.value <= right.value);
    }
    /// <summary>
    /// Compares two indices for greater than or equal.
    /// </summary>
    /// <param name="left">The first index.</param>
    /// <param name="right">The second index.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/>
    /// is grater than or equal to <paramref name="right"/>.</returns>
    public static bool operator >=(IndexInt left, IndexInt right)
    {
      return left.IsInfinite || (!right.IsInfinite && left.value >= right.value);
    }

    /// <summary>
    /// Compares an index and an integer for equality.
    /// </summary>
    /// <param name="a">The index.</param>
    /// <param name="b">The integer.</param>
    /// <returns><see langword="true"/> if <paramref name="a"/>
    /// is the same as <paramref name="b"/>.</returns>
    public static bool operator ==(IndexInt left, int right)
    {
      return !left.IsInfinite && !left.IsNegative && left.value == right;
    }
    /// <summary>
    /// Compares an index and an integer for equality.
    /// </summary>
    /// <param name="left">The index.</param>
    /// <param name="b">The integer.</param>
    /// <returns><see langword="true"/> if <paramref name="left"/>
    /// is the same as <paramref name="b"/>.</returns>
    public static bool operator !=(IndexInt left, int right)
    {
      return !(left == right);
    }

    public static bool operator <=(IndexInt left, int right)
    {
      if (left.IsInfinite)
      {
        return false;
      }
      else
      {
        return left.value <= right;
      }
    }
    public static bool operator >=(IndexInt left, int right)
    {
      if (left.IsInfinite)
      {
        return true;
      }
      else
      {
        return left.value >= right;
      }
    }

    public static bool operator >(IndexInt left, int right)
    {
      if (left.IsInfinite)
      {
        return true;
      }
      else
      {
        return left.value > right;
      }
    }
    public static bool operator <(IndexInt left, int right)
    {
      if (left.IsInfinite)
      {
        return false;
      }
      else
      {
        return left.value < right;
      }
    }

    #region Arithmetic
    /// <summary>
    /// Subtracts two indices.
    /// </summary>
    /// <param name="a">The index subtracted from.</param>
    /// <param name="b">The subtracted index.</param>
    /// <returns>The difference between <paramref name="a"/> and
    /// <paramref name="b"/>.</returns>
    public static IndexInt operator -(IndexInt a, IndexInt b)
    {
      if (a.IsNegative || b.IsNegative)
      {
        throw new InvalidOperationException("Subtracting negative index");
      }
      else if (b.IsInfinite)
      {
        throw new InvalidOperationException("Subtracting infinite");
      }
      else if (a.IsInfinite)
      {
        return Infinity;
      }
      else
      {
        return IndexInt.For(a.value - b.value);
      }
    }
    /// <summary>
    /// Adds two indices.
    /// </summary>
    /// <param name="a">The first index.</param>
    /// <param name="b">The second index.</param>
    /// <returns>The sum of <paramref name="a"/> and
    /// <paramref name="b"/>.</returns>
    public static IndexInt operator +(IndexInt a, IndexInt b)
    {
      if (a.IsNegative || b.IsNegative)
      {
        throw new InvalidOperationException("Adding negative index");
      }
      if (a.IsInfinite || b.IsInfinite || a.value > int.MaxValue - b.value)
      {
        return Infinity;
      }
      else
      {
        return IndexInt.ForNonNegative(a.value + b.value);
      }
    }
    /// <summary>
    /// Multiplies two indices.
    /// </summary>
    /// <param name="a">The first index.</param>
    /// <param name="b">The second index.</param>
    /// <returns>The product of <paramref name="a"/> and
    /// <paramref name="b"/>.</returns>
    public static IndexInt operator *(IndexInt a, IndexInt b)
    {
      if (a.IsNegative || b.IsNegative)
      {
        throw new InvalidOperationException("Multiplying negative index");
      }
      if (a.IsInfinite || b.IsInfinite)
      {
        return Infinity;
      }
      else
      {
        long result = (long)a.value * (long)b.value;
        if (result > int.MaxValue)
        {
          return Infinity;
        }
        return IndexInt.ForNonNegative((int)result);
      }
    }
    #endregion

    public IndexInt Divide(IndexInt divisor)
    {
      if(divisor.IsNegative || IsNegative)
      {
        throw new InvalidOperationException("Dividing negative index");
      }
      else if (divisor.IsInfinite)
      {
        return For(0);
      }

      int divisorInt = divisor.value;
      if (divisorInt == 0 || IsInfinite)
      {
        return Infinity;
      }
      else
      {
        return ForNonNegative(value / divisorInt);
      }
    }

    #endregion

    #region Aggregations
    /// <summary>
    /// Gets the lower of an index and an integer.
    /// </summary>
    /// <param name="indexInt">The index.</param>
    /// <param name="p">The integer.</param>
    /// <returns>The lower of <paramref name="indexInt"/> and <paramref name="p"/>.</returns>
    public static int Min(IndexInt indexInt, int p)
    {
      if (indexInt.IsInfinite)
      {
        return p;
      }
      else
      {
        return Math.Min(indexInt.AsInt, p);
      }
    }
    /// <summary>
    /// Gets the lower of two indexes.
    /// </summary>
    /// <param name="a">The first index.</param>
    /// <param name="b">The second index.</param>
    /// <returns>The minimum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static IndexInt Min(IndexInt a, IndexInt b)
    {
      return a < b ? a : b;
    }

    /// <summary>
    /// Gets the higher of two indexes.
    /// </summary>
    /// <param name="a">The first index.</param>
    /// <param name="b">The second index.</param>
    /// <returns>The maximum of <paramref name="a"/> and <paramref name="b"/>.</returns>
    public static IndexInt Max(IndexInt a, IndexInt b)
    {
      return a < b ? b : a;
    }

    /// <summary>
    /// Sums a range of indices.
    /// </summary>
    /// <typeparam name="T">The type of the range.</typeparam>
    /// <param name="range">The range of elements.</param>
    /// <param name="getter">The getter that gets an index for each element in the range.</param>
    /// <returns>The sum of indices corresponding to the elements in <paramref name="range"/>.</returns>
    public static IndexInt Sum<T>(IEnumerable<T> range, Func<T, IndexInt> getter)
    {
      IndexInt sum = new IndexInt();

      foreach (T element in range)
      {
        sum += getter(element);
      }

      return sum;
    }

    /// <summary>
    /// Comptes the minimum of a range of indices.
    /// </summary>
    /// <typeparam name="T">The type of the range.</typeparam>
    /// <param name="range">The range of elements.</param>
    /// <param name="getter">The getter that gets an index for each element in the range.</param>
    /// <returns>The minimum of indices corresponding to the elements in <paramref name="range"/>.</returns>
    public static IndexInt Min<T>(IEnumerable<T> range, Func<T, IndexInt> getter)
    {
      IndexInt min = Infinity;

      foreach (T element in range)
      {
        min = Min(min, getter(element));
      }

      return min;
    }

    /// <summary>
    /// Computes the maximum of a range of indices.
    /// </summary>
    /// <typeparam name="T">The type of the range.</typeparam>
    /// <param name="range">The range of elements.</param>
    /// <param name="getter">The getter that gets an index for each element in the range.</param>
    /// <returns>The maximum of indices corresponding to the elements in <paramref name="range"/>.</returns>
    public static IndexInt Max<T>(IEnumerable<T> range, Func<T, IndexInt> getter)
    {
      IndexInt max = new IndexInt();

      foreach (T element in range)
      {
        max = Max(max, getter(element));
      }

      return max;
    }
    #endregion
  }

}
