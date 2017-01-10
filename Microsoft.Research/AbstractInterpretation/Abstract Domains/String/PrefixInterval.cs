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

using Microsoft.Research.Regex;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.AbstractDomains.Numerical;

namespace Microsoft.Research.AbstractDomains.Strings
{

    /// <summary>
    /// Elements of the prefix abstract domain for strings.
    /// Represents a set of strings starting with a specified prefix.
    /// </summary>
    public class PrefixInterval : IntervalBase<PrefixInterval, Prefix>, IStringInterval<PrefixInterval>
    {
        private static Prefix Wrap(string s)
        {
            return new Prefix(s);
        }


        /// <summary>
        /// Constructs a prefix abstraction for the specified constant.
        /// </summary>
        /// <param name="constant">Constant string value.</param>
        public PrefixInterval(string constant)
          : base(Wrap(constant), Wrap(constant))
        {

        }
        /// <summary>
        /// Constructs a new instance of prefix.
        /// </summary>
        /// <param name="p">The prefix that is copied.</param>
        public PrefixInterval(PrefixInterval p)
          : base(p.LowerBound, p.UpperBound)
        {

        }

        private PrefixInterval(Prefix lower, Prefix upper)
          : base(lower, upper)
        {
        }

        #region Domain properties
        /// <summary>
        /// Gets the top element of the Prefix domain.
        /// </summary>
        public override PrefixInterval Top
        {
            get
            {
                return new PrefixInterval(lowerBound.Bottom, upperBound.Top);
            }
        }

        /// <summary>
        /// Gets the bottom element of the Prefix domain.
        /// </summary>
        public override PrefixInterval Bottom
        {
            get
            {
                return new PrefixInterval(lowerBound.Top, upperBound.Bottom);
            }
        }
        ///<inheritdoc/>
        public override bool IsTop
        {
            get { return lowerBound.IsBottom && upperBound.IsTop; }
        }
        ///<inheritdoc/>
        public override bool IsBottom
        {
            get { return !(lowerBound.LessThanEqual(upperBound)); }
        }

        public override bool IsInt32
        {
            get
            {
                return false;
            }
        }

        public override bool IsInt64
        {
            get
            {
                return false;
            }
        }

        public override bool IsLowerBoundMinusInfinity
        {
            get
            {
                return false;
            }
        }

        public override bool IsUpperBoundPlusInfinity
        {
            get
            {
                return false;
            }
        }

        public override bool IsNormal
        {
            get
            {
                return !IsTop && !IsBottom;
            }
        }

        ///<inheritdoc/>
        public bool ContainsValue(string value)
        {
            return (lowerBound.prefix == null || lowerBound.prefix.StartsWith(value, StringComparison.Ordinal)) && upperBound.ContainsValue(value);
        }
        ///<inheritdoc/>
        public bool Equals(PrefixInterval other)
        {
            return lowerBound.Equals(other.lowerBound) && upperBound.Equals(other.upperBound);
        }
        ///<inheritdoc/>
        public bool LessThanEqual(PrefixInterval other)
        {
            if (IsBottom)
                return true;
            else if (other.IsBottom)
                return false;
            return upperBound.LessThanEqual(other.upperBound) && other.lowerBound.LessThanEqual(lowerBound);
        }
        #endregion

        #region Domain operations
        ///<inheritdoc/>
        public override PrefixInterval Join(PrefixInterval or)
        {
            if (IsBottom)
                return or;
            if (or.IsBottom)
                return this;

            return new PrefixInterval(lowerBound.Meet(or.lowerBound), upperBound.Join(or.upperBound));
        }
        ///<inheritdoc/>
        public override PrefixInterval Meet(PrefixInterval and)
        {
            if (IsBottom)
                return this;
            if (and.IsBottom)
                return and;

            return new PrefixInterval(lowerBound.Join(and.lowerBound), upperBound.Meet(and.upperBound));
        }
        #endregion

        #region String operations

