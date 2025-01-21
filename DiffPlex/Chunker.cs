using System.Diagnostics;

namespace DiffPlex
{
    public class Chunker
    {
        public static IReadOnlyList<Memory<T>> ChunkBlock<T>(T[] data, int blockSize)
        {
            Debug.Assert(data.Length % blockSize == 0);

            var d = new Memory<T>(data);
            var s = new Memory<T>[data.Length];
            for (int i = 0; i < data.Length; i += blockSize)
                s[i] = d.Slice(i, blockSize);
            return s;
        }
    }
}