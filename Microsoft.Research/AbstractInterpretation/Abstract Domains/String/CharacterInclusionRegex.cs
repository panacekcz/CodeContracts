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

namespace Microsoft.Research.AbstractDomains.Strings
{
  /// <summary>
  /// Converts between Character Inclusion abstract domain
  /// and regexes.
  /// </summary>
  class CharacterInclusionRegex
  {
    /// <summary>
    /// The reference abstract element.
    /// </summary>
    private readonly CharacterInclusion value;

    public CharacterInclusionRegex(CharacterInclusion value)
    {
      this.value = value;
    }

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

        BitArray canMatchArray = converter.value.CreateBitArrayFor(element.CanMatchIntervals.Select(t => CharInterval.For(t.Item1, t.Item2)));
        BitArray mustMatchArray = converter.value.CreateBitArrayFor(element.MustMatchIntervals.Select(t => CharInterval.For(t.Item1, t.Item2)));

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



    public ProofOutcome IsMatch(Regex.AST.Element element)
    {
      IsMatchVisitor visitor = new IsMatchVisitor(this);
      bool closed = false;
      return visitor.VisitSimpleRegex(element, ref closed);
    }

    private static bool IsClosedConcatenation(Concatenation concat)
    {
      if (concat.Parts.Count >= 2)
      {
        return concat.Parts[0].IsStartAnchor() && concat.Parts[concat.Parts.Count - 1].IsEndAnchor();
      }
      else
      {
        return false;
      }
    }

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
        IEnumerable<CharInterval> allowed = single.CanMatchIntervals.Select(t => CharInterval.For(t.Item1, t.Item2));

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
        return factoryValue.FromDisallowed(single.MustMatchIntervals.Select(t => CharInterval.For(t.Item1, t.Item2)));
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



    public IStringPredicate PredicateFromRegex<Variable>(Regex.AST.Element regex, Variable thisVar)
     where Variable : class, IEquatable<Variable>
    {
      System.Diagnostics.Contracts.Contract.Requires(thisVar != null);

      CharacterInclusion trueSet = FromRegex(regex, false);
      CharacterInclusion falseSet = FromNegativeRegex(regex);

      return StringAbstractionPredicate.For(thisVar, trueSet, falseSet);
    }
  }
}
