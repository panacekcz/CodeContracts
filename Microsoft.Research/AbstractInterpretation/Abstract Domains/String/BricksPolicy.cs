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
    /// Indicates where the normalization is happening.
    /// </summary>
    public enum BrickNormalizationLocation
    {
        /// <summary>
        /// After joining two brick lists.
        /// </summary>
        Join,
        /// <summary>
        /// In evaluating operations.
        /// </summary>
        Operation,
        /// <summary>
        /// After converting from other representations.
        /// </summary>
        Conversion,
        /// <summary>
        /// After widening.
        /// </summary>
        Widening,
    }

    /// <summary>
    /// Provides configurable procedures for the bricks abstract domain.
    /// </summary>
    public interface IBricksPolicy
    {
        /// <summary>
        /// Normalizes a list of bricks.
        /// </summary>
        /// <param name="element">The list of bricks.</param>
        /// <param name="location">Kind of location where the normalization is performed.</param>
        /// <returns>The normalized list of bricks.</returns>
        Bricks Normalize(Bricks element, BrickNormalizationLocation location);
        /// <summary>
        /// Applies the widening operator.
        /// </summary>
        /// <param name="prev">The previous abstract element.</param>
        /// <param name="next">The new abstract element.</param>
        /// <returns>The result of widening <paramref name="prev"/> by <paramref name="next"/>.</returns>
        Bricks Widening(Bricks prev, Bricks next);
        /// <summary>
        /// Extends a list of bricks to the length of another list.
        /// </summary>
        /// <param name="shorter">The shorter list of bricks.</param>
        /// <param name="longer">The longer list of brick.</param>
        /// <returns>A list of brick representing the same strings as <paramref name="shorter"/>,
        /// with the same length as <paramref name="longer"/>
        /// </returns>
        Bricks Extend(Bricks shorter, Bricks longer);
    }
    /// <summary>
    /// Provides the default implementation of bricks configurable procedures.
    /// </summary>
    public class DefaultBricksPolicy : IBricksPolicy
    {
        /// <summary>
        /// Constructs the default bricks policy with the
        /// default settings.
        /// </summary>
        public DefaultBricksPolicy()
        {
            ListLengthLimit = IndexInt.For(1000);
            SetSizeLimit = IndexInt.For(2 << 17);
            RepeatDifferenceLimit = IndexInt.For(100);

            ExpandConstantRepetitions = true;
            MergeConstantSets = true;
        }

        /// <summary>
        /// Gets or sets the maximum number of bricks in a brick list.
        /// </summary>
        public IndexInt ListLengthLimit { get; set; }
        /// <summary>
        /// Gets or sets the maximum number of string constants in a brick.
        /// </summary>
        public IndexInt SetSizeLimit { get; set; }
        /// <summary>
        /// Gets or sets the maximum difference between the numbers of repetitions (in a single brick).
        /// </summary>
        public IndexInt RepeatDifferenceLimit { get; set; }

        /// <summary>
        /// Gets or sets whether successive bricks with exactly one repetition should
        /// be merged into one (strings concatenated) when doing normalization.
        /// </summary>
        public bool MergeConstantSets { get; set; }
        /// <summary>
        /// Gets or sets whether bricks with a constant number of repetitions should
        /// be expanded (strings repeated) when doing normalization.
        /// </summary>
        public bool ExpandConstantRepetitions { get; set; }

        /// <summary>
        /// Removes empty bricks from a list of bricks.
        /// </summary>
        /// <remarks>
        /// Empty bricks do not change the represented set of strings,
        /// so they can be removed.
        /// </remarks>
        /// <param name="bricks">A list of bricks to be modified.</param>
        /// <param name="changed">Set to <see langword="true"/> if <paramref name="bricks"/> was modified.</param>
        private static void RemoveEmptyBricksStep(List<Brick> bricks, ref bool changed)
        {
            for (int i = 0; i < bricks.Count; ++i)
            {
                if (bricks[i].max == 0)
                {
                    bricks.RemoveAt(i);
                    --i;
                    changed = true;
                }
            }
        }

        private static void MergeConstantSetsStep(List<Brick> bricks, ref bool changed)
        {
            for (int i = 0; i < bricks.Count - 1; ++i)
            {
                if (bricks[i].min == 1 && bricks[i].max == 1 && bricks[i + 1].min == 1 && bricks[i + 1].max == 1)
                {
                    bricks[i] = bricks[i].MergeSets(bricks[i + 1]);
                    bricks.RemoveAt(i + 1);
                    --i;
                    changed = true;
                }
            }
        }


        private static void ExpandConstantRepetitionsStep(List<Brick> bricks, ref bool changed)
        {
            for (int i = 0; i < bricks.Count; ++i)
            {
                if (bricks[i].max > 1 && bricks[i].max == bricks[i].min)
                {
                    bricks[i] = bricks[i].ExpandRepeats();
                    changed = true;
                }
            }

        }

        private static void MergeSameSetsStep(List<Brick> bricks, ref bool changed)
        {
            for (int i = 0; i < bricks.Count - 1; ++i)
            {
                // The condition is more strict than in the original article,
                // because it would reverse the following rule
                if (Brick.stringSetComparer.Equals(bricks[i].values, bricks[i + 1].values) && (bricks[i].min != bricks[i].max || bricks[i + 1].min != 0))
                {
                    if (bricks[i].values != null)
                    {
                        bricks[i] = bricks[i].MergeCardinalities(bricks[i + 1]);
                    }

                    bricks.RemoveAt(i + 1);
                    --i;
                    changed = true;
                }
            }

        }

        private static void BreakBrickStep(List<Brick> bricks, ref bool changed, bool expand)
        {
            for (int i = 0; i < bricks.Count; ++i)
            {
                if (bricks[i].min >= 1 && bricks[i].max != bricks[i].min)
                {
                    Brick left, right;
                    bricks[i].Break(out left, out right, expand);
                    bricks[i] = left;
                    i++;
                    bricks.Insert(i, right);
                    changed = true;
                }
            }
        }
        ///<inheritdoc/>
        public Bricks Normalize(Bricks element, BrickNormalizationLocation location)
        {
            List<Brick> bricks = element.bricks;

            bool changed;
            do
            {
                changed = false;

                // Rule 1
                RemoveEmptyBricksStep(bricks, ref changed);
                // Rule 2
                if (MergeConstantSets)
                {
                    MergeConstantSetsStep(bricks, ref changed);
                }
                // Rule 3
                if (ExpandConstantRepetitions)
                {
                    ExpandConstantRepetitionsStep(bricks, ref changed);
                }
                // Rule 4
                MergeSameSetsStep(bricks, ref changed);
                // Rule 5
                BreakBrickStep(bricks, ref changed, ExpandConstantRepetitions);

            } while (changed);

            return element;
        }

        private Brick Widening(Brick prev, Brick next)
        {
            Brick join = prev.Join(next);
            if (join.values != null && SetSizeLimit < join.values.Count)
            {
                return prev.Top;
            }
            if (RepeatDifferenceLimit < join.max - join.min)
            {
                return new Brick(join.values, IndexInt.For(0), IndexInt.Infinity);
            }
            else
            {
                return join;
            }
        }
        ///<inheritdoc/>
        public Bricks Widening(Bricks prev, Bricks next)
        {
            if (prev.IsBottom || next.IsTop)
            {
                return next;
            }
            if (next.IsBottom || prev.IsTop)
            {
                return prev;
            }

            if (ListLengthLimit > prev.bricks.Count || ListLengthLimit > next.bricks.Count)
            {
                return prev.Top;
            }
            else
            {
                Bricks result = prev.Zip(next, Widening);

                return result.Normalize(BrickNormalizationLocation.Widening);
            }
        }
        /// <summary>
        /// Extends the list to match the length of another list, 
        /// by adding empty bricks into the list.
        /// </summary>
        /// <param name="shorter">A list of brick.</param>
        /// <param name="longer">A list of brick of the same or greater length.</param>
        /// <returns>A list of bricks of the same length as <paramref name="longer"/></returns>
        public Bricks Extend(Bricks shorter, Bricks longer)
        {
            int targetLength = longer.bricks.Count;
            int emptyBricksToAdd = targetLength - shorter.bricks.Count;

            if (emptyBricksToAdd == 0)
            {
                // The list has the correct length already,
                //no need to create a new one.
                return shorter;
            }

            List<Brick> result = new List<Brick>();
            int sourceIndex = 0;

            for (int targetIndex = 0; targetIndex < targetLength; ++targetIndex)
            {
                if (emptyBricksToAdd > 0 && (sourceIndex >= shorter.bricks.Count ||
                  shorter.bricks[sourceIndex] != longer.bricks[targetIndex]))
                {
                    //Add an empty brick
                    result.Add(new Brick(""));
                    --emptyBricksToAdd;
                }
                else
                {
                    //Copy a brick from the shorter list
                    result.Add(shorter.bricks[sourceIndex]);
                    ++sourceIndex;
                }
            }
            return new Bricks(result, this);
        }
    }
}
