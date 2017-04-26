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
using Microsoft.Research.AbstractDomains.Strings;
using Microsoft.Research.DataStructures;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;

namespace Microsoft.Research.CodeAnalysis
{


    public static partial class AnalysisWrapper
    {
        public static IMethodResult<Variable> AnalyzeStrings<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>
        (
          string methodName,
          IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ILogOptions> driver, Analyzers.Strings.StringOptions options,
          Predicate<APC> cachePCs, DFAController controller
        )
          where Variable : IEquatable<Variable>
          where Expression : IEquatable<Expression>
          where Type : IEquatable<Type>
        {
            // We call the helper as a syntactic convenience, as there are too many type parameters!
            return TypeBindings<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>.HelperForStringAnalysis(methodName, driver, options, cachePCs, controller);
        }

        public static partial class TypeBindings<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable>
          where Variable : IEquatable<Variable>
          where Expression : IEquatable<Expression>
          where Type : IEquatable<Type>
        {

            internal interface IStringAbstractDomainFactory
            {
                IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> CreateTopValue(BoxedExpressionDecoder<Variable> decoder);
            }

            internal class StringAbstractDomainFactory<StringAbstraction> : IStringAbstractDomainFactory
              where StringAbstraction : class, IStringAbstraction<StringAbstraction>
            {
                private readonly IStringOperations<StringAbstraction, BoxedVariable<Variable>> operations;

                public StringAbstractDomainFactory(IStringOperations<StringAbstraction, BoxedVariable<Variable>> operations)
                {
                    this.operations = operations;
                }

                public IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> CreateTopValue(BoxedExpressionDecoder<Variable> decoder)
                {
                    return new StringAbstractDomain<BoxedVariable<Variable>, BoxedExpression, StringAbstraction>(decoder, operations);
                }
            }
            internal class StringPentagonsFactory<StringAbstraction> : IStringAbstractDomainFactory
              where StringAbstraction : class, IStringInterval<StringAbstraction>
            {
                private readonly IStringIntervalOperations<StringAbstraction, BoxedVariable<Variable>> operations;

                public StringPentagonsFactory(IStringIntervalOperations<StringAbstraction, BoxedVariable<Variable>> operations)
                {
                    this.operations = operations;
                }

                public IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> CreateTopValue(BoxedExpressionDecoder<Variable> decoder)
                {
                    return new StringPentagons<BoxedVariable<Variable>, BoxedExpression, StringAbstraction>(decoder, operations);
                }
            }

            private static IStringAbstractDomainFactory CreateFactoryForAbstraction<StringAbstraction>(IStringOperations<StringAbstraction, BoxedVariable<Variable>> operations)
              where StringAbstraction : class, IStringAbstraction<StringAbstraction>
            {
                return new StringAbstractDomainFactory<StringAbstraction>(operations);
            }
            private static IStringAbstractDomainFactory CreatePentagonFactoryForAbstraction<StringAbstraction>(IStringIntervalOperations<StringAbstraction, BoxedVariable<Variable>> operations)
              where StringAbstraction : class, IStringInterval<StringAbstraction>
            {
                return new StringPentagonsFactory<StringAbstraction>(operations);
            }

            internal static IStringAbstractDomainFactory CreateFactoryForAbstraction(Analyzers.Strings.StringDomainKind kind)
            {
                switch (kind)
                {
                    case Analyzers.Strings.StringDomainKind.Prefix:
                        return CreateFactoryForAbstraction(new AbstractDomains.Strings.Prefix.Operations<BoxedVariable<Variable>>());
                    case Analyzers.Strings.StringDomainKind.Suffix:
                        return CreateFactoryForAbstraction(new AbstractDomains.Strings.Suffix.Operations<BoxedVariable<Variable>>());
                    case Analyzers.Strings.StringDomainKind.CharacterInclusionASCII:
                        return CreateFactoryForAbstraction(new AbstractDomains.Strings.CharacterInclusion<BitArrayCharacterSet>.Operations<BoxedVariable<Variable>>(new ASCIIClassification(), new BitArrayCharacterSetFactory()));
                    case Analyzers.Strings.StringDomainKind.CharacterInclusionFull:
                        return CreateFactoryForAbstraction(new AbstractDomains.Strings.CharacterInclusion<BitArrayCharacterSet>.Operations<BoxedVariable<Variable>>(new CompleteClassification(), new BitArrayCharacterSetFactory()));
                    case Analyzers.Strings.StringDomainKind.Bricks:
                        IBricksPolicy policy = new DefaultBricksPolicy { MergeConstantSets = true, ExpandConstantRepetitions = false };
                        return CreateFactoryForAbstraction(new AbstractDomains.Strings.Bricks.Operations<BoxedVariable<Variable>>(policy));
                    case Analyzers.Strings.StringDomainKind.StringGraphs:
                        return CreateFactoryForAbstraction(new AbstractDomains.Strings.StringGraph.Operations<BoxedVariable<Variable>>());
                    case Analyzers.Strings.StringDomainKind.PrefixIntervals:
                        return CreateFactoryForAbstraction(new AbstractDomains.Strings.PrefixInterval.Operations<BoxedVariable<Variable>>());
                    case Analyzers.Strings.StringDomainKind.PrefixPentagons:
                        return CreatePentagonFactoryForAbstraction(new AbstractDomains.Strings.PrefixInterval.Operations<BoxedVariable<Variable>>());
                    case Analyzers.Strings.StringDomainKind.Tokens:
                        return CreateFactoryForAbstraction(new AbstractDomains.Strings.Tokens.Operations<BoxedVariable<Variable>>());
                    default:
                        throw new NotImplementedException("String abstract domain is not implemented");
                }

            }

            /// <summary>
            /// It runs the analysis. 
            /// It is there because so we can use the typebinding, and make the code less verbose
            /// </summary>
            internal static IMethodResult<Variable> HelperForStringAnalysis
            (
              string methodName,
              IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ILogOptions> driver,
              Analyzers.Strings.StringOptions options,
              Predicate<APC> cachePCs, DFAController controller
            )
            {
                var factory = CreateFactoryForAbstraction(options.Domain);

                var analysis = new StringValueAnalysis(methodName, driver, options, cachePCs, factory);

                var closure = driver.HybridLayer.CreateForward(analysis, new DFAOptions { Trace = driver.Options.TraceDFA }, controller);

                closure(analysis.GetTopValue());   // Do the analysis 

                return analysis;
            }

            #region Facility to forward operations on the abstract domain (implementation of IAbstractDomainOperations)
            public class AbstractOperationsImplementationString : IAbstractDomainOperations<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>>
            {
                public bool LookupState(IMethodResult<Variable> mr, APC pc, out IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> astate)
                {
                    astate = null;
                    StringValueAnalysis an = mr as StringValueAnalysis;
                    if (an == null)
                        return false;

                    return an.PreStateLookup(pc, out astate);
                }

                public IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> Join(IMethodResult<Variable> mr, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> astate1, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> astate2)
                {
                    StringValueAnalysis an = mr as StringValueAnalysis;
                    if (an == null)
                        return null;

                    bool bWeaker;
                    return an.Join(new Pair<APC, APC>(), astate1, astate2, out bWeaker, false);
                }

                public List<BoxedExpression> ExtractAssertions(
                  IMethodResult<Variable> mr,
                  IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> astate,
                  IExpressionContext<Local, Parameter, Method, Field, Type, Expression, Variable> context,
                  IDecodeMetaData<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly> metaDataDecoder)
                {
                    StringValueAnalysis an = mr as StringValueAnalysis;
                    if (an == null)
                        return null;

                    BoxedExpressionReader<Local, Parameter, Method, Field, Property, Event, Type, Variable, Expression, Attribute, Assembly> br = new BoxedExpressionReader<Local, Parameter, Method, Field, Property, Event, Type, Variable, Expression, Attribute, Assembly>(context, metaDataDecoder);

                    return an.ToListOfBoxedExpressions(astate, br);
                }

                public bool AssignInParallel(IMethodResult<Variable> mr, ref IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> astate, Dictionary<BoxedVariable<Variable>, FList<BoxedVariable<Variable>>> mapping, Converter<BoxedVariable<Variable>, BoxedExpression> convert)
                {
                    StringValueAnalysis an = mr as StringValueAnalysis;
                    if (an == null)
                        return false;

                    astate.AssignInParallel(mapping, convert);
                    return true;
                }
            }

            #endregion


            /// <summary>
            /// The analysis for the value of strings
            /// </summary>
            internal class StringValueAnalysis :
              GenericValueAnalysis<IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>, Analyzers.Strings.StringOptions>
            {
                private IStringAbstractDomainFactory abstractDomainFactory;

                #region Constructor
                internal StringValueAnalysis(
                  string methodName,
                  IMethodDriver<Local, Parameter, Method, Field, Property, Event, Type, Attribute, Assembly, Expression, Variable, ILogOptions> methodDriver,
                  Analyzers.Strings.StringOptions options,
                  Predicate<APC> cachePCs,
                  IStringAbstractDomainFactory abstractDomainFactory
                )
                  : base(methodName, methodDriver, options, cachePCs)
                {
                    this.abstractDomainFactory = abstractDomainFactory;
                }
                #endregion

                #region Transition

                #region Type matching
                private const string System_String = "System.String.";
                private const string System_Regex = "System.Text.RegularExpressions.Regex.";
                private const string System_StringBuilder = "System.Text.StringBuilder.";

                private bool IsACallToString(string methodName)
                {
                    System.Diagnostics.Contracts.Contract.Requires(methodName != null);
                    return methodName.StartsWith(System_String, StringComparison.Ordinal);
                }
                private bool IsACallToRegex(string methodName)
                {
                    System.Diagnostics.Contracts.Contract.Requires(methodName != null);
                    return methodName.StartsWith(System_Regex, StringComparison.Ordinal);
                }
                private bool IsACallToStringBuilder(string methodName)
                {
                    System.Diagnostics.Contracts.Contract.Requires(methodName != null);
                    return methodName.StartsWith(System_StringBuilder, StringComparison.Ordinal);
                }
                #endregion

                #region Private methods

                /// <summary>
                /// Gets a <see cref="BoxedExpression"/> for an method argument.
                /// </summary>
                /// <param name="pc">The program counter of the call.</param>
                /// <param name="argument">The argument variable.</param>
                /// <returns><see cref="BoxedExpression"/> for <paramref name="argument"/>.</returns>
                private BoxedExpression ExprAt(APC pc, Variable argument)
                {
                    return BoxedExpression.For(this.Context.ExpressionContext.Refine(pc, argument), this.Decoder.Outdecoder);
                }

                private IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>/*!*/ HandleCallToRegex<ArgList>(APC pc, string/*!*/ fullMethodName, Variable/*!*/ dest, ArgList/*!*/ args, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>/*!*/ state)
                  where ArgList : IIndexable<Variable>
                {
                    string method = fullMethodName.Substring(System_Regex.Length);

                    BoxedExpression target = ExprAt(pc, dest);
                    switch (method)
                    {
                        case "IsMatch(System.String,System.String)":
                            state.RegexIsMatch(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]));
                            break;
                    }

                    return state;
                }

