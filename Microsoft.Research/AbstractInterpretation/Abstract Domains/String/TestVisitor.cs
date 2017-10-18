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

// Modified by Vlastimil Dort (2015-2016)
// Master thesis String Analysis for Code Contracts

using Microsoft.Research.AbstractDomains.Expressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Expression visitor implementing the TestTrue and TestFalse operations on an abstract domain for strings.
    /// </summary>
    /// <typeparam name="AbstractDomain">Type of the string abstract domain.</typeparam>
    /// <typeparam name="Variable">Type of the variables.</typeparam>
    /// <typeparam name="Expression">Type of the expression.</typeparam>
    internal abstract class StringDomainTestVisitor<AbstractDomain, Variable, Expression>
      where AbstractDomain : IAbstractDomainForEnvironments<Variable, Expression>
      where Variable : IEquatable<Variable>
    {
        private readonly StringDomainTestTrueVisitor<AbstractDomain, Variable, Expression> testTrueVisitor;
        private readonly StringDomainTestFalseVisitor<AbstractDomain, Variable, Expression> testFalseVisitor;

        public StringDomainTestVisitor(IExpressionDecoder<Variable, Expression> decoder)
        {
            this.testTrueVisitor = new StringDomainTestTrueVisitor<AbstractDomain, Variable, Expression>(decoder, this);
            this.testFalseVisitor = new StringDomainTestFalseVisitor<AbstractDomain, Variable, Expression>(decoder, this);

            testFalseVisitor.TrueVisitor = testTrueVisitor;
            testTrueVisitor.FalseVisitor = testFalseVisitor;
        }
        internal protected abstract AbstractDomain TestVariableHolds(Variable var, bool holds, AbstractDomain data);

        public AbstractDomain VisitTrue(Expression e, AbstractDomain data)
        {
            return testTrueVisitor.Visit(e, data);
        }
        public AbstractDomain VisitFalse(Expression e, AbstractDomain data)
        {
            return testFalseVisitor.Visit(e, data);
        }
    }

    /// <summary>
    /// Expression visitor implementing the TestTrue operation on an abstract domain for strings.
    /// </summary>
    /// <typeparam name="AbstractDomain">Type of the string abstract domain.</typeparam>
    /// <typeparam name="Variable">Type of the variables.</typeparam>
    /// <typeparam name="Expression">Type of the expression.</typeparam>
    internal class StringDomainTestTrueVisitor<AbstractDomain, Variable, Expression> : TestTrueVisitor<AbstractDomain, Variable, Expression>
      where AbstractDomain : IAbstractDomainForEnvironments<Variable, Expression>
      where Variable : IEquatable<Variable>
    {
        private readonly StringDomainTestVisitor<AbstractDomain, Variable, Expression> parent;

        internal StringDomainTestTrueVisitor(IExpressionDecoder<Variable, Expression> decoder, StringDomainTestVisitor<AbstractDomain, Variable, Expression> parent) :
          base(decoder)
        {
            this.parent = parent;
        }

        public override AbstractDomain VisitEqual(Expression left, Expression right, Expression original, AbstractDomain data)
        {
            int value;
            if (Decoder.IsConstantInt(right, out value))
            {
                if (value == 0)
                {
                    return FalseVisitor.Visit(left, data);
                }
            }
            return data;
        }

        public override AbstractDomain VisitLessEqualThan(Expression left, Expression right, Expression original, AbstractDomain data)
        {
            return data;
        }

        public override AbstractDomain VisitLessThan(Expression left, Expression right, Expression original, AbstractDomain data)
        {
            return data;
        }

        public override AbstractDomain VisitNotEqual(Expression left, Expression right, Expression original, AbstractDomain data)
        {
            return data;
        }

        public override AbstractDomain VisitVariable(Variable var, Expression original, AbstractDomain data)
        {
            return parent.TestVariableHolds(var, true, data);
        }
    }

    /// <summary>
    /// Expression visitor implementing the TestFalse operation on an abstract domain for strings.
    /// </summary>
    /// <typeparam name="AbstractDomain">Type of the string abstract domain.</typeparam>
    /// <typeparam name="Variable">Type of the variables.</typeparam>
    /// <typeparam name="Expression">Type of the expression.</typeparam>
    internal class StringDomainTestFalseVisitor<AbstractDomain, Variable, Expression> : TestFalseVisitor<AbstractDomain, Variable, Expression>
      where AbstractDomain : IAbstractDomainForEnvironments<Variable, Expression>
      where Variable : IEquatable<Variable>
    {

        private readonly StringDomainTestVisitor<AbstractDomain, Variable, Expression> parent;
        internal StringDomainTestFalseVisitor(IExpressionDecoder<Variable, Expression> decoder, StringDomainTestVisitor<AbstractDomain, Variable, Expression> parent) :
          base(decoder)
        {
            this.parent = parent;
        }

        public override AbstractDomain VisitEqual(Expression left, Expression right, Expression original, AbstractDomain data)
        {
            int value;
            if (Decoder.IsConstantInt(right, out value))
            {
                if (value == 0)
                {
                    return TrueVisitor.Visit(left, data);
                }
            }
            return data;
        }

        public override AbstractDomain VisitLessEqualThan(Expression left, Expression right, Expression original, AbstractDomain data)
        {
            return data;
        }

        public override AbstractDomain VisitLessThan(Expression left, Expression right, Expression original, AbstractDomain data)
        {
            return data;
        }

        public override AbstractDomain VisitNotEqual(Expression left, Expression right, Expression original, AbstractDomain data)
        {
            return data;
        }

        public override AbstractDomain VisitVariable(Variable var, Expression original, AbstractDomain data)
        {
            return parent.TestVariableHolds(var, false, data);
        }
    }
}
