using System.Diagnostics.CodeAnalysis;

namespace DiffPlex
{
    public class MemoryComparer<T> : IEqualityComparer<Memory<T>>
    {
        public static MemoryComparer<T> Default { get; } = new MemoryComparer<T>();

        public bool Equals(Memory<T> x, Memory<T> y)
        {
            if (x.Equals(y))
            {
                return true;
            }

            if (x.Length != y.Length)
            {
                return false;
            }

            var sx = x.Span;
            var sy = y.Span;
            for (int i = 0; i < sx.Length; i++)
            {
                if (!Equals(sx[i], sy[i]))
                {
                    return false;
                }
            }

            return true;
        }

        public int GetHashCode([DisallowNull] Memory<T> obj)
        {
            int ret = 0;
            var span = obj.Span;
            for (int i = (obj.Length >= 8 ? obj.Length - 8 : 0); i < obj.Length; i++)
            {
                ret = HashCode.Combine(ret, span[i]?.GetHashCode());
            }
            return ret;
        }
    }
}