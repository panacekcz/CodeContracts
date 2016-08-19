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

using Microsoft.Research.Regex.AST;
using Microsoft.Research.Regex;
using Microsoft.Research.CodeAnalysis;

namespace Microsoft.Research.AbstractDomains.Strings
{
  /// <summary>
  /// Converts regexes to bricks.
  /// </summary>
  public class BricksRegex
  {
    private readonly Bricks element;

    public BricksRegex(Bricks element)
    {
      this.element = element;
    }

    private class BricksRegexVisitor : OpenClosedRegexVisitor<List<Brick>, RegexEndsData>
    {
      private IBricksPolicy bricksPolicy;
      private bool overapproximate;

      public BricksRegexVisitor(IBricksPolicy bricksPolicy, bool overapproximate)
      {
        this.bricksPolicy = bricksPolicy;
        this.overapproximate = overapproximate;
      }

      protected override List<Brick> Unsupported(Element regex, ref RegexEndsData data)
      {
        return new List<Brick> { new Brick(overapproximate) };
      }

      protected override List<Brick> Visit(Alternation element, ref RegexEndsData data)
      {
        bool isSingleBrick = true;
        HashSet<string> constants = new HashSet<string>();

        Bricks joined = new Bricks(new List<Brick>(), bricksPolicy);

        foreach (Element child in element.Patterns)
        {
          List<Brick> childBricks = VisitElement(child, ref data);

          if (isSingleBrick && childBricks.Count == 1 && childBricks[0].min == 1 && childBricks[0].max == 1)
          {
            constants.UnionWith(childBricks[0].values);
          }
          else if (!overapproximate)
          {
            // If we are underapproximating, do not allow joining lists
            return new List<Brick> { new Brick(false) };
          }
          else
          {
            if (isSingleBrick)
            {
              isSingleBrick = false;
              joined.bricks.Add(new Brick(constants));
            }

            joined = joined.Join(new Bricks(childBricks, bricksPolicy));
          }

        }

        if (isSingleBrick)
        {
          joined.bricks.Add(new Brick(constants));
        }

        return joined.bricks;
      }

      protected override List<Brick> VisitConcatenation(Concatenation element,
        int startIndex, int endIndex, RegexEndsData ends,
        ref RegexEndsData data)
      {
        bool isSingleString = true;
        StringBuilder singleString = new StringBuilder();
        List<Brick> brickList = new List<Brick>();

        for (int index = startIndex; index < endIndex; ++index)
        {
          RegexEndsData childEnds = ConcatChildEnds(ends, data, startIndex, endIndex, index);

          Element child = element.Parts[index];

          List<Brick> childBricks = VisitElement(child, ref childEnds);

          if (isSingleString && childBricks.Count == 1 &&
            childBricks[0].min == 1 && childBricks[0].max == 1 &&
            childBricks[0].values.Count == 1)
          {
            singleString.Append(childBricks[0].values.Single());
          }
          else
          {
            if (isSingleString)
            {
              if (singleString.Length != 0)
              {
                brickList.Add(new Brick(singleString.ToString(), 1, 1));
              }

              isSingleString = false;
              singleString = null;
            }
            brickList.AddRange(childBricks);
          }
        }

        if (isSingleString && singleString.Length != 0)
        {
          brickList.Add(new Brick(singleString.ToString(), 1, 1));
        }

        return brickList;
      }

      protected override List<Brick> Visit(Empty element, ref RegexEndsData data)
      {
        if (data.LeftClosed && data.RightClosed)
        {
          return new List<Brick> { new Brick("") };
        }
        else
        {
          return new List<Brick> { new Brick(true) };
        }
      }

      private List<Brick> Wrap(Brick brick, RegexEndsData endsData)
      {
        List<Brick> bricks = new List<Brick>();
        if (!endsData.LeftClosed)
        {
          bricks.Add(new Brick(true));
        }
        bricks.Add(brick);
        if (!endsData.RightClosed)
        {
          bricks.Add(new Brick(true));
        }

        return bricks;
      }

      protected override List<Brick> Visit(Loop element, ref RegexEndsData data)
      {
        if (element.Min == 1 && element.Max == 1)
        {
          // Single occurence, just pass the content
          return VisitElement(element.Content, ref data);
        }

        RegexEndsData closedEnds = new RegexEndsData(true, true);
        List<Brick> inner = VisitElement(element.Content, ref closedEnds);

        if (inner.Count == 1 && inner[0].min == 1 && inner[0].max == 1)
        {
          //A brick has single occurence, can apply the loop bounds
          IndexInt min = IndexInt.ForNonNegative(element.Min);
          IndexInt max = element.IsUnbounded ? IndexInt.Infinity : IndexInt.ForNonNegative(element.Max);
          return Wrap(new Brick(inner[0].values, min, max), data);
        }
        else
        {
          return new List<Brick> { new Brick(overapproximate) };
        }
      }

      protected override List<Brick> Visit(SingleElement element, ref RegexEndsData data)
      {
        HashSet<string> chars = new HashSet<string>();

        var intervals = overapproximate ? element.CanMatchIntervals : element.MustMatchIntervals;

        foreach (Tuple<char, char> interval in intervals)
        {
          for (int character = interval.Item1; character <= interval.Item2; ++character)
          {
            chars.Add(((char)character).ToString());
          }
        }

        return Wrap(new Brick(chars), data);
      }
    }



    /// <summary>
    /// Constructs a Bricks abstract element that overapproximates
    /// the specified regex.
    /// </summary>
    /// <param name="regex">A regex AST.</param>
    /// <returns>An abstract element overapproximating <paramref name="regex"/>.</returns>
    public Bricks BricksForRegex(Element regex)
    {
      BricksRegexVisitor visitor = new BricksRegexVisitor(element.Policy, true);

      RegexEndsData ends = new RegexEndsData();

      List<Brick> list = visitor.VisitSimpleRegex(regex, ref ends);

      return new Bricks(list, element.Policy);
    }


    /// <summary>
    /// Verifies whether the bricks match the specified regex expression.
    /// </summary>
    /// <param name="regex">AST of the regex.</param>
    /// <returns>Proven result of the match.</returns>
    public ProofOutcome IsMatch(Element regex)
    {
      Bricks overapproximation = BricksForRegex(regex);
      Bricks canMatchBricks = element.Meet(overapproximation);

      BricksRegexVisitor visitor = new BricksRegexVisitor(element.Policy, false);
      RegexEndsData ends = new RegexEndsData();
      Bricks underapproximation = new Bricks(visitor.VisitSimpleRegex(regex, ref ends), element.Policy);

      bool mustMatch = element.LessThanEqual(underapproximation);

      return ProofOutcomeUtils.Build(!canMatchBricks.IsBottom, !mustMatch);
    }
  }
}
