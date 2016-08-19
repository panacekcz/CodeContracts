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

namespace Microsoft.Research.Regex
{
  internal struct Void { }
  internal class CheckSupportVisitor : RegexVisitor<bool, Void>
  {
    public bool Check(Element regex)
    {
      Void unusedData;
      return VisitElement(regex, ref unusedData);
    }

    protected override bool Visit(Alternation element, ref Void data)
    {
      foreach (Element child in element.Patterns)
      {
        if (!VisitElement(child, ref data))
          return false;
      }
      return true;
    }

    protected override bool Visit(Anchor element, ref Void data)
    {
      return true;
    }

    protected override bool Visit(Assertion element, ref Void data)
    {
      return false;
    }

    protected override bool Visit(Boundary element, ref Void data)
    {
      return false;
    }

    protected override bool Visit(Capture element, ref Void data)
    {
      return VisitElement(element.Content, ref data);
    }

    protected override bool Visit(Comment element, ref Void data)
    {
      return true;
    }

    protected override bool Visit(Concatenation element, ref Void data)
    {
      foreach (Element child in element.Parts)
      {
        if (!VisitElement(child, ref data))
          return false;
      }
      return true;
    }

    protected override bool Visit(Empty element, ref Void data)
    {
      return true;
    }

    protected override bool Visit(Loop element, ref Void data)
    {
      return VisitElement(element.Content, ref data);
    }

    protected override bool Visit(NonBacktracking element, ref Void data)
    {
      return false;
    }

    protected override bool Visit(Options element, ref Void data)
    {
      return false;
    }

    protected override bool Visit(OptionsGroup element, ref Void data)
    {
      return false;
    }

    protected override bool Visit(Reference element, ref Void data)
    {
      return false;
    }

    protected override bool Visit(SimpleGroup element, ref Void data)
    {
      return VisitElement(element.Content, ref data);
    }

    protected override bool Visit(SingleElement element, ref Void data)
    {
      return true;
    }

    protected override bool VisitUnsupported(Element element, ref Void data)
    {
      return false;
    }
  }

  /// <summary>
  /// Visits the AST of a regex limited to concatenation, union, loops, anchors
  /// and character sets.
  /// </summary>
  /// <typeparam name="Result">The type of result passed bottom up.</typeparam>
  /// <typeparam name="Data">The type of data passed along the traversal.</typeparam>
  public abstract class SimpleRegexVisitor<Result, Data> : RegexVisitor<Result, Data>
  {
    /// <summary>
    /// Traverses the AST of a simple regex.
    /// </summary>
    /// <param name="regex">The regex.</param>
    /// <param name="data">The data passed along.</param>
    /// <returns>The result.</returns>
    public Result VisitSimpleRegex(Element regex, ref Data data)
    {
      CheckSupportVisitor checker = new CheckSupportVisitor();
      bool ok = checker.Check(regex);
      if (ok)
      {
        return VisitElement(regex, ref data);
      }
      else
      {
        return Unsupported(regex, ref data);
      }
    }
    /// <summary>
    /// Handles the regex which contains unsupported nodes.
    /// </summary>
    /// <param name="regex">The regex.</param>
    /// <param name="data">The data passed along.</param>
    /// <returns>The result.</returns>
    protected abstract Result Unsupported(Element regex, ref Data data);
    /// <inheritdoc/>
    protected override Result Visit(SimpleGroup element, ref Data data)
    {
      // Simple group is transparent
      return VisitElement(element.Content, ref data);
    }
    /// <inheritdoc/>
    protected override Result Visit(Capture element, ref Data data)
    {
      // Capture group is transparent
      return VisitElement(element.Content, ref data);
    }
    /// <inheritdoc/>
    protected override Result Visit(Comment element, ref Data data)
    {
      // Comment is ignored
      return Visit(new Empty(), ref data);
    }

    #region Invalid elements (filtered by CheckSupportVisitor)
    /// <inheritdoc cref="VisitUnsupported"/>
    protected override Result Visit(Boundary element, ref Data data)
    {
      throw new InvalidOperationException();
    }
    /// <inheritdoc cref="VisitUnsupported"/>
    protected override Result Visit(Reference element, ref Data data)
    {
      throw new InvalidOperationException();
    }
    /// <inheritdoc cref="VisitUnsupported"/>
    protected override Result Visit(Assertion element, ref Data data)
    {
      throw new InvalidOperationException();
    }
    /// <inheritdoc cref="VisitUnsupported"/>
    protected override Result Visit(NonBacktracking element, ref Data data)
    {
      throw new InvalidOperationException();
    }
    /// <inheritdoc cref="VisitUnsupported"/>
    protected override Result Visit(Options element, ref Data data)
    {
      throw new InvalidOperationException();
    }
    /// <inheritdoc cref="VisitUnsupported"/>
    protected override Result Visit(OptionsGroup element, ref Data data)
    {
      throw new InvalidOperationException();
    }
    /// <summary>
    /// This method should not be called.
    /// </summary>
    /// <param name="element">Unused.</param>
    /// <param name="data">Unused.</param>
    /// <returns>No return value.</returns>
    protected override Result VisitUnsupported(Element element, ref Data data)
    {
      throw new InvalidOperationException();
    }
    #endregion
  }
}
