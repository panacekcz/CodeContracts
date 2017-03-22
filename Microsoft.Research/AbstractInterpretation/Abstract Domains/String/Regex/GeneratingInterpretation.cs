using Microsoft.Research.Regex;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Regex.Model;

namespace Microsoft.Research.AbstractDomains.Strings.Regex
{
    /// <summary>
    /// Abstract state for generating regex interpretations.
    /// </summary>
    /// <typeparam name="TState">Abstract state representing the generated language.</typeparam>
    internal struct GeneratingState<TState>
    {
        /// <summary>
        /// The abstract state representing a language of strings where a match exists.
        /// </summary>
        public TState Open { get; set; }
        /// <summary>
        /// The abstract state representing a language of strings where the match ends at
        /// the current position.
        /// </summary>
        public TState Closed { get; set; }

        public GeneratingState(TState open, TState closed)
        {
            Open = open;
            Closed = closed;
        }
        public GeneratingState(TState both)
        {
            Open = both;
            Closed = both;
        }

        public override string ToString()
        {
            return Open.ToString() + ";" + Closed.ToString();
        }
    }


    /// <summary>
    /// Interpretation of regex that generates a corresponding abstract element.
    /// </summary>
    /// <typeparam name="TState">Type of the abstract element.</typeparam>
    internal class GeneratingInterpretation<TState> : IRegexInterpretation<GeneratingState<TState>>
    {
        private readonly IGeneratingOperationsForRegex<TState> operations;

        public GeneratingInterpretation(IGeneratingOperationsForRegex<TState> operations)
        {
            this.operations = operations;
        }

        #region IRegexInterpretation<GeneratingState<TState>> implementation
        public GeneratingState<TState> Bottom
        {
            get
            {
                return new GeneratingState<TState>(operations.Bottom);
            }
        }

        public GeneratingState<TState> Top
        {
            get
            {
                return new GeneratingState<TState>(operations.Top);
            }
        }

        public GeneratingState<TState> AddChar(GeneratingState<TState> prev, CharRanges must, CharRanges can)
        {
            CharRanges ranges = operations.IsUnderapproximating ? must : can;
            return new GeneratingState<TState>(operations.AddChar(prev.Closed, ranges, false), operations.AddChar(prev.Closed, ranges, true));
        }

        public GeneratingState<TState> AssumeEnd(GeneratingState<TState> prev)
        {
            //TODO: VD: if underapprox, should check that nothing else follows
            return new GeneratingState<TState>(prev.Closed);
        }

        public GeneratingState<TState> AssumeStart(GeneratingState<TState> prev)
        {
            TState d = prev.Closed;

            if (operations.CanBeEmpty(d))
            {
                return new GeneratingState<TState>(operations.Top, operations.Empty);
            }
            else
            {
                return Bottom;
            }
        }

        public GeneratingState<TState> BeginLookaround(GeneratingState<TState> prev, bool behind)
        {
            return Bottom;
        }

        public GeneratingState<TState> BeginLoop(GeneratingState<TState> prev, IndexInt min, IndexInt max)
        {
            return new GeneratingState<TState>(operations.Top, operations.Empty);
        }

        public GeneratingState<TState> EndLookaround(GeneratingState<TState> prev, GeneratingState<TState> next, bool behind)
        {
            if (operations.IsUnderapproximating)
                return Bottom;
            else
                return prev;
        }

        public GeneratingState<TState> EndLoop(GeneratingState<TState> prev, GeneratingState<TState> next, IndexInt min, IndexInt max)
        {
            TState loopedOpen = operations.Loop(prev.Closed, next.Closed, next.Open, min, max);
            TState loopedClosed = operations.Loop(prev.Closed, next.Closed, next.Closed, min, max);

            if (min == 0 || operations.CanBeEmpty(next.Closed))
            {
                loopedOpen = operations.Join(prev.Open, loopedOpen, false);
            }

            return new GeneratingState<TState>(loopedOpen, loopedClosed);
            //TODO: VD: check code above
            //throw new NotImplementedException();
        }

        public GeneratingState<TState> Join(GeneratingState<TState> left, GeneratingState<TState> right, bool widen)
        {
            return new GeneratingState<TState>(operations.Join(left.Open, right.Open, widen), operations.Join(left.Closed, right.Closed, widen));
        }

        public GeneratingState<TState> Unknown(GeneratingState<TState> data)
        {
            return operations.IsUnderapproximating ? Bottom : data;
        }
        #endregion
    }
}
