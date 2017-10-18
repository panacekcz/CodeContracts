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

// Created by Vlastimil Dort (2016)


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    /// <summary>
    /// Provides information about ordering of variables.
    /// </summary>
    /// <typeparam name="Variable">Type of variables in the queries.</typeparam>
    public interface IStringOrderQuery<Variable>
        where Variable : IEquatable<Variable>
    {
        /// <summary>
        /// Check whether it is known that the value of one variable must be less than or equal to the value of another variable.
        /// </summary>
        /// <param name="leftVariable">The variable on the left side of the comparison.</param>
        /// <param name="rightVariable">The variable on the right side of the comparison.</param>
        /// <returns>True, if <paramref name="leftVariable"/> is known to have a value less than or equal to <paramref name="rightVariable"/>.</returns>
        bool CheckMustBeLessEqualThan(Variable leftVariable, Variable rightVariable);
    }

    /// <summary>
    /// An implementation of <see cref="IStringOrderQuery{Variable}"/>, which does not 
    /// provide any information of the ordering of variables.
    /// </summary>
    /// <typeparam name="Variable">Type of variables in the queries.</typeparam>
    public class NoOrderQuery<Variable> : IStringOrderQuery<Variable>
        where Variable : IEquatable<Variable>
    {
        #region IStringOrderQuery<Variable> implementation
        public bool CheckMustBeLessEqualThan(Variable leftVariable, Variable rightVariable)
        {
            return false;
        }
        #endregion
    }
}
