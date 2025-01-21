using DiffPlex.Model;

namespace DiffPlex
{
    public class Patch<T>
    {
        public List<PatchBlock<T>> Blocks { get; }

        public Patch(DiffResult<T> diff)
        {
            Blocks = diff.DiffBlocks.Select(b => new PatchBlock<T>()
            {
                DeleteStart = b.DeleteStartA,
                DeleteCount = b.DeleteCountA,
                Insert = diff.NewPieces.Skip(b.InsertStartB).Take(b.InsertCountB).ToList()
            }).ToList();
        }

        public Patch(List<PatchBlock<T>> diff)
        {
            Blocks = diff;
        }

        public List<T> Apply(IReadOnlyList<T> data)
        {
            var patched = new List<T>();
            int head = 0;
            foreach (var block in Blocks)
            {
                patched.AddRange(data.Skip(head).Take(block.DeleteStart - head));
                patched.AddRange(block.Insert);
                head = block.DeleteStart + block.DeleteCount;
            }
            if (head < data.Count)
            {
                patched.AddRange(data.Skip(head));
            }
            return patched;
        }
    }
}
