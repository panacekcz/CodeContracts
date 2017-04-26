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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics.Contracts;
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.DataStructures;

namespace Microsoft.Research.CodeAnalysis
{
  public static partial class AnalysisWrapper
  {
    public static partial class TypeBindings<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>
      where Variable : IEquatable<Variable>
      where Expression : IEquatable<Expression>
      where Type : IEquatable<Type>
    {
      private class ClassStringWrapperPluginFactory :
      IMethodAnalysisClientFactory<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, GenericPlugInAnalysisForComposedAnalysis>
      {
        private readonly int id;
        private readonly string methodName;
        private readonly IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ILogOptions> mdriver;
        private readonly ILogOptions options;
        private readonly Predicate<APC> cachePCs;

        public ClassStringWrapperPluginFactory(int id, string methodName, IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ILogOptions> mdriver, ILogOptions options, Predicate<APC> cachePCs)
        {
          this.id = id;
          this.methodName = methodName;
          this.mdriver = mdriver;
          this.options = options;
          this.cachePCs = cachePCs;
        }

        public GenericPlugInAnalysisForComposedAnalysis Create<AState>(IAbstractAnalysis<Local, Parameter, Method, Field, Property, Type, Expression, Attribute, Assembly, AState, Variable> analysis, DFAController controller)
        {
          //VD: Ignoring the controller parameter
          return new StringWrapperPlugIn((StringValueAnalysis)analysis, id, methodName, mdriver, options, cachePCs);
        }
      }


      class StringWrapperPlugIn : GenericPlugInAnalysisForComposedAnalysis
      {
        public static GenericPlugInAnalysisForComposedAnalysis Create(IMethodAnalysis strAnalysis, int id, string methodName, IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ILogOptions> mdriver, ILogOptions options, Predicate<APC> cachePCs)
        {
          //VD: using null as controller, will be ignored in Create method above
          return strAnalysis.Instantiate(methodName, mdriver, cachePCs, new ClassStringWrapperPluginFactory(id, methodName, mdriver, options, cachePCs), null);
        }

        private readonly StringValueAnalysis stringAnalysis;

        public StringWrapperPlugIn(StringValueAnalysis stringAnalysis, int id, string methodName,
          IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ILogOptions> mdriver,
          ILogOptions options, Predicate<APC> cachePCs)
          : base(id, methodName, mdriver, new PlugInAnalysisOptions(options), cachePCs)
        {
          Contract.Requires(stringAnalysis != null);

          this.stringAnalysis = stringAnalysis;
        }

        private IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> Select(ArrayState state)
        {
          return (IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>)state.PluginAbstractStateAt(this.Id);
        }
        private ArrayState MakeState(IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> next, ArrayState old)
        {
          return old.UpdatePluginAt(this.Id, next);
        }
        public override ArrayState Entry(APC pc, Method method, ArrayState data)
        {
          return MakeState(this.stringAnalysis.Entry(pc, method, Select(data)), data);
        }

        public override ArrayState Assume(APC pc, string tag, Variable source, object provenance, ArrayState data)
        {
          var newData = data;
          var newSubState = this.stringAnalysis.Assume(pc, tag, source, provenance, Select(data));

          return MakeState(newSubState, newData);
        }

        public override ArrayState Assert(APC pc, string tag, Variable condition, object provenance, ArrayState data)
        {
          return MakeState(this.stringAnalysis.Assert(pc, tag, condition, provenance, Select(data)), data);
        }



        public override ArrayState Stelem(APC pc, Type type, Variable array, Variable index, Variable value, ArrayState data)
        {
          return MakeState(this.stringAnalysis.Stelem(pc, type, array, index, value, Select(data)), data);
        }

        public override ArrayState Ldelem(APC pc, Type type, Variable dest, Variable array, Variable index, ArrayState data)
        {
          return MakeState(this.stringAnalysis.LdelemWithNumerical(pc, type, dest, array, index, Select(data), data.Numerical), data);
        }

        public override ArrayState Starg(APC pc, Parameter argument, Variable source, ArrayState data)
        {
          return MakeState(this.stringAnalysis.Starg(pc, argument, source, Select(data)), data);
        }

        public override ArrayState Ldarg(APC pc, Parameter argument, bool isOld, Variable dest, ArrayState data)
        {
          return MakeState(this.stringAnalysis.Ldarg(pc, argument, isOld, dest, Select(data)), data);
        }

        public override ArrayState Stind(APC pc, Type type, bool @volatile, Variable ptr, Variable value, ArrayState data)
        {
          return MakeState(this.stringAnalysis.Stind(pc, type, @volatile, ptr, value, Select(data)), data);
        }

        public override ArrayState Ldind(APC pc, Type type, bool @volatile, Variable dest, Variable ptr, ArrayState data)
        {
          return MakeState(this.stringAnalysis.Ldind(pc, type, @volatile, dest, ptr, Select(data)), data);
        }

        public override ArrayState Stloc(APC pc, Local local, Variable source, ArrayState data)
        {
          return MakeState(this.stringAnalysis.Stloc(pc, local, source, Select(data)), data);
        }

        public override ArrayState Ldloc(APC pc, Local local, Variable dest, ArrayState data)
        {
          return MakeState(this.stringAnalysis.Ldloc(pc, local, dest, Select(data)), data);
        }

        public override ArrayState Stfld(APC pc, Field field, bool @volatile, Variable obj, Variable value, ArrayState data)
        {
          return MakeState(this.stringAnalysis.Stfld(pc, field, @volatile, obj, value, Select(data)), data);
        }

        public override ArrayState Ldfld(APC pc, Field field, bool @volatile, Variable dest, Variable obj, ArrayState data)
        {
          return MakeState(this.stringAnalysis.Ldfld(pc, field, @volatile, dest, obj, Select(data)), data);
        }


        public override ArrayState Call<TypeList, ArgList>(APC pc, Method method, bool tail, bool virt, TypeList extraVarargs, Variable dest, ArgList args, ArrayState data)
        {
          var domain = Select(data);
          var numerical = data.Numerical;

          var next = stringAnalysis.CallWithNumerical<TypeList, ArgList>(pc, method, tail, virt, extraVarargs, dest, args, domain, numerical, new NullQuery(data));

          return MakeState(next, data);
        }

        public override AbstractDomains.IAbstractDomainForEnvironments<BoxedVariable<Variable>, BoxedExpression> InitialState
        {
          get { return this.stringAnalysis.GetTopValue(); }
        }

        public override ArrayState.AdditionalStates Kind
        {
          get { return ArrayState.AdditionalStates.String; }
        }

        public override AbstractDomains.IAbstractDomainForEnvironments<BoxedVariable<Variable>, BoxedExpression> AssignInParallel(Dictionary<BoxedVariable<Variable>, DataStructures.FList<BoxedVariable<Variable>>> refinedMap, Converter<BoxedVariable<Variable>, BoxedExpression> convert, List<DataStructures.Pair<AbstractDomains.NormalizedExpression<BoxedVariable<Variable>>, AbstractDomains.NormalizedExpression<BoxedVariable<Variable>>>> equalities, ArrayState state)
        {
          var substate = Select(state);
          substate.AssignInParallel(refinedMap, convert);
          return substate;
        }


        public override bool SuggestAnalysisSpecificPostconditions(
            ContractInferenceManager inferenceManager,
            IFixpointInfo<APC, ArrayState> fixpointInfo,
            List<BoxedExpression> postconditions)
        {
            // The string analysis may use non-null information to provide better postconditions
            return stringAnalysis.SuggestAnalysisSpecificPostconditions(inferenceManager, new FixPointInfoProjectionOnStringState(this, fixpointInfo), new FixpointInfoProjectionOnNullQuery(this, fixpointInfo), postconditions);
        }

        public override IFactQuery<BoxedExpression, Variable> FactQuery(IFixpointInfo<APC, ArrayState> fixpoint)
        {
          return this.stringAnalysis.FactQuery(new FixPointInfoProjectionOnStringState(this, fixpoint));
        }
        public class FixPointInfoProjectionOnStringState
          : FixPointInfoProjection<ArrayState, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>>
        {
          readonly StringWrapperPlugIn analysis;

          public FixPointInfoProjectionOnStringState(StringWrapperPlugIn arrayAnalysis, IFixpointInfo<APC, ArrayState> fixpoint)
            : base(fixpoint)
          {
            Contract.Requires(arrayAnalysis != null);
            Contract.Requires(fixpoint != null);

            this.analysis = arrayAnalysis;
          }

          protected override ArrayState InitialValue
          {
            get
            {
              var result = this.analysis.GetTopValue();
              Contract.Assume(result != null);

              return result;
            }
          }

          protected override ArrayState
            MakeProductState(
            IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> init,
            ArrayState top)
          {
            Contract.Assume(init != null);

            return analysis.MakeState(init, top);
          }

          protected override IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> Project(ArrayState productAD)
          {
            return analysis.Select(productAD);
          }
        }


        private class NullQuery : INullQuery<BoxedVariable<Variable>>
        {
          private readonly ArrayState state;

          public NullQuery(ArrayState state)
          {
            this.state = state;
          }

          public bool IsNull(BoxedVariable<Variable> boxedVariable)
          {
            Variable variable;
            if (state.HasNonNullInfo && boxedVariable.TryUnpackVariable(out variable))
            {
              return state.NonNull.IsNull(variable);
            }
            else
            {
              return false;
            }
          }

          public bool IsNonNull(BoxedVariable<Variable> boxedVariable)
          {
            Variable variable;
            if (state.HasNonNullInfo && boxedVariable.TryUnpackVariable(out variable))
            {
              return state.NonNull.IsNonNull(variable);
            }
            else
            {
              return false;
            }
          }
        }

        public class FixpointInfoProjectionOnNullQuery
        : FixPointInfoProjection<ArrayState, INullQuery<BoxedVariable<Variable>>>
                {
                    private readonly StringWrapperPlugIn analysis;

                    public FixpointInfoProjectionOnNullQuery(StringWrapperPlugIn analysis, IFixpointInfo<APC, ArrayState> fixpoint)
                      : base(fixpoint)
                    {
                        Contract.Requires(analysis != null);
                        Contract.Requires(fixpoint != null);

                        this.analysis = analysis;
                    }

                    protected override ArrayState
                      InitialValue
                    {
                        get
                        {
                            var result = this.analysis.GetTopValue();
                            // Contract.Assume(result != null);

                            return result;
                        }
                    }

                    protected override ArrayState
                      MakeProductState(
                      INullQuery<BoxedVariable<Variable>> init,
                      ArrayState top)
                    {
                        // Not needed
                        throw new NotImplementedException();
                    }

                    protected override INullQuery<BoxedVariable<Variable>>
                      Project(ArrayState productAD)
                    {
                        return new NullQuery(productAD);
                    }
                }

            }

    }
  }
}