                private IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>/*!*/ HandleCallToStringBuilder<ArgList>(APC pc, string/*!*/ fullMethodName, Variable/*!*/ dest, ArgList/*!*/ args, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>/*!*/ state, AbstractDomains.INumericalAbstractDomain<BoxedVariable<Variable>, BoxedExpression> numerical)
                  where ArgList : IIndexable<Variable>
                {
                    string method = fullMethodName.Substring(System_StringBuilder.Length);

                    BoxedExpression target = ExprAt(pc, args[0]);

                    var slack = new BoxedVariable<Variable>(true);
                    var slackExp = BoxedExpression.Var(slack);

                    switch (method)
                    {
                        case "Append(System.String)":
                            state.Concat(slackExp, target, ExprAt(pc, args[1]));
                            state.Mutate(target);
                            state.RenameVariable(slack, Decoder.UnderlyingVariable(target));
                            break;
                        case "Insert(System.Int32,System.String)":
                            state.Insert(slackExp, target, ExprAt(pc, args[1]), ExprAt(pc, args[2]), numerical);
                            state.Mutate(target);
                            state.RenameVariable(slack, Decoder.UnderlyingVariable(target));
                            break;
                        case "Replace(System.Char,System.Char)":
                            state.ReplaceChar(slackExp, target, ExprAt(pc, args[1]), ExprAt(pc, args[2]), numerical);
                            state.Mutate(target);
                            state.RenameVariable(slack, Decoder.UnderlyingVariable(target));
                            break;
                        case "Remove(System.Int32,System.Int32)":
                            state.SubstringRemove(slackExp, target, ExprAt(pc, args[1]), ExprAt(pc, args[2]), true, numerical);
                            state.Mutate(target);
                            state.RenameVariable(slack, Decoder.UnderlyingVariable(target));
                            break;
                        case "Clear()":
                            state.Mutate(target);
                            state.Empty(target);
                            break;
                        case "get_Length":
                            state.GetLength(ExprAt(pc, dest), target, numerical);
                            break;
                        // "set_Length(System.Int32)" not supported
                        default:
                            state.Mutate(target);
                            break;
                    }
                    return state;
                }

