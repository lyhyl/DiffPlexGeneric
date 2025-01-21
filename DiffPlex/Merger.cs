namespace DiffPlex
{
    public class Merger
    {
        public static bool IsOverlap(int l1, int r1, int l2, int r2)
        {
            return !(r1 <= l2 || r2 <= l1) || (l1 == r1 && l2 == r2 && l1 == l2);
        }

        public static Patch<T> Merge<T>(Patch<T> patchA, Patch<T> patchB)
        {
            var mergedBlocks = new List<PatchBlock<T>>();
            int i = 0, j = 0;

            while (i < patchA.Blocks.Count && j < patchB.Blocks.Count)
            {
                var blockA = patchA.Blocks[i];
                var blockB = patchB.Blocks[j];

                if (IsOverlap(blockA.DeleteStart, blockA.DeleteEnd, blockB.DeleteStart, blockB.DeleteEnd))
                {
                    Console.WriteLine("conflict");

                    if (blockA.IsReplacement && blockB.IsReplacement)
                    {
                        Console.WriteLine("conflict replace auto resolve");

                        var l = Math.Min(blockA.DeleteStart, blockB.DeleteStart);
                        var r = Math.Max(blockA.DeleteEnd, blockB.DeleteEnd);
                        var ins = Enumerable.Repeat(default(T), r - l).ToList();
                        for (int k = 0; k < blockA.Insert.Count; k++)
                        {
                            ins[blockA.DeleteStart - l + k] = blockA.Insert[k];
                        }
                        for (int k = 0; k < blockB.Insert.Count; k++)
                        {
                            ins[blockB.DeleteStart - l + k] = blockB.Insert[k];
                        }
                        var mergedBlock = new PatchBlock<T>
                        {
                            DeleteStart = l,
                            DeleteCount = ins.Count,
                            Insert = ins!
                        };
                        mergedBlocks.Add(mergedBlock);
                    }
                    else
                    {
                        Console.WriteLine("conflict non-replace use B only");

                        mergedBlocks.Add(blockB);
                    }
                    i++;
                    j++;
                }
                else
                {
                    if (blockA.DeleteStart < blockB.DeleteStart)
                    {
                        mergedBlocks.Add(blockA);
                        i++;
                    }
                    else
                    {
                        mergedBlocks.Add(blockB);
                        j++;
                    }
                }
            }

            // Add remaining blocks from patchA
            while (i < patchA.Blocks.Count)
            {
                mergedBlocks.Add(patchA.Blocks[i]);
                i++;
            }

            // Add remaining blocks from patchB
            while (j < patchB.Blocks.Count)
            {
                mergedBlocks.Add(patchB.Blocks[j]);
                j++;
            }

            return new Patch<T>(mergedBlocks);
        }
    }
}
