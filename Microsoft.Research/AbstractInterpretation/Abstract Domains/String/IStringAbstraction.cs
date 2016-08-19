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

namespace Microsoft.Research.AbstractDomains.Strings
{
  /// <summary>
  /// The interface for an abstract domain for string values.
  /// </summary>
  /// <typeparam name="Self">Type of the string abstract domain.</typeparam>
  /// <typeparam name="Value">Type of the string values.</typeparam>
  public interface IStringAbstraction<Self, Value> : IAbstractDomain
      where Self : IStringAbstraction<Self, Value>
  {
    #region Domain properties
    /// <summary>
    /// Gets the top element of the domain.
    /// </summary>
    new Self Top { get; }
    /// <summary>
    /// Gets the bottom element of the domain.
    /// </summary>
    new Self Bottom { get; }
    /// <summary>
    /// Determines whether the element represents all strings.
    /// </summary>
    new bool IsTop { get; }
    /// <summary>
    /// Determines whether the element represents no strings.
    /// </summary>
    new bool IsBottom { get; }
    /// <summary>
    /// Determines whether the abstract element represents a concrete value.
    /// </summary>
    /// <param name="value">The concrete value.</param>
    /// <returns><see langword="true"/>, if <paramref name="value"/> is in the set of strings represented by this.</returns>
    bool ContainsValue(Value value);
    /// <summary>
    /// Determines whether two abstract elements are the same.
    /// </summary>
    /// <param name="other">Another abstract element.</param>
    /// <returns><see langword="true"/>, if this is the same as <paramref name="other"/>.</returns>
    bool Equals(Self other);
    /// <summary>
    /// Determines whether the element all the strings represented by another element.
    /// </summary>
    /// <param name="other">Another abstract element.</param>
    /// <returns><see langword="true"/>, if this represents fewer values than <paramref name="other"/>.</returns>
    bool LessThanEqual(Self other);
    #endregion
    #region Domain operations
    /// <summary>
    /// Computes the lowest upper bound of two elements.
    /// </summary>
    /// <param name="other">The other element.</param>
    /// <returns>The lowest upper bound of this and <paramref name="other"/>.</returns>
    Self Join(Self other);
    /// <summary>
    /// Computes the greatest lower bound of two elements.
    /// </summary>
    /// <param name="other">The other element.</param>
    /// <returns>The greatest bound of this and <paramref name="other"/>.</returns>
    Self Meet(Self other);
    /// <summary>
    /// Gets an abstract element from a concrete (constant) value.
    /// </summary>
    /// <param name="cst">The constant value.</param>
    /// <returns>An abstract element overapproximating a set of values containing <paramref name="cst"/>.</returns>
    Self Constant(Value cst);
    #endregion
  }

  /// <summary>
  /// The interface for abstract predicates about strings.
  /// </summary>
  public interface IStringPredicate : IAbstractDomain
  {
    bool ContainsValue(bool value);
    ProofOutcome ProofOutcome { get; }
  }

