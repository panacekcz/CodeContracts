﻿// CodeContracts
// 
// Copyright (c) Microsoft Corporation
// 
// All rights reserved. 
// 
// MIT License
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// Created by Vlastimil Dort (2015-2016)
// Master thesis String Analysis for Code Contracts

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Research.Regex.Model;
using Microsoft.Research.Regex;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.AbstractDomains.Strings.Regex;

namespace Microsoft.Research.AbstractDomains.Strings
{

    /// <summary>
    /// Provides regex-related functionality to <see cref="Bricks"/>.
    /// </summary>
    public class BricksRegex
    {
        private readonly Bricks element;

        public BricksRegex(Bricks element)
        {
            this.element = element;
        }

        private class BrickGeneratingState
        {
            internal Brick brick;
            internal BrickGeneratingState previous;
            internal int index;

            public BrickGeneratingState(Brick b, BrickGeneratingState previous = null)
            {
                brick = b;
                this.previous = previous;
                index = previous == null ? 0 : previous.index + 1;
            }

            public IEnumerable<Brick> Bricks
            {
                get
                {
                    BrickGeneratingState bs = this;
                    while (bs != null)
                    {
                        yield return bs.brick;
                        bs = bs.previous;
                    }
                }
            }

            internal static BrickGeneratingState Concat(BrickGeneratingState prev, BrickGeneratingState last)
            {
                List<Brick> lastList = last.Bricks.ToList();
                foreach(Brick brick in lastList)
                {
                    prev = new BrickGeneratingState(brick, prev);
                }
                return prev;
            }

            internal bool TryAppend(string c, out BrickGeneratingState next)
            {
                if(brick.values != null && brick.values.Count == 1 && brick.min == 1 && brick.max == 1)
                {
                    Brick newBrick = new Brick(brick.values.First() + c);
                    next = new BrickGeneratingState(newBrick, previous);
                    return true;
                }
                else if(brick.max == 0)
                {
                    Brick newBrick = new Brick(c);
                    next = new BrickGeneratingState(newBrick, previous);
                    return true;
                }
                else
                {
                    next = null;
                    return false;
                }
            }

            internal List<Brick> ToBrickList()
            {
                List<Brick> bricks = Bricks.ToList();
                bricks.Reverse();
                return bricks;
            }

            internal BrickGeneratingState NotEmpty()
            {
                if (previous == null && brick.max == 0)
                    return null;
                else
                    return this;
            }
        }
        
        private class BricksGeneratingOperations : IGeneratingOperationsForRegex<BrickGeneratingState>
        {
            private IBricksPolicy bricksPolicy;
            private readonly bool underapproximate;

            public BricksGeneratingOperations(IBricksPolicy bricksPolicy, bool underapproximate)
            {
                this.bricksPolicy = bricksPolicy;
                this.underapproximate = underapproximate;
            }

            public bool IsUnderapproximating
            {
                get { return underapproximate; }
            }
    
            private BrickGeneratingState CutToLength(BrickGeneratingState longer, BrickGeneratingState shorter, List<Brick> removed)
            {
                while (longer.index > shorter.index)
                {
                    removed.Add(longer.brick);
                    longer = longer.previous;
                }
                return longer;
            }

            private BrickGeneratingState CommonStatePrefix(BrickGeneratingState prev, BrickGeneratingState next, List<Brick> prevChange, List<Brick> nextChange)
            {
                prev = CutToLength(prev, next, prevChange);
                next = CutToLength(next, prev, nextChange);

                while (prev != next)
                {
                    prevChange.Add(prev.brick);
                    nextChange.Add(next.brick);

                    prev = prev.previous;
                    next = next.previous;
                }

                return prev;
            }

            private static bool IsListSetOfConstants(List<Brick> bricks)
            {
                return bricks.Count == 1 && bricks[0].min == 1 && bricks[0].max == 1;
            }

            public BrickGeneratingState Join(BrickGeneratingState prev, BrickGeneratingState next, bool widen)
            {
                List<Brick> prevChange = new List<Brick>();
                List<Brick> nextChange = new List<Brick>();
                
                BrickGeneratingState common = CommonStatePrefix(prev, next, prevChange, nextChange);

                if (IsListSetOfConstants(prevChange) && IsListSetOfConstants(nextChange))
                {
                    return new BrickGeneratingState(prevChange[0].Join(nextChange[0]), common);
                }
                else if (underapproximate)
                {
                    // If we are underapproximating, do not allow joining lists
                    return Bottom;
                }
                else
                {
                    prevChange.Reverse();
                    nextChange.Reverse();

                    Bricks joined = new Bricks(prevChange, bricksPolicy).Join(new Bricks(nextChange, bricksPolicy));

                    foreach (Brick b in joined.bricks)
                        common = new BrickGeneratingState(b, common);
                    return common;
                }

            }

