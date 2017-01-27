using Microsoft.Research.Regex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Regex.Model;

namespace Microsoft.Research.AbstractDomains.Strings.String.Regex
{

    interface IStringOperationsForRegex<D> 
    {
        D AppendChar(D prev, CharRanges next, bool under);

        D Top { get; }
        D Bottom { get; }
        D Join(D left, D right, bool under, bool widen);

        D AssumeStart(D prev, bool under);
        D AssumeEnd(D prev, bool under);
    }

    interface IStringOperationsForRegex2<D> : IStringOperationsForRegex<D>
    {
        D Empty(bool under);
        D AppendCharTop(D prev, CharRanges next, bool under);
    }

    struct RegexData2<D>
    {
        public D Open { get; set; }
        public D Closed { get; set; }

        public RegexData2(D open, D closed)
        {
            Open = open;
            Closed = closed;
        }
        public RegexData2(D both)
        {
            Open = both;
            Closed = both;
        }
    }

    class StringOperationsForRegex2<D> : IStringOperationsForRegex<RegexData2<D>>
    {
        private readonly IStringOperationsForRegex2<D> operations2;

        public StringOperationsForRegex2(IStringOperationsForRegex2<D> operations)
        {
            this.operations2 = operations;
        }

        public RegexData2<D> Bottom
        {
            get
            {
                return new RegexData2<D>(operations2.Bottom);
            }
        }

        public RegexData2<D> Top
        {
            get
            {
                return new RegexData2<D>(operations2.Top);
            }
        }

        public RegexData2<D> AppendChar(RegexData2<D> prev, CharRanges next, bool under)
        {
            return new RegexData2<D>(operations2.AppendCharTop(prev.Closed, next, under), operations2.AppendChar(prev.Closed, next, under));
        }

        public RegexData2<D> AssumeEnd(RegexData2<D> prev, bool under)
        {
            return new RegexData2<D>(prev.Closed);
        }

        public RegexData2<D> AssumeStart(RegexData2<D> prev, bool under)
        {
            return new RegexData2<D>(operations2.Top, operations2.Empty(under));
        }

        public RegexData2<D> Join(RegexData2<D> left, RegexData2<D> right, bool under, bool widen)
        {
            return new RegexData2<D>(operations2.Join(left.Open, right.Open, under, widen), operations2.Join(left.Closed, right.Closed, under, widen));
        }
    }

    class RegexInterpreter<D> : ModelVisitor<D, D>
    {
        IStringOperationsForRegex<D> operations;
        bool underapproximate;

        protected override D VisitUnknown(Unknown regex, ref D data)
        {
            if (underapproximate)
                return data = operations.Bottom;
            else
                return data = VisitElement(regex.Pattern, ref data);
        }

        protected override D VisitCharacter(Character element, ref D data)
        {
            return data = operations.AppendChar(data, underapproximate ? element.MustMatch : element.CanMatch, underapproximate);
        }

        protected override D VisitLoop(Loop element, ref D data)
        {
            throw new NotImplementedException();
        }

        protected override D VisitConcatenation(Concatenation element, ref D data)
        {
            foreach(var part in element.Parts)
            {
                VisitElement(element, ref data);
            }
            return data;
        }

        protected override D VisitAnchor(Anchor element, ref D data)
        {
            if(element.Kind == AnchorKind.Start)
            {
                return data = operations.AssumeStart(data, underapproximate);
            }
            else if(element.Kind == AnchorKind.End)
            {
                return data = operations.AssumeEnd(data, underapproximate);
            }
            else
            {
                throw new InvalidOperationException();
            }
        }

        protected override D VisitUnion(Union element, ref D data)
        {
            D joined = operations.Bottom;
            foreach (var part in element.Patterns)
            {
                D next = data;
                VisitElement(element, ref next);
                joined = operations.Join(joined, next, underapproximate, false);
            }
            return data;
        }
    }
}
