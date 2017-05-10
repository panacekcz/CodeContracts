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
using Microsoft.Research.Regex.Model;
using Microsoft.Research.CodeAnalysis;
using System.Collections;
using Microsoft.Research.AbstractDomains.Strings.Regex;
using System.Diagnostics.Contracts;

namespace Microsoft.Research.AbstractDomains.Strings
{

    /// <summary>
    /// Operations for generating a non-matching CharacterInclusion for a regex.
    /// (CharacterInclusion that over-approximates the set of non-matching strings.)
    /// </summary>
    internal class CharacterInclusionComplementGeneratingOperations<CharacterSet> : IGeneratingOperationsForRegex<CharacterInclusion<CharacterSet>>
        where CharacterSet : ICharacterSet<CharacterSet>
    {
        private readonly CharacterInclusion<CharacterSet> factoryElement;

        /// <summary>
        /// Creates a instance which uses a specified element to create other elements.
        /// </summary>
        /// <param name="factoryElement">Abstract element used as a factory to create other elements.</param>
        public CharacterInclusionComplementGeneratingOperations(CharacterInclusion<CharacterSet> factoryElement)
        {
            this.factoryElement = factoryElement;
        }

        #region IGeneratingOperationsForRegex<CharacterInclusion<CharacterSet>> implementation

        public CharacterInclusion<CharacterSet> Bottom
        {
            get
            {
                //No matching means all are non-matching
                return factoryElement.Top;
            }
        }

        public CharacterInclusion<CharacterSet> Top
        {
            get
            {
                // All matching means no non-matching
                return factoryElement.Bottom;
            }
        }

        public CharacterInclusion<CharacterSet> Empty
        {
            get
            {
                // All non-empty strings are matching
                return Bottom;
            }
        }

        public CharacterInclusion<CharacterSet> AddChar(CharacterInclusion<CharacterSet> prev, CharRanges next, bool closed)
        {
            if (!closed && prev.IsBottom)
            {
                IEnumerable<CharInterval> allowed = next.ToIntervals();
                return prev.FromDisallowed(allowed);
            }
            else
                return prev.Top;
        }

        public bool CanBeEmpty(CharacterInclusion<CharacterSet> prev)
        {
            //TODO: VD: verify:
            // Here the meaning is "the non-matching MUST be empty"
            return prev.MustBeEmpty;
        }

        public CharacterInclusion<CharacterSet> Join(CharacterInclusion<CharacterSet> left, CharacterInclusion<CharacterSet> right, bool widen)
        {
            return left.Meet(right);
        }
        public bool IsUnderapproximating
        {
            get
            {
                return true;
            }
        }

        public CharacterInclusion<CharacterSet> Loop(CharacterInclusion<CharacterSet> prev, CharacterInclusion<CharacterSet> loop, CharacterInclusion<CharacterSet> last, IndexInt min, IndexInt max)
        {
            return Bottom;
        }
        #endregion
    }


