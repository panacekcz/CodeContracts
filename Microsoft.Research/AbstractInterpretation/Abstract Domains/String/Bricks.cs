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

using Microsoft.Research.CodeAnalysis;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Represents an element of the Bricks abstract domain.
    /// </summary>
    public class Bricks : IStringAbstraction<Bricks, string>, IEquatable<Bricks>
    {
        #region Element state
        /// <summary>
        /// The list of bricks.
        /// </summary>
        /// <remarks>
        /// Must not be <see langword="null"/>.
        /// </remarks>
        internal readonly List<Brick> bricks;

        private readonly IBricksPolicy policy;
        #endregion
        #region Construction
        /// <summary>
        /// Creates a brick list representing a single string constant.
        /// </summary>
        /// <param name="constant">The constant represented by the bricks.</param>
        /// <param name="policy">The policy used in domain operators.</param>
        public Bricks(string constant, IBricksPolicy policy)
        {
            this.policy = policy;
            if (constant == "")
            {
                bricks = new List<Brick>();
            }
            else
            {
                bricks = new List<Brick> { new Brick(constant) };
            }
        }
        /// <summary>
        /// Creates a top or bottom bricks element.
        /// </summary>
        /// <param name="top">Whether the bricks are top or bottom.</param>
        /// <param name="policy">The policy used in domain operators.</param>
        public Bricks(bool top, IBricksPolicy policy)
        {
            this.policy = policy;
            bricks = new List<Brick> { new Brick(top) };
        }
        internal Bricks(List<Brick> list, IBricksPolicy policy)
        {
            this.policy = policy;
            this.bricks = list;
        }
        internal Bricks(Bricks source)
          : this(new List<Brick>(source.bricks), source.policy)
        {
        }

        #endregion

        #region Helpers
        private IndexInt MinLength
        {
            get
            {
                return IndexInt.Sum(bricks, br => br.MinLength);
            }
        }
        private IndexInt MaxLength
        {
            get
            {
                return IndexInt.Sum(bricks, br => br.MaxLength);
            }
        }

        internal Bricks Zip(Bricks other, Func<Brick, Brick, Brick> zipper)
        {
            Bricks self = this;
            if (bricks.Count > other.bricks.Count)
            {
                other = policy.Extend(other, self);
            }
            else if (other.bricks.Count > bricks.Count)
            {
                self = policy.Extend(self, other);
            }

            return new Bricks(self.bricks.Zip(other.bricks, zipper).ToList(), policy);
        }

        internal Bricks Normalize(BrickNormalizationLocation location)
        {
            return policy.Normalize(this, location);
        }
        /// <summary>
        /// Tries to get a contant.
        /// </summary>
        /// <returns>Represented constant or <see langword="null"/>.</returns>
        private string ToConstant()
        {
            StringBuilder builder = new StringBuilder();

            foreach (Brick brick in bricks)
            {
                string constant = brick.ToConstant();
                if (constant == null)
                    return null;
                else
                    builder.Append(constant);
            }
            return builder.ToString();
        }

        private string ToPrefix()
        {
            StringBuilder builder = new StringBuilder();
            foreach (Brick brick in bricks)
            {
                string constant = brick.ToConstant();
                if (constant == null)
                {
                    builder.Append(brick.ToPrefix());
                    break;
                }
                else
                {
                    builder.Append(constant);
                }
            }
            return builder.ToString();
        }

        private string ToSuffix()
        {
            List<string> parts = new List<string>();
            foreach (Brick brick in Enumerable.Reverse(bricks))
            {
                string constant = brick.ToConstant();
                if (constant == null)
                {
                    parts.Add(brick.ToSuffix());
                    break;
                }
                else
                {
                    parts.Add(constant);
                }
            }
            return StringBuilderUtils.BuildStringFromReverseList(parts);
        }

        #endregion

        #region Domain properties
        ///<inheritdoc/>
        public Bricks Top
        {
            get
            {
                return new Bricks(true, policy);
            }
        }
        ///<inheritdoc/>
        public Bricks Bottom
        {
            get
            {
                return new Bricks(false, policy);
            }
        }
        ///<inheritdoc/>
        public bool IsTop
        {
            get
            {
                return bricks.Count >= 1 && bricks.TrueForAll(br => br.IsTop);
            }
        }
        ///<inheritdoc/>
        public bool IsBottom
        {
            get
            {
                return bricks.Exists(br => br.IsBottom);
            }
        }
        ///<inheritdoc/>
        public bool ContainsValue(string value)
        {
            if (value == "")
            {
                return MinLength == 0;
            }
            // Overapproximation
            return true;
        }
        ///<inheritdoc/>
        public bool Equals(Bricks other)
        {
            return bricks.Count == other.bricks.Count && bricks.Zip(other.bricks, (a, b) => a == b).All(x => x);
        }
        ///<inheritdoc/>
        public bool LessThanEqual(Bricks other)
        {
            if (other.IsTop || IsBottom)
            {
                return true;
            }
            else if (IsTop || other.IsBottom)
            {
                return false;
            }
            else if (other.Equals(this))
            {
                return true;
            }

            int thisIndex = 0, otherIndex = 0;

            while (thisIndex < this.bricks.Count && otherIndex < other.bricks.Count)
            {
                if (other.bricks[otherIndex].IsTop)
                {
                    ++thisIndex;
                }
                else if (bricks[thisIndex].LessThanEqual(other.bricks[otherIndex]))
                {
                    ++thisIndex;
                    ++otherIndex;
                }
                else
                {
                    // underapproximation
                    return false;
                }
            }

            if (thisIndex < this.bricks.Count)
            {
                return Enumerable.Range(thisIndex, this.bricks.Count - thisIndex).All(index => this.bricks[index].MustBeEmpty);
            }
            else if (otherIndex < other.bricks.Count)
            {
                return Enumerable.Range(otherIndex, other.bricks.Count - otherIndex).All(index => other.bricks[index].CanBeEmpty);
            }
            else
            {
                return true;
            }

        }
        #endregion
        #region Domain operations
        public Bricks Join(Bricks other)
        {
            Bricks result = Zip(other, (a, b) => a.Join(b));

            return result.Normalize(BrickNormalizationLocation.Join);
        }

        private bool TryLeftDerivation(Brick self, Brick left, out Brick right)
        {
            if (self.IsTop)
            {
                right = self;
                return true;
            }

            if (left.min == 1 && left.max == 1 && left.values != null)
            {
                HashSet<string> rightValues = new HashSet<string>();
                foreach (string value in self.values)
                {
                    foreach (string prefix in left.values)
                    {
                        if (prefix.Length > value.Length)
                        {
                            right = null;
                            return false;
                        }

                        if (value.StartsWith(prefix, StringComparison.Ordinal))
                        {
                            rightValues.Add(value.Substring(prefix.Length));
                        }
                    }
                }

                if (self.max > 1)
                {
                    rightValues.UnionWith(self.values);
                }

                right = new Brick(rightValues, self.min, self.max);
                return true;
            }

            right = null;
            return false;
        }

        public Bricks Meet(Bricks other)
        {
            // Trivial cases
            if (this.IsBottom || other.IsTop)
                return this;
            if (this.IsTop || other.IsBottom)
                return other;
            if (this.Equals(other))
                return this;

            // Traverse the lists from the front
            List<Brick> otherList = new List<Brick>(other.bricks);
            List<Brick> thisList = new List<Brick>(this.bricks);

            int thisIndex = 0;
            int otherIndex = 0;

            List<Brick> result = new List<Brick>();

            while (thisIndex < thisList.Count && otherIndex < otherList.Count)
            {
                Brick thisFront = thisList[thisIndex];
                Brick otherFront = otherList[otherIndex];

                bool thisWholeBrick = (thisIndex == thisList.Count - 1) || !thisList[thisIndex + 1].CanOverlap(otherFront);
                bool otherWholeBrick = (otherIndex == otherList.Count - 1) || !otherList[otherIndex + 1].CanOverlap(thisFront);

                Brick derived;

                if (thisWholeBrick && otherWholeBrick)
                {
                    derived = thisFront.Meet(otherFront);

                    if (derived.IsBottom)
                    {
                        return Bottom;
                    }

                    result.Add(derived);

                    thisIndex++;
                    otherIndex++;
                }
                else if (TryLeftDerivation(otherFront, thisFront, out derived))
                {
                    if (derived.IsBottom)
                    {
                        return Bottom;
                    }
                    else if (!derived.MustBeEmpty)
                    {
                        otherList[otherIndex] = derived;
                    }
                    else
                    {
                        ++otherIndex;
                    }
                    result.Add(thisFront);
                    ++thisIndex;
                }
                else if (TryLeftDerivation(thisFront, otherFront, out derived))
                {
                    if (derived.IsBottom)
                    {
                        return Bottom;
                    }
                    else if (!derived.MustBeEmpty)
                    {
                        thisList[thisIndex] = derived;
                    }
                    else
                    {
                        ++thisIndex;
                    }
                    result.Add(otherFront);
                    ++otherIndex;
                }
                else
                {
                    // The simple cases do not work, add the rest of the biricks
                    while (thisIndex < thisList.Count)
                    {
                        result.Add(thisList[thisIndex]);
                        ++thisIndex;
                    }
                }

            }

            return new Bricks(result, policy);
        }
        #endregion

        public IBricksPolicy Policy
        {
            get { return policy; }
        }

        /// <summary>
        /// Implements string operations for the Bricks abstract domain.
        /// </summary>
        /// <typeparam name="Variable">The type representing variables.</typeparam>
        public class Operations<Variable> : IStringOperations<Bricks, Variable>
          where Variable : class, IEquatable<Variable>
        {
            private IBricksPolicy policy;

            /// <summary>
            /// Constructs the operations implementation object.
            /// </summary>
            /// <param name="policy">The policy used by the bricks.</param>
            public Operations(IBricksPolicy policy)
            {
                this.policy = policy;
            }

            #region Helper methods
            /// <summary>
            /// Concatenates two lists of bricks.
            /// </summary>
            /// <param name="left">The left list.</param>
            /// <param name="right">The right list.</param>
            /// <returns>A list containing <paramref name="left"/> bricks followed
            /// by <paramref name="right"/> bricks.</returns>
            private static List<Brick> ConcatLists(List<Brick> left, List<Brick> right)
            {
                List<Brick> br = new List<Brick>(left);
                br.AddRange(right);
                return br;
            }
            private static List<Brick> Prepend(Brick left, List<Brick> right)
            {
                List<Brick> br = new List<Brick> { left };
                br.AddRange(right);
                return br;
            }
            private static List<Brick> Append(List<Brick> left, Brick right)
            {
                List<Brick> br = new List<Brick>(left);
                br.Add(right);
                return br;
            }

            private void ComputeBrickSuffixes(Brick brick, IndexInt minIndex, IndexInt maxIndex, HashSet<string> leftSet, HashSet<string> rightSet)
            {
                foreach (string value in brick.values)
                {
                    if (maxIndex > value.Length)
                    {
                        // short

                        for (int i = 0; i <= value.Length; ++i)
                        {
                            rightSet.Add(value.Substring(i));
                            if (i >= minIndex.AsInt)
                            {
                                leftSet.Add(value.Substring(i));
                            }
                        }
                    }
                    else
                    {
                        // long

                        for (int i = Math.Max(minIndex.AsInt, 0); i <= Math.Min(maxIndex.AsInt, value.Length); ++i)
                        {
                            leftSet.Add(value.Substring(i));
                        }

                        rightSet.Add(value);
                    }
                }
            }

            private void ComputeBrickPrefixes(Brick brick, IndexInt minIndex, IndexInt maxIndex, HashSet<string> leftSet, HashSet<string> rightSet)
            {
                foreach (string value in brick.values)
                {
                    if (maxIndex > value.Length)
                    {
                        // short

                        for (int i = 0; i <= value.Length; ++i)
                        {
                            rightSet.Add(value.Substring(0, i));
                            if (i >= minIndex.AsInt)
                            {
                                leftSet.Add(value.Substring(0, i));
                            }
                        }
                    }
                    else
                    {
                        // long

                        for (int i = Math.Max(minIndex.AsInt, 0); i <= IndexInt.Min(maxIndex, IndexInt.For(value.Length)).AsInt; ++i)
                        {
                            leftSet.Add(value.Substring(0, i));
                        }
                    }

                }
            }

            private IndexInt RepeatCountAfter(IndexInt maxBrickRepeats, IndexInt minStringLength, IndexInt maxIndex)
            {

                if (minStringLength == 0 || maxIndex.IsInfinite)
                {
                    return maxBrickRepeats - IndexInt.ForNonNegative(1);
                }
                else
                {
                    int maxRepeatsBefore = (maxIndex.AsInt + minStringLength.AsInt - 1) / minStringLength.AsInt;
                    return IndexInt.Min(maxBrickRepeats, IndexInt.ForNonNegative(maxRepeatsBefore)) - IndexInt.ForNonNegative(1);
                }
            }


            private void AddBrickBefore(Brick brick, IndexInt minIndex, IndexInt maxIndex, List<Brick> destination)
            {
                if (brick.values == null)
                {
                    // All possible string - not affected
                    destination.Add(brick);
                    return;
                }

                IndexInt minStringLength = IndexInt.Min(brick.values, s => IndexInt.ForNonNegative(s.Length));
                IndexInt maxStringLength = IndexInt.Max(brick.values, s => IndexInt.ForNonNegative(s.Length));

                if (minIndex >= maxStringLength * brick.max)
                {
                    // Whole brick included
                    destination.Add(brick);
                }
                else if (minIndex > minStringLength)
                {
                    // A few full repeats
                    IndexInt min = minIndex.Divide(maxStringLength);
                    IndexInt max = minIndex.Divide(minStringLength);

                    Brick leftBrick = new Brick(brick.values, min, max);
                    Brick rightBrick = new Brick(brick.values, max.IsInfinite ? IndexInt.For(0) : brick.min - max, brick.max - min);

                    destination.Add(leftBrick);

                    IndexInt newMinIndex;
                    if (max.IsInfinite || max * maxStringLength > minIndex)
                    {
                        newMinIndex = IndexInt.For(0);
                    }
                    else
                    {
                        newMinIndex = minIndex - max * maxStringLength;
                    }

                    AddBrickBefore(rightBrick, newMinIndex, maxIndex - min * maxStringLength, destination);
                }
                else
                {

                    HashSet<string> leftSet = new HashSet<string>();
                    HashSet<string> rightSet = new HashSet<string>();
                    ComputeBrickPrefixes(brick, minIndex, maxIndex, leftSet, rightSet);
                    destination.Add(new Brick(leftSet));

                    if (rightSet.Count != 0)
                    {
                        Debug.Assert(minStringLength < maxIndex);

                        IndexInt maxRepeats = RepeatCountAfter(brick.max, minStringLength, maxIndex); ;

                        destination.Add(new Brick(rightSet, IndexInt.For(0), maxRepeats));

                    }
                }


            }
            private void AddBrickAfter(Brick brick, IndexInt minIndex, IndexInt maxIndex, List<Brick> destination)
            {

                if (maxIndex == 0 || brick.values == null)
                {
                    destination.Add(brick);
                    return;
                }

                IndexInt minStringLength = IndexInt.Min(brick.values, s => IndexInt.ForNonNegative(s.Length));
                IndexInt maxStringLength = IndexInt.Max(brick.values, s => IndexInt.ForNonNegative(s.Length));

                if (minIndex >= maxStringLength * brick.max)
                {
                    //Nothing included
                    return;
                }
                else if (minIndex > minStringLength)
                {
                    // Skip a few repeats
                    IndexInt min = minIndex.Divide(maxStringLength);
                    IndexInt max = minIndex.Divide(minStringLength);

                    Brick rightBrick = new Brick(brick.values, brick.min - max, brick.max - min);

                    IndexInt newMinIndex;
                    if (max.IsInfinite || max * maxStringLength > minIndex)
                    {
                        newMinIndex = IndexInt.For(0);
                    }
                    else
                    {
                        newMinIndex = minIndex - max * maxStringLength;
                    }

                    AddBrickAfter(rightBrick, newMinIndex, maxIndex - min * maxStringLength, destination);
                }
                else
                {
                    HashSet<string> leftSet = new HashSet<string>();
                    HashSet<string> rightSet = new HashSet<string>();
                    ComputeBrickSuffixes(brick, minIndex, maxIndex, leftSet, rightSet);
                    destination.Add(new Brick(leftSet));

                    IndexInt maxRepeats = RepeatCountAfter(brick.max, minStringLength, maxIndex);

                    destination.Add(new Brick(rightSet, IndexInt.For(0), maxRepeats));

                }


            }

            private List<Brick> Before(List<Brick> bricks, IndexInterval index)
            {
                if (index.LowerBound.IsInfinite)
                {
                    return bricks;
                }

                List<Brick> result = new List<Brick>();

                for (int i = 0; i < bricks.Count && index.UpperBound > 0; ++i)
                {
                    Brick brick = bricks[i];

                    AddBrickBefore(brick, index.LowerBound, index.UpperBound, result);

                    index = index.AfterOffset(IndexInterval.For(brick.MinLength, brick.MaxLength));
                }

                return result;
            }
            private List<Brick> After(List<Brick> bricks, IndexInterval index)
            {
                if (index.LowerBound.IsInfinite)
                {
                    return new List<Brick>();
                }

                List<Brick> result = new List<Brick>();

                for (int i = 0; i < bricks.Count; ++i)
                {
                    Brick brick = bricks[i];

                    AddBrickAfter(brick, index.LowerBound, index.UpperBound, result);

                    index = index.AfterOffset(IndexInterval.For(brick.MinLength, brick.MaxLength));
                }

                return result;
            }
            private Bricks ListToBricks(List<Brick> bricks)
            {
                return new Bricks(bricks, policy).Normalize(BrickNormalizationLocation.Operation);
            }
            private Bricks TemplateListToBricks(List<Brick> bricks)
            {
                return new Bricks(bricks, policy).Normalize(BrickNormalizationLocation.Conversion);
            }


            #endregion

            #region Operations returning strings
            ///<inheritdoc/>
            public Bricks Concat(WithConstants<Bricks> left, WithConstants<Bricks> right)
            {
                List<Brick> result = ConcatLists(left.ToAbstract(this).bricks, right.ToAbstract(this).bricks);
                return ListToBricks(result);
            }
            ///<inheritdoc/>
            public Bricks Insert(WithConstants<Bricks> self, IndexInterval index, WithConstants<Bricks> other)
            {
                List<Brick> allBricks = self.ToAbstract(this).bricks;
                List<Brick> result = Before(allBricks, index);
                List<Brick> rightPart = After(allBricks, index);

                result.AddRange(other.ToAbstract(this).bricks);
                result.AddRange(rightPart);

                return ListToBricks(result);
            }
            ///<inheritdoc/>
            public Bricks Replace(Bricks self, CharInterval from, CharInterval to)
            {
                List<Brick> bricks = self.bricks.Select(b => b.Replace(from, to)).ToList();

                return ListToBricks(bricks);
            }
            ///<inheritdoc/>
            public Bricks Replace(WithConstants<Bricks> self, WithConstants<Bricks> from, WithConstants<Bricks> to)
            {
                return Top;
            }
            ///<inheritdoc/>
            public Bricks Remove(Bricks self, IndexInterval index, IndexInterval length)
            {
                List<Brick> leftPart = Before(self.bricks, index);

                List<Brick> result;

                if (length.LowerBound.IsInfinite)
                {
                    result = leftPart;
                }
                else
                {
                    List<Brick> rightPart = After(After(self.bricks, index), length);
                    result = ConcatLists(leftPart, rightPart);
                }

                return ListToBricks(result);
            }
            ///<inheritdoc/>
            public Bricks Substring(Bricks self, IndexInterval index, IndexInterval length)
            {
                List<Brick> result = Before(After(self.bricks, index), length);

                return ListToBricks(result);
            }

            ///<inheritdoc/>
            public Bricks PadLeftRight(Bricks self, IndexInterval length, CharInterval fill, bool right)
            {
                Brick paddingBrick = self.PaddingBrick(length, fill);
                if ((object)paddingBrick == null)
                {
                    return self;
                }
                else
                {
                    return ListToBricks(right ? Append(self.bricks, paddingBrick) : Prepend(paddingBrick, self.bricks));
                }
            }

            private HashSet<string> Trim(Brick br, char[] trim, bool reverse)
            {
                HashSet<string> result = new HashSet<string>();
                foreach (string s in br.values)
                {
                    if (reverse)
                    {
                        result.Add(s.TrimEnd(trim));
                    }
                    else
                    {
                        result.Add(s.TrimStart(trim));
                    }
                }
                return result;
            }

            private List<Brick> Trim(List<Brick> bricks, char[] trim, bool reverse)
            {
                List<Brick> result = new List<Brick>();

                bool before = true;
                bool after = false;

                IEnumerable<Brick> bricksEnumerable = bricks;

                if (reverse)
                    bricksEnumerable = bricksEnumerable.Reverse();

                foreach (Brick brick in bricksEnumerable)
                {
                    if (after || brick.MustBeEmpty)
                    {
                        result.Add(brick);
                    }
                    else if (brick.values == null)
                    {
                        result.Add(brick);
                        before = false;
                    }
                    else
                    {
                        //TODO: values == null
                        HashSet<string> trimmed = Trim(brick, trim, reverse);
                        if (!trimmed.Contains(""))
                        {
                            //TODO: VD: min/max
                            if (!before)
                            {
                                trimmed.UnionWith(brick.values);
                            }

                            Brick trimmedBrick = new Brick(trimmed);
                            IndexInt one = IndexInt.ForNonNegative(1);
                            Brick nonTrimmedBrick = new Brick(brick.values, brick.min - one, brick.max - one);

                            result.Add(trimmedBrick);
                            result.Add(nonTrimmedBrick);

                            // Trimming ends here
                            before = false;
                            after = true;

                        }
                        else if (before && trimmed.Count == 1)
                        {
                            // Trimmed everything
                            // No need to add any brick
                        }
                        else
                        {
                            IndexInt min = brick.min;
                            IndexInt max = brick.max;

                            if (before)
                            {
                                HashSet<string> fullyTrimmedCopy = new HashSet<string>(trimmed);
                                Brick fullyTrimmedBrick = new Brick(fullyTrimmedCopy);
                                result.Add(fullyTrimmedBrick);

                                IndexInt one = IndexInt.ForNonNegative(1);
                                min = min - one;
                                max = max - one;
                            }

                            trimmed.UnionWith(brick.values);
                            Brick partiallyTrimmedBrick = new Brick(trimmed, min, max);
                            result.Add(partiallyTrimmedBrick);

                            // Trimming may and may not continue
                            before = false;
                        }
                    }
                }

                if (reverse)
                {
                    result.Reverse();
                }

                return result;
            }

            private Bricks Trim(WithConstants<Bricks> self, WithConstants<Bricks> trimmed, bool start, bool end)
            {
                string trimString = trimmed.ToAbstract(this).ToConstant();
                if (string.IsNullOrEmpty(trimString))
                {
                    // Not constant, or empty array
                    return Top;
                }

                char[] trimArray = trimString.ToCharArray();
                List<Brick> bricks = self.ToAbstract(this).bricks;

                if (end)
                    bricks = Trim(bricks, trimArray, true);

                if (start)
                    bricks = Trim(bricks, trimArray, false);

                return ListToBricks(bricks);
            }
            ///<inheritdoc/>
            public Bricks Trim(WithConstants<Bricks> self, WithConstants<Bricks> trimmed)
            {
                return Trim(self, trimmed, true, true);
            }
            ///<inheritdoc/>
            public Bricks TrimStartEnd(WithConstants<Bricks> self, WithConstants<Bricks> trimmed, bool end)
            {
                return Trim(self, trimmed, !end, end);
            }
            #endregion
            #region Operations returning integers
            ///<inheritdoc/>
            public CompareResult CompareOrdinal(WithConstants<Bricks> self, WithConstants<Bricks> other)
            {
                string selfPrefix = self.ToAbstract(this).ToPrefix();
                string otherPrefix = other.ToAbstract(this).ToPrefix();
                throw new NotImplementedException();
            }
            ///<inheritdoc/>
            public IndexInterval GetLength(Bricks self)
            {
                IndexInt lower = self.MinLength;
                IndexInt upper = self.MaxLength;

                return IndexInterval.For(lower, upper);
            }
            ///<inheritdoc/>
            public IndexInterval IndexOf(WithConstants<Bricks> self, WithConstants<Bricks> needle, IndexInterval offset, IndexInterval count, bool last)
            {
                if (!offset.IsFiniteConstant || offset.LowerBound != 0 || !count.IsInfinity)
                {
                    // Offset and count are not supported
                    return IndexInterval.Unknown;
                }

                Bricks selfBricks = self.ToAbstract(this);
                Bricks needleBricks = needle.ToAbstract(this);

                if (selfBricks.ToConstant() == "")
                {
                    if (last)
                    {
                        IndexInt one = IndexInt.ForNonNegative(1);
                        IndexInt minIndex = IndexInt.Max(selfBricks.MinLength, one) - one;
                        IndexInt maxIndex = IndexInt.Max(selfBricks.MaxLength, one) - one;
                        return IndexInterval.For(minIndex, maxIndex);
                    }
                    else
                    {
                        return IndexInterval.For(0);
                    }
                }
                else
                {
                    IndexInt maxSelfLength = selfBricks.MaxLength;
                    IndexInt minNeedleLength = needleBricks.MinLength;

                    if (minNeedleLength > maxSelfLength)
                    {
                        return IndexInterval.For(IndexInt.Negative);
                    }
                    else
                    {
                        return IndexInterval.For(IndexInt.Negative, maxSelfLength - minNeedleLength);
                    }
                }
            }

            #endregion
            #region Operations returning bool
            ///<inheritdoc/>
            public IStringPredicate IsEmpty(Bricks self, Variable selfVariable)
            {
                if (self.IsBottom)
                {
                    return FlatPredicate.Bottom;
                }
                else if (self.MinLength > 0)
                {
                    return FlatPredicate.False;
                }
                else if (self.MaxLength > 0)
                {
                    return FlatPredicate.Top;
                }
                else
                {
                    return FlatPredicate.True;
                }
            }


            private bool MustContain(Brick brick, string constant)
            {
                if (brick.min == 0 || brick.values == null)
                {
                    return false;
                }

                return brick.values.All(value => value.Contains(constant));
            }

            private Bricks MakeTemplate(List<Brick> bricks, bool start, bool end)
            {
                Brick top = new Brick(true);

                if (!start)
                    bricks = Prepend(top, bricks);
                if (!end)
                    bricks = Append(bricks, top);
                return TemplateListToBricks(bricks);
            }

            private bool NonContainmentTest(Bricks self, Bricks other, bool start, bool end)
            {
                Bricks template = MakeTemplate(other.bricks, start, end);

                Bricks intersection = self.Meet(template);

                return intersection.IsBottom;
            }
            private IStringPredicate MakeTemplatePredicate(Variable variable, string constant, bool start, bool end)
            {
                Bricks template = MakeTemplate(new List<Brick> { new Brick(constant) }, start, end);
                return StringAbstractionPredicate.ForTrue(variable, template);
            }

            ///<inheritdoc/>
            public IStringPredicate Contains(
              WithConstants<Bricks> self, Variable selfVariable,
              WithConstants<Bricks> other, Variable otherVariable)
            {
                Bricks selfBricks = self.ToAbstract(this);
                Bricks otherBricks = other.ToAbstract(this);

                string otherConstant = otherBricks.ToConstant();

                if (otherConstant != null)
                {
                    if (selfBricks.bricks.Exists(brick => MustContain(brick, otherConstant)))
                    {
                        return FlatPredicate.True;
                    }
                }

                if (NonContainmentTest(selfBricks, otherBricks, false, false))
                    return FlatPredicate.False;

                if (selfVariable != null && otherConstant != null)
                {
                    return MakeTemplatePredicate(selfVariable, otherConstant, false, false);
                }

                return FlatPredicate.Top;
            }
            ///<inheritdoc/>
            public IStringPredicate StartsWithOrdinal(
              WithConstants<Bricks> self, Variable selfVariable,
              WithConstants<Bricks> other, Variable otherVariable)
            {
                Bricks selfBricks = self.ToAbstract(this);
                Bricks otherBricks = other.ToAbstract(this);

                string otherConstant = otherBricks.ToConstant();

                if (otherConstant != null)
                {
                    string thisPrefix = selfBricks.ToPrefix();
                    if (thisPrefix.StartsWith(otherConstant, StringComparison.Ordinal))
                    {
                        return FlatPredicate.True;
                    }
                }

                if (NonContainmentTest(selfBricks, otherBricks, true, false))
                    return FlatPredicate.False;

                string otherPrefix = otherBricks.ToPrefix();

                if (selfVariable != null && otherPrefix != "")
                {
                    return MakeTemplatePredicate(selfVariable, otherPrefix, true, false);
                }

                return FlatPredicate.Top;
            }
            ///<inheritdoc/>
            public IStringPredicate EndsWithOrdinal(
              WithConstants<Bricks> self, Variable selfVariable,
              WithConstants<Bricks> other, Variable otherVariable)
            {
                Bricks selfBricks = self.ToAbstract(this);
                Bricks otherBricks = other.ToAbstract(this);

                string otherConstant = otherBricks.ToConstant();

                if (otherConstant != null)
                {
                    string thisSuffix = selfBricks.ToSuffix();
                    if (thisSuffix.EndsWith(otherConstant, StringComparison.Ordinal))
                    {
                        return FlatPredicate.True;
                    }
                }

                if (NonContainmentTest(selfBricks, otherBricks, false, true))
                    return FlatPredicate.False;

                string otherSuffix = otherBricks.ToSuffix();

                if (selfVariable != null && otherSuffix != "")
                {
                    return MakeTemplatePredicate(selfVariable, otherSuffix, false, true);
                }

                return FlatPredicate.Top;
            }
            ///<inheritdoc/>
            public IStringPredicate Equals(
              WithConstants<Bricks> self, Variable selfVariable,
              WithConstants<Bricks> other, Variable otherVariable)
            {
                Bricks selfBricks = self.ToAbstract(this);
                Bricks otherBricks = other.ToAbstract(this);

                string selfConstant = selfBricks.ToConstant();
                string otherConstant = otherBricks.ToConstant();

                if (otherConstant != null && selfConstant == otherConstant)
                {
                    return FlatPredicate.True;
                }

                if (NonContainmentTest(selfBricks, otherBricks, true, true))
                    return FlatPredicate.False;

                if (selfVariable != null)
                {
                    return StringAbstractionPredicate.ForTrue(selfVariable, otherBricks);
                }
                else if (otherVariable != null)
                {
                    return StringAbstractionPredicate.ForTrue(otherVariable, selfBricks);
                }
                else
                {
                    return FlatPredicate.Top;
                }
            }
            #endregion
            ///<inheritdoc/>
            public IStringPredicate RegexIsMatch(Bricks self, Variable selfVariable, Regex.AST.Element regex)
            {
                BricksRegex brickRegexConverter = new BricksRegex(self);

                ProofOutcome matchOutcome = brickRegexConverter.IsMatch(regex);

                if (matchOutcome != ProofOutcome.Top || selfVariable == null)
                {
                    return FlatPredicate.ForProofOutcome(matchOutcome);
                }
                else
                {
                    Bricks regexBricks = brickRegexConverter.BricksForRegex(regex).Normalize(BrickNormalizationLocation.Conversion);
                    return StringAbstractionPredicate.ForTrue(selfVariable, regexBricks);
                }
            }
            ///<inheritdoc/>
            public CharInterval GetCharAt(Bricks self, IndexInterval index)
            {
                // Remove the first index characters
                List<Brick> following = After(self.bricks, index);

                CharInterval result = CharInterval.Unreached;

                // Iterate through the bricks
                foreach (Brick brick in following)
                {
                    if (brick.values == null)
                    {
                        // Top brick can contain any character
                        return CharInterval.Unknown;
                    }

                    // Get the first character from all values
                    foreach (string value in brick.values)
                    {
                        if (value.Length >= 1)
                        {
                            result = result.Join(CharInterval.For(value[0]));
                        }
                    }

                    // If the brick can be empty, continue with the next brick
                    if (!brick.CanBeEmpty)
                    {
                        break;
                    }
                }
                return result;
            }
            ///<inheritdoc/>
            public Bricks SetCharAt(Bricks self, IndexInterval index, CharInterval value)
            {
                List<Brick> before = Before(self.bricks, index);
                Brick inserted = new Brick(self.CharIntervalToSet(value));

                before.Add(inserted);

                IndexInt one = IndexInt.For(1);
                List<Brick> after = After(self.bricks, IndexInterval.For(index.LowerBound + one, index.UpperBound + one));

                return new Bricks(ConcatLists(before, after), policy);
            }
            #region Factory
            ///<inheritdoc/>
            public Bricks Top
            {
                get { return new Bricks(true, policy); }
            }
            ///<inheritdoc/>
            public Bricks Constant(string constant)
            {
                return new Bricks(constant, policy);
            }
            #endregion
        }

        #region String operations

        private HashSet<string> CharIntervalToSet(CharInterval interval)
        {
            HashSet<string> set = new HashSet<string>();
            for (int character = interval.LowerBound; character <= interval.UpperBound; ++character)
            {
                set.Add(((char)character).ToString());
            }
            return set;
        }

        /// <summary>
        /// Creates a brick wich would extend the list of brick to have at least the specified length.
        /// </summary>
        /// <param name="length">The desired length in characters.</param>
        /// <param name="fill">The filling character.</param>
        /// <returns>The padding brick or <see langword="null"/> if the list is already long enough.</returns>
        private Brick PaddingBrick(IndexInterval length, CharInterval fill)
        {
            IndexInt min = this.MinLength;
            IndexInt max = this.MaxLength;

            if (length.UpperBound > min)
            {
                IndexInt minPadding = length.LowerBound > max ? length.LowerBound - max : IndexInt.For(0);
                return new Brick(CharIntervalToSet(fill), minPadding, length.UpperBound - min);
            }
            else
            {
                return null;
            }
        }
        #endregion

        #region Object overriden methods
        public override string ToString()
        {
            return string.Join("", bricks);
        }
        #endregion

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
            return LessThanEqual(a as Bricks);
        }

        IAbstractDomain IAbstractDomain.Bottom
        {
            get { return Bottom; }
        }

        IAbstractDomain IAbstractDomain.Top
        {
            get { return Top; }
        }

        public IAbstractDomain Join(IAbstractDomain a)
        {
            return Join(a as Bricks);
        }

        public IAbstractDomain Meet(IAbstractDomain a)
        {
            return Meet(a as Bricks);
        }

        public IAbstractDomain Widening(IAbstractDomain prev)
        {
            return policy.Widening(this, (Bricks)prev);
        }

        public object Clone()
        {
            return new Bricks(this);
        }
        public T To<T>(IFactory<T> factory)
        {
            return factory.Constant(true);
        }
        #endregion

        ///<inheritdoc/>
        public Bricks Constant(string cst)
        {
            return new Bricks(cst, policy);
        }
    }
}