  /// <summary>
  /// The interface for implementation of abstract string operation semantics.
  /// </summary>
  /// <typeparam name="StringAbstraction">The type of the string abstraction.</typeparam>
  /// <typeparam name="Variable">The type of variables used in predicates.</typeparam>
  public interface IStringOperations<StringAbstraction, Variable> : IStringAbstractionFactory<StringAbstraction, string>
    where StringAbstraction : IStringAbstraction<StringAbstraction, string>
    where Variable : IEquatable<Variable>
  {
    /// <summary>
    /// Evaluates the <see cref="String.Concat"/> method in abstract.
    /// </summary>
    /// <param name="left">Abstraction or constant value of the left argument.</param>
    /// <param name="right">Abstraction or constant value of the right argument.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction Concat(WithConstants<StringAbstraction> left, WithConstants<StringAbstraction> right);

    /// <summary>
    /// Evaluates the <see cref="String.Insert"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the this argument.</param>
    /// <param name="index">Abstraction of the index.</param>
    /// <param name="other">Abstraction or constant value fo the inserted string.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction Insert(WithConstants<StringAbstraction> self, IndexInterval index, WithConstants<StringAbstraction> other);

    /// <summary>
    /// Evaluates the <see cref="String.Replace(char,char)"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the this argument.</param>
    /// <param name="from">Abstraction of the replaced character.</param>
    /// <param name="to">Abstraction of the replacement character.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction Replace(StringAbstraction self, CharInterval from, CharInterval to);
    /// <summary>
    /// Evaluates the <see cref="String.Replace(string,string)"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the this argument.</param>
    /// <param name="from">Abstraction or constant value of the replaced string.</param>
    /// <param name="to">Abstraction or constant value of the replacement string.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction Replace(WithConstants<StringAbstraction> self, WithConstants<StringAbstraction> from, WithConstants<StringAbstraction> to);

    /// <summary>
    /// Evaluates the <see cref="String.Substring(int)"/> and <see cref="String.Substring(int, int)"/> methods in abstract.
    /// </summary>
    /// <param name="self">Abstraction of the this argument.</param>
    /// <param name="index">Abstraction of the substring index.</param>
    /// <param name="length">Abstraction of the substring length.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction Substring(StringAbstraction self, IndexInterval index, IndexInterval length);
    /// <summary>
    /// Evaluates the <see cref="String.Remove(int)"/> and <see cref="String.Remove(int, int)"/> methods in abstract.
    /// </summary>
    /// <param name="self">Abstraction of the this argument.</param>
    /// <param name="index">Abstraction of the substring index.</param>
    /// <param name="length">Abstraction of the substring length.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction Remove(StringAbstraction self, IndexInterval index, IndexInterval length);

    /// <summary>
    /// Evaluates the <see cref="String.PadLeft(int,char)"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction of the this argument.</param>
    /// <param name="length">Abstraction of the target length.</param>
    /// <param name="fill">Abstraction of padding character.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction PadLeft(StringAbstraction self, IndexInterval length, CharInterval fill);
    /// <summary>
    /// Evaluates the <see cref="String.PadRight(int,char)"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction of the this argument.</param>
    /// <param name="length">Abstraction of the target length.</param>
    /// <param name="fill">Abstraction of padding character.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction PadRight(StringAbstraction self, IndexInterval length, CharInterval fill);

    /// <summary>
    /// Evaluates the <see cref="String.Trim(char[])"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the this argument.</param>
    /// <param name="trimmed">Abstraction or constant value of the array of trimmed characters.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction Trim(WithConstants<StringAbstraction> self, WithConstants<StringAbstraction> trimmed);
    /// <summary>
    /// Evaluates the <see cref="String.TrimStart(char[])"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the this argument.</param>
    /// <param name="trimmed">Abstraction or constant value of the array of trimmed characters.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction TrimStart(WithConstants<StringAbstraction> self, WithConstants<StringAbstraction> trimmed);
    /// <summary>
    /// Evaluates the <see cref="String.TrimEnd(char[])"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the this argument.</param>
    /// <param name="trimmed">Abstraction or constant value of the array of trimmed characters.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction TrimEnd(WithConstants<StringAbstraction> self, WithConstants<StringAbstraction> trimmed);

    /// <summary>
    /// Evaluates the setter of a character in a string in abstract.
    /// </summary>
    /// <param name="self">Abstraction of the this argument.</param>
    /// <param name="index">Abstraction of the index argument.</param>
    /// <param name="value">Abstraction of new character value.</param>
    /// <returns>Abstraction of the return value.</returns>
    StringAbstraction SetCharAt(StringAbstraction self, IndexInterval index, CharInterval value);

    /// <summary>
    /// Evaluates the <see cref="String.IsNullOrEmpty"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the string argument.</param>
    /// <param name="selfVariable">Variable containing the value of the string argument, or <see langword="null"/>.</param>
    /// <returns>Abstraction of the return value.</returns>
    IStringPredicate IsEmpty(StringAbstraction self, Variable selfVariable);

    /// <summary>
    /// Evaluates the <see cref="String.Contains"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the this argument.</param>
    /// <param name="selfVariable">Variable containing the value of the this argument, or <see langword="null"/>.</param>
    /// <param name="other">Abstraction or constant value of the contained string argument.</param>
    /// <param name="otherVariable">Variable containing the value of the contained string argument, or <see langword="null"/>.</param>
    /// <returns>Abstraction of the return value.</returns>
    IStringPredicate Contains(WithConstants<StringAbstraction> self, Variable selfVariable,
      WithConstants<StringAbstraction> other, Variable otherVariable);
    /// <summary>
    /// Evaluates the <see cref="String.StartsWith"/> method, with an argument of <see cref="StringComparison.Ordinal"/>, in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the this argument.</param>
    /// <param name="selfVariable">Variable containing the value of the this argument, or <see langword="null"/>.</param>
    /// <param name="other">Abstraction or constant value of the contained string argument.</param>
    /// <param name="otherVariable">Variable containing the value of the contained string argument, or <see langword="null"/>.</param>
    /// <returns>Abstraction of the return value.</returns>
    IStringPredicate StartsWithOrdinal(WithConstants<StringAbstraction> self, Variable selfVariable,
      WithConstants<StringAbstraction> other, Variable otherVariable);
    /// <summary>
    /// Evaluates the <see cref="String.EndsWith"/> method, with an argument of <see cref="StringComparison.Ordinal"/>, in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the this argument.</param>
    /// <param name="selfVariable">Variable containing the value of the this argument, or <see langword="null"/>.</param>
    /// <param name="other">Abstraction or constant value of the contained string argument.</param>
    /// <param name="otherVariable">Variable containing the value of the contained string argument, or <see langword="null"/>.</param>
    /// <returns>Abstraction of the return value.</returns>
    IStringPredicate EndsWithOrdinal(WithConstants<StringAbstraction> self, Variable selfVariable,
      WithConstants<StringAbstraction> other, Variable otherVariable);

    /// <summary>
    /// Evaluates the <see cref="String.Equals"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the left argument.</param>
    /// <param name="selfVariable">Variable containing the value of the left argument, or <see langword="null"/>.</param>
    /// <param name="other">Abstraction or constant value of the right argument.</param>
    /// <param name="otherVariable">Variable containing the value of the right argument, or <see langword="null"/>.</param>
    /// <returns>Abstraction of the return value.</returns>
    IStringPredicate Equals(WithConstants<StringAbstraction> self, Variable selfVariable,
      WithConstants<StringAbstraction> other, Variable otherVariable);

    /// <summary>
    /// Evaluates the <see cref="String.CompareOrdinal"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the left argument.</param>
    /// <param name="other">Abstraction or constant value of the right argument.</param>
    /// <returns>Abstraction of the return value.</returns>
    CompareResult CompareOrdinal(WithConstants<StringAbstraction> self, WithConstants<StringAbstraction> other);

    /// <summary>
    /// Evaluates the <see cref="String.Length"/> property in abstract.
    /// </summary>
    /// <param name="self">Abstraction of the this argument.</param>
    /// <returns>Abstraction of the return value.</returns>
    IndexInterval GetLength(StringAbstraction self);

    /// <summary>
    /// Evaluates the <see cref="String.IndexOf"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the this argument.</param>
    /// <param name="needle">Abstraction or constant value of the needle argument.</param>
    /// <param name="offset">Abstraction of the offset argument.</param>
    /// <param name="count">Abstraction of the count argument.</param>
    /// <returns>Abstraction of the return value.</returns>
    IndexInterval IndexOf(WithConstants<StringAbstraction> self, WithConstants<StringAbstraction> needle, IndexInterval offset, IndexInterval count);
    /// <summary>
    /// Evaluates the <see cref="String.LastIndexOf"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction or constant value of the this argument.</param>
    /// <param name="needle">Abstraction or constant value of the needle argument.</param>
    /// <param name="offset">Abstraction of the offset argument.</param>
    /// <param name="count">Abstraction of the count argument.</param>
    /// <returns>Abstraction of the return value.</returns>
    IndexInterval LastIndexOf(WithConstants<StringAbstraction> self, WithConstants<StringAbstraction> needle, IndexInterval offset, IndexInterval count);

    /// <summary>
    /// Evaluates the getter of a character in a string in abstract.
    /// </summary>
    /// <param name="self">Abstraction of the this argument.</param>
    /// <param name="index">Abstraction of the index argument.</param>
    /// <returns>Abstraction of the return value.</returns>
    CharInterval GetCharAt(StringAbstraction self, IndexInterval index);

    /// <summary>
    /// Evaluates the <see cref="System.Text.RegularExpressions.Regex.IsMatch"/> method in abstract.
    /// </summary>
    /// <param name="self">Abstraction of the this argument.</param>
    /// <param name="selfVariable">Variable containing the value of the left argument, or <see langword="null"/>.</param>
    /// <param name="regex">The AST of the regex.</param>
    /// <returns>Abstraction of the return value.</returns>
    IStringPredicate RegexIsMatch(StringAbstraction self, Variable selfVariable, Regex.AST.Element regex);
  }

