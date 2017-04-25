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
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.DataStructures;

namespace Microsoft.Research.AbstractDomains.Strings
{
  
    /// <summary>
    /// Represents an element of Character Inclusion abstract domain.
    /// </summary>
    /// <remarks>
    /// The elements of the domain are sets of allowed and mandatory characters.
    /// The set of mandatory characters is a subset of the set of allowed characters,
    /// the only exception is the bottom element.
    /// </remarks>
    public class CharacterInclusion<CharacterSet> : IStringInterval<CharacterInclusion<CharacterSet>>
        where CharacterSet : ICharacterSet<CharacterSet>
    {
        #region Internal state
        internal readonly ICharacterClassification classification;
        internal readonly CharacterSet mandatory, allowed;
        #endregion

        #region Internal helper methods
        /// <summary>
        /// Creates a <see cref="BitArray"/> with the correct length for this element.
        /// </summary>
        /// <param name="value">The value to which all elments of the array are initialized.</param>
        /// <returns>A new instance of <see cref="BitArray"/>.</returns>
        internal CharacterSet CreateCharacterSetFor(bool value)
        {
            return allowed.Create(value, classification.Buckets);
        }
        /// <summary>
        /// Creates a <see cref="BitArray"/> initialized using a string constant.
        /// </summary>
        /// <param name="constant">String constant containing characters that should be set in the array.</param>
        /// <returns>A new instance of <see cref="BitArray"/> where the elements corresponding to characters in
        /// <paramref name="constant"/> are set.</returns>
        internal CharacterSet CreateCharacterSetFor(string constant)
        {
            Contract.Requires(constant != null);

            CharacterSet array = allowed.Empty();
            AddCharactersFrom(array, constant);
            return array;
        }
        internal CharacterSet CreateCharacterSetFor(string constant, ICharacterSetFactory<CharacterSet> setFactory)
        {
            Contract.Requires(constant != null);
            Contract.Requires(setFactory != null);

            CharacterSet characterSet = setFactory.Create(false, classification.Buckets);
            AddCharactersFrom(characterSet, constant);
            return characterSet;
        }
        internal void AddCharactersFrom(CharacterSet array, string constant)
        {
            foreach (char character in constant)
            {
                array.Add(classification[character]);
            }
        }
        internal CharacterSet CreateCharacterSetFor(IEnumerable<CharInterval> intervals)
        {
            Contract.Requires(intervals != null);

            CharacterSet array = allowed.Empty();
            foreach (CharInterval interval in intervals)
            {
                for (int character = interval.LowerBound; character <= interval.UpperBound; ++character)
                    array.Add(classification[(char)character]);
            }
            return array;
        }
        private CharacterSet CreateCharacterSetFor(CharInterval interval)
        {
            CharacterSet array = allowed.Empty();
            for (int character = interval.LowerBound; character <= interval.UpperBound; ++character)
            {
                array.Add(classification[(char)character]);
            }
            return array;
        }

        internal static bool AllowedIntersects(CharacterInclusion<CharacterSet> setA, CharacterInclusion<CharacterSet> setB)
        {
            return setA.allowed.Intersects(setB.allowed);
        }
        internal static bool MandatorySubsetAllowed(CharacterInclusion<CharacterSet> setMandatory, CharacterInclusion<CharacterSet> setAllowed)
        {
            return setMandatory.mandatory.IsSubset(setAllowed.allowed);
        }

        #endregion

        #region Construction
        /// <summary>
        /// Constructs a trivial element of the Character Inclusion abstract domain.
        /// </summary>
        /// <param name="top">Whether the element should be top (<see langword="true"/>) or bottom (<see langword="false"/>).</param>
        /// <param name="classification">The character classification used in the abstract domain.</param>
        public CharacterInclusion(bool top, ICharacterClassification classification, ICharacterSetFactory<CharacterSet> setFactory)
        {
            Contract.Requires(classification != null);

            this.classification = classification;
            mandatory = setFactory.Create(!top, classification.Buckets);
            allowed = setFactory.Create(top, classification.Buckets);
        }
        /// <summary>
        /// Contsturcts an abstract element for the specified string constant.
        /// </summary>
        /// <param name="constant">The concrete constant string.</param>
        /// <param name="classification">The character classification used in the abstract domain.</param>
        public CharacterInclusion(string constant, ICharacterClassification classification, ICharacterSetFactory<CharacterSet> setFactory)
        {
            this.classification = classification;
            
            allowed = mandatory = CreateCharacterSetFor(constant, setFactory);
        }

        public CharacterInclusion(string mandatoryCharacters, string allowedCharacters, ICharacterClassification classification, ICharacterSetFactory<CharacterSet> setFactory)
        {
            this.classification = classification;
            this.mandatory = CreateCharacterSetFor(mandatoryCharacters, setFactory);
            this.allowed = CreateCharacterSetFor(allowedCharacters, setFactory);
        }

        private CharacterInclusion(CharacterSet mandatory, CharacterSet allowed, ICharacterClassification classification)
        {
            this.classification = classification;
            this.mandatory = mandatory;
            this.allowed = allowed;
        }

        public CharacterInclusion<CharacterSet> Empty
        {
            get
            {
                CharacterSet empty = CreateCharacterSetFor(false);
                return new CharacterInclusion<CharacterSet>(empty, empty, classification);
            }
        }
        public CharacterInclusion<CharacterSet> Constant(string c)
        {
            CharacterSet characterSet = CreateCharacterSetFor(c);
            return new CharacterInclusion<CharacterSet>(characterSet, characterSet, classification);
        }
        public CharacterInclusion<CharacterSet> FromDisallowed(IEnumerable<CharInterval> intervals)
        {
            //overapproximate all strings that do not contain a character from the intervals
            CharacterSet disallowed = CreateCharacterSetFor(intervals);
            for (int i = 0; i < classification.Buckets; ++i)
            {
                if (disallowed.Contains(i) && !classification.IsSingleton(i))
                {
                    // we allow non-singleton classes, because we do not know
                    // that all characters from the class were disallowed
                    disallowed.Remove(i);
                }
            }
            disallowed.Invert(classification.Buckets);

            return new CharacterInclusion<CharacterSet>(CreateCharacterSetFor(false), disallowed, classification);
        }
        public CharacterInclusion<CharacterSet> Character(IEnumerable<CharInterval> intervals, bool closed)
        {
            CharacterSet allowed = CreateCharacterSetFor(intervals);
            CharacterSet mandatory;
            if (allowed.IsSingleton)
            {
                mandatory = allowed;
            }
            else
            {
                mandatory = CreateCharacterSetFor(false);
            }
            if (!closed)
            {
                allowed = CreateCharacterSetFor(true);
            }
            return new CharacterInclusion<CharacterSet>(mandatory, allowed, classification);
        }

        #endregion

        #region Domain properties
        /// <summary>
        /// Gets the top element of the Character Inclusion domain.
        /// </summary>
        public CharacterInclusion<CharacterSet> Top
        {
            get
            {
                return new CharacterInclusion<CharacterSet>(CreateCharacterSetFor(false), CreateCharacterSetFor(true), classification);
            }
        }
        /// <summary>
        /// Gets the top element of the Character Inclusion domain.
        /// </summary>
        public CharacterInclusion<CharacterSet> Bottom
        {
            get
            {
                return new CharacterInclusion<CharacterSet>(CreateCharacterSetFor(true), CreateCharacterSetFor(false), classification);
            }
        }
        ///<inheritdoc/>
        public bool IsTop
        {
            get
            {
                return mandatory.IsEmpty && allowed.IsFull(classification.Buckets);
            }
        }
        ///<inheritdoc/>
        public bool IsBottom
        {
            get
            {
                return !mandatory.IsSubset(allowed);
            }
        }
        ///<inheritdoc/>
        public bool ContainsValue(string value)
        {
            CharacterSet notFound = mandatory.MutableClone();
            // Check that all characters of value are allowed
            foreach (char character in value)
            {
                if (!allowed.Contains(classification[character]))
                    return false;
                notFound.Remove(classification[character]);
            }
            
            return notFound.IsEmpty;
        }

        ///<inheritdoc/>
        public bool Equals(CharacterInclusion<CharacterSet> other)
        {
            return mandatory.Equals(other.mandatory) && allowed.Equals(other.allowed);
        }
        ///<inheritdoc/>
        public bool LessThanEqual(CharacterInclusion<CharacterSet> other)
        {
            return other.mandatory.IsSubset(mandatory) && allowed.IsSubset(other.allowed);
        }
        #endregion

        #region Public properties
        /// <summary>
        /// Determines whether the concrete string must be an empty string.
        /// </summary>
        public bool MustBeEmpty
        {
            get
            {
                return allowed.IsEmpty;
            }
        }
        /// <summary>
        /// Determines whether all the concrete strings are non-empty.
        /// </summary>
        public bool MustBeNonEmpty
        {
            get
            {
                return !mandatory.IsEmpty;
            }
        }
        /// <summary>
        /// Determines whether all the concrete strings must contain 
        /// a specified character.
        /// </summary>
        /// <param name="character">A character.</param>
        /// <returns>Whether all the concrete strings must contain <paramref name="character"/>.
        /// </returns>
        public bool MustContain(char character)
        {
            int bucket = classification[character];
            return mandatory.Contains(bucket) && classification.IsSingleton(bucket);
        }
        /// <summary>
        /// Determines whether some of the concrete strings can contain a specified
        /// character.
        /// </summary>
        /// <param name="character">A character.</param>
        /// <returns>Whether any of the concrete strings can contain <paramref name="character"/></returns>
        public bool CanContain(char character)
        {
            return allowed.Contains(classification[character]);
        }
        #endregion

        #region Domain operations
        ///<inheritdoc/>
        public CharacterInclusion<CharacterSet> Join(CharacterInclusion<CharacterSet> other)
        {
            Contract.Requires(classification == other.classification);

            CharacterSet newMandatory = mandatory.Intersection(other.mandatory);
            CharacterSet newAllowed = allowed.Union(other.allowed);

            return new CharacterInclusion<CharacterSet>(newMandatory, newAllowed, classification);
        }
        ///<inheritdoc/>
        public CharacterInclusion<CharacterSet> Meet(CharacterInclusion<CharacterSet> other)
        {
            Contract.Requires(classification == other.classification);

            CharacterSet newMandatory = mandatory.Union(other.mandatory);
            CharacterSet newAllowed = allowed.Intersection(other.allowed);

            return new CharacterInclusion<CharacterSet>(newMandatory, newAllowed, classification);
        }
        #endregion

        #region Public operations
        /// <summary>
        /// Computes abstraction for strings created by combining the characters
        /// with characters from another string (for example concatenation).
        /// </summary>
        /// <param name="other">Abstraction of the other string.</param>
        /// <returns>Abstraction for combinations of this string and <paramref name="other"/>.</returns>
        public CharacterInclusion<CharacterSet> Combine(CharacterInclusion<CharacterSet> other)
        {
            Contract.Requires(classification == other.classification);

            if (IsBottom)
                return this;
            if (other.IsBottom)
                return other;

            CharacterSet newMandatory = mandatory.Union(other.mandatory);
            CharacterSet newAllowed = allowed.Union(other.allowed);

            return new CharacterInclusion<CharacterSet>(newMandatory, newAllowed, classification);
        }
        #endregion

        /// <summary>
        /// Implements string operations for the Character Inclusion abstract domain.
        /// </summary>
        /// <typeparam name="Variable">The type representing variables.</typeparam>
        public class Operations<Variable> : IStringOperations<CharacterInclusion<CharacterSet>, Variable>
          where Variable : class, IEquatable<Variable>
        {
            private readonly ICharacterClassification classification;
            private readonly ICharacterSetFactory<CharacterSet> setFactory;

            /// <summary>
            /// Construct operations for <see cref="CharacterInclusion"/> elements 
            /// with the specified character classification.
            /// </summary>
            /// <param name="classification">The character classifcation used by the elements.</param>
            public Operations(ICharacterClassification classification, ICharacterSetFactory<CharacterSet> setFactory)
            {
                this.classification = classification;
                this.setFactory = setFactory;
            }
            #region Factory
            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> Top
            {
                get { return new CharacterInclusion<CharacterSet>(true, classification, setFactory); }
            }
            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> Constant(string constant)
            {
                return new CharacterInclusion<CharacterSet>(constant, classification, setFactory);
            }
            #endregion

            private CharacterInclusion<CharacterSet> For(CharacterSet required, CharacterSet allowed)
            {
                return new CharacterInclusion<CharacterSet>(required, allowed, classification);
            }

            #region Operations returning strings
            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> Concat(WithConstants<CharacterInclusion<CharacterSet>> left, WithConstants<CharacterInclusion<CharacterSet>> right)
            {
                return left.ToAbstract(this).Combine(right.ToAbstract(this));
            }
            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> Insert(WithConstants<CharacterInclusion<CharacterSet>> self, IndexInterval index, WithConstants<CharacterInclusion<CharacterSet>> other)
            {
                IndexInt lowestIndex = index.LowerBound;

                if (self.IsConstant)
                {
                    // If the string is constant, index must be at most its length
                    if (lowestIndex > self.Constant.Length)
                        return new CharacterInclusion<CharacterSet>(false, classification, setFactory);
                }
                else
                {
                    // If the string is empty, index must be 0
                    if (self.Abstract.MustBeEmpty && lowestIndex > 0)
                        return new CharacterInclusion<CharacterSet>(false, classification, setFactory);
                }

                return self.ToAbstract(this).Combine(other.ToAbstract(this));
            }
            private static BitArray ReplaceIn(BitArray ba, int from, int to)
            {
                if (ba[from])
                {
                    BitArray n = new BitArray(ba);
                    n[from] = false;
                    n[to] = true;
                    return n;
                }
                else
                    return ba;
            }

            private bool IsSingleCharInterval(CharInterval interval)
            {
                return interval.IsConstant && classification.IsSingleton(classification[interval.LowerBound]);
            }

            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> Replace(CharacterInclusion<CharacterSet> self, CharInterval from, CharInterval to)
            {
                CharacterSet fromArray = self.CreateCharacterSetFor(from);
                CharacterSet toArray = self.CreateCharacterSetFor(to);

                CharacterSet newAllowed = self.allowed;

                if (newAllowed.Intersects(fromArray))
                {
                    // Copy the array so that it can be modified
                    newAllowed = newAllowed.MutableClone();

                    // Some character may be replaced
                    if (IsSingleCharInterval(from))
                    {
                        // We know exactly which one it is,
                        // so we can remove it
                        newAllowed.Remove(classification[from.LowerBound]);
                    }

                    // We must add the possible replacements
                    newAllowed.UnionWith(toArray);
                }

                // Copy the array so that it can be modified
                CharacterSet newMandatory = self.mandatory.MutableClone();

                // If all of the possible replaced characters are mandatory,
                // and do not have equivalents, replacement must take place.
                bool mustReplace = true;
                for (int character = from.LowerBound; character <= from.UpperBound; ++character)
                {
                    if (!self.MustContain((char)character))
                    {
                        mustReplace = false;
                        break;
                    }
                }

                // Invert the from array (mutable)
                fromArray.Invert(classification.Buckets);
                newMandatory.IntersectWith(fromArray);

                if (mustReplace && to.IsConstant)
                {
                    // Replacement must take place and we know what
                    // the replacement is
                    newMandatory.Add(classification[to.LowerBound]);
                }

                return new CharacterInclusion<CharacterSet>(newMandatory, newAllowed, classification);
            }
            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> Replace(
              WithConstants<CharacterInclusion<CharacterSet>> self,
              WithConstants<CharacterInclusion<CharacterSet>> from,
              WithConstants<CharacterInclusion<CharacterSet>> to)
            {
                CharacterInclusion<CharacterSet> selfSet = self.ToAbstract(this);
                CharacterInclusion<CharacterSet> fromSet = from.ToAbstract(this);
                CharacterInclusion<CharacterSet> toSet = to.ToAbstract(this);

                if (!CanContain(selfSet, fromSet))
                {
                    return selfSet;
                }

                CharacterSet newAllowed = selfSet.allowed.Union(toSet.allowed);
                CharacterSet newMandatory = selfSet.mandatory.Except(fromSet.allowed);

                return new CharacterInclusion<CharacterSet>(newMandatory, newAllowed, classification);
            }

            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> Substring(CharacterInclusion<CharacterSet> self, IndexInterval index, IndexInterval length)
            {
                bool lengthSpecified = !length.LowerBound.IsInfinite;

                // The string must be non-empty, because the index is not zero or
                // the length is specified and not zero
                bool mustNotBeEmpty = (index.LowerBound > 0 || (lengthSpecified && length.LowerBound > 0));

                bool empty = length.IsFiniteConstant && length.UpperBound == 0;
                bool full = !lengthSpecified && index.IsFiniteConstant && index.UpperBound == 0;
                bool willNotBeEmpty = lengthSpecified && length.LowerBound > 0;

                return self.Part(mustNotBeEmpty, empty, willNotBeEmpty, full);
            }

            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> Remove(CharacterInclusion<CharacterSet> self, IndexInterval index, IndexInterval length)
            {
                bool lengthSpecified = !length.LowerBound.IsInfinite;

                // If the index or length are not zero, or the length is not specified,
                // the string must not be empty.
                bool mustNotBeEmpty = index.LowerBound > 0 || length.LowerBound > 0;

                bool empty = !lengthSpecified && index.IsFiniteConstant && index.UpperBound == 0;
                bool full = length.IsFiniteConstant && length.UpperBound == 0;
                bool willNotBeEmpty = index.LowerBound > 0;

                return self.Part(mustNotBeEmpty, empty, willNotBeEmpty, full);
            }

            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> PadLeftRight(CharacterInclusion<CharacterSet> self, IndexInterval length, CharInterval fill, bool right)
            {
                return Pad(self, length, fill);
            }

            /// <summary>
            /// Determines whether the bit array represents exactly one character.
            /// </summary>
            /// <param name="set">A set of character classes.</param>
            /// <returns>Whether the character set allows exactly one character class with
            /// exactly one character.</returns>
            private bool IsSingletonSet(CharacterSet set)
            {
                bool found = false;
                for (int i = 0; i < classification.Buckets; ++i)
                {
                    if (set.Contains(i))
                    {
                        if (found)
                        {
                            // found more than one class
                            return false;
                        }
                        else
                        {
                            found = true;
                            if (!classification.IsSingleton(i))
                            {
                                // found class with more characters
                                return false;
                            }
                        }
                    }
                }

                return found;
            }

            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> Trim(WithConstants<CharacterInclusion<CharacterSet>> self, WithConstants<CharacterInclusion<CharacterSet>> trimmed)
            {
                CharacterInclusion<CharacterSet> selfSet = self.ToAbstract(this);
                CharacterInclusion<CharacterSet> trimmedSet = trimmed.ToAbstract(this);

                if (selfSet.allowed.IsSubset(trimmedSet.mandatory) && IsSingletonSet(selfSet.allowed))
                {
                    // If all allowed characters are provably trimmed, the result is empty string
                    return selfSet.Empty;
                }
                else
                {
                    // Otherwise, keep in mandatory set those, which are not allowed in the trimmed set
                    CharacterSet newMandatory = selfSet.mandatory.Except(trimmedSet.allowed);

                    return new CharacterInclusion<CharacterSet>(newMandatory, selfSet.allowed, classification);
                }
            }
            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> TrimStartEnd(WithConstants<CharacterInclusion<CharacterSet>> self, WithConstants<CharacterInclusion<CharacterSet>> trimmed, bool end)
            {
                return Trim(self, trimmed);
            }
            #endregion

            #region Predicate operations
            ///<inheritdoc/>
            public IStringPredicate IsEmpty(CharacterInclusion<CharacterSet> self, Variable selfVariable)
            {
                if (self.MustBeEmpty)
                {
                    return FlatPredicate.True;
                }

                if (self.MustBeNonEmpty)
                {
                    return FlatPredicate.False;
                }

                // We can assume empty
                if (selfVariable != null)
                {
                    return StringAbstractionPredicate.ForTrue(selfVariable, Constant(""));
                }
                else
                {
                    return FlatPredicate.Top;
                }
            }

            private IStringPredicate ContainCommon(
              WithConstants<CharacterInclusion<CharacterSet>> self, Variable selfVariable,
              WithConstants<CharacterInclusion<CharacterSet>> other, Variable otherVariable, bool any)
            {
                // any = containment is true if other is anywhere

                CharacterInclusion<CharacterSet> selfSet = self.ToAbstract(this);

                if (!other.IsConstant)
                {
                    CharacterInclusion<CharacterSet> otherSet = other.Abstract;

                    if (otherSet.MustBeEmpty)
                    {
                        return FlatPredicate.True;
                    }
                    if (!otherSet.mandatory.IsSubset(selfSet.allowed))
                    {
                        return FlatPredicate.False;
                    }

                    //NOTE: if both variables are available, we could theoretically
                    // create predicate on both..
                    if (selfVariable != null)
                    {
                        //if we assume the predicate, then we must contain all mandatory chars from the substring
                        CharacterInclusion<CharacterSet> trueAbstraction = Extend(otherSet);
                        return StringAbstractionPredicate.ForTrue(selfVariable, trueAbstraction);
                    }
                    else if (otherVariable != null)
                    {
                        // if we assume the predicate, then the var must only contain allowed chars from here
                        CharacterInclusion<CharacterSet> trueAbstraction = selfSet.Part(false, false, false, false);
                        return StringAbstractionPredicate.ForTrue(otherVariable, trueAbstraction);
                    }
                    else
                    {
                        return FlatPredicate.Top;
                    }
                }
                else
                {
                    string otherConstant = other.Constant;
                    int otherLength = otherConstant.Length;
                    if (otherLength == 0)
                    {
                        // Empty string is always contained
                        return FlatPredicate.True;
                    }
                    else if (otherConstant.Length == 1)
                    {
                        // We are testing for containment of a single character
                        char character = otherConstant[0];
                        int characterBucket = classification[character];

                        if (!selfSet.allowed.Contains(characterBucket))
                        {
                            // The character is not allowed
                            return FlatPredicate.False;
                        }
                        if (selfSet.mandatory.Contains(characterBucket) && classification.IsSingleton(characterBucket))
                        {
                            // The character is mandatory in the string
                            if (any || selfSet.allowed.IsSingleton)
                            {
                                // We are looking for any occurence, or no other characters are allowed
                                return FlatPredicate.True;
                            }
                        }

                        if (selfVariable != null)
                        {
                            CharacterSet full = selfSet.CreateCharacterSetFor(true);
                            CharacterSet empty = selfSet.CreateCharacterSetFor(false);
                            CharacterSet with = empty.With(characterBucket);
                            CharacterSet without = with.Inverted(classification.Buckets);

                            CharacterInclusion<CharacterSet> trueAbstraction = new CharacterInclusion<CharacterSet>(with, full, classification);
                            CharacterInclusion<CharacterSet> falseAbstraction = new CharacterInclusion<CharacterSet>(empty, without, classification);

                            return StringAbstractionPredicate.For(selfVariable, trueAbstraction, falseAbstraction);
                        }
                        else
                        {
                            return FlatPredicate.Top;
                        }
                    }
                    else
                    {
                        foreach (char character in otherConstant)
                        {
                            if (!selfSet.allowed.Contains(selfSet.classification[character]))
                                return FlatPredicate.False;
                        }

                        // If the predicate is true, characters from the constant are mandatory

                        if (selfVariable != null)
                        {
                            CharacterSet newMandatory = selfSet.CreateCharacterSetFor(otherConstant);
                            CharacterSet full = selfSet.CreateCharacterSetFor(true);

                            CharacterInclusion<CharacterSet> trueAbstraction = new CharacterInclusion<CharacterSet>(newMandatory, full, classification);
                            return StringAbstractionPredicate.ForTrue(selfVariable, trueAbstraction);
                        }
                        else
                        {
                            return FlatPredicate.Top;
                        }
                    }
                }
            }

            ///<inheritdoc/>
            public IStringPredicate Contains(
              WithConstants<CharacterInclusion<CharacterSet>> self, Variable selfVariable,
              WithConstants<CharacterInclusion<CharacterSet>> other, Variable otherVariable)
            {
                return ContainCommon(self, selfVariable, other, otherVariable, true);
            }
            ///<inheritdoc/>
            public IStringPredicate StartsEndsWithOrdinal(
              WithConstants<CharacterInclusion<CharacterSet>> self, Variable selfVariable,
              WithConstants<CharacterInclusion<CharacterSet>> other, Variable otherVariable, bool ends)
            {
                return ContainCommon(self, selfVariable, other, otherVariable, false);
            }
  
            ///<inheritdoc/>
            public IStringPredicate Equals(
              WithConstants<CharacterInclusion<CharacterSet>> self, Variable selfVariable,
              WithConstants<CharacterInclusion<CharacterSet>> other, Variable otherVariable)
            {
                CharacterInclusion<CharacterSet> selfSet = self.ToAbstract(this);
                CharacterInclusion<CharacterSet> otherSet = other.ToAbstract(this);

                if (selfSet.MustBeEmpty && otherSet.MustBeEmpty)
                {
                    // Both strings must be empty, so they are equal
                    return FlatPredicate.True;
                }
                else if (!selfSet.mandatory.IsSubset(otherSet.allowed) || !otherSet.mandatory.IsSubset(selfSet.allowed))
                {
                    //One string must contain characters that are not allowed in the other one
                    return FlatPredicate.False;
                }
                else
                {
                    if (selfVariable != null)
                    {
                        return StringAbstractionPredicate.ForTrue(selfVariable, otherSet);
                    }
                    else if (otherVariable != null)
                    {
                        return StringAbstractionPredicate.ForTrue(otherVariable, selfSet);
                    }
                    else
                    {
                        return FlatPredicate.Top;
                    }
                }
            }
            #endregion
            #region String operations returning integers

            private char Max(CharacterSet cs)
            {
                for(int i = char.MaxValue; i >= char.MinValue; --i)
                {
                    if (cs.Contains(classification[(char)i]))
                    {
                        return (char)i;
                    }
                }
                throw new InvalidOperationException();
            }
            private char Min(CharacterSet cs)
            {
                for (int i = char.MinValue; i <= char.MaxValue; ++i)
                {
                    if (cs.Contains(classification[(char)i]))
                    {
                        return (char)i;
                    }
                }
                throw new InvalidOperationException();
            }

            private bool CanBeLess(CharacterInclusion<CharacterSet> self, CharacterInclusion<CharacterSet> other)
            {
                // If other must be empty, self cannot ever be less
                if (other.MustBeEmpty)
                {
                    return false;
                }
                // If self can be empty (and other can be non-empty), self can be less
                if (!self.MustBeNonEmpty)
                {
                    return true;
                }

                // Now we know that self.mandatory, self.allowed and other.allowed are non-empty

                char maxOther = Max(other.allowed);
                char minSelf = Min(self.allowed);

                if (minSelf < maxOther)
                {
                    // Self can contain a character less than other
                    return true;
                }
                else
                {
                    char maxMandatory = Max(self.mandatory);
                    if (maxMandatory <= maxOther)
                    {
                        // Self can consist entirely of characters less than 
                        // or equal than other, and be shorter
                        return true;
                    }
                }

                return false;
            }
            ///<inheritdoc/>
            public CompareResult CompareOrdinal(WithConstants<CharacterInclusion<CharacterSet>> self, WithConstants<CharacterInclusion<CharacterSet>> other)
            {
                CharacterInclusion<CharacterSet> selfSet = self.ToAbstract(this);
                CharacterInclusion<CharacterSet> otherSet = other.ToAbstract(this);

                bool less = CanBeLess(selfSet, otherSet);
                bool greater = CanBeLess(otherSet, selfSet);
                bool equal = selfSet.mandatory.IsSubset(otherSet.allowed) && otherSet.mandatory.IsSubset(selfSet.allowed);

                return CompareResultExtensions.Build(less, equal, greater);
            }
            ///<inheritdoc/>
            public IndexInterval GetLength(CharacterInclusion<CharacterSet> self)
            {
                if (self.MustBeEmpty)
                {
                    return IndexInterval.For(0);
                }
                else
                {
                    return IndexInterval.For(IndexInt.For(self.mandatory.Count), IndexInt.Infinity);
                }
            }

            private bool CanContain(CharacterInclusion<CharacterSet> self, CharacterInclusion<CharacterSet> other)
            {
                return other.mandatory.IsSubset(self.allowed);
            }

            ///<inheritdoc/>
            public IndexInterval IndexOf(WithConstants<CharacterInclusion<CharacterSet>> self,
                WithConstants<CharacterInclusion<CharacterSet>> needle,
                IndexInterval offset, IndexInterval count, bool last)
            {
                if (!offset.IsFiniteConstant || offset.LowerBound != 0 || !count.IsInfinity)
                {
                    // Offset and count are not supported
                    return IndexInterval.Unknown;
                }

                CharacterInclusion<CharacterSet> selfSet = self.ToAbstract(this);

                if (needle.IsConstant)
                {
                    string needleString = needle.Constant;

                    if (needleString == "")
                    {
                        if (last)
                        {
                            return IndexInterval.For(IndexInt.For(0), IndexInt.Infinity);
                        }
                        else
                        {
                            return IndexInterval.For(0);
                        }
                    }
                    else if (needleString.Length == 1)
                    {
                        char needleChar = needleString[0];

                        if (!selfSet.CanContain(needleChar))
                        {
                            return IndexInterval.For(-1);
                        }
                        else if (selfSet.MustContain(needleChar))
                        {
                            if (!last && selfSet.allowed.Count == 1)
                                return IndexInterval.For(0);
                            else
                                return IndexInterval.For(IndexInt.For(0), IndexInt.Infinity);
                        }
                        else
                        {
                            if (!last && selfSet.allowed.Count == 1)
                            {
                                return IndexInterval.For(-1, 0);
                            }
                            else
                            {
                                return IndexInterval.For(IndexInt.For(-1), IndexInt.Infinity);
                            }
                        }

                    }
                }

                if (CanContain(selfSet, needle.ToAbstract(this)))
                {
                    return IndexInterval.Unknown;
                }
                else
                {
                    return IndexInterval.For(-1);
                }
            }

            #endregion
            #region Regex operations
            ///<inheritdoc/>
            public IStringPredicate RegexIsMatch(CharacterInclusion<CharacterSet> self, Variable selfVariable, Microsoft.Research.Regex.Model.Element regex)
            {
                CharacterInclusionRegex<CharacterSet> characterSetRegexConverter = new CharacterInclusionRegex<CharacterSet>(self);

                ProofOutcome isMatch = characterSetRegexConverter.IsMatch(regex);

                if (isMatch == ProofOutcome.Top && selfVariable != null)
                {
                    return characterSetRegexConverter.PredicateFromRegex(regex, selfVariable);
                }

                return FlatPredicate.ForProofOutcome(isMatch);
            }
            ///<inheritdoc/>
            public IEnumerable<Microsoft.Research.Regex.Model.Element> ToRegex(CharacterInclusion<CharacterSet> self)
            {
                return new CharacterInclusionRegex<CharacterSet>(self).GetRegex();
            }
            #endregion
            #region Array operations
            ///<inheritdoc/>
            public CharacterInclusion<CharacterSet> SetCharAt(CharacterInclusion<CharacterSet> self, IndexInterval index, CharInterval value)
            {
                CharacterSet values = self.CreateCharacterSetFor(value);

                // Allowed are the old and new characters
                CharacterSet allowed = self.allowed.Union(values);
                // Mandatory is only the new character, if it is a single bucket
                CharacterSet mandatory = values.IsSingleton ? values : self.CreateCharacterSetFor(false);

                return new CharacterInclusion<CharacterSet>(mandatory, allowed, classification);
            }
            ///<inheritdoc/>
            public CharInterval GetCharAt(CharacterInclusion<CharacterSet> self, IndexInterval index)
            {
                int min = char.MaxValue;
                int max = char.MinValue;

                for (int character = char.MinValue; character <= char.MaxValue; ++character)
                {
                    if (self.allowed.Contains(classification[(char)character]))
                    {
                        min = Math.Min(min, character);
                        max = Math.Max(max, character);
                    }
                }

                return CharInterval.For((char)min, (char)max);
            }
            #endregion


            internal CharacterInclusion<CharacterSet> Extend(CharacterInclusion<CharacterSet> self)
            {
                CharacterSet newAllowed = setFactory.Create(true, classification.Buckets);
                return new CharacterInclusion<CharacterSet>(self.mandatory, newAllowed, classification);
            }
            private CharacterInclusion<CharacterSet> Pad(CharacterInclusion<CharacterSet> self, IndexInterval length, CharInterval padding)
            {
                CharacterSet newMandatory = self.mandatory;
                CharacterSet newAllowed = self.allowed;

                if (length.LowerBound > 0 && padding.IsConstant)
                {

                    int allowedCount = self.allowed.Count;
                    int paddingClass = classification[padding.LowerBound];

                    if (allowedCount == 0 || (allowedCount == 1 && self.allowed.Contains(paddingClass)))
                    {
                        // We want to pad, the padding is known
                        // and no other characters than padding are allowed
                        // therefore the character must occur.

                        newMandatory = newMandatory.With(paddingClass);
                    }
                }

                // If we already have at least as many characters as the maximum length,
                // no padding is possible.
                if (length.UpperBound > self.mandatory.Count)
                {
                    newAllowed = newAllowed.MutableClone();
                    for (int character = padding.LowerBound; character <= padding.UpperBound; ++character)
                    {
                        newAllowed.Add(classification[(char)character]);
                    }
                }

                return new CharacterInclusion<CharacterSet>(newMandatory, newAllowed, classification);
            }
        }
        /// <summary>
        /// Creates abstraction for a part of the string.
        /// </summary>
        /// <param name="mustNotBeEmpty">Whether the source string is required to be non-empty.</param>
        /// <param name="empty">Whether we take an empty part of the string.</param>
        /// <param name="willNotBeEmpty">Whether the result string is known to be non-empty.</param>
        /// <param name="full">Whether we take the whole string.</param>
        /// <returns>Abstraction for a part of a string.</returns>
        internal CharacterInclusion<CharacterSet> Part( bool mustNotBeEmpty, bool empty, bool willNotBeEmpty, bool full)
        {
            Contract.Requires(!(!mustNotBeEmpty && empty && full));
            Contract.Requires(!(empty && willNotBeEmpty));


            if (IsBottom)
                return this;

            if (mustNotBeEmpty && MustBeEmpty)
                return Bottom;

            if (willNotBeEmpty && allowed.IsSingleton)
            {
                // We know that the string will not be empty, but only one category
                // is allowed, so it is mandatory.
                return new CharacterInclusion<CharacterSet>(allowed, allowed, classification);
            }

            if (empty)
            {
                return Empty;
            }
            if (full)
            {
                return this;
            }

            CharacterSet newMandatory = CreateCharacterSetFor(false);
            return new CharacterInclusion<CharacterSet>(newMandatory, allowed, classification);
        }

        #region Object method override
        public override string ToString()
        {
            StringBuilder str = new StringBuilder();

            int mc = 0;

            for (int i = 0; i < classification.Buckets && mc < 50; ++i)
            {
                if (mandatory.Contains(i))
                {
                    str.Append(classification.ToString(i));
                    ++mc;
                }
            }

            str.Append(" ");

            mc = 0;

            for (int i = 0; i < classification.Buckets && mc < 50; ++i)
            {
                if (!mandatory.Contains(i) && allowed.Contains(i))
                {
                    str.Append(classification.ToString(i));
                    ++mc;
                }
            }

            return str.ToString();
        }
        public override bool Equals(object obj)
        {
            return Equals(obj as CharacterInclusion<CharacterSet>);
        }
        public override int GetHashCode()
        {
            return 0;
        }
#endregion
#region IAbstractDomain implementation
        bool IAbstractDomain.IsBottom
        {
            get { return IsBottom; }
        }

        bool IAbstractDomain.IsTop
        {
            get { return IsTop; }
        }

        bool IAbstractDomain.LessEqual(IAbstractDomain a)
        {
            return LessThanEqual(a as CharacterInclusion<CharacterSet>);
        }

        IAbstractDomain IAbstractDomain.Bottom
        {
            get { return Bottom; }
        }

        IAbstractDomain IAbstractDomain.Top
        {
            get { return Top; }
        }

        public IAbstractDomain Join(IAbstractDomain a)
        {
            return Join(a as CharacterInclusion<CharacterSet>);
        }

        public IAbstractDomain Meet(IAbstractDomain a)
        {
            return Meet(a as CharacterInclusion<CharacterSet>);
        }

        public IAbstractDomain Widening(IAbstractDomain prev)
        {
            return Join(prev as CharacterInclusion<CharacterSet>);
        }

        public object Clone()
        {
            return new CharacterInclusion<CharacterSet>(mandatory, allowed, classification);
        }
        public T To<T>(IFactory<T> factory)
        {
            return factory.Constant(true);
        }

#endregion

#region IStringInterval
        public bool CheckMustBeLessEqualThan(CharacterInclusion<CharacterSet> greaterEqual)
        {
            //Checks that all strings contain only those characters that all strings in greaterEqual contain.
            return allowed.IsSubset(greaterEqual.mandatory);
        }

        public bool TryRefineLessEqual(ref CharacterInclusion<CharacterSet> lessEqual)
        {
            // In lessEqual, keep the strings that contain only characters contained in all strings in this element
            CharacterSet newAllowed = lessEqual.allowed.Intersection(mandatory);
            if (newAllowed.Equals(lessEqual.allowed))
                return false;
            else
            {
                lessEqual = new CharacterInclusion<CharacterSet>(lessEqual.mandatory, newAllowed, lessEqual.classification);
                return true;
            }

        }

        public bool TryRefineGreaterEqual(ref CharacterInclusion<CharacterSet> greaterEqual)
        {
            CharacterSet newMandatory = greaterEqual.mandatory.Intersection(allowed);
            if (newMandatory.Equals(greaterEqual.mandatory))
                return false;
            else
            {
                greaterEqual = new CharacterInclusion<CharacterSet>(newMandatory, greaterEqual.allowed, greaterEqual.classification);
                return true;
            }
        }
#endregion
    }
}
