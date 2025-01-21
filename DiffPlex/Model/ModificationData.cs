namespace DiffPlex.Model
{
    public class ModificationData<T>
    {
        public int[] HashedPieces { get; set; }
        public bool[] Modifications { get; set; }
        public IReadOnlyList<T> Pieces { get; set; }

        public ModificationData(IReadOnlyList<T> pieces)
        {
            Pieces = pieces;
            HashedPieces = new int[pieces.Count];
            Modifications = new bool[pieces.Count];
        }
    }
}