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
  /// Specifies the kind of Regex anchor.
  /// </summary>
  public enum AnchorKind
  {
    StringStart, //A
    StringEnd, //Z
    LineStart, //^
    LineEnd, //$
    End //z
  }

  /// <summary>
  /// Represents a zero-width anchor.
  /// </summary>
  public class Anchor : Element
  {
    private readonly AnchorKind kind;

    /// <summary>
    /// Gets the kind of this anchor.
    /// </summary>
    public AnchorKind Kind
    {
      get { return kind; }
    }

    public Anchor(AnchorKind kind)
    {
      this.kind = kind;
    }

    public override string ToString()
    {
      switch (kind)
      {
        case AnchorKind.StringStart:
          return "\\A";
        case AnchorKind.StringEnd:
          return "\\Z";
        case AnchorKind.LineStart:
          return "^";
        case AnchorKind.LineEnd:
          return "$";
        case AnchorKind.End:
          return "\\z";
        default:
          return "(?ANCHOR)";
      }
    }

    internal override void GenerateString(StringBuilder builder)
    {
      builder.Append(ToString());
    }
  }
}
