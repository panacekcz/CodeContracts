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
  /// Represents a single character node in Regex AST.
  /// </summary>
  public class Character : SingleElement
  {
    private readonly char value;

    /// <summary>
    /// Gets the matched character value.
    /// </summary>
    public char Value { get { return value; } }

    /// <summary>
    /// Constructs a character AST node for the specified character.
    /// </summary>
    /// <param name="value">Value of the represented character.</param>
    public Character(char value)
    {
      this.value = value;
    }

    public bool IsMatch(char character)
    {
      return character == value;
    }

    public override bool CanMatch(char character)
    {
      return IsMatch(character);
    }
    public override bool MustMatch(char character)
    {
      return IsMatch(character);
    }

    internal override void GenerateString(StringBuilder builder)
    {
      if (value >= '0' && value <= '9' || value >= 'a' && value <= 'z' || value >= 'A' && value <= 'Z')
        builder.Append(value);
      else
        builder.AppendFormat("\\u{0:X4}", (int)value);
    }

    public IEnumerable<Tuple<char, char>> IsMatchIntervals
    {
      get { yield return new Tuple<char, char>(value, value); }
    }

    public override IEnumerable<Tuple<char, char>> CanMatchIntervals
    {
      get { return IsMatchIntervals; }
    }

    public override IEnumerable<Tuple<char, char>> MustMatchIntervals
    {
      get { return IsMatchIntervals; }
    }
  }
}
