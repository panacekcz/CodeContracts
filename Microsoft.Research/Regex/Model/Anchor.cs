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
    /// Represents an anchor in a regex.
    /// </summary>
    public abstract class Anchor : Element
    {
        /// <summary>
        /// Instance representing the begin anchor.
        /// </summary>
        public static readonly Begin Begin = new Begin();
        /// <summary>
        /// Instance representing the end anchor.
        /// </summary>
        public static readonly End End = new End();
    }
    /// <summary>
    /// Represents a begin anchor in a regex.
    /// </summary>
    public class Begin : Anchor {
        internal Begin() { }
        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append("begin()");
        }
    }
    /// <summary>
    /// Represents an end anchor in a regex.
    /// </summary>
    public class End : Anchor {
        internal End() { }
        internal override void GenerateString(StringBuilder builder)
        {
            builder.Append("end()");
        }
    }
}
