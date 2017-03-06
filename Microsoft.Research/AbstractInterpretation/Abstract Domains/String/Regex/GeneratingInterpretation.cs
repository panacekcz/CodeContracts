using Microsoft.Research.Regex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Regex.Model;

namespace Microsoft.Research.AbstractDomains.Strings.Regex
{
    internal struct GeneratingState<D>
    {
        public D Open { get; set; }
        public D Closed { get; set; }

        public GeneratingState(D open, D closed)
        {
            Open = open;
            Closed = closed;
        }
        public GeneratingState(D both)
        {
            Open = both;
            Closed = both;
        }
    }


    /// <summary>
    /// Interpretation of regex that generates a corresponding abstract element.
    /// </summary>
    /// <typeparam name="D">Typef of the abstract element.</typeparam>
    internal class GeneratingInterpretation<D> : IRegexInterpretation<GeneratingState<D>>
    {
        private readonly IGeneratingOperationsForRegex<D> operations;

        public GeneratingInterpretation(IGeneratingOperationsForRegex<D> operations)
        {
            this.operations = operations;
        }

        public GeneratingState<D> Bottom
        {
            get
            {
                return new GeneratingState<D>(operations.Bottom);
            }
        }

        public GeneratingState<D> Top
        {
            get
            {
                return new GeneratingState<D>(operations.Top);
            }
        }

        public GeneratingState<D> AddChar(GeneratingState<D> prev, CharRanges must, CharRanges can)
        {
            CharRanges ranges = operations.IsUnderapproximating ? must : can;
            return new GeneratingState<D>(operations.AddChar(prev.Closed, ranges, false), operations.AddChar(prev.Closed, ranges, true));
        }

        public GeneratingState<D> AssumeEnd(GeneratingState<D> prev)
        {
            return new GeneratingState<D>(prev.Closed);
        }

        public GeneratingState<D> AssumeStart(GeneratingState<D> prev)
        {
            D d = prev.Closed;

            if (operations.CanBeEmpty(d))
            {
                return new GeneratingState<D>(operations.Top, operations.Empty);
            }
            else
            {
                D bot = operations.Bottom;
                return new GeneratingState<D>(bot, bot);
            }
        }

        public GeneratingState<D> BeginLoop(GeneratingState<D> prev, IndexInt min, IndexInt max)
        {
            return new GeneratingState<D>(operations.Top, operations.Empty);
            //throw new NotImplementedException();
        }

        public GeneratingState<D> EndLoop(GeneratingState<D> prev, GeneratingState<D> next, IndexInt min, IndexInt max)
        {
            D loopedOpen = operations.Loop(prev.Closed, next.Closed, next.Open, min, max);
            D loopedClosed = operations.Loop(prev.Closed, next.Closed, next.Closed, min, max);

            if (min == 0 || operations.CanBeEmpty(next.Closed))
            {
                loopedOpen = operations.Join(prev.Open, loopedOpen, false);
            }

            return new GeneratingState<D>(loopedOpen, loopedClosed);
            //throw new NotImplementedException();
        }

        public GeneratingState<D> Join(GeneratingState<D> left, GeneratingState<D> right, bool widen)
        {
            return new GeneratingState<D>(operations.Join(left.Open, right.Open, widen), operations.Join(left.Closed, right.Closed, widen));
        }

        public GeneratingState<D> Unknown(GeneratingState<D> data)
        {
            return operations.IsUnderapproximating ? Bottom : data;
        }
    }
}
