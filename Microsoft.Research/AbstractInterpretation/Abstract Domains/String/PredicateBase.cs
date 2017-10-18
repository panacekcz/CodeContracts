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

// Created by Vlastimil Dort (2016-2017)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.DataStructures;

namespace Microsoft.Research.AbstractDomains.Strings
{
    public abstract class PredicateBase : IStringPredicate
    {
        protected readonly bool canBeTrue, canBeFalse;

        protected PredicateBase(bool canBeTrue, bool canBeFalse)
        {
            this.canBeTrue = canBeTrue;
            this.canBeFalse = canBeFalse;
        }

        public virtual bool IsBottom
        {
            get { return !canBeTrue && !canBeFalse; }
        }

        public virtual bool IsTop
        {
            get { return canBeTrue && canBeFalse; }
        }


        IAbstractDomain IAbstractDomain.Bottom
        {
            get { return FlatPredicate.Bottom; }
        }

        IAbstractDomain IAbstractDomain.Top
        {
            get { return FlatPredicate.Top; }
        }

        /// <summary>
        /// Converts the predicate to <see cref="ProofOutcome"/> enumeration.
        /// </summary>
        public ProofOutcome ProofOutcome
        {
            get
            {
                return ProofOutcomeUtils.Build(canBeTrue, canBeFalse);
            }
        }

        public T To<T>(IFactory<T> factory)
        {
            if (IsBottom)
            {
                return factory.IdentityForOr;
            }
            else
            {
                return factory.IdentityForAnd;
            }
        }

        public virtual bool ContainsValue(bool value)
        {
            return value ? canBeTrue : canBeFalse;
        }

        public virtual IStringPredicate RenameVariable<Variable>(Variable oldName, Variable newName)
           where Variable : class, IEquatable<Variable>
        {
            return this;
        }

        public virtual IStringPredicate AssignInParallel<Variable>(Dictionary<Variable, FList<Variable>> sourcesToTargets)
            where Variable : class, IEquatable<Variable>
        {
            return this;
        }

        public virtual IAbstractDomain Join(IAbstractDomain a)
        {
            // Join the two predicates as flat predicates (possible overapproximation).
            // Subclasses may give more precise results
            PredicateBase c = a as PredicateBase;
            if (c != null)
            {
                return new FlatPredicate(canBeTrue | c.canBeTrue, canBeFalse | c.canBeFalse);
            }
            else
            {
                if (IsBottom)
                {
                    return a;
                }
                else if (a.IsBottom)
                {
                    return this;
                }
                else
                {
                    return FlatPredicate.Top;//Overapproximate
                }
            }
        }

        public virtual IAbstractDomain Widening(IAbstractDomain prev)
        {
            // Join guarantees termination
            return Join(prev);
        }

        public virtual bool LessEqual(IAbstractDomain a)
        {
            FlatPredicate c = a as FlatPredicate;
            if (c != null)
            {
                return (!canBeTrue | c.canBeTrue) & (!canBeFalse | c.canBeFalse);
            }
            else
            {
                return !canBeTrue && !canBeFalse;
            }
        }

        public virtual bool RefersToVariable<Variable>(Variable variable)
        {
            return false;
        }
        public abstract IAbstractDomain Meet(IAbstractDomain a);
        public abstract object Clone();
    }
}