    /// <summary>
    /// Operations for generating a matching CharacterInclusion for a regex.
    /// (CharacterInclusion that over-approximates the set of matching strings.)
    /// </summary>
    internal class CharacterInclusionGeneratingOperations<CharacterSet> : IGeneratingOperationsForRegex<CharacterInclusion<CharacterSet>>
        where CharacterSet : ICharacterSet<CharacterSet>
    {
        private readonly CharacterInclusion<CharacterSet> factory;
        /// <summary>
        /// Creates a instance which uses a specified element to create other elements.
        /// </summary>
        /// <param name="factoryElement">Abstract element used as a factory to create other elements.</param>
        public CharacterInclusionGeneratingOperations(CharacterInclusion<CharacterSet> factory)
        {
            this.factory = factory;
        }

        #region IGeneratingOperationsForRegex<CharacterInclusion<CharacterSet>> implementation
        public CharacterInclusion<CharacterSet> Bottom
        {
            get
            {
                return factory.Bottom;
            }
        }

        public CharacterInclusion<CharacterSet> Top
        {
            get
            {
                return factory.Top;
            }
        }
        public CharacterInclusion<CharacterSet> Empty
        {
            get
            {
                return factory.Constant("");
            }
        }

        public bool IsUnderapproximating
        {
            get
            {
                return false;
            }
        }

        public CharacterInclusion<CharacterSet> AddChar(CharacterInclusion<CharacterSet> prev, CharRanges next, bool closed)
        {
            IEnumerable<CharInterval> allowed = next.ToIntervals();
            //Closed - Adds char to mandatory and allowed
            //Open - Adds char to mandatory and sets allowed to all chars
            CharacterInclusion<CharacterSet> ci = prev.Character(allowed, closed);
            return prev.Combine(ci);
        }

        public bool CanBeEmpty(CharacterInclusion<CharacterSet> prev)
        {
            return !prev.MustBeNonEmpty;
        }

        public CharacterInclusion<CharacterSet> Join(CharacterInclusion<CharacterSet> left, CharacterInclusion<CharacterSet> right, bool widen)
        {
            return widen ? (CharacterInclusion<CharacterSet>)left.Widening(right) : left.Join(right);
        }

        public CharacterInclusion<CharacterSet> Loop(CharacterInclusion<CharacterSet> prev, CharacterInclusion<CharacterSet> loop, CharacterInclusion<CharacterSet> last, IndexInt min, IndexInt max)
        {
            if (min == 0)
            {
                return prev.Combine(last.Part(false, false, false, false));
            }
            else
            {
                return prev.Combine(last);
            }
        }
        #endregion
    }


    /// <summary>
    /// Abstract state for CharacterInclusion regex matching.
    /// </summary>
    internal struct CharacterInclusionMatchingState<CharacterSet>
        where CharacterSet : ICharacterSet<CharacterSet>
    {
        public CharacterSet encountered;
        public CharacterSet looped;

        //TODO: VD: Closed start can be represented by setting encountered to full?
        public bool startAnchor;
        public bool endAnchor;
        public bool empty;
        public bool bottom;

        /*public CharacterInclusionMatchingState(CharacterSet encountered, bool fail, bool closedStart)
        {
            this.encountered = encountered;
            this.fail = fail;
            this.closedStart = closedStart;
        }*/
        public bool Accepts(CharacterInclusion<CharacterSet> ci)
        {
            if (bottom)
                return false;

            if (!empty)
            {
                if (!ci.mandatory.Intersects(encountered))
                    return false;
                if ((startAnchor || endAnchor) && !(ci.allowed.IsSingleton))
                    return false;
            }
            if (startAnchor && endAnchor && !ci.allowed.IsSubset(looped))
                return false;

            return true;
        }
    }

