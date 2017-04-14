// CodeContracts
// 
// Copyright (c) Charles University (2016-2017)
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

using Microsoft.Research.Regex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Regex.Model;

namespace Microsoft.Research.AbstractDomains.Strings.Regex
{
    /// <summary>
    /// Abstract state for generating regex interpretations.
    /// </summary>
    /// <typeparam name="TState">Abstract state representing the generated language.</typeparam>
    internal struct GeneratingState<TState>
    {
        /// <summary>
        /// The abstract state representing a language of strings where a match exists.
        /// </summary>
        public TState Open { get; private set; }
        /// <summary>
        /// The abstract state representing a language of strings where the match ends at
        /// the current position.
        /// </summary>
        public TState Closed { get; private set; }

        public bool IsEnd { get; private set; }

        public GeneratingState(TState open, TState closed, bool isEnd)
        {
            Open = open;
            Closed = closed;
            IsEnd = isEnd;
        }
        public GeneratingState(TState both, bool isEnd)
        {
            Open = both;
            Closed = both;
            IsEnd = isEnd;
        }

        public override string ToString()
        {
            return Open.ToString() + ";" + Closed.ToString();
        }
    }


    /// <summary>
    /// Interpretation of regex that generates a corresponding abstract element.
    /// </summary>
    /// <typeparam name="TState">Type of the abstract element.</typeparam>
    internal class GeneratingInterpretation<TState> : IRegexInterpretation<GeneratingState<TState>>
    {
        private readonly IGeneratingOperationsForRegex<TState> operations;

        public GeneratingInterpretation(IGeneratingOperationsForRegex<TState> operations)
        {
            this.operations = operations;
        }

        #region IRegexInterpretation<GeneratingState<TState>> implementation
        public GeneratingState<TState> Bottom
        {
            get
            {
                return new GeneratingState<TState>(operations.Bottom, true);
            }
        }

        public GeneratingState<TState> Top
        {
            get
            {
                return new GeneratingState<TState>(operations.Top, false);
            }
        }

        public GeneratingState<TState> AddChar(GeneratingState<TState> prev, CharRanges must, CharRanges can)
        {
            if (prev.IsEnd)
                return Bottom;

            CharRanges ranges = operations.IsUnderapproximating ? must : can;
            return new GeneratingState<TState>(operations.AddChar(prev.Closed, ranges, false), operations.AddChar(prev.Closed, ranges, true), false);
        }

        public GeneratingState<TState> AssumeEnd(GeneratingState<TState> prev)
        {
            return new GeneratingState<TState>(prev.Closed, true);
        }

        public GeneratingState<TState> AssumeStart(GeneratingState<TState> prev)
        {
            TState d = prev.Closed;

            if (operations.CanBeEmpty(d))
            {
                return new GeneratingState<TState>(operations.Top, operations.Empty, prev.IsEnd);
            }
            else
            {
                return Bottom;
            }
        }

        public GeneratingState<TState> BeginLookaround(GeneratingState<TState> prev, bool behind)
        {
            return Bottom;
        }

        public GeneratingState<TState> BeginLoop(GeneratingState<TState> prev, IndexInt min, IndexInt max)
        {
            return new GeneratingState<TState>(operations.Top, operations.Empty, false);
        }

        public GeneratingState<TState> EndLookaround(GeneratingState<TState> prev, GeneratingState<TState> next, bool behind)
        {
            if (operations.IsUnderapproximating)
                return Bottom;
            else
                return prev;
        }

        public GeneratingState<TState> EndLoop(GeneratingState<TState> prev, GeneratingState<TState> next, IndexInt min, IndexInt max)
        {
            TState loopedOpen = operations.Loop(prev.Closed, next.Closed, next.Open, min, max);
            TState loopedClosed = operations.Loop(prev.Closed, next.Closed, next.Closed, min, max);

            if (min == 0 || operations.CanBeEmpty(next.Closed))
            {
                loopedOpen = operations.Join(prev.Open, loopedOpen, false);
            }

            return new GeneratingState<TState>(loopedOpen, loopedClosed, false);
            //TODO: VD: check code above (including isEnd)
            //throw new NotImplementedException();
        }

        public GeneratingState<TState> Join(GeneratingState<TState> left, GeneratingState<TState> right, bool widen)
        {
            var open = operations.Join(left.Open, right.Open, widen);
            var closed = operations.Join(left.Closed, right.Closed, widen);

            var isEnd = operations.IsUnderapproximating ? left.IsEnd || right.IsEnd : left.IsEnd && right.IsEnd;

            return new GeneratingState<TState>(open, closed, isEnd);
        }

        public GeneratingState<TState> Unknown(GeneratingState<TState> data)
        {
            return operations.IsUnderapproximating ? Bottom : data;
        }
        #endregion
    }
}
