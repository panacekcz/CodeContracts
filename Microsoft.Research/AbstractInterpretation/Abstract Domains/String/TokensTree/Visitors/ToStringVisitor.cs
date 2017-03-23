using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.TokensTree
{
    /// <summary>
    /// Generates a string representation of a tokens tree.
    /// </summary>
    /// <remarks>
    /// Repeat node is *, inner node is {cX...}. (if the nodes is not accepting) or {cX...}! if accepting.
    /// For str it is {s{t{r{}!}.}.}.
    /// </remarks>
    internal class ToStringVisitor : TokensTreeVisitor<string>
    {
        public string ToString(TokensTreeNode node)
        {
            return VisitNode(node);
        }

        #region TokensTreeVisitor<string> overrides

        protected override string VisitInnerNode(InnerNode inn)
        {
            StringBuilder sb = new StringBuilder();

            sb.Append('{');

            foreach (var child in inn.children.OrderBy(child => child.Key))
            {
                sb.Append(child.Key);
                sb.Append(VisitNode(child.Value));
            }
           
            sb.Append('}');
            sb.Append(inn.Accepting ? '!' : '.');

            return sb.ToString();
        }

        protected override string VisitRepeatNode(RepeatNode inn)
        {
            return "*";
        }
        #endregion

    }

 
}
