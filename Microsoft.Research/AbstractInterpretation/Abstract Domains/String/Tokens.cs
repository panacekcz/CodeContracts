// CodeContracts
// 
// Copyright (c) Microsoft Corporation
// 
// All rights reserved. 
// 
// MIT License
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"), to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED *AS IS*, WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.

// Created by Vlastimil Dort (2016)

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Research.AbstractDomains.Strings.PrefixTree;
using Microsoft.Research.Regex;
using Microsoft.Research.Regex.AST;
using Microsoft.Research.CodeAnalysis;

namespace Microsoft.Research.AbstractDomains.Strings
{
  
  public class Tokens : IStringAbstraction<Tokens, string>
  {
    private readonly InnerNode root;

    internal Tokens(InnerNode root)
    {
      this.root = root;
    }

    public Tokens Top
    {
      get
      {
        return new Tokens(PrefixTreeBuilder.Unknown());
      }
    }

    public Tokens Bottom
    {
      get
      {
        return new Tokens(PrefixTreeBuilder.Unreached());
      }
    }

    public bool IsTop
    {
      get
      {
        if (!root.Accepting)
          return false;
        for(int c=char.MinValue; c <= char.MaxValue; ++c)
        {
          PrefixTreeNode next;
          if (!root.children.TryGetValue((char)c, out next))
          {
            return false;
          }
          else if (!(next is RepeatNode))
            return false;
        }

        return true;
      }
    }

    public bool IsBottom
    {
      get
      {
        return !root.Accepting && root.children.Count == 0;
      }
    }

    IAbstractDomain IAbstractDomain.Bottom
    {
      get
      {
        return Bottom;
      }
    }

    IAbstractDomain IAbstractDomain.Top
    {
      get
      {
        return Top;
      }
    }

    public bool ContainsValue(string s)
    {
      InnerNode current = root;
      foreach (char c in s)
      {
        PrefixTreeNode next;
        if (!current.children.TryGetValue(c, out next))
        {
          return false;
        }
        else if (next is RepeatNode)
          current = root;
        else
          current = (InnerNode)next;
      }

      return current.Accepting;
    }

