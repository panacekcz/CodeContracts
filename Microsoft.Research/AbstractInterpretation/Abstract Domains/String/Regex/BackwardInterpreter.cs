using Microsoft.Research.Regex.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Regex
{
    /// <summary>
    /// Interprets regular expressions in a backward direction.
    /// </summary>
    /// <typeparam name="TState">Interpreter data (abstract state).</typeparam>
    internal class BackwardRegexInterpreter<TState> : RegexInterpreter<TState>
    {
        public BackwardRegexInterpreter(IRegexInterpretation<TState> operations) :
            base(operations)
        {
        }

        #region ModelVisitor<Void, TState> overrides
        protected override Void VisitConcatenation(Concatenation element, ref TState data)
        {
            // Visit the parts in reverse order
            foreach (var part in Enumerable.Reverse(element.Parts))
            {
                VisitElement(part, ref data);
            }
            return null;
        }

        protected override Void VisitAnchor(Begin element, ref TState data)
        {
            data = operations.AssumeEnd(data);
            return null;
        }
        protected override Void VisitAnchor(End element, ref TState data)
        {
            data = operations.AssumeStart(data);
            return null;
        }
        protected override Void VisitLookaround(Lookaround lookaround, ref TState data)
        {
            TState nextData = operations.BeginLookaround(data, !lookaround.Behind);
            VisitElement(lookaround.Pattern, ref nextData);
            data = operations.EndLookaround(data, nextData, !lookaround.Behind);
            return null;
        }
        #endregion
    }
}
