using Microsoft.Research.Regex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Regex
{
    interface IRegexInterpretation<D>
    {
        D Top { get; }
        D Bottom { get; }
        D Unknown(D data);
        D AddChar(D data, CharRanges must, CharRanges can);
        D AssumeStart(D data);
        D AssumeEnd(D data);
        D Join(D prev, D next, bool widen);
        D BeginLoop(D prev, IndexInt min, IndexInt max);
        D EndLoop(D prev, D next, IndexInt min, IndexInt max);
    }


    /// <summary>
    /// Implementation of string operations used in regex interpretation.
    /// </summary>
    /// <typeparam name="D">The abstract domain type.</typeparam>
    interface IMatchingOperationsForRegex<D, SD>
    {
        /// <summary>
        /// Evaluates operation of adding a character to a string.
        /// </summary>
        /// <param name="prev">The previous abstract state before the operation.</param>
        /// <param name="next">Possible next character.</param>
        /// <param name="under">If true, underapproximate the state.</param>
        /// <returns>The abstract state after the operation.</returns>
        D MatchChar(SD input, D prev, CharRanges next, bool under);

        /// <summary>
        /// Gets the top element of the domain, representing all states.
        /// </summary>
        D GetTop(SD input);
        /// <summary>
        /// Gets the bottom element of the domain, representing unreachability.
        /// </summary>
        D GetBottom(SD input);
        /// <summary>
        /// Joins two abstract elements of the domain.
        /// </summary>
        /// <param name="left">The first element.</param>
        /// <param name="right">The second element.</param>
        /// <param name="under">If true, underapproximate the state.</param>
        /// <param name="widen">If true, ensure convergence.</param>
        /// <returns>The join of left and right.</returns>
        D Join(SD input, D left, D right, bool under, bool widen);

        /// <summary>
        /// Assumes that we are at the start of the string.
        /// </summary>
        /// <param name="prev">The previous abstract state before the operation.</param>
        /// <param name="under">If true, underapproximate the state.</param>
        /// <returns>The element restricted to empty string.</returns>
        D AssumeStart(SD input, D prev, bool under);

        D AssumeEnd(SD input, D prev, bool under);

        D BeginLoop(SD input, D prev, bool under);
        D EndLoop(SD input, D prev, D next, IndexInt min, IndexInt max, bool under);
    }

    interface IGeneratingOperationsForRegex<D>
    {
        bool IsUnderapproximating { get; }

        D Top { get; }
        D Bottom { get; }

        D Empty { get; }
        bool CanBeEmpty(D prev);

        D AddChar(D prev, CharRanges next, bool closed);
        D Join(D left, D right, bool widen);

        D Loop(D prev, D loop, D last, IndexInt min, IndexInt max);
    }

}
