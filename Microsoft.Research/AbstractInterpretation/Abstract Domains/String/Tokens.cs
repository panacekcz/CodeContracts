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
using Microsoft.Research.AbstractDomains.Strings.PrefixTree;
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.AST;
using Microsoft.Research.CodeAnalysis;

namespace Microsoft.Research.AbstractDomains.Strings
{

    public class Tokens : IStringAbstraction<Tokens, string>
    {
        private readonly InnerNode root;

        internal Tokens(InnerNode root)
        {
            this.root = root;
        }

        public Tokens Top
        {
            get
            {
                return new Tokens(PrefixTreeBuilder.Unknown());
            }
        }

        public Tokens Bottom
        {
            get
            {
                return new Tokens(PrefixTreeBuilder.Unreached());
            }
        }

        public bool IsTop
        {
            get
            {
                if (!root.Accepting)
                    return false;
                for (int c = char.MinValue; c <= char.MaxValue; ++c)
                {
                    PrefixTreeNode next;
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
            InnerNode current = root;
            foreach (char c in s)
            {
                PrefixTreeNode next;
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
                    return new Tokens(PrefixTreeBuilder.Unknown());
                }
            }

            public Tokens Substring(Tokens tokens, IndexInterval index, IndexInterval length)
            {
                //TODO: cutoff at various places
                return tokens.Top;
            }

            public Tokens Remove(Tokens tokens, IndexInterval index, IndexInterval length)
            {
                //TODO: cutoff at various places
                return tokens.Top;
            }

            public Tokens Concat(WithConstants<Tokens> left, WithConstants<Tokens> right)
            {
                Tokens leftAbstraction = left.ToAbstract(this);
                Tokens rightAbstraction = right.ToAbstract(this);

                PrefixTreeBounded boundedVisitor = new PrefixTreeBounded();
                bool bounded = boundedVisitor.IsBounded(leftAbstraction.root) && boundedVisitor.IsBounded(rightAbstraction.root);

                PrefixTreeMerger merger = new PrefixTreeMerger();

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

            public Tokens Insert(WithConstants<Tokens> self, IndexInterval index, WithConstants<Tokens> other)
            {
                Tokens selfAbstraction = self.ToAbstract(this), otherAbstraction = other.ToAbstract(this);

                IntervalMarkVisitor imv = new IntervalMarkVisitor(index);
                imv.Collect(selfAbstraction.root);

                
                if(imv.Nodes.Count == 1)
                {
                    //TODO:
                }

                //Split at the index, merge with the inserted part
                PrefixTreeMerger merger = new PrefixTreeMerger();

                MarkSplitVisitor msv = new MarkSplitVisitor(merger, imv.Nodes);
                msv.Split(selfAbstraction.root);
                RepeatVisitor rv = new RepeatVisitor(merger);
                rv.Repeat(otherAbstraction.root);

                return new Tokens(merger.Build());
            }

            public Tokens Replace(Tokens self, CharInterval from, CharInterval to)
            {
                var merger = new PrefixTreeMerger();
                new ReplaceCharVisitor(merger, from, to).ReplaceChar(self.root);
                return new Tokens(merger.Build());
            }

            public Tokens Replace(WithConstants<Tokens> self, WithConstants<Tokens> from, WithConstants<Tokens> to)
            {
                Tokens selfAbstract = self.ToAbstract(this);
                Tokens fromAbstract = from.ToAbstract(this);
                Tokens toAbstract = to.ToAbstract(this);

                PrefixTreeBackwardSearch ptbs = new PrefixTreeBackwardSearch(selfAbstract.root, fromAbstract.root, true);
                ptbs.Solve();
                ptbs.BackwardStage(true);
                // Split everywhere from can occur (both start and end) merge with to
                PrefixTreeMerger merger = new PrefixTreeMerger();
                MarkSplitVisitor splitter = new MarkSplitVisitor(merger, ptbs.GetEndpoints());
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
                PrefixTreeMerger merger = new PrefixTreeMerger();
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
                        InnerNode padding = (InnerNode)PrefixTreeBuilder.FromCharInterval(fill, length.LowerBound.AsInt - oldLength.UpperBound.AsInt);
                        ConcatVisitor cv = new ConcatVisitor(merger, padding);

                        cv.ConcatTo(self.root);
                    }
                    else
                    {
                        RepeatVisitor rv = new RepeatVisitor(merger);
                        rv.Repeat(self.root);
                        PrefixTreeNode padding = PrefixTreeBuilder.CharIntervalTokens(fill);
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

                    merger.Cutoff(PrefixTreeBuilder.PrependFromCharInterval(fill, mustPad, root));
                    if (mayPadMore)
                        merger.Cutoff(PrefixTreeBuilder.CharIntervalTokens(fill));

                }
                return new Tokens(merger.Build());
            }

            private Tokens AnyTrim(WithConstants<Tokens> self, WithConstants<Tokens> trimmed)
            {
                Tokens selfAbstraction = self.ToAbstract(this), trimmedAbstraction = trimmed.ToAbstract(this);
                PrefixTreeMerger merger = new PrefixTreeMerger();

                NodeCollectVisitor ncv = new NodeCollectVisitor();
                ncv.Collect(selfAbstraction.root);

                MarkSplitVisitor msv = new MarkSplitVisitor(merger, ncv.Nodes);
                msv.Split(selfAbstraction.root);

                return new Tokens(merger.Build());
            }

            public Tokens Trim(WithConstants<Tokens> self, WithConstants<Tokens> trimmed)
            {
                //TODO:
                //return Top;
                return AnyTrim(self, trimmed);
            }

            public Tokens TrimStartEnd(WithConstants<Tokens> self, WithConstants<Tokens> trimmed, bool end)
            {
                return AnyTrim(self, trimmed);
                //TODO:
                //START:
                // if does not contain repeat nodes,
                // then trim the characters from the root, merge the cutted subtrees with root

                // if does contain repeat nodes, do the same, but keep the original root
                //END:

                // trim from accepting nodes
                // if reached root, ???

                //return Top;
            }

            public Tokens SetCharAt(Tokens self, IndexInterval index, CharInterval value)
            {
                IndexInterval end = index.Add(1);

                IntervalMarkVisitor imv = new IntervalMarkVisitor(index);
                imv.Collect(self.root);
                IntervalMarkVisitor imve = new IntervalMarkVisitor(end);
                imve.Collect(self.root);

                if (imv.Nodes.Count == 1)
                {
                    //TODO:
                }

                HashSet<InnerNode> union = new HashSet<InnerNode>(imv.Nodes);
                union.UnionWith(imve.Nodes);

                //Split at the index, merge with the inserted part
                PrefixTreeMerger merger = new PrefixTreeMerger();

                MarkSplitVisitor msv = new MarkSplitVisitor(merger, union);
                msv.Split(self.root);
                merger.Cutoff(PrefixTreeBuilder.CharIntervalNode(value, RepeatNode.Repeat));

                return new Tokens(merger.Build());
            }

            public IStringPredicate IsEmpty(Tokens self, Variable selfVariable)
            {
                bool canBeEmpty = self.root.Accepting;
                bool canBeNonEmpty = self.root.children.Count > 0;

                if (canBeEmpty && canBeNonEmpty && selfVariable != null)
                {
                    return StringAbstractionPredicate.ForTrue(selfVariable, new Tokens(PrefixTreeBuilder.Empty()));
                }

                return new FlatPredicate(canBeEmpty, canBeNonEmpty);
            }

            public IStringPredicate Contains(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
            {

                // Seems there is no possibiliry to return predicate here


                Tokens selfAbstraction = self.ToAbstract(this), otherAbstraction = other.ToAbstract(this);

                PrefixTreeForwardSearch ptfm = new PrefixTreeForwardSearch(selfAbstraction.root, otherAbstraction.root, true);
                ptfm.Solve();
                if (ptfm.AnyEnd())
                    return FlatPredicate.Top;
                else
                    return FlatPredicate.False;
            }

            public IStringPredicate StartsWithOrdinal(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
            {
                Tokens selfAbstraction = self.ToAbstract(this), otherAbstraction = other.ToAbstract(this);

                // Seems there is no possiblity to return predicate here

                // Can return true only if other is constant
                ConstantVisitor cv = new ConstantVisitor();
                string prefix = cv.GetConstant(otherAbstraction.root);

                if (prefix != null)
                {
                    InnerNode node = selfAbstraction.root;
                    bool unique = true;

                    foreach (char c in prefix)
                    {
                        PrefixTreeNode ptn;
                        if (!node.children.TryGetValue(c, out ptn))
                        {
                            return FlatPredicate.False;
                        }
                        if (node.children.Count > 1)
                            unique = false;
                        node = ptn.ToInner(selfAbstraction.root);
                    }

                    return unique ? FlatPredicate.True : FlatPredicate.Top;
                }
                else
                {
                    //cubic occurence finder that will aling nodes by pairs from the start
                    // used also for replace, meet, endswith, indexOf

                    PrefixTreeForwardSearch ptfm = new PrefixTreeForwardSearch(selfAbstraction.root, otherAbstraction.root, false);
                    ptfm.Solve();
                    if (ptfm.AnyEnd())
                        return FlatPredicate.Top;
                    else
                        return FlatPredicate.False;
                }
            }

            public IStringPredicate EndsWithOrdinal(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
            {
                // Seems there is no possiblity to return predicate here 

                // Even if other is a single constant, then if we tried to represent te suffix, due to the fact that all characters are
                // possible in the first part, there must be a repeat node after each character, which will collapse anything joined to it
                // That means, any string that may contain an entirely unknown part, is automatically TOP in this domain.
                //TODO: if other is constant return true if possible

                Tokens selfAbstraction = self.ToAbstract(this), otherAbstraction = other.ToAbstract(this);

                PrefixTreeForwardSearch ptfm = new PrefixTreeForwardSearch(selfAbstraction.root, otherAbstraction.root, true);
                ptfm.Solve();
                if (ptfm.AlignedEnd())
                    return FlatPredicate.Top;
                else
                    return FlatPredicate.False;
            }

            public IStringPredicate Equals(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
            {
                //True only if both are constants
                //False if they cannot be equal
                // return self predicate
                Tokens selfAbstraction = self.ToAbstract(this), otherAbstraction = other.ToAbstract(this);
                if (EqualityVisitor.CanBeEqual(selfAbstraction.root, otherAbstraction.root))
                {
                    string lc = new ConstantVisitor().GetConstant(selfAbstraction.root);
                    string rc = new ConstantVisitor().GetConstant(otherAbstraction.root);
                    if (lc == rc)
                        return FlatPredicate.True;

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

                bool canle = CompareVisitor.CanBeLessEqual(leftRoot, rightRoot);
                bool cange = CompareVisitor.CanBeLessEqual(rightRoot, leftRoot);
                bool canlt = StrictCompareVisitor.CanBeLess(leftRoot, rightRoot);
                bool cangt = StrictCompareVisitor.CanBeLess(rightRoot, leftRoot);

                return CompareResultExtensions.Build(canle && canlt, canle && cange, cange && cangt);
            }

            public IndexInterval GetLength(Tokens self)
            {
                LengthIntervalVisitor liv = new LengthIntervalVisitor();
                return liv.GetLengthInterval(self.root);
            }

            public IndexInterval IndexOf(WithConstants<Tokens> self, WithConstants<Tokens> needle, IndexInterval offset, IndexInterval count, bool last)
            {
                Tokens selfAbstraction = self.ToAbstract(this);
                Tokens needleAbstraction = needle.ToAbstract(this);
                //TODO: use offset and count to limit the beginnings/ends
                PrefixTreeBackwardSearch ptbs = new PrefixTreeBackwardSearch(selfAbstraction.root, needleAbstraction.root, true);
                ptbs.Solve();
                ptbs.BackwardStage(true);

                IndexOfVisitor iof = new IndexOfVisitor(ptbs.GetStarts());

                IndexInterval ii = iof.Interval;
                PrefixTreeBounded boundedVisitor = new PrefixTreeBounded();
                if (boundedVisitor.IsBounded(selfAbstraction.root))
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
                CharAtVisitor cav = new CharAtVisitor();
                return cav.CharAt(index, self.root);
            }

            public IStringPredicate RegexIsMatch(Tokens self, Variable selfVariable, Element regex)
            {
                //regex underapprox less equal self -> return true
                //self meet regex overapprox is bottom -> return false
                // else return overapprox predicate.
                Tokens regexUnder = TokensRegex.FromRegex(regex, true);
                Tokens regexOver = TokensRegex.FromRegex(regex, false);

                Tokens negRegexUnder = TokensRegex.FromNegativeRegex(regex, true);
                Tokens negRegexOver = TokensRegex.FromNegativeRegex(regex, false);

                if (self.LessThanEqual(regexUnder) || self.Meet(negRegexOver).IsBottom)
                    return FlatPredicate.True;


                if (self.Meet(regexOver).IsBottom || self.LessThanEqual(negRegexUnder))
                    return FlatPredicate.False;

                return StringAbstractionPredicate.For(selfVariable, regexOver, negRegexOver);
            }

            public Tokens Constant(string constant)
            {
                return new Tokens(PrefixTreeBuilder.FromString(constant));
            }
        }

        public bool Equals(Tokens other)
        {
            return PrefixTreeNodeComparer.Comparer.Equals(root, other.root);
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
            return PrefixTreeNodeComparer.Comparer.GetHashCode(root);
        }

        public bool LessThanEqual(Tokens other)
        {
            return PrefixTreePreorder.LessEqual(root, other.root);
        }

        public Tokens Join(Tokens other)
        {
            PrefixTreeMerger merger = new PrefixTreeMerger();
            merger.Cutoff(root);
            merger.Cutoff(other.root);

            return new Tokens(merger.Build());
        }

        public Tokens Meet(Tokens other)
        {
            //TODO: intersection
            return this;
        }

        public Tokens Constant(string cst)
        {
            return new Tokens(PrefixTreeBuilder.FromString(cst));
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
            PrefixTreeMerger merger = new PrefixTreeMerger();
            RepeatVisitor rv = new RepeatVisitor(merger);
            rv.Repeat(root);
            rv.Repeat((prev as Tokens).root);

            //TODO: this is not enough i think
            return new Tokens(merger.Build());

        }

        public T To<T>(IFactory<T> factory)
        {
            return factory.Constant(true);
        }

        public object Clone()
        {
            return new Tokens(root);
        }
        public override string ToString()
        {
            ToStringVisitor visitor = new ToStringVisitor();
            return visitor.ToString(root);
        }

    }


}
