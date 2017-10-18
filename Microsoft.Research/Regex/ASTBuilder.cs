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
    /// <summary>
    /// Helpers for building regex AST.
    /// </summary>
    public static class ASTBuilder
    {
        /// <summary>
        /// Builds an AST for a union of two regexes, with flattening nested unions.
        /// </summary>
        /// <param name="element1"></param>
        /// <param name="element2"></param>
        /// <returns></returns>
        public static AST.Alternation Union(AST.Element element1, AST.Element element2)
        {
            AST.Alternation a = new AST.Alternation();
            if (element1 is AST.Alternation)
                a.Patterns.AddRange(((AST.Alternation)element1).Patterns);
            else
                a.Patterns.Add(element1);

            if (element2 is AST.Alternation)
                a.Patterns.AddRange(((AST.Alternation)element2).Patterns);
            else
                a.Patterns.Add(element2);

            return a;
        }
    }
}
