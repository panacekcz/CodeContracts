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
    internal struct ParserInput
    {
        /// <summary>
        /// Input string.
        /// </summary>
        private readonly string inputString;
        /// <summary>
        /// Current position.
        /// </summary>
        private int current;

        public ParserInput(string inputString)
        {
            this.inputString = inputString;
            this.current = 0;
        }


        public bool HasNext()
        {
            return current < inputString.Length;
        }
        public char Current()
        {
            return inputString[current];
        }
        public char Next()
        {
            return inputString[current++];
        }
        public void Prev()
        {
            --current;
        }

        public int Position { get { return current; } set { current = value; } }

        public string ParseUntil(char end)
        {
            int index = inputString.IndexOf(end, current);
            if (index < 0)
            {
                throw new ParseException("expected " + end);
            }
            int old = current;
            current = index + 1;
            return inputString.Substring(old, index - old);
        }

    }

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
        /// <summary>
        /// Input string.
        /// </summary>
        ParserInput input;
        /// <summary>
        /// Current number for an unnamed group.
        /// </summary>
        private int groupCounter;
        #endregion

        private RegexParser(string inputString)
        {
            this.input = new ParserInput(inputString);
            this.groupCounter = 1;
        }
       
        AST.Element ParseUnicodeEscapeSeq()
        {
            int c = ParseHexadecimal(4);
            return new AST.UnicodeEscapeCharacter((char)c);
        }

        AST.Element ParseOctalEscapeSeq()
        {
            return null;
        }

        /// <summary>
        /// Parses control sequence such as "\\cA".
        /// </summary>
        /// <returns>AST element for the control sequence.</returns>
        AST.Element ParseControlSeq()
        {
            char c = input.Next();

            if (c >= 'A' && c <= 'Z')
            {
                return new AST.ControlEscapeCharacter((char)(c - 'A' + 1));
            }
            else
            {
                throw new ParseException("Invalid control sequence");
            }
        }

        AST.Element ParseNamedCategory(bool negative)
        {
            string name = input.ParseUntil('>');
            AST.NamedSet namedSet = new AST.NamedSet(name, negative);
            return namedSet;
        }

        AST.Element ParseHexadecimal()
        {
            int c = ParseHexadecimal(2);
            return new AST.HexadecimalEscapeCharacter((char)c);
        }
        AST.Element ParseBackReference()
        {
            char lt = input.Next();

            if (lt != '<')
            {
                throw new ParseException("Invalid backreference");
            }

            string name = input.ParseUntil('>');

            return new AST.Reference(name);
        }

        AST.Element ParseEscapeSeq(bool set)
        {
            if (!input.HasNext())
            {
                throw new ParseException("Invalid escape sequence");
            }

            char c = input.Next();
            switch (c)
            {
                case 'a':
                case 'e':
                case 'f':
                case 'n':
                case 'v':
                case 't':
                case 'r':
                    return new AST.EscapeCharacter(c);
                case 'A':
                    return new AST.Anchor(AST.AnchorKind.StringStart);
                case 'b':
                    if (set)
                        return new AST.EscapeCharacter('b');
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
                case 'G':
                    return new AST.Anchor(AST.AnchorKind.PreviousMatchEnd);
                case 'k':
                    return ParseBackReference();
                case 'p':
                    return ParseNamedCategory(false);
                case 'P':
                    return ParseNamedCategory(true);
                case 's':
                    return new AST.PredefinedSet(AST.PredefinedSet.SetKind.Whitespace, false);
                case 'S':
                    return new AST.PredefinedSet(AST.PredefinedSet.SetKind.Whitespace, true);
                case 'u':
                    return ParseUnicodeEscapeSeq();
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
                case '1':
                case '2':
                case '3':
                case '4':
                case '5':
                case '6':
                case '7':
                case '8':
                case '9':
                    input.Prev();
                    return new AST.Reference(ParseDecimal().ToString());
                default:
                    return new AST.DefaultEscapeCharacter(c);
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

            while (input.HasNext())
            {
                char c = input.Next();

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
                            elements.Add(new AST.LiteralCharacter(']'));
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
                            elements.Add(new AST.LiteralCharacter('^'));
                            state = RangeState.Char;
                        }
                        break;
                    case '-':
                        char end = input.Next();

                        if (end == ']')
                        {
                            // End of set, add - as character
                            elements.Add(new AST.LiteralCharacter('-'));
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
                            elements.Add(new AST.LiteralCharacter('-'));
                            state = RangeState.Char;
                        }
                        break;
                    default:
                        elements.Add(new AST.LiteralCharacter(c));
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


        AST.Element ParseGroup()
        {
            if (input.Next() != '?')
            {
                input.Prev();
                return new AST.Capture(NextGroup(), Parse(true));
            }
            else
            {
                switch (input.Next())
                {
                    case '<':
                        if (input.Current() == '=') {
                            input.Next();
                            return new AST.Assertion(false, true, Parse(true));
                        }
                        else if (input.Current() == '!')
                        {
                            input.Next();
                            return new AST.Assertion(true, true, Parse(true));
                        }
                        else
                            return new AST.Capture(input.ParseUntil('>'), Parse(true));
                    case '\'':
                        return new AST.Capture(input.ParseUntil('\''), Parse(true));
                    case '>':
                        return new AST.NonBacktracking(Parse(true));
                    case '=':
                        return new AST.Assertion(false, false, Parse(true));
                    case '!':
                        return new AST.Assertion(true, false, Parse(true));
                    case '(':
                        return new AST.Alternation();
                    case '#':
                        return new AST.Comment(input.ParseUntil(')'));
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
            public QuantifierFactory quantifierFactory;

            private void FinishQuantifier()
            {
                if (currentElement != null && quantifierFactory != null)
                {
                    currentElement = quantifierFactory.Create(currentElement);
                    quantifierFactory = null;
                }

            }

            private void FinishCat()
            {
                FinishQuantifier();

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
                FinishQuantifier();

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

            public void Quantifier(QuantifierFactory factory)
            {
                if (currentElement == null)
                {
                    throw new ParseException("Quantifier without inner element");
                }
                if (quantifierFactory != null)
                {
                    throw new ParseException("Nested quantifier");
                }
                quantifierFactory = factory;
            }

            public void MakeLoopLazy()
            {
                if(quantifierFactory == null)
                    throw new ParseException("Missing quantifier");
                quantifierFactory.Lazy = true;
            }
        }
        int ParseDecimal()
        {
            try
            {
                checked
                {
                    int r = 0;
                    char c = input.Next();

                    while (c >= '0' && c <= '9')
                    {
                        r = r * 10 + (c - '0');

                        if (input.HasNext())
                            c = input.Next();
                        else
                            return r;
                    }

                    input.Prev();

                    return r;
                }
            }
            catch(OverflowException)
            {
                throw new ParseException("Integer value too large");
            }
        }

        int ParseHexadecimal(int l)
        {
            int r = 0;

            for (int i = 0; i < l; ++i)
            {
                char c = input.Next();
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

        QuantifierFactory TryParseRepeats(AST.Element inner, out AST.Element outElement)
        {
            int before = input.Position;
            int min = ParseDecimal();

            if (input.Position == before)
            {
                //If there is no digit, it is not a quantifier
                outElement = new AST.LiteralCharacter('{');
                return null;
            }

            char c = input.Next();

            // If the maximum is not specified at all, it is the same as minimum.
            int max = min;
            if (c == ',')
            {
                int beforeMax = input.Position;
                max = ParseDecimal();
                if (input.Position == beforeMax)
                {
                    // If the maximum is empty, it is unbounded
                    max = AST.Loop.UNBOUNDED;
                }
                c = input.Next();
            }
            if (c != '}')
            {
                //If there is an unexpected character, treat the brace as a character
                input.Position = before;
                outElement = new AST.LiteralCharacter('{');
                return null;
            }

            outElement = null;
            return new LoopFactory { Min = min, Max = max };
        }

        private AST.Element Parse(bool group)
        {
            ExpressionBuilder builder = new ExpressionBuilder();

            while (input.HasNext())
            {

                char c = input.Next();
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
                        builder.Quantifier(new PositiveIterationFactory());
                        break;
                    case '?':
                        if (builder.quantifierFactory != null)
                        {
                            builder.quantifierFactory.Lazy = true;
                        }
                        else
                        {
                            builder.Quantifier(new OptionalFactory());
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
                        AST.Element normalElement;
                        QuantifierFactory qf = TryParseRepeats(builder.currentElement, out normalElement);
                        if(normalElement != null)
                            builder.CatElement(normalElement);
                        else if(qf != null)
                            builder.Quantifier(qf);
                        break;
                    case '.':
                        builder.CatElement(new AST.Wildcard());
                        break;
                    case '*':
                        builder.Quantifier(new IterationFactory());
                        break;
                    case '[':
                        builder.CatElement(ParseRange());
                        break;
                    case '\\':
                        builder.CatElement(ParseEscapeSeq(false));
                        break;
                    default:
                        builder.CatElement(new AST.LiteralCharacter(c));
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

        private abstract class QuantifierFactory
        {
            public bool Lazy { get; set; }
            public abstract AST.Quantifier Create(AST.Element inner);
        }

        private class LoopFactory : QuantifierFactory
        {
            public int Min { get; set; }
            public int Max { get; set; }
            public override Quantifier Create(Element inner)
            {
                return new Loop(Min, Max, inner, Lazy);
            }
        }
        private class OptionalFactory : QuantifierFactory
        {
            public override Quantifier Create(Element inner)
            {
                return new Optional(inner, Lazy);
            }
        }
        private class IterationFactory : QuantifierFactory
        {
            public override Quantifier Create(Element inner)
            {
                return new Iteration(inner, Lazy);
            }
        }
        private class PositiveIterationFactory : QuantifierFactory
        {
            public override Quantifier Create(Element inner)
            {
                return new PositiveIteration(inner, Lazy);
            }
        }
    }
}