    public class Operations<Variable> : IStringOperations<Tokens, Variable>
      where Variable:class,IEquatable<Variable>
    {
      public Tokens Top
      {
        get
        {
          return new Tokens(PrefixTreeBuilder.Unknown());
        }
      }

      public Tokens Substring(Tokens tokens, IndexInterval index, IndexInterval length)
      {
        //TODO: cutoff at various places
        return tokens.Top;
      }

      public Tokens Remove(Tokens tokens, IndexInterval index, IndexInterval length)
      {
        //TODO: cutoff at various places
        return tokens.Top;
      }

      public Tokens Insert(Tokens tokens, IndexInterval index, Tokens insertion)
      {
        //TODO: cutoff at index and merge
        return tokens.Top;
      }

      public Tokens Concat(WithConstants<Tokens> left, WithConstants<Tokens> right)
      {
        /*
         *    TrieNode Repeat(TrieNode inner)
    {
      RepeatVisitor rv = new RepeatVisitor();
      return rv.AcceptNode(rv);
    }

    TrieNode Concat(TrieNode left, TrieNode right)
    {
      // if any of the trees contains repeat node, append to root, change ends in the left one to repeats
      TrieNode repeatedLeft = Repeat(left);
      return repeatedLeft; //TODO: merge
    }
    */
        throw new NotImplementedException();
      }

      public Tokens Insert(WithConstants<Tokens> self, IndexInterval index, WithConstants<Tokens> other)
      {
        throw new NotImplementedException();
      }

      public Tokens Replace(Tokens self, CharInterval from, CharInterval to)
      {
        throw new NotImplementedException();
      }

      public Tokens Replace(WithConstants<Tokens> self, WithConstants<Tokens> from, WithConstants<Tokens> to)
      {
        throw new NotImplementedException();
      }

      public Tokens PadLeft(Tokens self, IndexInterval length, CharInterval fill)
      {
        //TODO:
        // compare length intervals.
        // if may pad, add repeating node with fill edge from root
        // there may be more ways to optimize, for example if there are no repeat nodes and we know we will have to pad, add nodes at root?
        return self.Top;
      }

      public Tokens PadRight(Tokens self, IndexInterval length, CharInterval fill)
      {
                LengthIntervalVisitor liv = new LengthIntervalVisitor();
                IndexInterval oldLength = liv.GetLengthInterval(self.root);

                if (oldLength.LowerBound >= length.UpperBound)
                {
                    //The string must be longer, no action
                    return self;
                }
                else if(oldLength.IsFiniteConstant && length.IsFiniteConstant)
                {
                    // We know exactly how many characters to add
                    PrefixTreeNode padding = PrefixTreeBuilder.FromCharInterval(fill, length.LowerBound.AsInt - oldLength.UpperBound.AsInt);
                    
                    //TODO: concatenate padding
                }
                else
                {
                    //TODO: change accepting to repeat
                    PrefixTreeNode padding = PrefixTreeBuilder.CharIntervalTokens(fill);
                    //TODO: join padding
                }
                
        //TODO:
        // compare lengths
        // add repeating node, or add constant string of fill chars at accepting nodes
        return self.Top;
      }

      public Tokens Trim(WithConstants<Tokens> self, WithConstants<Tokens> trimmed)
      {
        //TODO:
        return Top;
      }

      public Tokens TrimStart(WithConstants<Tokens> self, WithConstants<Tokens> trimmed)
      {
        //TODO:
        // if does not contain repeat nodes,
        // then trim the characters from the root, merge the cutted subtrees with root

        // if does contain repeat nodes, do the same, but keep the original root
        return Top;
      }

      public Tokens TrimEnd(WithConstants<Tokens> self, WithConstants<Tokens> trimmed)
      {
        //TODO:

        // trim from accepting nodes
        // if reached root, ???

        return Top;
      }

      public Tokens SetCharAt(Tokens self, IndexInterval index, CharInterval value)
      {
        //TODO:
        return Top;
      }

      public IStringPredicate IsEmpty(Tokens self, Variable selfVariable)
      {
        bool canBeEmpty = self.root.Accepting;
        bool canBeNonEmpty = self.root.children.Count > 0;

        if(canBeEmpty && canBeNonEmpty && selfVariable != null)
        {
          return StringAbstractionPredicate.ForTrue(selfVariable, new Tokens(PrefixTreeBuilder.Empty()));
        }

        return new FlatPredicate(canBeEmpty, canBeNonEmpty);
      }

      public IStringPredicate Contains(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
      {
        throw new NotImplementedException();
        // Seems there is no possibiliry to return predicate here
      }

      public IStringPredicate StartsWithOrdinal(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
      {
        // Seems there is no possiblity to return predicate here
        throw new NotImplementedException();
      }

      public IStringPredicate EndsWithOrdinal(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
      {
        // Seems there is no possiblity to return predicate here
        throw new NotImplementedException();
      }

      public IStringPredicate Equals(WithConstants<Tokens> self, Variable selfVariable, WithConstants<Tokens> other, Variable otherVariable)
      {
                //True only if both are constants
        // return self predicate
        throw new NotImplementedException();
      }

      public CompareResult CompareOrdinal(WithConstants<Tokens> self, WithConstants<Tokens> other)
      {
        throw new NotImplementedException();
      }

      public IndexInterval GetLength(Tokens self)
      {
        LengthIntervalVisitor liv = new LengthIntervalVisitor();
        return liv.GetLengthInterval(self.root);
      }

      public IndexInterval IndexOf(WithConstants<Tokens> self, WithConstants<Tokens> needle, IndexInterval offset, IndexInterval count)
      {
        throw new NotImplementedException();
      }

      public IndexInterval LastIndexOf(WithConstants<Tokens> self, WithConstants<Tokens> needle, IndexInterval offset, IndexInterval count)
      {
        throw new NotImplementedException();
      }

      public CharInterval GetCharAt(Tokens self, IndexInterval index)
      {
        throw new NotImplementedException();
      }

      public IStringPredicate RegexIsMatch(Tokens self, Variable selfVariable, Element regex)
      {
                //regex underapprox less equal self -> return true
                //self meet regex overapprox is bottom -> return false
                // else return overapprox predicate.
                Tokens regexUnder = TokensRegex.FromRegex(regex, true);
                if (self.LessThanEqual(regexUnder))
                    return FlatPredicate.True;


                Tokens regexOver = TokensRegex.FromRegex(regex, false);
                if (self.Meet(regexOver).IsBottom)
                    return FlatPredicate.False;


                return StringAbstractionPredicate.ForTrue(selfVariable, regexOver);
        //throw new NotImplementedException();
      }

      public Tokens Constant(string constant)
      {
        return new Tokens(PrefixTreeBuilder.FromString(constant));
      }
    }





 

    public bool Equals(Tokens other)
    {
      return root.Equals(other.root);
    }

    public bool LessThanEqual(Tokens other)
    {
      return PrefixTreeUtils.LessEqual(root, other.root);
    }

    public Tokens Join(Tokens other)
    {
      PrefixTreeJoiner joiner = new PrefixTreeJoiner();
      joiner.Add(root);
      joiner.Add(other.root);

      return new Tokens(joiner.Result());
    }

    public Tokens Meet(Tokens other)
    {
      throw new NotImplementedException();
    }

    public Tokens Constant(string cst)
    {
      return new Tokens(PrefixTreeBuilder.FromString(cst));
    }

    public bool LessEqual(IAbstractDomain a)
    {
      return LessThanEqual(a as Tokens);
    }

    public IAbstractDomain Join(IAbstractDomain a)
    {
      return Join(a as Tokens);
    }

    public IAbstractDomain Meet(IAbstractDomain a)
    {
      return Meet(a as Tokens);
    }

    public IAbstractDomain Widening(IAbstractDomain prev)
    {
            Tokens join = Join(prev as Tokens);
            RepeatVisitor rv = new RepeatVisitor();
            return new Tokens((InnerNode)rv.Repeat(join.root));
      
    }

    public T To<T>(IFactory<T> factory)
    {
      return factory.Constant(true);
    }

    public object Clone()
    {
      return new Tokens(root);
    }
  }

  
}
