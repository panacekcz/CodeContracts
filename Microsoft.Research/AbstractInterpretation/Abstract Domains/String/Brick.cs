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
using System.Diagnostics;
using System.Diagnostics.Contracts;

using Microsoft.Research.CodeAnalysis;

namespace Microsoft.Research.AbstractDomains.Strings
{
  /// <summary>
  /// Elements of brick domain.
  /// Represents strings formed by a limited number of constants.
  /// </summary>
  public class Brick
  {
    /// <summary>
    /// Compares sets of strings for equality.
    /// </summary>
    internal static IEqualityComparer<HashSet<string>> stringSetComparer = HashSet<string>.CreateSetComparer();

    #region Element state
    /// <summary>
    /// The set of possible constant values.
    /// </summary>
    /// <remarks>
    /// Can be <see langword="null"/>, in which case means any value.
    /// </remarks>
    internal readonly HashSet<string> values;
    /// <summary>
    /// The minimum number of occurences.
    /// </summary>
    internal readonly IndexInt min;
    /// <summary>
    /// The maximum number of occurences.
    /// </summary>
    internal readonly IndexInt max;
    #endregion

    #region Construction
    /// <summary>
    /// Constructs a brick abstracting (precisely) a single constant.
    /// </summary>
    /// <param name="constant">The constant string value.</param>
    public Brick(string constant)
    {
      Contract.Requires(constant != null);

      if (constant == "")
      {
        this.values = new HashSet<string>();
        this.min = IndexInt.ForNonNegative(0);
        this.max = IndexInt.ForNonNegative(0);
      }
      else
      {
        this.values = new HashSet<string> { constant };
        this.min = IndexInt.ForNonNegative(1);
        this.max = IndexInt.ForNonNegative(1);
      }
    }

    /// <summary>
    /// Constructs a brick with a single value and the specified integer numbers of occurences.
    /// </summary>
    /// <param name="value">The single string value.</param>
    /// <param name="min">The minimum number of occurences.</param>
    /// <param name="max">The maximum number of occurences.</param>
    public Brick(string value, int min, int max)
      : this(value,
      IndexInt.ForNonNegative(min),
      IndexInt.ForNonNegative(max))
    {
    }

    /// <summary>
    /// Constructs a brick with a single value and the specified numbers of occurences.
    /// </summary>
    /// <param name="value">The single string value.</param>
    /// <param name="min">The minimum number of occurences.</param>
    /// <param name="max">The maximum number of occurences.</param>
    public Brick(string value, IndexInt min, IndexInt max)
    {
      Contract.Requires(value != null);

      this.values = new HashSet<string> { value };
      this.min = min;
      this.max = max;
    }

    /// <summary>
    /// Constructs a top or bottom brick.
    /// </summary>
    /// <param name="top">Whether to construct top</param>
    public Brick(bool top)
    {
      this.values = top ? null : new HashSet<string>();
      this.min = top ? IndexInt.ForNonNegative(0) : IndexInt.Infinity;
      this.max = top ? IndexInt.Infinity : IndexInt.ForNonNegative(0);
    }

    internal Brick(HashSet<string> values, IndexInt min, IndexInt max)
    {
      this.values = values;
      this.min = min;
      this.max = max;
    }

    internal Brick(HashSet<string> values)
    {
      Contract.Requires(values != null);

      this.values = values;
      this.min = IndexInt.ForNonNegative(1);
      this.max = IndexInt.ForNonNegative(1);
    }
    #endregion
    #region Domain operations
    public Brick Join(Brick other)
    {
      Contract.Requires(other != null);

      HashSet<string> unionSet;
      if (values == null || other.values == null)
      {
        unionSet = null;
      }
      else
      {
        unionSet = new HashSet<string>(values);
        unionSet.UnionWith(other.values);
      }
      return new Brick(unionSet, IndexInt.Min(min, other.min), IndexInt.Max(max, other.max));
    }
    #endregion
    #region Domain properties
    public bool IsTop
    {
      get
      {
        return values == null;
      }
    }
    public bool IsBottom
    {
      get
      {
        return min > max || (min > 0 && values != null && values.Count == 0);
      }
    }
    public Brick Bottom
    {
      get
      {
        return new Brick(false);
      }
    }
    public Brick Top
    {
      get
      {
        return new Brick(true);
      }
    }
    public bool Equals(Brick other)
    {
      return min == other.min && max == other.max && stringSetComparer.Equals(values, other.values);
    }
    #endregion

