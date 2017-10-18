// CodeContracts
// 
// Copyright (c) Microsoft Corporation
// Copyright (c) Charles University
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
    /// Represents an interval of possible string indices.
    /// </summary>
    public class IndexInterval : Numerical.IntervalBase<IndexInterval, IndexInt>
    {
        private const int wideningThreshold = 100;

        private IndexInterval(IndexInt lowerBound, IndexInt upperBound)
          : base(lowerBound, upperBound)
        {
        }

        #region IntervalBase implementation
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
            get { return upperBound.IsInfinite; }
        }

        public override bool IsNormal
        {
            get { return !IsBottom && !IsTop; }
        }

        public override IndexInterval ToUnsigned()
        {
            throw new NotImplementedException();
        }

        public override bool LessEqual(IndexInterval a)
        {
            bool result;
            if (AbstractDomainsHelper.TryTrivialLessEqual(this, a, out result))
            {
                return result;
            }

            return this.LowerBound >= a.LowerBound && this.UpperBound <= a.UpperBound;

        }

        public override bool IsBottom
        {
            get { return this.LowerBound > this.UpperBound; }
        }

        public override bool IsTop
        {
            get { return lowerBound.IsNegative && upperBound.IsInfinite; }
        }

        public override IndexInterval Bottom
        {
            get { return new IndexInterval(IndexInt.Infinity, IndexInt.Negative); }
        }

        public override IndexInterval Top
        {
            get { return new IndexInterval(IndexInt.Negative, IndexInt.Infinity); }
        }

        public override IndexInterval Join(IndexInterval a)
        {
            return IndexInterval.For(IndexInt.Min(lowerBound, a.lowerBound), IndexInt.Max(upperBound, a.upperBound));
        }

        public override IndexInterval Meet(IndexInterval a)
        {
            return IndexInterval.For(IndexInt.Max(lowerBound, a.lowerBound), IndexInt.Min(upperBound, a.upperBound));
        }

        public override IndexInterval Widening(IndexInterval a)
        {
            IndexInterval joined = Join(a);
            if (joined.IsUpperBoundPlusInfinity || (!joined.LowerBound.IsInfinite &&
              joined.UpperBound.AsInt - joined.LowerBound.AsInt >= wideningThreshold))
            {
                return Unknown;
            }

            return joined;
        }

        public override IndexInterval DuplicateMe()
        {
            return new IndexInterval(lowerBound, upperBound);
        }
        #endregion

        #region Factory methods

        public static IndexInterval Infinity
        {
            get
            {
                return IndexInterval.For(IndexInt.Infinity);
            }
        }

        public static IndexInterval Unreached
        {
            get
            {
                return new IndexInterval(IndexInt.Infinity, IndexInt.Negative);
            }
        }

        /// <summary>
        /// Gets an index interval representing an unknown value (top).
        /// </summary>
        public static IndexInterval Unknown
        {
            get
            {
                return new IndexInterval(IndexInt.Negative, IndexInt.Infinity);
            }
        }
        /// <summary>
        /// Gets an index interval representing an unknown non-negative value.
        /// </summary>
        public static IndexInterval UnknownNonNegative
        {
            get
            {
                return new IndexInterval(IndexInt.For(0), IndexInt.Infinity);
            }
        }


        /// <summary>
        /// Makes an index interval containing the constant index.
        /// </summary>
        /// <param name="constant">The constant index.</param>
        /// <returns>Index interval containing only <paramref name="constant"/>.</returns>
        public static IndexInterval For(IndexInt constant)
        {
            return new IndexInterval(constant, constant);
        }

        /// <summary>
        /// Makes an index interval with the specified lower and upper bound indices
        /// </summary>
        /// <param name="lowerBound">The lower bound index.</param>
        /// <param name="upperBound">The upper bound index.</param>
        /// <returns>Index interval containing indices between <paramref name="lowerBound"/> and <paramref name="upperBound"/>.</returns>
        public static IndexInterval For(IndexInt lowerBound, IndexInt upperBound)
        {
            return new IndexInterval(lowerBound, upperBound);
        }
        public static IndexInterval For(int lowerBound, int upperBound)
        {
            return For(IndexInt.For(lowerBound), IndexInt.For(upperBound));
        }

        public static IndexInterval For(int intConstant)
        {
            return For(IndexInt.For(intConstant));
        }
        #endregion

        /// <summary>
        /// Determines whether the interval represents a signle index or infinity.
        /// </summary>
        public bool IsConstant
        {
            get
            {
                return lowerBound == upperBound && !lowerBound.IsNegative;
            }
        }

        /// <summary>
        /// Determines whether the interval is just infinity.
        /// </summary>
        public bool IsInfinity
        {
            get
            {
                return lowerBound.IsInfinite && upperBound.IsInfinite;
            }
        }

        /// <summary>
        /// Determines whether the interval contains only a single integer (finite, non-negative).
        /// </summary>
        public bool IsFiniteConstant
        {
            get
            {
                return lowerBound == upperBound && !lowerBound.IsNegative && !upperBound.IsInfinite;
            }
        }

        /// <summary>
        /// Converts an interval of rational numbers to an interval of indices.
        /// </summary>
        /// <param name="interval">The rational interval.</param>
        /// <returns>The index interval corresponding to <paramref name="interval"/>.</returns>
        public static IndexInterval For(Numerical.Interval interval)
        {
            if (interval.IsUpperBoundPlusInfinity)
            {
                return For(IndexInt.For((int)interval.LowerBound.PreviousInt32), IndexInt.Infinity);
            }
            else
            {
                return For((int)interval.LowerBound.PreviousInt32, (int)interval.UpperBound.NextInt32);
            }
        }

        /// <summary>
        /// Converts the interval if indices to disjoint numerical intervals.
        /// </summary>
        /// <returns>Disjoint numerical intervals representing the same indices.</returns>
        public Numerical.DisInterval ToDisInterval()
        {
            return Numerical.DisInterval.For(lowerBound.ToRational(), upperBound.ToRational());
        }

        public override bool Equals(object obj)
        {
            IndexInterval interval = obj as IndexInterval;
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
            return lowerBound.GetHashCode() + 33 * upperBound.GetHashCode();
        }

        /// <summary>
        /// Computes indices after moving by an offset interval. (Similar to subtraction).
        /// </summary>
        /// <param name="offset">The interval of the offset.</param>
        /// <returns>An interval of indices after <paramref name="offset"/></returns>
        public IndexInterval AfterOffset(IndexInterval offset)
        {
            if (offset.lowerBound > upperBound)
            {
                return Bottom;
            }

            IndexInt upperAfter = upperBound - offset.lowerBound;
            IndexInt lowerAfter;
            if (offset.upperBound > lowerBound)
            {
                lowerAfter = IndexInt.For(0);
            }
            else
            {
                lowerAfter = lowerBound - offset.upperBound;
            }

            return IndexInterval.For(lowerAfter, upperAfter);
        }

        public IndexInterval Add(int offset)
        {
            return new IndexInterval(lowerBound.Add(offset), upperBound.Add(offset));

        }

        public static IndexInterval operator+(IndexInterval left, IndexInterval right)
        {
            return new IndexInterval(left.lowerBound + right.LowerBound, left.upperBound + right.upperBound);
        }

        public bool ContainsValue(int value)
        {
            return lowerBound <= value && upperBound >= value;
        }
    }
}
