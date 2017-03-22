using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{

    /// <summary>
    /// Visis regular expression models.
    /// </summary>
    /// <typeparam name="Data">Data passed along traversing individual elements.</typeparam>
    /// <typeparam name="Result">Data returned from each elements.</typeparam>
    public abstract class ModelVisitor<Result, Data>
    {
        /// <summary>
        /// Visits an element by calling the appropriate Visit method for the actual type of
        /// the element.
        /// </summary>
        /// <param name="element">Element of the regex model.</param>
        /// <param name="data">Data passed to the Visit method</param>
        /// <returns>Data returned from the Visit method.</returns>
        protected Result VisitElement(Element element, ref Data data)
        {
            if (element is Begin)
                return VisitAnchor((Begin)element, ref data);
            else if (element is End)
                return VisitAnchor((End)element, ref data);
            else if (element is Concatenation)
                return VisitConcatenation((Concatenation)element, ref data);
            else if (element is Union)
                return VisitUnion((Union)element, ref data);
            else if (element is Character)
                return VisitCharacter((Character)element, ref data);
            else if (element is Unknown)
                return VisitUnknown((Unknown)element, ref data);
            else if (element is Loop)
                return VisitLoop((Loop)element, ref data);
            else if (element is Lookaround)
                return VisitLookaround((Lookaround)element, ref data);
            else
                throw new InvalidOperationException();
        }
        /// <summary>
        /// Visits a begin anchor.
        /// </summary>
        /// <param name="anchor">The begin anchor element in the regex model.</param>
        /// <param name="data">Data passed from the caller.</param>
        /// <returns>Result returned to the caller.</returns>
        protected abstract Result VisitAnchor(Begin anchor, ref Data data);
        /// <summary>
        /// Visits a end anchor.
        /// </summary>
        /// <param name="anchor">The end anchor element in the regex model.</param>
        /// <param name="data">Data passed from the caller.</param>
        /// <returns>Result returned to the caller.</returns>
        protected abstract Result VisitAnchor(End anchor, ref Data data);
        /// <summary>
        /// Visits a concatenation element.
        /// </summary>
        /// <param name="concatenation">The concatenation element in the regex model.</param>
        /// <param name="data">Data passed from the caller.</param>
        /// <returns>Result returned to the caller.</returns>
        protected abstract Result VisitConcatenation(Concatenation concatenation, ref Data data);
        /// <summary>
        /// Visits a union element.
        /// </summary>
        /// <param name="union">The union element in the regex model.</param>
        /// <param name="data">Data passed from the caller.</param>
        /// <returns>Result returned to the caller.</returns>
        protected abstract Result VisitUnion(Union union, ref Data data);
        /// <summary>
        /// Visits a character element.
        /// </summary>
        /// <param name="character">The character element in the regex model.</param>
        /// <param name="data">Data passed from the caller.</param>
        /// <returns>Result returned to the caller.</returns>
        protected abstract Result VisitCharacter(Character character, ref Data data);
        /// <summary>
        /// Visits an unknown group element.
        /// </summary>
        /// <param name="unknown">The unknown group element in the regex model.</param>
        /// <param name="data">Data passed from the caller.</param>
        /// <returns>Result returned to the caller.</returns>
        protected abstract Result VisitUnknown(Unknown unknown, ref Data data);
        /// <summary>
        /// Visits a loop model element.
        /// </summary>
        /// <param name="loop">The loop element in the regex model.</param>
        /// <param name="data">Data passed from the caller.</param>
        /// <returns>Result returned to the caller.</returns>
        protected abstract Result VisitLoop(Loop loop, ref Data data);
        /// <summary>
        /// Visits a positive lookaround model element.
        /// </summary>
        /// <param name="lookaround">The lookaround element in the regex model.</param>
        /// <param name="data">Data passed from the caller.</param>
        /// <returns>Result returned to the caller.</returns>
        protected abstract Result VisitLookaround(Lookaround lookaround, ref Data data);

    }
}