                private IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>/*!*/ HandleToStringCall<ArgList>(
                    APC pc, Variable/*!*/ dest, ArgList/*!*/ args,
                    IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>/*!*/ state)
                  where ArgList : IIndexable<Variable>
                {
                    if (HasStringBuilderType(pc, args[0]))
                    {
                        state.Copy(ExprAt(pc, dest), ExprAt(pc, args[0]));
                    }
                    else
                    {
                        state.Unknown(ExprAt(pc, dest));
                    }
                    return state;
                }

                private IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>/*!*/ HandleCallToString<ArgList>(
                    APC pc, string/*!*/ fullMethodName, Variable/*!*/ dest, ArgList/*!*/ args,
                    IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>/*!*/ state,
                    AbstractDomains.INumericalAbstractDomain<BoxedVariable<Variable>, BoxedExpression> numerical, INullQuery<BoxedVariable<Variable>> nullQuery)
                  where ArgList : IIndexable<Variable>
                {
                    string method = fullMethodName.Substring(System_String.Length);

                    BoxedExpression target = ExprAt(pc, dest);
                    switch (method)
                    {
                        case "Clone()":
                        case "ToString()":
                        case "Copy(System.String)":
                        case "Intern(System.String)":
                        case "ToCharArray()":
                            state.Copy(target, ExprAt(pc, args[0]));
                            break;
                        case "Concat(System.String,System.String)":
                            state.Concat(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]));
                            break;
                        case "Concat(System.String,System.String,System.String)":
                            state.Concat(target, new[] { ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]) });
                            break;
                        case "Concat(System.String,System.String,System.String,System.String)":
                            state.Concat(target, new[] { ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), ExprAt(pc, args[3]) });
                            break;
                        case "Insert(System.Int32,System.String)":
                            state.Insert(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), numerical);
                            break;
                        case "Remove(System.Int32)":
                            state.SubstringRemove(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), null, true, numerical);
                            break;
                        case "Remove(System.Int32,System.Int32)":
                            state.SubstringRemove(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), true, numerical);
                            break;
                        case "Substring(System.Int32)":
                            state.SubstringRemove(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), null, false, numerical);
                            break;
                        case "Substring(System.Int32,System.Int32)":
                            state.SubstringRemove(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), false, numerical);
                            break;
                        case "Replace(System.Char,System.Char)":
                            state.ReplaceChar(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), numerical);
                            break;
                        case "Replace(System.String,System.String)":
                            state.ReplaceString(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]));
                            break;
                        case "PadLeft(System.Int32)":
                            state.PadLeftRight(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), null, false, numerical);
                            break;
                        case "PadLeft(System.Int32,System.Char)":
                            state.PadLeftRight(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), false, numerical);
                            break;
                        case "PadRight(System.Int32)":
                            state.PadLeftRight(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), null, true, numerical);
                            break;
                        case "PadRight(System.Int32,System.Char)":
                            state.PadLeftRight(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), true, numerical);
                            break;
                        case "Trim(System.Char[])":
                            state.Trim(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]));
                            break;
                        case "TrimStart(System.Char[])":
                            state.TrimStartEnd(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), false);
                            break;
                        case "TrimEnd(System.Char[])":
                            state.TrimStartEnd(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), true);
                            break;
                        case "IsNullOrEmpty(System.String)":
                            state.IsNullOrEmpty(target, ExprAt(pc, args[0]));
                            break;
                        case "Contains(System.String)":
                            state.Contains(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]));
                            break;
                        case "EndsWith(System.String,System.StringComparison)":
                            state.StartsEndsWith(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), true);
                            break;
                        case "StartsWith(System.String,System.StringComparison)":
                            state.StartsEndsWith(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), false);
                            break;
                        case "Equals(System.String,System.String)":
                            state.Equals(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), nullQuery);
                            break;
                        case "CompareOrdinal(System.String,System.String)":
                            state.CompareOrdinal(target, ExprAt(pc, args[0]), ExprAt(pc, args[0]), numerical, nullQuery);
                            break;
                        case "IndexOf(System.String,System.StringComparison)":
                            state.IndexOf(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), null, null, ExprAt(pc, args[2]), false, numerical);
                            break;
                        case "IndexOf(System.String,System.Int32,System.StringComparison)":
                            state.IndexOf(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), null, ExprAt(pc, args[3]), false, numerical);
                            break;
                        case "IndexOf(System.String,System.Int32,System.Int32,System.StringComparison)":
                            state.IndexOf(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), ExprAt(pc, args[3]), ExprAt(pc, args[4]), false, numerical);
                            break;
                        case "IndexOf(System.Char)":
                            state.IndexOfChar(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), null, null, false, numerical);
                            break;
                        case "IndexOf(System.Char,System.Int32)":
                            state.IndexOfChar(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), null, false, numerical);
                            break;
                        case "IndexOf(System.Char,System.Int32,System.Int32)":
                            state.IndexOfChar(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), ExprAt(pc, args[3]), false, numerical);
                            break;
                        case "LastIndexOf(System.String,System.StringComparison)":
                            state.IndexOf(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), null, null, ExprAt(pc, args[2]), true, numerical);
                            break;
                        case "LastIndexOf(System.String,System.Int32,System.StringComparison)":
                            state.IndexOf(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), null, ExprAt(pc, args[3]), true, numerical);
                            break;
                        case "LastIndexOf(System.String,System.Int32,System.Int32,System.StringComparison)":
                            state.IndexOf(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), ExprAt(pc, args[3]), ExprAt(pc, args[4]), true, numerical);
                            break;
                        case "LastIndexOf(System.Char)":
                            state.IndexOfChar(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), null, null, true, numerical);
                            break;
                        case "LastIndexOf(System.Char,System.Int32)":
                            state.IndexOfChar(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), null, true, numerical);
                            break;
                        case "LastIndexOf(System.Char,System.Int32,System.Int32)":
                            state.IndexOfChar(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), ExprAt(pc, args[2]), ExprAt(pc, args[3]), true, numerical);
                            break;
                        case "get_Chars(System.Int32)":
                            state.GetChar(target, ExprAt(pc, args[0]), ExprAt(pc, args[1]), numerical);
                            break;
                        case "get_Length":
                            state.GetLength(target, ExprAt(pc, args[0]), numerical);
                            break;
                            /* Analysis of those methods is possible, but currently not implemented:
                             * case "IndexOfAny(System.Char[])":
                             * case "LastIndexOfAny(System.Char[])":
                             *   break;
                             */
                    }

                    return state;
                }

                /// <summary>
                /// Mutates argument values on method calls
                /// </summary>
                private IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> MutateArguments<ArgList>(APC pc, Method method, ArgList args, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> domain)
                    where ArgList : IIndexable<Variable>
                {
                    for (int i = 0; i < args.Count; ++i)
                    {
                        Variable arg = args[i];

                        if (HasStringBuilderType(pc, arg) || HasCharArrayType(pc, arg))
                        {
                            domain.Mutate(ExprAt(pc, args[i]));
                        }
                    }
                    return domain;
                }

                #endregion

                #region Visitor handlers
                public override IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> Newobj<ArgList>(APC pc, Method ctor, Variable dest, ArgList args, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> data)
                {
                    // Handle constructor calls

                    string ctorName = this.DecoderForMetaData.FullName(ctor);

                    var state = base.Newobj<ArgList>(pc, ctor, dest, args, data);

                    BoxedExpression target = ExprAt(pc, dest);

                    switch (ctorName)
                    {
                        case "System.String.#ctor(System.String)":
                        case "System.String.#ctor(System.Char[])":
                            // Convert char array to string
                            state.Copy(target, ExprAt(pc, args[0]));
                            break;
                        case "System.Text.StringBuilder.#ctor()":
                            // Create empty StringBuilder
                            state.Empty(target);
                            break;
                        case "System.Text.StringBuilder.#ctor(System.String)":
                            // Convert string to StringBuilder
                            state.Copy(target, ExprAt(pc, args[0]));
                            break;
                    }

                    return state;
                }

                public IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> LdelemWithNumerical(APC pc, Type type, Variable dest, Variable array, Variable index, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> data, AbstractDomains.INumericalAbstractDomain<BoxedVariable<Variable>, BoxedExpression> numerical)
                {
                    IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> state = base.Ldelem(pc, type, dest, array, index, data);
                    if (type.Equals(DecoderForMetaData.System_Char))
                    {
                        state.GetChar(ExprAt(pc, dest), ExprAt(pc, array), ExprAt(pc, index), numerical);
                    }
                    return state;
                }

                public override IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> Ldnull(APC pc, Variable dest, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> data)
                {
                    IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> result = base.Ldnull(pc, dest, data);
                    result.Empty(ExprAt(pc, dest));
                    return result;
                }

                private bool IsCharArrayType(Type type)
                {
                    if (type.Equals(this.DecoderForMetaData.ArrayType(this.DecoderForMetaData.System_Char, 1)))
                    {
                        return true;
                    }

                    return false;
                }
                private bool IsStringBuilderType(Type type)
                {
                    return DecoderForMetaData.FullName(type) == "System.Text.StringBuilder";
                }

                private bool HasStringBuilderType(APC pc, Variable var)
                {
                    var flattype = this.Context.ValueContext.GetType(pc, var);

                    if (flattype.IsNormal)
                    {
                        var type = flattype.Value;
                        return IsStringBuilderType(type);
                    }

                    return false;
                }

                private bool HasCharArrayType(APC pc, Variable var)
                {
                    var flattype = this.Context.ValueContext.GetType(pc, var);

                    if (flattype.IsNormal)
                    {
                        var type = flattype.Value;
                        return IsCharArrayType(type);
                    }

                    return false;
                }

                private bool HasStringType(APC pc, Variable var)
                {
                    var flattype = this.Context.ValueContext.GetType(pc, var);

                    if (flattype.IsNormal)
                    {
                        var type = flattype.Value;

                        if (type.Equals(this.DecoderForMetaData.System_String))
                        {
                            return true;
                        }
                    }

                    return false;
                }

                public override IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> Binary(APC pc, BinaryOperator op, Variable dest, Variable s1, Variable s2, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> data)
                {
                    var state = base.Binary(pc, op, dest, s1, s2, data);

                    if (op == BinaryOperator.Cobjeq && HasStringType(pc, s1) && HasStringType(pc, s2))
                    {
                        state.Equals(ExprAt(pc, dest), ExprAt(pc, s1), ExprAt(pc, s2), null);
                    }

                    return state;
                }

                public override IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> Entry(
                    APC pc, Method method, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> state)
                {
                    state = base.Entry(pc, method, state);

                    // Relate string builder values to the ToString call 
                    foreach (var param in this.DecoderForMetaData.Parameters(method).Enumerate())
                    {
                        Variable sbParam;
                        var postPC = Context.MethodContext.CFG.Post(pc);
                        if (this.Context.ValueContext.TryParameterValue(postPC, param, out sbParam))
                        {
                            var paramType = this.DecoderForMetaData.ParameterType(param);
                            if (IsStringBuilderType(paramType))
                            {
                                Variable sbToString;
                                if (this.Context.ValueContext.TryGetToString(postPC, sbParam, out sbToString))
                                {
                                    BoxedExpression paramExp = ExprAt(pc,sbParam);
                                    BoxedExpression toStringExp = ExprAt(pc, sbToString);
                                    state.Copy(paramExp, toStringExp);
                                }
                            }
                        }
                    }

                    return state;
                }
                /// <summary>
                /// Here we catch the calls to methods of String, so that we can apply operations on string, as concatenations. etc.
                /// </summary>
                public override IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> Call<TypeList, ArgList>(APC pc, Method method, bool tail, bool virt, TypeList extraVarargs, Variable dest, ArgList args, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> data)
                // where ArgList : IIndexable<int>
                {
                    // Put null as the numerical domain
                    return CallWithNumerical(pc, method, tail, virt, extraVarargs, dest, args, data, null, null);
                }
                #endregion
                public IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> CallWithNumerical<TypeList, ArgList>(
                  APC pc,
                  Method method,
                  bool tail,
                  bool virt,
                  TypeList extraVarargs,
                  Variable dest,
                  ArgList args,
                  IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> data,
                  AbstractDomains.INumericalAbstractDomain<BoxedVariable<Variable>, BoxedExpression> numerical,
                  INullQuery<BoxedVariable<Variable>> nullQuery
                  )
                  where TypeList : IIndexable<Type>
                  where ArgList : IIndexable<Variable>
                {
                    string methodName = this.DecoderForMetaData.FullName(method);

                    IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> result;
                    var baseResult = base.Call<TypeList, ArgList>(pc, method, tail, virt, extraVarargs, dest, args, data);

                    if (IsACallToString(methodName))
                    {
                        result = HandleCallToString(pc, methodName, dest, args, baseResult, numerical, nullQuery);
                    }
                    else if (IsACallToRegex(methodName))
                    {
                        result = HandleCallToRegex(pc, methodName, dest, args, baseResult);
                    }
                    else if (IsACallToStringBuilder(methodName))
                    {
                        result = HandleCallToStringBuilder(pc, methodName, dest, args, baseResult, numerical);
                    }
                    else if (methodName == "System.Object.ToString()")
                    {
                        result = HandleToStringCall(pc, dest, args, baseResult);
                    }
                    else if (methodName == "System.Array.get_Length" && HasCharArrayType(pc, args[0]))
                    {
                        result = baseResult;
                        result.GetLength(ExprAt(pc, dest), ExprAt(pc, args[0]), numerical);
                    }
                    else
                    {
                        result = MutateArguments(pc, method, args, baseResult);
                    }

                    return result;
                }

                #endregion

                #region Implementation of the abstract interface

                private BoxedExpression GetExpressionWithPath(APC exitPC, Variable arrVar)
                {
                    var vc = this.Context.ValueContext;
                    var arrayPath = vc.VisibleAccessPathListFromPost(exitPC, arrVar);

                    if (arrayPath != null)
                    {
                        return BoxedExpression.Var(arrVar, arrayPath);
                    }

                    Variable retVar;
                    if (vc.TryResultValue(exitPC, out retVar) && retVar.Equals(arrVar))
                    {
                        return BoxedExpression.Result<Type>(this.DecoderForMetaData.ReturnType(this.MethodDriver.CurrentMethod));
                    }

                    return null;
                }

                public override bool SuggestAnalysisSpecificPostconditions(ContractInferenceManager inferenceManager, IFixpointInfo<APC, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>> fixpointInfo, List<BoxedExpression> postconditions)
                {
                    return SuggestAnalysisSpecificPostconditions(inferenceManager, fixpointInfo, null, postconditions);
                }

                private BoxedExpression RegexPostcondittion(BoxedExpression varExp, string regex)
                {
                    var regexExpr = BoxedExpression.Const(regex, DecoderForMetaData.System_String, DecoderForMetaData);
                    return BoxedExpression.Binary(BinaryOperator.RegexIsMatch, varExp, regexExpr);
                }

                private BoxedExpression RelationPostcondition(BoxedExpression varExp, BoxedExpression relatedExp, AbstractDomains.Expressions.ExpressionOperator op)
                {
                    
                    var relatedExpOld = BoxedExpression.Old(relatedExp, DecoderForMetaData.System_String);

                    switch (op)
                    {
                        case AbstractDomains.Expressions.ExpressionOperator.StartsWith:
                            return BoxedExpression.Binary(BinaryOperator.StartsWith, varExp, relatedExpOld);
                        case AbstractDomains.Expressions.ExpressionOperator.EndsWith:
                            return BoxedExpression.Binary(BinaryOperator.EndsWith, varExp, relatedExpOld);
                        case AbstractDomains.Expressions.ExpressionOperator.Contains:
                            return BoxedExpression.Binary(BinaryOperator.Contains, varExp, relatedExpOld);
                        default:
                            throw new InvalidOperationException("Bad enum value");
                    }
                }

                public bool SuggestAnalysisSpecificPostconditions(
                    ContractInferenceManager inferenceManager,
                    IFixpointInfo<APC, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>> fixpointInfo,
                    IFixpointInfo<APC, INullQuery<BoxedVariable<Variable>>> nullInfo,
                    List<BoxedExpression> postconditions)
                {
                    // Entry and exit program points
                    var normalExitPC = this.Context.MethodContext.CFG.NormalExit;
                    var entryPC = this.Context.MethodContext.CFG.EntryAfterRequires;

                    // Get string and non-null information at exit
                    IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> astate;
                    INullQuery<BoxedVariable<Variable>> nullQuery = null;
                    if (PreState(normalExitPC, fixpointInfo, out astate) && !astate.IsTop && (nullInfo == null || nullInfo.PreState(normalExitPC, out nullQuery)))
                    {
                        foreach(var boxedVar in astate.Variables)
                        {
                            // Ignore variables that are null
                            if (nullQuery != null && nullQuery.IsNull(boxedVar))
                                continue;

                            Variable variable;
                            if(boxedVar.TryUnpackVariable(out variable))
                            {
                                BoxedExpression varExp = GetExpressionWithPath(normalExitPC, variable);
                                if (varExp != null)
                                {
                                    // Non-relational properties
                                    foreach (string regex in astate.RegexForVariable(boxedVar))
                                    {
                                        var regexPostcondition = RegexPostcondittion(varExp, regex);

                                        if (!astate.CheckMustBeNonNull(boxedVar, nullQuery))
                                        {
                                            var nullCheckExpr = BoxedExpression.Binary(BinaryOperator.Ceq, varExp, BoxedExpression.Const(null, DecoderForMetaData.System_String, DecoderForMetaData));
                                            regexPostcondition = BoxedExpression.BinaryLogicalOr(nullCheckExpr, regexPostcondition);
                                        }

                                        postconditions.Add(regexPostcondition);
                                        // TODO: simpler exprs for starts/ends/contains
                                        // this.Encoder.CompoundExpressionFor(AbstractDomains.Expressions.ExpressionType.String, AbstractDomains.Expressions.ExpressionOperator.RegexIsMatch, )
                                    }

                                    // Relational properties
                                    foreach (StringRelation<BoxedVariable<Variable>> relation in astate.RelationsForVariable(boxedVar))
                                    {
                                        var relatedBoxedVar = relation.RelatedVariable;
                                        Variable relatedVar;
                                        if (relatedBoxedVar.TryUnpackVariable(out relatedVar))
                                        {
                                            var relatedPath = Context.ValueContext.AccessPathList(entryPC, relatedVar, false, false);

                                            if (relatedPath != null)
                                            {
                                                var relatedExp = BoxedExpression.Var(relatedVar, relatedPath);
                                                postconditions.Add(RelationPostcondition(varExp, relatedExp, relation.Operator));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                    }

                    return false;
                }

                public override bool TrySuggestPostconditionForOutParameters(IFixpointInfo<APC, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>> fixpointInfo, List<BoxedExpression> postconditions, Variable p, FList<PathElement> path)
                {
                    return false;
                }

                public override IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> GetTopValue()
                {
                    return abstractDomainFactory.CreateTopValue(this.Decoder);

                }
                #endregion

                #region IMethodResult<Label,Expression> Members

                public ProofOutcome ValidateExplicitAssertion(APC pc, Expression expr)
                {
                    throw new Exception("The method or operation is not implemented.");
                }

                #endregion

                #region Fact Query

                public override IFactQuery<BoxedExpression, Variable> FactQuery(IFixpointInfo<APC, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>> fixpoint)
                {
                    return new SimpleLogicInference<Local, Parameter, Method, Field, Type, Expression, Variable>(this.Context,
                      new FactBase(fixpoint), null, null, this.MethodDriver.BasicFacts.IsUnreachable, null);
                }

                class FactBase : IFactBase<Variable>
                {
                    IFixpointInfo<APC, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>> fixpoint;

                    public FactBase(IFixpointInfo<APC, IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression>> fixpoint)
                    {
                        this.fixpoint = fixpoint;
                    }

                    private ProofOutcome IsVariableTrue(APC pc, Variable variable)
                    {
                        IStringAbstractDomain<BoxedVariable<Variable>, BoxedExpression> domain;
                        if (fixpoint.PreState(pc, out domain))
                        {
                            BoxedVariable<Variable> boxedVariable = new BoxedVariable<Variable>(variable);
                            return domain.EvalBool(boxedVariable);
                        }
                        else
                            return ProofOutcome.Top;
                    }


                    #region IFactBase<Variable> Members

                    public ProofOutcome IsNull(APC pc, Variable variable)
                    {
                        return IsVariableTrue(pc, variable).Negate();
                    }
                    public ProofOutcome IsNonNull(APC pc, Variable variable)
                    {
                        return IsVariableTrue(pc, variable);
                    }

                    public bool IsUnreachable(APC pc)
                    {
                        return false;
                    }

                    public FList<BoxedExpression> InvariantAt(APC pc, FList<Variable> filter, bool replaceVarsWithAccessPaths = true)
                    {
                        return FList<BoxedExpression>.Empty;
                    }

                    #endregion
                }

                #endregion
            }

        }
    }
}
