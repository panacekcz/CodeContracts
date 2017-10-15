// CodeContracts
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

using Microsoft.Research.Regex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Regex
{
    /// <summary>
    /// Performs interpretation of a regex.
    /// </summary>
    /// <typeparam name="TData">Type of the abstract state.</typeparam>
    interface IRegexInterpretation<TData>
    {
        /// <summary>
        /// Gets the top abstract state.
        /// </summary>
        TData Top { get; }
        /// <summary>
        /// Gets the bottom abstract state.
        /// </summary>
        TData Bottom { get; }

        TData Unknown(TData data);
        /// <summary>
        /// Joins two abstract states.
        /// </summary>
        /// <param name="prev">The abstract state before join.</param>
        /// <param name="next">The abstract state to be added.</param>
        /// <param name="widen">Whether to ensure termination.</param>
        /// <returns>Joined state of <paramref name="prev"/> and <paramref name="next"/>.</returns>
        TData Join(TData prev, TData next, bool widen);

        TData AddChar(TData data, CharRanges must, CharRanges can);

        TData AssumeStart(TData data);
        TData AssumeEnd(TData data);
        
        TData BeginLoop(TData prev, IndexInt min, IndexInt max);
        TData EndLoop(TData prev, TData next, IndexInt min, IndexInt max);

        TData BeginLookaround(TData prev, bool behind);
        TData EndLookaround(TData prev, TData next, bool behind);
    }


    /// <summary>
    /// Implementation of string operations used in regex interpretation.
    /// </summary>
    /// <typeparam name="TData">The abstract domain type.</typeparam>
    interface IMatchingOperationsForRegex<TData, TInput>
    {
        /// <summary>
        /// Evaluates operation of adding a character to a string.
        /// </summary>
        /// <param name="prev">The previous abstract state before the operation.</param>
        /// <param name="next">Possible next character.</param>
        /// <param name="under">If true, underapproximate the state.</param>
        /// <returns>The abstract state after the operation.</returns>
        TData MatchChar(TInput input, TData prev, CharRanges next, bool under);

        /// <summary>
        /// Gets the top element of the domain, representing all states.
        /// </summary>
        TData GetTop(TInput input);
        /// <summary>
        /// Gets the bottom element of the domain, representing unreachability.
        /// </summary>
        TData GetBottom(TInput input);
        /// <summary>
        /// Joins two abstract elements of the domain.
        /// </summary>
        /// <param name="left">The first element.</param>
        /// <param name="right">The second element.</param>
        /// <param name="under">If true, underapproximate the state.</param>
        /// <param name="widen">If true, ensure convergence.</param>
        /// <returns>The join of left and right.</returns>
        TData Join(TInput input, TData left, TData right, bool widen, bool under);

        /// <summary>
        /// Assumes that we are at the start of the string.
        /// </summary>
        /// <param name="prev">The previous abstract state before the operation.</param>
        /// <param name="under">If true, underapproximate the state.</param>
        /// <returns>The element restricted to empty string.</returns>
        TData AssumeStart(TInput input, TData prev, bool under);

        TData AssumeEnd(TInput input, TData prev, bool under);

        TData BeginLoop(TInput input, TData prev, bool under);
        TData EndLoop(TInput input, TData prev, TData next, IndexInt min, IndexInt max, bool under);
    }

    public struct GeneratingLoopState<TData>
    {
        public TData loopOpen;
        public TData loopClosed;
        public bool resultClosed;

        public TData Last {
            get {
                return resultClosed ? loopClosed : loopOpen;
            }
        }
    }


    /// <summary>
    /// Provides abstract operations for generating a language from a regex.
    /// </summary>
    /// <typeparam name="TData">The abstract state representing the generated language.</typeparam>
    interface IGeneratingOperationsForRegex<TData>
    {
        /// <summary>
        /// True if the operations ensure that the state is an underapproximation of the generated language,
        /// false if it is an overapproximation. 
        /// </summary>
        bool IsUnderapproximating { get; }

        /// <summary>
        /// Gets the top element (representing all strings).
        /// </summary>
        TData Top { get; }
        /// <summary>
        /// Gets the bottom element (representing no strings).
        /// </summary>
        TData Bottom { get; }

        /// <summary>
        /// Gets an element representing an empty string.
        /// </summary>
        TData Empty { get; }
        /// <summary>
        /// Decidest whether an empty string can be generated.
        /// </summary>
        /// <param name="data">An abstract state.</param>
        /// <returns>True, if an empty string is contained in the language represented by data.</returns>
        bool CanBeEmpty(TData data);

        TData AddChar(TData prev, CharRanges next, bool closed);
        TData Join(TData left, TData right, bool widen);

        TData Loop(TData prev, GeneratingLoopState<TData> loop, IndexInt min, IndexInt max);
    }

}