        /// <summary>
        /// Implements string operations for the Prefix abstract domain.
        /// </summary>
        /// <typeparam name="Variable">The type representing variables.</typeparam>
        public class Operations<Variable> : IStringIntervalOperations<PrefixInterval, Variable>
          where Variable : class, IEquatable<Variable>
        {
            private Prefix.Operations<Variable> prefixOperations = new Prefix.Operations<Variable>();

            #region IStringAbstractionFactory implementation
            public PrefixInterval Top
            {
                get { return new PrefixInterval(Wrap(null), Wrap("")); }
            }

            public PrefixInterval Constant(string v)
            {
                return new PrefixInterval(v);
            }
            #endregion

            private PrefixInterval ForUpperBound(Prefix upperBound)
            {
                return new PrefixInterval(upperBound.Bottom, upperBound);
            }
            private WithConstants<Prefix> GetUpperBoundArg(WithConstants<PrefixInterval> intervalWC)
            {
                PrefixInterval interval = intervalWC.ToAbstract(this);
                if (interval.IsSingleton)
                {
                    return new WithConstants<Prefix>(interval.upperBound.prefix);
                }
                else
                {
                    return new WithConstants<Prefix>(interval.upperBound);
                }
            }

            public IEnumerable<OrderPredicate<Variable>> ConcatOrder(Variable targetVariable, Variable selfVariable, Variable otherVariable)
            {
                if (selfVariable != null)
                {
                    return new OrderPredicate<Variable>[]
                    {
          OrderPredicate.For(targetVariable, selfVariable)
                    };
                }
                else
                {
                    return Enumerable.Empty<OrderPredicate<Variable>>();
                }
            }

            #region Operations returning strings
            ///<inheritdoc/>
            public PrefixInterval Concat(WithConstants<PrefixInterval> left, WithConstants<PrefixInterval> right)
            {
                PrefixInterval leftInterval = left.ToAbstract(this);
                PrefixInterval rightInterval = right.ToAbstract(this);

                if (leftInterval.IsSingleton)
                {
                    string leftConstant = leftInterval.lowerBound.prefix;
                    // only if there is constant on the left, we can use 
                    // information about the right part
                    return new PrefixInterval(Wrap(leftConstant + rightInterval.lowerBound.prefix), Wrap(leftConstant + rightInterval.upperBound.prefix));
                }
                else
                {
                    return leftInterval;
                }
            }
            ///<inheritdoc/>
            public PrefixInterval Insert(WithConstants<PrefixInterval> self, IndexInterval index, WithConstants<PrefixInterval> other)
            {
                /*PrefixInterval selfInterval = self.ToAbstract(this);
                PrefixInterval otherInterval = other.ToAbstract(this);

                if (index.IsFiniteConstant)
                {
                  int i = index.LowerBound.AsInt;

                  if (i > selfPrefix.prefix.Length)
                  {
                    if (self.IsConstant)
                    {
                      return selfPrefix.Bottom;
                    }
                    else
                    {
                      return selfPrefix;
                    }
                  }
                  else
                  {
                    if (other.IsConstant)
                    {
                      return new Prefix(selfPrefix.prefix.Insert(i, other.Constant));
                    }
                    else
                    {
                      return new Prefix(selfPrefix.prefix.Substring(0, i) + otherPrefix.prefix);
                    }
                  }

                }
                else if (index.LowerBound <= selfPrefix.prefix.Length)
                {
                  return new Prefix(selfPrefix.prefix.Substring(0, index.LowerBound.AsInt));
                }
                else
                {
                  return selfPrefix.Top;
                }*/

                return ForUpperBound(prefixOperations.Insert(GetUpperBoundArg(self), index, GetUpperBoundArg(other)));
            }
            ///<inheritdoc/>
            public PrefixInterval Replace(PrefixInterval self, CharInterval from, CharInterval to)
            {
                /*
                if (from.IsConstant && to.IsConstant)
                  return new Prefix(self.prefix.Replace(from.LowerBound, to.LowerBound));

                int l = 0;

                while (l < self.prefix.Length && !from.Contains(self.prefix[l]))
                {
                  ++l;
                }

                return new Prefix(self.prefix.Substring(0, l));*/

                return ForUpperBound(prefixOperations.Replace(self.upperBound, from, to));
            }
            ///<inheritdoc/>
            public PrefixInterval Replace(WithConstants<PrefixInterval> self, WithConstants<PrefixInterval> from, WithConstants<PrefixInterval> to)
            {
                /*
                PrefixInterval selfPrefix = self.ToAbstract(this);

                if (from.IsConstant && to.IsConstant)
                {
                  KMP kmp = new KMP(from.Constant);
                  string repl = kmp.PrefixOfReplace(selfPrefix.prefix, to.Constant);
                  return new Prefix(repl);
                }
                else
                {
                  return Top;
                }
                */

                return ForUpperBound(prefixOperations.Replace(GetUpperBoundArg(self), GetUpperBoundArg(from), GetUpperBoundArg(to)));
            }
            ///<inheritdoc/>
            public PrefixInterval Substring(PrefixInterval self, IndexInterval index, IndexInterval length)
            {
                /*if (index.IsFiniteConstant)
                {
                  int indexInt = index.LowerBound.AsInt;

                  if (!length.LowerBound.IsInfinite && indexInt + length.LowerBound.AsInt < self.prefix.Length)
                  {
                    return new Prefix(self.prefix.Substring(indexInt, length.LowerBound.AsInt));
                  }
                  else if (indexInt < self.prefix.Length)
                  {
                    return new Prefix(self.prefix.Substring(indexInt));
                  }
                  else
                  {
                    return self.Top;
                  }
                }
                else
                {
                  return self.Top;
                }*/

                return ForUpperBound(prefixOperations.Substring(self.upperBound, index, length));
            }
            ///<inheritdoc/>
            public PrefixInterval Remove(PrefixInterval self, IndexInterval index, IndexInterval length)
            {
                /*if (index.LowerBound.AsInt >= self.prefix.Length)
                {
                  return self;
                }
                else if (index.IsFiniteConstant && length.IsFiniteConstant && length.LowerBound.AsInt + index.LowerBound.AsInt < self.prefix.Length)
                {
                  return new Prefix(self.prefix.Remove(index.LowerBound.AsInt, length.LowerBound.AsInt));
                }
                else
                {
                  return new Prefix(self.prefix.Remove(index.LowerBound.AsInt));
                }*/

                return ForUpperBound(prefixOperations.Remove(self.upperBound, index, length));
            }
            ///<inheritdoc/>
            public PrefixInterval PadLeftRight(PrefixInterval self, IndexInterval length, CharInterval fill, bool right)
            {
                /*if (length.UpperBound.AsInt <= self.prefix.Length)
                  return self;
                else if (fill.IsConstant)
                  return new Prefix(StringUtils.LongestConstantPrefix(self.prefix, fill.LowerBound));
                else
                  return Top;*/
                  if(right)
                    return self;
                  else
                return ForUpperBound(prefixOperations.PadLeftRight(self.upperBound, length, fill, false));
            }
          
            ///<inheritdoc/>
            public PrefixInterval Trim(WithConstants<PrefixInterval> self, WithConstants<PrefixInterval> trimmed)
            {
                /*if (trimmed.IsConstant && trimmed.Constant != "")
                {
                  PrefixInterval selfPrefix = self.ToAbstract(this);
                  return new Prefix(selfPrefix.prefix.Trim(trimmed.Constant.ToCharArray()));
                }
                else
                {
                  return Top;
                }*/

                return ForUpperBound(prefixOperations.Trim(GetUpperBoundArg(self), GetUpperBoundArg(trimmed)));
            }
            ///<inheritdoc/>
            public PrefixInterval TrimStartEnd(WithConstants<PrefixInterval> self, WithConstants<PrefixInterval> trimmed, bool end)
            {
                if(end)
                    return ForUpperBound(prefixOperations.TrimStartEnd(GetUpperBoundArg(self), GetUpperBoundArg(trimmed), true));

                if (trimmed.IsConstant && trimmed.Constant != "")
                {
                    char[] trimArray = trimmed.Constant.ToCharArray();
                    PrefixInterval selfInterval = self.ToAbstract(this);

                    string trimmedLower = selfInterval.lowerBound.IsBottom ? null : selfInterval.lowerBound.prefix.TrimStart(trimArray);
                    string trimmedUpper = selfInterval.upperBound.prefix.TrimStart(trimArray);

                    return new PrefixInterval(Wrap(trimmedLower), Wrap(trimmedUpper));
                }
                else
                {
                    return Top;
                }
            }

            #endregion
            #region Operations returning integers


            ///<inheritdoc/>
            public CompareResult CompareOrdinal(
              WithConstants<PrefixInterval> self,
              WithConstants<PrefixInterval> other)
            {
                return prefixOperations.CompareOrdinal(GetUpperBoundArg(self), GetUpperBoundArg(other));

            }
            ///<inheritdoc/>
            public IndexInterval GetLength(PrefixInterval self)
            {
                IndexInt shortest = IndexInt.For(self.upperBound.prefix.Length);
                IndexInt longest = self.lowerBound.IsBottom ? IndexInt.Infinity : IndexInt.For(self.lowerBound.prefix.Length);

                return IndexInterval.For(shortest, longest);
            }
            ///<inheritdoc/>
            public IndexInterval IndexOf(WithConstants<PrefixInterval> self, WithConstants<PrefixInterval> needle, IndexInterval offset, IndexInterval count, bool last)
            {
                /*if (!offset.IsFiniteConstant || offset.LowerBound != 0 || !count.IsInfinity)
                {
                  // Offset and count are not supported
                  return IndexInterval.Unknown;
                }

                PrefixInterval selfAbstraction = self.ToAbstract(this);

                if (needle.IsConstant)
                {
                  int knownIndex = selfAbstraction.upperBound.prefix.IndexOf(needle.Constant, StringComparison.Ordinal);

                  if (knownIndex >= 0)
                  {
                    return IndexInterval.For(knownIndex);
                  }
                  else if (!selfAbstraction.lowerBound.IsBottom)
                  {
                    int possibleIndex = selfAbstraction.prefix.IndexOf(needle.Constant, StringComparison.Ordinal);
                  }
                  else
                  {
                    //NOTE: would be better with multiple intervals
                    return IndexInterval.Unknown;
                  }
                }
                else
                {
                  return IndexInterval.Unknown;
                }
                */
                return prefixOperations.IndexOf(GetUpperBoundArg(self), GetUpperBoundArg(needle), offset, count, last);
            }
     
            #endregion

            #region Predicate operations
            ///<inheritdoc/>
            public IStringPredicate IsEmpty(PrefixInterval self, Variable selfVariable)
            {
                //Lower bound does not tell anything
                return prefixOperations.IsEmpty(self.upperBound, selfVariable);
            }
            ///<inheritdoc/>
            public IStringPredicate Contains(WithConstants<PrefixInterval> self, Variable selfVariable, WithConstants<PrefixInterval> other, Variable otherVariable)
            {
                /*Prefix selfPrefix = self.ToAbstract(this);
                Prefix otherPrefix = other.ToAbstract(this);

                if (other.IsConstant && selfPrefix.prefix.Contains(other.Constant))
                {
                  return FlatPredicate.True;
                }
                else if (self.IsConstant && !self.Constant.Contains(otherPrefix.prefix))
                {
                  return FlatPredicate.False;
                }
                else
                {
                  return FlatPredicate.Top;
                }*/

                return prefixOperations.Contains(GetUpperBoundArg(self), selfVariable, GetUpperBoundArg(other), otherVariable);
            }
            ///<inheritdoc/>
            public IStringPredicate StartsWithOrdinal(WithConstants<PrefixInterval> self, Variable selfVariable, WithConstants<PrefixInterval> other, Variable otherVariable)
            {
                return StartsWithOrdinal(self, selfVariable, other, otherVariable, new NoOrderQuery<Variable>());
            }
            public IStringPredicate StartsWithOrdinal(WithConstants<PrefixInterval> self, Variable selfVariable, WithConstants<PrefixInterval> other, Variable otherVariable, IStringOrderQuery<Variable> orderQuery)
            {

                if (selfVariable != null && otherVariable != null && orderQuery.CheckMustBeLessEqualThan(selfVariable, otherVariable))
                    return FlatPredicate.True;

                PrefixInterval selfInterval = self.ToAbstract(this);
                PrefixInterval otherInterval = other.ToAbstract(this);

                if (selfInterval.upperBound.LessThanEqual(otherInterval.lowerBound))
                {
                    return FlatPredicate.True;
                }
                else if (!selfInterval.lowerBound.LessThanEqual(otherInterval.upperBound))
                {
                    return FlatPredicate.False;
                }

                if (selfVariable != null && otherVariable != null)
                {
                    return OrderPredicate.For(selfVariable, otherVariable);
                }
                else if (selfVariable != null)
                {
                    return StringAbstractionPredicate.ForTrue(selfVariable, ForUpperBound(otherInterval.upperBound));
                }
                else if (otherVariable != null)
                {
                    return StringAbstractionPredicate.ForTrue(otherVariable, new PrefixInterval(selfInterval.lowerBound, selfInterval.upperBound.Top));
                }
                else
                {
                    return FlatPredicate.Top;
                }

            }
            ///<inheritdoc/>
            public IStringPredicate EndsWithOrdinal(WithConstants<PrefixInterval> self, Variable selfVariable, WithConstants<PrefixInterval> other, Variable otherVariable, IStringOrderQuery<Variable> orderQuery)
            {
                return EndsWithOrdinal(self, selfVariable, other, otherVariable);
            }

            ///<inheritdoc/>
            public IStringPredicate EndsWithOrdinal(WithConstants<PrefixInterval> self, Variable selfVariable, WithConstants<PrefixInterval> other, Variable otherVariable)
            {
                /*Prefix selfPrefix = self.ToAbstract(this);
                Prefix otherPrefix = other.ToAbstract(this);

                if (other.IsConstant && other.Constant == "")
                {
                  return FlatPredicate.True;
                }
                else if (self.IsConstant && !self.Constant.Contains(otherPrefix.prefix))
                {
                  return FlatPredicate.False;
                }
                else
                {
                  return FlatPredicate.Top;
                }*/

                return prefixOperations.EndsWithOrdinal(GetUpperBoundArg(self), selfVariable, GetUpperBoundArg(other), otherVariable);
            }
            ///<inheritdoc/>
            public IStringPredicate Equals(WithConstants<PrefixInterval> self, Variable selfVariable,
              WithConstants<PrefixInterval> other, Variable otherVariable)
            {
                return Equals(self, selfVariable, other, otherVariable, new NoOrderQuery<Variable>());
            }

            public IStringPredicate Equals(WithConstants<PrefixInterval> self, Variable selfVariable,
              WithConstants<PrefixInterval> other, Variable otherVariable, IStringOrderQuery<Variable> orderQuery)
            {
                if (selfVariable != null && otherVariable != null && orderQuery.CheckMustBeLessEqualThan(selfVariable, otherVariable) && orderQuery.CheckMustBeLessEqualThan(otherVariable, selfVariable))
                {
                    return FlatPredicate.True;
                }

                PrefixInterval selfInterval = self.ToAbstract(this);
                PrefixInterval otherInterval = other.ToAbstract(this);

                if (selfInterval.upperBound.LessThanEqual(otherInterval.lowerBound) && otherInterval.upperBound.LessThanEqual(selfInterval.lowerBound))
                {
                    return FlatPredicate.True;
                }
                else if (!selfInterval.lowerBound.LessThanEqual(otherInterval.upperBound) || !otherInterval.lowerBound.LessThanEqual(selfInterval.upperBound))
                {
                    return FlatPredicate.False;
                }

                if (selfVariable != null && !otherInterval.IsTop)
                {
                    return StringAbstractionPredicate.ForTrue(selfVariable, otherInterval);
                }
                else if (otherVariable != null && !selfInterval.IsTop)
                {
                    return StringAbstractionPredicate.ForTrue(otherVariable, selfInterval);
                }
                else if (selfVariable != null && otherVariable != null)
                {
                    return OrderPredicate.For(selfVariable, otherVariable);
                }
                else
                {
                    return FlatPredicate.Top;
                }


            }
            ///<inheritdoc/>
            public IStringPredicate RegexIsMatch(PrefixInterval self, Variable selfVariable, Regex.AST.Element regex)
            {
                PrefixRegex prefixRegexConverter = new PrefixRegex(self.upperBound);
                CodeAnalysis.ProofOutcome outcome = prefixRegexConverter.IsMatch(regex);

                /*if (outcome == CodeAnalysis.ProofOutcome.Top)
                {
                  Prefix regexPrefix = prefixRegexConverter.PrefixForRegex(regex);
                  if (!regexPrefix.IsBottom && !regexPrefix.IsTop && selfVariable != null)
                  {
                    return StringAbstractionPredicate.ForTrue(selfVariable, regexPrefix);
                  }
                }*/

                return FlatPredicate.ForProofOutcome(outcome);
            }

            #endregion

            ///<inheritdoc/>
            public PrefixInterval SetCharAt(PrefixInterval self, IndexInterval index, CharInterval value)
            {
                /*if (index.LowerBound > self.prefix.Length)
                {
                  // Setting too far
                  return self;
                }
                else
                {
                  // Cut before the first possible changed position
                  string changedPrefix = self.prefix.Substring(0, index.LowerBound.AsInt);
                  if (index.IsConstant && value.IsConstant)
                  {
                    // We know exactly what changed
                    changedPrefix += value.LowerBound.ToString();
                    if (index.LowerBound.AsInt < self.prefix.Length - 1)
                    {
                      // if there is a part after, it can be preserved
                      changedPrefix += self.prefix.Substring(index.LowerBound.AsInt + 1);
                    }
                  }
                  return new Prefix(changedPrefix);
                }
                */

                return ForUpperBound(prefixOperations.SetCharAt(self.upperBound, index, value));
            }

            ///<inheritdoc/>
            public CharInterval GetCharAt(PrefixInterval self, IndexInterval index)
            {
                /*if (index.UpperBound >= self.prefix.Length)
                {
                  return CharInterval.Unknown;
                }
                else
                {
                  CharInterval interval = CharInterval.Unreached;

                  for (int i = IndexInt.Max(index.LowerBound, IndexInt.For(0)).AsInt; i < self.prefix.Length && index.UpperBound >= i; ++i)
                  {
                    interval = interval.Join(CharInterval.For(self.prefix[i]));
                  }
                  return interval;
                }*/

                return prefixOperations.GetCharAt(self.upperBound, index);
            }
        }

