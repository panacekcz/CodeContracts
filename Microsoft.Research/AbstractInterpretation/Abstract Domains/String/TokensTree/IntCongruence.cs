﻿// CodeContracts
// 
// Copyright 2016-2017 Charles University
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

// Created by Vlastimil Dort (2016-2017)

using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Represents a set of integers that have the same remainder when divided by a certain visitor.
    /// </summary>
    internal struct Congruence
    {
        private readonly int divisor, remainder;

        public int Divisor { get { return divisor; } }
        public int Remainder { get { return remainder; } }

        private static int GreatestCommonDivisor(int a, int b)
        {
            Contract.Requires(a >= 0);
            Contract.Requires(b >= 0);

            while (b > 0)
            {
                int next = a % b;
                a = b;
                b = next;
            }

            return a;
        }
        private static int Modulo(int a, int b)
        {
            Contract.Requires(a >= 0);
            Contract.Requires(b >= 0);

            return b == 0 ? a : a % b;
        }

        private Congruence(int divisor, int remainder)
        {
            this.divisor = divisor;
            this.remainder = remainder;
        }

        public bool IsBottom
        {
            get
            {
                return divisor != 0 && remainder >= divisor;
            }
        }
        public bool IsConstant
        {
            get
            {
                return divisor == 0;
            }
        }
        public Congruence Add(int constant)
        {
            checked
            {
                if (IsBottom)
                    return this;
                if (IsConstant)
                    return For(remainder + 1);
                return For(divisor, remainder + constant);
            }
        }
        public Congruence Add(Congruence other)
        {
            checked
            {
                if (IsBottom)
                    return other;
                else if (other.IsBottom)
                    return this;

                int newDivisor = GreatestCommonDivisor(divisor, other.divisor);

                return For(newDivisor, remainder + other.remainder);
            }
        }
        public Congruence Join(Congruence other)
        {
            if (IsBottom)
                return other;
            else if (other.IsBottom)
                return this;

            int newDivisor = GreatestCommonDivisor(divisor, other.divisor);
            int newLeft = Modulo(remainder, newDivisor);
            int newRight = Modulo(other.remainder, newDivisor);

            newDivisor = GreatestCommonDivisor(newDivisor, Math.Abs(newLeft - newRight));
            return For(newDivisor, newLeft);
        }

        public Congruence WithDivisor(int otherDivisor)
        {
            if (IsBottom)
                return this;

            int newDivisor = GreatestCommonDivisor(divisor, otherDivisor);
            return For(newDivisor, Modulo(remainder, newDivisor));
        }

        public static Congruence For(int divider, int remainder)
        {
            if (divider < 0 || remainder < 0)
                throw new ArgumentOutOfRangeException();

            return new Congruence(divider, Modulo(remainder, divider));
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

        public int CommonDivisor
        {
            get
            {
                return GreatestCommonDivisor(divisor, remainder);
            }
        }

        public int RemainderFor(int number)
        {
            return Modulo(number, divisor);
        }
    }

    /// <summary>
    /// Stores a pair of congruences, one for the length of the repeated part,
    /// one for the length of the suffix part.
    /// </summary>
    internal struct CongruencePair
    {
        private readonly Congruence repeat, suffix;

        public Congruence Repeat { get { return repeat; } }
        public Congruence Suffix { get { return suffix; } }

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

        /// <summary>
        /// Congruence for the length of the whole string including repeating and suffix part.
        /// </summary>
        public Congruence Total
        {
            get
            {
                Congruence repeated = repeat.Join(Congruence.For(0));
                return repeated.Add(suffix);
            }
        }
    }
}
