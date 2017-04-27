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
    public abstract class Character : SingleElement
    {
        protected readonly char value;

        /// <summary>
        /// Gets the matched character value.
        /// </summary>
        public char Value { get { return value; } }

        protected Character(char value)
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

        public IEnumerable<CharRange> IsMatchIntervals
        {
            get { yield return new CharRange(value, value); }
        }

        public override CharRanges CanMatchRanges
        {
            get { return new CharRanges(IsMatchIntervals); }
        }

        public override CharRanges MustMatchRanges
        {
            get { return new CharRanges(IsMatchIntervals); }
        }
    }

    public class UnicodeEscapeCharacter : Character
    {
        /// <summary>
        /// Constructs a character AST node for the specified character.
        /// </summary>
        /// <param name="value">Value of the represented character.</param>
        public UnicodeEscapeCharacter(char value)
            : base(value)
        {
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.AppendFormat("\\u{0:X4}", (int)value);
        }
    }

    public class DefaultEscapeCharacter : Character
    {
        /// <summary>
        /// Constructs a character AST node for the specified character.
        /// </summary>
        /// <param name="value">Value of the represented character.</param>
        public DefaultEscapeCharacter(char value)
            : base(value)
        {
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.AppendFormat("\\{0}", value);
        }
    }

    public class ControlEscapeCharacter : Character
    {
        /// <summary>
        /// Constructs a character AST node for the specified character.
        /// </summary>
        /// <param name="value">Value of the represented character.</param>
        public ControlEscapeCharacter(char value)
            : base(value)
        {
            if (value == 0 || value > 'Z' - 'A' + 1)
                throw new ArgumentOutOfRangeException();
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.AppendFormat("\\c{0}", (char)('A' + value - 1));
        }
    }

    public class HexadecimalEscapeCharacter : Character
    {
        /// <summary>
        /// Constructs a character AST node for the specified character.
        /// </summary>
        /// <param name="value">Value of the represented character.</param>
        public HexadecimalEscapeCharacter(char value)
            : base(value)
        {
            if (value > 255)
                throw new ArgumentOutOfRangeException();
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.AppendFormat("\\x{0:X2}", (int)value);
        }
    }
    public class EscapeCharacter : Character
    {
        private readonly char escapeChar;

        public static char GetValue(char escapeChar)
        {
            switch (escapeChar) {
                case 'a':
                    return '\u0007';
                case 'b':
                    return '\u0008';
                case 'e':
                    return '\u001B';
                case 'f':
                    return '\u000C';
                case 'n':
                    return '\n';
                case 'r':
                    return '\u000d';
                case 't':
                    return '\u0009';
                case 'v':
                    return '\u000B';
                default:
                    throw new ArgumentException();
            }
        }
        /// <summary>
        /// Constructs a character AST node for the specified character.
        /// </summary>
        /// <param name="value">Value of the represented character.</param>
        public EscapeCharacter(char escapeChar)
            : base(GetValue(escapeChar))
        {
            this.escapeChar = escapeChar;
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.AppendFormat("\\{0}", escapeChar);
        }
    }
    
    public class LiteralCharacter : Character
    { 
        /// <summary>
        /// Constructs a character AST node for the specified character.
        /// </summary>
        /// <param name="value">Value of the represented character.</param>
        public LiteralCharacter(char value)
            : base(value)
        {
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append(value);   
        }
    }
}
