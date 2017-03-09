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