  /// <summary>
  /// The interface for a factory creating abstract elements
  /// of an abstract domain for strings.
  /// </summary>
  /// <typeparam name="Abstraction">Type of the string abstract domain.</typeparam>
  /// <typeparam name="Value">Type of the string values.</typeparam>
  public interface IStringAbstractionFactory<Abstraction, Value>
   where Abstraction : IStringAbstraction<Abstraction, Value>
  {
    /// <summary>
    /// Gets the top element of the domain.
    /// </summary>
    Abstraction Top { get; }
    /// <summary>
    /// Gets an abstract element from a concrete (constant) value.
    /// </summary>
    /// <param name="constant">The constant value.</param>
    /// <returns>An abstract element overapproximating a set of values containing <paramref name="constant"/>.</returns>
    Abstraction Constant(Value constant);
  }

  /// <summary>
  /// Stores either an abstract element or a string constant.
  /// </summary>
  /// <typeparam name="Abstraction">Type of the string abstract domain.</typeparam>
  public struct WithConstants<Abstraction>
    where Abstraction : IStringAbstraction<Abstraction, string>
  {
    private readonly string constant;
    private readonly Abstraction abstraction;

    /// <summary>
    /// Wraps an abstract element.
    /// </summary>
    /// <param name="abstraction">The abstract element.</param>
    public WithConstants(Abstraction abstraction)
    {
      this.constant = null;
      this.abstraction = abstraction;
    }

    /// <summary>
    /// Wraps a string constant
    /// </summary>
    /// <param name="abstraction">The constant value.</param>
    public WithConstants(string constant)
    {
      this.constant = constant;
      this.abstraction = default(Abstraction);
    }

    /// <summary>
    /// Determines whether the value is a constant.
    /// </summary>
    public bool IsConstant
    {
      get
      {
        return constant != null;
      }
    }

    /// <summary>
    /// Determines whether the value is a bottom abstract element.
    /// </summary>
    public bool IsBottom
    {
      get
      {
        return constant == null && abstraction.IsBottom;
      }
    }

    /// <summary>
    /// Gets the abstract element for the stored value. If it is a constant,
    /// it is converted to a abstract element.
    /// </summary>
    /// <param name="factory">Factory for creating the abstract elements.</param>
    /// <returns></returns>
    public Abstraction ToAbstract(IStringAbstractionFactory<Abstraction, string> factory)
    {
      if (constant == null)
      {
        return abstraction;
      }
      else
      {
        return factory.Constant(constant);
      }
    }

    /// <summary>
    /// Gets the stored constant.
    /// </summary>
    public string Constant
    {
      get
      {
        return constant;
      }
    }
    /// <summary>
    /// Gets the stored abstract element.
    /// </summary>
    public Abstraction Abstract
    {
      get
      {
        return abstraction;
      }
    }
  }


}