        #endregion

        #region Object
        public override string ToString()
        {
            if (IsBottom)
                return "_|_";
            else if (lowerBound.IsBottom)
                return upperBound.ToString();
            else if (upperBound.LessThanEqual(lowerBound))
            {
                return upperBound.prefix.ToString();
            }
            else
                return string.Format("{0}[{1}]", upperBound.prefix, lowerBound.prefix.Substring(upperBound.prefix.Length));


        }

        public override bool Equals(object obj)
        {
            return Equals(obj as PrefixInterval);
        }

        public override int GetHashCode()
        {
            return lowerBound.GetHashCode() + 33 * upperBound.GetHashCode();
        }
        #endregion
        ///<inheritdoc/>
        public PrefixInterval Constant(string cst)
        {
            return new PrefixInterval(cst);
        }

        public override PrefixInterval ToUnsigned()
        {
            return this;
        }

        public override bool LessEqual(PrefixInterval a)
        {
            return LessThanEqual(a);
        }

        public override PrefixInterval Widening(PrefixInterval a)
        {
            PrefixInterval jn = Join(a);
            if (jn.lowerBound.IsBottom)
                return jn;
            else
                return new PrefixInterval(jn.lowerBound.Bottom, jn.upperBound);
        }

        public override PrefixInterval DuplicateMe()
        {
            return new PrefixInterval(lowerBound, upperBound);
        }

        public bool CheckMustBeLessEqualThan(PrefixInterval greaterEqual)
        {
            return upperBound.LessThanEqual(greaterEqual.lowerBound);
        }

        public bool TryRefineLessEqual(ref PrefixInterval lessEqual)
        {
            if (upperBound.LessThanEqual(lessEqual.upperBound) && !(upperBound.Equals(lessEqual.upperBound)))
            {
                lessEqual = new PrefixInterval(lessEqual.lowerBound, upperBound);
                return true;
            }
            else
            {
                return false;
            }
        }

        public bool TryRefineGreaterEqual(ref PrefixInterval greaterEqual)
        {
            if (greaterEqual.lowerBound.LessThanEqual(lowerBound) && !(lowerBound.Equals(greaterEqual.lowerBound)))
            {
                greaterEqual = new PrefixInterval(lowerBound, greaterEqual.upperBound);
                return true;
            }
            else
            {
                return false;
            }
        }
    }

}
