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

// Modified by Vlastimil Dort (2016)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.CodeAnalysis;
using Microsoft.Research.DataStructures;
using Microsoft.Research.AbstractDomains.Expressions;
using System.Diagnostics;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Abstract domain element from the "string pentagons" domain, adapting the idea of Pentagons
    /// to string values.
    /// </summary>
    /// <typeparam name="Variable"></typeparam>
    /// <typeparam name="Expression"></typeparam>
    /// <typeparam name="StringAbstraction">Interval abstraction of strings.</typeparam>
    public class StringPentagons<Variable, Expression, StringAbstraction> :
      StringAbstractDomain<Variable, Expression, StringAbstraction>,
      IStringOrderQuery<Variable>

      where StringAbstraction : class, IStringInterval<StringAbstraction>
      where Variable : class, IEquatable<Variable>
      where Expression : class
    {
        private SimpleFunctionalAbstractDomain<Variable, SetOfConstraints<Variable>> upperBounds;

        private IStringIntervalOperations<StringAbstraction, Variable> IntervalOperations
        {
            get { return (IStringIntervalOperations<StringAbstraction, Variable>)operations; }
        }

        private StringPentagons(
          IExpressionDecoder<Variable, Expression> decoder,
          IStringIntervalOperations<StringAbstraction, Variable> operations,
          SimpleFunctionalAbstractDomain<Variable, StringAbstraction> intervals,
          SimpleFunctionalAbstractDomain<Variable, SetOfConstraints<Variable>> upperBounds,
          SimpleFunctionalAbstractDomain<Variable, IStringPredicate> predicates,
          StringSimpleTestVisitor testVisitor
          ) :  base(decoder, operations, intervals, predicates, testVisitor)
        {
            this.upperBounds = upperBounds;
        }

        public StringPentagons(IExpressionDecoder<Variable, Expression> decoder, IStringIntervalOperations<StringAbstraction, Variable> operations)
            : base(decoder, operations)
        {
            this.upperBounds = new SimpleFunctionalAbstractDomain<Variable, SetOfConstraints<Variable>>();
        }

        public override bool IsBottom
        {
            get
            {
                return base.IsBottom || upperBounds.IsBottom;
            }
        }

        public override bool IsTop
        {
            get
            {
                return base.IsTop && upperBounds.IsTop;
            }
        }

        public override IAbstractDomain Bottom
        {
            get
            {
                return new StringPentagons<Variable, Expression, StringAbstraction>(
                  decoder, IntervalOperations, strings.Bottom, upperBounds.Bottom, predicates.Bottom, testVisitor
                  );
            }
        }

        public override IAbstractDomain Top
        {
            get
            {
                return new StringPentagons<Variable, Expression, StringAbstraction>(
                  decoder, IntervalOperations, strings.Top, upperBounds.Top, predicates.Top, testVisitor
                  );
            }
        }

        public override List<Variable> Variables
        {
            get
            {
                var l = new List<Variable>(strings.Keys);
                l.AddRange(upperBounds.Keys);
                l.AddRange(predicates.Keys);
                return l;
            }
        }

        #region String operations

        public override void Empty(Expression targetExp)
        {
            StringAbstraction targetAbstraction = operations.Constant("");
            AssignTargetStringAbstraction(targetExp, targetAbstraction);
        }

        public override void Copy(Expression targetExp, Expression sourceExp)
        {

            Variable targetVariable = decoder.UnderlyingVariable(targetExp);
            Variable sourceVariable = decoder.UnderlyingVariable(sourceExp);

            if (targetVariable != null && sourceVariable != null)
            {
                TestTrueLessEqualThan(targetVariable, sourceVariable);
                TestTrueLessEqualThan(sourceVariable, targetVariable);
            }
            base.Copy(targetExp, sourceExp);
        }

        public override void Concat(Expression targetExp, Expression leftExp, Expression rightExp)
        {
            Variable leftVariable, rightVariable;
            WithConstants<StringAbstraction> leftAbstraction = EvalStringArgument(leftExp, out leftVariable, NullHandling.Empty);
            WithConstants<StringAbstraction> rightAbstraction = EvalStringArgument(rightExp, out rightVariable, NullHandling.Empty);
            Variable targetVariable = decoder.UnderlyingVariable(targetExp);

            // Interval concatenation
            StringAbstraction targetAbstraction = operations.Concat(leftAbstraction, rightAbstraction);

            AssignTargetStringAbstraction(targetVariable, targetAbstraction);

            // Get predicates about variables
            foreach (var order in IntervalOperations.ConcatOrder(targetVariable, leftVariable, rightVariable))
            {
                TestTruePredicate(order);
            }
        }

        public override void SubstringRemove(Expression targetExp, Expression valueExp, Expression indexExp, Expression lengthExp, bool remove, INumericalAbstractDomain<Variable, Expression> numericalDomain)
        {
            base.SubstringRemove(targetExp, valueExp, indexExp, lengthExp, remove, numericalDomain);

            Variable targetVariable = decoder.UnderlyingVariable(targetExp);
            Variable sourceVariable = decoder.UnderlyingVariable(valueExp);

            if (targetVariable != null && sourceVariable != null)
            {
                IndexInterval indexInterval = EvalIndexArgumentInterval(indexExp, numericalDomain);
                IndexInterval lengthInterval = EvalLengthArgumentInterval(lengthExp, numericalDomain);

                

                // Get predicates about variables
                foreach (var order in IntervalOperations.SubstringRemoveOrder(targetVariable, sourceVariable, indexInterval, lengthInterval, remove))
                {
                    TestTruePredicate(order);
                }
            }
        }

        public override void Contains(Expression targetExp, Expression valueExp, Expression partExp)
        {
            Variable partVar, valueVar;
            WithConstants<StringAbstraction> partAbstraction = EvalStringArgument(partExp, out partVar, NullHandling.Exception);
            WithConstants<StringAbstraction> valueAbstraction = EvalStringArgument(valueExp, out valueVar, NullHandling.Exception);

            IStringPredicate targetPredicate;

            if (partAbstraction.IsConstant && valueAbstraction.IsConstant)
            {
                bool result;
                result = valueAbstraction.Constant.Contains(partAbstraction.Constant);

                targetPredicate = new FlatPredicate(result);
            }
            else if (partAbstraction.IsBottom || valueAbstraction.IsBottom)
            {
                targetPredicate = FlatPredicate.Bottom;
            }
            else
            {
                targetPredicate = IntervalOperations.Contains(valueAbstraction, valueVar, partAbstraction, partVar, this);
            }

            AssignPredicate(targetExp, targetPredicate);
        }

        public override void StartsEndsWith(Expression targetExp, Expression valueExp, Expression partExp, Expression comparisonExp, bool end)
        {
            // Check that the comparison type is Ordinal. Otherwise, nothing is known.
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
                    if (end)
                        result = valueAbstraction.Constant.EndsWith(partAbstraction.Constant, StringComparison.Ordinal);
                    else
                        result = valueAbstraction.Constant.StartsWith(partAbstraction.Constant, StringComparison.Ordinal);

                    targetPredicate = new FlatPredicate(result);
                }
                else if (partAbstraction.IsBottom || valueAbstraction.IsBottom)
                {
                    targetPredicate = FlatPredicate.Bottom;
                }
                else
                {
                    targetPredicate = IntervalOperations.StartsEndsWithOrdinal(valueAbstraction, valueVar, partAbstraction, partVar, end, this);
                }

                AssignPredicate(targetExp, targetPredicate);
            }
            else
            {
                UnassignPredicate(targetExp);
            }
        }

        #endregion

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

                foreach (var x in strings.Keys.Union(upperBounds.Keys))
                {
                    string xAsString = decoder != null ? decoder.NameOf(x) : x.ToString();
                    tempStr.Append(xAsString);
                    if (strings.ContainsKey(x))
                    {
                        tempStr.Append(": " + strings[x]);
                    }
                    if (upperBounds.ContainsKey(x))
                    {
                        tempStr.Append("<=");
                        foreach (var y in upperBounds[x].Values)
                        {
                            tempStr.Append(" ");
                            tempStr.Append(decoder.NameOf(y));
                        }
                    }

                    tempStr.Append(", ");
                }



                foreach (var x in predicates.Keys)
                {
                    string xAsString = decoder != null ? decoder.NameOf(x) : x.ToString();
                    tempStr.Append(xAsString + ": " + predicates[x] + ", ");
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

        public override bool LessEqual(IAbstractDomain a)
        {
            var right = (StringPentagons<Variable, Expression, StringAbstraction>)a;

            return strings.LessEqual(right.strings) && upperBounds.LessEqual(right.upperBounds) && predicates.LessEqual(right.predicates);
        }


        private StringPentagons<Variable, Expression, StringAbstraction> Join(StringPentagons<Variable, Expression, StringAbstraction> right)
        {
            // Here we do not have trivial joins as we want to join maps of different cardinality
            if (IsBottom)
                return right;
            if (right.IsBottom)
                return this;

            var joinUpperBounds = new SimpleFunctionalAbstractDomain<Variable, SetOfConstraints<Variable>>();

            foreach (var pair in upperBounds.Elements)       // For all the elements in the intersection do the point-wise join
            {
                var intersection = new SetOfConstraints<Variable>(new Set<Variable>(), false);
                var newValues = new Set<Variable>();

                SetOfConstraints<Variable> right_x;
                if (right.upperBounds.TryGetValue(pair.Key, out right_x))
                {
                    intersection = pair.Value.Join(right_x);
                    if (!intersection.IsTop)
                    {
                        newValues.AddRange(intersection.Values);
                    }

                    // Foreach x <= y
                    if (right_x.IsNormal())
                    {
                        foreach (var y in right_x.Values)
                        {
                            if (newValues.Contains(y))
                            {
                                continue;
                            }

                            if (CheckMustBeLessEqualThan(pair.Key, y))    // If the intervals imply this relation, just keep it
                            {
                                newValues.Add(y);   // Add x <= y
                            }
                        }
                    }
                }

                // Foreach x <= y
                if (pair.Value.IsNormal())
                {
                    foreach (var y in pair.Value.Values)
                    {
                        if (newValues.Contains(y))
                        {
                            continue;
                        }

                        if (right.CheckMustBeLessEqualThan(pair.Key, y))    // If the intervals imply this relation, just keep it
                        {
                            newValues.Add(y);   // Add x <= y everywhere
                            right.TestTrueLessEqualThan(pair.Key, y);
                        }
                    }
                }

                if (!newValues.IsEmpty)
                {
                    joinUpperBounds[pair.Key] = new SetOfConstraints<Variable>(newValues, false);
                }
            }

            foreach (var x_pair in right.upperBounds.Elements)
            {
                if (upperBounds.ContainsKey(x_pair.Key))
                {
                    // Case already handled
                    continue;
                }

                var newValues = new Set<Variable>();

                // Foreach x <= y
                if (x_pair.Value.IsNormal())
                {
                    foreach (var y in x_pair.Value.Values)
                    {
                        if (CheckMustBeLessEqualThan(x_pair.Key, y))    // If the intervals imply this relation, just keep it
                        {
                            newValues.Add(y);   // Add x <= y
                            TestTrueLessEqualThan(x_pair.Key, y);
                        }
                    }

                    if (!newValues.IsEmpty)
                    {
                        joinUpperBounds[x_pair.Key] = new SetOfConstraints<Variable>(newValues, false);
                    }
                }
            }

            var joinIntervals = strings.Join(right.strings);
            var joinPredicates = predicates.Join(right.predicates);

            return new StringPentagons<Variable, Expression, StringAbstraction>(decoder, IntervalOperations, joinIntervals, joinUpperBounds, joinPredicates, testVisitor);
        }
        public override IAbstractDomain Join(IAbstractDomain a)
        {
            return Join((StringPentagons<Variable, Expression, StringAbstraction>)a);
        }

        public override IAbstractDomain Meet(IAbstractDomain a)
        {
            var right = (StringPentagons<Variable, Expression, StringAbstraction>)a;

            var meetIntervals = strings.Meet(right.strings);
            var meetUpperBounds = upperBounds.Meet(right.upperBounds);
            var meetPredicates = predicates.Meet(right.predicates);

            return new StringPentagons<Variable, Expression, StringAbstraction>(decoder, IntervalOperations, meetIntervals, meetUpperBounds, meetPredicates, testVisitor);
        }

        public override IAbstractDomain Widening(IAbstractDomain prev)
        {
            var right = (StringPentagons<Variable, Expression, StringAbstraction>)prev;

            var widenIntervals = strings.Widening(right.strings);
            var widenUpperBounds = upperBounds.Widening(right.upperBounds);
            var widenPredicates = predicates.Widening(right.predicates);

            return new StringPentagons<Variable, Expression, StringAbstraction>(decoder, IntervalOperations, widenIntervals, widenUpperBounds, widenPredicates, testVisitor);
        }

        public override T To<T>(IFactory<T> factory)
        {
            var upperBoundsTo = upperBounds.To(factory);

            return factory.And(base.To(factory), upperBoundsTo);
        }

        public override object Clone()
        {
            return new StringPentagons<Variable, Expression, StringAbstraction>(
              decoder,
              IntervalOperations,
              (SimpleFunctionalAbstractDomain<Variable, StringAbstraction>)strings.Clone(),
              (SimpleFunctionalAbstractDomain<Variable, SetOfConstraints<Variable>>)upperBounds.Clone(),
              (SimpleFunctionalAbstractDomain<Variable, IStringPredicate>)predicates.Clone(),
              testVisitor
              );
        }

        public override void Assign(Expression x, Expression exp)
        {
            Variable variable = this.decoder.UnderlyingVariable(x);
            if (variable != null)
            {
                strings.ResetToNormal();
                strings[variable] = EvalInterval(exp);
                upperBounds.ResetToNormal();
                upperBounds[variable] = EvalConstraints(exp);
                predicates.ResetToNormal();
                predicates[variable] = EvalBoolExpression(exp);
            }
        }


        public override void RemoveVariable(Variable var)
        {
            base.RemoveVariable(var);
            RemoveFromBounds(var);
        }

        protected override void DoVariableRenaming(Variable oldName, Variable newName)
        {
            // rename in strings and predicates
            base.DoVariableRenaming(oldName, newName);

            // rename in bounds
            foreach (var key in upperBounds.Keys.ToArray()) // Keys stored in array because the collection is modified
            {
                Variable newKey;

                if (key.Equals(oldName)) {
                    newKey = newName;
                }
                else
                {
                    newKey = key;
                }

                SetOfConstraints<Variable> newValue = upperBounds[key];

                if (newValue.Contains(oldName))
                    newValue = newValue.Add(newName);

                upperBounds[newKey] = newValue;
            }
        }

        private void TestTruePredicate(OrderPredicate<Variable> orderPredicate)
        {
            foreach (Variable geqVarianble in orderPredicate.GreaterEqualVariables.Values)
            {
                TestTrueLessEqualThan(orderPredicate.LessEqualVariable, geqVarianble);
            }
        }

        protected override StringAbstractDomain<Variable, Expression, StringAbstraction>/*!*/ Test(Variable assumedVariable, bool holds)
        {

            // We must create a copy of the domain because the test visitor assumes that (see Not-LogicalAnd)
            // If this changes, this method might be simplified to just mutate this
            StringPentagons<Variable, Expression, StringAbstraction> mutable = new StringPentagons<Variable, Expression, StringAbstraction>(decoder, IntervalOperations, strings, upperBounds, predicates, testVisitor);

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
                else if (predicate is OrderPredicate<Variable>)
                {
                    mutable.TestTruePredicate(predicate as OrderPredicate<Variable>);

                }
                else if (!predicate.ContainsValue(holds))
                {
                    // The known information contradicts, so we are unreachable
                    return (StringPentagons<Variable, Expression, StringAbstraction>)Bottom;
                    // Could be also done as Meet on FlatPredicate
                }
                //else remains
            }

            //Change the known information
            mutable.predicates[assumedVariable] = new FlatPredicate(holds);

            return mutable;
        }

        private void TestTrueLessEqualThan(Variable leftVariable, Variable rightVariable)
        {
            // First, intervals

            StringAbstraction leftInterval = EvalInterval(leftVariable);
            StringAbstraction rightInterval = EvalInterval(rightVariable);

            if (leftInterval.TryRefineGreaterEqual(ref rightInterval))
            {
                strings[rightVariable] = rightInterval;
            }

            if (rightInterval.TryRefineLessEqual(ref leftInterval))
            {
                strings[leftVariable] = leftInterval;
            }

            // Second, upperBounds

            // add rightVariable ( and transitively all the constraints of rightVariable) to the constraints of leftVariable

            var newValues = new Set<Variable>();
            newValues.Add(rightVariable);

            SetOfConstraints<Variable> leftConstraints;
            if (upperBounds.TryGetValue(leftVariable, out leftConstraints))
                newValues.AddRange(leftConstraints.Values);
            SetOfConstraints<Variable> rightConstraints;
            if (upperBounds.TryGetValue(rightVariable, out rightConstraints))
                newValues.AddRange(rightConstraints.Values);

            var newConstraints = new SetOfConstraints<Variable>(newValues, false);
            upperBounds[leftVariable] = newConstraints;

            // and backwards for each constraint with leftVariable, add also rightVariable
            if (newConstraints.IsNormal())
            {
                //Copied from WeakUpperBounds
                var toBeUpdated = new List<Pair<Variable, SetOfConstraints<Variable>>>();

                // "e1 < e2", so we search all the variables to see if "e0 < e1"
                foreach (var pair in upperBounds.Elements)
                {
                    if (pair.Value.IsNormal() && pair.Value.Contains(leftVariable))
                    {
                        var values = new Set<Variable>(pair.Value.Values);
                        values.AddRange(newValues);

                        toBeUpdated.Add(pair.Key, new SetOfConstraints<Variable>(values, false));
                    }
                }

                if (toBeUpdated.Count > 0)
                {
                    foreach (var pair in toBeUpdated)
                    {
                        upperBounds[pair.One] = pair.Two;
                    }
                }
            }

        }

        public bool CheckMustBeLessEqualThan(Variable leftVariable, Variable rightVariable)
        {
            StringAbstraction leftInterval = EvalInterval(leftVariable);
            StringAbstraction rightInterval = EvalInterval(rightVariable);

            if (leftInterval.CheckMustBeLessEqualThan(rightInterval))
                return true;

            SetOfConstraints<Variable> leftUpperBounds;

            if (upperBounds.TryGetValue(leftVariable, out leftUpperBounds) && leftUpperBounds.IsNormal())
            {
                if (leftUpperBounds.Contains(rightVariable))
                    return true;
            }

            return false;
        }

        public void AssumeDomainSpecificFact(DomainSpecificFact fact)
        {
            // Do nothing
        }


        static internal void AddUpperBound(Variable key, Variable upperBound, Dictionary<Variable, Set<Variable>> map)
        {

            Set<Variable> bounds;
            if (!map.TryGetValue(key, out bounds))
            {
                bounds = new Set<Variable>();
                map[key] = bounds;
            }

            bounds.Add(upperBound);
        }

        public override void AssignInParallel(Dictionary<Variable, FList<Variable>> sourcesToTargets, Converter<Variable, Expression> convert)
        {
            strings.ResetToNormal();
            upperBounds.ResetToNormal();
            predicates.ResetToNormal();

            // First do upper bounds (oracle is not really used)
            var oldToNewMap = new Dictionary<Variable, FList<Variable>>(sourcesToTargets);
            // when x has several targets including itself, the canonical element shouldn't be itself
            foreach (var sourceToTargets in sourcesToTargets)
            {
                var source = sourceToTargets.Key;
                var targets = sourceToTargets.Value;
                if (targets.Length() > 1 && targets.Head.Equals(source))
                {
                    var newTargets = FList<Variable>.Cons(targets.Tail.Head, FList<Variable>.Cons(source, targets.Tail.Tail));
                    oldToNewMap[source] = newTargets;
                }
            }

            var newMappings = new Dictionary<Variable, Set<Variable>>(upperBounds.Count);

            foreach (var oldLeft_Pair in upperBounds.Elements)
            {
                if (!oldToNewMap.ContainsKey(oldLeft_Pair.Key))
                {
                    continue;
                }

                var targets = oldToNewMap[oldLeft_Pair.Key];
                var newLeft = targets.Head; // our canonical element

                var oldBounds = oldLeft_Pair.Value;
                if (!oldBounds.IsNormal())
                {
                    continue;
                }

                foreach (var oldRight in oldBounds.Values)
                {
                    if (!oldToNewMap.ContainsKey(oldRight))
                    {
                        continue;
                    }

                    var newRight = oldToNewMap[oldRight].Head; // our canonical element
                    AddUpperBound(newLeft, newRight, newMappings);
                }
            }

            // Precision improvements:
            //
            // Consider:
            //   if (x < y) x = y;
            //   Debug.Assert(x >= y);
            //
            // This is an example where at the end of the then branch, we have a single old variable being assigned to new new variables:
            //   x := y'  and y := y'
            // Since in this branch, we obviously have y' => y', the new renamed state should have y => x and x => y. That way, at the join,
            // the constraint x >= y is retained.
            // 
            foreach (var pair in sourcesToTargets)
            {
                var targets = pair.Value;
                var newCanonical = targets.Head;
                targets = targets.Tail;
                while (targets != null)
                {
                    // make all other targets equal to canonical (rather than n^2 combinations)
                    AddUpperBound(newCanonical, targets.Head, newMappings);
                    AddUpperBound(targets.Head, newCanonical, newMappings);
                    targets = targets.Tail;
                }
            }

            // now clear the current state
            upperBounds.ClearElements();

            // now add the new mappings
            foreach (var key in newMappings.Keys)
            {
                var bounds = newMappings[key];

                if (bounds.Count == 0)
                    continue;

                var newBoundsFromClosure = new Set<Variable>();

                foreach (var upp in bounds)
                {
                    if (!upp.Equals(key) && newMappings.ContainsKey(upp))
                    {
                        newBoundsFromClosure.AddRange(newMappings[upp]);
                    }
                }

                bounds.AddRange(newBoundsFromClosure);

                upperBounds.AddElement(key, new SetOfConstraints<Variable>(bounds, false));
            }



            // Then do intervals
            var values = new Dictionary<Variable, StringAbstraction>();

            foreach (var exp in sourcesToTargets.Keys)
            {
                var value = EvalInterval(convert(exp));
                if (!value.IsTop)
                {
                    foreach (var target in sourcesToTargets[exp].GetEnumerable())
                    {
                        values[target] = value;
                    }
                }
            }

            strings.SetElements(values);

            var rvalues = new Dictionary<Variable, IStringPredicate>();

            foreach (var exp in sourcesToTargets.Keys)
            {
                var value = EvalBoolExpression(convert(exp));
                var value_a = value.AssignInParallel(oldToNewMap);
                if (!value_a.IsTop)
                {
                    foreach (var target in sourcesToTargets[exp].GetEnumerable())
                    {
                        rvalues[target] = value_a;
                    }
                }
            }

            predicates.SetElements(rvalues);

            // Add equivalence predicate

        }

        #region Eval
        private StringAbstraction EvalInterval(Expression exp)
        {
            string constant;
            if (decoder.TypeOf(exp) == ExpressionType.String && decoder.TryValueOf(exp, ExpressionType.String, out constant))
            {
                return operations.Constant(constant);
            }
            else
                return EvalInterval(decoder.UnderlyingVariable(exp));
        }

        private StringAbstraction EvalInterval(Variable v)
        {
            StringAbstraction abs;
            if (v == null || !strings.TryGetValue(v, out abs))
            {
                abs = operations.Top;
            }

            return abs;
        }

        private SetOfConstraints<Variable> EvalConstraints(Expression exp)
        {
            Variable variable = decoder.UnderlyingVariable(exp);
            SetOfConstraints<Variable> s;
            if(variable != null && upperBounds.TryGetValue(variable, out s))
            {
                return s;
            }
            else
            {
                return SetOfConstraints<Variable>.Unknown;
            }
        }


        private IStringPredicate EvalBoolExpression(Expression exp)
        {
            bool constant;
            if (decoder.TypeOf(exp) == ExpressionType.Bool && decoder.TryValueOf(exp, ExpressionType.Bool, out constant))
            {
                return new FlatPredicate(constant);
            }
            else
                return EvalBoolVariable(decoder.UnderlyingVariable(exp));
        }

        private IStringPredicate EvalBoolVariable(Variable variable)
        {
            if (variable != null && predicates.ContainsKey(variable))
            {
                return predicates[variable];
            }
            else
            {
                return FlatPredicate.Top;
            }
        }
        private bool TryEvalIntConstant(Expression exp, out int val)
        {
            return this.decoder.IsConstantInt(exp, out val);
        }
        private WithConstants<StringAbstraction> EvalStringArgument(Expression expression, out Variable variable, NullHandling nullHandling)
        {
            Debug.Assert(expression != null);

            switch (this.decoder.OperatorFor(expression))
            {
                case ExpressionOperator.Constant:
                    string constant;

                    decoder.TryValueOf<string>(expression, ExpressionType.String, out constant);
                    // Constant strings are returned as constants
                    if (decoder.TypeOf(expression) == ExpressionType.String && decoder.TryValueOf<string>(expression, ExpressionType.String, out constant))
                    {
                        variable = null;
                        return new WithConstants<StringAbstraction>(constant);
                    }
                    else if (decoder.IsNull(expression))
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
                    variable = decoder.UnderlyingVariable(expression);
                    if (strings.ContainsKey(variable))
                    {
                        return new WithConstants<StringAbstraction>(strings[variable]);
                    }
                    else
                    {
                        return new WithConstants<StringAbstraction>(operations.Top);
                    }
            }
            variable = null;
            return new WithConstants<StringAbstraction>(operations.Top);
        }

        #endregion

        #region Assign

        private void RemoveFromBounds(Variable var)
        {
            var toUpdate = new Dictionary<Variable, SetOfConstraints<Variable>>(upperBounds.Count);

            var forVar = new Set<Variable>();

            SetOfConstraints<Variable> value;
            if (upperBounds.TryGetValue(var, out value) && value.IsNormal())
            {
                forVar = new Set<Variable>(value.Values);
            }

            foreach (var x_Pair in upperBounds.Elements)
            {
                var constraints = x_Pair.Value;

                if (!constraints.IsNormal() || !constraints.Contains(var))
                {
                    continue;
                }

                var newConstraints = new Set<Variable>(constraints.Values);
                newConstraints.AddRange(forVar);
                newConstraints.Remove(var);

                toUpdate[x_Pair.Key] = new SetOfConstraints<Variable>(newConstraints);
            }

            foreach (var x_pair in toUpdate)
            {
                upperBounds[x_pair.Key] = x_pair.Value;
            }

            upperBounds.RemoveElement(var);
        }

        private void UnassignPredicate(Expression target)
        {
            predicates.RemoveElement(decoder.UnderlyingVariable(target));
        }
        private void AssignPredicate(Expression target, IStringPredicate targetPredicate)
        {
            Debug.Assert(targetPredicate != null);
            predicates[decoder.UnderlyingVariable(target)] = targetPredicate;
        }
        #endregion

        public override IEnumerable<StringRelation<Variable>> RelationsForVariable(Variable var)
        {
            SetOfConstraints<Variable> vars;
            if(upperBounds.TryGetValue(var, out vars))
            {
                // TODO: implement inference for other orderings
                if (typeof(StringAbstraction) == typeof(PrefixInterval))
                {
                    return vars.Values.Select(relVar => new StringRelation<Variable> { Operator = ExpressionOperator.StartsWith, RelatedVariable = relVar });
                }
            }

            return Enumerable.Empty<StringRelation<Variable>>();
        }
    }


}
