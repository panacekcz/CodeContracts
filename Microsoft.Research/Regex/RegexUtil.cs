using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex
{
    public static class RegexUtil
    {
        /// <summary>
        /// Converts a regex string to a model.
        /// </summary>
        /// <param name="regex">Regex </param>
        /// <returns>Model of <paramref name="regex"/>.</returns>
        public static Model.Element ModelForRegex(string regex)
        {
            var ast = RegexParser.Parse(regex);
            var modelCreator = new CreateModelVisitor();

            return modelCreator.CreateModelForAST(ast);
        }

        public static string RegexForModel(Model.Element e)
        {
            var astCreator = new ModelASTVisitor();
            return astCreator.CreateASTForModel(e).ToString();
        }
    }
}
