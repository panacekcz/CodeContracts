// Copyright (c) Charles University
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Created by Vlastimil Dort
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Regex.AST;
using Microsoft.Research.Regex.Model;

namespace Microsoft.Research.Regex
{
    /// <summary>
    /// Generates an AST for a regex model.
    /// </summary>
    internal class ModelASTVisitor : Model.ModelVisitor<AST.Element, Void>
    {
        private const string regular = ";/<>!:,=";
        private const string special = ".(){}\\?*+-$^[]";

        /// <summary>
        /// Selects when to use escape sequences to represent characters.
        /// </summary>
        public enum EscapingStrategy
        {
            /// <summary>
            /// Use literals for alphanumeric and ascii symbols without special meaning,
            /// use simple escape for ascii symbols with special meaning,
            /// use unicode escape sequences for non-ascii.
            /// </summary>
            EscapeSpecial,
            /// <summary>
            /// Use literals for alphanumeric characters, unicode escape sequences for all other characters.
            /// </summary>
            EscapeUnicodeExceptAlnum,
            /// <summary>
            /// Use unicode escape sequences for all characters.
            /// </summary>
            EscapeUnicodeAll,
        }

        /// <summary>
        /// Gets or sets the used escaping strategy.
        /// </summary>
        public EscapingStrategy Escaping { get; set; }

        #region ModelVisitor overrides
        protected override AST.Element VisitAnchor(End anchor, ref Void data)
        {
            return new AST.Anchor(AnchorKind.End);
        }

        protected override AST.Element VisitAnchor(Begin anchor, ref Void data)
        {
            return new AST.Anchor(AnchorKind.LineStart);
        }

        private AST.Character Character(char value)
        {
            bool alnumLiteral = (Escaping == EscapingStrategy.EscapeUnicodeExceptAlnum || Escaping == EscapingStrategy.EscapeSpecial);
            bool escapeSpecial = Escaping == EscapingStrategy.EscapeSpecial;
            bool regularLiteral = Escaping == EscapingStrategy.EscapeSpecial;

            if (alnumLiteral && (value >= '0' && value <= '9' || value >= 'a' && value <= 'z' || value >= 'A' && value <= 'Z'))
                return new AST.LiteralCharacter(value);
            else if (regularLiteral && regular.IndexOf(value) != -1)
                return new AST.LiteralCharacter(value);
            else if (escapeSpecial && special.IndexOf(value)!= -1)
                return new AST.DefaultEscapeCharacter(value);
            else
                return new AST.UnicodeEscapeCharacter(value);
        }

        private AST.SingleElement GetCharRange(char low, char high)
        {
            if (low == high)
                return Character(low);
            else
                return new AST.Range(low, high);
        }

        private void AddCharRange(char low, char high, List<AST.SingleElement> destination)
        {
            if (low < char.MaxValue && low + 1 == high)
            {
                destination.Add(Character(low));
                destination.Add(Character(high));
            }
            else
            {
                destination.Add(GetCharRange(low, high));
            }
        }

        protected override AST.Element VisitCharacter(Model.Character character, ref Void data)
        {
            var ranges = character.CanMatch.Ranges.ToArray();

            if (ranges.Length == 1 && ranges[0].Low == ranges[0].High)
            {
                // Single character
                return Character(ranges[0].Low);
            }
            else
            {
                Array.Sort(ranges, (r1, r2) => r1.Low.CompareTo(r2.Low));

                // Set of characters
                var setRanges = new List<AST.SingleElement>();
                int lastLow = 0;
                int lastHigh = -1;

                foreach (var range in ranges)
                {
                    if (range.Low - 1 != lastHigh)
                    {
                        if (lastHigh != -1)
                            AddCharRange((char)lastLow, (char)lastHigh, setRanges);
                        lastLow = range.Low;
                    }

                    lastHigh = range.High;
                }

                if (lastHigh != -1)
                    AddCharRange((char)lastLow, (char)lastHigh, setRanges);

                AST.CharacterSet characterSet = new CharacterSet(false, setRanges, null);
                return characterSet;
            }

        }

        protected override AST.Element VisitConcatenation(Model.Concatenation concatenation, ref Void data)
        {
            var concatAST = new AST.Concatenation();
            foreach(var part in concatenation.Parts)
            {
                var element = VisitElement(part, ref data);

                if (element is Alternation)
                    element = new SimpleGroup(element);

                concatAST.Parts.Add(element);
            }
            return concatAST;
        }

        protected override AST.Element VisitLookaround(Lookaround lookaround, ref Void data)
        {
            var inner = VisitElement(lookaround.Pattern, ref data);
            return new AST.Assertion(false, lookaround.Behind, inner);
        }

        protected override AST.Element VisitLoop(Model.Loop loop, ref Void data)
        {
            var inner = VisitElement(loop.Pattern, ref data);
            if (loop.Min == 1 && loop.Max == 1)
            {
                return inner;
            }
            else
            {
                if (!(inner is AST.SingleElement))
                    inner = new AST.SimpleGroup(inner);

                if (loop.Min == 0 && loop.Max == Model.Loop.Unbounded)
                    return new AST.Iteration(inner, false);
                else if (loop.Min == 1 && loop.Max == Model.Loop.Unbounded)
                    return new AST.PositiveIteration(inner, false);
                else if (loop.Min == 0 && loop.Max == 1)
                    return new AST.Optional(inner, false);
                else
                    return new AST.Loop(loop.Min, loop.Max, inner, false);
            }
            
        }

        protected override AST.Element VisitUnion(Union union, ref Void data)
        {
            var unionAST = new AST.Alternation();
            foreach (var part in union.Patterns)
            {
                unionAST.Patterns.Add(VisitElement(part, ref data));
            }
            return unionAST;
        }

        protected override AST.Element VisitUnknown(Unknown unknown, ref Void data)
        {
            throw new UnknownRegexException("Cannot create AST of regex");
        }
        #endregion

        public AST.Element CreateASTForModel(Model.Element e)
        {
            Void v;
            return VisitElement(e, ref v);
        }
    }
}