            public BrickGeneratingState Empty
            {
                get
                {
                    return new BrickGeneratingState(new Brick(""));
                }
            }
            public BrickGeneratingState Top
            {
                get
                {
                    return new BrickGeneratingState(new Brick(true));
                }
            }
            public BrickGeneratingState Bottom
            {
                get
                {
                    return new BrickGeneratingState(new Brick(false));
                }
            }



            public bool CanBeEmpty(BrickGeneratingState state)
            {
                return state.Bricks.All(b => b.CanBeEmpty);
            }

            public BrickGeneratingState Loop(BrickGeneratingState prev, BrickGeneratingState loop, BrickGeneratingState last, IndexInt min, IndexInt max)
            {
                if (min == 1 && max == 1)
                {
                    // Single occurence, append last to prev
                    return BrickGeneratingState.Concat(prev.NotEmpty(), last);
                }

                if (last.previous == null && last.brick.min == 1 && last.brick.max == 1)
                {
                    //A brick has single occurence, can apply the loop bounds
                    Brick loopedBrick = new Brick(last.brick.values, min, max);
                    return new BrickGeneratingState(loopedBrick, prev.NotEmpty());
                }
                else
                {
                    // Cannot represent the loop
                    return new BrickGeneratingState(new Brick(!underapproximate));
                }
            }

            public BrickGeneratingState AddChar(BrickGeneratingState prev, CharRanges next, bool closed)
            {
                HashSet<string> chars = new HashSet<string>();

                foreach (var range in next.Ranges)
                {
                    foreach (char character in range)
                    {
                        chars.Add(character.ToString());
                    }
                }

                BrickGeneratingState r;
                if (chars.Count == 1 && prev.TryAppend(chars.First(), out r))
                {
                 
                }
                else
                {
                    r = new BrickGeneratingState(new Brick(chars), prev.NotEmpty());
                }

                if (!closed)
                {
                    r = new BrickGeneratingState(new Brick(true), r);
                }

                return r;
            }
        }

        private ForwardRegexInterpreter<GeneratingState<BrickGeneratingState>> CreateInterpreter(bool underapproximate)
        {
            BricksGeneratingOperations operations = new BricksGeneratingOperations(element.Policy, underapproximate);
            GeneratingInterpretation<BrickGeneratingState> interpretation = new GeneratingInterpretation<BrickGeneratingState>(operations);
            ForwardRegexInterpreter<GeneratingState<BrickGeneratingState>> interpreter = new ForwardRegexInterpreter<GeneratingState<BrickGeneratingState>>(interpretation);
            return interpreter;
        }

        private Bricks BricksForRegex(Element regex, bool underapproximate)
        {
            var interpreter = CreateInterpreter(underapproximate);
            var result = interpreter.Interpret(regex);

            return new Bricks(result.Open.ToBrickList(), element.Policy);
        }

        /// <summary>
        /// Constructs a Bricks abstract element that overapproximates
        /// the specified regex.
        /// </summary>
        /// <param name="regex">A regex AST.</param>
        /// <returns>An abstract element overapproximating <paramref name="regex"/>.</returns>
        public Bricks BricksForRegex(Element regex)
        {
            return BricksForRegex(regex, false);
        }


        /// <summary>
        /// Verifies whether the bricks match the specified regex expression.
        /// </summary>
        /// <param name="regex">AST of the regex.</param>
        /// <returns>Proven result of the match.</returns>
        public ProofOutcome IsMatch(Element regex)
        {
            Bricks overapproximation = BricksForRegex(regex);
            Bricks canMatchBricks = element.Meet(overapproximation);
            bool canMatch = !canMatchBricks.IsBottom;

            Bricks underapproximation = BricksForRegex(regex, true);
            bool mustMatch = element.LessThanEqual(underapproximation);

            return ProofOutcomeUtils.Build(canMatch, !mustMatch);
        }

    }

}
