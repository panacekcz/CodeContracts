// Copyright (c) Charles University
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

// Created by Vlastimil Dort

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
        /// <param name="regex">Regex string.</param>
        /// <returns>Model of <paramref name="regex"/>.</returns>
        public static Model.Element ModelForRegex(string regex)
        {
            var ast = RegexParser.Parse(regex);
            var modelCreator = new CreateModelVisitor();

            return modelCreator.CreateModelForAST(ast);
        }

        /// <summary>
        /// Converts a regex model to a string.
        /// </summary>
        /// <param name="model">Regex model </param>
        /// <returns>String representation of <paramref name="model"/>.</returns>
        public static string RegexForModel(Model.Element model)
        {
            var astCreator = new ModelASTVisitor();
            return astCreator.CreateASTForModel(model).ToString();
        }
    }
}
