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
    /// Represents possible results of the <see cref="IComparable.CompareTo"/> method.
    /// </summary>
    public enum CompareResult
    {
        /// <summary>
        /// No possible result (unreachable).
        /// </summary>
        Bottom,
        /// <summary>
        /// Less (negative).
        /// </summary>
        Less,
        /// <summary>
        /// Equal (zero).
        /// </summary>
        Equal,
        /// <summary>
        /// Less or equal (negative, zero).
        /// </summary>
        LessEqual,
        /// <summary>
        /// Greater (positive).
        /// </summary>
        Greater,
        /// <summary>
        /// Not equal (negative, positive).
        /// </summary>
        NotEqual,
        /// <summary>
        /// Greater or equal (zero, positive).
        /// </summary>
        GreaterEqual,
        /// <summary>
        /// All possible results (negative, zero, positive).
        /// </summary>
        Top
    }

    /// <summary>
    /// Extension methods for <see cref="CompareResult"/>.
    /// </summary>
    public static class CompareResultExtensions
    {
        /// <summary>
        /// Creates a compare result corresponding to compare with swapped sides.
        /// </summary>
        /// <param name="result">The original compare result.</param>
        /// <returns>The swapped compare result.</returns>
        public static CompareResult SwapSides(this CompareResult result)
        {
            return Build(result.HasFlag(CompareResult.Greater), result.HasFlag(CompareResult.Equal), result.HasFlag(CompareResult.Less));
        }

        /// <summary>
        /// Converts <see cref="CompareResult"/> to <see cref="Numerical.DisInterval"/>.
        /// </summary>
        /// <param name="compareResult">Possible compare results.</param>
        /// <returns>Intervals of possible compare result integer values.</returns>
        public static Numerical.DisInterval ToDisInterval(this CompareResult compareResult)
        {
            switch (compareResult)
            {
                case CompareResult.Bottom:
                    return Numerical.DisInterval.UnreachedInterval;
                case CompareResult.Less:
                    return Numerical.DisInterval.For(Numerical.Rational.MinusInfinity, -1);
                case CompareResult.Equal:
                    return Numerical.DisInterval.Zero;
                case CompareResult.LessEqual:
                    return Numerical.DisInterval.Negative;
                case CompareResult.Greater:
                    return Numerical.DisInterval.For(1, Numerical.Rational.PlusInfinity);
                case CompareResult.NotEqual:
                    return Numerical.DisInterval.NotZero;
                case CompareResult.GreaterEqual:
                    return Numerical.DisInterval.Positive;
                default:
                    return Numerical.DisInterval.UnknownInterval;
            }

        }

        /// <summary>
        /// Builds a <see cref="CompareResult"/> value from possible results.
        /// </summary>
        /// <param name="less">Whether negativer result is possible.</param>
        /// <param name="equal">Whether zero result is possible.</param>
        /// <param name="greater">Whether positive result is possible.</param>
        /// <returns>A <see cref="CompareResult"/> representing the specified possible result.</returns>
        public static CompareResult Build(bool less, bool equal, bool greater)
        {
            CompareResult result = CompareResult.Bottom;
            if (less)
                result |= CompareResult.Less;
            if (equal)
                result |= CompareResult.Equal;
            if (greater)
                result |= CompareResult.Greater;
            return result;
        }

        public static CompareResult FromInt(int result)
        {
            if (result < 0)
                return CompareResult.Less;
            else if (result > 0)
                return CompareResult.Greater;
            else
                return CompareResult.Equal;

        }
    }
}
