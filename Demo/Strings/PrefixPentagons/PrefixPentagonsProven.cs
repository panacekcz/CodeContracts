using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrefixPentagons
{
    public class PrefixPentagonsProven
    {
        public void Concat(string s, string t)
        {
            StringBuilder sb = new StringBuilder(s);
            sb.Append(t);
            string u = sb.ToString();

            Contract.Assert(u.StartsWith(s, StringComparison.Ordinal));
        }


        public void Insert(string s, string t)
        {
            StringBuilder sb = new StringBuilder(s);
            sb.Insert(0, t);
            string u = sb.ToString();

            Contract.Assert(u.StartsWith(t, StringComparison.Ordinal));
        }

        public void PostcondUnchanged(StringBuilder pre)
        {
            Contract.Ensures(pre.ToString().StartsWith(Contract.OldValue(pre.ToString()), StringComparison.Ordinal));
        }
        public void Postcond(StringBuilder pre)
        {
            Contract.Ensures(pre.ToString().StartsWith(Contract.OldValue(pre.ToString()), StringComparison.Ordinal));
            pre.Append("YX");
        }

        public void Constants()
        {
            StringBuilder sb = new StringBuilder("abcd");
            string a = sb.ToString();
            sb.Append("ghi");
            Contract.Assert(sb.ToString().StartsWith(a, StringComparison.Ordinal));
        }

        public void Call(StringBuilder pre)
        {
            Contract.Ensures(pre.ToString().StartsWith(Contract.OldValue(pre.ToString()), StringComparison.Ordinal));
            PostcondUnchanged(pre);
        }
    }
}
