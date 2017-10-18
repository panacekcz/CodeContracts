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

namespace Microsoft.Research.Regex
{
    public static class RegexExtensions
    {
        /// <summary>
        /// Determines, whether the Regex element is a constant character.
        /// If yes, stores the character in an output parameter.
        /// </summary>
        /// <param name="element">The regex AST element.</param>
        /// <param name="character">Destination of the constant character.</param>
        /// <returns><see langword="true"/>, if the element is a constant character</returns>
        public static bool IsConstantChar(this AST.Element element, out char character)
        {
            if (!(element is AST.Character))
            {
                character = default(char);
                return false;
            }
            AST.Character characterNode = (AST.Character)element;
            character = characterNode.Value;
            return true;
        }
        /// <summary>
        /// Determnies, whether the regex element is a start-of-string anchor.
        /// </summary>
        /// <param name="element">The regex element.</param>
        /// <returns><see langword="true"/>, if <paramref name="element"/> is a string start anchor</returns>
        public static bool IsStartAnchor(this AST.Element element)
        {
            if (!(element is AST.Anchor))
            {
                return false;
            }
            var anchor = (AST.Anchor)element;
            return anchor.Kind == AST.AnchorKind.LineStart || anchor.Kind == AST.AnchorKind.StringStart;
        }
        /// <summary>
        /// Determnies, whether the regex element is an end-of-string anchor.
        /// </summary>
        /// <param name="element">The regex element.</param>
        /// <returns><see langword="true"/>, if <paramref name="element"/> is a string end anchor</returns>
        public static bool IsEndAnchor(this AST.Element element)
        {
            if (!(element is AST.Anchor))
            {
                return false;
            }
            var anchor = (AST.Anchor)element;
            return anchor.Kind == AST.AnchorKind.End;
        }
        /// <summary>
        /// Determines whether the single-character regex element has no
        /// possible matching character.
        /// </summary>
        /// <param name="singleElement">A single-character regex element.</param>
        /// <returns><see langword="true"/>, if <paramref name="singleElement"/> cannot match any character.</returns>
        public static bool IsEmptyCanMatchSet(this AST.SingleElement singleElement)
        {
            return !singleElement.CanMatchRanges.Ranges.Any();
        }

        /// <summary>
        /// Determines whether the single-character regex element one possible
        /// matching character
        /// </summary>
        /// <param name="singleElement">A single-character regex element.</param>
        /// <param name="singleChar">Stores the single character.</param>
        /// <returns><see langword="true"/>, if <paramref name="singleElement"/> can match one character.</returns>
        public static bool TryCanMatchSingleChar(this AST.SingleElement singleElement, out char singleChar)
        {
            var intervals = singleElement.CanMatchRanges;
            using (var enumerator = intervals.Ranges.GetEnumerator())
            {
                singleChar = '\0';
                if (!enumerator.MoveNext())
                {
                    return false;
                }

                var interval = enumerator.Current;
                if (interval.Low == interval.High)
                {
                    singleChar = interval.Low;
                }
                else
                {
                    return false;
                }

                if (enumerator.MoveNext())
                {
                    return false;
                }

                return true;
            }
        }

        public static bool IsEmptyConcatenation(this AST.Element element)
        {
            return element is AST.Empty || (element is AST.Concatenation && ((AST.Concatenation)element).Parts.Count == 0);
        }
    }
}
