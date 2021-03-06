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

using Microsoft.Research.Regex.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Regex;

namespace Microsoft.Research.AbstractDomains.Strings.Regex
{
    /// <summary>
    /// Abstract state for matching regex interpretations.
    /// </summary>
    /// <typeparam name="TState">Abstract state representing a language.</typeparam>
    internal struct MatchingState<TState>
    {
        private readonly TState over, under;

        /// <summary>
        /// Gets the over-approximating abstract state.
        /// </summary>
        public TState Over { get { return over; } }
        /// <summary>
        /// Gets the over-approximating abstract state.
        /// </summary>
        public TState Under { get { return under; } }

        public MatchingState(TState over, TState under)
        {
            this.over = over;
            this.under = under;
        }

        public override string ToString()
        {
            return Under.ToString() + ";" + Over.ToString();
        }
    }

    /// <summary>
    /// Interprets a regex in order to determine whether it can/must match a specified input.
    /// </summary>
    /// <typeparam name="TState">Type of the abstract state used during interpretation.</typeparam>
    /// <typeparam name="TInput">Type of the abstract input.</typeparam>
    internal class MatchingInterpretation<TState, TInput> : IRegexInterpretation<MatchingState<TState>>
    {
        private readonly IMatchingOperationsForRegex<TState, TInput> operations;
        private readonly TInput input;

        /// <summary>
        /// Creates an interpretation for a specified input.
        /// </summary>
        /// <param name="operations">Operations for updating the abstract state.</param>
        /// <param name="input">The abstract input.</param>
        public MatchingInterpretation(IMatchingOperationsForRegex<TState, TInput> operations, TInput input)
        {
            this.operations = operations;
            this.input = input;
        }

        #region IRegexInterpretation<MatchingState<TState>> implementation

        public MatchingState<TState> Bottom
        {
            get
            {
                TState bot = operations.GetBottom(input);
                return new MatchingState<TState>(bot, bot);
            }
        }

        public MatchingState<TState> Top
        {
            get
            {
                TState top = operations.GetTop(input);
                return new MatchingState<TState>(top, top);
            }
        }

        public MatchingState<TState> AddChar(MatchingState<TState> data, CharRanges must, CharRanges can)
        {
            return new MatchingState<TState>(operations.MatchChar(input, data.Over, can, false), operations.MatchChar(input, data.Under, must, true));
        }

        public MatchingState<TState> AssumeEnd(MatchingState<TState> data)
        {
            return new MatchingState<TState>(operations.AssumeEnd(input, data.Over, false), operations.AssumeEnd(input, data.Under, true));
        }

        public MatchingState<TState> AssumeStart(MatchingState<TState> data)
        {
            return new MatchingState<TState>(operations.AssumeStart(input, data.Over, false), operations.AssumeStart(input, data.Under, true));
        }

        public MatchingState<TState> Join(MatchingState<TState> prev, MatchingState<TState> next, bool widen)
        {
            return new MatchingState<TState>(operations.Join(input, prev.Over, next.Over, widen, false), operations.Join(input, prev.Under, next.Under, widen, true));
        }

        public MatchingState<TState> Unknown(MatchingState<TState> data)
        {
            return new MatchingState<TState>(data.Over, operations.GetBottom(input));
        }

        public MatchingState<TState> BeginLoop(MatchingState<TState> prev, IndexInt min, IndexInt max)
        {
            return new MatchingState<TState>(operations.BeginLoop(input, prev.Over, false), operations.BeginLoop(input, prev.Under, true));
        }

        public MatchingState<TState> EndLoop(MatchingState<TState> prev, MatchingState<TState> next, IndexInt min, IndexInt max)
        {
            return new MatchingState<TState>(operations.EndLoop(input, prev.Over, next.Over, min, max, false), operations.EndLoop(input, prev.Under, next.Under, min, max, true));
        }

        public MatchingState<TState> BeginLookaround(MatchingState<TState> prev, bool behind)
        {
            throw new NotImplementedException();
        }

        public MatchingState<TState> EndLookaround(MatchingState<TState> prev, MatchingState<TState> next, bool behind)
        {
            throw new NotImplementedException();
        }
        #endregion
    }

    

}
