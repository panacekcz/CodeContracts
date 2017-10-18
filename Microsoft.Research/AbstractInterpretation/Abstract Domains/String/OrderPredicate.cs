// CodeContracts
// 
// Copyright (c) Microsoft Corporation
// Copyright (c) Charles University
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
          where Variable : class, IEquatable<Variable>
        {
            return new OrderPredicate<Variable>(leqVar, new SetOfConstraints<Variable>(geqVar), true, true);
        }
    }

    /// <summary>
    /// Represents a predicate which is satisfied if one variable is less than or equal to other variables.
    /// </summary>
    /// <typeparam name="Variable"></typeparam>
    public class OrderPredicate<Variable> : PredicateBase
      where Variable : class, IEquatable<Variable>
    {
        private readonly Variable stringVariable;
        private readonly SetOfConstraints<Variable> geqVariables;

        internal OrderPredicate(Variable stringVariable, SetOfConstraints<Variable> geqVariables, bool canBeTrue, bool canBeFalse)
            : base(canBeTrue, canBeFalse)
        {
            this.stringVariable = stringVariable;
            this.geqVariables = geqVariables;
        }

        /// <summary>
        /// Determines whether the predicate refers to the specified variable.
        /// </summary>
        /// <param name="v">The variable of interest.</param>
        /// <returns>True, if v is one of the varaibles in the predicate.</returns>
        public override bool RefersToVariable<ArgVariable>(ArgVariable v)
        {
            if (v is Variable)
            {
                Variable vv = v as Variable;
                return stringVariable.Equals(v) || geqVariables.Values.Any(l => l.Equals(v));
            }

            return false;
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

        public override bool IsBottom
        {
            get
            {
                return base.IsBottom || geqVariables.IsBottom;
            }
        }

        public override bool IsTop
        {
            get
            {
                return base.IsTop && geqVariables.IsTop;
            }
        }


        public override object Clone()
        {
            return new OrderPredicate<Variable>(stringVariable, geqVariables, canBeTrue, canBeFalse);
        }


        public override IAbstractDomain Join(IAbstractDomain a)
        {
            if (a is OrderPredicate<Variable>)
            {
                var other = (OrderPredicate<Variable>)a;
                if (stringVariable.Equals(other.stringVariable)) { 
                    return new OrderPredicate<Variable>(
                        stringVariable,
                        geqVariables.Join(other.geqVariables),
                        canBeTrue || other.canBeTrue,
                        canBeFalse || other.canBeFalse
                        );
                }
            }
            return base.Join(a);
        }

        public override bool LessEqual(IAbstractDomain a)
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
                return base.LessEqual(a);
            }
        }

        public override IAbstractDomain Meet(IAbstractDomain a)
        {
            if (a is OrderPredicate<Variable>)
            {
                var other = (OrderPredicate<Variable>)a;
                if (stringVariable.Equals(other.stringVariable))
                {
                    return new OrderPredicate<Variable>(stringVariable, geqVariables.Meet(other.geqVariables), canBeTrue & other.canBeTrue, canBeFalse & other.canBeFalse);
                }
            }

            if (a is IStringPredicate)
            {
                bool aCanBeFalse = ((IStringPredicate)a).ContainsValue(false);
                bool aCanBeTrue = ((IStringPredicate)a).ContainsValue(true);

                if (!aCanBeFalse || !aCanBeTrue)
                {
                    return new OrderPredicate<Variable>(stringVariable, geqVariables, canBeTrue & aCanBeTrue, canBeFalse & aCanBeFalse);
                }
            }

            return this;
        }

        public override IAbstractDomain Widening(IAbstractDomain prev)
        {
            if (prev is OrderPredicate<Variable>)
            {
                var other = (OrderPredicate<Variable>)prev;
                if (stringVariable.Equals(other.stringVariable))
                {
                    return new OrderPredicate<Variable>(stringVariable, geqVariables.Widening(other.geqVariables), canBeTrue || other.canBeTrue, canBeFalse || other.canBeFalse);
                }
            }
            return base.Widening(prev);
        }

        public override string ToString()
        {
            return stringVariable.ToString() + "<=" + geqVariables.ToString();
        }

        public override IStringPredicate AssignInParallel<Variable1>(Dictionary<Variable1, FList<Variable1>> sourcesToTargets)
        {
            FList<Variable1> list;
            if (sourcesToTargets.TryGetValue((Variable1)(object)stringVariable, out list) && !list.IsEmpty())
            {
                Set<Variable1> newVars = new Set<Variable1>();

                foreach (Variable v in geqVariables.Values)
                {
                    FList<Variable1> vlist;
                    if (sourcesToTargets.TryGetValue((Variable1)(object)v, out vlist))
                    {
                        newVars.AddRange(vlist.GetEnumerable());
                    }
                }

                return new OrderPredicate<Variable1>(list.Head, new SetOfConstraints<Variable1>(newVars, false), canBeTrue, canBeFalse);
            }
            else
            {
                return FlatPredicate.Top;
            }
        }

        public override IStringPredicate RenameVariable<Variable1>(Variable1 oldName, Variable1 newName)
        {
            bool changed = false;

            Variable newStringVariable = stringVariable;

            if(stringVariable.Equals(oldName as Variable))
            {
                newStringVariable = newName as Variable;
                changed = true;
            }
            SetOfConstraints<Variable> newGeqVariables = geqVariables;
            if(geqVariables.Contains(oldName as Variable))
            {
                newGeqVariables = newGeqVariables.Add(newName as Variable);
            }

            if (changed)
            {
                return new OrderPredicate<Variable>(newStringVariable, newGeqVariables, canBeTrue, canBeFalse);
            }
            else
            {
                return this;
            }
        }
    }
}
