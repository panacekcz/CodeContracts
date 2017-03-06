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
    /// <typeparam name="D">Interpreter state</typeparam>
    abstract class RegexInterpreter<D> : ModelVisitor<Void, D>
    {
        protected readonly IRegexInterpretation<D> operations;

        public RegexInterpreter(IRegexInterpretation<D> operations)
        {
            this.operations = operations;
        }

        public D Interpret(Element model)
        {
            D data = operations.Top;
            VisitElement(model, ref data);
            return data;
        }

        protected override Void VisitUnknown(Unknown regex, ref D data)
        {
            VisitElement(regex.Pattern, ref data);

            data = operations.Unknown(data);
            return null;
        }

        protected override Void VisitCharacter(Character element, ref D data)
        {
            data = operations.AddChar(data, element.MustMatch, element.CanMatch);
            return null;
        }

        private IndexInt LoopBoundIndexInt(int loopBound)
        {
            if (loopBound == Loop.Unbounded)
                return IndexInt.Infinity;
            else
                return IndexInt.ForNonNegative(loopBound);
        }

        protected override Void VisitLoop(Loop element, ref D data)
        {
            D next = data;
            next = operations.BeginLoop(data, LoopBoundIndexInt(element.Min), LoopBoundIndexInt(element.Max));
            VisitElement(element.Pattern, ref next);
            data = operations.EndLoop(data, next, LoopBoundIndexInt(element.Min), LoopBoundIndexInt(element.Max));
            return null;
        }

        protected override Void VisitUnion(Union element, ref D data)
        {
            D joined = operations.Bottom;
            foreach (var part in element.Patterns)
            {
                D next = data;
                VisitElement(part, ref next);
                joined = operations.Join(joined, next, false);
            }
            data = joined;
            return null;
        }
    }
}
