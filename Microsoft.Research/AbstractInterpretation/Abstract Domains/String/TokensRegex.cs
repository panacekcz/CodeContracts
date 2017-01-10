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

// Created by Vlastimil Dort (2016)

using Microsoft.Research.AbstractDomains.Strings.PrefixTree;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.AST;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Visitis a regular expression AST and builds a prefix tree.
    /// </summary>
    /// <remarks>
    /// While it may seem good to interpret the regex from back and prepending (or from the front and appending) characters, 
    /// due to the fact, that this domain cannot represent any pattern that contains an entirely unknown part (that means all unanchored patterns),
    /// it really suffices to interpret the closed part, and if it is open, TOP is always returnet (if overapproximating)
    /// </remarks>
    class TokensFromRegex : OpenClosedRegexVisitor<InnerNode, InnerNode>
    {
        private bool underapproximate;
        public TokensFromRegex(bool underapprox)
        {
            this.underapproximate = underapprox;
        }

        protected override InnerNode VisitConcatenation(Concatenation element, int startIndex, int endIndex, RegexEndsData ends, ref InnerNode data)
        {
            throw new NotImplementedException();

        }

        protected override InnerNode Unsupported(Element regex, ref InnerNode data)
        {
            return data = PrefixTreeBuilder.Unknown();
        }

        protected override InnerNode Visit(Loop element, ref InnerNode data)
        {
            InnerNode inner = VisitSimpleRegex(element.Content, ref data);
            PrefixTreeMerger merger = new PrefixTreeMerger();
            new RepeatVisitor(merger).Repeat(inner);

            return merger.Build();
        }

        protected override InnerNode Visit(Concatenation element, ref InnerNode data)
        {
            foreach (var c in element.Parts)
            {
                VisitElement(c, ref data);
            }
            return data;
        }

        protected override InnerNode Visit(Anchor element, ref InnerNode data)
        {
            //TODO:what about start-of string???

            if (element.IsStartAnchor())
            {
                return data = PrefixTreeBuilder.Empty();
            }

            throw new NotImplementedException();
        }

        protected override InnerNode Visit(SingleElement element, ref InnerNode data)
        {
            IEnumerable<Tuple<char,char>> ci;
            if (underapproximate)
                ci = element.MustMatchIntervals;
            else
                ci = element.CanMatchIntervals;

            return data = PrefixTreeBuilder.CharIntervalsNode(ci.Select(t => CharInterval.For(t.Item1, t.Item2)), data);
        }

        protected override InnerNode Visit(Empty element, ref InnerNode data)
        {
            return data = PrefixTreeBuilder.Empty();
        }

        protected override InnerNode Visit(Alternation element, ref InnerNode data)
        {
            //TODO: underapproximate
            //InnerNode bot = PrefixTreeBuilder.Unreached();

            PrefixTreeMerger merger = new PrefixTreeMerger();

            foreach (var e in element.Patterns)
            {
                InnerNode inn = data;
                merger.Cutoff(VisitElement(e, ref inn));
            }

            return data = merger.Build();
        }


        public InnerNode Build(Element element)
        {
            InnerNode end = PrefixTreeBuilder.Unknown();
            return VisitSimpleRegex(element, ref end);
        }


    }

    internal class TokensFromNegativeRegex : OpenClosedRegexVisitor<InnerNode, bool>
    {
        public TokensFromNegativeRegex(bool u) { }
        public InnerNode Build(Regex.AST.Element e) { bool c = false; return VisitSimpleRegex(e, ref c); }

        protected override InnerNode Unsupported(Element regex, ref bool data)
        {
            throw new NotImplementedException();
        }

        protected override InnerNode Visit(SingleElement element, ref bool data)
        {
            throw new NotImplementedException();
        }

        protected override InnerNode Visit(Empty element, ref bool data)
        {
            throw new NotImplementedException();
        }

        protected override InnerNode Visit(Loop element, ref bool data)
        {
            throw new NotImplementedException();
        }

        protected override InnerNode Visit(Alternation element, ref bool data)
        {
            throw new NotImplementedException();
        }

        protected override InnerNode VisitConcatenation(Concatenation element, int startIndex, int endIndex, RegexEndsData ends, ref bool data)
        {
            throw new NotImplementedException();
        }
    }

    internal class TokensRegex
    {
        public static Tokens FromRegex(Element regex, bool underapproximate)
        {
            TokensFromRegex tfr = new TokensFromRegex(underapproximate);

            return new Tokens(tfr.Build(regex));
        }

        public static Tokens FromNegativeRegex(Element regex, bool underapproximate)
        {
            TokensFromNegativeRegex tfr = new TokensFromNegativeRegex(underapproximate);

            return new Tokens(tfr.Build(regex));
        }
    }
}
