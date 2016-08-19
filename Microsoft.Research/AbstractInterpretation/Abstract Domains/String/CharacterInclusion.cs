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
  public class CharacterInclusion : IStringAbstraction<CharacterInclusion, string>
  {
    #region Internal state
    internal readonly ICharacterClassification classification;
    internal readonly BitArray mandatory, allowed;
    #endregion

    #region Internal helper methods
    /// <summary>
    /// Creates a <see cref="BitArray"/> with the correct length for this element.
    /// </summary>
    /// <param name="value">The value to which all elments of the array are initialized.</param>
    /// <returns>A new instance of <see cref="BitArray"/>.</returns>
    internal BitArray CreateBitArrayFor(bool value)
    {
      return new BitArray(classification.Buckets, value);
    }
    /// <summary>
    /// Creates a <see cref="BitArray"/> initialized using a string constant.
    /// </summary>
    /// <param name="constant">String constant containing characters that should be set in the array.</param>
    /// <returns>A new instance of <see cref="BitArray"/> where the elements corresponding to characters in
    /// <paramref name="constant"/> are set.</returns>
    internal BitArray CreateBitArrayFor(string constant)
    {
      Contract.Requires(constant != null);

      BitArray array = new System.Collections.BitArray(classification.Buckets);
      foreach (char character in constant)
      {
        array[classification[character]] = true;
      }
      return array;
    }

    internal BitArray CreateBitArrayFor(IEnumerable<CharInterval> intervals)
    {
      Contract.Requires(intervals != null);

      BitArray array = new System.Collections.BitArray(classification.Buckets);
      foreach (CharInterval interval in intervals)
      {
        for (int character = interval.LowerBound; character <= interval.UpperBound; ++character)
          array[classification[(char)character]] = true;
      }
      return array;
    }

    /// <summary>
    /// Compares two bit arrays of equal length.
    /// </summary>
    /// <param name="arrayA">The first bit array.</param>
    /// <param name="arrayB">The second bit array.</param>
    /// <returns><see langword="true"/>, if the array have the same bits set.</returns>
    internal static bool Equal(BitArray arrayA, BitArray arrayB)
    {
      Contract.Requires(arrayA != null && arrayB != null);
      Contract.Requires(arrayA.Length == arrayB.Length);

      for (int i = 0; i < arrayA.Length; ++i)
      {
        if (arrayA[i] != arrayB[i])
          return false;
      }
      return true;
    }

    /// <summary>
    /// Determines whether a bit array represents a subset
    /// of another bit array with the same length.
    /// </summary>
    /// <param name="arrayA">The first bit array.</param>
    /// <param name="arrayB">The second bit array.</param>
    /// <returns><see langword="true"/>, if all bits in <paramref name="arrayA"/>
    /// are also set in <paramref name="arrayB"/>.</returns>
    internal static bool Subset(BitArray arrayA, BitArray arrayB)
    {
      Contract.Requires(arrayA.Length == arrayB.Length);

      for (int i = 0; i < arrayA.Length; ++i)
      {
        if (arrayA[i] && !arrayB[i])
          return false;
      }
      return true;
    }

    /// <summary>
    /// Determines whether two bit array intersect.
    /// </summary>
    /// <param name="arrayA">The first bit array.</param>
    /// <param name="arrayB">The second bit array.</param>
    /// <returns><see langword="true"/> if there exists a bit
    /// that is set both in
    /// <paramref name="arrayA"/> and <paramref name="arrayB"/>.</returns>
    internal static bool Intersects(BitArray arrayA, BitArray arrayB)
    {
      Contract.Requires(arrayA.Length == arrayB.Length); 

      for (int i = 0; i < arrayA.Length; ++i)
      {
        if (arrayA[i] && arrayB[i])
          return true;
      }
      return false;
    }
    /// <summary>
    /// Counts the number of bits set in the array.
    /// </summary>
    /// <param name="array">An array of bits.</param>
    /// <returns>Number of set bits in <paramref name="array"/>.</returns>
    internal static int CountBits(BitArray array)
    {
      int bits = 0;
      for (int i = 0; i < array.Length; ++i)
      {
        if (array[i])
        {
          bits++;
        }
      }
      return bits;
    }
    /// <summary>
    /// Finds the index of the first bit set in an array.
    /// </summary>
    /// <param name="array">An array of bits.</param>
    /// <returns>The lowest index of a bit set in <paramref name="array"/>,
    /// or -1 if no bit is set.</returns>
    internal static int Min(BitArray array)
    {
      Contract.Requires(array != null);

      for (int i = 0; i < array.Length; ++i)
      {
        if (array[i])
        {
          return i;
        }
      }
      return -1;
    }
    /// <summary>
    /// Finds the index of the last bit set in an array.
    /// </summary>
    /// <param name="array">An array of bits.</param>
    /// <returns>The highest index of a bit set in <paramref name="array"/>,
    /// or -1 if no bit is set.</returns>
    internal static int Max(BitArray array)
    {
      Contract.Requires(array != null);

      for (int i = array.Length - 1; i >= 0; --i)
      {
        if (array[i])
        {
          return i;
        }
      }
      return -1;
    }
    /// <summary>
    /// Creates a new bit array by AND operation of two arrays.
    /// </summary>
    /// <param name="arrayA">The first bit array.</param>
    /// <param name="arrayB">The second bit array.</param>
    /// <returns>A new instance of bit array representing the conjunction of 
    /// <paramref name="arrayA"/> and <paramref name="arrayB"/>.
    /// </returns>
    internal static BitArray And(BitArray arrayA, BitArray arrayB)
    {
      BitArray newArray = new BitArray(arrayA);
      return newArray.And(arrayB);
    }
    /// <summary>
    /// Creates a new bit array by OR operation of two arrays.
    /// </summary>
    /// <param name="arrayA">The first bit array.</param>
    /// <param name="arrayB">The second bit array.</param>
    /// <returns>A new instance of bit array representing the disjunction of 
    /// <paramref name="arrayA"/> and <paramref name="arrayB"/>.
    /// </returns>
    internal static BitArray Or(BitArray arrayA, BitArray arrayB)
    {
      BitArray newArray = new BitArray(arrayA);
      return newArray.Or(arrayB);
    }

    internal static bool AllowedIntersects(CharacterInclusion setA, CharacterInclusion setB)
    {
      return Intersects(setA.allowed, setB.allowed);
    }
    internal static bool MandatorySubsetAllowed(CharacterInclusion setMandatory, CharacterInclusion setAllowed)
    {
      return Subset(setMandatory.mandatory, setAllowed.allowed);
    }

    #endregion

    #region Construction
    /// <summary>
    /// Constructs a trivial element of the Character Inclusion abstract domain.
    /// </summary>
    /// <param name="top">Whether the element should be top (<see langword="true"/>) or bottom (<see langword="false"/>).</param>
    /// <param name="classification">The character classification used in the abstract domain.</param>
    public CharacterInclusion(bool top, ICharacterClassification classification)
    {
      Contract.Requires(classification != null);

      this.classification = classification;
      mandatory = CreateBitArrayFor(!top);
      allowed = CreateBitArrayFor(top);
    }
    /// <summary>
    /// Contsturcts an abstract element for the specified string constant.
    /// </summary>
    /// <param name="constant">The concrete constant string.</param>
    /// <param name="classification">The character classification used in the abstract domain.</param>
    public CharacterInclusion(string constant, ICharacterClassification classification)
    {
      this.classification = classification;
      allowed = mandatory = CreateBitArrayFor(constant);
    }

    public CharacterInclusion(string mandatoryCharacters, string allowedCharacters, ICharacterClassification classification)
    {
      this.classification = classification;
      this.mandatory = CreateBitArrayFor(mandatoryCharacters);
      this.allowed = CreateBitArrayFor(allowedCharacters);
    }

    private CharacterInclusion(IEnumerable<CharInterval> mandatory, IEnumerable<CharInterval> allowed, ICharacterClassification classification)
    {
      this.classification = classification;
      this.mandatory = CreateBitArrayFor(mandatory);
      this.allowed = CreateBitArrayFor(allowed);
    }


    private CharacterInclusion(BitArray mandatory, BitArray allowed, ICharacterClassification classification)
    {
      this.classification = classification;
      this.mandatory = mandatory;
      this.allowed = allowed;
    }

    public CharacterInclusion Constant(string c)
    {
      return new CharacterInclusion(c, classification);
    }
    public CharacterInclusion FromDisallowed(IEnumerable<CharInterval> intervals)
    {
      //overapproximate all strings that do not contain a character from the intervals
      BitArray disallowed = CreateBitArrayFor(intervals);
      for (int i = 0; i < classification.Buckets; ++i)
      {
        if (disallowed[i] && !classification.IsSingleton(i))
        {
          // we allow non-singleton classes, because we do not know
          // that all characters from the class were disallowed
          disallowed[i] = false;
        }
      }

      return new CharacterInclusion(CreateBitArrayFor(false), disallowed.Not(), classification);
    }
    public CharacterInclusion Character(IEnumerable<CharInterval> intervals, bool closed)
    {
      BitArray allowed = CreateBitArrayFor(intervals);
      BitArray mandatory;
      if (CountBits(allowed) == 1)
      {
        mandatory = allowed;
      }
      else
      {
        mandatory = CreateBitArrayFor(false);
      }
      if (!closed)
      {
        allowed = CreateBitArrayFor(true);
      }
      return new CharacterInclusion(mandatory, allowed, classification);
    }

    #endregion

    #region Domain properties
    /// <summary>
    /// Gets the top element of the Character Inclusion domain.
    /// </summary>
    public CharacterInclusion Top
    {
      get
      {
        return new CharacterInclusion(true, classification);
      }
    }
    /// <summary>
    /// Gets the top element of the Character Inclusion domain.
    /// </summary>
    public CharacterInclusion Bottom
    {
      get
      {
        return new CharacterInclusion(false, classification);
      }
    }
    ///<inheritdoc/>
    public bool IsTop
    {
      get
      {
        for (int i = 0; i < allowed.Length; ++i)
        {
          if (mandatory[i] || !allowed[i])
            return false;
        }
        return true;
      }
    }
    ///<inheritdoc/>
    public bool IsBottom
    {
      get
      {
        return !Subset(mandatory, allowed);
      }
    }
    ///<inheritdoc/>
    public bool ContainsValue(string value)
    {
      BitArray notFound = new BitArray(mandatory);
      // Check that all characters of value are allowed
      foreach (char character in value)
      {
        if (!allowed[classification[character]])
          return false;
        notFound[classification[character]] = false;
      }
      // Check that all mandatory characters are found in value
      for (int i = 0; i < notFound.Length; ++i)
      {
        if (notFound[i])
          return false;
      }
      return true;
    }

    ///<inheritdoc/>
    public bool Equals(CharacterInclusion other)
    {
      return Equal(mandatory, other.mandatory) && Equal(allowed, other.allowed);
    }
    ///<inheritdoc/>
    public bool LessThanEqual(CharacterInclusion other)
    {
      return Subset(other.mandatory, mandatory) && Subset(allowed, other.allowed);
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
        return CountBits(allowed) == 0;
      }
    }
    /// <summary>
    /// Determines whether all the concrete strings are non-empty.
    /// </summary>
    public bool MustBeNonEmpty
    {
      get
      {
        for (int i = 0; i < mandatory.Length; ++i)
        {
          if (mandatory[i])
            return true;
        }
        return false;
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
      return mandatory[bucket] && classification.IsSingleton(bucket);
    }
    /// <summary>
    /// Determines whether some of the concrete strings can contain a specified
    /// character.
    /// </summary>
    /// <param name="character">A character.</param>
    /// <returns>Whether any of the concrete strings can contain <paramref name="character"/></returns>
    public bool CanContain(char character)
    {
      return allowed[classification[character]];
    }
    #endregion

    #region Domain operations
    ///<inheritdoc/>
    public CharacterInclusion Join(CharacterInclusion other)
    {
      Contract.Requires(classification == other.classification);

      BitArray newMandatory = And(this.mandatory, other.mandatory);
      BitArray newAllowed = Or(this.allowed, other.allowed);

      return new CharacterInclusion(newMandatory, newAllowed, classification);
    }
    ///<inheritdoc/>
    public CharacterInclusion Meet(CharacterInclusion other)
    {
      Contract.Requires(classification == other.classification);

      BitArray newMandatory = Or(this.mandatory, other.mandatory);
      BitArray newAllowed = And(this.allowed, other.allowed);

      return new CharacterInclusion(newMandatory, newAllowed, classification);
    }
    #endregion

    #region Public operations
    /// <summary>
    /// Computes abstraction for strings created by combining the characters
    /// with characters from another string (for example concatenation).
    /// </summary>
    /// <param name="other">Abstraction of the other string.</param>
    /// <returns>Abstraction for combinations of this string and <paramref name="other"/>.</returns>
    public CharacterInclusion Combine(CharacterInclusion other)
    {
      Contract.Requires(classification == other.classification);

      if (IsBottom)
        return this;
      if (other.IsBottom)
        return other;

      BitArray newMandatory = Or(mandatory, other.mandatory);
      BitArray newAllowed = Or(allowed, other.allowed);

      return new CharacterInclusion(newMandatory, newAllowed, classification);
    }
    #endregion

    /// <summary>
    /// Implements string operations for the Character Inclusion abstract domain.
    /// </summary>
    /// <typeparam name="Variable">The type representing variables.</typeparam>
    public class Operations<Variable> : IStringOperations<CharacterInclusion, Variable>
      where Variable : class, IEquatable<Variable>
    {
      private readonly ICharacterClassification classification;

      /// <summary>
      /// Construct operations for <see cref="CharacterInclusion"/> elements 
      /// with the specified character classification.
      /// </summary>
      /// <param name="classification">The character classifcation used by the elements.</param>
      public Operations(ICharacterClassification classification)
      {
        this.classification = classification;
      }
      #region Factory
      ///<inheritdoc/>
      public CharacterInclusion Top
      {
        get { return new CharacterInclusion(true, classification); }
      }
      ///<inheritdoc/>
      public CharacterInclusion Constant(string constant)
      {
        return new CharacterInclusion(constant, classification);
      }
      #endregion

      #region Operations returning strings
      ///<inheritdoc/>
      public CharacterInclusion Concat(WithConstants<CharacterInclusion> left, WithConstants<CharacterInclusion> right)
      {
        return left.ToAbstract(this).Combine(right.ToAbstract(this));
      }
      ///<inheritdoc/>
      public CharacterInclusion Insert(WithConstants<CharacterInclusion> self, IndexInterval index, WithConstants<CharacterInclusion> other)
      {
        IndexInt lowestIndex = index.LowerBound;

        if (self.IsConstant)
        {
          // If the string is constant, index must be at most its length
          if (lowestIndex > self.Constant.Length)
            return new CharacterInclusion(false, classification);
        }
        else
        {
          // If the string is empty, index must be 0
          if (self.Abstract.MustBeEmpty && lowestIndex > 0)
            return new CharacterInclusion(false, classification);
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
      private BitArray CreateBitArrayFor(CharInterval interval)
      {
        BitArray array = new System.Collections.BitArray(classification.Buckets);
        for (int character = interval.LowerBound; character <= interval.UpperBound; ++character)
        {
          array[classification[(char)character]] = true;
        }
        return array;
      }
      ///<inheritdoc/>
      public CharacterInclusion Replace(CharacterInclusion self, CharInterval from, CharInterval to)
      {
        BitArray fromArray = CreateBitArrayFor(from);
        BitArray toArray = CreateBitArrayFor(to);

        BitArray newAllowed = self.allowed;

        if (Intersects(newAllowed, fromArray))
        {
          // Copy the array so that it can be modified
          newAllowed = new BitArray(newAllowed);

          // Some character may be replaced
          if (IsSingleCharInterval(from))
          {
            // We know exactly which one it is,
            // so we can remove it
            newAllowed[classification[from.LowerBound]] = false;
          }

          // We must add the possible replacements
          newAllowed.Or(toArray);
        }

        // Copy the array so that it can be modified
        BitArray newMandatory = new BitArray(self.mandatory);

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
        BitArray notFromArray = fromArray.Not();
        newMandatory.And(fromArray);

        if (mustReplace && to.IsConstant)
        {
          // Replacement must take place and we know what
          // the replacement is
          newMandatory[classification[to.LowerBound]] = true;
        }

        return new CharacterInclusion(newMandatory, newAllowed, classification);
      }
      ///<inheritdoc/>
      public CharacterInclusion Replace(WithConstants<CharacterInclusion> self,
        WithConstants<CharacterInclusion> from,
        WithConstants<CharacterInclusion> to)
      {
        CharacterInclusion selfSet = self.ToAbstract(this);
        CharacterInclusion fromSet = from.ToAbstract(this);
        CharacterInclusion toSet = to.ToAbstract(this);

        if (!CanContain(selfSet, fromSet))
        {
          return selfSet;
        }

        BitArray newAllowed = Or(selfSet.allowed, toSet.allowed);
        BitArray newMandatory = new BitArray(fromSet.allowed).Not();
        newMandatory.And(selfSet.mandatory);

        return new CharacterInclusion(newMandatory, newAllowed, classification);
      }

      ///<inheritdoc/>
      public CharacterInclusion Substring(CharacterInclusion self, IndexInterval index, IndexInterval length)
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
      public CharacterInclusion Remove(CharacterInclusion self, IndexInterval index, IndexInterval length)
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
      public CharacterInclusion PadLeft(CharacterInclusion self, IndexInterval length, CharInterval fill)
      {
        return self.Pad(length, fill);
      }

      ///<inheritdoc/>
      public CharacterInclusion PadRight(CharacterInclusion self, IndexInterval length, CharInterval fill)
      {
        return self.Pad(length, fill);
      }

      /// <summary>
      /// Determines whether the bit array represents exactly one character.
      /// </summary>
      /// <param name="array">A bit array of character classes.</param>
      /// <returns>Whether the bit array allows exactly one character class with
      /// exactly one character.</returns>
      private bool IsSingletonSet(BitArray array)
      {
        bool found = false;
        for (int i = 0; i < array.Count; ++i)
        {
          if (array[i])
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
      public CharacterInclusion Trim(WithConstants<CharacterInclusion> self, WithConstants<CharacterInclusion> trimmed)
      {
        CharacterInclusion selfSet = self.ToAbstract(this);
        CharacterInclusion trimmedSet = trimmed.ToAbstract(this);

        if (Subset(selfSet.allowed, trimmedSet.mandatory) && IsSingletonSet(selfSet.allowed))
        {
          // If all allowed characters are provably trimmed, the result is empty string
          return new CharacterInclusion("", classification);
        }
        else
        {
          // Otherwise, keep in mandatory set those, which are not allowed in the trimmed set
          BitArray newMandatory = new BitArray(trimmedSet.allowed);
          newMandatory.Not();
          newMandatory.And(selfSet.mandatory);

          return new CharacterInclusion(newMandatory, selfSet.allowed, classification);
        }
      }
      ///<inheritdoc/>
      public CharacterInclusion TrimStart(WithConstants<CharacterInclusion> self, WithConstants<CharacterInclusion> trimmed)
      {
        return Trim(self, trimmed);
      }
      ///<inheritdoc/>
      public CharacterInclusion TrimEnd(WithConstants<CharacterInclusion> self, WithConstants<CharacterInclusion> trimmed)
      {
        return Trim(self, trimmed);
      }
      #endregion

      #region Predicate operations
      ///<inheritdoc/>
      public IStringPredicate IsEmpty(CharacterInclusion self, Variable selfVariable)
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
        WithConstants<CharacterInclusion> self, Variable selfVariable,
        WithConstants<CharacterInclusion> other, Variable otherVariable, bool any)
      {
        // any = containment is true if other is anywhere

        CharacterInclusion selfSet = self.ToAbstract(this);

        if (!other.IsConstant)
        {
          CharacterInclusion otherSet = other.Abstract;

          if (otherSet.MustBeEmpty)
          {
            return FlatPredicate.True;
          }
          if (!Subset(otherSet.mandatory, selfSet.allowed))
          {
            return FlatPredicate.False;
          }

          //NOTE: if both variables are available, we could theoretically
          // create predicate on both..
          if (selfVariable != null)
          {
            //if we assume the predicate, then we must contain all mandatory chars from the substring
            CharacterInclusion trueAbstraction = otherSet.Extend();
            return StringAbstractionPredicate.ForTrue(selfVariable, trueAbstraction);
          }
          else if (otherVariable != null)
          {
            // if we assume the predicate, then the var must only contain allowed chars from here
            CharacterInclusion trueAbstraction = selfSet.Part(false, false, false, false);
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

            if (!selfSet.allowed[characterBucket])
            {
              // The character is not allowed
              return FlatPredicate.False;
            }
            if (selfSet.mandatory[characterBucket] && classification.IsSingleton(characterBucket))
            {
              // The character is mandatory in the string
              if (any || CountBits(selfSet.allowed) == 1)
              {
                // We are looking for any occurence, or no other characters are allowed
                return FlatPredicate.True;
              }
            }

            if (selfVariable != null)
            {
              BitArray full = new BitArray(selfSet.classification.Buckets, true);
              BitArray empty = new BitArray(selfSet.classification.Buckets, false);
              BitArray with = new BitArray(selfSet.classification.Buckets, false);
              with.Set(characterBucket, true);
              BitArray without = new BitArray(with).Not();

              CharacterInclusion trueAbstraction = new CharacterInclusion(with, full, classification);
              CharacterInclusion falseAbstraction = new CharacterInclusion(empty, without, classification);

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
              if (!selfSet.allowed[selfSet.classification[character]])
                return FlatPredicate.False;
            }

            // If the predicate is true, characters from the constant are mandatory

            if (selfVariable != null)
            {
              BitArray newMandatory = selfSet.CreateBitArrayFor(otherConstant);
              BitArray full = selfSet.CreateBitArrayFor(true);

              CharacterInclusion trueAbstraction = new CharacterInclusion(newMandatory, full, classification);
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
        WithConstants<CharacterInclusion> self, Variable selfVariable,
        WithConstants<CharacterInclusion> other, Variable otherVariable)
      {
        return ContainCommon(self, selfVariable, other, otherVariable, true);
      }
      ///<inheritdoc/>
      public IStringPredicate StartsWithOrdinal(
        WithConstants<CharacterInclusion> self, Variable selfVariable,
        WithConstants<CharacterInclusion> other, Variable otherVariable)
      {
        return ContainCommon(self, selfVariable, other, otherVariable, false);
      }
      ///<inheritdoc/>
      public IStringPredicate EndsWithOrdinal(
        WithConstants<CharacterInclusion> self, Variable selfVariable,
        WithConstants<CharacterInclusion> other, Variable otherVariable)
      {
        return ContainCommon(self, selfVariable, other, otherVariable, false);
      }
      ///<inheritdoc/>
      public IStringPredicate Equals(
        WithConstants<CharacterInclusion> self, Variable selfVariable,
        WithConstants<CharacterInclusion> other, Variable otherVariable)
      {
        CharacterInclusion selfSet = self.ToAbstract(this);
        CharacterInclusion otherSet = other.ToAbstract(this);

        if (selfSet.MustBeEmpty && otherSet.MustBeEmpty)
        {
          // Both strings must be empty, so they are equal
          return FlatPredicate.True;
        }
        else if (!Subset(selfSet.mandatory, otherSet.allowed) || !Subset(otherSet.mandatory, selfSet.allowed))
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

      private bool CanBeLess(CharacterInclusion self, CharacterInclusion other)
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

        // Now we know that self.mandatory and other.allowed are non-empty

        int maxOther = Max(other.allowed);
        int minSelf = Min(self.allowed);

        if (minSelf < maxOther)
        {
          // Self can contain a character less than other
          return true;
        }
        else
        {
          int maxMandatory = Max(self.mandatory);
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
      public CompareResult CompareOrdinal(WithConstants<CharacterInclusion> self, WithConstants<CharacterInclusion> other)
      {
        CharacterInclusion selfSet = self.ToAbstract(this);
        CharacterInclusion otherSet = other.ToAbstract(this);

        bool less = CanBeLess(selfSet, otherSet);
        bool greater = CanBeLess(otherSet, selfSet);
        bool equal = Subset(selfSet.mandatory, otherSet.allowed) && Subset(otherSet.mandatory, selfSet.allowed);

        return CompareResultExtensions.Build(less, equal, greater);
      }
      ///<inheritdoc/>
      public IndexInterval GetLength(CharacterInclusion self)
      {
        if (self.MustBeEmpty)
        {
          return IndexInterval.For(0);
        }
        else
        {
          return IndexInterval.For(IndexInt.For(CountBits(self.mandatory)), IndexInt.Infinity);
        }
      }

      private bool CanContain(CharacterInclusion self, CharacterInclusion other)
      {
        for (int i = 0; i < self.allowed.Length; ++i)
        {
          if (other.mandatory[i] && !self.allowed[i])
            return false;
        }

        return true;
      }


      private IndexInterval IndexCommon(WithConstants<CharacterInclusion> self,
        WithConstants<CharacterInclusion> needle,
        IndexInterval offset, IndexInterval count, bool last)
      {
        if (!offset.IsFiniteConstant || offset.LowerBound != 0 || !count.IsInfinity)
        {
          // Offset and count are not supported
          return IndexInterval.Unknown;
        }

        CharacterInclusion selfSet = self.ToAbstract(this);

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
              if (!last && CountBits(selfSet.allowed) == 1)
                return IndexInterval.For(0);
              else
                return IndexInterval.For(IndexInt.For(0), IndexInt.Infinity);
            }
            else
            {
              if (!last && CountBits(selfSet.allowed) == 1)
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
      ///<inheritdoc/>
      public IndexInterval IndexOf(WithConstants<CharacterInclusion> self, WithConstants<CharacterInclusion> needle, IndexInterval offset, IndexInterval count)
      {
        return IndexCommon(self, needle, offset, count, false);
      }
      ///<inheritdoc/>
      public IndexInterval LastIndexOf(WithConstants<CharacterInclusion> self, WithConstants<CharacterInclusion> needle, IndexInterval offset, IndexInterval count)
      {
        return IndexCommon(self, needle, offset, count, true);
      }
      #endregion
      ///<inheritdoc/>
      public IStringPredicate RegexIsMatch(CharacterInclusion self, Variable selfVariable, Regex.AST.Element regex)
      {
        CharacterInclusionRegex characterSetRegexConverter = new CharacterInclusionRegex(self);

        ProofOutcome isMatch = characterSetRegexConverter.IsMatch(regex);

        if (isMatch == ProofOutcome.Top && selfVariable != null)
        {
          return characterSetRegexConverter.PredicateFromRegex(regex, selfVariable);
        }

        return FlatPredicate.ForProofOutcome(isMatch);
      }
      ///<inheritdoc/>
      public CharacterInclusion SetCharAt(CharacterInclusion self, IndexInterval index, CharInterval value)
      {
        BitArray values = CreateBitArrayFor(value);

        // Allowed are the old and new characters
        BitArray allowed = Or(self.allowed, values);
        // Mandatory is only the new character, if it is a single bucket
        BitArray mandatory = CountBits(values) == 1 ? values : new BitArray(classification.Buckets);

        return new CharacterInclusion(mandatory, allowed, classification);
      }
      ///<inheritdoc/>
      public CharInterval GetCharAt(CharacterInclusion self, IndexInterval index)
      {
        int min = char.MaxValue;
        int max = char.MinValue;

        for (int character = char.MinValue; character <= char.MaxValue; ++character)
        {
          if (self.allowed[classification[(char)character]])
          {
            min = Math.Min(min, character);
            max = Math.Max(max, character);
          }
        }

        return CharInterval.For((char)min, (char)max);
      }
    }

    #region String operations


    /// <summary>
    /// Creates abstraction for a part of the string.
    /// </summary>
    /// <param name="mustNotBeEmpty">Whether the source string is required to be non-empty.</param>
    /// <param name="empty">Whether we take an empty part of the string.</param>
    /// <param name="willNotBeEmpty">Whether the result string is known to be non-empty.</param>
    /// <param name="full">Whether we take the whole string.</param>
    /// <returns>Abstraction for a part of a string.</returns>
    internal CharacterInclusion Part(bool mustNotBeEmpty, bool empty, bool willNotBeEmpty, bool full)
    {
      Contract.Requires(!(!mustNotBeEmpty && empty && full));
      Contract.Requires(!(empty && willNotBeEmpty));

      if (IsBottom)
        return this;

      if (mustNotBeEmpty && MustBeEmpty)
        return Bottom;

      if (willNotBeEmpty && CountBits(allowed) == 1)
      {
        // We know that the string will not be empty, but only one category
        // is allowed, so it is mandatory.
        return new CharacterInclusion(allowed, allowed, classification);
      }

      if (empty)
      {
        return new CharacterInclusion("", classification);
      }
      if (full)
      {
        return this;
      }

      BitArray newMandatory = CreateBitArrayFor(false);
      return new CharacterInclusion(newMandatory, allowed, classification);
    }
    internal CharacterInclusion Extend()
    {
      BitArray newAllowed = CreateBitArrayFor(true);
      return new CharacterInclusion(mandatory, newAllowed, classification);
    }
    private CharacterInclusion Pad(IndexInterval length, CharInterval padding)
    {
      BitArray newMandatory = mandatory;
      BitArray newAllowed = allowed;

      if (length.LowerBound > 0 && padding.IsConstant)
      {

        int allowedCount = CountBits(allowed);
        int paddingClass = classification[padding.LowerBound];

        if (allowedCount == 0 || (allowedCount == 1 && allowed[paddingClass]))
        {
          // We want to pad, the padding is known
          // and no other characters than padding are allowed
          // therefore the character must occur.

          newMandatory = new BitArray(newMandatory);
          newMandatory[paddingClass] = true;
        }
      }

      // If we already have at least as many characters as the maximum length,
      // no padding is possible.
      if (length.UpperBound > CountBits(mandatory))
      {
        newAllowed = new BitArray(newAllowed);
        for (int character = padding.LowerBound; character <= padding.UpperBound; ++character)
        {
          newAllowed[classification[(char)character]] = true;
        }
      }

      return new CharacterInclusion(newMandatory, newAllowed, classification);
    }
    #endregion

    #region Object method override
    public override string ToString()
    {
      StringBuilder str = new StringBuilder();

      int mc = 0;

      for (int i = 0; i < mandatory.Length && mc < 50; ++i)
      {
        if (mandatory[i])
        {
          str.Append((char)i);
          ++mc;
        }
      }

      str.Append(" ");

      mc = 0;

      for (int i = 0; i < mandatory.Length && mc < 50; ++i)
      {
        if (!mandatory[i] && allowed[i])
        {
          str.Append((char)i);
          ++mc;
        }
      }

      return str.ToString();
    }
    public override bool Equals(object obj)
    {
      return Equals(obj as CharacterInclusion);
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
      return LessThanEqual(a as CharacterInclusion);
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
      return Join(a as CharacterInclusion);
    }

    public IAbstractDomain Meet(IAbstractDomain a)
    {
      return Meet(a as CharacterInclusion);
    }

    public IAbstractDomain Widening(IAbstractDomain prev)
    {
      return Join(prev as CharacterInclusion);
    }

    public object Clone()
    {
      return new CharacterInclusion(mandatory, allowed, classification);
    }
    public T To<T>(IFactory<T> factory)
    {
      return factory.Constant(true);
    }
    #endregion
  }
}
