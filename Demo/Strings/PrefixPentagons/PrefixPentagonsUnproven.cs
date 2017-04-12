using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefixPentagons
{
    public class PrefixPentagonsUnproven
    {
        public void Concat(string s, string t)
        {
            StringBuilder sb = new StringBuilder(s);
            sb.Append(t);
            string u = sb.ToString();

            Contract.Assert(u.StartsWith(t, StringComparison.Ordinal));
        }

        public void Insert(string s, string t)
        {
            StringBuilder sb = new StringBuilder(s);
            sb.Insert(0, t);
            string u = sb.ToString();

            Contract.Assert(u.StartsWith(s, StringComparison.Ordinal));
        }
        public void PostcondUnknown(StringBuilder pre, string s)
        {
            Contract.Ensures(pre.ToString().StartsWith(Contract.OldValue(pre.ToString()), StringComparison.Ordinal));
            pre.Clear();
            pre.Append(s);
        }
        
        public void Postcond(StringBuilder pre)
        {
            Contract.Ensures(pre.ToString().StartsWith(Contract.OldValue(pre.ToString()), StringComparison.Ordinal));
            pre.Insert(0, "YX");
        }
        private void ArbitraryCall(StringBuilder sb)
        {
        }
        public void Call(StringBuilder pre)
        {
            Contract.Ensures(pre.ToString().StartsWith(Contract.OldValue(pre.ToString()), StringComparison.Ordinal));
            ArbitraryCall(pre);
        }

    }
}
