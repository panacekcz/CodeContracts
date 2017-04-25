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
using Microsoft.Research.AbstractDomains.Strings.Graphs;
using Microsoft.Research.CodeAnalysis;
namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Elements of the "string graph" abstract domain for strings.
    /// </summary>
    public class StringGraph : IStringAbstraction<StringGraph>
    {
        #region Private state
        /// <summary>
        /// The root node of the graph. Not <see langword="null"/>.
        /// </summary>
        private readonly Node root;
        #endregion
        #region Construction

        internal StringGraph(Node root)
        {
            this.root = root;
        }
        /// <summary>
        /// Constructs a string graph for a single character.
        /// </summary>
        /// <param name="value">The value of the character.</param>
        /// <returns>A string graph for <paramref name="value"/>.</returns>
        public static StringGraph ForChar(char value)
        {
            return new StringGraph(new CharNode(value));
        }
        /// <summary>
        /// Constructs a string graph for a contatenation of string graphs.
        /// </summary>
        /// <param name="parts">String graphs representing the parts.</param>
        /// <returns>String graph for concatenation of <paramref name="parts"/>.</returns>
        public static StringGraph ForConcat(params StringGraph[] parts)
        {
            return ForConcat((IEnumerable<StringGraph>)parts);
        }
        /// <summary>
        /// Constructs a string graph for a contatenation of string graphs.
        /// </summary>
        /// <param name="parts">String graphs representing the parts.</param>
        /// <returns>String graph for concatenation of <paramref name="parts"/>.</returns>
        public static StringGraph ForConcat(IEnumerable<StringGraph> parts)
        {
            return new StringGraph(new ConcatNode(parts.Select(part => part.root)));
        }

        internal static StringGraph ForConcat(params Node[] parts)
        {
            return new StringGraph(new ConcatNode(parts));
        }

        /// <summary>
        /// Constructs a string graph for a union of string graphs.
        /// </summary>
        /// <param name="alternatives">String graphs representing the alternatives.</param>
        /// <returns>String graph for union of <paramref name="alternatives"/>.</returns>
        public static StringGraph ForUnion(params StringGraph[] alternatives)
        {
            return ForUnion((IEnumerable<StringGraph>)alternatives);
        }
        /// <summary>
        /// Constructs a string graph for a union of string graphs.
        /// </summary>
        /// <param name="alternatives">String graphs representing the alternatives.</param>
        /// <returns>String graph for union of <paramref name="alternatives"/>.</returns>
        public static StringGraph ForUnion(IEnumerable<StringGraph> alternatives)
        {
            return new StringGraph(new OrNode(alternatives.Select(alternative => alternative.root)));
        }
        /// <summary>
        /// Constructs a string graph for a single string constant.
        /// </summary>
        /// <param name="constant">The value of the string constant.</param>
        /// <returns>A string graph for <paramref name="constant"/>.</returns>
        public static StringGraph ForString(string constant)
        {
            return new StringGraph(NodeBuilder.CreateConcatNodeForString(constant));
        }
        /// <summary>
        /// Gets a string graph for no strings.
        /// </summary>
        public static StringGraph ForBottom
        {
            get
            {
                return new StringGraph(new BottomNode());
            }
        }
        /// <summary>
        /// Gets a string graph for all strings.
        /// </summary>
        public static StringGraph ForMax
        {
            get
            {
                return new StringGraph(new MaxNode());
            }
        }

        #endregion

        #region Domain properties
        public StringGraph Top
        {
            get { return ForMax; }
        }

        public StringGraph Bottom
        {
            get { return ForBottom; }
        }

        public bool IsTop
        {
            get { return root.Label.Kind == NodeKind.Max; }
        }

        public bool IsBottom
        {
            get { return root.Label.Kind == NodeKind.Bottom; }
        }

        public bool ContainsValue(string value)
        {
            // overapproximation
            return true;
        }
        #endregion

        #region Domain operators
        ///<inheritdoc/>
        public bool Equals(StringGraph other)
        {
            if (other == null)
            {
                return false;
            }
            if (other == this)
            {
                return true;
            }

            //underapproximation
            return false;
        }
        ///<inheritdoc/>
        public bool LessThanEqual(StringGraph other)
        {
            if (other.IsTop || IsBottom)
            {
                return true;
            }
            if (IsTop || other.IsBottom)
            {
                return false;
            }

            if (Equals(other))
            {
                return true;
            }

            // underapproximation
            return false;
        }
        ///<inheritdoc/>
        public StringGraph Join(StringGraph other)
        {
            return ForUnion(this, other);
        }
        ///<inheritdoc/>
        public StringGraph Meet(StringGraph other)
        {
            // Try compatibility of prefixes
            PrefixVisitor prefixes = new PrefixVisitor();
            Prefix thisPrefix = prefixes.Extract(root);
            Prefix otherPrefix = prefixes.Extract(other.root);

            Prefix meetPrefix = thisPrefix.Meet(otherPrefix);

            if (meetPrefix.IsBottom)
            {
                return Bottom;
            }

            // Try compatibility of suffixes
            SuffixVisitor suffixes = new SuffixVisitor();
            Suffix thisSuffix = suffixes.Extract(root);
            Suffix otherSuffix = suffixes.Extract(other.root);

            Suffix meetSuffix = thisSuffix.Meet(otherSuffix);

            if (meetSuffix.IsBottom)
            {
                return Bottom;
            }

            // Try compatibility of lengths
            LengthVisitor lengths = new LengthVisitor();
            lengths.ComputeLengthsFor(root);
            lengths.ComputeLengthsFor(other.root);
            IndexInterval thisLength = lengths.GetLengthFor(root);
            IndexInterval otherLength = lengths.GetLengthFor(other.root);
            IndexInterval meetLength = thisLength.Meet(otherLength);
            if (meetLength.IsBottom)
            {
                return Bottom;
            }

            // Try returning the more restrictive element
            // according to the computed properties
            if (!meetPrefix.IsTop)
            {
                return thisPrefix.LessThanEqual(otherPrefix) ? this : other;
            }
            if (!meetSuffix.IsTop)
            {
                return thisSuffix.LessThanEqual(otherSuffix) ? this : other;
            }
            if (otherLength.LessEqual(thisLength))
            {
                return other;
            }

            return this;
        }
        ///<inheritdoc/>
        public StringGraph Constant(string cst)
        {
            return ForString(cst);
        }
        #endregion

        #region IAbstractDomain implementation

        public bool LessEqual(IAbstractDomain a)
        {
            return LessThanEqual((StringGraph)a);
        }

        IAbstractDomain IAbstractDomain.Bottom
        {
            get { return ForBottom; }
        }

        IAbstractDomain IAbstractDomain.Top
        {
            get { return ForMax; }
        }

        public IAbstractDomain Join(IAbstractDomain a)
        {
            return Join((StringGraph)a);
        }

        public IAbstractDomain Meet(IAbstractDomain a)
        {
            return Meet((StringGraph)a);
        }

        public IAbstractDomain Widening(IAbstractDomain prev)
        {
            return Top;
        }

        public T To<T>(IFactory<T> factory)
        {
            return factory.Constant(true);
        }

        public object Clone()
        {
            return new StringGraph(root);
        }
        #endregion
        #region Object overrides
        public override string ToString()
        {
            return root.ToString();
        }
        #endregion

        /// <summary>
        /// Implements operations for the StringGraph abstract domain. 
        /// </summary>
        /// <typeparam name="Variable">The type of variables used to create predicates.</typeparam>
        public class Operations<Variable> : IStringOperations<StringGraph, Variable>
          where Variable : class, IEquatable<Variable>
        {
            #region Operations returning strings

            private StringGraph ToResultStringGraph(Node node)
            {
                CompactVisitor compacter = new CompactVisitor();
                return new StringGraph(compacter.Compact(node));
            }

            ///<inheritdoc/>
            public StringGraph Concat(WithConstants<StringGraph> left, WithConstants<StringGraph> right)
            {
                StringGraph leftGraph = left.ToAbstract(this);
                StringGraph rightGraph = right.ToAbstract(this);

                StringGraph result = ForConcat(leftGraph, rightGraph);

                return ToResultStringGraph(result.root);
            }
            ///<inheritdoc/>
            public StringGraph Insert(WithConstants<StringGraph> self, IndexInterval index, WithConstants<StringGraph> other)
            {
                GraphSlicer slicer = new GraphSlicer(self.ToAbstract(this).root);
                Node graphBefore = slicer.Before(index);
                Node graphAfter = slicer.After(index);
                Node inserted = other.ToAbstract(this).root;

                return ForConcat(graphBefore, inserted, graphAfter);
            }
            ///<inheritdoc/>
            public StringGraph Replace(StringGraph self, CharInterval from, CharInterval to)
            {
                ReplaceCharVisitor replaceVisitor = new ReplaceCharVisitor(from, () => NodeBuilder.CreateNodeForInterval(to));
                return ToResultStringGraph(replaceVisitor.ReplaceIn(self.root));
            }
            ///<inheritdoc/>
            public StringGraph Replace(WithConstants<StringGraph> self, WithConstants<StringGraph> from, WithConstants<StringGraph> to)
            {
                return Top;
            }
            ///<inheritdoc/>
            public StringGraph Substring(StringGraph self, IndexInterval index, IndexInterval length)
            {
                GraphSlicer slicer = new GraphSlicer(self.root);
                Node graphAfter = slicer.After(index);
                if (!length.LowerBound.IsInfinite)
                {
                    GraphSlicer afterSlicer = new GraphSlicer(graphAfter);
                    graphAfter = afterSlicer.Before(length);
                }
                return ToResultStringGraph(graphAfter);
            }
            ///<inheritdoc/>
            public StringGraph Remove(StringGraph self, IndexInterval index, IndexInterval length)
            {
                GraphSlicer slicer = new GraphSlicer(self.root);
                Node graphBefore = slicer.Before(index);
                if (length.LowerBound.IsInfinite)
                {
                    return new StringGraph(graphBefore);
                }
                else
                {
                    Node graphAfter = slicer.After(index);
                    GraphSlicer afterSlicer = new GraphSlicer(graphAfter);
                    graphAfter = afterSlicer.After(length);

                    return ForConcat(graphBefore, graphAfter);
                }
            }
            ///<inheritdoc/>
            public StringGraph PadLeftRight(StringGraph self, IndexInterval length, CharInterval fill, bool right)
            {
                IndexInterval lengthBefore = GetLength(self);

                if (length.UpperBound > lengthBefore.LowerBound)
                {

                    ConcatNode padded = new ConcatNode();
                    if (right)
                    {
                        padded.children.Add(self.root);
                        padded.children.Add(NodeBuilder.CreatePaddingNode(fill, length));
                    }
                    else
                    {
                        padded.children.Add(NodeBuilder.CreatePaddingNode(fill, length));
                        padded.children.Add(self.root);
                    }
                    return ToResultStringGraph(padded);
                }
                else
                {
                    return self;
                }
            }

            private string ArgumentToConstant(WithConstants<StringGraph> argument)
            {
                if (argument.IsConstant)
                {
                    return argument.Constant;
                }
                else
                {
                    Node argumentRoot = argument.Abstract.root;
                    ConstantsVisitor constants = new ConstantsVisitor();
                    constants.ComputeConstantsFor(argumentRoot);
                    return constants.GetConstantFor(argumentRoot);
                }
            }
            private Prefix ArgumentToPrefix(WithConstants<StringGraph> argument)
            {
                if (argument.IsConstant)
                {
                    return new Prefix(argument.Constant);
                }
                else
                {
                    Node argumentRoot = argument.Abstract.root;
                    PrefixVisitor prefixVisitor = new PrefixVisitor();
                    return prefixVisitor.Extract(argumentRoot);
                }
            }
            private Suffix ArgumentToSuffix(WithConstants<StringGraph> argument)
            {
                if (argument.IsConstant)
                {
                    return new Suffix(argument.Constant);
                }
                else
                {
                    Node argumentRoot = argument.Abstract.root;
                    SuffixVisitor suffixVisitor = new SuffixVisitor();
                    return suffixVisitor.Extract(argumentRoot);
                }
            }

            private StringGraph Trim(WithConstants<StringGraph> self, WithConstants<StringGraph> trimmed, bool start, bool end)
            {
                string trimmedConstant = ArgumentToConstant(trimmed);
                if (trimmedConstant == null)
                {
                    return Top;
                }

                HashSet<char> trimmedSet = new HashSet<char>(trimmedConstant);

                Node root = self.ToAbstract(this).root;

                if (start)
                {
                    TrimStartVisitor trimVisitor = new TrimStartVisitor(trimmedSet);
                    root = trimVisitor.Trim(root);
                }
                if (end)
                {
                    TrimEndVisitor trimVisitor = new TrimEndVisitor(trimmedSet);
                    root = trimVisitor.Trim(root);
                }
                return ToResultStringGraph(root);
            }
            ///<inheritdoc/>
            public StringGraph Trim(WithConstants<StringGraph> self, WithConstants<StringGraph> trimmed)
            {
                return Trim(self, trimmed, true, true);
            }
            ///<inheritdoc/>
            public StringGraph TrimStartEnd(WithConstants<StringGraph> self, WithConstants<StringGraph> trimmed, bool end)
            {
                return Trim(self, trimmed, !end, end);
            }
            ///<inheritdoc/>
            public StringGraph SetCharAt(StringGraph self, IndexInterval index, CharInterval value)
            {
                GraphSlicer slicer = new GraphSlicer(self.root);
                Node graphBefore = slicer.Before(index);
                IndexInt one = IndexInt.For(1);
                Node graphAfter = slicer.After(IndexInterval.For(index.LowerBound + one, index.UpperBound + one));
                return ForConcat(graphBefore, NodeBuilder.CreateNodeForInterval(value), graphAfter);
            }
            #endregion

            #region Operations returning bool
            ///<inheritdoc/>
            public IStringPredicate IsEmpty(StringGraph self, Variable selfVariable)
            {
                IndexInterval length = GetNodeLength(self.root);
                if (length.UpperBound == IndexInt.For(0))
                {
                    return FlatPredicate.True;
                }
                else if (length.LowerBound > IndexInt.For(0))
                {
                    return FlatPredicate.False;
                }
                else if (selfVariable != null)
                {
                    return StringAbstractionPredicate.ForTrue(selfVariable, ForConcat(new Node[] { }));
                }
                else
                {
                    return FlatPredicate.Top;
                }
            }
            ///<inheritdoc/>
            public IStringPredicate Contains(WithConstants<StringGraph> self, Variable selfVariable, WithConstants<StringGraph> other, Variable otherVariable)
            {
                string otherConstant = ArgumentToConstant(other);
                if (otherConstant != null)
                {
                    ContainsVisitor containsVisitor = new ContainsVisitor(otherConstant);
                    if (containsVisitor.MustContain(self.ToAbstract(this).root))
                    {
                        return FlatPredicate.True;
                    }
                }
                MaxNode max = new MaxNode();
                StringGraph template = ForConcat(max, other.ToAbstract(this).root, max);

                return IsMatchTemplate(self, selfVariable, template);
            }

            private IStringPredicate IsMatchTemplate(WithConstants<StringGraph> self, Variable selfVariable, StringGraph template)
            {
                StringGraph match = self.ToAbstract(this).Meet(template);
                if (match.IsBottom)
                {
                    return FlatPredicate.False;
                }
                else if (selfVariable != null)
                {
                    return StringAbstractionPredicate.ForTrue(selfVariable, template);
                }
                else
                {
                    return FlatPredicate.Top;
                }
            }
            ///<inheritdoc/>
            public IStringPredicate StartsEndsWithOrdinal(WithConstants<StringGraph> self, Variable selfVariable, WithConstants<StringGraph> other, Variable otherVariable, bool ends)
            {
                string otherConstant = ArgumentToConstant(other);
                if (otherConstant != null)
                {
                    if (ends)
                    {
                        Suffix selfSuffix = ArgumentToSuffix(self);

                        if (selfSuffix.suffix.EndsWith(otherConstant, StringComparison.Ordinal))
                        {
                            return FlatPredicate.True;
                        }
                        else if (!selfSuffix.ContainsValue(otherConstant))
                        {
                            return FlatPredicate.False;
                        }
                    }
                    else
                    {
                        Prefix selfPrefix = ArgumentToPrefix(self);
                        if (selfPrefix.prefix.StartsWith(otherConstant, StringComparison.Ordinal))
                        {
                            return FlatPredicate.True;
                        }
                        else if (!selfPrefix.ContainsValue(otherConstant))
                        {
                            return FlatPredicate.False;
                        }
                    }
                }

                StringGraph template;
                if (ends)
                {
                    template = ForConcat(new MaxNode(), other.ToAbstract(this).root);
                }
                else
                {
                    template = ForConcat(other.ToAbstract(this).root, new MaxNode());
                }

                return IsMatchTemplate(self, selfVariable, template);
            }
            ///<inheritdoc/>
            public IStringPredicate Equals(WithConstants<StringGraph> self, Variable selfVariable, WithConstants<StringGraph> other, Variable otherVariable)
            {
                string selfConstant = ArgumentToConstant(self);
                if (selfConstant != null && selfConstant == ArgumentToConstant(other))
                {
                    return FlatPredicate.True;
                }

                if (selfVariable == null)
                {
                    StringGraph template = ForConcat(self.ToAbstract(this).root);
                    return IsMatchTemplate(other, otherVariable, template);
                }
                else
                {
                    StringGraph template = ForConcat(other.ToAbstract(this).root);
                    return IsMatchTemplate(self, selfVariable, template);
                }
            }
            #endregion
            #region Operations returning int

            private class DummyVariable : IEquatable<DummyVariable>
            {
                public bool Equals(DummyVariable other)
                {
                    return other != null;
                }
            }

            ///<inheritdoc/>
            public CompareResult CompareOrdinal(WithConstants<StringGraph> self, WithConstants<StringGraph> other)
            {
                Prefix selfPrefix = ArgumentToPrefix(self);
                Prefix otherPrefix = ArgumentToPrefix(other);
                return new Prefix.Operations<DummyVariable>().CompareOrdinal(new WithConstants<Prefix>(selfPrefix), new WithConstants<Prefix>(otherPrefix));
            }

            private IndexInterval GetNodeLength(Node node)
            {
                LengthVisitor lengths = new LengthVisitor();
                lengths.ComputeLengthsFor(node);
                return lengths.GetLengthFor(node);
            }
            ///<inheritdoc/>
            public IndexInterval GetLength(StringGraph self)
            {
                return GetNodeLength(self.root);
            }
            ///<inheritdoc/>
            public IndexInterval IndexOf(WithConstants<StringGraph> self, WithConstants<StringGraph> needle, IndexInterval offset, IndexInterval count, bool last)
            {
                if (!offset.IsFiniteConstant || offset.LowerBound != 0 || !count.IsInfinity)
                {
                    // Offset and count are not supported
                    return IndexInterval.Unknown;
                }

                IndexInterval selfLength = GetNodeLength(self.ToAbstract(this).root);

                string needleConstant = ArgumentToConstant(needle);
                if (needleConstant == "")
                {
                    if (last)
                    {
                        IndexInt one = IndexInt.ForNonNegative(1);
                        IndexInt minIndex = IndexInt.Max(selfLength.LowerBound, one) - one;
                        IndexInt maxIndex = IndexInt.Max(selfLength.UpperBound, one) - one;
                        return IndexInterval.For(minIndex, maxIndex);
                    }
                    else
                    {
                        return IndexInterval.For(0);
                    }
                }

                IndexInterval needleLength = GetNodeLength(self.ToAbstract(this).root);

                return IndexInterval.For(IndexInt.Negative, selfLength.UpperBound - needleLength.LowerBound);
            }
            
            ///<inheritdoc/>
            public CharInterval GetCharAt(StringGraph self, IndexInterval index)
            {
                GraphSlicer slicer = new GraphSlicer(self.root);
                return slicer.CharAt(index);
            }
            #endregion
            #region Regex operations
            ///<inheritdoc/>
            public IStringPredicate RegexIsMatch(StringGraph self, Variable selfVariable, Microsoft.Research.Regex.Model.Element regex)
            {
                StringGraphRegex stringGraphRegexConverter = new StringGraphRegex(self);

                ProofOutcome isMatchOutcome = stringGraphRegexConverter.IsMatch(regex);

                if (isMatchOutcome != ProofOutcome.Top || selfVariable == null)
                {
                    return FlatPredicate.ForProofOutcome(isMatchOutcome);
                }
                else
                {
                    StringGraph graph = stringGraphRegexConverter.StringGraphForRegex(regex);

                    return StringAbstractionPredicate.ForTrue(selfVariable, graph);
                }
            }
            ///<inheritdoc/>
            public IEnumerable<Microsoft.Research.Regex.Model.Element> ToRegex(StringGraph self)
            {
                return new StringGraphRegex(self).GetRegex();
            }
            #endregion

            #region Factory methods
            ///<inheritdoc/>
            public StringGraph Top
            {
                get { return StringGraph.ForMax; }
            }
            ///<inheritdoc/>
            public StringGraph Constant(string constant)
            {
                return StringGraph.ForString(constant);
            }
            #endregion
        }
    }
}
