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

namespace Microsoft.Research.Regex
{
  /// <summary>
  /// Parser for regular expression.
  /// </summary>
  public class RegexParser
  {
    /// <summary>
    /// Parses a regular expression string.
    /// </summary>
    /// <param name="input">The regular expression string.</param>
    /// <returns>The AST for <paramref name="input"/>.</returns>
    public static AST.Element Parse(string input)
    {
      RegexParser parser = new RegexParser(input);
      return parser.Parse(false);
    }

    #region Private state
    private readonly string input;
    private int current;
    private int groupCounter;
    #endregion

    private RegexParser(string input)
    {
      this.input = input;
      this.current = 0;
      this.groupCounter = 1;
    }

    private bool HasNext()
    {
      return current < input.Length;
    }
    private char Next()
    {
      return input[current++];
    }
    private void Prev()
    {
      --current;
    }

    AST.Element ParseUnicodeEscapeSeq()
    {
      int c = ParseHexadecimal(4);
      return new AST.Character((char)c);
    }


    AST.Element ParseOctalEscapeSeq()
    {
      return null;
    }

    AST.Element ParseControlSeq()
    {
      char c = Next();

      if (c >= 'A' && c <= 'Z')
      {
        return new AST.Character((char)(c - 'A' + 1));
      }
      else
      {
        throw new ParseException("Invalid control sequence");
      }
    }

    AST.Element ParseNamedCategory(bool negative)
    {
      string name = ParseUntil('>');
      AST.NamedSet namedSet = new AST.NamedSet(name, negative);
      return namedSet;
    }

    AST.Element ParseHexadecimal()
    {
      int c = ParseHexadecimal(2);
      return new AST.Character((char)c);
    }
    AST.Element ParseBackReference()
    {
      char lt = Next();

      if (lt != '<')
      {
        throw new ParseException("Invalid backreference");
      }

      string name = ParseUntil('>');

      return new AST.Reference(name);
    }

    AST.Element ParseEscapeSeq(bool set)
    {
      if (!HasNext())
      {
        throw new ParseException("Invalid escape sequence");
      }

      char c = Next();
      switch (c)
      {
        case 'a':
          return new AST.Character('\u0007');
        case 'A':
          return new AST.Anchor(AST.AnchorKind.StringStart);
        case 'b':
          if (set)
            return new AST.Character('\u0008');
          else
            return new AST.Boundary(false);

        case 'B':
          return new AST.Boundary(true);
        case 'c':
          return ParseControlSeq();
        case 'd':
          return new AST.PredefinedSet(AST.PredefinedSet.SetKind.DecimalDigit, false);
        case 'D':
          return new AST.PredefinedSet(AST.PredefinedSet.SetKind.DecimalDigit, true);
        case 'e':
          return new AST.Character('\u001B');
        case 'f':
          return new AST.Character('\u000C');
        case 'G':
          return new AST.UnsupportedElement();
        case 'k':
          return ParseBackReference();
        case 'n':
          return new AST.Character('\n');
        case 'p':
          return ParseNamedCategory(false);
        case 'P':
          return ParseNamedCategory(true);
        case 'r':
          return new AST.Character('\u000d');
        case 's':
          return new AST.PredefinedSet(AST.PredefinedSet.SetKind.Whitespace, false);
        case 'S':
          return new AST.PredefinedSet(AST.PredefinedSet.SetKind.Whitespace, true);
        case 't':
          return new AST.Character('\u0009');
        case 'u':
          return ParseUnicodeEscapeSeq();
        case 'v':
          return new AST.Character('\u000B');
        case 'w':
          return new AST.PredefinedSet(AST.PredefinedSet.SetKind.Word, false);
        case 'W':
          return new AST.PredefinedSet(AST.PredefinedSet.SetKind.Word, true);
        case 'x':
          return ParseHexadecimal();
        case 'z':
          return new AST.Anchor(AST.AnchorKind.End);
        case 'Z':
          return new AST.Anchor(AST.AnchorKind.StringEnd);
        case '0':
          return ParseOctalEscapeSeq();
        default:
          return new AST.Character(c);
      }

    }

