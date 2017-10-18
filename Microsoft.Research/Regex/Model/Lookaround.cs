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
    /// Represents a positive lookaround in a regex.
    /// </summary>
    public class Lookaround : Element
    {
        /// <summary>
        /// Gets or sets the direction (true = lookbehind, false = lookahead).
        /// </summary>
        public bool Behind { get; set; }
        /// <summary>
        /// Gets or sets the pattern that is repeated.
        /// </summary>
        public Element Pattern { get; set; }

        public Lookaround(Element pattern, bool behind)
        {
            Pattern = pattern;
            Behind = behind;
        }

        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append(Behind ? "behind(" : "ahead(");
            Pattern.GenerateString(builder);
            builder.Append(")");
        }
    }

}
