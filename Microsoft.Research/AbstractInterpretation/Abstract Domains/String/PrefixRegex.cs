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
using Microsoft.Research.Regex.AST;
using Microsoft.Research.AbstractDomains.Strings.Regex;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Generates prefix from the back
    /// </summary>
    public class PrefixGeneratingOperations : LinearGeneratingOperations<Prefix>
    {
        protected override Prefix Extend(Prefix prev, char single)
        {
            return new Prefix(single + prev.prefix);
        }
    }

    public class PrefixMatchingOperations : LinearMatchingOperations<Prefix>
    {
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
    }



    /// <summary>
    /// Converts between <see cref="Prefix"/> and regexes.
    /// </summary>
    public class PrefixRegex
    {

        #region Private state
        private readonly Prefix self;
        #endregion

        public PrefixRegex(Prefix self)
        {
            this.self = self;
        }

        public Prefix AssumeMatch(Microsoft.Research.Regex.Model.Element regex)
        {
            PrefixMatchingOperations operations = new PrefixMatchingOperations();
            MatchingInterpretation<LinearMatchingState<Prefix>, Prefix> interpretation = new MatchingInterpretation<LinearMatchingState<Prefix>, Prefix>(operations, this.self);
            ForwardRegexInterpreter<MatchingState<LinearMatchingState<Prefix>>> interpreter = new ForwardRegexInterpreter<MatchingState<LinearMatchingState<Prefix>>>(interpretation);

            var result = interpreter.Interpret(regex);
            return result.Over.currentElement;

        }

        /// <summary>
        /// Verifies whether the prefix matches the specified regex expression.
        /// </summary>
        /// <param name="regex">AST of the regex.</param>
        /// <returns>Proven result of the match.</returns>
        public ProofOutcome IsMatch(Microsoft.Research.Regex.Model.Element regex)
        {
            var operations = new PrefixMatchingOperations();
            MatchingInterpretation<LinearMatchingState<Prefix>, Prefix> interpretation = new MatchingInterpretation<LinearMatchingState<Prefix>, Prefix>(operations, this.self);
            ForwardRegexInterpreter<MatchingState<LinearMatchingState<Prefix>>> interpreter = new ForwardRegexInterpreter<MatchingState<LinearMatchingState<Prefix>>>(interpretation);

            var result = interpreter.Interpret(regex);

            bool canMatch = !result.Over.currentElement.IsBottom;
            bool mustMatch = self.LessThanEqual(result.Under.currentElement);

            return ProofOutcomeUtils.Build(canMatch, !mustMatch);

        }
    }

    }
