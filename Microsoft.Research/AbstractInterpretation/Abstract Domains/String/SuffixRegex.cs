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
    /// Generates suffix from the front
    /// </summary>
    public class SuffixOperationsForRegex : LinearGeneratingOperations<Suffix>
    {
        protected override Suffix Extend(Suffix prev, char single)
        {
            return new Suffix(prev.suffix + single);
        }
    }

    public class SuffixMatchingOperations : LinearMatchingOperations<Suffix>
    {
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
    }



    public class SuffixRegex
    {
        private Suffix self;

        public SuffixRegex(Suffix suffix)
        {
            self = suffix;
        }

        // 
        /// <summary>
        /// Computes a suffix which overapproximates all strings matching a regex.
        /// </summary>
        /// <param name="regex">The model of the regex.</param>
        /// <returns>The suffix overapproximating <paramref name="regex"/>.</returns>
        public Suffix AssumeMatch(Element regex)
        {
            SuffixMatchingOperations operations = new SuffixMatchingOperations();
            MatchingInterpretation<LinearMatchingState<Suffix>, Suffix> interpretation = new MatchingInterpretation<LinearMatchingState<Suffix>, Suffix>(operations, this.self);
            BackwardRegexInterpreter<MatchingState<LinearMatchingState<Suffix>>> interpreter = new BackwardRegexInterpreter<MatchingState<LinearMatchingState<Suffix>>>(interpretation);

            var result = interpreter.Interpret(regex);
            return result.Over.currentElement;

        }

        /// <summary>
        /// Verifies whether the suffix matches the specified regex expression.
        /// </summary>
        /// <param name="regex">The model of the regex.</param>
        /// <returns>Proven result of the match.</returns>
        public ProofOutcome IsMatch(Element regex)
        {
            var operations = new SuffixMatchingOperations();
            MatchingInterpretation<LinearMatchingState<Suffix>, Suffix> interpretation = new MatchingInterpretation<LinearMatchingState<Suffix>, Suffix>(operations, this.self);
            BackwardRegexInterpreter<MatchingState<LinearMatchingState<Suffix>>> interpreter = new BackwardRegexInterpreter<MatchingState<LinearMatchingState<Suffix>>>(interpretation);

            var result = interpreter.Interpret(regex);

            bool canMatch = !result.Over.currentElement.IsBottom;
            bool mustMatch = self.LessThanEqual(result.Under.currentElement);

            return ProofOutcomeUtils.Build(canMatch, !mustMatch);

        }

    }
}
