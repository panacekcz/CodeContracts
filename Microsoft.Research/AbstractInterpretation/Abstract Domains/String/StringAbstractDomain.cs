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

// Modified by Vlastimil Dort (2015-2016)
// Master thesis String Analysis for Code Contracts

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Research.AbstractDomains.Expressions;
using System.Diagnostics;
using Microsoft.Research.DataStructures;

namespace Microsoft.Research.AbstractDomains.Strings
{
    internal enum NullHandling
    {
        Exception,
        Empty,
        Distinct,
    }
    public class SimpleFunctionalAbstractDomain<Domain, Codomain> : FunctionalAbstractDomain<SimpleFunctionalAbstractDomain<Domain, Codomain>, Domain, Codomain>
        where Codomain : class, IAbstractDomain
    {

        internal void ResetToNormal()
        {
            this.State = AbstractState.Normal;
        }

        protected override string ToLogicalFormula(Domain d, Codomain c)
        {
            return null;
        }

        protected override T To<T>(Domain d, Codomain c, IFactory<T> factory)
        {
            return factory.Constant(true);
        }

        public SimpleFunctionalAbstractDomain()
        {

        }

        public SimpleFunctionalAbstractDomain(SimpleFunctionalAbstractDomain<Domain, Codomain> source)
        {
            foreach (var x in source.Keys)
            {
                var cloned = source[x].Clone() as Codomain;
                this[x] = cloned;
            }
        }

        #region From FunctionalAbstractDomain
        public override object Clone()
        {
            return new SimpleFunctionalAbstractDomain<Domain, Codomain>(this);
        }

        protected override SimpleFunctionalAbstractDomain<Domain, Codomain> Factory()
        {
            return new SimpleFunctionalAbstractDomain<Domain, Codomain>();
        }
        #endregion
    }

