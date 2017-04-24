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

        protected override AST.Element VisitCharacter(Model.Character character, ref Void data)
        {
            var ranges = character.CanMatch.Ranges.ToArray();

            if (ranges.Length == 1 && ranges[0].Low == ranges[0].High)
                return new AST.Character(ranges[0].Low);
            else
            {
                var setRanges = new List<AST.SingleElement>();

                foreach(var range in ranges)
                {
                    if(range.Low == range.High)
                        setRanges.Add(new AST.Character(range.Low));
                    else
                        setRanges.Add(new AST.Range(range.Low, range.High));
                }

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
                concatAST.Parts.Add(VisitElement(part, ref data));
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
            return new AST.Loop(loop.Min, loop.Max, VisitElement(loop.Pattern, ref data), false);
            
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
