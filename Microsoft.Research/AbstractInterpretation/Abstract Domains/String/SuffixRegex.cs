// CodeContracts
// 
// Copyright (c) Microsoft Corporation
// Copyright (c) Charles University
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
    /// Implements regex matching operations for the Suffix domain.
    /// </summary>
    internal class SuffixMatchingOperations : LinearMatchingOperations<Suffix>
    {
        #region LinearMatchingOperations<Suffix> overrides
        protected override Suffix Extend(Suffix prev, char single)
        {
            return new Suffix(single + prev.suffix);
        }
        protected override int GetLength(Suffix element)
        {
            return element.suffix.Length;
        }
        protected override bool IsCompatible(Suffix element, int index, CharRanges ranges)
        {
            return ranges.Contains(element.suffix[element.suffix.Length - index - 1]);
        }

        protected override Suffix JoinUnder(Suffix prev, Suffix next)
        {
            if (prev.IsBottom)
                return next;
            if (next.IsBottom)
                return prev;
            return prev.suffix.Length > next.suffix.Length ? next : prev;
        }
        #endregion
    }


    /// <summary>
    /// Provides regex-related functionality for the Suffix domain.
    /// </summary>
    public class SuffixRegex
    {
        private Suffix value;

        public SuffixRegex(Suffix suffix)
        {
            value = suffix;
        }

        /// <summary>
        /// Creates a regular expression for the stored suffix.
        /// </summary>
        /// <returns>A single regular expression matching the suffix.</returns>
        public IEnumerable<Element> GetRegex()
        {
            //Sequence of characters followed by anchor
            Concatenation sequence = new Concatenation();
            foreach (char c in value.suffix)
                sequence.Parts.Add(new Character(c));
            sequence.Parts.Add(Anchor.End);
            return new Element[] { sequence };
        }

        /// <summary>
        /// Computes a suffix which overapproximates all strings matching a regex.
        /// </summary>
        /// <param name="regex">The model of the regex.</param>
        /// <returns>The suffix overapproximating <paramref name="regex"/>.</returns>
        public Suffix AssumeMatch(Element regex)
        {
            var operations = new SuffixMatchingOperations();
            var interpretation = new MatchingInterpretation<LinearMatchingState<Suffix>, Suffix>(operations, this.value);
            var interpreter = new BackwardRegexInterpreter<MatchingState<LinearMatchingState<Suffix>>>(interpretation);

            var result = interpreter.Interpret(regex);
            return result.Over.currentElement;

        }

        /// <summary>
        /// Verifies whether the suffix matches the specified regex.
        /// </summary>
        /// <param name="regex">The model of the regex.</param>
        /// <returns>Proven result of the match.</returns>
        public ProofOutcome IsMatch(Element regex)
        {
            var operations = new SuffixMatchingOperations();
            var interpretation = new MatchingInterpretation<LinearMatchingState<Suffix>, Suffix>(operations, this.value);
            var interpreter = new BackwardRegexInterpreter<MatchingState<LinearMatchingState<Suffix>>>(interpretation);

            var result = interpreter.Interpret(regex);

            bool canMatch = !result.Over.currentElement.IsBottom;
            bool mustMatch = value.LessThanEqual(result.Under.currentElement);

            return ProofOutcomeUtils.Build(canMatch, !mustMatch);
        }
    }
}