    /// <summary>
    /// A generic abstract domain abstracting (environments of) strings.
    /// </summary>
    /// <typeparam name="Expression">Type of expressions in the operations.</typeparam>
    public class StringAbstractDomain<Variable, Expression, StringAbstraction> :
      IAbstractDomainForEnvironments<Variable, Expression>,
      IStringAbstractDomain<Variable, Expression>
      where StringAbstraction : class, IStringAbstraction<StringAbstraction, string>
      where Variable : class, IEquatable<Variable>
      where Expression : class
    {
        #region Private state
        private readonly IExpressionDecoder<Variable, Expression> decoder;
        private readonly StringSimpleTestVisitor testVisitor;
        // Implementation of string operations
        private readonly IStringOperations<StringAbstraction, Variable> operations;
        // Abstractions of string variables
        private SimpleFunctionalAbstractDomain<Variable, StringAbstraction> strings;
        // Abstractions and predicates of boolean variables
        private SimpleFunctionalAbstractDomain<Variable, IStringPredicate> predicates;
        #endregion

        #region Constructor

        public StringAbstractDomain(
          IExpressionDecoder<Variable, Expression>/*!*/ decoder,
          IStringOperations<StringAbstraction, Variable> operations)
        {
            this.decoder = decoder;
            this.operations = operations;

            this.strings = new SimpleFunctionalAbstractDomain<Variable, StringAbstraction>();
            this.predicates = new SimpleFunctionalAbstractDomain<Variable, IStringPredicate>();

            this.testVisitor = new StringSimpleTestVisitor(decoder);
        }

        private StringAbstractDomain(IExpressionDecoder<Variable, Expression>/*!*/ decoder,
          IStringOperations<StringAbstraction, Variable> operations,
          SimpleFunctionalAbstractDomain<Variable, StringAbstraction> strings,
          SimpleFunctionalAbstractDomain<Variable, IStringPredicate> predicates,
          StringSimpleTestVisitor testVisitor
          )
        {
            this.decoder = decoder;
            this.operations = operations;
            this.strings = strings;
            this.predicates = predicates;
            this.testVisitor = testVisitor;
        }

        private StringAbstractDomain(StringAbstractDomain<Variable, Expression, StringAbstraction>/*!*/ source)
        {
            this.decoder = source.decoder;
            this.operations = source.operations;
            this.strings = source.strings;
            this.predicates = source.predicates;
            this.testVisitor = source.testVisitor;
        }
        #endregion


        #region IPureExpressionAssignmentsWithForward<Expression> Members

        public void Assign(Expression x, Expression exp)
        {
            this.strings.ResetToNormal();
            this.strings[this.decoder.UnderlyingVariable(x)] = EvalStringAbstraction(exp);
            this.predicates.ResetToNormal();
            this.predicates[this.decoder.UnderlyingVariable(x)] = EvalBoolExpression(exp);
        }

        #endregion

        #region IPureExpressionAssignments<Expression> Members

        public List<Variable> Variables
        {
            get
            {
                var l = new List<Variable>(this.strings.Keys);
                l.AddRange(this.predicates.Keys);
                return l;
            }
        }

        public void AddVariable(Variable var)
        {
            // do nothing
        }

        public void ProjectVariable(Variable var)
        {
            this.RemoveVariable(var);
        }

        public void RemoveVariable(Variable var)
        {
            this.strings.RemoveElement(var);
            this.predicates.RemoveElement(var);
        }

        public void RenameVariable(Variable OldName, Variable NewName)
        {
            this.strings[NewName] = this.strings[OldName];
            this.predicates[NewName] = this.predicates[OldName];
            this.RemoveVariable(OldName);
        }

        #endregion

        #region IPureExpressionTest<Expression> Members

        


        private class StringSimpleTestVisitor : StringDomainTestVisitor<StringAbstractDomain<Variable, Expression, StringAbstraction>, Variable, Expression>
        {
            public StringSimpleTestVisitor(IExpressionDecoder<Variable, Expression> decoder)
              : base(decoder)
            {
            }
            protected internal override StringAbstractDomain<Variable, Expression, StringAbstraction> TestVariableHolds(Variable var, bool holds, StringAbstractDomain<Variable, Expression, StringAbstraction> data)
            {
                return data.Test(var, holds);
            }
        }

        public IAbstractDomainForEnvironments<Variable, Expression>/*!*/ TestTrue(Expression/*!*/ guard)
        {
            return testVisitor.VisitTrue(guard, this);
        }
        public IAbstractDomainForEnvironments<Variable, Expression>/*!*/ TestFalse(Expression/*!*/ guard)
        {
            return testVisitor.VisitFalse(guard, this);
        }

        private StringAbstractDomain<Variable, Expression, StringAbstraction>/*!*/ Test(Variable assumedVariable, bool holds)
        {

            // We must create a copy of the domain because the test visitor assumes that (see Not-LogicalAnd)
            // If this changes, this method might be simplified to just mutate this
            StringAbstractDomain<Variable, Expression, StringAbstraction> mutable = new StringAbstractDomain<Variable, Expression, StringAbstraction>(this);

            IStringPredicate predicate;
            if (mutable.predicates.TryGetValue(assumedVariable, out predicate))
            {
                if (predicate is StringAbstractionPredicate<StringAbstraction, Variable>)
                {
                    var abstractionPredicate = predicate as StringAbstractionPredicate<StringAbstraction, Variable>;

                    if (mutable.strings.ContainsKey(abstractionPredicate.DependentVariable))
                    {
                        StringAbstraction old = mutable.strings[abstractionPredicate.DependentVariable];
                        mutable.strings[abstractionPredicate.DependentVariable] = old.Meet(abstractionPredicate.AbstractionIf(holds));
                    }
                    else
                    {
                        mutable.strings[abstractionPredicate.DependentVariable] = abstractionPredicate.AbstractionIf(holds);
                    }
                }
                else if (!predicate.ContainsValue(holds))
                {
                    // The known information contradicts, so we are unreachable
                    return (StringAbstractDomain<Variable, Expression, StringAbstraction>)Bottom;
                    // Could be also done as Meet on FlatPredicate
                }
                //else remains
            }

            //Change the known information
            mutable.predicates[assumedVariable] = new FlatPredicate(holds);

            return mutable;
        }



        public FlatAbstractDomain<bool> CheckIfHolds(Expression/*!*/ exp)
        {
            return new FlatAbstractDomain<bool>(true).Top;
        }

        void IPureExpressionTest<Variable, Expression>.AssumeDomainSpecificFact(DomainSpecificFact fact)
        {
            // Do nothing
        }
        #endregion

        #region IAssignInParallel<Expression> Members

        public void AssignInParallel(Dictionary<Variable, FList<Variable>> sourcesToTargets, Converter<Variable, Expression> convert)
        {

            this.strings.ResetToNormal();
            this.predicates.ResetToNormal();

            if (sourcesToTargets.Count == 0)
            {
                // do nothing...
            }
            else
            {
                // Evaluate the values in the pre-state
                var values = new Dictionary<Variable, StringAbstraction>();

                foreach (var exp in sourcesToTargets.Keys)
                {
                    values[exp] = EvalStringAbstraction(convert(exp));
                }

                // Update the values
                foreach (var exp in sourcesToTargets.Keys)
                {
                    var value = values[exp];   // The new value in the pre-state

                    foreach (var target in sourcesToTargets[exp].GetEnumerable())
                    {
                        if (!value.IsTop)
                        {
                            this.strings[target] = value;
                        }
                        else
                        {
                            if (this.strings.ContainsKey(target))
                            {
                                this.strings.RemoveElement(target);
                            }
                        }
                    }

                }

                // Evaluate the values in the pre-state
                var rvalues = new Dictionary<Variable, IStringPredicate>();

                foreach (var exp in sourcesToTargets.Keys)
                {
                    rvalues[exp] = EvalBoolExpression(convert(exp));
                }

                // Update the values
                foreach (var exp in sourcesToTargets.Keys)
                {
                    var value = rvalues[exp].AssignInParallel(sourcesToTargets);   // The new value in the pre-state

                    foreach (var target in sourcesToTargets[exp].GetEnumerable())
                    {
                        if (!value.IsTop)
                        {
                            this.predicates[target] = value;
                        }
                        else
                        {
                            if (this.predicates.ContainsKey(target))
                            {
                                this.predicates.RemoveElement(target);
                            }
                        }
                    }

                }

            }
        }

        #endregion

        #region String operations
        #region Trivial string operations
        public void Empty(Expression targetExp)
        {
            Debug.Assert(targetExp != null);

            AssignLeftTarget(targetExp, operations.Constant(""));
        }

        public void Copy(Expression targetExp, Expression valueExp)
        {
            Debug.Assert(targetExp != null && valueExp != null);

            AssignLeftTarget(targetExp, EvalStringAbstraction(valueExp));
        }
        #endregion

        private bool TryBottomArguments(out StringAbstraction targetAbstraction, params WithConstants<StringAbstraction>[] argumentAbstractions)
        {
            foreach (WithConstants<StringAbstraction> argument in argumentAbstractions)
            {
                if (argument.IsBottom)
                {
                    targetAbstraction = argument.Abstract;
                    return true;
                }
            }
            targetAbstraction = null;
            return false;
        }

        #region Concatenation operations
        /// <inheritdoc/>
        public void Concat(Expression targetExp, Expression leftExp, Expression rightExp)
        {
            Debug.Assert(targetExp != null && leftExp != null && rightExp != null);

            WithConstants<StringAbstraction> leftAbstraction = EvalStringArgument(leftExp, NullHandling.Empty);
            WithConstants<StringAbstraction> rightAbstraction = EvalStringArgument(rightExp, NullHandling.Empty);

            StringAbstraction targetAbstraction;

            if (leftAbstraction.IsConstant && rightAbstraction.IsConstant)
            {
                targetAbstraction = operations.Constant(string.Concat(leftAbstraction.Constant, rightAbstraction.Constant));
            }
            else if (!TryBottomArguments(out targetAbstraction, leftAbstraction, rightAbstraction))
            {
                targetAbstraction = operations.Concat(leftAbstraction, rightAbstraction);
            }

            AssignLeftTarget(targetExp, targetAbstraction);
        }

        /// <inheritdoc/>
        public void Concat(Expression targetExp, Expression[] argExps)
        {
            Debug.Assert(argExps.Length > 0);

            // The concatenation is evaluated left-to-right, which may be not the best order for some abstract domains
            WithConstants<StringAbstraction> currentAbstraction = EvalStringArgument(argExps[0], NullHandling.Empty);

            if (currentAbstraction.IsConstant || !currentAbstraction.Abstract.IsBottom)
            {
                foreach (Expression otherExp in System.Linq.Enumerable.Skip(argExps, 1))
                {
                    WithConstants<StringAbstraction> otherAbstraction = EvalStringArgument(otherExp, NullHandling.Empty);

                    if (!otherAbstraction.IsConstant && otherAbstraction.Abstract.IsBottom)
                    {
                        currentAbstraction = otherAbstraction;
                        break;
                    }

                    if (currentAbstraction.IsConstant && otherAbstraction.IsConstant)
                    {
                        currentAbstraction = new WithConstants<StringAbstraction>(string.Concat(currentAbstraction.Constant, otherAbstraction.Constant));
                    }
                    else
                    {
                        currentAbstraction = new WithConstants<StringAbstraction>(operations.Concat(currentAbstraction, otherAbstraction));
                    }
                }
            }
            AssignLeftTarget(targetExp, currentAbstraction.ToAbstract(operations));
        }
        #endregion
        /// <inheritdoc/>
        public void Insert(Expression targetExp, Expression valueExp, Expression indexExp, Expression partExp, INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            Debug.Assert(targetExp != null && valueExp != null && indexExp != null && partExp != null);

            WithConstants<StringAbstraction> valueAbstraction = EvalStringArgument(valueExp, NullHandling.Exception);
            IndexInterval indexAbstraction = EvalIndexInterval(indexExp, numericalDomain);
            indexAbstraction = indexAbstraction.Meet(IndexInterval.UnknownNonNegative);
            WithConstants<StringAbstraction> partAbstraction = EvalStringArgument(partExp, NullHandling.Exception);

            StringAbstraction targetAbstraction;

            if (valueAbstraction.IsConstant && partAbstraction.IsConstant && indexAbstraction.IsFiniteConstant)
            {
                if (indexAbstraction.LowerBound.AsInt <= valueAbstraction.Constant.Length)
                {
                    targetAbstraction = operations.Constant(valueAbstraction.Constant.Insert(indexAbstraction.LowerBound.AsInt, partAbstraction.Constant));
                }
                else
                {
                    targetAbstraction = operations.Top.Bottom;
                }
            }
            else if (indexAbstraction.IsBottom)
            {
                targetAbstraction = operations.Top.Bottom;
            }
            else if (!TryBottomArguments(out targetAbstraction, valueAbstraction, partAbstraction))
            {
                targetAbstraction = operations.Insert(valueAbstraction, indexAbstraction, partAbstraction);
            }

            AssignLeftTarget(targetExp, targetAbstraction);
        }

        #region Replace operations
        /// <inheritdoc/>
        public void ReplaceChar(Expression targetExp, Expression valueExp, Expression fromExp, Expression toExp, INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            Debug.Assert(targetExp != null && valueExp != null && fromExp != null && toExp != null);

            CharInterval fromInterval = EvalCharInterval(fromExp, numericalDomain);
            CharInterval toInterval = EvalCharInterval(toExp, numericalDomain);

            WithConstants<StringAbstraction> valueAbstraction = EvalStringArgument(valueExp, NullHandling.Exception);

            StringAbstraction targetAbstraction;

            if (fromInterval.IsConstant && toInterval.IsConstant && valueAbstraction.IsConstant)
            {
                targetAbstraction = operations.Constant(valueAbstraction.Constant.Replace(fromInterval.LowerBound, toInterval.LowerBound));
            }
            else if (fromInterval.IsBottom || toInterval.IsBottom)
            {
                targetAbstraction = operations.Top.Bottom;
            }
            else if (!TryBottomArguments(out targetAbstraction, valueAbstraction))
            {
                targetAbstraction = operations.Replace(valueAbstraction.ToAbstract(operations), fromInterval, toInterval);
            }

            AssignLeftTarget(targetExp, targetAbstraction);
        }
        /// <inheritdoc/>
        public void ReplaceString(Expression targetExp, Expression valueExp, Expression fromExp, Expression toExp)
        {
            Debug.Assert(targetExp != null && valueExp != null && fromExp != null && toExp != null);

            WithConstants<StringAbstraction> valueAbstraction = EvalStringArgument(valueExp, NullHandling.Exception);
            WithConstants<StringAbstraction> fromAbstraction = EvalStringArgument(fromExp, NullHandling.Exception);
            WithConstants<StringAbstraction> toAbstraction = EvalStringArgument(toExp, NullHandling.Exception);

            StringAbstraction targetAbstraction;

            if (valueAbstraction.IsConstant && fromAbstraction.IsConstant && toAbstraction.IsConstant)
            {
                targetAbstraction = operations.Constant(valueAbstraction.Constant.Replace(fromAbstraction.Constant, toAbstraction.Constant));
            }
            else if (!TryBottomArguments(out targetAbstraction, valueAbstraction, fromAbstraction, toAbstraction))
            {
                targetAbstraction = operations.Replace(valueAbstraction, fromAbstraction, toAbstraction);
            }

            AssignLeftTarget(targetExp, targetAbstraction);
        }
        #endregion
        #region Substring operations

        /// <inheritdoc/>
        public void SubstringRemove(Expression targetExp, Expression valueExp, Expression indexExp, Expression lengthExp, bool remove,
            INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            Debug.Assert(targetExp != null && valueExp != null && indexExp != null);
            WithConstants<StringAbstraction> valueAbstraction = EvalStringArgument(valueExp, NullHandling.Exception);
            IndexInterval indexAbstraction = EvalIndexInterval(indexExp, numericalDomain);
            indexAbstraction = indexAbstraction.Meet(IndexInterval.UnknownNonNegative);
            IndexInterval lengthAbstraction = EvalIndexIntervalOrDefault(lengthExp, numericalDomain, IndexInt.Infinity);
            lengthAbstraction = lengthAbstraction.Meet(IndexInterval.UnknownNonNegative);

            StringAbstraction targetAbstraction;

            if (valueAbstraction.IsConstant && indexAbstraction.IsFiniteConstant && lengthAbstraction.IsConstant)
            {
                string valueString = valueAbstraction.Constant;
                IndexInt lengthIndex = lengthAbstraction.LowerBound;
                int index = indexAbstraction.LowerBound.AsInt;

                if (index <= valueString.Length && (lengthIndex.IsInfinite || remove ? (lengthIndex.AsInt + index < valueString.Length) : (lengthIndex.AsInt + index <= valueString.Length)))
                {
                    if (lengthIndex.IsInfinite)
                    {
                        targetAbstraction = operations.Constant(remove ? valueString.Remove(index) : valueString.Substring(index));
                    }
                    else
                    {
                        targetAbstraction = operations.Constant(remove ? valueString.Remove(index, lengthIndex.AsInt) : valueString.Substring(index, lengthIndex.AsInt));
                    }
                }
                else
                {
                    targetAbstraction = operations.Top.Bottom;
                }
            }
            else if (!TryBottomArguments(out targetAbstraction, valueAbstraction))
            {
                targetAbstraction = operations.Substring(valueAbstraction.ToAbstract(operations), indexAbstraction, lengthAbstraction);
            }
            AssignLeftTarget(targetExp, targetAbstraction);
        }

        #endregion
        #region Padding operations

        /// <inheritdoc/>
        public void PadLeftRight(Expression targetExp, Expression valueExp, Expression lengthExp, Expression fillExp, bool right, INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            Debug.Assert(targetExp != null && valueExp != null && lengthExp != null);

            CharInterval fillInterval = EvalCharIntervalOrDefault(fillExp, numericalDomain, ' ');
            IndexInterval lengthAbstraction = EvalIndexInterval(lengthExp, numericalDomain);

            WithConstants<StringAbstraction> valueAbstraction = EvalStringArgument(valueExp, NullHandling.Exception);

            StringAbstraction targetAbstraction;

            if (valueAbstraction.IsConstant && fillInterval.IsConstant && lengthAbstraction.IsFiniteConstant)
            {
                if (lengthAbstraction.LowerBound.IsNegative)
                {
                    targetAbstraction = operations.Top.Bottom;
                }
                else
                {
                    string padded;
                    if (!right)
                    {
                        padded = valueAbstraction.Constant.PadLeft(lengthAbstraction.LowerBound.AsInt, fillInterval.LowerBound);
                    }
                    else
                    {
                        padded = valueAbstraction.Constant.PadRight(lengthAbstraction.LowerBound.AsInt, fillInterval.LowerBound);
                    }
                    targetAbstraction = operations.Constant(padded);
                }

            }
            else if (!TryBottomArguments(out targetAbstraction, valueAbstraction))
            {
                targetAbstraction = operations.PadLeftRight(valueAbstraction.ToAbstract(operations), lengthAbstraction, fillInterval, right);
            }

            AssignLeftTarget(targetExp, targetAbstraction);
        }
        #endregion

        private void Trim(Expression targetExp, Expression valueExp, Expression trimExp, bool start, bool end)
        {
            Debug.Assert(start || end);

            WithConstants<StringAbstraction> valueAbstraction = EvalStringArgument(valueExp, NullHandling.Exception);
            WithConstants<StringAbstraction> trimAbstraction = EvalStringArgument(trimExp, NullHandling.Empty);
            StringAbstraction targetAbstraction;

            if (valueAbstraction.IsConstant && trimAbstraction.IsConstant && !string.IsNullOrEmpty(trimAbstraction.Constant))
            {
                string trimmed;
                char[] trimArray = trimAbstraction.Constant.ToCharArray();
                if (!start)
                {
                    trimmed = valueAbstraction.Constant.TrimEnd(trimArray);
                }
                else if (!end)
                {
                    trimmed = valueAbstraction.Constant.TrimStart(trimArray);
                }
                else
                {
                    trimmed = valueAbstraction.Constant.Trim(trimArray);
                }

                targetAbstraction = operations.Constant(trimmed);
            }
            else if (!TryBottomArguments(out targetAbstraction, valueAbstraction, trimAbstraction))
            {
                if (start && end)
                {
                    targetAbstraction = operations.Trim(valueAbstraction, trimAbstraction);
                }
                else
                { 
                    targetAbstraction = operations.TrimStartEnd(valueAbstraction, trimAbstraction, end);
                }
                
            }
            AssignLeftTarget(targetExp, targetAbstraction);
        }
        /// <inheritdoc/>
        public void Trim(Expression targetExp, Expression valueExp, Expression trimExp)
        {
            Trim(targetExp, valueExp, trimExp, true, true);
        }
        /// <inheritdoc/>
        public void TrimStartEnd(Expression targetExp, Expression valueExp, Expression trimExp, bool end)
        {
            Trim(targetExp, valueExp, trimExp, !end, end);
        }
        
        /// <inheritdoc/>
        public void IsNullOrEmpty(Expression targetExp, Expression valueExp)
        {
            Debug.Assert(targetExp != null && valueExp != null);

            Variable valueVar;
            WithConstants<StringAbstraction> valueAbstraction = EvalStringArgument(valueExp, out valueVar, NullHandling.Empty);

            IStringPredicate targetPredicate;

            if (valueAbstraction.IsConstant)
            {
                targetPredicate = new FlatPredicate(string.IsNullOrEmpty(valueAbstraction.Constant));
            }
            else if (valueAbstraction.Abstract.IsBottom)
            {
                targetPredicate = FlatPredicate.Bottom;
            }
            else
            {
                targetPredicate = operations.IsEmpty(valueAbstraction.ToAbstract(operations), valueVar);
            }

            AssignRightTarget(targetExp, targetPredicate);
        }

        #region Containment operations
        /// <inheritdoc/>
        public void Contains(Expression targetExp, Expression valueExp, Expression partExp)
        {
            Debug.Assert(targetExp != null && valueExp != null && partExp != null);

            Variable valueVar, partVar;
            WithConstants<StringAbstraction> valueAbstraction = EvalStringArgument(valueExp, out valueVar, NullHandling.Exception);
            WithConstants<StringAbstraction> partAbstraction = EvalStringArgument(partExp, out partVar, NullHandling.Exception);

            IStringPredicate targetPredicate;

            if (valueAbstraction.IsConstant && partAbstraction.IsConstant)
            {
                targetPredicate = new FlatPredicate(valueAbstraction.Constant.Contains(partAbstraction.Constant));
            }
            else if (valueAbstraction.IsBottom || partAbstraction.IsBottom)
            {
                targetPredicate = FlatPredicate.Bottom;
            }
            else
            {
                targetPredicate = operations.Contains(valueAbstraction, valueVar, partAbstraction, partVar);
            }

            AssignRightTarget(targetExp, targetPredicate);
        }
        /// <inheritdoc/>
        public void StartsEndsWith(Expression targetExp, Expression valueExp, Expression partExp, Expression comparisonExp, bool ends)
        {
            int comparison;

            if (TryEvalIntConstant(comparisonExp, out comparison) && (StringComparison)comparison == StringComparison.Ordinal)
            {
                Variable partVar, valueVar;
                WithConstants<StringAbstraction> partAbstraction = EvalStringArgument(partExp, out partVar, NullHandling.Exception);
                WithConstants<StringAbstraction> valueAbstraction = EvalStringArgument(valueExp, out valueVar, NullHandling.Exception);

                IStringPredicate targetPredicate;

                if (partAbstraction.IsConstant && valueAbstraction.IsConstant)
                {
                    bool result;
                    if (!ends)
                    {
                        result = valueAbstraction.Constant.StartsWith(partAbstraction.Constant, StringComparison.Ordinal);
                    }
                    else
                    {
                        result = valueAbstraction.Constant.EndsWith(partAbstraction.Constant, StringComparison.Ordinal);
                    }

                    targetPredicate = new FlatPredicate(result);
                }
                else if (partAbstraction.IsBottom || valueAbstraction.IsBottom)
                {
                    targetPredicate = FlatPredicate.Bottom;
                }
                else
                {
                    if (!ends)
                    {
                        targetPredicate = operations.StartsWithOrdinal(valueAbstraction, valueVar, partAbstraction, partVar);
                    }
                    else
                    {
                        targetPredicate = operations.EndsWithOrdinal(valueAbstraction, valueVar, partAbstraction, partVar);
                    }
                }

                AssignRightTarget(targetExp, targetPredicate);
            }
            else
            {
                UnassignRightTarget(targetExp);
            }
        }
        #endregion

        #region Comparison operations

        private void TestNulls(ref WithConstants<StringAbstraction> abs, Variable var, INullQuery<Variable> query, out bool canBeNull, out bool canBeNonNull)
        {

            if (abs.Constant != null)
            {
                canBeNonNull = true;
                canBeNull = false;
            }
            else if (abs.Abstract == null)
            {
                abs = new WithConstants<StringAbstraction>("");
                canBeNonNull = false;
                canBeNull = true;
                return;
            }
            else if (var != null && query != null)
            {
                canBeNull = !query.IsNonNull(var);
                canBeNonNull = !query.IsNull(var);
            }
            else
            {
                canBeNonNull = true;
                canBeNull = true;
            }
        }
        /// <inheritdoc/>
        public void Equals(Expression targetExp, Expression leftExp, Expression rightExp, INullQuery<Variable> nullQuery)
        {
            Debug.Assert(targetExp != null && leftExp != null && rightExp != null);

            Variable leftVar, rightVar;
            WithConstants<StringAbstraction> leftAbstraction = EvalStringArgument(leftExp, out leftVar, NullHandling.Distinct);
            WithConstants<StringAbstraction> rightAbstraction = EvalStringArgument(rightExp, out rightVar, NullHandling.Distinct);

            bool leftCanBeNonNull, leftCanBeNull;
            bool rightCanBeNonNull, rightCanBeNull;

            TestNulls(ref leftAbstraction, leftVar, nullQuery, out leftCanBeNull, out leftCanBeNonNull);
            TestNulls(ref rightAbstraction, rightVar, nullQuery, out rightCanBeNull, out rightCanBeNonNull);

            // If both can be nulls, they can be equal
            bool canBeEqual = leftCanBeNull && rightCanBeNull;
            // If one can be null and the other can be non-null, they can be non-equal.
            bool canBeNonEqual = (leftCanBeNull && rightCanBeNonNull) || (rightCanBeNull && leftCanBeNonNull);

            IStringPredicate targetAbstraction = new FlatPredicate(canBeEqual, canBeNonEqual);

            // If both can be non null, and the result is not top yet, ask the abstract domain
            if (leftCanBeNonNull && rightCanBeNonNull && !(canBeEqual && canBeNonEqual))
            {
                IStringPredicate compared = operations.Equals(leftAbstraction, leftVar, rightAbstraction, rightVar);
                targetAbstraction = (IStringPredicate)targetAbstraction.Join(compared);
            }

            AssignRightTarget(targetExp, targetAbstraction);
        }
        /// <inheritdoc/>
        public void CompareOrdinal(Expression targetExp, Expression leftExp, Expression rightExp,
          INumericalAbstractDomain<Variable, Expression> numericalDomain, INullQuery<Variable> nullQuery)
        {
            Debug.Assert(targetExp != null && leftExp != null && rightExp != null);

            Variable targetVar = VariableFor(targetExp);

            if (numericalDomain != null && targetVar != null)
            {
                WithConstants<StringAbstraction> leftAbstraction = EvalStringArgument(leftExp, NullHandling.Distinct);
                WithConstants<StringAbstraction> rightAbstraction = EvalStringArgument(rightExp, NullHandling.Distinct);

                bool leftCanBeNonNull, leftCanBeNull;
                bool rightCanBeNonNull, rightCanBeNull;

                TestNulls(ref leftAbstraction, VariableFor(leftExp), nullQuery, out leftCanBeNull, out leftCanBeNonNull);
                TestNulls(ref rightAbstraction, VariableFor(rightExp), nullQuery, out rightCanBeNull, out rightCanBeNonNull);

                CompareResult targetResult = CompareResult.Bottom;

                if (leftCanBeNull && rightCanBeNull)
                {
                    targetResult |= CompareResult.Equal;
                }
                if (leftCanBeNull && rightCanBeNonNull)
                {
                    targetResult |= CompareResult.Less;
                }
                if (rightCanBeNull && leftCanBeNonNull)
                {
                    targetResult |= CompareResult.Greater;
                }

                if (targetResult != CompareResult.Top && leftCanBeNonNull && rightCanBeNonNull)
                {
                    targetResult |= operations.CompareOrdinal(leftAbstraction, rightAbstraction);
                }

                numericalDomain.AssumeInDisInterval(targetVar, targetResult.ToDisInterval());
            }
        }
        #endregion
        /// <inheritdoc/>
        public void GetLength(Expression targetExp, Expression sourceExp, INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            Debug.Assert(targetExp != null && sourceExp != null);

            Variable targetVar = VariableFor(targetExp);

            if (numericalDomain != null && targetVar != null)
            {
                WithConstants<StringAbstraction> sourceAbstraction = EvalStringArgument(sourceExp, NullHandling.Exception);
                IndexInterval targetLength;
                if (sourceAbstraction.IsConstant)
                {
                    targetLength = IndexInterval.For(sourceAbstraction.Constant.Length);
                }
                else if (sourceAbstraction.IsBottom)
                {
                    targetLength = IndexInterval.Unknown.Bottom;
                }
                else
                {
                    targetLength = operations.GetLength(sourceAbstraction.Abstract);
                }

                numericalDomain.AssumeInDisInterval(targetVar, targetLength.ToDisInterval());
            }
        }
        #region Index operaitons
        public void IndexOfChar(Expression indexExp, Expression thisExp, Expression needleExp, Expression offsetExp, Expression countExp, bool last, INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            CharInterval needleInterval = EvalCharInterval(needleExp, numericalDomain);

            if (needleInterval.IsConstant)
            {
                string needleString = needleInterval.LowerBound.ToString();

                WithConstants<StringAbstraction> needleAbstraction = new WithConstants<StringAbstraction>(needleString);
                GeneralIndexOfConst(indexExp, thisExp, needleAbstraction, offsetExp, countExp, numericalDomain, last);
            }
        }

        private void GeneralIndexOfConst(Expression indexExp, Expression thisExp, WithConstants<StringAbstraction> needleAbstraction, Expression offsetExp, Expression countExp, INumericalAbstractDomain<Variable, Expression> numericalDomain, bool last)
        {
            IndexInterval offsetInterval = EvalIndexIntervalOrDefault(offsetExp, numericalDomain, IndexInt.ForNonNegative(0));
            IndexInterval countInterval = EvalIndexIntervalOrDefault(countExp, numericalDomain, IndexInt.Infinity);
            WithConstants<StringAbstraction> thisAbstraction = EvalStringArgument(thisExp, NullHandling.Exception);

            Variable indexVar = VariableFor(indexExp);
            if (indexVar != null)
            {
                IndexInterval indexInterval;

                if (thisAbstraction.IsBottom || needleAbstraction.IsBottom || offsetInterval.IsBottom || countInterval.IsBottom)
                {
                    indexInterval = IndexInterval.Unknown.Bottom;
                }
                else
                {
                    indexInterval = operations.IndexOf(thisAbstraction, needleAbstraction, offsetInterval, countInterval, last);
                }
       
                AssignIndexInterval(indexVar, indexInterval, numericalDomain);
            }
        }

        public void IndexOf(Expression indexExp, Expression thisExp, Expression needleExp, Expression offsetExp, Expression countExp, Expression cmpExp, bool last, INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            int comparison;
            if (TryEvalIntConstant(cmpExp, out comparison) && (StringComparison)comparison == StringComparison.Ordinal)
            {
                WithConstants<StringAbstraction> needleAbstraction = EvalStringArgument(needleExp, NullHandling.Exception);
                GeneralIndexOfConst(indexExp, thisExp, needleAbstraction, offsetExp, countExp, numericalDomain, last);
            }
        }
        #endregion

        #region Array operations

        public void GetChar(Expression targetExp, Expression sourceExp, Expression indexExp, INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            Variable targetVariable = VariableFor(targetExp);

            if (numericalDomain != null && targetVariable != null)
            {
                IndexInterval indexInterval = EvalIndexInterval(indexExp, numericalDomain);
                WithConstants<StringAbstraction> sourceAbstraction = EvalStringArgument(sourceExp, NullHandling.Exception);

                CharInterval targetInterval;

                if (sourceAbstraction.IsConstant && indexInterval.IsFiniteConstant)
                {
                    int indexInt = indexInterval.LowerBound.AsInt;
                    if (indexInt >= 0 && sourceAbstraction.Constant.Length > indexInt)
                    {
                        targetInterval = CharInterval.For(sourceAbstraction.Constant[indexInt]);
                    }
                    else
                    {
                        targetInterval = CharInterval.Unreached;
                    }
                }
                else if (sourceAbstraction.IsBottom || indexInterval.IsBottom)
                {
                    targetInterval = CharInterval.Unreached;
                }
                else
                {
                    targetInterval = operations.GetCharAt(sourceAbstraction.ToAbstract(operations), indexInterval);
                }

                numericalDomain.AssumeInDisInterval(targetVariable, targetInterval.ToDisInterval());
            }
        }
        #endregion

        /// <inheritdoc/>
        public void RegexIsMatch(Expression targetExp, Expression valueExp, Expression regexExp)
        {
            Debug.Assert(targetExp != null && valueExp != null && regexExp != null);

            string regexString;
            if (this.TryEvalStringConstant(regexExp, out regexString))
            {
                Variable valueVar = VariableFor(valueExp);
                StringAbstraction valueAbstraction = EvalStringArgument(valueExp, NullHandling.Exception).ToAbstract(operations);

                IStringPredicate targetPredicate;
                if (valueAbstraction.IsBottom)
                {
                    targetPredicate = FlatPredicate.Bottom;
                }
                else
                {
                    try
                    {
                        Regex.AST.Element regexAST = Regex.RegexParser.Parse(regexString);
                        targetPredicate = operations.RegexIsMatch(valueAbstraction, valueVar, regexAST);
                    }
                    catch (Regex.ParseException)
                    {
                        targetPredicate = FlatPredicate.Top;
                    }
                }

                AssignRightTarget(targetExp, targetPredicate);
            }
            else
            {
                UnassignRightTarget(targetExp);
            }
        }

        public void Unknown(Expression unknownExp)
        {
            // Forget about this variable
            UnassignLeftTarget(unknownExp);
        }

        public void Mutate(Expression mutatedExp)
        {
            Variable mutatedVariable = VariableFor(mutatedExp);

            if (mutatedVariable != null)
            {

                // Find predicates that involve the variable
                List<Variable> removedPredicates = new List<Variable>();
                foreach (var element in this.predicates.Elements)
                {
                    if (element.Value is StringAbstractionPredicate<StringAbstraction, Variable>)
                    {
                        var predicate = element.Value as StringAbstractionPredicate<StringAbstraction, Variable>;
                        if (predicate.DependentVariable.Equals(mutatedVariable))
                        {
                            removedPredicates.Add(element.Key);
                        }
                    }
                }
                // Forget about those predicates
                foreach (var removedPredicate in removedPredicates)
                {
                    this.predicates.RemoveElement(removedPredicate);
                }
            }
        }
        #endregion

        #region Assign to variables
        private void UnassignLeftTarget(Expression target)
        {
            this.strings.RemoveElement(this.decoder.UnderlyingVariable(target));
        }
        private void AssignLeftTarget(Expression target, StringAbstraction targetAbstraction)
        {
            this.strings[this.decoder.UnderlyingVariable(target)] = targetAbstraction;
        }
        private void UnassignRightTarget(Expression target)
        {
            this.predicates.RemoveElement(this.decoder.UnderlyingVariable(target));
        }
        private void AssignRightTarget(Expression target, IStringPredicate targetPredicate)
        {
            Debug.Assert(targetPredicate != null);
            this.predicates[this.decoder.UnderlyingVariable(target)] = targetPredicate;
        }
        private void AssignIndexInterval(Variable indexVar, IndexInterval indexAbstraction, INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            //If index abstraction allows negative, only -1 is assumed (can happen only in return values of IndexOf)
            if (numericalDomain != null)
            {
                Numerical.DisInterval disInterval = indexAbstraction.ToDisInterval();
                numericalDomain.AssumeInDisInterval(indexVar, disInterval);
            }
        }
        #endregion
        #region Evaluate expressions

        private Variable VariableFor(Expression expression)
        {
            return decoder.UnderlyingVariable(expression);
        }

        #region Boolean expressions
        public CodeAnalysis.ProofOutcome EvalBool(Variable variable)
        {
            if (!predicates.ContainsKey(variable))
                return CodeAnalysis.ProofOutcome.Top;

            IStringPredicate predicate = predicates[variable];

            return predicate.ProofOutcome;
        }

        private IStringPredicate EvalBoolConstant(Expression exp)
        {
            Debug.Assert(exp != null);
            IStringPredicate result;
            bool val;
            if (this.decoder.TryValueOf<bool>(exp, ExpressionType.Bool, out val))
            {
                result = new FlatPredicate(val);
            }
            else
            {
                result = FlatPredicate.Top;
            }

            return result;
        }
        private IStringPredicate EvalBoolVariable(Variable variable)
        {
            Debug.Assert(variable != null);

            if (predicates.ContainsKey(variable))
            {
                return predicates[variable];
            }
            else
            {
                return FlatPredicate.Top;
            }
        }
        private IStringPredicate EvalBoolExpression(Expression/*!*/ exp)
        {
            IStringPredicate result;
            switch (this.decoder.OperatorFor(exp))
            {
                case ExpressionOperator.Constant:
                    result = EvalBoolConstant(exp);
                    break;

                case ExpressionOperator.Variable:
                    var v = this.decoder.UnderlyingVariable(exp);
                    result = EvalBoolVariable(v);
                    break;

                default:
                    result = FlatPredicate.Top;
                    break;
            }

            return result;
        }
        #endregion
        #region Strings
        private StringAbstraction EvalStringAbstraction(Expression expression)
        {
            Debug.Assert(expression != null);
            return EvalStringArgument(expression, NullHandling.Empty).ToAbstract(operations);
        }

        private WithConstants<StringAbstraction> EvalStringArgument(Expression expression, NullHandling nullHandling)
        {
            Debug.Assert(expression != null);
            Variable variable;
            return EvalStringArgument(expression, out variable, nullHandling);
        }



        private WithConstants<StringAbstraction> EvalStringArgument(Expression expression, out Variable variable, NullHandling nullHandling)
        {
            Debug.Assert(expression != null);

            switch (this.decoder.OperatorFor(expression))
            {
                case ExpressionOperator.Constant:
                    string constant;

                    this.decoder.TryValueOf<string>(expression, ExpressionType.String, out constant);
                    // Constant strings are returned as constants
                    if (this.decoder.TypeOf(expression) == ExpressionType.String && this.decoder.TryValueOf<string>(expression, ExpressionType.String, out constant))
                    {
                        variable = null;
                        return new WithConstants<StringAbstraction>(constant);
                    }
                    else if (this.decoder.IsNull(expression))
                    {
                        // Null is evaluated according to the null handling pattern of the operation
                        variable = null;
                        switch (nullHandling)
                        {
                            case NullHandling.Distinct:
                                return new WithConstants<StringAbstraction>((string)null);
                            case NullHandling.Empty:
                                return new WithConstants<StringAbstraction>("");
                            case NullHandling.Exception:
                                return new WithConstants<StringAbstraction>(operations.Top.Bottom);
                        }
                    }
                    break;

                case ExpressionOperator.Variable:
                    // Variables evaluate according to the abstract elements stored in the left part
                    variable = this.decoder.UnderlyingVariable(expression);
                    if (this.strings.ContainsKey(variable))
                    {
                        return new WithConstants<StringAbstraction>(this.strings[variable]);
                    }
                    else
                    {
                        return new WithConstants<StringAbstraction>(operations.Top);
                    }
            }
            variable = null;
            return new WithConstants<StringAbstraction>(operations.Top);
        }

        private bool TryEvalStringConstant(Expression exp, out string constant)
        {
            if (this.decoder.OperatorFor(exp) == ExpressionOperator.Constant && this.decoder.TypeOf(exp) == ExpressionType.String)
            {
                return this.decoder.TryValueOf<string>(exp, ExpressionType.String, out constant);
            }
            else
            {
                constant = null;
                return false;
            }
        }
        #endregion
        #region Numerical
        private bool TryEvalCharConstant(Expression exp, out char val)
        {
            int i;
            if (TryEvalIntConstant(exp, out i) && i >= char.MinValue && i <= char.MaxValue)
            {
                val = (char)i;
                return true;
            }
            else
            {
                val = default(char);
                return false;
            }
        }

        private bool TryEvalIntConstant(Expression exp, out int val)
        {
            return this.decoder.IsConstantInt(exp, out val);
        }



        /// <summary>
        /// Evaluates char expression to a <see cref="CharInterval"/> if the expression is
        /// not <see langword="null"/>, otherwise use a default character.
        /// </summary>
        /// <param name="expr">The char expression or <see langword="null"/>.</param>
        /// <param name="numericalDomain">Numerical domain used to get the possible
        /// values of the expression, or <see langword="null"/>.</param>
        /// <param name="defaultChar">Default character used if <paramref name="expr"/>
        /// is <see langword="null"/>.</param>
        /// <returns>Character interval containing possible values of <paramref name="expr"/>
        /// or <paramref name="defaultChar"/> if <paramref name="expr"/> is <see langword="null"/>.
        /// </returns>
        private CharInterval EvalCharIntervalOrDefault(Expression expr, INumericalAbstractDomain<Variable, Expression> numericalDomain, char defaultChar)
        {
            if (expr == null)
            {
                return CharInterval.For(defaultChar);
            }
            else
            {
                return EvalCharInterval(expr, numericalDomain);
            }
        }

        /// <summary>
        /// Evaluates char expression to a <see cref="CharInterval"/>.
        /// </summary>
        /// <param name="expr">The char expression.</param>
        /// <param name="numericalDomain">Numerical domain used to get the possible
        /// values of the expression, or <see langword="null"/>.</param>
        /// <returns>Character interval containing possible values of <paramref name="expr"/>.
        /// </returns>
        private CharInterval EvalCharInterval(Expression expr, INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            Debug.Assert(expr != null);

            char constant;
            if (TryEvalCharConstant(expr, out constant))
            {
                return CharInterval.For(constant);
            }
            else if (numericalDomain != null)
            {
                Numerical.Interval interval = numericalDomain.BoundsFor(expr).AsInterval;
                return CharInterval.For(interval);
            }
            else
            {
                return CharInterval.Unknown;
            }
        }


        private IndexInterval EvalIndexInterval(Expression expr, INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            System.Diagnostics.Debug.Assert(expr != null);

            int constant;
            if (TryEvalIntConstant(expr, out constant))
            {
                return IndexInterval.For(constant);
            }
            else if (numericalDomain != null)
            {
                Numerical.Interval interval = numericalDomain.BoundsFor(expr).AsInterval;
                return IndexInterval.For(interval);
            }
            else
            {
                return IndexInterval.Unknown;
            }
        }
        private IndexInterval EvalIndexIntervalOrDefault(Expression expr, INumericalAbstractDomain<Variable, Expression> numericalDomain, IndexInt constant)
        {
            if (expr == null)
            {
                return IndexInterval.For(constant);
            }
            else
            {
                return EvalIndexInterval(expr, numericalDomain);
            }
        }
        #endregion

        #endregion

        #region ToString
        public override string ToString()
        {
            string result;

            if (this.IsBottom)
            {
                result = "_|_";
            }
            else if (this.IsTop)
            {
                result = "Top";
            }
            else
            {
                var tempStr = new StringBuilder();

                foreach (var x in this.strings.Keys)
                {
                    string xAsString = this.decoder != null ? this.decoder.NameOf(x) : x.ToString();
                    tempStr.Append(xAsString + ": " + this.strings[x] + ", ");
                }
                foreach (var x in this.predicates.Keys)
                {
                    string xAsString = this.decoder != null ? this.decoder.NameOf(x) : x.ToString();
                    tempStr.Append(xAsString + ": " + this.predicates[x] + ", ");
                }


                result = tempStr.ToString();
                int indexOfLastComma = result.LastIndexOf(',');
                if (indexOfLastComma > 0)
                {
                    result = result.Remove(indexOfLastComma);
                }
            }

            return result;
        }

        public string ToString(Expression exp)
        {
            if (this.decoder != null)
            {
                return ExpressionPrinter.ToString(exp, this.decoder);
            }
            else
            {
                return "< missing expression decoder >";
            }
        }
        #endregion

        #region IAbstractDomain
        public bool IsBottom
        {
            get
            {
                return strings.IsBottom || predicates.IsBottom;
            }
        }

        public bool IsTop
        {
            get
            {
                return strings.IsTop && predicates.IsTop;
            }
        }

        public IAbstractDomain Bottom
        {
            get
            {
                return new StringAbstractDomain<Variable, Expression, StringAbstraction>(
                  decoder, operations, strings.Bottom, predicates.Bottom, testVisitor
                  );
            }
        }

        public IAbstractDomain Top
        {
            get
            {
                return new StringAbstractDomain<Variable, Expression, StringAbstraction>(
                  decoder, operations, strings.Top, predicates.Top, testVisitor
                  );
            }
        }

        public bool LessEqual(IAbstractDomain a)
        {
            var right = (StringAbstractDomain<Variable, Expression, StringAbstraction>)a;

            return strings.LessEqual(right.strings) && predicates.LessEqual(right.predicates);
        }

        public IAbstractDomain Join(IAbstractDomain a)
        {
            var right = (StringAbstractDomain<Variable, Expression, StringAbstraction>)a;

            var joinStrings = strings.Join(right.strings);
            var joinPredicates = predicates.Join(right.predicates);

            return new StringAbstractDomain<Variable, Expression, StringAbstraction>(decoder, operations, joinStrings, joinPredicates, testVisitor);
        }


        public IAbstractDomain Meet(IAbstractDomain a)
        {
            var right = (StringAbstractDomain<Variable, Expression, StringAbstraction>)a;

            var meetStrings = strings.Meet(right.strings);
            var meetPredicates = predicates.Meet(right.predicates);

            return new StringAbstractDomain<Variable, Expression, StringAbstraction>(decoder, operations, meetStrings, meetPredicates, testVisitor);
        }

        public IAbstractDomain Widening(IAbstractDomain prev)
        {
            var prevSad = (StringAbstractDomain<Variable, Expression, StringAbstraction>)prev;
            var widenStrings = strings.Widening(prevSad.strings);
            var widenPredicates = predicates.Widening(prevSad.predicates);

            return new StringAbstractDomain<Variable, Expression, StringAbstraction>(decoder, operations, widenStrings, widenPredicates, testVisitor);
        }


        public T To<T>(IFactory<T> factory)
        {
            var stringsTo = strings.To(factory);
            var predicatesTo = predicates.To(factory);

            return factory.And(stringsTo, predicatesTo);
        }

        public object Clone()
        {
            return new StringAbstractDomain<Variable, Expression, StringAbstraction>(
              decoder, operations,
              (SimpleFunctionalAbstractDomain<Variable, StringAbstraction>)strings.Clone(),
              (SimpleFunctionalAbstractDomain<Variable, IStringPredicate>)predicates.Clone(),
              testVisitor
              );
        }


        #endregion

    }
}
