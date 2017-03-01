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

using Microsoft.Research.Regex;
using Microsoft.Research.Regex.AST;
using Microsoft.Research.CodeAnalysis;
using System.Collections;
using Microsoft.Research.AbstractDomains.Strings.Regex;

namespace Microsoft.Research.AbstractDomains.Strings
{
#if vdfalse
    /// <summary>
    /// Operations for generating a non-matching CharacterInclusion for a regex.
    /// (CharacterInclusion that over-approximates the set of non-matching strings.)
    /// </summary>
    class CharacterInclusionComplementGeneratingOperations<CharacterSet> : IGeneratingOperationsForRegex<CharacterInclusion<CharacterSet>>
        where CharacterSet : ICharacterSet<CharacterSet>
    {
        private readonly CharacterInclusion<CharacterSet> factory;
        public CharacterInclusion<CharacterSet> Bottom
        {
            get
            {
                //No matching means all are non-matching
                return factory.Top;
            }
        }

        public CharacterInclusion<CharacterSet> Top
        {
            get
            {
                //All matching means no non-matching
                return factory.Bottom;
            }
        }

        public CharacterInclusion<CharacterSet> Empty
        {
            get
            {
                // returns top
                return Bottom;
            }
        }

        public CharacterInclusionComplementGeneratingOperations(CharacterInclusion<CharacterSet> factory)
        {
            this.factory = factory;
        }

        public CharacterInclusion<CharacterSet> AddChar(CharacterInclusion<CharacterSet> prev, CharRanges next, bool closed)
        {
            if (!closed && prev.IsBottom)
            {
                IEnumerable<CharInterval> allowed = next.ToIntervals();
                return prev.Character(allowed, true);
            }
            else
                return prev.Top;
        }

        public bool CanBeEmpty(CharacterInclusion<CharacterSet> prev)
        {
            //TODO: verify:
            // Here the meaning is "the non-matching MUST be empty"
            return prev.MustBeEmpty;
        }

        public CharacterInclusion<CharacterSet> Join(CharacterInclusion<CharacterSet> left, CharacterInclusion<CharacterSet> right, bool widen)
        {
            return left.Meet(right);
        }
        public bool IsUnderapproximating
        {
            get
            {
                return true;
            }
        }
    }


    /// <summary>
    /// Operations for generating a matching CharacterInclusion for a regex.
    /// (CharacterInclusion that over-approximates the set of matching strings.)
    /// </summary>
    internal class CharacterInclusionGeneratingOperations<CharacterSet> : IGeneratingOperationsForRegex<CharacterInclusion<CharacterSet>>
        where CharacterSet : ICharacterSet<CharacterSet>
    {
        private readonly CharacterInclusion<CharacterSet> factory;

        public CharacterInclusion<CharacterSet> Bottom
        {
            get
            {
                return factory.Bottom;
            }
        }

        public CharacterInclusion<CharacterSet> Top
        {
            get
            {
                return factory.Top;
            }
        }
        public CharacterInclusion<CharacterSet> Empty
        {
            get
            {
                return Top;
            }
        }

        public bool IsUnderapproximating
        {
            get
            {
                return false;
            }
        }

        public CharacterInclusionGeneratingOperations(CharacterInclusion<CharacterSet> factory)
        {
            this.factory = factory;
        }

        public CharacterInclusion<CharacterSet> AddChar(CharacterInclusion<CharacterSet> prev, CharRanges next, bool closed)
        {
            IEnumerable<CharInterval> allowed = next.ToIntervals();
            //Closed - Adds char to mandatory and allowed
            //Open - Adds char to mandatory and sets allowed to all chars
            CharacterInclusion<CharacterSet> ci = prev.Character(allowed, closed);
            return prev.Combine(ci);
        }

        public bool CanBeEmpty(CharacterInclusion<CharacterSet> prev)
        {
            return !prev.MustBeNonEmpty;
        }

        public CharacterInclusion<CharacterSet> Join(CharacterInclusion<CharacterSet> left, CharacterInclusion<CharacterSet> right, bool widen)
        {
            return widen ? (CharacterInclusion<CharacterSet>)left.Widening(right) : left.Join(right);
        }
    }


