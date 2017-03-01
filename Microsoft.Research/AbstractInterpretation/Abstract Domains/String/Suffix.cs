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

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Elements of the suffix abstract domain for strings.
    /// Represents a set of strings ending with a specified suffix.
    /// </summary>
    public class Suffix : IEquatable<Suffix>, IStringAbstraction<Suffix, string>
    {
        /// <summary>
        /// The suffix which the represented strings end with.
        /// </summary>
        /// <remarks>
        /// Can be <see langword="null"/>, in which case represents no strings.
        /// </remarks>
        internal readonly string suffix;

        /// <summary>
        /// Creates a suffix element from the specified suffix.
        /// </summary>
        /// <param name="suffix">The suffix the represented strings end with.</param>
        public Suffix(string suffix)
        {
            this.suffix = suffix;
        }
        /// <summary>
        /// Creates a new instance of the suffix element.
        /// </summary>
        /// <param name="original">The original element.</param>
        public Suffix(Suffix original)
        {
            suffix = original.suffix;
        }

        #region Domain properties

        /// <summary>
        /// Gets the top element.
        /// </summary>
        public Suffix Top
        {
            get
            {
                return new Suffix("");
            }
        }

        /// <summary>
        /// Gets the bottom element.
        /// </summary>
        public Suffix Bottom
        {
            get
            {
                return new Suffix((string)null);
            }
        }
        /// <summary>
        /// Determines whether this element is a top element.
        /// </summary>
        public bool IsTop
        {
            get { return suffix == ""; }
        }
        /// <summary>
        /// Determines whether this element is a bottom element.
        /// </summary>
        public bool IsBottom
        {
            get { return suffix == null; }
        }
        /// <summary>
        /// Determines whether the specified string value is represented by this element.
        /// </summary>
        /// <param name="value">A string value.</param>
        /// <returns><see langword="true"/>, if <paramref name="value"/> ends with the represented suffix.</returns>
        public bool ContainsValue(string value)
        {
            if (IsBottom)
                return false;
            return value.EndsWith(suffix, StringComparison.Ordinal);
        }
        /// <summary>
        /// Determines whether this suffix element is the same element as another suffix element.
        /// </summary>
        /// <param name="other">Another suffix element to be compared to.</param>
        /// <returns><see langword="true"/>, if the two suffix elements are the same.</returns>
        public bool Equals(Suffix other)
        {
            return suffix == other.suffix;
        }
        /// <summary>
        /// Determines whether this suffix element is less than or equal to another suffix element in the abstract domain lattice.
        /// </summary>
        /// <param name="other">Another suffix element to be compared to.</param>
        /// <returns><see langword="true"/>, if the this element is less than or equal to <paramref name="other"/>.</returns>
        public bool LessThanEqual(Suffix other)
        {
            if (suffix == null)
            {
                return true;
            }
            else if (other.suffix == null)
            {
                return false;
            }
            else
            {
                return suffix.EndsWith(other.suffix, StringComparison.Ordinal);
            }
        }
        #endregion

        #region Domain operations
        /// <summary>
        /// Joins two suffix elements.
        /// </summary>
        /// <remarks>
        /// The join of two suffix elements is their longest common suffix.
        /// </remarks>
        /// <param name="other">Another suffix element to join with.</param>
        /// <returns>The join of this and <paramref name="other"/>.</returns>
        public Suffix Join(Suffix other)
        {
            if (IsBottom)
                return other;
            if (other.IsBottom)
                return this;

            return new Suffix(StringUtils.LongestCommonSuffix(suffix, other.suffix));
        }
        ///<inheritdoc/>
        public Suffix Meet(Suffix and)
        {
            if (IsBottom)
            {
                return this;
            }
            if (and.IsBottom)
            {
                return and;
            }

            if (suffix.EndsWith(and.suffix, StringComparison.Ordinal))
            {
                return this;
            }
            if (and.suffix.EndsWith(suffix, StringComparison.Ordinal))
            {
                return and;
            }

            return Bottom;
        }
        #endregion

        /// <summary>
        /// Implements string operations for the Suffix abstract domain.
        /// </summary>
        /// <typeparam name="Variable">The type representing variables.</typeparam>
        public class Operations<Variable> : IStringOperations<Suffix, Variable>
          where Variable : class, IEquatable<Variable>
        {

            #region Operation returning strings
            ///<inheritdoc/>
            public Suffix Concat(WithConstants<Suffix> left, WithConstants<Suffix> right)
            {
                Suffix leftSuffix = left.ToAbstract(this);

                if (right.IsConstant)
                {
                    return new Suffix(leftSuffix.suffix + right.Constant);
                }
                else
                {
                    return right.Abstract;
                }
            }
            ///<inheritdoc/>
            public Suffix Insert(WithConstants<Suffix> self, IndexInterval index, WithConstants<Suffix> other)
            {
                Suffix selfSuffix = self.ToAbstract(this);
                Suffix otherSuffix = other.ToAbstract(this);

                if (self.IsConstant)
                {
                    // Inserting into a constant
                    int indexLowerInt = index.LowerBound.AsInt;

                    if (indexLowerInt > selfSuffix.suffix.Length)
                    {
                        // Index out of bounds
                        return selfSuffix.Bottom;
                    }
                    else if (index.IsConstant)
                    {
                        // Inserting into a constant at a constant index
                        return new Suffix(otherSuffix.suffix + self.Constant.Substring(indexLowerInt));
                    }
                }

                // The last possible index where insert can happen
                int indexInt = index.UpperBound > selfSuffix.suffix.Length ? selfSuffix.suffix.Length : index.UpperBound.AsInt;
                bool otherConstant = other.IsConstant;

                if (indexInt == 0 || (otherConstant && other.Constant == ""))
                {
                    // Inserting an empty string does no change
                    // Inserting at the beginning does not change the suffix
                    return selfSuffix;
                }
                else
                {
                    int otherLength = otherSuffix.suffix.Length;

                    if (otherLength > 0)
                    {
                        char constantChar = selfSuffix.suffix[indexInt - 1];
                        int common;
                        for (common = 0; common < indexInt; ++common)
                        {
                            if (selfSuffix.suffix[indexInt - common - 1] != constantChar)
                                break;
                            if (common < otherLength)
                            {
                                if (otherSuffix.suffix[otherLength - common - 1] != constantChar)
                                    break;
                            }
                            else if (!otherConstant)
                            {
                                break;
                            }
                        }
                        indexInt -= common;
                    }

                    string post = selfSuffix.suffix.Substring(indexInt);
                    return new Suffix(post);
                }
            }
            ///<inheritdoc/>
            public Suffix Replace(Suffix self, CharInterval from, CharInterval to)
            {

                if (from.IsConstant && to.IsConstant)
                {
                    // Both characters are known, so we replace them in the suffix
                    return new Suffix(self.suffix.Replace(from.LowerBound, to.LowerBound));
                }
                else
                {
                    // We cut off the first character from end, that can change
                    int l = self.suffix.Length - 1;
                    while (l >= 0 && !from.Contains(self.suffix[l]))
                    {
                        --l;
                    }
                    return new Suffix(self.suffix.Substring(l + 1));
                }
            }
            ///<inheritdoc/>
            public Suffix Replace(WithConstants<Suffix> self, WithConstants<Suffix> from, WithConstants<Suffix> to)
            {
                return Top;
            }

            ///<inheritdoc/>
            public Suffix Substring(Suffix self, IndexInterval index, IndexInterval length)
            {
                if (length.LowerBound.IsInfinite)
                {
                    if (index.UpperBound > self.suffix.Length)
                    {
                        return Top;
                    }
                    else
                    {
                        return new Suffix(self.suffix.Substring(index.UpperBound.AsInt));
                    }
                }
                else
                {
                    // Finite substring can be anywhere in the string
                    return Top;
                }
            }

            ///<inheritdoc/>
            public Suffix Remove(Suffix self, IndexInterval index, IndexInterval length)
            {
                if (length.IsFiniteConstant)
                {
                    int l = length.LowerBound.AsInt;

                    if (l == 0)
                    {
                        // optimization for l == 0
                        return self;
                    }
                    else if (l >= self.suffix.Length)
                    {
                        return self.Top;
                    }

                    int i = IndexInt.Min(index.UpperBound, self.suffix.Length - l);

                    string common = self.suffix.Substring(i + l);
                    string before = self.suffix.Substring(0, i);
                    string after = self.suffix.Substring(0, i + l);

                    return new Suffix(StringUtils.LongestCommonSuffix(before, after) + common);
                }
                else
                {
                    // Remove until end - always Top
                    // Remove unknown length
                    return self.Top;
                }
            }

            ///<inheritdoc/>
            public Suffix PadLeftRight(Suffix self, IndexInterval length, CharInterval fill, bool right)
            {
                if (!right)
                    return self;

                if (length.IsConstant && fill.IsConstant)
                {
                    int len = length.LowerBound.AsInt;

                    if (len <= self.suffix.Length)
                    {
                        return self;
                    }
                    else
                    {
                        return new Suffix(StringUtils.LongestConstantSuffix(self.suffix, fill.LowerBound));
                    }
                }
                else
                {
                    return self.Top;
                }
            }

            ///<inheritdoc/>
            public Suffix Trim(WithConstants<Suffix> self, WithConstants<Suffix> trimmed)
            {
                if (trimmed.IsConstant && trimmed.Constant != "")
                {
                    Suffix selfSuffix = self.ToAbstract(this);
                    return new Suffix(selfSuffix.suffix.Trim(trimmed.Constant.ToCharArray()));
                }
                else
                {
                    return Top;
                }
            }

            ///<inheritdoc/>
            public Suffix TrimStartEnd(WithConstants<Suffix> self, WithConstants<Suffix> trimmed, bool end)
            {
                if (trimmed.IsConstant && trimmed.Constant != "")
                {
                    Suffix selfSuffix = self.ToAbstract(this);
                    if(end)
                        return new Suffix(selfSuffix.suffix.TrimEnd(trimmed.Constant.ToCharArray()));
                    else
                        return new Suffix(selfSuffix.suffix.TrimStart(trimmed.Constant.ToCharArray()));
                }
                else
                {
                    return Top;
                }
            }
            #endregion
            #region Operations returning bool
            ///<inheritdoc/>
            public IStringPredicate IsEmpty(Suffix self, Variable selfVariable)
            {
                if (self.IsTop)
                {
                    return FlatPredicate.Top;
                }
                return FlatPredicate.False;
            }
            ///<inheritdoc/>
            public IStringPredicate Contains(WithConstants<Suffix> self, Variable selfVariable, WithConstants<Suffix> other, Variable otherVariable)
            {
                Suffix selfSuffix = self.ToAbstract(this);
                Suffix otherSuffix = other.ToAbstract(this);

                if (other.IsConstant && selfSuffix.suffix.Contains(other.Constant))
                {
                    return FlatPredicate.True;
                }
                else if (self.IsConstant && !self.Constant.Contains(otherSuffix.suffix))
                {
                    return FlatPredicate.False;
                }
                else
                {
                    return FlatPredicate.Top;
                }
            }
            ///<inheritdoc/>
            public IStringPredicate StartsEndsWithOrdinal(WithConstants<Suffix> self, Variable selfVariable, WithConstants<Suffix> other, Variable otherVariable, bool ends)
            {
                Suffix selfSuffix = self.ToAbstract(this);
                Suffix otherSuffix = other.ToAbstract(this);

                if (ends)
                {
                    if (other.IsConstant && selfSuffix.suffix.EndsWith(other.Constant, StringComparison.Ordinal))
                    {
                        return FlatPredicate.True;
                    }

                    if (!self.IsConstant && otherSuffix.suffix.EndsWith(selfSuffix.suffix, StringComparison.Ordinal))
                    {
                        if (selfVariable != null)
                        {
                            return StringAbstractionPredicate.ForTrue(selfVariable, otherSuffix);
                        }
                        else
                        {
                            return FlatPredicate.Top;
                        }
                    }

                    if (other.IsConstant || !selfSuffix.suffix.EndsWith(otherSuffix.suffix, StringComparison.Ordinal))
                    {
                        return FlatPredicate.False;
                    }
                }
                else
                {
                    if (other.IsConstant && other.Constant == "")
                    {
                        return FlatPredicate.True;
                    }
                    else if (self.IsConstant && !self.Constant.Contains(otherSuffix.suffix))
                    {
                        return FlatPredicate.False;
                    }
                }

                return FlatPredicate.Top;
            }
            ///<inheritdoc/>
            public IStringPredicate Equals(WithConstants<Suffix> self, Variable selfVariable, WithConstants<Suffix> other, Variable otherVariable)
            {
                Suffix selfSuffix = self.ToAbstract(this);
                Suffix otherSuffix = other.ToAbstract(this);

                bool canBeEqual = selfSuffix.suffix.EndsWith(otherSuffix.suffix, StringComparison.Ordinal);
                canBeEqual |= otherSuffix.suffix.EndsWith(selfSuffix.suffix, StringComparison.Ordinal);

                return new FlatPredicate(canBeEqual, true);
            }
            #endregion
            #region String operations returning integers
            ///<inheritdoc/>
            public CompareResult CompareOrdinal(WithConstants<Suffix> self, WithConstants<Suffix> other)
            {
                Suffix selfSuffix = self.ToAbstract(this);
                Suffix otherSuffix = other.ToAbstract(this);

                bool canBeEqual = selfSuffix.suffix.EndsWith(otherSuffix.suffix, StringComparison.Ordinal);
                canBeEqual |= otherSuffix.suffix.EndsWith(selfSuffix.suffix, StringComparison.Ordinal);

                return canBeEqual ? CompareResult.Top : CompareResult.NotEqual;
            }
            ///<inheritdoc/>
            public IndexInterval GetLength(Suffix self)
            {
                // The value cannot be shorter, but can be longer
                return IndexInterval.For(IndexInt.For(self.suffix.Length), IndexInt.Infinity);
            }

            ///<inheritdoc/>
            public IndexInterval IndexOf(WithConstants<Suffix> self, WithConstants<Suffix> needle, IndexInterval offset, IndexInterval count, bool last)
            {
                if (!offset.IsFiniteConstant || offset.LowerBound != 0 || !count.IsInfinity)
                {
                    // Offset and count are not supported
                    return IndexInterval.Unknown;
                }

                if (needle.IsConstant)
                {
                    if (last)
                    {
                        int index = self.ToAbstract(this).suffix.LastIndexOf(needle.Constant, StringComparison.Ordinal);
                        if (index >= 0)
                        {
                            return IndexInterval.For(IndexInt.For(index), IndexInt.Infinity);
                        }
                    }
                    else {
                        if (needle.Constant == "")
                        {
                            return IndexInterval.For(0);
                        }
                        else if (self.ToAbstract(this).suffix.Contains(needle.Constant))
                        {
                            return IndexInterval.For(IndexInt.For(0), IndexInt.Infinity);
                        }
                    }
                }
                return IndexInterval.Unknown;
            }
            #endregion
            ///<inheritdoc/>
            public IStringPredicate RegexIsMatch(Suffix self, Variable selfVariable, Microsoft.Research.Regex.Model.Element regex)
            {
                SuffixRegex suffixRegexConverter = new SuffixRegex(self);

                CodeAnalysis.ProofOutcome outcome = suffixRegexConverter.IsMatch(regex);

                if (outcome == CodeAnalysis.ProofOutcome.Top)
                {
                    Suffix regexSuffix = suffixRegexConverter.AssumeMatch(regex);
                    if (!regexSuffix.IsBottom && !regexSuffix.IsTop)
                    {
                        return StringAbstractionPredicate.ForTrue(selfVariable, regexSuffix);
                    }
                }

                return FlatPredicate.ForProofOutcome(outcome);
            }
            ///<inheritdoc/>
            public Suffix Top
            {
                get { return new Suffix(""); }
            }
            ///<inheritdoc/>
            public Suffix Constant(string v)
            {
                return new Suffix(v);
            }
            ///<inheritdoc/>
            public Suffix SetCharAt(Suffix self, IndexInterval index, CharInterval value)
            {

                if (index.UpperBound < self.suffix.Length)
                {
                    //If the index is low enough, we may retain part of the suffix
                    return new Suffix(self.suffix.Substring(index.UpperBound.AsInt));
                }
                else
                {
                    return Top;
                }
            }
            ///<inheritdoc/>
            public CharInterval GetCharAt(Suffix self, IndexInterval index)
            {
                // For every valid index, we can construct string with that suffix
                // with any character at that index
                return CharInterval.Unknown;
            }
        }

        #region Object
        public override string ToString()
        {
            if (IsBottom)
                return "_|_";
            else
                return "*" + suffix;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Suffix);
        }

        public override int GetHashCode()
        {
            if (suffix == null)
                return 0;
            return suffix.GetHashCode();
        }
        #endregion

        ///<inheritdoc/>
        public Suffix Constant(string constant)
        {
            return new Suffix(constant);
        }

        #region IAbstractDomain
        bool IAbstractDomain.IsBottom
        {
            get { return IsBottom; }
        }

        bool IAbstractDomain.IsTop
        {
            get { return IsTop; }
        }

        bool IAbstractDomain.LessEqual(IAbstractDomain a)
        {
            return LessThanEqual(a as Suffix);
        }

        IAbstractDomain IAbstractDomain.Bottom
        {
            get { return new Suffix((string)null); }
        }

        IAbstractDomain IAbstractDomain.Top
        {
            get { return new Suffix(""); }
        }

        public IAbstractDomain Join(IAbstractDomain a)
        {
            return Join(a as Suffix);
        }

        public IAbstractDomain Meet(IAbstractDomain a)
        {
            return Meet(a as Suffix);
        }

        public IAbstractDomain Widening(IAbstractDomain prev)
        {
            return Join(prev as Suffix);
        }

        public object Clone()
        {
            return new Suffix(this);
        }
        public T To<T>(IFactory<T> factory)
        {
            return factory.Constant(true);
        }
        #endregion
    }

}
