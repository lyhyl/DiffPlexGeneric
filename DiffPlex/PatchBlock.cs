namespace DiffPlex
{
    public record PatchBlock<T>
    {
        public int DeleteStart;
        public int DeleteCount;
        public required IReadOnlyList<T> Insert;

        public int DeleteEnd => DeleteStart + DeleteCount;
        public bool IsReplacement => Insert.Count == DeleteCount;
    }
}