    private enum RangeState
    {
      Negative, First, Char, Other, Subtraction
    }

    AST.CharacterSet ParseRange()
    {
      bool negative = false;
      List<AST.SingleElement> elements = new List<AST.SingleElement>();
      AST.CharacterSet subtraction = null;
      //Subtraction must be the last element

      //special chars: - ^ ] [
      // ] must be escaped

      // subtraction is -, followed by [
      // subtraction -[abc]  cannot be empty, otherwise it is a character ]
      // example: 
      // "^[\\]a-[]]]*$"

      RangeState state = RangeState.Negative;

      while (HasNext())
      {
        char c = Next();

        if (c != ']' && state == RangeState.Subtraction)
        {
          throw new ParseException("Subtraction must be at the end");
        }

        switch (c)
        {
          case '\\':
            AST.Element el = ParseEscapeSeq(true);
            elements.Add((AST.SingleElement)el);
            if (el is AST.Character)
              state = RangeState.Char;
            else
              state = RangeState.Other;
            break;
          case ']':
            if (state <= RangeState.First)
            {
              // if it is the first character, it is taken literally
              elements.Add(new AST.Character(']'));
              state = RangeState.Char;
            }
            else
            {
              // otherwise, ends the set
              return new AST.CharacterSet(negative, elements, subtraction);
            }
            break;
          case '^':
            if (state == RangeState.Negative)
            {
              // directly after the '[', means the range is negative
              negative = true;
              state = RangeState.First;
            }
            else
            {
              elements.Add(new AST.Character('^'));
              state = RangeState.Char;
            }
            break;
          case '-':
            char end = Next();

            if (end == ']')
            {
              // End of set, add - as character
              elements.Add(new AST.Character('-'));
              return new AST.CharacterSet(negative, elements, subtraction);
            }
            else if (end == '[')
            {
              // Subtraction set
              subtraction = ParseRange();
              // The subtraction set must be at the end
              state = RangeState.Subtraction;
            }
            else if (state == RangeState.Char)
            {
              //Range
              AST.Character start = (AST.Character)elements[elements.Count - 1];
              if (start.Value > end)
              {
                throw new ParseException("Inverse range");
              }

              elements[elements.Count - 1] = new AST.Range(start.Value, end);
              state = RangeState.Other;
            }
            else
            {
              elements.Add(new AST.Character('-'));
              state = RangeState.Char;
            }
            break;
          default:
            elements.Add(new AST.Character(c));
            state = RangeState.Char;
            break;
        }
      }
      throw new ParseException("Expected ]");
    }

    string NextGroup()
    {
      return groupCounter++.ToString();
    }

    string ParseUntil(char end)
    {
      int index = input.IndexOf(end, current);
      if (index < 0)
      {
        throw new ParseException("expected " + end);
      }
      int old = current;
      current = index + 1;
      return input.Substring(old, index - old);
    }

    AST.Element ParseGroup()
    {
      if (Next() != '?')
      {
        return new AST.Capture(NextGroup(), Parse(true));
      }
      else
      {
        switch (Next())
        {
          case '<':
            //TODO: lookbehind; name can contain only word chars
            return new AST.Capture(ParseUntil('>'), Parse(true));
          case '\'':
            return new AST.Capture(ParseUntil('\''), Parse(true));
          case '>':
            return new AST.NonBacktracking(Parse(true));
          case '=':
            return new AST.Assertion(false, false, Parse(true));
          case '!':
            return new AST.Assertion(true, false, Parse(true));
          case '(':
            return new AST.Alternation();
          case '#':
            return new AST.Comment(ParseUntil(')'));
          case ':':
            return new AST.SimpleGroup(Parse(true));
          default:
            throw new ParseException("Unknown group");
        }
      }
    }

    private struct ExpressionBuilder
    {
      public AST.Alternation currentAlternation;
      public AST.Concatenation currentConcatenation;
      public AST.Element currentElement;

      private void FinishCat()
      {
        if (currentConcatenation != null && currentElement != null)
        {
          currentConcatenation.Parts.Add(currentElement);
          currentElement = null;
        }
      }

