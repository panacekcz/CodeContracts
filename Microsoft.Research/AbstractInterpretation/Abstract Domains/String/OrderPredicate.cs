// CodeContracts
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

// Created by Vlastimil Dort (2016)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.DataStructures;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Helper class for order predicates.
    /// </summary>
    static class OrderPredicate
    {
        /// <summary>
        /// Creates an order predicate, which is satisfied if the value of leqVar is less than or equal to geqVar.
        /// </summary>
        /// <typeparam name="Variable">Type representing variables.</typeparam>
        /// <param name="leqVar">The variable that should be less than or equal to geqVar.</param>
        /// <param name="geqVar">The variable that should be greater than or equal to leqVar.</param>
        /// <returns></returns>
        public static OrderPredicate<Variable> For<Variable>(Variable leqVar, Variable geqVar)
          where Variable : IEquatable<Variable>
        {
            return new OrderPredicate<Variable>(leqVar, new SetOfConstraints<Variable>(geqVar));
        }
    }

    /// <summary>
    /// Represents a predicate which is satisfied if one variable is less than or equal to other variables.
    /// </summary>
    /// <typeparam name="Variable"></typeparam>
    public class OrderPredicate<Variable> : IStringPredicate
      where Variable : IEquatable<Variable>
    {
        private readonly Variable stringVariable;
        private readonly SetOfConstraints<Variable> geqVariables;

        internal OrderPredicate(Variable stringVariable, SetOfConstraints<Variable> geqVariables)
        {
            this.stringVariable = stringVariable;
            this.geqVariables = geqVariables;
        }

        public IAbstractDomain Bottom
        {
            get
            {
                return FlatPredicate.Bottom;
            }
        }

        /// <summary>
        /// Determines whether the predicate refers to the specified variable.
        /// </summary>
        /// <param name="v">The variable of interest.</param>
        /// <returns>True, if v is one of the varaibles in the predicate.</returns>
        public bool RefersToVariable(Variable v)
        {
            return stringVariable.Equals(v) || geqVariables.Values.Any(l => l.Equals(v));
        }

        /// <summary>
        /// Gets the variable that is less than or equal to <see cref="GreaterEqualVariables"/> if this predicate holds.
        /// </summary>
        public Variable LessEqualVariable
        {
            get { return stringVariable; }
        }
        /// <summary>
        /// Gets the variables that are greater than or equal to <see cref="LessEqualVariable"/> if this predicate holds.
        /// </summary>
        public SetOfConstraints<Variable> GreaterEqualVariables
        {
            get { return geqVariables; }
        }

        public bool IsBottom
        {
            get
            {
                return geqVariables.IsBottom;
            }
        }

        public bool IsTop
        {
            get
            {
                return geqVariables.IsTop;
            }
        }

        public ProofOutcome ProofOutcome
        {
            get
            {
                return ProofOutcome.Top;
            }
        }

        public IAbstractDomain Top
        {
            get
            {
                return FlatPredicate.Top;
            }
        }

        public object Clone()
        {
            return new OrderPredicate<Variable>(stringVariable, geqVariables);
        }

        public bool ContainsValue(bool value)
        {
            return true;
        }

        public IAbstractDomain Join(IAbstractDomain a)
        {
            if (a is OrderPredicate<Variable>)
            {
                var other = (OrderPredicate<Variable>)a;
                if (!stringVariable.Equals(other.stringVariable))
                    return Top;

                return new OrderPredicate<Variable>(stringVariable, geqVariables.Join(other.geqVariables));
            }
            else
            {
                return Top;
            }
        }

        public bool LessEqual(IAbstractDomain a)
        {
            if (a.IsTop)
            {
                return true;
            }
            else if (a is OrderPredicate<Variable>)
            {
                var other = (OrderPredicate<Variable>)a;
                return stringVariable.Equals(other.stringVariable) && geqVariables.LessEqual(other.geqVariables);
            }
            else
            {
                return false;
            }
        }

        public IAbstractDomain Meet(IAbstractDomain a)
        {
            if (a is OrderPredicate<Variable>)
            {
                var other = (OrderPredicate<Variable>)a;
                if (!stringVariable.Equals(other.stringVariable))
                    return Top;

                return new OrderPredicate<Variable>(stringVariable, geqVariables.Meet(other.geqVariables));
            }
            else
            {
                return Top;
            }
        }

        public T To<T>(IFactory<T> factory)
        {
            return factory.Constant(true);
        }

        public IAbstractDomain Widening(IAbstractDomain prev)
        {
            if (prev is OrderPredicate<Variable>)
            {
                var other = (OrderPredicate<Variable>)prev;
                if (!stringVariable.Equals(other.stringVariable))
                    return Top;

                return new OrderPredicate<Variable>(stringVariable, geqVariables.Widening(other.geqVariables));
            }
            else
            {
                return Top;
            }
        }

        public override string ToString()
        {
            return stringVariable.ToString() + "<=" + geqVariables.ToString();
        }

        public IStringPredicate AssignInParallel<Variable1>(Dictionary<Variable1, FList<Variable1>> sourcesToTargets)
        {
            FList<Variable1> list;
            if (sourcesToTargets.TryGetValue((Variable1)(object)stringVariable, out list) && !list.IsEmpty())
            {
                Set<Variable> newVars = new Set<Variable>();

                foreach (Variable v in geqVariables.Values)
                {
                    FList<Variable1> vlist;
                    if (sourcesToTargets.TryGetValue((Variable1)(object)v, out vlist))
                    {
                        newVars.AddRange(vlist.GetEnumerable().Select(ov => (Variable)(object)ov));
                    }
                }

                return new OrderPredicate<Variable>((Variable)(object)list.Head, new SetOfConstraints<Variable>(newVars, false));
            }
            else
            {
                return FlatPredicate.Top;
            }
        }
    }
}