    #region Object override methods
    public override string ToString()
    {
      string valueSet;
      if (values == null)
      {
        valueSet = "*";
      }
      else
      {
        valueSet = "{" + string.Join(",", values) + "}";
      }
      return string.Format("{0}[{1},{2}]", valueSet, min, max);
    }
    public override bool Equals(object obj)
    {
      Brick otherBrick = obj as Brick;
      if (otherBrick == null)
      {
        return false;
      }
      return this == otherBrick;
    }
    public override int GetHashCode()
    {
      return values == null ? 0 : stringSetComparer.GetHashCode(values);
    }
    #endregion
    #region Operators
    public static bool operator !=(Brick left, Brick right)
    {
      return !(left == right);
    }
    public static bool operator ==(Brick left, Brick right)
    {
      if ((object)left != null && (object)right != null)
      {
        return left.min == right.min && left.max == right.max && stringSetComparer.Equals(left.values, right.values);
      }
      else
      {
        return (object)left == null && (object)right == null;
      }
    }
    #endregion
    #region Helpers
    #region Normalization helpers
    internal Brick MergeCardinalities(Brick brick)
    {
      Debug.Assert(values != null && brick.values != null);
      Debug.Assert(stringSetComparer.Equals(values, brick.values));

      return new Brick(values, min + brick.min, max + brick.max);
    }
    internal Brick MergeSets(Brick brick)
    {
      Debug.Assert(values != null && brick.values != null);
      Debug.Assert(min == 1 && max == 1 && brick.min == 1 && brick.max == 1);

      var set = new HashSet<string>(values.SelectMany(_ => brick.values, string.Concat));
      return new Brick(set);
    }
    internal Brick ExpandRepeats()
    {
      Debug.Assert(min == max && !max.IsInfinite);

      var set = new HashSet<string> { "" };
      for (int i = 0; i < min.AsInt; ++i)
      {
        set = new HashSet<string>(set.SelectMany(_ => values, string.Concat));
      }
      return new Brick(set);
    }

    internal void Break(out Brick mandatory, out Brick optional, bool expandRepeats)
    {
      Debug.Assert(min >= 1 && max > min);

      Brick mandatoryRepeats = new Brick(values, min, min);
      if (expandRepeats)
      {
        mandatory = mandatoryRepeats.ExpandRepeats();
      }
      else
      {
        mandatory = mandatoryRepeats;
      }
      optional = new Brick(values, IndexInt.ForNonNegative(0), max - min);
    }
    #endregion
    #region Helper properties
    /// <summary>
    /// Determines whether the brick represents only (at most) an empty string.
    /// </summary>
    internal bool MustBeEmpty
    {
      get
      {
        return max == 0 || (values != null && values.Count == 1 && values.Contains(""));
      }
    }
    /// <summary>
    /// Determines whether the brick represents (at least) an empty string.
    /// </summary>
    internal bool CanBeEmpty
    {
      get
      {
        return !IsBottom && (values == null || values.Contains("") || min == 0);
      }
    }
    internal IndexInt MinLength
    {
      get
      {
        if (values == null)
          return IndexInt.ForNonNegative(0);
        else
          return min * IndexInt.Min(values, value => IndexInt.For(value.Length));
      }
    }
    internal IndexInt MaxLength
    {
      get
      {
        if (values == null)
        {
          return IndexInt.Infinity;
        }
        else
        {
          return max * IndexInt.Max(values, value => IndexInt.For(value.Length));
        }
      }
    }

    #endregion

