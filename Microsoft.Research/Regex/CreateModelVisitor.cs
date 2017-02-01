using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Regex.AST;
using Microsoft.Research.Regex.Model;

namespace Microsoft.Research.Regex
{

    public static class ASTBuilder
    {
        public static AST.Alternation Union(AST.Element e, AST.Element f)
        {
            AST.Alternation a = new AST.Alternation();
            if (e is AST.Alternation)
                a.Patterns.AddRange(((AST.Alternation)e).Patterns);
            else
                a.Patterns.Add(e);

            if (f is AST.Alternation)
                a.Patterns.AddRange(((AST.Alternation)f).Patterns);
            else
                a.Patterns.Add(f);

            return a;
        }
    }

    /// <summary>
    /// Collects group captures from a regex ast.
    /// </summary>
    internal class CollectCapturesVisitor : RegexVisitor<bool, Void>
    {
        private Dictionary<string, AST.Element> captures = new Dictionary<string, AST.Element>();

        protected override bool Visit(Boundary element, ref Void data)
        {
            return false;
        }

        protected override bool Visit(Comment element, ref Void data)
        {
            return false;
        }

        protected override bool Visit(Empty element, ref Void data)
        {
            return false;
        }

        protected override bool Visit(NonBacktracking element, ref Void data)
        {
            return VisitElement(element.Content, ref data);
        }

        protected override bool Visit(OptionsGroup element, ref Void data)
        {
            return VisitElement(element.Content, ref data);
        }

        protected override bool Visit(SimpleGroup element, ref Void data)
        {
            return VisitElement(element.Content, ref data);
        }

        protected override bool Visit(SingleElement element, ref Void data)
        {
            return false;
        }

        protected override bool Visit(Reference element, ref Void data)
        {
            return false;
        }

        protected override bool Visit(Options element, ref Void data)
        {
            return false;
        }

        protected override bool Visit(AST.Loop element, ref Void data)
        {
            return VisitElement(element.Content, ref data);
        }

        protected override bool Visit(AST.Concatenation element, ref Void data)
        {
            bool found = false;
            foreach (var c in element.Parts)
                found |= VisitElement(c, ref data);
            return found;
        }

        protected override bool Visit(Capture element, ref Void data)
        {
            string groupName = element.Name;
            AST.Element previous;
            if(captures.TryGetValue(groupName, out previous))
            {
                captures[groupName] = ASTBuilder.Union(previous, element.Content);
            }
            else
            {
                captures.Add(groupName, element.Content);
            }

            VisitElement(element.Content, ref data);

            return true;
        }

        protected override bool Visit(Assertion element, ref Void data)
        {
            //Grouping works in assertions as well
            return VisitElement(element.Content, ref data);
        }

        protected override bool Visit(AST.Anchor element, ref Void data)
        {
            return false;
        }

        protected override bool Visit(Alternation element, ref Void data)
        {
            bool found = false;
            foreach (var c in element.Patterns)
                found |= VisitElement(c, ref data);
            return found;
        }

        protected override bool VisitUnsupported(AST.Element element, ref Void data)
        {
            throw new NotImplementedException();
        }
    }

    internal class CreateModelVisitor : RegexVisitor<Model.Element, Void>
    {
        

        public Model.Element CreateModelForAST(AST.Element ast)
        {
            Void data;
            return VisitElement(ast, ref data);
        }


        protected override Model.Element Visit(Assertion element, ref Void data)
        {
            throw new NotImplementedException();
        }

        protected override Model.Element Visit(Capture element, ref Void data)
        {
            throw new NotImplementedException();
        }

        protected override Model.Element Visit(AST.Concatenation element, ref Void data)
        {
            Model.Concatenation concat = new Model.Concatenation();

            foreach(var part in element.Parts)
            {
                Model.Element partModel = VisitElement(part, ref data);
                if(partModel is Model.Concatenation)
                {
                    var partConcat = (Model.Concatenation)partModel;
                    concat.Parts.AddRange(partConcat.Parts);
                }
                else
                {
                    concat.Parts.Add(partModel);
                }
            }
            return concat;
        }

        protected override Model.Element Visit(AST.Loop element, ref Void data)
        {
            var inner = VisitElement(element.Content, ref data);
            return new Model.Loop(inner, element.Min, element.Max);
        }

        protected override Model.Element Visit(Options element, ref Void data)
        {
            throw new NotImplementedException();
        }

        protected override Model.Element Visit(Reference element, ref Void data)
        {
            throw new NotImplementedException();
        }

        protected override Model.Element Visit(SingleElement element, ref Void data)
        {
            return new Model.Character(element.MustMatchRanges, element.CanMatchRanges);
        }

        protected override Model.Element Visit(SimpleGroup element, ref Void data)
        {
            return VisitElement(element.Content, ref data);
        }

        protected override Model.Element Visit(OptionsGroup element, ref Void data)
        {
            throw new NotImplementedException();
        }

        protected override Model.Element Visit(NonBacktracking element, ref Void data)
        {
            return new Unknown(VisitElement(element.Content, ref data));
        }

        protected override Model.Element Visit(Empty element, ref Void data)
        {
            return new Model.Concatenation();
        }

        protected override Model.Element Visit(Comment element, ref Void data)
        {
            return new Model.Concatenation();
        }

        protected override Model.Element Visit(Boundary element, ref Void data)
        {
            return new Model.Unknown(new Model.Concatenation());
        }

        protected override Model.Element Visit(AST.Anchor element, ref Void data)
        {
            if(element.Kind == AST.AnchorKind.LineStart)
            {
                return Model.Anchor.Begin;
            }

            else if (element.Kind == AnchorKind.End)
            {
                return Model.Anchor.End;
            }

            throw new NotImplementedException();
        }

        protected override Model.Element Visit(Alternation element, ref Void data)
        {
            Model.Union union = new Model.Union();

            foreach (var part in element.Patterns)
            {
                Model.Element partModel = VisitElement(part, ref data);
                if (partModel is Model.Union)
                {
                    var partUnion = (Model.Union)partModel;
                    union.Patterns.AddRange(partUnion.Patterns);
                }
                else
                {
                    union.Patterns.Add(partModel);
                }
            }
            return union;
        }

        protected override Model.Element VisitUnsupported(AST.Element element, ref Void data)
        {
            return new Model.Unknown(new Model.Loop(new Model.Character(new CharRanges(), new CharRanges(new CharRange(char.MinValue, char.MaxValue))), 0, -1));
        }
    }
}
