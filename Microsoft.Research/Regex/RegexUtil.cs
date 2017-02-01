using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex
{
    public static class RegexUtil
    {
        public static Model.Element ModelForRegex(String regex)
        {
            var ast = RegexParser.Parse(regex);
            var modelCreator = new CreateModelVisitor();

            return modelCreator.CreateModelForAST(ast);

        }
    }
}