    #region Conversion helpers
    /// <summary>
    /// If the brick represents a single string constant, gets that constant.
    /// </summary>
    /// <returns>A string constant or <see langword="null"/>.</returns>
    internal string ToConstant()
    {
      if (values == null)
      {
        // Top brick
        return null;
      }
      else if (max == 0)
      {
        return "";
      }
      else if (values.Count == 1)
      {
        string value = values.First();
        if ((min == 1 && max == 1) || value == "")
        {
          // A single value
          return value;
        }
        else if (min == max)
        {
          // Constant number of repetitions
          StringBuilder sb = new StringBuilder();
          for (int i = 0; i < min.AsInt; ++i)
            sb.Append(value);
          return sb.ToString();
        }
        else
        {
          // Variable number of repetitions
          return null;
        }
      }
      else
      {
        // Multiple values
        return null;
      }
    }
    /// <summary>
    /// Extracts a prefix common to all strings represented by this brick.
    /// </summary>
    /// <returns>A common prefix string. Not <see langword="null"/>.</returns>
    internal string ToPrefix()
    {
      if (values == null || min == 0)
      {
        return "";
      }
      else
      {
        string prefix = null;
        foreach (string value in values)
        {
          if (prefix == null)
          {
            prefix = value;
          }
          else
          {
            prefix = StringUtils.LongestCommonPrefix(prefix, value);
          }
        }
        return prefix;
      }
    }
    /// <summary>
    /// Extracts a suffix common to all strings represented by this brick.
    /// </summary>
    /// <returns>A common suffix string. Not <see langword="null"/>.</returns>
    internal string ToSuffix()
    {
      if (values == null || min == 0)
      {
        return "";
      }
      else
      {
        string suffix = null;
        foreach (string value in values)
        {
          if (suffix == null)
          {
            suffix = value;
          }
          else
          {
            suffix = StringUtils.LongestCommonSuffix(suffix, value);
          }
        }
        return suffix;
      }
    }

    #endregion

    #region Operation helpers
    internal Brick Replace(CharInterval from, CharInterval to)
    {
      if (values == null)
      {
        return this;
      }

      if (from.IsConstant && to.IsConstant)
      {
        var set = new HashSet<string>(values.Select(s => s.Replace(from.LowerBound, to.LowerBound)));
        return new Brick(set, min, max);
      }
      else
      {
        return Top;
      }
    }

    internal bool IsSameFixedLength(Brick other)
    {
      int length = values.Min(value => value.Length);

      return values.All(value => value.Length == length) && other.values.All(value => value.Length == length);
    }

    internal bool LessThanEqual(Brick other)
    {
      if (other.IsTop || IsBottom)
      {
        return true;
      }
      else if (IsTop || other.IsBottom)
      {
        return false;
      }

      if (values.IsSubsetOf(other.values) && min >= other.min && max <= other.max)
      {
        return true;
      }

      // underapproximation
      return false;
    }

    internal Brick Meet(Brick other)
    {
      if (IsTop || other.IsBottom)
      {
        return other;
      }
      else if (other.IsTop || IsBottom)
      {
        return this;
      }
      else if (this == other)
      {
        return this;
      }

      if ((!this.CanOverlap(other) && !other.CanOverlap(this)) || IsSameFixedLength(other))
      {
        HashSet<string> newValues = new HashSet<string>(values);
        newValues.IntersectWith(other.values);
        return new Brick(newValues, IndexInt.Max(this.min, other.min), IndexInt.Min(this.max, other.max));
      }

      //overapproximation
      return this;
    }

    internal bool CanOverlap(Brick other)
    {
      // Empty string can always overlap
      if (CanBeEmpty || other.CanBeEmpty)
      {
        return true;
      }

      foreach (string value in values)
      {
        KMP kmp = new KMP(value);
        foreach (string otherValue in other.values)
        {
          if (kmp.CanOverlap(otherValue))
          {
            return true;
          }
        }
      }
      return false;
    }

    #endregion

    #endregion
  }
}
