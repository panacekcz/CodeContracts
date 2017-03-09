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

using Microsoft.Research.AbstractDomains.Strings.Regex;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    public abstract class LinearGeneratingOperations<D> : IGeneratingOperationsForRegex<D>
    where D : IStringAbstraction<D>
    {
        private const bool under = false;
        D factoryElement;

        protected abstract D Extend(D prev, char single);

        public bool IsUnderapproximating
        {
            get
            {
                return under;
            }
        }
        public D Bottom
        {
            get
            {
                return factoryElement.Bottom;
            }
        }

        public D Top
        {
            get
            {
                return factoryElement.Top;
            }
        }

        public D AddChar(D prev, CharRanges next, bool closed)
        {
            if (closed && (under || prev.IsBottom))
                return prev.Bottom;
            else if (prev.IsBottom)
                return prev;

            char single;
            if (next.TryGetSingleton(out single))
            {
                return Extend(prev, single);
            }
            else
            {
                return (!closed && under) ? prev.Bottom : prev;
            }
        }

        public D AssumeStart(D prev)
        {
            if (under || !prev.IsTop)
                return prev.Bottom;
            else
                return prev;
        }

        public D Empty
        {
            get
            {
                if (under)
                    return factoryElement.Bottom;
                else
                    return factoryElement.Top;
            }
        }
        public bool CanBeEmpty(D p)
        {
            return p.IsTop;
        }

        public D Join(D left, D right, bool widen)
        {
            return left.Join(right);
        }

        public D Loop(D prev, D loop, D last, IndexInt min, IndexInt max)
        {
            if (min == 0)
                return prev;
            else
                //TODO: here could return more if there would be BeginLoop...
                return prev;
        }
    }


    /// <summary>
    /// Utility methods for working with <see cref="IndeInt"/>.
    /// </summary>
    internal static class IndexUtils
    {
        public static IndexInt JoinIndices(IndexInt a, IndexInt b)
        {
            if (a.IsNegative || b.IsInfinite)
                return b;
            if (b.IsNegative || a.IsInfinite)
                return a;

            if (a == b)
                return a;
            else
                return IndexInt.Infinity;
        }
        public static IndexInt MeetIndices(IndexInt a, IndexInt b)
        {
            if (a.IsNegative || b.IsInfinite)
                return a;
            if (b.IsNegative || a.IsInfinite)
                return b;

            if (a == b)
                return a;
            else
                return IndexInt.Negative;
        }
    }

    internal struct LinearMatchingState<D>
    {
        internal D currentElement;
        internal IndexInt currentIndex;

        public LinearMatchingState(D element, IndexInt index)
        {
            currentElement = element;
            currentIndex = index;
        }
    }


    /// <summary>
    /// Matches prefix against regex
    /// </summary>
    internal abstract class LinearMatchingOperations<D> : IMatchingOperationsForRegex<LinearMatchingState<D>, D>
        where D : IStringAbstraction<D>
    {
        public LinearMatchingState<D> GetBottom(D input)
        {
            // In over, We guarantee no match
            return new LinearMatchingState<D>(input.Bottom, IndexInt.Negative);

        }

        public LinearMatchingState<D> GetTop(D input)
        {
            // In under, We guarantee match on all inputs on all indices
            return new LinearMatchingState<D>(input, IndexInt.Infinity);
        }

        protected abstract D Extend(D prev, char single);
        protected abstract int GetLength(D element);
        protected abstract bool IsCompatible(D element, int index, CharRanges ranges);
        protected abstract D JoinUnder(D prev, D next);

        public LinearMatchingState<D> MatchChar(D input, LinearMatchingState<D> data, CharRanges range, bool under)
        {
            if (data.currentIndex.IsInfinite)
            {
                if (under)
                {
                    // Garanteed match at all indices -> fix the index to the first matching character

                    //TODO: VD: should use input or currentprefix?
                    for (int i = 0; i < GetLength(input); ++i)
                    {
                        if (IsCompatible(input, i, range))
                        {
                            return new LinearMatchingState<D> (data.currentElement, IndexInt.For(i));
                        }
                    }

                    // Character not found -> match not guaranteed
                    return GetBottom(input);
                }
                else
                {
                    return data;
                }
            }
            else if (data.currentIndex.IsNegative)
            {
                if (under)
                {
                    // Match at an unknown index ->can only guarantee match if all chars guaranteed to match AND we know we are not beyond the end
                    return GetBottom(input);
                }
                else
                {
                    return data;
                }
            }

            // Guaranteed match at a specific index
            int index = data.currentIndex.AsInt;
            char singleton = default(char);

            if (index < GetLength(input))
            {
                if (!IsCompatible(input, index, range))
                {
                    // Matching character that is not there.
                    return GetBottom(input);
                }
                else
                {
                    return new LinearMatchingState<D>(data.currentElement, data.currentIndex.Add(1));
                }
            }
            else if (GetLength(data.currentElement) == index && (under ? range.TryGetFirst(out singleton) : range.TryGetSingleton(out singleton)))
            {
                // We cannot gurantee match for the prefix, but we can guarantee it if the input has a longer prefix
                // If there are more chars guaranteed to match, we can select any of them
                return new LinearMatchingState<D>(Extend(data.currentElement, singleton), data.currentIndex.Add(1));
            }
            else
            {
                // Again - we cannot guarantee match because we do not know whether there are more characters and which ones.
                return under ? GetBottom(input) : new LinearMatchingState<D>(data.currentElement, data.currentIndex.Add(1));
            }
        }

        public LinearMatchingState<D> AssumeEnd(D input, LinearMatchingState<D> data, bool under)
        {
            if (under)
            {
                // If match is guaranteed on all indices, it is guaranteed on some index
                if (data.currentIndex.IsInfinite)
                    return new LinearMatchingState<D>(data.currentElement, IndexInt.Negative);
                else
                    // No guarantee
                    return GetBottom(input);
            }
            else
            {
                //over
                return data;
            }
        }

        public LinearMatchingState<D> AssumeStart(D input, LinearMatchingState<D> data, bool under)
        {
            // In under: If guranteed on all indices, it is guranteed at the start
            if (data.currentIndex.IsInfinite || data.currentIndex == 0)
                return new LinearMatchingState<D>(data.currentElement, IndexInt.For(0));
            else
                return GetBottom(input);
        }

        public LinearMatchingState<D> Join(D input, LinearMatchingState<D> prev, LinearMatchingState<D> next, bool widen, bool under)
        {
            // In under:
            // If one of them is infinite index, then we return JOIN of strings and the other index
            // If both indexes are the same int, then we JOIN the string and use the index
            // If both indexes are different ints, then we JOIN the string and use bottom index

            var index = under ? IndexUtils.MeetIndices(prev.currentIndex, next.currentIndex) :
                IndexUtils.JoinIndices(prev.currentIndex, next.currentIndex);

            var str = under ? JoinUnder(prev.currentElement, next.currentElement) : prev.currentElement.Join(next.currentElement);

            return new LinearMatchingState<D>(str, index);
        }

        public LinearMatchingState<D> BeginLoop(D input, LinearMatchingState<D> prev, bool under)
        {
            return prev;
        }

        public LinearMatchingState<D> EndLoop(D input, LinearMatchingState<D> prev, LinearMatchingState<D> next, IndexInt min, IndexInt max, bool under)
        {
            if (under)
            {
                if (min == 0)
                    return prev;
                else if (max >= 1)
                    //Underapproximate by considering only the first iteration
                    return next;
                else
                    return GetBottom(input);
            }
            else
            {
                //TODO: could be better, use widneing
                return GetTop(input);
            }
        }
    }
}
