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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.AbstractDomains.Strings.TokensTree;
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.AST;
using Microsoft.Research.CodeAnalysis;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// The Tokens abstract domain, where elements are prefix trees extended with repeat nodes
    /// and node sharing.
    /// </summary>
    public class Tokens : IStringAbstraction<Tokens>
    {
        /// <summary>
        /// May be null, which means Top.
        /// </summary>
        private readonly InnerNode root;

        internal InnerNode GetRoot()
        {
            return root ?? TokensTreeBuilder.Unknown();            
        }

        internal Tokens(InnerNode root)
        {
            this.root = root;
        }

        public Tokens Top
        {
            get
            {
                return new Tokens(null);
            }
        }

        public Tokens Bottom
        {
            get
            {
                return new Tokens(TokensTreeBuilder.Unreached());
            }
        }

        public bool IsTop
        {
            get
            {
                if (root == null)
                    return true;
                // The root node must be accepting and for each character, it must contain 
                // a child repeat node.
                if (!root.Accepting)
                    return false;

                for (int c = char.MinValue; c <= char.MaxValue; ++c)
                {
                    TokensTreeNode next;
                    if (!root.children.TryGetValue((char)c, out next))
                    {
                        return false;
                    }
                    else if (!(next is RepeatNode))
                        return false;
                }

                return true;
            }
        }

        public bool IsBottom
        {
            get
            {
                if (root == null)
                    return false;

                // The root node must not be accepting and not have any children.
                return !root.Accepting && root.children.Count == 0;
            }
        }

        IAbstractDomain IAbstractDomain.Bottom
        {
            get
            {
                return Bottom;
            }
        }

        IAbstractDomain IAbstractDomain.Top
        {
            get
            {
                return Top;
            }
        }

        public bool ContainsValue(string s)
        {
            if (root == null)
                return true;

            InnerNode current = root;
            foreach (char c in s)
            {
                TokensTreeNode next;
                if (!current.children.TryGetValue(c, out next))
                {
                    return false;
                }
                else if (next is RepeatNode)
                    current = root;
                else
                    current = (InnerNode)next;
            }

            return current.Accepting;
        }

        public class Operations<Variable> : IStringOperations<Tokens, Variable>
          where Variable : class, IEquatable<Variable>
        {
            public Tokens Top
            {
                get
                {
                    return new Tokens(null);
                }
            }

            private InnerNode After(InnerNode node, IndexInterval index)
            {

                if (index.UpperBound == 0)
                    return node;
                TokensTreeMerger merger = new TokensTreeMerger();
                After(node, index, merger);

                return merger.Build();
            }
            private void After(InnerNode node, IndexInterval index, TokensTreeMerger merger)
            { 
                LengthCongruenceVisitor lcv = new LengthCongruenceVisitor();
                int c = lcv.GetRepeatCommonDivisor(node);
                
                bool isBounded = node.IsBounded();

                IntervalMarkVisitor imv = new IntervalMarkVisitor(index, isBounded);
                imv.Collect(node, c);

                SliceAfterVisitor sliceAfterTransformer = new SliceAfterVisitor(merger, imv.Nodes);
                sliceAfterTransformer.Split(node, isBounded);
                if (index.UpperBound.IsInfinite)
                    merger.Cutoff(TokensTreeBuilder.Empty());
            }

            private InnerNode Before(InnerNode node, IndexInterval index)
            {
                if (index.LowerBound.IsInfinite)
                    return node;

                TokensTreeMerger merger = new TokensTreeMerger();
                Before(node, index, merger);
                return merger.Build();
            }

            private void Before(InnerNode node, IndexInterval index, TokensTreeMerger merger)
            {
                LengthCongruenceVisitor lcv = new LengthCongruenceVisitor();
                int c = lcv.GetRepeatCommonDivisor(node);
                
                bool isBounded = node.IsBounded();


                IntervalMarkVisitor imv = new IntervalMarkVisitor(index, isBounded);
                imv.Collect(node, c);

                SliceBeforeVisitor sliceBeforeVisitor = new SliceBeforeVisitor(merger, imv.Nodes, imv, index.UpperBound);
                sliceBeforeVisitor.SliceBefore(node);
            }



            public Tokens Substring(Tokens tokens, IndexInterval index, IndexInterval length)
            {
                if (tokens.root == null)
                    return tokens;

                return new Tokens(Before(After(tokens.root, index), length));
            }

            public Tokens Remove(Tokens tokens, IndexInterval index, IndexInterval length)
            {
                if (tokens.root == null)
                    return tokens;


                InnerNode node = tokens.root;


                InnerNode before = Before(node, index);
                InnerNode after = After(node, index + length);

                if(after.accepting && after.children.Count == 0)
                {
                    return new Tokens(before);
                }
                else {
                    TokensTreeMerger merger = new TokensTreeMerger();
                    new RepeatVisitor(merger).Repeat(before);
                    merger.Cutoff(after);
                    return new Tokens(merger.Build());
                }


            }

            public Tokens Concat(WithConstants<Tokens> left, WithConstants<Tokens> right)
            {
                Tokens leftAbstraction = left.ToAbstract(this);
                Tokens rightAbstraction = right.ToAbstract(this);

                if (leftAbstraction.root == null)
                    return leftAbstraction;
                if (rightAbstraction.root == null)
                    return rightAbstraction;

                IsBoundedVisitor boundedVisitor = new IsBoundedVisitor();
                bool bounded = boundedVisitor.IsBounded(leftAbstraction.root) && boundedVisitor.IsBounded(rightAbstraction.root);

                TokensTreeMerger merger = new TokensTreeMerger();

                if (!bounded)
                {

                    RepeatVisitor rv = new RepeatVisitor(merger);
                    rv.Repeat(leftAbstraction.root);
                    merger.Cutoff(rightAbstraction.root);

                }
                else
                {
                    ConcatVisitor cv = new ConcatVisitor(merger, rightAbstraction.root);
                    cv.ConcatTo(leftAbstraction.root);
                }

                return new Tokens(merger.Build());
            }

            private static void SplitAtIndex(InnerNode root, IndexInterval interval, TokensTreeMerger merger)
            {
                LengthCongruenceVisitor lcv = new LengthCongruenceVisitor();
                int repeatDivisor = lcv.GetRepeatCommonDivisor(root);
                
                bool isBounded = root.IsBounded();

                IntervalMarkVisitor imv = new IntervalMarkVisitor(interval, isBounded);
                imv.Collect(root, repeatDivisor);

                SplitVisitor msv = new SplitVisitor(merger, imv.Nodes);
                msv.Split(root);
            }

            public Tokens Insert(WithConstants<Tokens> self, IndexInterval index, WithConstants<Tokens> other)
            {
                Tokens selfTokens = self.ToAbstract(this), otherTokens = other.ToAbstract(this);

                InnerNode selfRoot = selfTokens.GetRoot(), otherRoot = otherTokens.GetRoot();

                //Split at the index, merge with the inserted part
                TokensTreeMerger merger = new TokensTreeMerger();

                SplitAtIndex(selfRoot, index, merger);
                RepeatVisitor rv = new RepeatVisitor(merger);
                rv.Repeat(otherRoot);

                return new Tokens(merger.Build());
            }

            public Tokens Replace(Tokens self, CharInterval from, CharInterval to)
            {
                var merger = new TokensTreeMerger();
                new ReplaceCharVisitor(merger, from, to).ReplaceChar(self.GetRoot());
                return new Tokens(merger.Build());
            }

            public Tokens Replace(WithConstants<Tokens> self, WithConstants<Tokens> from, WithConstants<Tokens> to)
            {
                Tokens selfAbstract = self.ToAbstract(this);
                Tokens fromAbstract = from.ToAbstract(this);
                Tokens toAbstract = to.ToAbstract(this);

                if(selfAbstract.root == null || fromAbstract.root == null || toAbstract.root == null)
                    return Top;

                BidirectionalSearch ptbs = new BidirectionalSearch(selfAbstract.root, fromAbstract.root, true);
                ptbs.Solve();
                ptbs.BackwardStage(true);
                var endPoints = ptbs.GetStartsAndEnds();
                if (endPoints.Count == 0)
                    return selfAbstract;

                // Split everywhere from can occur (both start and end) merge with to
                TokensTreeMerger merger = new TokensTreeMerger();
                SplitVisitor splitter = new SplitVisitor(merger, endPoints);
                splitter.Split(selfAbstract.root);
                RepeatVisitor rv = new RepeatVisitor(merger);
                rv.Repeat(toAbstract.root);


                return new Tokens(merger.Build());
            }

            public Tokens PadLeftRight(Tokens self, IndexInterval length, CharInterval fill, bool right)
            {
                //LEFT:
                // compare length intervals.
                // if may pad, add repeating node with fill edge from root
                // there may be more ways to optimize, for example if there are no repeat nodes and we know we will have to pad, add nodes at root?
                //RIGHT:
                // compare lengths
                // add repeating node, or add constant string of fill chars at accepting nodes

                InnerNode root = self.root;

                if (root == null)
                    return self;

                TokensTreeMerger merger = new TokensTreeMerger();
                LengthIntervalVisitor liv = new LengthIntervalVisitor();
                IndexInterval oldLength = liv.GetLengthInterval(root);

                if (right)
                {

                    if (oldLength.LowerBound >= length.UpperBound)
                    {
                        //The string must be longer, no action
                        return self;
                    }
                    else if (oldLength.IsFiniteConstant && length.IsFiniteConstant)
                    {
                        // We know exactly how many characters to add
                        InnerNode padding = (InnerNode)TokensTreeBuilder.FromCharInterval(fill, length.LowerBound.AsInt - oldLength.UpperBound.AsInt);
                        ConcatVisitor cv = new ConcatVisitor(merger, padding);

                        cv.ConcatTo(root);
                    }
                    else
                    {
                        RepeatVisitor rv = new RepeatVisitor(merger);
                        rv.Repeat(root);
                        TokensTreeNode padding = TokensTreeBuilder.CharIntervalTokens(fill);
                        merger.Cutoff(padding);
                    }

                }
                else
                {
                    int mustPad;
                    bool mayPadMore;

                    if (oldLength.UpperBound.IsInfinite)
                    {
                        //There are repeat nodes. We are not sure we will pad
                        mustPad = 0;
                    }
                    else
                    {
                        //It is finite, 
                        mustPad = length.LowerBound.AsInt - oldLength.UpperBound.AsInt;
                    }

                    mayPadMore = length.UpperBound.IsInfinite || length.UpperBound.AsInt > oldLength.LowerBound.AsInt + mustPad;

                    merger.Cutoff(TokensTreeBuilder.PrependCharInterval(fill, mustPad, root));
                    if (mayPadMore)
                        merger.Cutoff(TokensTreeBuilder.CharIntervalTokens(fill));

                }
                return new Tokens(merger.Build());
            }

            private Tokens AnyTrim(WithConstants<Tokens> self, WithConstants<Tokens> trimmed)
            {
                Tokens selfAbstraction = self.ToAbstract(this);

                if (selfAbstraction.root == null)
                    return selfAbstraction;

                TokensTreeMerger merger = new TokensTreeMerger();

                NodeCollectVisitor ncv = new NodeCollectVisitor();
                ncv.Collect(selfAbstraction.root);

                SplitVisitor msv = new SplitVisitor(merger, ncv.Nodes);
                msv.Split(selfAbstraction.root);

                return new Tokens(merger.Build());
            }

            public Tokens Trim(WithConstants<Tokens> self, WithConstants<Tokens> trimmed)
            {
                return AnyTrim(self, trimmed);
            }

            public Tokens TrimStartEnd(WithConstants<Tokens> self, WithConstants<Tokens> trimmed, bool end)
            {
                return AnyTrim(self, trimmed);
            }

            public Tokens SetCharAt(Tokens self, IndexInterval index, CharInterval value)
            {
                IndexInterval end = index.Add(1);
                InnerNode root = self.GetRoot();

                LengthCongruenceVisitor lcv = new LengthCongruenceVisitor();
                int repeatDivisor = lcv.GetRepeatCommonDivisor(root);
                
                bool isBounded = root.IsBounded();

                IntervalMarkVisitor imv = new IntervalMarkVisitor(index, isBounded);
                imv.Collect(root, repeatDivisor);
                IntervalMarkVisitor imve = new IntervalMarkVisitor(end, isBounded);
                imve.Collect(root, repeatDivisor);

                HashSet<InnerNode> union = new HashSet<InnerNode>(imv.Nodes);
                union.UnionWith(imve.Nodes);

                //Split at the index, merge with the inserted part
                TokensTreeMerger merger = new TokensTreeMerger();

                SplitVisitor msv = new SplitVisitor(merger, union);
                msv.Split(self.root);
                merger.Cutoff(TokensTreeBuilder.CharIntervalTokens(value));

                return new Tokens(merger.Build());
            }

            public IStringPredicate IsEmpty(Tokens self, Variable selfVariable)
            {
                if(self.root == null)
                {
                    return StringAbstractionPredicate.ForTrue(selfVariable, new Tokens(TokensTreeBuilder.Empty()));
                }

                bool canBeEmpty = self.root.Accepting;
                bool canBeNonEmpty = self.root.children.Count > 0;

                if (canBeEmpty && canBeNonEmpty && selfVariable != null)
                {
                    return StringAbstractionPredicate.ForTrue(selfVariable, new Tokens(TokensTreeBuilder.Empty()));
                }

                return new FlatPredicate(canBeEmpty, canBeNonEmpty);
            }

            public IStringPredicate Contains(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
            {
                // Seems there is no possibility to return predicate here
                Tokens selfAbstraction = self.ToAbstract(this), otherAbstraction = other.ToAbstract(this);

                InnerNode selfRoot = selfAbstraction.root;
                InnerNode otherRoot = otherAbstraction.root;

                if(otherRoot == null)
                    return FlatPredicate.Top;

                if(selfRoot == null)
                {
                    if (new ConstantVisitor().GetConstant(otherRoot) == "")
                        return FlatPredicate.True;
                    else
                        return FlatPredicate.Top;
                }

                ForwardSearch forwardSearch = new ForwardSearch(selfRoot, otherRoot, true);
                forwardSearch.Solve();
                if (forwardSearch.FoundAnyEnd())
                {
                    // Try to prove containment for constants
                    var constant = new ConstantVisitor().GetConstant(otherRoot);
                    if(constant != null)
                    {
                        if (constant == "")
                            return FlatPredicate.True;

                        MustContainVisitor mcv = new MustContainVisitor(constant, false);

                        if (mcv.MustContain(selfRoot))
                            return FlatPredicate.True;
                    }

                    return FlatPredicate.Top;
                }
                else
                    return FlatPredicate.False;
            }

            public IStringPredicate StartsEndsWithOrdinal(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable, bool ends)
            {
                if (ends)
                    return EndsWithOrdinal(self, selfVariable, other, otherVariable);
                else
                    return StartsWithOrdinal(self, selfVariable, other, otherVariable);
            }

            private IStringPredicate StartsWithOrdinal(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
            {
                Tokens selfAbstraction = self.ToAbstract(this), otherAbstraction = other.ToAbstract(this);
                // Seems there is no possiblity to return predicate here

                InnerNode otherRoot = otherAbstraction.root;
                InnerNode selfRoot = selfAbstraction.root;

                if(otherRoot == null)
                    return FlatPredicate.Top;

                // Can return true only if other is constant
                ConstantVisitor cv = new ConstantVisitor();
                string prefix = cv.GetConstant(otherRoot);

                if(selfRoot == null)
                {
                    if (prefix == "")
                        return FlatPredicate.True;
                    else
                        return FlatPredicate.Top;
                }

                if (prefix != null)
                {
                    InnerNode node = selfRoot;
                    bool unique = true;

                    foreach (char c in prefix)
                    {
                        TokensTreeNode ptn;
                        if (!node.children.TryGetValue(c, out ptn))
                        {
                            return FlatPredicate.False;
                        }
                        if (node.children.Count > 1)
                            unique = false;
                        node = ptn.ToInner(selfRoot);
                    }

                    return unique ? FlatPredicate.True : FlatPredicate.Top;
                }
                else
                {
                    //cubic occurence finder that will aling nodes by pairs from the start
                    // used also for replace, meet, endswith, indexOf

                    ForwardSearch ptfm = new ForwardSearch(selfRoot, otherRoot, false);
                    ptfm.Solve();
                    if (ptfm.FoundAnyEnd())
                        return FlatPredicate.Top;
                    else
                        return FlatPredicate.False;
                }
            }

            private IStringPredicate EndsWithOrdinal(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
            {
                // Seems there is no possiblity to return predicate here 

                // Even if other is a single constant, then if we tried to represent te suffix, due to the fact that all characters are
                // possible in the first part, there must be a repeat node after each character, which will collapse anything joined to it
                // That means, any string that may contain an entirely unknown part, is automatically TOP in this domain.

                Tokens selfAbstraction = self.ToAbstract(this), otherAbstraction = other.ToAbstract(this);

                InnerNode otherRoot = otherAbstraction.root;
                InnerNode selfRoot = selfAbstraction.root;

                if (otherRoot == null)
                    return FlatPredicate.Top;
                if (selfRoot == null)
                {
                    var constant = new ConstantVisitor().GetConstant(otherRoot);
                    if (constant == "")
                        return FlatPredicate.True;
                    else
                        return FlatPredicate.Top;
                }

                ForwardSearch ptfm = new ForwardSearch(selfRoot, otherRoot, true);
                ptfm.Solve();
                if (ptfm.FoundAlignedEnd())
                {
                    var constant = new ConstantVisitor().GetConstant(otherRoot);
                    if (constant != null)
                    {
                        if (constant == "")
                            return FlatPredicate.True;

                        MustContainVisitor mcv = new MustContainVisitor(constant, true);

                        if (mcv.MustContain(selfRoot))
                            return FlatPredicate.True;
                    }
                    return FlatPredicate.Top;
                }
                else
                    return FlatPredicate.False;
            }

            public IStringPredicate Equals(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
            {
                //True only if both are constants
                //False if they cannot be equal
                // return self predicate
                Tokens selfAbstraction = self.ToAbstract(this), otherAbstraction = other.ToAbstract(this);

                InnerNode otherRoot = otherAbstraction.root;
                InnerNode selfRoot = selfAbstraction.root;

                if (selfRoot == null || otherRoot == null || EqualityRelation.CanBeEqual(selfRoot, otherRoot))
                {

                    if (selfRoot != null && otherRoot != null)
                    {
                        string lc = new ConstantVisitor().GetConstant(selfRoot);
                        string rc = new ConstantVisitor().GetConstant(otherRoot);
                        if (lc == rc)
                            return FlatPredicate.True;
                    }

                    if (otherVariable != null)
                        return StringAbstractionPredicate.ForTrue(otherVariable, selfAbstraction);
                    else if (selfVariable != null)
                        return StringAbstractionPredicate.ForTrue(selfVariable, otherAbstraction);
                    else
                        return FlatPredicate.Top;
                }
                else
                {
                    return FlatPredicate.False;
                }
            }

            private string ToConstant(InnerNode nd, WithConstants<Tokens> arg)
            {
                if (arg.IsConstant)
                    return arg.Constant;
                else
                {
                    ConstantVisitor cv = new ConstantVisitor();
                    return cv.GetConstant(nd);
                }

            }

            public CompareResult CompareOrdinal(WithConstants<Tokens> self, WithConstants<Tokens> other)
            {
                //Preorders on nodes
                InnerNode leftRoot = self.ToAbstract(this).root, rightRoot = other.ToAbstract(this).root;

                if (leftRoot == null || rightRoot == null)
                    return CompareResult.Top;

                bool canle = LessThanEqualRelation.CanBeLessEqual(leftRoot, rightRoot);
                bool cange = LessThanEqualRelation.CanBeLessEqual(rightRoot, leftRoot);
                bool canlt = LessThanRelation.CanBeLess(leftRoot, rightRoot);
                bool cangt = LessThanRelation.CanBeLess(rightRoot, leftRoot);

                return CompareResultExtensions.Build(canle && canlt, canle && cange, cange && cangt);
            }

            public IndexInterval GetLength(Tokens self)
            {
                if (self.root == null)
                    return IndexInterval.UnknownNonNegative;

                LengthIntervalVisitor liv = new LengthIntervalVisitor();
                return liv.GetLengthInterval(self.root);
            }

            public IndexInterval IndexOf(WithConstants<Tokens> self, WithConstants<Tokens> needle, IndexInterval offset, IndexInterval count, bool last)
            {
                Tokens selfAbstraction = self.ToAbstract(this);
                Tokens needleAbstraction = needle.ToAbstract(this);
                //TODO:  use offset and count to limit the beginnings/ends

                if (selfAbstraction.root == null || needleAbstraction.root == null)
                    return IndexInterval.Unknown;

                BidirectionalSearch ptbs = new BidirectionalSearch(selfAbstraction.root, needleAbstraction.root, true);
                ptbs.Solve();
                ptbs.BackwardStage(true);

                IndexOfVisitor iof = new IndexOfVisitor(ptbs.GetStarts());

                IndexInterval ii = iof.Interval;
                
                if (selfAbstraction.root.IsBounded())
                {
                    return ii;
                }
                else
                {
                    return IndexInterval.For(ii.LowerBound, IndexInt.Infinity);
                }
                
            }

 

            public CharInterval GetCharAt(Tokens self, IndexInterval index)
            {
                InnerNode root = self.root;
                if (root == null)
                    return CharInterval.Unknown;

                CharInterval result = CharInterval.Unreached;

                bool bounded = root.IsBounded();
                LengthCongruenceVisitor lcv = new LengthCongruenceVisitor();
                int repeatDivisor = lcv.GetRepeatCommonDivisor(root);

                IntervalMarkVisitor imv = new IntervalMarkVisitor(index, bounded);
                imv.Collect(root, repeatDivisor);
                
                foreach(InnerNode node in imv.Nodes) {
                    if (node.children.Count > 0)
                    {
                        char min = node.children.Keys.Min();
                        char max = node.children.Keys.Max();
                        result = result.Join(CharInterval.For(min, max));
                    }
                }
                return result;
            }

            #region Regex operations
            public IStringPredicate RegexIsMatch(Tokens self, Variable selfVariable, Microsoft.Research.Regex.Model.Element regex)
            {
                //regex underapprox less equal self -> return true
                //self meet regex overapprox is bottom -> return false
                // else return overapprox predicate.
                Tokens regexUnder = TokensRegex.TokensForRegex(regex, true);
                Tokens regexOver = TokensRegex.TokensForRegex(regex, false);

                Tokens negRegexUnder = TokensRegex.TokensForNegativeRegex(regex, true);
                Tokens negRegexOver = TokensRegex.TokensForNegativeRegex(regex, false);

                if (self.LessThanEqual(regexUnder) || self.Meet(negRegexOver).IsBottom)
                    return FlatPredicate.True;

                if (self.Meet(regexOver).IsBottom/* || self.LessThanEqual(negRegexUnder)*/)
                    return FlatPredicate.False;

                if (selfVariable != null)
                    return StringAbstractionPredicate.For(selfVariable, regexOver, negRegexOver);
                else
                    return FlatPredicate.Top;
            }

            ///<inheritdoc/>
            public IEnumerable<Microsoft.Research.Regex.Model.Element> ToRegex(Tokens self)
            {
                return TokensRegex.RegexForTokens(self);
            }
            #endregion

            public Tokens Constant(string constant)
            {
                return new Tokens(TokensTreeBuilder.FromString(constant));
            }
        }

        public bool Equals(Tokens other)
        {
            bool selfTop = root == null;
            bool otherTop = other.root == null;

            if (selfTop && otherTop)
                return true;
            else if (selfTop || otherTop)
                return false;
            
            return NodeComparer.Comparer.Equals(root, other.root);
        }
 

        public bool LessThanEqual(Tokens other)
        {
            if (other.IsTop)
                return true;
            if (IsTop)
                return false;
            return PreorderRelation.LessEqual(root, other.root);
        }

        public Tokens Join(Tokens other)
        {
            if (root == null)
                return this;
            else if (other.root == null)
                return other;

            TokensTreeMerger merger = new TokensTreeMerger();
            merger.Cutoff(root);
            merger.Cutoff(other.root);

            return new Tokens(merger.Build());
        }

        public Tokens Meet(Tokens other)
        {
            if (root == null)
                return other;
            else if (other.root == null)
                return this;

            return new Tokens(MeetRelation.Meet(root, other.root));
        }

        public Tokens Constant(string cst)
        {
            return new Tokens(TokensTreeBuilder.FromString(cst));
        }

        public bool LessEqual(IAbstractDomain a)
        {
            return LessThanEqual(a as Tokens);
        }

        public IAbstractDomain Join(IAbstractDomain a)
        {
            return Join(a as Tokens);
        }

        public IAbstractDomain Meet(IAbstractDomain a)
        {
            return Meet(a as Tokens);
        }

        public IAbstractDomain Widening(IAbstractDomain prev)
        {
            if (root == null)
                return this;

            Tokens prevTokens = (Tokens)prev;

            if (prevTokens.root == null)
                return prevTokens;

            WideningMerger merger = new WideningMerger();
          
            return new Tokens(merger.Widening(prevTokens.root, root));
        }

        public T To<T>(IFactory<T> factory)
        {
            return factory.Constant(true);
        }

        public object Clone()
        {
            return new Tokens(root);
        }

        #region object overrides
        public override string ToString()
        {
            if(root == null)
            {
                return "T";
            }
            ToStringVisitor visitor = new ToStringVisitor();
            return visitor.ToString(root);
        }
        public override bool Equals(object obj)
        {
            Tokens other = obj as Tokens;
            if (other == null)
                return false;
            return Equals(other);
        }
        public override int GetHashCode()
        {
            if (root == null)
                return 0;
            return NodeComparer.Comparer.GetHashCode(root);
        }
        #endregion
    }
}