    /// <summary>
    /// Abstract state for CharacterInclusion regex matching.
    /// </summary>
    internal class CharacterInclusionMatchingOperations<CharacterSet> : IMatchingOperationsForRegex<CharacterInclusionMatchingState<CharacterSet>, CharacterInclusion<CharacterSet>>
        where CharacterSet : ICharacterSet<CharacterSet>
    {

        private CharacterSet emptySet;

        public CharacterInclusionMatchingOperations(CharacterInclusion<CharacterSet> input)
        {
            emptySet = input.CreateCharacterSetFor(false);
        }

        public CharacterInclusionMatchingState<CharacterSet> AssumeEnd(CharacterInclusion<CharacterSet> input, CharacterInclusionMatchingState<CharacterSet> prev, bool under)
        {
            if (prev.bottom)
                return prev;

            if (under)
            {

                /*if (!input.allowed.IsSubset(prev.encountered))
                    return GetBottom(input);
                return prev;*/
                return new CharacterInclusionMatchingState<CharacterSet>
                {
                    encountered = prev.encountered,
                    looped = prev.looped,
                    startAnchor = prev.startAnchor,
                    endAnchor = true,
                    empty = prev.empty,
                    bottom = false
                };
            }
            else
            {
                if (prev.startAnchor)
                {
                    //Test encountered against input
                    if (!input.mandatory.IsSubset(prev.encountered))
                        return GetBottom(input);
                }

                return prev;
            }
        }

        public CharacterInclusionMatchingState<CharacterSet> AssumeStart(CharacterInclusion<CharacterSet> input, CharacterInclusionMatchingState<CharacterSet> prev, bool under)
        {
            if (prev.bottom)
                return prev;

            if (under)
            {
                if (prev.empty)
                {
                    // set empty and loopsafe to false
                    return new CharacterInclusionMatchingState<CharacterSet>
                    {
                        encountered = emptySet,
                        looped = emptySet,
                        startAnchor = true,
                        endAnchor = prev.endAnchor,
                        empty = true,
                        bottom = false,
                    };
                }
                else
                {
                    return GetBottom(input);
                }
            }
            else
            {
                // Set encountered to false
                return new CharacterInclusionMatchingState<CharacterSet>
                {
                    encountered = emptySet,
                    looped = emptySet,
                    startAnchor = true,
                    endAnchor = prev.endAnchor,
                    empty = true,
                    bottom = false,
                };
            }
        }

        public CharacterInclusionMatchingState<CharacterSet> GetBottom(CharacterInclusion<CharacterSet> input)
        {
            return new CharacterInclusionMatchingState<CharacterSet>
            {
                encountered = emptySet,
                looped = emptySet,
                startAnchor = false,
                endAnchor = false,
                empty = false,
                bottom = true,
            };
        }

        public CharacterInclusionMatchingState<CharacterSet> GetTop(CharacterInclusion<CharacterSet> input)
        {
            return new CharacterInclusionMatchingState<CharacterSet>
            {
                encountered = emptySet,
                looped = emptySet,
                startAnchor = false,
                endAnchor = false,
                empty = true,
                bottom = false,
            };
        }

        public CharacterInclusionMatchingState<CharacterSet> Join(CharacterInclusion<CharacterSet> input, CharacterInclusionMatchingState<CharacterSet> left, CharacterInclusionMatchingState<CharacterSet> right, bool widen, bool under)
        {
            if (left.bottom)
                return right;
            if (right.bottom)
                return left;
            // TODO: VD:  check under-join
            return new CharacterInclusionMatchingState<CharacterSet>
            {
                encountered = left.encountered.Union(right.encountered),
                looped = left.looped.Intersection(right.looped),
                startAnchor = under ? (left.startAnchor || right.startAnchor) : left.startAnchor && right.startAnchor,
                endAnchor = under ? (left.endAnchor || right.endAnchor) : left.endAnchor && right.endAnchor,
                //TODO: VD: empty
                bottom = false,
            };
        }

        public CharacterInclusionMatchingState<CharacterSet> MatchChar(CharacterInclusion<CharacterSet> input, CharacterInclusionMatchingState<CharacterSet> prev, CharRanges next, bool under)
        {
            if (prev.bottom)
                return prev;

            //TODO: VD:  convert charranges to set
            CharacterSet cs = input.CreateCharacterSetFor(next.ToIntervals());

            if (under)
            {
                if (!prev.empty || prev.endAnchor)
                    return GetBottom(input);

                return new CharacterInclusionMatchingState<CharacterSet>
                {
                    encountered = cs,
                    looped = emptySet,
                    startAnchor = prev.startAnchor,
                    endAnchor = false,
                    empty = false,
                    bottom = false,
                };
            }
            else
            {
                //Test against input
                if (!input.allowed.Intersects(cs))
                    return GetBottom(input);
                //Add to encountered
                return new CharacterInclusionMatchingState<CharacterSet>
                {
                    encountered = prev.encountered.Union(cs),
                    looped = emptySet,
                    startAnchor = prev.startAnchor,
                    endAnchor = false,
                    empty = false, //TODO: char after end anchor, should go to bottom
                    bottom = false, 
                };
            }
        }

        public CharacterInclusionMatchingState<CharacterSet> BeginLoop(CharacterInclusion<CharacterSet> input, CharacterInclusionMatchingState<CharacterSet> prev, bool under)
        {
            if (under)
            {
                return new CharacterInclusionMatchingState<CharacterSet>
                {
                    encountered = input.CreateCharacterSetFor(false),
                    looped = input.CreateCharacterSetFor(false),
                    startAnchor = false,
                    endAnchor = false,
                    empty = true,
                    bottom = false,
                };
            }
            else
            {
                return new CharacterInclusionMatchingState<CharacterSet>
                {
                    encountered = input.CreateCharacterSetFor(false),
                    looped = input.CreateCharacterSetFor(false),
                    startAnchor = false,
                    endAnchor = false,
                    empty = true,
                    bottom = false,
                };
            }
        }

        public CharacterInclusionMatchingState<CharacterSet> EndLoop(CharacterInclusion<CharacterSet> input, CharacterInclusionMatchingState<CharacterSet> prev, CharacterInclusionMatchingState<CharacterSet> next, IndexInt min, IndexInt max, bool under)
        {
            if (under)
            {
                if (min == 0 || next.empty)
                {
                    if (prev.endAnchor || next.bottom || next.startAnchor || next.endAnchor)
                    {
                        return prev;
                    }
                    else if (prev.empty)
                    {
                        return new CharacterInclusionMatchingState<CharacterSet>
                        {
                            encountered = input.CreateCharacterSetFor(false),
                            looped = next.encountered,
                            startAnchor = prev.startAnchor,
                            endAnchor = false,
                            empty = true,
                            bottom = false,
                        };
                    }
                    else
                    {
                        return prev;
                        //TODO: VD: remove?
                        /*return new CharacterInclusionMatchingState<CharacterSet>
                        {
                            encountered = setFactory.Create(false, classification.Buckets),
                            looped = setFactory.Create(false, classification.Buckets),
                            startAnchor = prev.startAnchor,
                            endAnchor = false,
                            empty = false,
                            bottom = false,
                        };*/
                    }
                }
                else if (min == 1 && !next.endAnchor && !next.startAnchor && prev.empty && !prev.endAnchor && !next.bottom)
                {
                    return new CharacterInclusionMatchingState<CharacterSet>
                    {
                        encountered = next.encountered,
                        looped = next.encountered,
                        startAnchor = prev.startAnchor,
                        endAnchor = false,
                        empty = false,
                        bottom = false,
                    };
                }
                else
                {
                    return GetBottom(input);
                }
            }
            else
            {
                if (min == 0)
                {
                    if (next.bottom)
                        return prev;
                    else
                    {
                        return new CharacterInclusionMatchingState<CharacterSet>
                        {
                            encountered = prev.encountered.Union(next.encountered),
                            looped = prev.looped,
                            startAnchor = prev.startAnchor,
                            endAnchor = prev.endAnchor,
                            empty = prev.empty,
                            bottom = false,
                        };
                    }
                }
                else
                {
                    if (next.bottom)
                        return next;
                    else
                    {
                        return new CharacterInclusionMatchingState<CharacterSet>
                        {
                            encountered = prev.encountered.Union(next.encountered),
                            looped = prev.looped,
                            startAnchor = prev.startAnchor,
                            endAnchor = prev.endAnchor,
                            empty = prev.empty,
                            bottom = false,
                        };
                    }
                }
            }
        }
    }

