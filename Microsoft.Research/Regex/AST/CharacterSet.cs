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

namespace Microsoft.Research.Regex.AST
{
  /// <summary>
  /// Represents a set of characters.
  /// </summary>
  public class CharacterSet : SingleElement
  {
    private readonly List<SingleElement> elements;
    private readonly bool negative;
    private readonly CharacterSet subtraction;

    /// <summary>
    /// Gets the list of specified characters. Not <see langword="null"/>.
    /// </summary>
    public List<SingleElement> Elements { get { return elements; } }
    /// <summary>
    /// Gets whether the specified characters are not matched (<see langword="true"/>) or matched (<see langword="false"/>).
    /// </summary>
    public bool Negative { get { return negative; } }
    /// <summary>
    /// Gets the set of characters that is subtracted from the specified set. Can be <see langword="null"/>.
    /// </summary>
    public CharacterSet Subtraction { get { return subtraction; } }

    internal CharacterSet(bool negative, List<SingleElement> elements, CharacterSet subtraction)
    {
      System.Diagnostics.Contracts.Contract.Requires(elements != null);

      this.negative = negative;
      this.elements = elements;
      this.subtraction = subtraction;
    }



    internal override void GenerateString(StringBuilder builder)
    {
      builder.Append("[");
      if (negative)
        builder.Append("^");

      foreach (SingleElement element in elements)
      {
        element.GenerateString(builder);
      }

      if (subtraction != null)
      {
        builder.Append('-');
        subtraction.GenerateString(builder);
      }

      builder.Append("]");
    }

    private bool CanPositiveMatch(char character)
    {
      if (elements.Exists(element => element.CanMatch(character)))
      {
        return subtraction == null || !subtraction.MustMatch(character);
      }
      else
      {
        return false;
      }
    }

    private bool MustPositiveMatch(char character)
    {
      if (elements.Exists(element => element.MustMatch(character)))
      {
        return subtraction == null || !subtraction.CanMatch(character);
      }
      else
      {
        return false;
      }
    }


    public override bool CanMatch(char character)
    {
      return negative ? !MustPositiveMatch(character) : CanPositiveMatch(character);
    }

    public override bool MustMatch(char character)
    {
      return negative ? !CanPositiveMatch(character) : MustPositiveMatch(character);
    }

    private IEnumerable<Tuple<char, char>> MatchIntervals(bool canMatch)
    {
      char lower = char.MinValue;
      bool inside = false;
      // Find intervals of matching characters
      for (int character = char.MinValue; character <= char.MaxValue; ++character)
      {
        char charCharacter = (char)character;
        if (canMatch ? CanMatch(charCharacter) : MustMatch(charCharacter))
        {
          if (!inside)
          {
            lower = charCharacter;
            inside = true;
          }
        }
        else if (inside)
        {
          inside = false;
          yield return new Tuple<char, char>(lower, (char)(charCharacter - 1));
        }
      }

      if (inside)
      {
        yield return new Tuple<char, char>(lower, char.MaxValue);
      }


    }

    public override IEnumerable<Tuple<char, char>> CanMatchIntervals
    {
      get
      {
        return MatchIntervals(true);
      }
    }
    public override IEnumerable<Tuple<char, char>> MustMatchIntervals
    {
      get
      {
        return MatchIntervals(false);
      }
    }
  }
}
