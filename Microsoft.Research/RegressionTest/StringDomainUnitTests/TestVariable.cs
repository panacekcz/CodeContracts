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

using Microsoft.Research.AbstractDomains.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StringDomainUnitTests
{
    /// <summary>
    /// A class used as a type of variable in tests of 
    /// predicate methods.
    /// </summary>
    public class TestVariable : IEquatable<TestVariable>
    {
        public static readonly TestVariable Var1 = new TestVariable(ExpressionType.String);
        public static readonly TestVariable Var2 = new TestVariable(ExpressionType.String);
        public static readonly TestVariable Var3 = new TestVariable(ExpressionType.String);

        public static readonly TestVariable BoolVar = new TestVariable(ExpressionType.Bool);

        internal ExpressionType expressionType;

        private TestVariable(ExpressionType type) { expressionType = type; }

        public bool Equals(TestVariable other)
        {
            return object.ReferenceEquals(this, other);
        }
    }
}
