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

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Elements of the prefix abstract domain for strings.
    /// Represents a set of strings starting with a specified prefix.
    /// </summary>
    public class Prefix : IStringAbstraction<Prefix>
    {
        /// <summary>
        /// The prefix which the represented strings start with.
        /// </summary>
        /// <remarks>
        /// Can be <see langword="null"/>, in which case represents no strings.
        /// </remarks>
        internal readonly string prefix;

        /// <summary>
        /// Constructs a prefix abstraction for the specified constant.
        /// </summary>
        /// <param name="constant">Constant string value.</param>
        public Prefix(string constant)
        {
            prefix = constant;
        }
        /// <summary>
        /// Constructs a new instance of prefix.
        /// </summary>
        /// <param name="p">The prefix that is copied.</param>
        public Prefix(Prefix p)
        {
            prefix = p.prefix;
        }

        #region Domain properties
        /// <summary>
        /// Gets the top element of the Prefix domain.
        /// </summary>
        public Prefix Top
        {
            get
            {
                return new Prefix("");
            }
        }

        /// <summary>
        /// Gets the bottom element of the Prefix domain.
        /// </summary>
        public Prefix Bottom
        {
            get
            {
                return new Prefix((string)null);
            }
        }
        ///<inheritdoc/>
        public bool IsTop
        {
            get { return prefix == ""; }
        }
        ///<inheritdoc/>
        public bool IsBottom
        {
            get { return prefix == null; }
        }
        ///<inheritdoc/>
        public bool ContainsValue(string value)
        {
            if (IsBottom)
                return false;
            return value.StartsWith(prefix, StringComparison.Ordinal);
        }
        ///<inheritdoc/>
        public bool Equals(Prefix other)
        {
            return prefix == other.prefix;
        }
        ///<inheritdoc/>
        public bool LessThanEqual(Prefix other)
        {
            return prefix == null || (other.prefix != null && prefix.StartsWith(other.prefix, StringComparison.Ordinal));
        }
        #endregion

        #region Domain operations
        ///<inheritdoc/>
        public Prefix Join(Prefix or)
        {
            if (IsBottom)
                return or;
            if (or.IsBottom)
                return this;

            return new Prefix(StringUtils.LongestCommonPrefix(prefix, or.prefix));
        }
        ///<inheritdoc/>
        public Prefix Meet(Prefix and)
        {
            if (IsBottom)
                return this;
            if (and.IsBottom)
                return and;

            if (prefix.StartsWith(and.prefix, StringComparison.Ordinal))
                return this;
            if (and.prefix.StartsWith(prefix, StringComparison.Ordinal))
                return and;

            return Bottom;
        }
        #endregion

        #region String operations

        /// <summary>
        /// Implements string operations for the Prefix abstract domain.
        /// </summary>
        /// <typeparam name="Variable">The type representing variables.</typeparam>
        public class Operations<Variable> : IStringOperations<Prefix, Variable>
          where Variable : class, IEquatable<Variable>
        {
            #region IStringAbstractionFactory implementation
            public Prefix Top
            {
                get { return new Prefix(""); }
            }

            public Prefix Constant(string v)
            {
                return new Prefix(v);
            }
            #endregion

            #region Operations returning strings
            ///<inheritdoc/>
            public Prefix Concat(WithConstants<Prefix> left, WithConstants<Prefix> right)
            {
                Prefix rightPrefix = right.ToAbstract(this);

                if (left.IsConstant)
                {
                    // only if there is constant on the left, we can use 
                    // information about the right part
                    return new Prefix(left.Constant + rightPrefix.prefix);
                }
                else
                {
                    return left.Abstract;
                }
            }
            ///<inheritdoc/>
            public Prefix Insert(WithConstants<Prefix> self, IndexInterval index, WithConstants<Prefix> other)
            {
                Prefix selfPrefix = self.ToAbstract(this);
                Prefix otherPrefix = other.ToAbstract(this);

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
                }
            }
            ///<inheritdoc/>
            public Prefix Replace(Prefix self, CharInterval from, CharInterval to)
            {

                if (from.IsConstant && to.IsConstant)
                    return new Prefix(self.prefix.Replace(from.LowerBound, to.LowerBound));

                int l = 0;

                while (l < self.prefix.Length && !from.Contains(self.prefix[l]))
                {
                    ++l;
                }

                return new Prefix(self.prefix.Substring(0, l));
            }
            ///<inheritdoc/>
            public Prefix Replace(WithConstants<Prefix> self, WithConstants<Prefix> from, WithConstants<Prefix> to)
            {
                Prefix selfPrefix = self.ToAbstract(this);

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
            }
            ///<inheritdoc/>
            public Prefix Substring(Prefix self, IndexInterval index, IndexInterval length)
            {
                if (index.IsFiniteConstant)
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
                }
            }
            ///<inheritdoc/>
            public Prefix Remove(Prefix self, IndexInterval index, IndexInterval length)
            {
                if (index.LowerBound.AsInt >= self.prefix.Length)
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
                }
            }
            ///<inheritdoc/>
            public Prefix PadLeftRight(Prefix self, IndexInterval length, CharInterval fill, bool right)
            {
                if (right || length.UpperBound.AsInt <= self.prefix.Length)
                    return self;
                else if (fill.IsConstant)
                    return new Prefix(StringUtils.LongestConstantPrefix(self.prefix, fill.LowerBound));
                else
                    return Top;
            }

            ///<inheritdoc/>
            public Prefix Trim(WithConstants<Prefix> self, WithConstants<Prefix> trimmed)
            {
                if (trimmed.IsConstant && trimmed.Constant != "")
                {
                    Prefix selfPrefix = self.ToAbstract(this);
                    return new Prefix(selfPrefix.prefix.Trim(trimmed.Constant.ToCharArray()));
                }
                else
                {
                    return Top;
                }
            }
            ///<inheritdoc/>
            public Prefix TrimStartEnd(WithConstants<Prefix> self, WithConstants<Prefix> trimmed, bool end)
            {
                if (trimmed.IsConstant && trimmed.Constant != "")
                {
                    Prefix selfPrefix = self.ToAbstract(this);

                    string trimmedPrefix;
                    if (end) trimmedPrefix = selfPrefix.prefix.TrimEnd(trimmed.Constant.ToCharArray());
                    else
                        trimmedPrefix = selfPrefix.prefix.TrimStart(trimmed.Constant.ToCharArray());

                    return new Prefix(trimmedPrefix);
                }
                else
                {
                    return Top;
                }
            }

            #endregion
            #region Operations returning integers

            private CompareResult CompareOrdinal(WithConstants<Prefix> self, Prefix other)
            {
                Prefix selfPrefix = self.ToAbstract(this);

                int comparison = string.CompareOrdinal(selfPrefix.prefix, other.prefix);

                if (self.IsConstant && comparison == 0)
                {
                    return CompareResult.LessEqual;
                }

                bool selfStartsWithOther = selfPrefix.prefix.StartsWith(other.prefix, StringComparison.Ordinal);

                if (selfStartsWithOther || (!self.IsConstant && other.prefix.StartsWith(selfPrefix.prefix, StringComparison.Ordinal)))
                {
                    return CompareResult.Top;
                }
                else if (comparison < 0)
                {
                    return CompareResult.Less;
                }
                else
                {
                    return CompareResult.Greater;
                }

            }
            ///<inheritdoc/>
            public CompareResult CompareOrdinal(
              WithConstants<Prefix> self,
              WithConstants<Prefix> other)
            {
                if (other.IsConstant)
                {
                    return CompareOrdinal(other, self.ToAbstract(this)).SwapSides();
                }
                else
                {
                    return CompareOrdinal(self, other.Abstract);
                }

            }
            ///<inheritdoc/>
            public IndexInterval GetLength(Prefix self)
            {
                // The value cannot be shorter, but can be longer
                return IndexInterval.For(IndexInt.For(self.prefix.Length), IndexInt.Infinity);
            }
            ///<inheritdoc/>
            public IndexInterval IndexOf(WithConstants<Prefix> self, WithConstants<Prefix> needle, IndexInterval offset, IndexInterval count, bool last)
            {
                if (!offset.IsFiniteConstant || offset.LowerBound != 0 || !count.IsInfinity)
                {
                    // Offset and count are not supported
                    return IndexInterval.Unknown;
                }

                Prefix selfAbstraction = self.ToAbstract(this);

                if (needle.IsConstant)
                {
                    int index;
                    if (last)
                        index = selfAbstraction.prefix.LastIndexOf(needle.Constant, StringComparison.Ordinal);
                    else
                        index = selfAbstraction.prefix.IndexOf(needle.Constant, StringComparison.Ordinal);

                    if (index >= 0)
                    {
                        if (last)
                            return IndexInterval.For(IndexInt.ForNonNegative(index), IndexInt.Infinity);
                        else
                            return IndexInterval.For(index);
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
            }
            #endregion

            #region Predicate operations
            ///<inheritdoc/>
            public IStringPredicate IsEmpty(Prefix self, Variable selfVariable)
            {
                if (self.IsBottom)
                    return FlatPredicate.Bottom;
                if (self.IsTop)
                    return FlatPredicate.Top;
                return FlatPredicate.False;
            }
            ///<inheritdoc/>
            public IStringPredicate Contains(WithConstants<Prefix> self, Variable selfVariable, WithConstants<Prefix> other, Variable otherVariable)
            {
                Prefix selfPrefix = self.ToAbstract(this);
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
                }
            }
            ///<inheritdoc/>
            public IStringPredicate StartsEndsWithOrdinal(WithConstants<Prefix> self, Variable selfVariable, WithConstants<Prefix> other, Variable otherVariable, bool ends)
            {
                Prefix selfPrefix = self.ToAbstract(this);
                Prefix otherPrefix = other.ToAbstract(this);

                if (ends)
                {
                    if (other.IsConstant && other.Constant == "")
                    {
                        // All strings end with an empty string
                        return FlatPredicate.True;
                    }
                    else if (self.IsConstant && !self.Constant.Contains(otherPrefix.prefix))
                    {
                        // A constant must contain all prefixes of a suffix.
                        return FlatPredicate.False;
                    }
                    else
                    {
                        return FlatPredicate.Top;
                    }
                }
                else
                {
                    if (selfPrefix.prefix.StartsWith(otherPrefix.prefix, StringComparison.Ordinal))
                    {
                        if (other.IsConstant)
                        {
                            return FlatPredicate.True;
                        }
                    }
                    else if (self.IsConstant || !otherPrefix.prefix.StartsWith(selfPrefix.prefix, StringComparison.Ordinal))
                    {
                        return FlatPredicate.False;
                    }

                    if (selfVariable != null)
                    {
                        return StringAbstractionPredicate.ForTrue(selfVariable, otherPrefix);
                    }
                    else
                    {
                        return FlatPredicate.Top;
                    }
                }
            }

            ///<inheritdoc/>
            public IStringPredicate Equals(WithConstants<Prefix> self, Variable selfVariable,
              WithConstants<Prefix> other, Variable otherVariable)
            {
                Prefix selfPrefix = self.ToAbstract(this);
                Prefix otherPrefix = other.ToAbstract(this);

                bool canBeEqual = StringUtils.CanBeEqualPrefix(selfPrefix.prefix, otherPrefix.prefix);

                if (canBeEqual && selfVariable != null)
                {
                    return StringAbstractionPredicate.ForTrue(selfVariable, otherPrefix);
                }
                else if (canBeEqual && otherVariable != null)
                {
                    return StringAbstractionPredicate.ForTrue(otherVariable, selfPrefix);
                }
                else
                {
                    return new FlatPredicate(canBeEqual, true);
                }
            }
            ///<inheritdoc/>
            public IStringPredicate RegexIsMatch(Prefix self, Variable selfVariable, Microsoft.Research.Regex.Model.Element regex)
            {
                
                PrefixRegex prefixRegexConverter = new PrefixRegex(self);
                CodeAnalysis.ProofOutcome outcome = prefixRegexConverter.IsMatch(regex);

                if (outcome == CodeAnalysis.ProofOutcome.Top)
                {
                    Prefix regexPrefix = prefixRegexConverter.AssumeMatch(regex);
                    if (!regexPrefix.IsBottom && !regexPrefix.IsTop && selfVariable != null)
                    {
                        return StringAbstractionPredicate.ForTrue(selfVariable, regexPrefix);
                    }
                }

                return FlatPredicate.ForProofOutcome(outcome);
            }

            #endregion

            ///<inheritdoc/>
            public Prefix SetCharAt(Prefix self, IndexInterval index, CharInterval value)
            {
                if (index.LowerBound > self.prefix.Length)
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
            }

            ///<inheritdoc/>
            public CharInterval GetCharAt(Prefix self, IndexInterval index)
            {
                if (index.UpperBound >= self.prefix.Length)
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
                }
            }
        }

        #endregion

        #region Object
        public override string ToString()
        {
            if (IsBottom)
                return "_|_";
            else
                return prefix + "*";
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Prefix);
        }

        public override int GetHashCode()
        {
            if (prefix == null)
                return 0;
            return prefix.GetHashCode();
        }
        #endregion
        ///<inheritdoc/>
        public Prefix Constant(string cst)
        {
            return new Prefix(cst);
        }

        #region IAbstractDomain implementation
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
            return LessThanEqual(a as Prefix);
        }

        IAbstractDomain IAbstractDomain.Bottom
        {
            get { return new Prefix((string)null); }
        }

        IAbstractDomain IAbstractDomain.Top
        {
            get { return new Prefix(""); }
        }

        public IAbstractDomain Join(IAbstractDomain a)
        {
            return Join(a as Prefix);
        }

        public IAbstractDomain Meet(IAbstractDomain a)
        {
            return Meet(a as Prefix);
        }

        public IAbstractDomain Widening(IAbstractDomain prev)
        {
            return Join(prev as Prefix);
        }

        public object Clone()
        {
            return new Prefix(this);
        }
        public T To<T>(IFactory<T> factory)
        {
            return factory.Constant(true);
        }
        #endregion
    }

}
