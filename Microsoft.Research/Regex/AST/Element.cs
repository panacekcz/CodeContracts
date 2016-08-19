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
  /// Represents an element of a regex syntax.
  /// </summary>
  public abstract class Element
  {
    internal abstract void GenerateString(StringBuilder builder);

    public override string ToString()
    {
      StringBuilder builder = new StringBuilder();
      GenerateString(builder);
      return builder.ToString();
    }
  }

  /// <summary>
  /// Represensts an element which matches exactly one character.
  /// </summary>
  public abstract class SingleElement : Element
  {
    /// <summary>
    /// Determines whether the character could match the element.
    /// </summary>
    /// <param name="character">The checked character.</param>
    /// <returns><see langword="true"/>, if <paramref name="character"/> could match the element.</returns>
    public abstract bool CanMatch(char character);
    /// <summary>
    /// Determines whether the character must match the element.
    /// </summary>
    /// <param name="character">The checked character.</param>
    /// <returns><see langword="true"/>, if <paramref name="character"/> must match the element.</returns>
    public abstract bool MustMatch(char character);

    /// <summary>
    /// Enumerates ranges of characters, that could match the element.
    /// </summary>
    /// <remarks>
    /// All characters outside the enumerated ranges must not match.
    /// </remarks>
    public abstract IEnumerable<Tuple<char, char>> CanMatchIntervals { get; }
    /// <summary>
    /// Enumerates ranges of characters, that must match the element.
    /// </summary>
    public abstract IEnumerable<Tuple<char, char>> MustMatchIntervals { get; }
  }

  /// <summary>
  /// Represents an element which is not supported by this implementation.
  /// </summary>
  public class UnsupportedElement : Element
  {
    internal override void GenerateString(StringBuilder builder)
    {
      builder.Append("(?UNSUPPORTED)");
    }
  }
}
