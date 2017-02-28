using Microsoft.Research.Regex.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.Regex
{
    class BackwardRegexInterpreter<D> : RegexInterpreter<D>
    {
        public BackwardRegexInterpreter(IRegexInterpretation<D> operations) :
            base(operations)
        {
        }

        protected override Void VisitConcatenation(Concatenation element, ref D data)
        {
            foreach (var part in Enumerable.Reverse(element.Parts))
            {
                VisitElement(part, ref data);
            }
            return null;
        }

        protected override Void VisitAnchor(Begin element, ref D data)
        {
            data = operations.AssumeEnd(data);
            return null;
        }
        protected override Void VisitAnchor(End element, ref D data)
        {
            data = operations.AssumeStart(data);
            return null;
        }

    }
}
