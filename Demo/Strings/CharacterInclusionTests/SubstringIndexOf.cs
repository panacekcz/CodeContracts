using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CharacterInclusionTests
{
    class SubstringIndexOf
    {
        public string AfterSlash(string argument)
        {
            Contract.Requires(argument.Contains("/"));

            return argument.Substring(argument.IndexOf('/'));
        }
    }
}
