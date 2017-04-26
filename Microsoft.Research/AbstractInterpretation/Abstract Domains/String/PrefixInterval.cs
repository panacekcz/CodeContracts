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

        #region Construction
        /// <summary>
        /// Constructs a prefix interval abstraction for the specified constant.
        /// </summary>
        /// <param name="constant">Constant string value.</param>
        public PrefixInterval(string constant)
          : base(Wrap(constant), Wrap(constant))
        {

        }
        /// <summary>
        /// Constructs a prefix interval abstraction for the specified bounds.
        /// </summary>
        /// <param name="prefixOf">The string which the represented strings are prefixes of.</param>
        /// <param name="prefix">The known prefix of the represented string.</param>
        public PrefixInterval(string prefixOf, string prefix)
          : base(Wrap(prefixOf), Wrap(prefix))
        {

        }
        /// <summary>
        /// Constructs a new instance of prefix interval from another instance.
        /// </summary>
        /// <param name="p">The prefix interval that is copied.</param>
        public PrefixInterval(PrefixInterval p)
          : base(p.LowerBound, p.UpperBound)
        {

        }

        private PrefixInterval(Prefix lower, Prefix upper)
          : base(lower, upper)
        {
        }
        #endregion

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
            public PrefixInterval Bottom
            {
                get { return new PrefixInterval(Wrap(""), Wrap(null)); }
            }

            private PrefixInterval ForUpperBound(Prefix upperBound)
            {
                return new PrefixInterval(upperBound.Bottom, upperBound);
            }
            private PrefixInterval For(string lower, string upper)
            {
                return new PrefixInterval(Wrap(lower), Wrap(upper));
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

            public IEnumerable<OrderPredicate<Variable>> SubstringRemoveOrder(Variable targetVariable, Variable sourceVariable, IndexInterval index, IndexInterval length, bool remove)
            {
                if((!remove && index.IsFiniteConstant && index.LowerBound == 0) || (!remove && index.IsInfinity))
                {
                    return new OrderPredicate<Variable>[]
                    {
                        OrderPredicate.For(sourceVariable, targetVariable)
                    };
                }
                return Enumerable.Empty<OrderPredicate<Variable>>();
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
                    return For(leftConstant + rightInterval.lowerBound.prefix, leftConstant + rightInterval.upperBound.prefix);
                }
                else
                {
                    return leftInterval;
                }
            }
            ///<inheritdoc/>
            public PrefixInterval Insert(WithConstants<PrefixInterval> self, IndexInterval index, WithConstants<PrefixInterval> other)
            {
                return ForUpperBound(prefixOperations.Insert(GetUpperBoundArg(self), index, GetUpperBoundArg(other)));
            }
            ///<inheritdoc/>
            public PrefixInterval Replace(PrefixInterval self, CharInterval from, CharInterval to)
            {

                if (from.IsConstant && to.IsConstant && !self.lowerBound.IsBottom)
                    return For(self.lowerBound.prefix.Replace(from.LowerBound, to.LowerBound), self.upperBound.prefix.Replace(from.LowerBound, to.LowerBound));
                else
                    return ForUpperBound(prefixOperations.Replace(self.upperBound, from, to));
            }
            ///<inheritdoc/>
            public PrefixInterval Replace(WithConstants<PrefixInterval> self, WithConstants<PrefixInterval> from, WithConstants<PrefixInterval> to)
            {
                return ForUpperBound(prefixOperations.Replace(GetUpperBoundArg(self), GetUpperBoundArg(from), GetUpperBoundArg(to)));
            }
            ///<inheritdoc/>
            public PrefixInterval Substring(PrefixInterval self, IndexInterval index, IndexInterval length)
            {
                return ForUpperBound(prefixOperations.Substring(self.upperBound, index, length));
            }
            ///<inheritdoc/>
            public PrefixInterval Remove(PrefixInterval self, IndexInterval index, IndexInterval length)
            {
                return ForUpperBound(prefixOperations.Remove(self.upperBound, index, length));
            }
            ///<inheritdoc/>
            public PrefixInterval PadLeftRight(PrefixInterval self, IndexInterval length, CharInterval fill, bool right)
            {
                if (right)
                    return self;
                else
                    return ForUpperBound(prefixOperations.PadLeftRight(self.upperBound, length, fill, false));
            }

            ///<inheritdoc/>
            public PrefixInterval Trim(WithConstants<PrefixInterval> self, WithConstants<PrefixInterval> trimmed)
            {
                return ForUpperBound(prefixOperations.Trim(GetUpperBoundArg(self), GetUpperBoundArg(trimmed)));
            }
            ///<inheritdoc/>
            public PrefixInterval TrimStartEnd(WithConstants<PrefixInterval> self, WithConstants<PrefixInterval> trimmed, bool end)
            {
                if (end)
                    return ForUpperBound(prefixOperations.TrimStartEnd(GetUpperBoundArg(self), GetUpperBoundArg(trimmed), true));

                if (trimmed.IsConstant && trimmed.Constant != "")
                {
                    char[] trimArray = trimmed.Constant.ToCharArray();
                    PrefixInterval selfInterval = self.ToAbstract(this);

                    string trimmedLower = selfInterval.lowerBound.IsBottom ? null : selfInterval.lowerBound.prefix.TrimStart(trimArray);
                    string trimmedUpper = selfInterval.upperBound.prefix.TrimStart(trimArray);

                    return For(trimmedLower, trimmedUpper);
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

            private IndexInt LastPossibleIndex(PrefixInterval selfAbstraction, PrefixInterval needleAbstraction) {
                
                if (selfAbstraction.lowerBound.IsBottom)
                {
                    // The haystack is not a prefix of a known string, so there can be a match at any index large enough
                    return IndexInt.Infinity;
                }
                else
                {
                    // Up to the last possible index. May be -1, in which case it is definitely not there
                    int lastPossible = selfAbstraction.lowerBound.prefix.LastIndexOf(needleAbstraction.upperBound.prefix);
                    return IndexInt.For(lastPossible);
                }
            }

            private IndexInt FirstPossibleIndex(PrefixInterval selfAbstraction, PrefixInterval needleAbstraction)
            {
                if (selfAbstraction.lowerBound.IsBottom)
                {
                    // The haystack is not a prefix of a known string, so there can be a match at any index large enough
                    int firstPossibleKnown = selfAbstraction.UpperBound.prefix.IndexOf(needleAbstraction.upperBound.prefix);

                    if (firstPossibleKnown == -1)
                    {
                        // No match in the known part, but there can be matches after, and partially overlapping
                        firstPossibleKnown = Math.Max(0, selfAbstraction.upperBound.prefix.Length - needleAbstraction.upperBound.prefix.Length + 1);                        
                    }

                    return IndexInt.ForNonNegative(firstPossibleKnown);
                }
                else
                {
                    // First possible index. May be -1, in which case it is definitely not there
                    int firstPossible = selfAbstraction.lowerBound.prefix.IndexOf(needleAbstraction.upperBound.prefix);
                    return IndexInt.For(firstPossible);
                }
            }

            ///<inheritdoc/>
            public IndexInterval IndexOf(WithConstants<PrefixInterval> self, WithConstants<PrefixInterval> needle, IndexInterval offset, IndexInterval count, bool last)
            {
                if (!offset.IsFiniteConstant || offset.LowerBound != 0 || !count.IsInfinity)
                {
                    // Offset and count are not supported
                    return IndexInterval.Unknown;
                }

                PrefixInterval selfAbstraction = self.ToAbstract(this);
                PrefixInterval needleAbstraction = needle.ToAbstract(this);

                // Find first/last index of a definite substring
                int definiteSubstring;
                if (needleAbstraction.lowerBound.IsBottom) {
                    // The needle is not a prefix of a known string, so there is no definite match
                    definiteSubstring = -1;
                }
                else if (last)
                    definiteSubstring = selfAbstraction.upperBound.prefix.LastIndexOf(needleAbstraction.lowerBound.prefix);
                else
                    definiteSubstring = selfAbstraction.upperBound.prefix.IndexOf(needleAbstraction.lowerBound.prefix);

                if(definiteSubstring == -1)
                {
                    // There is no definite substring, may return -1, and up to the last possible index
                    return IndexInterval.For(IndexInt.Negative, LastPossibleIndex(selfAbstraction, needleAbstraction));
                }
                else if(last)
                {
                    // There is a definite substring. Try to find the last possible substring    
                    return IndexInterval.For(IndexInt.ForNonNegative(definiteSubstring), LastPossibleIndex(selfAbstraction, needleAbstraction));
                }
                else
                {
                    // There is a definite substring. Find first possible substring
                    return IndexInterval.For(FirstPossibleIndex(selfAbstraction, needleAbstraction), IndexInt.ForNonNegative(definiteSubstring));
                }
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
                return Contains(self, selfVariable, other, otherVariable, new NoOrderQuery<Variable>());
            }

            ///<inheritdoc/>
            public IStringPredicate Contains(WithConstants<PrefixInterval> self, Variable selfVariable, WithConstants<PrefixInterval> other, Variable otherVariable, IStringOrderQuery<Variable> orderQuery)
            {
                PrefixInterval selfInterval = self.ToAbstract(this);
                PrefixInterval otherInterval = other.ToAbstract(this);

                if (selfVariable != null && otherVariable != null && orderQuery.CheckMustBeLessEqualThan(selfVariable, otherVariable))
                {
                    // If other is prefix of self, then it is contained
                    return FlatPredicate.True;
                }
                else if (!otherInterval.lowerBound.IsBottom && selfInterval.upperBound.prefix.Contains(otherInterval.lowerBound.prefix))
                {
                    // The other string is a prefix of something which is in the known prefix.
                    return FlatPredicate.True;
                }
                else if (!selfInterval.lowerBound.IsBottom && !otherInterval.upperBound.IsTop)
                {
                    int index = selfInterval.lowerBound.prefix.IndexOf(otherInterval.upperBound.prefix);
                    int length = otherInterval.upperBound.prefix.Length;

                    if (index == -1)
                    {
                        // This string is a prefix of some string, but it does not contain a prefix of the other string
                        return FlatPredicate.False;
                    }
                    else if (selfVariable != null && index + length > self.ToAbstract(this).UpperBound.prefix.Length)
                    {
                        // If this string contains the other, then it must begin with a longer prefix 
                        return StringAbstractionPredicate.ForTrue(selfVariable, new PrefixInterval(selfInterval.lowerBound, Wrap(selfInterval.lowerBound.prefix.Substring(0, index + length))));
                    }
                }

                return FlatPredicate.Top;
            }
            ///<inheritdoc/>
            public IStringPredicate StartsEndsWithOrdinal(WithConstants<PrefixInterval> self, Variable selfVariable, WithConstants<PrefixInterval> other, Variable otherVariable, bool ends)
            {
                return StartsEndsWithOrdinal(self, selfVariable, other, otherVariable, ends, new NoOrderQuery<Variable>());
            }
            public IStringPredicate StartsEndsWithOrdinal(WithConstants<PrefixInterval> self, Variable selfVariable, WithConstants<PrefixInterval> other, Variable otherVariable, bool ends, IStringOrderQuery<Variable> orderQuery)
            {
                if (ends)
                    return prefixOperations.StartsEndsWithOrdinal(GetUpperBoundArg(self), selfVariable, GetUpperBoundArg(other), otherVariable, ends);


                if (selfVariable != null && otherVariable != null && orderQuery.CheckMustBeLessEqualThan(selfVariable, otherVariable))
                {
                    return FlatPredicate.True;
                }

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
            public IStringPredicate RegexIsMatch(PrefixInterval self, Variable selfVariable, Microsoft.Research.Regex.Model.Element regex)
            {
                PrefixRegex prefixRegexConverter = new PrefixRegex(self.upperBound);
                CodeAnalysis.ProofOutcome outcome = prefixRegexConverter.IsMatch(regex);

                if (outcome == CodeAnalysis.ProofOutcome.Top)
                {
                    Prefix regexPrefix = prefixRegexConverter.AssumeMatch(regex);
                    if (!regexPrefix.IsBottom && !regexPrefix.IsTop && selfVariable != null)
                    {
                        return StringAbstractionPredicate.ForTrue(selfVariable, ForUpperBound(regexPrefix));
                    }
                }

                return FlatPredicate.ForProofOutcome(outcome);
            }

            #endregion

            ///<inheritdoc/>
            public IEnumerable<Microsoft.Research.Regex.Model.Element> ToRegex(PrefixInterval self)
            {
                return new PrefixRegex(self.upperBound).GetRegexWithLowerBound(self.lowerBound);
            }

            ///<inheritdoc/>
            public PrefixInterval SetCharAt(PrefixInterval self, IndexInterval index, CharInterval value)
            {
                if (index.IsConstant && value.IsConstant && !self.lowerBound.IsBottom)
                {
                    int indexInt = index.LowerBound.AsInt;
                    string oldLower = self.lowerBound.prefix;

                    if (indexInt >= oldLower.Length)
                        // Definitely out of range
                        return Top.Bottom;

                    string newLower = oldLower.Substring(0, indexInt) + value.LowerBound + oldLower.Substring(indexInt + 1);
                    string newPrefix = newLower.Substring(0, Math.Max(indexInt + 1, self.upperBound.prefix.Length));
                    return For(newLower, newPrefix);
                }
                else
                {
                    return ForUpperBound(prefixOperations.SetCharAt(self.upperBound, index, value));
                }
            }

            ///<inheritdoc/>
            public CharInterval GetCharAt(PrefixInterval self, IndexInterval index)
            {
                if (self.lowerBound.IsBottom)
                {
                    return prefixOperations.GetCharAt(self.upperBound, index);
                }
                else if (index.LowerBound >= self.lowerBound.prefix.Length)
                {
                    //Lowest index is after the maximum length
                    return CharInterval.Unreached;
                }
                else
                {
                    CharInterval interval = CharInterval.Unreached;

                    for (int i = IndexInt.Max(index.LowerBound, IndexInt.For(0)).AsInt; i < self.lowerBound.prefix.Length && index.UpperBound >= i; ++i)
                    {
                        interval = interval.Join(CharInterval.For(self.lowerBound.prefix[i]));
                    }
                    return interval;
                }

            }
        }

        #endregion

        #region Object overrides
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
            {
                return string.Format("{0}[{1}]", upperBound.prefix, lowerBound.prefix.Substring(upperBound.prefix.Length));
            }
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
        #region IntervalBase overrides
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
        #endregion

        #region IStringInterval<PrefixInterval> implementation
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
        #endregion
    }

}
