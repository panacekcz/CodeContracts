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
using Microsoft.Research.Regex.AST;

namespace Microsoft.Research.Regex
{
#if vdfalse
    /// <summary>
    /// Stores information about whether the ends of regex are open 
    /// or closed when visiting regex trees.
    /// </summary>
    public struct RegexEndsData
    {
        private readonly bool leftClosed, rightClosed;
        /// <summary>
        /// Whether the regex is closed on the left end (the match is at the start).
        /// </summary>
        public bool LeftClosed
        {
            get { return leftClosed; }
        }
        /// <summary>
        /// Whether the regex is closed on the right end (the match is at the end).
        /// </summary>
        public bool RightClosed
        {
            get { return rightClosed; }
        }

        public RegexEndsData(bool leftClosed, bool rightClosed)
        {
            this.leftClosed = leftClosed;
            this.rightClosed = rightClosed;
        }

    }

    /// <summary>
    /// Visits the AST of a regex limited to concatenation, union, loops
    /// and character sets, trying to find anchors in concatenation nodes, to
    /// distinguish, whether the regular expression is closed (matches the whole
    /// string), or open (can match just a part of it).
    /// </summary>
    /// <typeparam name="Result">The type of result passed bottom up.</typeparam>
    /// <typeparam name="Data">The type of data passed along the traversal.</typeparam>
    public abstract class OpenClosedRegexVisitor<Result, Data> : SimpleRegexVisitor<Result, Data>
    {
        protected abstract Result VisitConcatenation(Concatenation element, int startIndex, int endIndex, RegexEndsData ends, ref Data data);

        protected RegexEndsData ConcatChildEnds(RegexEndsData outer, RegexEndsData inner, int startIndex, int endIndex, int index)
        {
            bool leftClosed = true, rightClosed = true;

            if (index == startIndex)
            {
                leftClosed = outer.LeftClosed | inner.LeftClosed;
            }
            if (index == endIndex - 1)
            {
                rightClosed = outer.RightClosed | inner.RightClosed;
            }

            return new RegexEndsData(leftClosed, rightClosed);
        }

        /// <inheritdoc/>
        protected override Result Visit(Concatenation element, ref Data data)
        {
            StringBuilder val = new StringBuilder();

            int startIndex = 0, endIndex = element.Parts.Count;

            bool closedStart = false, closedEnd = false;

            if (element.Parts.Count >= 1)
            {
                if (element.Parts[0].IsStartAnchor())
                {
                    ++startIndex;
                    closedStart = true;
                }
                if (element.Parts[endIndex - 1].IsEndAnchor())
                {
                    --endIndex;
                    closedEnd = true;
                }
            }

            return VisitConcatenation(element, startIndex, endIndex, new RegexEndsData(closedStart, closedEnd), ref data);
        }

        /// <inheritdoc/>
        protected override Result Visit(Anchor element, ref Data data)
        {
            return Unsupported(element, ref data);
        }

    }
#endif
}
