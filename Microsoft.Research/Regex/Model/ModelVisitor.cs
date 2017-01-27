using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex.Model
{
    public abstract class ModelVisitor<Result, Data>
    {
        protected Result VisitElement(Element e, ref Data data)
        {
            if (e is Anchor)
                return VisitAnchor((Anchor)e, ref data);
            else if (e is Concatenation)
                return VisitConcatenation((Concatenation)e, ref data);
            else if (e is Union)
                return VisitUnion((Union)e, ref data);
            else if (e is Character)
                return VisitCharacter((Character)e, ref data);
            else if (e is Unknown)
                return VisitUnknown((Unknown)e, ref data);
            else if (e is Loop)
                return VisitLoop((Loop)e, ref data);
            else
                throw new InvalidOperationException();
        }

        protected abstract Result VisitAnchor(Anchor a, ref Data data);
        protected abstract Result VisitConcatenation(Concatenation a, ref Data data);
        protected abstract Result VisitUnion(Union a, ref Data data);
        protected abstract Result VisitCharacter(Character a, ref Data data);
        protected abstract Result VisitUnknown(Unknown a, ref Data data);
        protected abstract Result VisitLoop(Loop a, ref Data data);

    }
}
