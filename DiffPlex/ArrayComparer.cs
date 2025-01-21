using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace DiffPlex
{
    public class ArrayComparer<T> : IEqualityComparer<T[]>
    {
        public static ArrayComparer<T> Default { get; } = new ArrayComparer<T>();
        public bool Equals(T[]? x, T[]? y)
        {
            return StructuralComparisons.StructuralEqualityComparer.Equals(x, y);
        }
        public int GetHashCode([DisallowNull] T[] obj)
        {
            return StructuralComparisons.StructuralEqualityComparer.GetHashCode(obj);
        }
    }
}