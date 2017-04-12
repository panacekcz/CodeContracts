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

// Created by Vlastimil Dort (2015-2016)
// Master thesis String Analysis for Code Contracts

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


    /// <summary>
    /// Represents top, bottom, or a constant predicate.
    /// </summary>
    public class FlatPredicate : PredicateBase
    {
        private static readonly FlatPredicate bottom = new FlatPredicate(false, false);
        private static readonly FlatPredicate top = new FlatPredicate();
        private static readonly FlatPredicate @true = new FlatPredicate(true);
        private static readonly FlatPredicate @false = new FlatPredicate(false);

        /// <summary>
        /// Gets the bottom (unreached) value.
        /// </summary>
        public static FlatPredicate Bottom
        {
            get
            {
                return Bottom;
            }
        }
        /// <summary>
        /// Gets the top (unknown) value.
        /// </summary>
        public static FlatPredicate Top
        {
            get
            {
                return top;
            }
        }

        /// <summary>
        /// Gets the constant true value.
        /// </summary>
        public static FlatPredicate True
        {
            get
            {
                return @true;
            }
        }
        /// <summary>
        /// Gets the constant false value.
        /// </summary>
        public static FlatPredicate False
        {
            get
            {
                return @false;
            }
        }
        /// <summary>
        /// Converts the <see cref="ProofOutcome"/> enumeration to a predicate.
        /// </summary>
        /// <param name="outcome">The converted proof outcome (one of 4 values).</param>
        /// <returns>The predicate with the same value as <paramref name="outcome"/>.</returns>
        public static FlatPredicate ForProofOutcome(CodeAnalysis.ProofOutcome outcome)
        {
            bool canBeTrue = outcome == CodeAnalysis.ProofOutcome.Top || outcome == CodeAnalysis.ProofOutcome.True;
            bool canBeFalse = outcome == CodeAnalysis.ProofOutcome.Top || outcome == CodeAnalysis.ProofOutcome.False;
            return new FlatPredicate(canBeTrue, canBeFalse);
        }

        /// <summary>
        /// Creates an abstract predicate with the specifed possibility of values.
        /// </summary>
        /// <param name="canBeTrue">Whether the predicate can be true.</param>
        /// <param name="canBeFalse">Whether the predicate can be false.</param>
        public FlatPredicate(bool canBeTrue, bool canBeFalse)
            : base(canBeTrue, canBeFalse)
        {
            
        }
        /// <summary>
        /// Creates a top predicate.
        /// </summary>
        public FlatPredicate()
          : base(true, true)
        {
        }

        /// <summary>
        /// Creates a constant predicate with the specified value.
        /// </summary>
        /// <param name="boolConstant">The constant boolean value.</param>
        public FlatPredicate(bool boolConstant)
          : base(boolConstant, !boolConstant)
        {

        }

        public override IAbstractDomain Meet(IAbstractDomain a)
        {
            FlatPredicate c = a as FlatPredicate;
            if (c != null)
            {
                return new FlatPredicate(canBeTrue & c.canBeTrue, canBeFalse & c.canBeFalse);
            }
            else if (IsBottom)
            {
                return this;
            }
            else if (IsTop)
            {
                return a;
            }
            else
            {
                // The subclasses may provide more precise definition
                return a.Meet(this);
            }
        }

        public override object Clone()
        {
            return new FlatPredicate(canBeTrue, canBeFalse);
        }


        public override string ToString()
        {
            if (canBeTrue)
            {
                return canBeFalse ? "Top" : "True";
            }
            else
            {
                return canBeFalse ? "False" : "Bottom";
            }
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as FlatPredicate);
        }

        public override int GetHashCode()
        {
            return ProofOutcome.GetHashCode();
        }

        public bool Equals(FlatPredicate other)
        {
            if ((object)other == null)
            {
                return false;
            }
            return other.canBeFalse == canBeFalse && other.canBeTrue == canBeTrue;
        }

       
    }
}