      public AST.Element Build()
      {
        FinishCat();
        AST.Element el = currentConcatenation ?? currentElement ?? new AST.Empty();

        if (currentAlternation != null)
        {
          currentAlternation.Patterns.Add(el);

          return currentAlternation;
        }
        else
        {
          return el;
        }
      }
      public void CatElement(AST.Element el)
      {
        if (currentElement != null)
        {
          if (currentConcatenation == null)
          {
            currentConcatenation = new AST.Concatenation();
          }
          currentConcatenation.Parts.Add(currentElement);
        }
        currentElement = el;
      }
      public void Alternative()
      {
        FinishCat();
        if (currentAlternation == null)
        {
          currentAlternation = new AST.Alternation();
        }

        currentAlternation.Patterns.Add(currentConcatenation ?? currentElement ?? new AST.Empty());
        currentElement = null;
        currentConcatenation = null;
      }

      public void Loop(int min, int max)
      {
        if (currentElement == null)
        {
          throw new ParseException("Quantifier without inner element");
        }
        if (currentElement is AST.Loop)
        {
          throw new ParseException("Nested quantifier");
        }
        currentElement = new AST.Loop(min, max, currentElement, false);
      }

      public void MakeLoopLazy()
      {
        AST.Loop oldLoop = (AST.Loop)currentElement;
        currentElement = new AST.Loop(oldLoop.Min, oldLoop.Max, oldLoop.Content, true);
      }
    }
    int ParseDecimal()
    {
      int r = 0;

      while (input[current] >= '0' && input[current] <= '9')
      {
        r = r * 10 + input[current] - '0';
        ++current;
      }

      return r;
    }

    int ParseHexadecimal(int l)
    {
      int r = 0;

      for (int i = 0; i < l; ++i)
      {
        char c = Next();
        int q = 0;
        if (c >= '0' && c <= '9')
        {
          q = c - '0';
        }
        else if (c >= 'a' && c <= 'f')
        {
          q = c - 'a' + 10;
        }
        else if (c >= 'A' && c <= 'F')
        {
          q = c - 'A' + 10;
        }
        else
        {
          throw new ParseException("Invalid hexadecimal digit");
        }

        r += (q) << (4 * (l - i - 1));
      }

      return r;
    }

    AST.Element ParseRepeats(AST.Element inner)
    {
      int min = ParseDecimal();
      int max = -1;
      char c = Next();
      if (c == ',')
      {
        max = ParseDecimal();
        c = Next();
      }
      if (c != '}')
      {
        throw new ParseException("Expected }");
      }

      return new AST.Loop(min, max, inner, false);
    }

    private AST.Element Parse(bool group)
    {
      ExpressionBuilder builder = new ExpressionBuilder();

      while (HasNext())
      {

        char c = Next();
        switch (c)
        {
          case ')':
            if (group)
              return builder.Build();
            else
              throw new ParseException("Unexpected )");
          case '|':
            builder.Alternative();
            break;
          case '+':
            builder.Loop(1, -1);
            break;
          case '?':
            if (builder.currentElement is AST.Loop)
            {
              builder.MakeLoopLazy();
            }
            else
            {
              builder.Loop(0, 1);
            }
            break;
          case '(':
            builder.CatElement(ParseGroup());
            break;
          case '^':
            builder.CatElement(new AST.Anchor(AST.AnchorKind.LineStart));
            break;
          case '$':
            builder.CatElement(new AST.Anchor(AST.AnchorKind.LineEnd));
            break;
          case '{':
            //TODO: quantifiers cannot be nested
            builder.currentElement = ParseRepeats(builder.currentElement);
            break;
          case '.':
            builder.CatElement(new AST.Wildcard());
            break;
          case '*':
            builder.Loop(0, AST.Loop.UNBOUNDED);
            break;
          case '[':
            builder.CatElement(ParseRange());
            break;
          case '\\':
            builder.CatElement(ParseEscapeSeq(false));
            break;
          default:
            builder.CatElement(new AST.Character(c));
            break;
        }
      }

      if (group)
      {
        throw new ParseException("Missing )");
      }
      else
      {
        return builder.Build();
      }
    }
  }
}
