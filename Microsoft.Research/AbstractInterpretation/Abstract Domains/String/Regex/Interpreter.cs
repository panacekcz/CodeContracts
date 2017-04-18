// CodeContracts
// 
// Copyright 2016-2017 Charles University
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

// Created by Vlastimil Dort (2016-2017)

using Microsoft.Research.Regex.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Regex
{
    /// <summary>
    /// Interprets a regex model.
    /// </summary>
    /// <typeparam name="TState">Interpreter state</typeparam>
    internal abstract class RegexInterpreter<TState> : ModelVisitor<Void, TState>
    {
        protected readonly IRegexInterpretation<TState> operations;

        /// <summary>
        /// Creates an interpreter using specified interpretation operations.
        /// </summary>
        /// <param name="operations">Interpretation operations.</param>
        public RegexInterpreter(IRegexInterpretation<TState> operations)
        {
            this.operations = operations;
        }

        /// <summary>
        /// Interprets a regex model.
        /// </summary>
        /// <param name="model">The regex model to be interpreted.</param>
        /// <returns>The final state of the interpretation.</returns>
        public TState Interpret(Element model)
        {
            TState data = operations.Top;
            VisitElement(model, ref data);
            return data;
        }


        private IndexInt LoopBoundIndexInt(int loopBound)
        {
            if (loopBound == Loop.Unbounded)
                return IndexInt.Infinity;
            else
                return IndexInt.ForNonNegative(loopBound);
        }

        #region ModelVisitor<Void, D> overrides

        protected override Void VisitUnknown(Unknown regex, ref TState data)
        {
            VisitElement(regex.Pattern, ref data);

            data = operations.Unknown(data);
            return null;
        }

        protected override Void VisitCharacter(Character element, ref TState data)
        {
            data = operations.AddChar(data, element.MustMatch, element.CanMatch);
            return null;
        }


        protected override Void VisitLoop(Loop element, ref TState data)
        {
            TState next = data;
            next = operations.BeginLoop(data, LoopBoundIndexInt(element.Min), LoopBoundIndexInt(element.Max));
            VisitElement(element.Pattern, ref next);
            data = operations.EndLoop(data, next, LoopBoundIndexInt(element.Min), LoopBoundIndexInt(element.Max));
            return null;
        }

        protected override Void VisitUnion(Union element, ref TState data)
        {
            TState joined = operations.Bottom;
            foreach (var part in element.Patterns)
            {
                TState next = data;
                VisitElement(part, ref next);
                joined = operations.Join(joined, next, false);
            }
            data = joined;
            return null;
        }
        #endregion
    }
}
