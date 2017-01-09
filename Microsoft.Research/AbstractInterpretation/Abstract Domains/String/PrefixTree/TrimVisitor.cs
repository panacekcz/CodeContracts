using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings.PrefixTree
{
    /// <summary>
    /// Visitor of prefix tree performing trimming
    /// </summary>
    /// <remarks>
    /// To trim from the start
    /// 
    /// </remarks>
    class TrimVisitor : PrefixTreeVisitor<Void>
    {
        int depth = 0;
        IndexInterval trimAmount;
        CharInterval trimCharInterval;
        bool trimFromRoot = false;

        public TrimVisitor(CharInterval trimCharInterval)
        {
            this.trimCharInterval = trimCharInterval;
        }


        protected override Void VisitInnerNode(InnerNode inn)
        {
            throw new NotImplementedException();
        }

        protected override Void VisitRepeatNode(RepeatNode inn)
        {
            if (trimAmount.UpperBound >= depth)
                trimFromRoot = true;

            throw new NotImplementedException();
        }
    }
}
