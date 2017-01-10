using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    public interface IStringOrderQuery<Variable>
        where Variable : IEquatable<Variable>
    {
        bool CheckMustBeLessEqualThan(Variable leftVariable, Variable rightVariable);
    }

    public class NoOrderQuery<Variable> : IStringOrderQuery<Variable>
        where Variable : IEquatable<Variable>
    {
        public bool CheckMustBeLessEqualThan(Variable leftVariable, Variable rightVariable)
        {
            return false;
        }
    }
}
