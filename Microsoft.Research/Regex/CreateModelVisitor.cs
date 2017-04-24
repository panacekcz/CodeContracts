using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.Regex.AST;
using Microsoft.Research.Regex.Model;

namespace Microsoft.Research.Regex
{
    internal struct Void { }

    /// <summary>
    /// Collects group captures from a regex AST.
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

    /// <summary>
    /// Creates regex model from regex AST.
    /// </summary>
    internal class CreateModelVisitor : RegexVisitor<Model.Element, Void>
    {
        /// <summary>
        /// Creates regex model from regex AST.
        /// </summary>
        public Model.Element CreateModelForAST(AST.Element ast)
        {
            Void data;
            return VisitElement(ast, ref data);
        }

        protected override Model.Element Visit(Assertion element, ref Void data)
        {
            if (element.Negative)
            {
                // Negative assertions not supported
                return new Model.Unknown(new Model.Concatenation());
            }
            else
            {
                return new Model.Lookaround(VisitElement(element.Content, ref data), element.Behind);
            }
        }

        protected override Model.Element Visit(Capture element, ref Void data)
        {
            return VisitElement(element.Content, ref data);
        }

        protected override Model.Element Visit(AST.Concatenation element, ref Void data)
        {
            // For concatenation of 1 element, do not create an element
            if (element.Parts.Count == 1)
                return VisitElement(element.Parts[0], ref data);

            Model.Concatenation concat = new Model.Concatenation();

            foreach(var part in element.Parts)
            {
                Model.Element partModel = VisitElement(part, ref data);
                // Flatten nested concatenations
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

            // If due to flattening, the result concatenation has 1 element, use it directly
            if (concat.Parts.Count == 1)
                return concat.Parts[0];

            return concat;
        }

        protected override Model.Element Visit(AST.Loop element, ref Void data)
        {
            // Max 0 occurences = empty
            if (element.Max == 0)
                return new Model.Concatenation();

            var inner = VisitElement(element.Content, ref data);

            // Min = max = 1 occurence - use the content directly
            if (element.Max == 1 && element.Min == 1){
                return inner;
            }

            return new Model.Loop(inner, element.Min, element.Max);
        }

        protected override Model.Element Visit(Options element, ref Void data)
        {
            // This should not happen, because AST with such elements should be rejected
            throw new InvalidOperationException();
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
            // Simple non-capturing group is transparent.
            return VisitElement(element.Content, ref data);
        }

        protected override Model.Element Visit(OptionsGroup element, ref Void data)
        {
            return VisitUnsupported(element, ref data);
        }

        protected override Model.Element Visit(NonBacktracking element, ref Void data)
        {
            return new Unknown(VisitElement(element.Content, ref data));
        }

        protected override Model.Element Visit(Empty element, ref Void data)
        {
            // Empty element is modeled by an empty concatenation
            return new Model.Concatenation();
        }

        protected override Model.Element Visit(Comment element, ref Void data)
        {
            // Comment is treated as an empty element and modeled by an empty concatenation
            return new Model.Concatenation();
        }

        protected override Model.Element Visit(Boundary element, ref Void data)
        {
            return new Model.Unknown(new Model.Concatenation());
        }

        protected override Model.Element Visit(AST.Anchor element, ref Void data)
        {
            // We assume we are not in multiline mode
            if(element.Kind == AST.AnchorKind.LineStart || element.Kind == AST.AnchorKind.StringStart)
            {
                // Start of string
                return Model.Anchor.Begin;
            }
            else if (element.Kind == AnchorKind.End)
            {
                // End of string
                return Model.Anchor.End;
            }
            else if(element.Kind == AnchorKind.LineEnd || element.Kind == AnchorKind.StringEnd) {
                // End of string or 
                return new Model.Union(Model.Anchor.End, new Model.Unknown(new Model.Concatenation()));
            }
            else
            {
                return new Model.Unknown(new Model.Concatenation());
            }
        }


        protected override Model.Element Visit(Alternation element, ref Void data)
        {
            // For union of 1 element, do not create an element
            if (element.Patterns.Count == 1)
                return VisitElement(element.Patterns[0], ref data);

            Model.Union union = new Model.Union();

            foreach (var part in element.Patterns)
            {
                Model.Element partModel = VisitElement(part, ref data);
                // Flatten nested unions
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

            // If, due to flattening, the result concatenation has 1 element, use it directly
            if (union.Patterns.Count == 1)
                return union.Patterns[0];

            return union;
        }

        protected override Model.Element VisitUnsupported(AST.Element element, ref Void data)
        {
            return new Model.Unknown(new Model.Loop(new Model.Character(new CharRanges(), new CharRanges(new CharRange(char.MinValue, char.MaxValue))), 0, -1));
        }
    }
}
