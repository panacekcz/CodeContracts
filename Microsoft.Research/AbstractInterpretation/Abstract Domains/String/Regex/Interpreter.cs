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