    /// <summary>
    /// Abstract state for CharacterInclusion regex matching.
    /// </summary>
    internal struct CharacterInclusionMatchingState<CharacterSet>
        where CharacterSet : ICharacterSet<CharacterSet>
    {
        public CharacterSet encountered;
        //TODO: VD: Closed start can be represented by setting encountered to full
        public bool closedStart;
        public bool fail;

        public CharacterInclusionMatchingState(CharacterSet encountered, bool fail, bool closedStart)
        {
            this.encountered = encountered;
            this.fail = fail;
            this.closedStart = closedStart;
        }
    }

    /// <summary>
    /// Abstract state for CharacterInclusion regex matching.
    /// </summary>
    internal class CharacterInclusionMatchingOperations<CharacterSet> : IMatchingOperationsForRegex<CharacterInclusionMatchingState<CharacterSet>, CharacterInclusion<CharacterSet>>
        where CharacterSet : ICharacterSet<CharacterSet>
    {
        ICharacterSetFactory<CharacterSet> setFactory;
        ICharacterClassification classification;

        public CharacterInclusionMatchingState<CharacterSet> AssumeEnd(CharacterInclusion<CharacterSet> input, CharacterInclusionMatchingState<CharacterSet> prev, bool under)
        {
            if (prev.closedStart) {
                //Test encountered against input
                if (!input.mandatory.IsSubset(prev.encountered))
                    return GetBottom(input);
            }

            return prev;
        }

        public CharacterInclusionMatchingState<CharacterSet> AssumeStart(CharacterInclusion<CharacterSet> input, CharacterInclusionMatchingState<CharacterSet> prev, bool under)
        {
            if (prev.fail)
                return prev;

            return new CharacterInclusionMatchingState<CharacterSet>(setFactory.Create(false, classification.Buckets), false, true);
        }

        public CharacterInclusionMatchingState<CharacterSet> GetBottom(CharacterInclusion<CharacterSet> input)
        {
            return new CharacterInclusionMatchingState<CharacterSet>(setFactory.Create(false, classification.Buckets), true, false);
        }

        public CharacterInclusionMatchingState<CharacterSet> GetTop(CharacterInclusion<CharacterSet> input)
        {
            return new CharacterInclusionMatchingState<CharacterSet>(setFactory.Create(false, classification.Buckets), false, false);
        }

        public CharacterInclusionMatchingState<CharacterSet> Join(CharacterInclusion<CharacterSet> input, CharacterInclusionMatchingState<CharacterSet> left, CharacterInclusionMatchingState<CharacterSet> right, bool under, bool widen)
        {
            if (left.fail)
                return right;
            if (right.fail)
                return left;

            return new CharacterInclusionMatchingState<CharacterSet>(left.encountered.Union(right.encountered), false, left.closedStart && right.closedStart);
        }

        public CharacterInclusionMatchingState<CharacterSet> MatchChar(CharacterInclusion<CharacterSet> input, CharacterInclusionMatchingState<CharacterSet> prev, CharRanges next, bool under)
        {
            CharacterSet cs;

            //Test against input
            if (!input.allowed.Intersects(cs))
                return GetBottom(input);
            //Add to encountered
            return new CharacterInclusionMatchingState<CharacterSet>(prev.encountered.Union(cs), false, prev.closedStart);
        }
    }
#endif
    /// <summary>
    /// Converts between Character Inclusion abstract domain
    /// and regexes.
    /// </summary>
    public class CharacterInclusionRegex<CharacterSet>
        where CharacterSet : ICharacterSet<CharacterSet>
    {

        /// <summary>
        /// The reference abstract element.
        /// </summary>
        private readonly CharacterInclusion<CharacterSet> value;

        public CharacterInclusionRegex(CharacterInclusion<CharacterSet> value)
        {
            this.value = value;
        }
#if vdfalse
    private class IsMatchVisitor : OpenClosedRegexVisitor<ProofOutcome, bool>
    {
      private readonly CharacterInclusionRegex converter;

      public IsMatchVisitor(CharacterInclusionRegex converter)
      {
        this.converter = converter;
      }

      protected override ProofOutcome Visit(Empty element, ref bool closed)
      {
        if (closed)
        {
          return ProofOutcomeUtils.Build(!converter.value.MustBeEmpty, !converter.value.MustBeNonEmpty);
        }
        else
        {
          return ProofOutcome.True;
        }
      }
      protected override ProofOutcome Visit(Alternation element, ref bool data)
      {
        bool canMatch = false;
        bool canNotMatch = true;

        foreach (Element pattern in element.Patterns)
        {
          ProofOutcome patternMatch = VisitElement(pattern, ref data);
          canMatch |= ProofOutcomeUtils.CanBeTrue(patternMatch);
          canNotMatch &= ProofOutcomeUtils.CanBeFalse(patternMatch);
        }

        return ProofOutcomeUtils.Build(canMatch, canNotMatch);
      }
      protected override ProofOutcome VisitConcatenation(Concatenation element, int startIndex, int endIndex, RegexEndsData ends, ref bool data)
      {
        bool innerClosed = data || (ends.LeftClosed && ends.RightClosed);
        bool halfClosed = ends.LeftClosed ^ ends.RightClosed;

        if (endIndex - startIndex == 1 && !halfClosed)
        {
          //Just a single element
          return VisitElement(element.Parts[startIndex], ref innerClosed);
        }
        else
        {
          innerClosed = false;

          CharacterInclusion canMatchCI = converter.FromRegex(element, innerClosed);
          bool canMatch = CharacterInclusion.MandatorySubsetAllowed(converter.value, canMatchCI);

          for (int index = startIndex; index < endIndex && canMatch; ++index)
          {
            ProofOutcome part = VisitElement(element.Parts[index], ref innerClosed);
            canMatch &= ProofOutcomeUtils.CanBeTrue(part);
          }

          return canMatch ? ProofOutcome.Top : ProofOutcome.False;
        }
      }
      protected override ProofOutcome Visit(SingleElement element, ref bool closed)
      {
        bool canMatch, canNotMatch;

        BitArray canMatchArray = converter.value.CreateBitArrayFor(element.CanMatchRanges.ToIntervals());
        BitArray mustMatchArray = converter.value.CreateBitArrayFor(element.MustMatchRanges.ToIntervals());

        bool allowedIntersect = CharacterInclusion.Intersects(converter.value.allowed, canMatchArray);

        if (closed)
        {
          canNotMatch = true;
          canMatch = allowedIntersect && CharacterInclusion.Subset(converter.value.mandatory, canMatchArray) && CharacterInclusion.CountBits(converter.value.mandatory) <= 1;
        }
        else
        {
          canNotMatch = CharacterInclusion.CountBits(mustMatchArray) == 0 || !CharacterInclusion.Subset(canMatchArray, converter.value.mandatory);
          canMatch = allowedIntersect;
        }

        return ProofOutcomeUtils.Build(canMatch, canNotMatch);
      }
      protected override ProofOutcome Visit(Loop element, ref bool closed)
      {
        bool canMatch, canNotMatch;
        bool innerClosed = false;

        if (closed)
        {
          if (!element.IsUnbounded || element.Min > 1 || (element.Min == 1 && !converter.value.MustBeNonEmpty))
          {
            canNotMatch = true;
          }
          else
          {
            CharacterInclusion canNotMatchCI = converter.FromNegativeRegex(element.Content);

            canNotMatch = CharacterInclusion.AllowedIntersects(canNotMatchCI, converter.value);
          }

          CharacterInclusion canMatchCI = converter.FromRegex(element.Content, true);
          if (element.Min == 0 && !converter.value.MustBeNonEmpty)
          {
            canMatch = true;
          }
          else if (ProofOutcomeUtils.CanBeTrue(VisitElement(element.Content, ref innerClosed)))
          {
            canMatch = CharacterInclusion.MandatorySubsetAllowed(converter.value, canMatchCI);
          }
          else
          {
            canMatch = false;
          }
        }
        else // Open
        {
          canMatch = element.Min == 0 || ProofOutcomeUtils.CanBeTrue(VisitElement(element.Content, ref innerClosed));
          if (element.Min == 0)
          {
            canNotMatch = false;
          }
          else if (element.Min == 1)
          {
            CharacterInclusion canNotMatchCI = converter.FromNegativeRegex(element.Content);
            canNotMatch = CharacterInclusion.MandatorySubsetAllowed(converter.value, canNotMatchCI);
          }
          else
          {
            canNotMatch = true;
          }
        }

        return ProofOutcomeUtils.Build(canMatch, canNotMatch);
      }
      protected override ProofOutcome Unsupported(Element regex, ref bool data)
      {
        return ProofOutcome.Top;
      }
    }



    public ProofOutcome IsMatch(Microsoft.Research.Regex.Model.Element element)
    {
      IsMatchVisitor visitor = new IsMatchVisitor(this);
      bool closed = false;
      return visitor.VisitSimpleRegex(element, ref closed);
    }


#endif
#if false
        #region Convert matched regex to CharacterSet

        private class FromRegexVisitor : SimpleRegexVisitor<CharacterInclusion, bool>
    {
      private readonly CharacterInclusion factoryValue;

      public FromRegexVisitor(CharacterInclusion factoryValue)
      {
        this.factoryValue = factoryValue;
      }


      protected override CharacterInclusion Visit(SingleElement single, ref bool closed)
      {
        IEnumerable<CharInterval> allowed = single.CanMatchRanges.ToIntervals();

        return factoryValue.Character(allowed, closed);
      }

      protected override CharacterInclusion Visit(Anchor anchor, ref bool closed)
      {
        if (closed)
        {
          return factoryValue.Constant("");
        }
        else
        {
          return factoryValue.Top;
        }
      }

      protected override CharacterInclusion Visit(Concatenation concat, ref bool closed)
      {
        bool innerClosed = closed || IsClosedConcatenation(concat);

        CharacterInclusion result = factoryValue.Constant("");

        foreach (Element part in concat.Parts)
        {
          result = result.Combine(VisitElement(part, ref innerClosed));
        }

        return result;
      }

      protected override CharacterInclusion Visit(Alternation union, ref bool closed)
      {
        CharacterInclusion result = factoryValue.Bottom;

        foreach (Element part in union.Patterns)
        {
          result = result.Join(VisitElement(part, ref closed));
        }

        return result;
      }

      protected override CharacterInclusion Visit(Loop loop, ref bool closed)
      {
        CharacterInclusion result = VisitElement(loop.Content, ref closed);

        if (loop.Min == 0)
        {
          result = result.Part(false, false, false, false);
        }

        return result;
      }
      protected override CharacterInclusion Visit(Empty element, ref bool data)
      {
        return factoryValue.Top;
      }
      protected override CharacterInclusion Unsupported(Element regex, ref bool data)
      {
        return factoryValue.Top;
      }
    }

    private CharacterInclusion FromRegex(Element element, bool closed)
    {
      FromRegexVisitor visitor = new FromRegexVisitor(value);
      return visitor.VisitSimpleRegex(element, ref closed);
    }
        #endregion
        #region Convert negative regex to CharacterSet

    private class FromNegativeRegexVisitor : SimpleRegexVisitor<CharacterInclusion, Void>
    {
      private readonly CharacterInclusion factoryValue;

      public FromNegativeRegexVisitor(CharacterInclusion factoryValue)
      {
        this.factoryValue = factoryValue;
      }

      protected override CharacterInclusion Visit(SingleElement single, ref Void data)
      {
        return factoryValue.FromDisallowed(single.MustMatchRanges.ToIntervals());
      }

      protected override CharacterInclusion Visit(Concatenation concat, ref Void data)
      {
        int length = concat.Parts.Count;
        if (length == 0)
        {
          // Empty concatenation - always matches
          return factoryValue.Bottom;
        }
        else if (length == 1)
        {
          // Single part - inherit
          return VisitElement(concat.Parts[0], ref data);
        }
        else
        {
          // Multiple parts - says nothing
          return factoryValue.Top;
        }
      }

      protected override CharacterInclusion Visit(Alternation alternate, ref Void data)
      {
        CharacterInclusion result = factoryValue.Top;
        foreach (Element pattern in alternate.Patterns)
        {
          result = result.Meet(VisitElement(pattern, ref data));
        }
        return result;
      }

      protected override CharacterInclusion Visit(Loop loop, ref Void data)
      {
        if (loop.Min == 0)
        {
          return factoryValue.Bottom;
        }
        else if (loop.Min == 1)
        {
          return VisitElement(loop.Content, ref data);
        }
        else
        {
          return factoryValue.Top;
        }
      }

      protected override CharacterInclusion Visit(Empty element, ref Void data)
      {
        return factoryValue.Bottom;
      }

      protected override CharacterInclusion Visit(Anchor element, ref Void data)
      {
        return factoryValue.Bottom;
      }

      protected override CharacterInclusion Unsupported(Element regex, ref Void data)
      {
        return factoryValue.Top;
      }
    }



    private CharacterInclusion FromNegativeRegex(Element element)
    {
      FromNegativeRegexVisitor visitor = new FromNegativeRegexVisitor(value);
      Void unusedData = new Void();
      return visitor.VisitSimpleRegex(element, ref unusedData);
    }
        #endregion

#endif

        public ProofOutcome IsMatch(Microsoft.Research.Regex.Model.Element regex)
        {
#if vdfalse
            var operations = new CharacterInclusionMatchingOperations<CharacterSet>();
            MatchingInterpretation<CharacterInclusionMatchingState<CharacterSet>, CharacterInclusion<CharacterSet>> interpretation = new MatchingInterpretation<CharacterInclusionMatchingState<CharacterSet>, CharacterInclusion<CharacterSet>>(operations, this.value);
            ForwardRegexInterpreter<MatchingState<CharacterInclusionMatchingState<CharacterSet>>> interpreter = new ForwardRegexInterpreter<MatchingState<CharacterInclusionMatchingState<CharacterSet>>>(interpretation);

            var result = interpreter.Interpret(regex);


            bool canMatch = !result.Over.fail;
            bool mustMatch = false;//TODO: value.LessThanEqual(result.Under.currentElement);

            return ProofOutcomeUtils.Build(canMatch, !mustMatch);
#endif
        return ProofOutcome.Top;
        }
    
        public CharacterInclusion<CharacterSet> Assume(Microsoft.Research.Regex.Model.Element regex, bool match)
        {
            /*    
                    IGeneratingOperationsForRegex<CharacterInclusion<CharacterSet>> operations;
                    if(match)
                        operations = new CharacterInclusionGeneratingOperations<CharacterSet>(value);
                    else
                        operations = new CharacterInclusionComplementGeneratingOperations<CharacterSet>(value);

                    GeneratingInterpretation<CharacterInclusion<CharacterSet>> interpretation = new GeneratingInterpretation<CharacterInclusion<CharacterSet>>(operations);
                    ForwardRegexInterpreter<GeneratingState<CharacterInclusion<CharacterSet>>> interpreter = new ForwardRegexInterpreter<GeneratingState<CharacterInclusion<CharacterSet>>>(interpretation);

                    var result = interpreter.Interpret(regex);
                    return result.Open;
                 */
            return value.Top;  
        }


    public IStringPredicate PredicateFromRegex<Variable>(Microsoft.Research.Regex.Model.Element regex, Variable thisVar)
            where Variable : class, IEquatable<Variable>
        {
        /*
            System.Diagnostics.Contracts.Contract.Requires(thisVar != null);

            CharacterInclusion<CharacterSet> matchSet = Assume(regex, true);
            CharacterInclusion<CharacterSet> nonMatchSet = Assume(regex, false);

            return StringAbstractionPredicate.For(thisVar, matchSet, nonMatchSet);
            */
        return FlatPredicate.Top;
        }
    }

}
