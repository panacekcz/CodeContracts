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

using Microsoft.Research.Regex.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Regex
{
    /// <summary>
    /// Interprets regular expressions in a forward direction.
    /// </summary>
    /// <typeparam name="TState">Interpreter data (abstract state).</typeparam>
    internal class ForwardRegexInterpreter<TState> : RegexInterpreter<TState>
    {
        public ForwardRegexInterpreter(IRegexInterpretation<TState> operations) :
            base(operations)
        {
        }

        #region ModelVisitor<Void, TState> overrides
        protected override Void VisitConcatenation(Concatenation element, ref TState data)
        {
            // Visit the parts in forward direction
            foreach (var part in element.Parts)
            {
                VisitElement(part, ref data);
            }
            return null;
        }

        protected override Void VisitAnchor(Begin element, ref TState data)
        {
            data = operations.AssumeStart(data);
            return null;
        }
        protected override Void VisitAnchor(End element, ref TState data)
        {
            data = operations.AssumeEnd(data);
            return null;
        }
        protected override Void VisitLookaround(Lookaround lookaround, ref TState data)
        {
            TState nextData = operations.BeginLookaround(data, lookaround.Behind);
            VisitElement(lookaround.Pattern, ref nextData);
            data = operations.EndLookaround(data, nextData, lookaround.Behind);
            return null;
        }
        #endregion

    }
}
