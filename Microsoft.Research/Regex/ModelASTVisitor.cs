using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Regex.AST;
using Microsoft.Research.Regex.Model;

namespace Microsoft.Research.Regex
{
    class ModelASTVisitor : Model.ModelVisitor<AST.Element, Void>
    {
        protected override AST.Element VisitAnchor(End anchor, ref Void data)
        {
            return new AST.Anchor(AnchorKind.End);
        }

        protected override AST.Element VisitAnchor(Begin anchor, ref Void data)
        {
            return new AST.Anchor(AnchorKind.LineStart);
        }

        private AST.SingleElement CharRange(char low, char high)
        {
            if (low == high)
                return new AST.Character(low);
            else
                return new AST.Range(low, high);
        }

        protected override AST.Element VisitCharacter(Model.Character character, ref Void data)
        {
            var ranges = character.CanMatch.Ranges.ToArray();

            if (ranges.Length == 1 && ranges[0].Low == ranges[0].High)
            {
                // Single character
                return new AST.Character(ranges[0].Low);
            }
            else
            {
                Array.Sort(ranges, (r1, r2) => r1.Low.CompareTo(r2.Low));

                // Set of characters
                var setRanges = new List<AST.SingleElement>();
                int lastLow = 0;
                int lastHigh = -1;

                foreach (var range in ranges)
                {
                    if(range.Low - 1 != lastHigh)
                    {
                        if(lastHigh != -1)
                            setRanges.Add(CharRange((char)lastLow, (char)lastHigh));
                        lastLow = range.Low;
                    }

                    lastHigh = range.High;
                }

                if (lastHigh != -1)
                    setRanges.Add(CharRange((char)lastLow, (char)lastHigh));

                AST.CharacterSet characterSet = new CharacterSet(false, setRanges, null);
                return characterSet;
            }
            
        }

        public AST.Element CreateASTForModel(Model.Element e)
        {
            Void v;
            return VisitElement(e, ref v);
        }

        protected override AST.Element VisitConcatenation(Model.Concatenation concatenation, ref Void data)
        {
            var concatAST = new AST.Concatenation();
            foreach(var part in concatenation.Parts)
            {
                var element = VisitElement(part, ref data);

                if (element is Alternation)
                    element = new SimpleGroup(element);

                concatAST.Parts.Add(element);
            }
            return concatAST;
        }

        protected override AST.Element VisitLookaround(Lookaround lookaround, ref Void data)
        {
            var inner = VisitElement(lookaround.Pattern, ref data);
            return new AST.Assertion(false, lookaround.Behind, inner);
        }

        protected override AST.Element VisitLoop(Model.Loop loop, ref Void data)
        {
            var inner = VisitElement(loop.Pattern, ref data);
            if (loop.Min == 1 && loop.Max == 1)
            {
                return inner;
            }
            else
            {
                if (!(inner is AST.SingleElement))
                    inner = new AST.SimpleGroup(inner);
                return new AST.Loop(loop.Min, loop.Max, inner, false);
            }
            
        }

        protected override AST.Element VisitUnion(Union union, ref Void data)
        {
            var unionAST = new AST.Alternation();
            foreach (var part in union.Patterns)
            {
                unionAST.Patterns.Add(VisitElement(part, ref data));
            }
            return unionAST;
        }

        protected override AST.Element VisitUnknown(Unknown unknown, ref Void data)
        {
            throw new UnknownRegexException("Cannot create AST of regex");
        }
    }
}
