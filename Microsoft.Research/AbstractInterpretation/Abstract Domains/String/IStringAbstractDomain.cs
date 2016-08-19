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

namespace Microsoft.Research.AbstractDomains.Strings
{
  /// <summary>
  /// Represents an abstract domain for environments, supporting string operations.
  /// </summary>
  /// <typeparam name="Variable">Type of variables in the environments.</typeparam>
  /// <typeparam name="Expression">Type of expressions in the operations.</typeparam>
  public interface IStringAbstractDomain<Variable, Expression> : IAbstractDomainForEnvironments<Variable, Expression>
    where Variable : class, IEquatable<Variable>
  {
    /// <summary>
    /// Applies a transition function, which initializes a variable to an empty string.
    /// </summary>
    /// <param name="targetExp">The expression where the empty string is assigned.</param>
    void Empty(Expression targetExp);

    /// <summary>
    /// Applies a transition function, which assigns a variable
    /// </summary>
    /// <param name="sourceExp">The evaluated expression.</param>
    /// <param name="targetExp">The expression where string is assinged.</param>
    void Copy(Expression targetExp, Expression sourceExp);

    /// <summary>
    /// Applies a transition function for the <see cref="String.Concat(String,String)"/> method.
    /// </summary>
    /// <remarks>Calling this method modifies the instance.</remarks>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="leftExp">The left argument of the method.</param>
    /// <param name="rightExp">The right argument of the method.</param>
    void Concat(Expression targetExp, Expression leftExp, Expression rightExp);

    /// <summary>
    /// Applies a transition function for the <see cref="String.Concat(String,String,String)"/> method.
    /// </summary>
    /// <remarks>Calling this method modifies the instance.</remarks>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="leftExp">The first argument of the method.</param>
    /// <param name="middleExp">The second argument of the method.</param>
    /// <param name="rightExp">The third argument of the method.</param>
    void Concat(Expression targetExp, Expression leftExp, Expression middleExp, Expression rightExp);

    /// <summary>
    /// Applies a transition function for the <see cref="String.Concat(String,String,String,String)"/> method.
    /// </summary>
    /// <remarks>Calling this method modifies the instance.</remarks>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="leftExp">The first argument of the <see cref="String.Concat"/> method.</param>
    /// <param name="middleLeftExp">The second argument of the <see cref="String.Concat"/> method.</param>
    /// <param name="middleRightExp">The third argument of the <see cref="String.Concat"/> method.</param>
    /// <param name="rightExp">The fourth argument of the <see cref="String.Concat"/> method.</param>
    void Concat(Expression targetExp, Expression leftExp, Expression middleLeftExp, Expression middleRightExp, Expression rightExp);

    /// <summary>
    /// Applies a transition function for the <see cref="String.Insert"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="indexExp">The expression that evaluates the index argument.</param>
    /// <param name="partExp">The expression that evaluates the inserted string argument.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to evaluate integer arguments, or <see langword="null"/>.</param>
    void Insert(Expression targetExp, Expression valueExp, Expression indexExp, Expression partExp,
      INumericalAbstractDomain<Variable, Expression> numericalDomain);

    /// <summary>
    /// Applies a transition function for the <see cref="String.Replace(char,char)"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="fromExp">The expression that evaluates the replaced character argument.</param>
    /// <param name="toExp">The expression that evaluates the replacement character argument.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to evaluate integer arguments, or <see langword="null"/>.</param>
    void ReplaceChar(Expression targetExp, Expression valueExp, Expression fromExp, Expression toExp,
      INumericalAbstractDomain<Variable, Expression> numericalDomain);
    /// <summary>
    /// Applies a transition function for the <see cref="String.Replace(String,String)"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="fromExp">The expression that evaluates the replaced string argument.</param>
    /// <param name="toExp">The expression that evaluates the replacement string argument.</param>
    void ReplaceString(Expression targetExp, Expression valueExp, Expression fromExp, Expression toExp);

    /// <summary>
    /// Applies a transition function for the <see cref="String.Substring"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="indexExp">The expression that evaluates the index argument.</param>
    /// <param name="lengthExp">The expression that evaluates the length argument, or <see langword="null"/>.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to evaluate integer arguments, or <see langword="null"/>.</param>
    void Substring(Expression targetExp, Expression valueExp, Expression indexExp, Expression lengthExp,
      INumericalAbstractDomain<Variable, Expression> numericalDomain);
    /// <summary>
    /// Applies a transition function for the <see cref="String.Remove"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="indexExp">The expression that evaluates the index argument.</param>
    /// <param name="lengthExp">The expression that evaluates the length argument, or <see langword="null"/>.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to evaluate integer arguments, or <see langword="null"/>.</param>
    void Remove(Expression targetExp, Expression valueExp, Expression indexExp, Expression lengthExp,
      INumericalAbstractDomain<Variable, Expression> numericalDomain);

    /// <summary>
    /// Applies a transition function for the <see cref="String.PadLeft"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="lengthExp">The expression that evaluates the length argument.</param>
    /// <param name="charExp">The expression that evaluates the padding character argument.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to evaluate integer arguments, or <see langword="null"/>.</param>
    void PadLeft(Expression targetExp, Expression valueExp, Expression lengthExp, Expression charExp,
      INumericalAbstractDomain<Variable, Expression> numericalDomain);
    /// <summary>
    /// Applies a transition function for the <see cref="String.PadRight(int,char)"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="lengthExp">The expression that evaluates the length argument.</param>
    /// <param name="charExp">The expression that evaluates the padding character argument.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to evaluate integer arguments, or <see langword="null"/>.</param>
    void PadRight(Expression targetExp, Expression valueExp, Expression lengthExp, Expression charExp,
      INumericalAbstractDomain<Variable, Expression> numericalDomain);

    /// <summary>
    /// Applies a transition function for the <see cref="String.Trim"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="trimExp">The expression that evaluates the trimmed characters argument.</param>
    void Trim(Expression targetExp, Expression valueExp, Expression trimExp);
    /// <summary>
    /// Applies a transition function for the <see cref="String.TrimStart"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="trimExp">The expression that evaluates the trimmed characters argument.</param>
    void TrimStart(Expression targetExp, Expression valueExp, Expression trimExp);
    /// <summary>
    /// Applies a transition function for the <see cref="String.TrimEnd"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="trimExp">The expression that evaluates the trimmed characters argument.</param>
    void TrimEnd(Expression targetExp, Expression valueExp, Expression trimExp);

    /// <summary>
    /// Applies a transition function for the <see cref="String.IsNullOrEmpty"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the argument.</param>
    void IsNullOrEmpty(Expression targetExp, Expression valueExp);

    /// <summary>
    /// Applies a transition function for the <see cref="String.Contains"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="partExp">The expression that evaluates the needle argument.</param>
    void Contains(Expression targetExp, Expression valueExp, Expression partExp);
    /// <summary>
    /// Applies a transition function for the <see cref="String.StartsWith"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="partExp">The expression that evaluates the needle argument.</param>
    /// <param name="comparisonExp">The expression that evaluates the comparison type argument.</param>
    void StartsWith(Expression targetExp, Expression valueExp, Expression partExp, Expression comparisonExp);
    /// <summary>
    /// Applies a transition function for the <see cref="String.EndsWith"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="partExp">The expression that evaluates the needle argument.</param>
    /// <param name="comparisonExp">The expression that evaluates the comparison type argument.</param>
    void EndsWith(Expression targetExp, Expression valueExp, Expression partExp, Expression comparisonExp);

    /// <summary>
    /// Applies a transition function for the <see cref="String.Equals"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="leftExp">The left argument of the method.</param>
    /// <param name="rightExp">The right argument of the method.</param>
    /// <param name="nullQuery">Information about null values, or <see langword="null"/>.</param>
    void Equals(Expression targetExp, Expression leftExp, Expression rightExp, INullQuery<Variable> nullQuery);

    /// <summary>
    /// Applies a transition function for the <see cref="String.CompareOrdinal"/> method,
    /// optionally querying or updating a numerical domain.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="leftExp">The left argument of the method.</param>
    /// <param name="rightExp">The right argument of the method.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to store the result, or <see langword="null"/>.</param>
    /// <param name="nullQuery">Information about null values, or <see langword="null"/>.</param>
    void CompareOrdinal(Expression targetExp, Expression leftExp, Expression rightExp,
      INumericalAbstractDomain<Variable, Expression> numericalDomain, INullQuery<Variable> nullQuery);

    /// <summary>
    /// Applies a transition function for the <see cref="String.Length"/> property.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the property is called.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to store the result, or <see langword="null"/>.</param>
    void GetLength(Expression targetExp, Expression valueExp,
      INumericalAbstractDomain<Variable, Expression> numericalDomain);

    /// <summary>
    /// Applies a transition function for the <see cref="String.IndexOf"/> method,
    /// optionally querying or updating a numerical domain. 
    /// </summary>
    /// <param name="indexExp">The expression where there result index is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="needleExp">The expression that evaluates the needle argument.</param>
    /// <param name="offsetExp">The expression that evaluates the needle argument, or <see langword="null"/>.</param>
    /// <param name="countExp">The expression that evaluates the count argument, or <see langword="null"/>.</param>
    /// <param name="cmpExp">The expression that evaluates the compare options argument, or <see langword="null"/>.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to evaluate integer arguments and store the result, or <see langword="null"/>.</param>
    void IndexOf(Expression indexExp, Expression valueExp, Expression needleExp, Expression offsetExp, Expression countExp,
      Expression cmpExp, INumericalAbstractDomain<Variable, Expression> numericalDomain);
    /// <summary>
    /// Applies a transition function for the <see cref="String.LastIndexOf"/> method,
    /// optionally querying or updating a numerical domain. 
    /// </summary>
    /// <param name="indexExp">The expression where there result index is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="needleExp">The expression that evaluates the needle argument.</param>
    /// <param name="offsetExp">The expression that evaluates the needle argument, or <see langword="null"/>.</param>
    /// <param name="countExp">The expression that evaluates the count argument, or <see langword="null"/>.</param>
    /// <param name="cmpExp">The expression that evaluates the compare options argument, or <see langword="null"/>.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to evaluate integer arguments and store the result, or <see langword="null"/>.</param>
    void LastIndexOf(Expression indexExp, Expression valueExp, Expression needleExp, Expression offsetExp, Expression countExp,
      Expression cmpExp, INumericalAbstractDomain<Variable, Expression> numericalDomain);

    /// <summary>
    /// Applies a transition function for the <see cref="String.IndexOf"/> method,
    /// optionally querying or updating a numerical domain. 
    /// </summary>
    /// <param name="indexExp">The expression where there result index is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="needleExp">The expression that evaluates the needle argument.</param>
    /// <param name="offsetExp">The expression that evaluates the needle argument, or <see langword="null"/>.</param>
    /// <param name="countExp">The expression that evaluates the count argument, or <see langword="null"/>.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to evaluate integer arguments and store the result, or <see langword="null"/>.</param>
    void IndexOfChar(Expression indexExp, Expression valueExp, Expression needleExp, Expression offsetExp, Expression countExp,
      INumericalAbstractDomain<Variable, Expression> numericalDomain);

    /// <summary>
    /// Applies a transition function for the <see cref="String.LastIndexOf"/> method,
    /// optionally querying or updating a numerical domain. 
    /// </summary>
    /// <param name="indexExp">The expression where there result index is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="needleExp">The expression that evaluates the needle argument.</param>
    /// <param name="offsetExp">The expression that evaluates the needle argument, or <see langword="null"/>.</param>
    /// <param name="countExp">The expression that evaluates the count argument, or <see langword="null"/>.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to evaluate integer arguments and store the result, or <see langword="null"/>.</param>
    void LastIndexOfChar(Expression indexExp, Expression valueExp, Expression needleExp, Expression offsetExp, Expression countExp,
      INumericalAbstractDomain<Variable, Expression> numericalDomain);

    /// <summary>
    /// Applies a transition function getting a character from a string.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="indexExp">The expression that evaluates the index.</param>
    /// <param name="numericalDomain">The numerical abstract domain used to evaluate integer arguments and store the result, or <see langword="null"/>.</param>
    void GetChar(Expression targetExp, Expression valueExp, Expression indexExp,
      INumericalAbstractDomain<Variable, Expression> numericalDomain);

    /// <summary>
    /// Applies a transition function for the <see cref="System.Text.RegularExpressions.Regex.IsMatch"/> method.
    /// </summary>
    /// <param name="targetExp">The expression where there result is assigned.</param>
    /// <param name="valueExp">The expression of the object on which the method is called.</param>
    /// <param name="regexExp">The expression of regular expression string.</param>
    void RegexIsMatch(Expression targetExp, Expression valueExp, Expression regexExp);

    /// <summary>
    /// Handles an unknown string operation.
    /// </summary>
    /// <param name="targetExp">The result of the unknown operation.</param>
    void Unknown(Expression targetExp);

    /// <summary>
    /// Handles possible mutation of a mutable string type.
    /// </summary>
    /// <param name="mutatedExp">The expression that is mutated.</param>
    void Mutate(Expression mutatedExp);

    /// <summary>
    /// Evaluates a bool variable in the represented environment.
    /// </summary>
    /// <param name="variable">The variable to be evaluated.</param>
    /// <returns>An overapproximation of the value of variable.</returns>
    CodeAnalysis.ProofOutcome EvalBool(Variable variable);
  }

  /// <summary>
  /// Provides information about possibility of <see langword="null"/> values.
  /// </summary>
  /// <typeparam name="Variable">The type of variables.</typeparam>
  public interface INullQuery<Variable>
    where Variable : class, IEquatable<Variable>
  {
    /// <summary>
    /// Determines whether the variable is known to be <see langword="null"/>.
    /// </summary>
    /// <param name="variable">A string variable.</param>
    /// <returns><see langword="true"/>, if <paramref name="variable"/> must be <see langword="null"/>.</returns>
    bool IsNull(Variable variable);
    /// <summary>
    /// Determines whether the variable is known to be non-<see langword="null"/>.
    /// </summary>
    /// <param name="variable">A string variable.</param>
    /// <returns><see langword="true"/>, if <paramref name="variable"/> must not be <see langword="null"/>.</returns>
    bool IsNonNull(Variable variable);
  }
}
