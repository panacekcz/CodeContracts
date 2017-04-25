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

using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.Model;
using Microsoft.Research.AbstractDomains.Strings.Regex;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Implements regex matching operations for the Prefix domain.
    /// </summary>
    internal class PrefixMatchingOperations : LinearMatchingOperations<Prefix>
    {
        #region LinearMatchingOperations<Prefix> overrides
        protected override Prefix Extend(Prefix prev, char single)
        {
            return new Prefix(prev.prefix + single);
        }
        protected override int GetLength(Prefix element)
        {
            return element.prefix.Length;
        }
        protected override bool IsCompatible(Prefix element, int index, CharRanges ranges)
        {
            return ranges.Contains(element.prefix[index]);
        }
        protected override Prefix JoinUnder(Prefix prev, Prefix next)
        {
            if (prev.IsBottom)
                return next;
            if (next.IsBottom)
                return prev;
            return prev.prefix.Length > next.prefix.Length ? next : prev;
        }
        #endregion
    }

    /// <summary>
    /// Provides regex-related functionality for the <see cref="Prefix"/> domain.
    /// </summary>
    public class PrefixRegex
    {

        #region Private state
        private readonly Prefix value;
        #endregion

        public PrefixRegex(Prefix value)
        {
            this.value = value;
        }

        /// <summary>
        /// Creates a regular expression for the stored prefix.
        /// </summary>
        /// <returns>A single regular expression matching the prefix.</returns>
        public IEnumerable<Element> GetRegex()
        {
            // Sequence of characters preceded by anchor
            Concatenation sequence = new Concatenation();
            sequence.Parts.Add(Anchor.Begin);
            foreach(char c in value.prefix)
                sequence.Parts.Add(new Character(c));

            return new Element[] { sequence };
        }

        /// <summary>
        /// Computes a prefix which overapproximates all strings matching a regex.
        /// </summary>
        /// <param name="regex">The model of the regex.</param>
        /// <returns>The suffix overapproximating <paramref name="regex"/>.</returns>
        public Prefix AssumeMatch(Element regex)
        {
            PrefixMatchingOperations operations = new PrefixMatchingOperations();
            var interpretation = new MatchingInterpretation<LinearMatchingState<Prefix>, Prefix>(operations, this.value);
            var interpreter = new ForwardRegexInterpreter<MatchingState<LinearMatchingState<Prefix>>>(interpretation);

            var result = interpreter.Interpret(regex);
            return result.Over.currentElement;
        }

        /// <summary>
        /// Verifies whether the prefix matches the specified regex.
        /// </summary>
        /// <param name="regex">Model of the regex.</param>
        /// <returns>Proven result of the match.</returns>
        public ProofOutcome IsMatch(Microsoft.Research.Regex.Model.Element regex)
        {
            var operations = new PrefixMatchingOperations();
            var interpretation = new MatchingInterpretation<LinearMatchingState<Prefix>, Prefix>(operations, this.value);
            var interpreter = new ForwardRegexInterpreter<MatchingState<LinearMatchingState<Prefix>>>(interpretation);

            var result = interpreter.Interpret(regex);

            bool canMatch = !result.Over.currentElement.IsBottom;
            bool mustMatch = value.LessThanEqual(result.Under.currentElement);

            return ProofOutcomeUtils.Build(canMatch, !mustMatch);

        }
    }
}
