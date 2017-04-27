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
    public abstract class Quantifier : Element
    {
        
        private readonly bool lazy;
        private readonly Element content;

        public Element Content { get { return content; } }
        public bool Lazy { get { return lazy; } }
        public abstract int Min { get; }
        public abstract int Max { get; }
        public abstract bool IsUnbounded { get; }

        protected Quantifier(Element content, bool lazy)
        {
            this.content = content;
            this.lazy = lazy;
        }

        internal abstract void GenerateQuantifier(StringBuilder builder);
        internal override void GenerateString(StringBuilder builder)
        {
            Content.GenerateString(builder);
            GenerateQuantifier(builder);

            if (Lazy)
            {
                builder.Append('?');
            }
        }
    }

    public class Iteration : Quantifier
    {
        public override int Max
        {
            get
            {
                return -1;
            }
        }
        public override int Min
        {
            get
            {
                return 0;
            }
        }

        public Iteration(Element content, bool lazy):
            base(content, lazy)
        {
        }

        public override bool IsUnbounded { get { return true; } }
        internal override void GenerateQuantifier(StringBuilder builder)
        {
            builder.Append('*');
        }
    }

    public class PositiveIteration : Quantifier
    {
        public override int Max
        {
            get
            {
                return -1;
            }
        }
        public override int Min
        {
            get
            {
                return 1;
            }
        }

        public PositiveIteration(Element content, bool lazy) :
            base(content, lazy)
        {
        }

        public override bool IsUnbounded { get { return true; } }
        internal override void GenerateQuantifier(StringBuilder builder)
        {
            builder.Append('+');
        }
    }

    public class Optional : Quantifier
    {
        public override int Max
        {
            get
            {
                return 1;
            }
        }
        public override int Min
        {
            get
            {
                return 0;
            }
        }

        public Optional(Element content, bool lazy) :
            base(content, lazy)
        {
        }

        public override bool IsUnbounded { get { return false; } }
        internal override void GenerateQuantifier(StringBuilder builder)
        {
            builder.Append('?');
        }
    }


    public class Loop : Quantifier
    {
        internal const int UNBOUNDED = -1;
        private readonly int min, max;

        public override int Min { get { return min; } }
        public override int Max { get { return max; } }
        public override bool IsUnbounded { get { return max == UNBOUNDED; } }
        
        public Loop(int min, int max, Element content, bool lazy):
            base(content, lazy)
        {
            this.min = min;
            this.max = max;
        }

        internal override void GenerateQuantifier(StringBuilder builder)
        {
            builder.Append('{');
            builder.Append(min);
            if (min != max)
            {
                builder.Append(',');
                if (max != -1)
                {
                    builder.Append(max);
                }
            }
            builder.Append('}');
        }

    }
}