    /// <summary>
    /// Converts between Character Inclusion abstract domain
    /// and regexes.
    /// </summary>
    public class CharacterInclusionRegex<CharacterSet>
        where CharacterSet : ICharacterSet<CharacterSet>
    {

        /// <summary>
        /// The reference abstract element.
        /// </summary>
        private readonly CharacterInclusion<CharacterSet> value;

        public CharacterInclusionRegex(CharacterInclusion<CharacterSet> value)
        {
            this.value = value;
        }

        /// <summary>
        /// Creates a regular expression for the stored character inclusion.
        /// </summary>
        /// <returns>A single regular expression matching the character inclusion.</returns>
        public IEnumerable<Element> GetRegex()
        {
            List<Element> regexes = new List<Element>();

            if (!value.allowed.IsFull(value.classification.Buckets))
            {
                if (value.allowed.IsEmpty)
                {
                    // Must be empty
                    var concat = new Concatenation();
                    concat.Parts.Add(Anchor.Begin);
                    concat.Parts.Add(Anchor.End);
                    regexes.Add(concat);
                }
                else
                {

                    // Iteration of a set of characters
                    var rangeList = new List<CharRange>();

                    for (int i = 0; i < value.classification.Buckets; ++i)
                    {
                        if (value.allowed.Contains(i))
                        {
                            CharInterval interval = value.classification.ToInterval(i);
                            rangeList.Add(new CharRange(interval.LowerBound, interval.UpperBound));
                        }
                    }
                    CharRanges ranges = new CharRanges(rangeList);

                    var set = new Character(ranges, ranges);

                    int lower = value.MustBeNonEmpty ? 1 : 0;

                    var loop = new Loop(set, lower, Loop.Unbounded);

                    var concat = new Concatenation();
                    concat.Parts.Add(Anchor.Begin);
                    concat.Parts.Add(loop);
                    concat.Parts.Add(Anchor.End);

                    regexes.Add(concat);
                }
            }

            if (!value.mandatory.IsEmpty)
            {
                // For each mandatory class, add a regex
                for (int i = 0; i < value.classification.Buckets; ++i)
                {
                    if (value.mandatory.Contains(i))
                    {
                        CharInterval interval = value.classification.ToInterval(i);
                        CharRanges ranges = new CharRanges(new CharRange(interval.LowerBound, interval.UpperBound));
                        var set = new Character(ranges, ranges);
                        regexes.Add(set);
                    }
                }
            }

            return regexes;
        }

        public ProofOutcome IsMatch(Microsoft.Research.Regex.Model.Element regex)
        {
            // Create forward matching interpreter
            var operations = new CharacterInclusionMatchingOperations<CharacterSet>(this.value);
            var interpretation = new MatchingInterpretation<CharacterInclusionMatchingState<CharacterSet>, CharacterInclusion<CharacterSet>>(operations, this.value);
            var interpreter = new ForwardRegexInterpreter<MatchingState<CharacterInclusionMatchingState<CharacterSet>>>(interpretation);

            var result = interpreter.Interpret(regex);

            bool canMatch = !result.Over.bottom;
            bool mustMatch = result.Under.Accepts(value);

            return ProofOutcomeUtils.Build(canMatch, !mustMatch);
        }

        public CharacterInclusion<CharacterSet> Assume(Microsoft.Research.Regex.Model.Element regex, bool match)
        {
            // Create forward generating interpreter
            IGeneratingOperationsForRegex<CharacterInclusion<CharacterSet>> operations;
            if (match)
                operations = new CharacterInclusionGeneratingOperations<CharacterSet>(value);
            else
                operations = new CharacterInclusionComplementGeneratingOperations<CharacterSet>(value);

            var interpretation = new GeneratingInterpretation<CharacterInclusion<CharacterSet>>(operations);
            var interpreter = new ForwardRegexInterpreter<GeneratingState<CharacterInclusion<CharacterSet>>>(interpretation);

            var result = interpreter.Interpret(regex);
            return result.Open;

        }

        public IStringPredicate PredicateFromRegex<Variable>(Microsoft.Research.Regex.Model.Element regex, Variable thisVar)
                where Variable : class, IEquatable<Variable>
        {
            Contract.Requires(thisVar != null);
            Contract.Requires(regex != null);

            CharacterInclusion<CharacterSet> matchSet = Assume(regex, true);
            CharacterInclusion<CharacterSet> nonMatchSet = Assume(regex, false);

            return StringAbstractionPredicate.For(thisVar, matchSet, nonMatchSet);
        }
    }

}
