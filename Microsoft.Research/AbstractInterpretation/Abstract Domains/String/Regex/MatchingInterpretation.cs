using Microsoft.Research.Regex.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Regex;

namespace Microsoft.Research.AbstractDomains.Strings.Regex
{
    internal struct MatchingState<D>
    {
        private readonly D over, under;

        public D Over { get { return over; } }
        public D Under { get { return under; } }

        public MatchingState(D o, D u)
        {
            over = o;
            under = u;
        }
    }

    internal class MatchingInterpretation<D, SD> : IRegexInterpretation<MatchingState<D>>
    {
        private readonly IMatchingOperationsForRegex<D, SD> operations;
        private readonly SD input;

        public MatchingState<D> Bottom
        {
            get
            {
                D bot = operations.GetBottom(input);
                return new MatchingState<D>(bot, bot);
            }
        }

        public MatchingState<D> Top
        {
            get
            {
                D top = operations.GetTop(input);
                return new MatchingState<D>(top, top);
            }
        }

        public MatchingInterpretation(IMatchingOperationsForRegex<D, SD> operations, SD input)
        {
            this.operations = operations;
            this.input = input;
        }

        public MatchingState<D> AddChar(MatchingState<D> data, CharRanges must, CharRanges can)
        {
            return new MatchingState<D>(operations.MatchChar(input, data.Over, can, false), operations.MatchChar(input, data.Under, must, true));
        }

        public MatchingState<D> AssumeEnd(MatchingState<D> data)
        {
            return new MatchingState<D>(operations.AssumeEnd(input, data.Over, false), operations.AssumeEnd(input, data.Under, true));
        }

        public MatchingState<D> AssumeStart(MatchingState<D> data)
        {
            return new MatchingState<D>(operations.AssumeStart(input, data.Over, false), operations.AssumeStart(input, data.Under, true));
        }

        public MatchingState<D> Join(MatchingState<D> prev, MatchingState<D> next, bool widen)
        {
            return new MatchingState<D>(operations.Join(input, prev.Over, next.Over, false, widen), operations.Join(input, prev.Under, next.Under, true, widen));
        }

        public MatchingState<D> Unknown(MatchingState<D> data)
        {
            return new MatchingState<D>(data.Over, operations.GetBottom(input));
        }

        public MatchingState<D> BeginLoop(MatchingState<D> prev, IndexInt min, IndexInt max)
        {
            return new MatchingState<D>(operations.BeginLoop(input, prev.Over, false), operations.BeginLoop(input, prev.Under, true));
        }

        public MatchingState<D> EndLoop(MatchingState<D> prev, MatchingState<D> next, IndexInt min, IndexInt max)
        {
            return new MatchingState<D>(operations.EndLoop(input, prev.Over, next.Over, min, max, false), operations.EndLoop(input, prev.Under, next.Under, min, max, true));
        }
    }

    

}
