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

using Microsoft.Research.AbstractDomains.Strings.TokensTree;
using Microsoft.Research.AbstractDomains.Strings.Regex;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Abstract state for generating Tokens from regex.
    /// </summary>
    internal struct TokensRegexState
    {
        internal InnerNode prefix;
        internal InnerNode result;

        public TokensRegexState(InnerNode prefix, InnerNode result)
        {
            this.prefix = prefix;
            this.result = result;    
        }

        public override string ToString()
        {
            return prefix.ToString() + result.ToString();
        }
    }


    /// <summary>
    /// Visitis a regular expression AST and builds a prefix tree.
    /// </summary>
    /// <remarks>
    /// While it may seem good to interpret the regex from back and prepending (or from the front and appending) characters, 
    /// due to the fact, that this domain cannot represent any pattern that contains an entirely unknown part (that means all unanchored patterns),
    /// it really suffices to interpret the closed part, and if it is open, TOP is always returned (if overapproximating)
    /// </remarks>
    class TokensRegexOperations : IGeneratingOperationsForRegex<TokensRegexState>
    {
        private readonly bool underapproximate;
        private readonly InnerNode emptyNode, topNode;

        public TokensRegexOperations(bool underapproximate)
        {
            this.underapproximate = underapproximate;
            emptyNode = TokensTreeBuilder.Empty();
            topNode = TokensTreeBuilder.Unknown();
        }


        public InnerNode GetResult(TokensRegexState state)
        {
            // Makes a single result from the two parts
            // If one of the parts is empty, return the other one
            if (state.prefix.IsEmpty())
                return state.result;
            else if (state.result.IsEmpty())
                return state.prefix;
            else if (underapproximate)
            {
                //If underapproximating, cannot join the parts, but if one of them can 
                // be empty, may assume that it is empty
                if (state.prefix.Accepting)
                    return state.result;
                else if (state.result.Accepting)
                    return state.prefix;
                else
                    return TokensTreeBuilder.Unreached();
            }
            else
            {
                // Otherwise, repeat the first part and join with the other one
                TokensTreeMerger merger = new TokensTreeMerger();
                RepeatVisitor rv = new RepeatVisitor(merger);
                rv.Repeat(state.prefix);
                merger.Cutoff(state.result);
                return merger.Build();
            }
        }

        #region IGeneratingOperationsForRegex<TRState> implementation

        public bool IsUnderapproximating
        {
            get { return underapproximate; }
        }

        public TokensRegexState Loop(TokensRegexState prev, TokensRegexState loop, TokensRegexState last, IndexInt min, IndexInt max)
        {
 
            if (underapproximate)
            {
                //Only allow unlimited repetition
                if(min != 0 || !max.IsInfinite)
                    return Bottom;

                InnerNode prevResult = GetResult(prev);
                InnerNode loopResult = GetResult(loop);

                UnderapproximatingMerger merger = new UnderapproximatingMerger();
                merger.Cutoff(prevResult);
                merger.Cutoff(loopResult);

                return new TokensRegexState(emptyNode, merger.Build());
            }
            else
            {
                // Repeat the loop and append the suffix
                TokensTreeMerger merger = new TokensTreeMerger();
                var rv = new RepeatVisitor(merger);

                rv.Repeat(last.prefix);
                rv.Repeat(last.result);

                if (prev.result.IsEmpty())
                {
                    merger.Cutoff(prev.prefix);
                }
                else
                {
                    rv.Repeat(prev.prefix);
                    merger.Cutoff(prev.result);
                }

                return new TokensRegexState(emptyNode, merger.Build());
            }
        }

        public TokensRegexState AddChar(TokensRegexState prev, CharRanges ranges, bool closed)
        {
            // If underapproximating, assume the end is empty, otherwise it can contain anything, so cannot return anything better than top
            if (!closed && !underapproximate)
                return Top;

            return new TokensRegexState(TokensTreeBuilder.PrependCharIntervals(ranges.ToIntervals(), prev.prefix), prev.result);
        }

        public bool CanBeEmpty(TokensRegexState state)
        {
            return state.prefix.accepting && state.result.accepting;
        }

        public TokensRegexState Empty
        {
            get
            {
                return new TokensRegexState(emptyNode, emptyNode);
            }
        }

        public TokensRegexState Top
        {
            get
            {
                return new TokensRegexState(emptyNode, topNode);
            }
        }
        public TokensRegexState Bottom
        {
            get
            {
                return new TokensRegexState(TokensTreeBuilder.Unreached(), TokensTreeBuilder.Unreached());
            }
        }

        public TokensRegexState Join(TokensRegexState prev, TokensRegexState next, bool widen)
        {
            if (underapproximate)
            {
                InnerNode prevNode = GetResult(prev);
                InnerNode nextNode = GetResult(next);

                UnderapproximatingMerger merger = new UnderapproximatingMerger();
                merger.Cutoff(prevNode);
                merger.Cutoff(nextNode);
                return new TokensRegexState(emptyNode, merger.Build());
            }
            else
            {
                // Avoid merging empty parts with non-empty parts
                if (prev.prefix.IsEmpty() && next.result.IsEmpty())
                    next = new TokensRegexState(emptyNode, GetResult(next));
                else if (next.prefix.IsEmpty() && prev.result.IsEmpty())
                    prev = new TokensRegexState(emptyNode, GetResult(prev));

                TokensTreeMerger mergerPrefix = new TokensTreeMerger();
                mergerPrefix.Cutoff(prev.prefix);
                mergerPrefix.Cutoff(next.prefix);

                TokensTreeMerger mergerResult = new TokensTreeMerger();
                mergerResult.Cutoff(prev.result);
                mergerResult.Cutoff(next.result);

                return new TokensRegexState(mergerPrefix.Build(), mergerResult.Build());
            }
        }

        #endregion
    }


    internal struct TokensNegativeRegexState
    {
        internal readonly InnerNode root;
        internal readonly bool isOpenStart;
        internal readonly bool isOpenEnd;

        public TokensNegativeRegexState(InnerNode root, bool isOpenStart, bool isOpenEnd)
        {
            this.root = root;
            this.isOpenStart = isOpenStart;
            this.isOpenEnd = isOpenEnd;
        }

    }

   
    internal class TokensNegativeRegexOperations : IGeneratingOperationsForRegex<TokensNegativeRegexState>
    {
        private readonly bool underapproximate;
        public TokensNegativeRegexOperations(bool underapproximate) {
            this.underapproximate = underapproximate;
        }


        public InnerNode GetResult(TokensNegativeRegexState state)
        {
            if (!state.isOpenStart || !state.isOpenEnd)
                return underapproximate ? TokensTreeBuilder.Unknown() : TokensTreeBuilder.Unreached();
            else
            {
                return ComplementVisitor.Complement(state.root);
            }
        }

        public TokensNegativeRegexState Bottom
        {
            get
            {
                return new TokensNegativeRegexState(TokensTreeBuilder.Unreached(), true, true);
            }
        }

        public TokensNegativeRegexState Empty
        {
            get
            {
                return new TokensNegativeRegexState(TokensTreeBuilder.Empty(), false, false);
            }
        }

        public bool IsUnderapproximating
        {
            get
            {
                return underapproximate;
            }
        }

        public TokensNegativeRegexState Top
        {
            get
            {
                return new TokensNegativeRegexState(TokensTreeBuilder.Empty(), true, true);
            }
        }

        public TokensNegativeRegexState AddChar(TokensNegativeRegexState prev, CharRanges ranges, bool closed)
        {
            if (prev.root.IsBottom())
                return prev;

            return new TokensNegativeRegexState(
                TokensTreeBuilder.PrependCharIntervals(ranges.ToIntervals(), prev.root), prev.isOpenStart, !closed
                );
        }

        public bool CanBeEmpty(TokensNegativeRegexState prev)
        {
            return !underapproximate;
        }

        public TokensNegativeRegexState Join(TokensNegativeRegexState left, TokensNegativeRegexState right, bool widen)
        {
            // The tree does not contain any repeat nodes, thst means merging works for both over- and under- approximation

            TokensTreeMerger merger = new TokensTreeMerger();
            merger.Cutoff(left.root);
            merger.Cutoff(right.root);

            return new TokensNegativeRegexState(merger.Build(), left.isOpenStart && right.isOpenStart, left.isOpenEnd && right.isOpenEnd);
        }

        public TokensNegativeRegexState Loop(TokensNegativeRegexState prev, TokensNegativeRegexState loop, TokensNegativeRegexState last, IndexInt min, IndexInt max)
        {
            return underapproximate ? Bottom : Top;
        }
    }

    /// <summary>
    /// Provides regex-related functionality to the Tokens abstract domain.
    /// </summary>
    public class TokensRegex
    {
        /// <summary>
        /// Creates tokens abstract element for a regular expression.
        /// </summary>
        /// <param name="regex">Model of the regular expression.</param>
        /// <param name="underapproximate">If true, the result contains only matching strings (or less).
        /// If false, the result contains all matching strings (or more).</param>
        /// <returns>A tokens element which is an over- or under- approximation of the language matching <paramref name="regex"/>.</returns>
        public static Tokens TokensForRegex(Element regex, bool underapproximate)
        {
            TokensRegexOperations operations = new TokensRegexOperations(underapproximate);
            GeneratingInterpretation<TokensRegexState> interpretation = new GeneratingInterpretation<TokensRegexState>(operations);
            BackwardRegexInterpreter<GeneratingState<TokensRegexState>> interpreter = new BackwardRegexInterpreter<GeneratingState<TokensRegexState>>(interpretation);

            var result = interpreter.Interpret(regex);

            return new Tokens(operations.GetResult(result.Open));
        }

        /// <summary>
        /// Creates tokens abstract element for a negative regular expression.
        /// </summary>
        /// <param name="regex">Model of the regular expression.</param>
        /// <param name="underapproximate">If true, the result contains only non-matching strings (or less).
        /// If false, the result contains all non-matching strings (or more).</param>
        /// <returns>A tokens element which is an over- or under- approximation of the language not matching <paramref name="regex"/>.</returns>
        public static Tokens TokensForNegativeRegex(Element regex, bool underapproximate)
        {
            TokensNegativeRegexOperations operations = new TokensNegativeRegexOperations(!underapproximate);
            GeneratingInterpretation<TokensNegativeRegexState> interpretation = new GeneratingInterpretation<TokensNegativeRegexState>(operations);
            BackwardRegexInterpreter<GeneratingState<TokensNegativeRegexState>> interpreter = new BackwardRegexInterpreter<GeneratingState<TokensNegativeRegexState>>(interpretation);

            var result = interpreter.Interpret(regex);

            return new Tokens(operations.GetResult(result.Open));
        }


        public static IEnumerable<Element> RegexForTokens(Tokens self)
        {
            Element repeat = ToRegexVisitor.ToRegex(self.GetRoot(), true);
            Element suffix = ToRegexVisitor.ToRegex(self.GetRoot(), false);
            Concatenation concat = new Concatenation();
            concat.Parts.Add(Anchor.Begin);
            if (repeat != null)
            {
                concat.Parts.Add(new Loop(repeat, 0, Loop.Unbounded));
            }
            concat.Parts.Add(suffix);
            concat.Parts.Add(Anchor.End);
            return new Element[] { concat };
        }

    }

    class ToRegexVisitor : TokensTree.TokensTreeVisitor<Element>
    {
        bool repeatMode;

        public static Element ToRegex(InnerNode root, bool repeatMode)
        {
            ToRegexVisitor visitor = new ToRegexVisitor();
            visitor.repeatMode = repeatMode;
            return visitor.VisitNode(root);
        }

        protected override Element VisitRepeatNode(RepeatNode repeatNode)
        {
            if (repeatMode)
                return new Concatenation();
            else
                return null;
        }
        

        protected override Element VisitInnerNode(InnerNode innerNode)
        {
            Dictionary<TokensTreeNode, List<char>> nodesToChars = new Dictionary<TokensTreeNode, List<char>>();
            foreach(var c in innerNode.children)
            {
                List<char> chars;
                if(!nodesToChars.TryGetValue(c.Value, out chars))
                {
                    chars = new List<char>();
                    nodesToChars[c.Value] = chars;
                }

                chars.Add(c.Key);
            }

            Union un = new Union();

            foreach(var n in nodesToChars)
            {
                Element sub = VisitNode(n.Key);
                
                if(sub != null)
                {
                    CharRanges ranges = new CharRanges(n.Value.Select(c => new CharRange(c, c)));
                    Character chr = new Character(ranges, ranges);
                    Concatenation concat = new Concatenation();
                    concat.Parts.Add(chr);
                    if (sub is Concatenation)
                    {
                        concat.Parts.AddRange(((Concatenation)sub).Parts);
                    }
                    else { 
                        concat.Parts.Add(sub);
                    }
                    un.Patterns.Add(concat);
                }
            }

            if (!repeatMode && innerNode.Accepting)
                un.Patterns.Add(new Concatenation());

            if (un.Patterns.Count == 0)
                return null;

            if (un.Patterns.Count == 1)
                return un.Patterns[0];

            return un;
        }
    }

}
