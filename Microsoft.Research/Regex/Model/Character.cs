// Copyright (c) Charles University
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Created by Vlastimil Dort

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    /// <summary>
    /// Represents a single character in a regex.
    /// </summary>
    public class Character : Element
    {
        private CharRanges mustMatch, canMatch;
        /// <summary>
        /// Gets the ranges that are known to match this element.
        /// </summary>
        public CharRanges MustMatch { get { return mustMatch; } }
        /// <summary>
        /// Gets the ranges of all characters that may match this element.
        /// </summary>
        public CharRanges CanMatch { get { return canMatch; } }

        /// <summary>
        /// Constructs a single character elements from character ranges.
        /// </summary>
        /// <param name="mustMatch">The ranges that are known to match this element.</param>
        /// <param name="canMatch">The ranges of all characters that may match this element.</param>
        public Character(CharRanges mustMatch, CharRanges canMatch)
        {
            this.mustMatch = mustMatch;
            this.canMatch = canMatch;
        }

        public Character(char c)
        {
            var ranges = new CharRanges(new CharRange(c, c));
            this.canMatch = ranges;
            this.mustMatch = ranges;
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append("char(");
            bool first = true;
            foreach(var range in CanMatch.Ranges)
            {
                if (first)
                    first = false;
                else
                    builder.Append(",");

                if(range.Low != range.High)
                    builder.AppendFormat("{0:X}-{1:X}", (int)range.Low, (int)range.High);
                else
                    builder.AppendFormat("{0:X}", (int)range.Low);
            }

            builder.Append(")");
        }
    }

}
