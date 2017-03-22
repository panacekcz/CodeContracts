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
            emptyNode = PrefixTreeBuilder.Empty();
            topNode = PrefixTreeBuilder.Unknown();
        }

        private bool IsEmpty(InnerNode node)
        {
            return node.Accepting && node.children.Count == 0;
        }

        public InnerNode GetResult(TokensRegexState state)
        {
            // Makes a single result from the two parts
            // If one of the parts is empty, return the other one
            if (IsEmpty(state.prefix))
                return state.result;
            else if (IsEmpty(state.result))
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
                    return PrefixTreeBuilder.Unreached();
            }
            else
            {
                // Otherwise, repeat the first part and join with the other one
                PrefixTreeMerger merger = new PrefixTreeMerger();
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
                PrefixTreeMerger merger = new PrefixTreeMerger();
                var rv = new RepeatVisitor(merger);

                rv.Repeat(last.prefix);
                rv.Repeat(last.result);

                if (IsEmpty(prev.result))
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

            return new TokensRegexState(PrefixTreeBuilder.PrependCharIntervals(ranges.ToIntervals(), prev.prefix), prev.result);
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
                return new TokensRegexState(PrefixTreeBuilder.Unreached(), PrefixTreeBuilder.Unreached());
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
                if (IsEmpty(prev.prefix) && IsEmpty(next.result))
                    next = new TokensRegexState(emptyNode, GetResult(next));
                else if (IsEmpty(next.prefix) && IsEmpty(prev.result))
                    prev = new TokensRegexState(emptyNode, GetResult(prev));

                PrefixTreeMerger mergerPrefix = new PrefixTreeMerger();
                mergerPrefix.Cutoff(prev.prefix);
                mergerPrefix.Cutoff(next.prefix);

                PrefixTreeMerger mergerResult = new PrefixTreeMerger();
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

    /// <summary>
    /// This one should 
    /// </summary>
    internal class TokensNegativeRegexOperations : IGeneratingOperationsForRegex<TokensNegativeRegexState>
    {
        private readonly bool underapproximate;
        public TokensNegativeRegexOperations(bool underapproximate) {
            this.underapproximate = underapproximate;
        }


        public InnerNode GetResult(TokensNegativeRegexState state)
        {
            if (!state.isOpenStart || !state.isOpenEnd)
                return underapproximate ? PrefixTreeBuilder.Unknown() : PrefixTreeBuilder.Unreached();
            else
            {
                return ComplementVisitor.Complement(state.root);
            }
        }

        public TokensNegativeRegexState Bottom
        {
            get
            {
                return new TokensNegativeRegexState(PrefixTreeBuilder.Unreached(), true, true);
            }
        }

        public TokensNegativeRegexState Empty
        {
            get
            {
                return new TokensNegativeRegexState(PrefixTreeBuilder.Empty(), false, false);
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
                return new TokensNegativeRegexState(PrefixTreeBuilder.Empty(), true, true);
            }
        }

        public TokensNegativeRegexState AddChar(TokensNegativeRegexState prev, CharRanges ranges, bool closed)
        {
            if (!prev.root.Accepting && prev.root.children.Count == 0)
                return prev;

            return new TokensNegativeRegexState(
                PrefixTreeBuilder.PrependCharIntervals(ranges.ToIntervals(), prev.root), prev.isOpenStart, !closed
                );
        }

        public bool CanBeEmpty(TokensNegativeRegexState prev)
        {
            return false;
        }

        public TokensNegativeRegexState Join(TokensNegativeRegexState left, TokensNegativeRegexState right, bool widen)
        {
            //TODO: chech if all the cases work

            // The tree does not contain any repeat nodes

            PrefixTreeMerger merger = new PrefixTreeMerger();
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

    }

}
