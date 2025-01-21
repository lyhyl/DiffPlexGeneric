namespace DiffPlex.Model
{
    public class DiffResult<T>
    {
        public IReadOnlyList<T> OldPieces { get; }
        public IReadOnlyList<T> NewPieces { get; }
        public IList<DiffBlock> DiffBlocks { get; }

        public DiffResult(IReadOnlyList<T> oldPieces, IReadOnlyList<T> newPieces, IList<DiffBlock> blocks)
        {
            OldPieces = oldPieces;
            NewPieces = newPieces;
            DiffBlocks = blocks;
        }
    }
}