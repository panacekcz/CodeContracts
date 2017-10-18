// CodeContracts
// 
// Copyright (c) Charles University
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
    /// Represents a predefined set of characters
    /// </summary>
    public class PredefinedSet : SingleElement
    {

        public enum SetKind
        {
            /// <summary>
            /// Represents word characters (\w).
            /// </summary>
            Word,
            /// <summary>
            /// Represents decimal digits (\d).
            /// </summary>
            DecimalDigit,
            /// <summary>
            /// Represents whitespace characters (\s).
            /// </summary>
            Whitespace
        }

        private readonly bool negative;
        private readonly SetKind kind;

        /// <summary>
        /// Gets whether the set matches the excluded characters.
        /// </summary>
        public bool Negative { get { return negative; } }
        /// <summary>
        /// Gets the kind of predefined set.
        /// </summary>
        public SetKind Kind { get { return kind; } }

        public PredefinedSet(SetKind kind, bool negative)
        {
            this.kind = kind;
            this.negative = negative;
        }

        private bool IsAsciiMatch(char character)
        {
            return IsPositiveAsciiMatch(character) ^ negative;
        }

        private bool IsPositiveAsciiMatch(char character)
        {
            switch (kind)
            {
                case SetKind.Word:
                    return character >= '0' && character <= '9' || character >= 'a' && character <= 'z' || character >= 'A' && character <= 'Z' || character == '_';
                case SetKind.Whitespace:
                    return character >= 9 && character <= 13 || character == 32;
                case SetKind.DecimalDigit:
                    return character >= '0' && character <= '9';
                default:
                    return true;
            }
        }

        public override bool CanMatch(char character)
        {
            if (character >= 128)
                return true;
            else
                return IsAsciiMatch(character);
        }
        public override bool MustMatch(char character)
        {
            if (character >= 128)
                return false;
            else
                return IsAsciiMatch(character);
        }

        private IEnumerable<CharRange> IsMatchRanges(bool overapproximate)
        {
            if (negative)
                return NegativeIsMatchRanges(overapproximate);
            else
                return PositiveIsMatchRanges(overapproximate);
        }

        private IEnumerable<CharRange> NegativeIsMatchRanges(bool overapproximate)
        {
            char current = (char)0;

            foreach(CharRange rng in PositiveIsMatchRanges(false))
            {
                if(rng.Low > current)
                {
                    yield return new CharRange(current, (char)(rng.Low - 1));
                }
                current = (char)(rng.High + 1);
            }

            if (overapproximate)
            {
                yield return new CharRange(current, char.MaxValue);
            }
            else if(current < 128)
            {
                yield return new CharRange(current, (char)127);
            }
        }

        private IEnumerable<CharRange> PositiveIsMatchRanges(bool overapproximate)
        {
  
            switch (kind)
            {
                case SetKind.DecimalDigit:
                    yield return new CharRange('0', '9');
                    break;
                case SetKind.Word:
                    yield return new CharRange('0', '9');
                    yield return new CharRange('A', 'Z');
                    yield return new CharRange('_', '_');
                    yield return new CharRange('a', 'z');
                    break;
                case SetKind.Whitespace:
                    yield return new CharRange((char)9, (char)13);
                    yield return new CharRange(' ', ' ');
                    break;
                default:
                    yield return new CharRange((char)0, (char)127);
                    break;
            }
            
            if (overapproximate)
            {
                yield return new CharRange((char)128, char.MaxValue);
            }
        }

        public override CharRanges CanMatchRanges
        {
            get { return new CharRanges(new CharRange(char.MinValue, char.MaxValue)); }
        }
        public override CharRanges MustMatchRanges
        {
            get { return new CharRanges(); }
        }

        public override string ToString()
        {
            return EscapeSequenceFor(kind, negative);
        }
        private static string EscapeSequenceFor(SetKind kind, bool negative)
        {
            switch (kind)
            {
                case SetKind.Word:
                    return negative ? "\\W" : "\\w";
                case SetKind.Whitespace:
                    return negative ? "\\S" : "\\s";
                case SetKind.DecimalDigit:
                    return negative ? "\\D" : "\\d";
                default:
                    throw new System.ComponentModel.InvalidEnumArgumentException("kind", (int)kind, typeof(SetKind));
            }
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append(ToString());
        }
    }
}
