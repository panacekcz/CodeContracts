﻿using Microsoft.Research.Regex.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.Regex
{
    public static class ModelBuilder
    {
        public static Element AllStrings()
        {
            var allChars = new CharRanges(new CharRange(char.MinValue, char.MaxValue));
            return new Loop(new Character(allChars, allChars), 0, Loop.Unbounded);
        }
    }
}