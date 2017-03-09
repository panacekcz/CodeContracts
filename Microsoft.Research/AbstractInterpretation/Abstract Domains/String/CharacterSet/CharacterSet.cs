using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Research.AbstractDomains.Strings
{
    public interface ICharacterSetFactory<CharacterSet>
          where CharacterSet : ICharacterSet<CharacterSet>
    {
        CharacterSet Create(bool value, int size);
        CharacterSet CreateSingleton(char value, ICharacterClassification classif);
        CharacterSet CreateIntervals(IEnumerable<CharInterval> intervals, ICharacterClassification classif);
    }


    /// <summary>
    /// Represents a set of character classes, providing non-mutating and mutating operations.
    /// </summary>
    /// <typeparam name="CharacterSet">Type of the implementing class.</typeparam>
    public interface ICharacterSet<CharacterSet> : IEquatable<CharacterSet>
        where CharacterSet : ICharacterSet<CharacterSet>
    {
        /// <summary>
        /// Checks wheter the set contains no characters.
        /// </summary>
        bool IsEmpty { get; }
        /// <summary>
        /// Checks wheter the set contains all <paramref name="size"/> classes.
        /// </summary>
        bool IsFull(int size);
        /// <summary>
        /// Gets the number of characters in the set.
        /// </summary>
        int Count { get; }
        /// <summary>
        /// Checks wheter the set contains exactly 1 character class.
        /// </summary>
        bool IsSingleton { get; }
        /// <summary>
        /// Checks whether the set contains a specified character class.
        /// </summary>
        /// <param name="characterClass">Index of the character class.</param>
        /// <returns>True if the set contains <paramref name="characterClass"/>.</returns>
        bool Contains(int characterClass);
        /// <summary>
        /// Checks whether the set contains a class that is also contained in another set.
        /// </summary>
        /// <param name="set">Another set of the same number of classes.</param>
        /// <returns>True if the set contains a class that is also contained in <paramref name="set"/>.</returns>
        bool Intersects(CharacterSet set);
        bool IsSubset(CharacterSet set);

        CharacterSet Union(CharacterSet set);
        CharacterSet Intersection(CharacterSet set);
        CharacterSet Except(CharacterSet set);

        CharacterSet With(int value);
        CharacterSet Without(int value);

        CharacterSet Empty();
        CharacterSet Create(bool full, int size);
        CharacterSet MutableClone();
        CharacterSet Inverted(int size);

        void Add(int value);
        void Remove(int value);
        void IntersectWith(CharacterSet set);
        void UnionWith(CharacterSet set);
        void Invert(int size);
    }

}
