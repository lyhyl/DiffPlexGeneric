﻿using DiffPlex.Model;

namespace DiffPlex
{
    public class Differ
    {
        public static DiffResult<T> CreateDiffs<T>(IReadOnlyList<T> oldPieces, IReadOnlyList<T> newPieces) where T : notnull
        {
            return CreateDiffs(oldPieces, newPieces, null);
        }

        public static DiffResult<T> CreateDiffs<T>(IReadOnlyList<T> oldPieces, IReadOnlyList<T> newPieces, IEqualityComparer<T>? comparer) where T : notnull
        {
            var modOld = new ModificationData<T>(oldPieces);
            var modNew = new ModificationData<T>(newPieces);

            var pieceHash = new Dictionary<T, int>(comparer);
            BuildPieceHashes(modOld, pieceHash);
            BuildPieceHashes(modNew, pieceHash);

            BuildModificationData(modOld, modNew);

            int piecesALength = modOld.HashedPieces.Length;
            int piecesBLength = modNew.HashedPieces.Length;
            int posA = 0;
            int posB = 0;

            var lineDiffs = new List<DiffBlock>();

            do
            {
                while (posA < piecesALength && posB < piecesBLength
                       && !modOld.Modifications[posA] && !modNew.Modifications[posB])
                {
                    posA++;
                    posB++;
                }

                int beginA = posA;
                int beginB = posB;
                for (; posA < piecesALength && modOld.Modifications[posA]; posA++) ;

                for (; posB < piecesBLength && modNew.Modifications[posB]; posB++) ;

                int deleteCount = posA - beginA;
                int insertCount = posB - beginB;
                if (deleteCount > 0 || insertCount > 0)
                {
                    lineDiffs.Add(new DiffBlock(beginA, deleteCount, beginB, insertCount));
                }
            } while (posA < piecesALength && posB < piecesBLength);

            return new DiffResult<T>(oldPieces, newPieces, lineDiffs);
        }

        protected static EditLengthResult CalculateEditLength(int[] A, int startA, int endA, int[] B, int startB, int endB)
        {
            int N = endA - startA;
            int M = endB - startB;
            int MAX = M + N + 1;

            var forwardDiagonal = new int[MAX + 1];
            var reverseDiagonal = new int[MAX + 1];
            return CalculateEditLength(A, startA, endA, B, startB, endB, forwardDiagonal, reverseDiagonal);
        }

        private static EditLengthResult CalculateEditLength(
            int[] A, int startA, int endA,
            int[] B, int startB, int endB,
            int[] forwardDiagonal, int[] reverseDiagonal)
        {
            ArgumentNullException.ThrowIfNull(A);
            ArgumentNullException.ThrowIfNull(B);

            if (A.Length == 0 && B.Length == 0)
            {
                return new EditLengthResult();
            }

            int N = endA - startA;
            int M = endB - startB;
            int MAX = M + N + 1;
            int HALF = MAX / 2;
            int delta = N - M;
            bool deltaEven = delta % 2 == 0;
            forwardDiagonal[1 + HALF] = 0;
            reverseDiagonal[1 + HALF] = N + 1;

            //Log.WriteLine("Comparing strings");
            //Log.WriteLine("\t{0} of length {1}", A, A.Length);
            //Log.WriteLine("\t{0} of length {1}", B, B.Length);

            for (int D = 0; D <= HALF; D++)
            {
                //Log.WriteLine("\nSearching for a {0}-Path", D);
                // forward D-path
                //Log.WriteLine("\tSearching for forward path");
                Edit lastEdit;
                for (int k = -D; k <= D; k += 2)
                {
                    //Log.WriteLine("\n\t\tSearching diagonal {0}", k);
                    int kIndex = k + HALF;
                    int x, y;
                    if (k == -D || (k != D && forwardDiagonal[kIndex - 1] < forwardDiagonal[kIndex + 1]))
                    {
                        x = forwardDiagonal[kIndex + 1]; // y up    move down from previous diagonal
                        lastEdit = Edit.InsertDown;
                        //Log.Write("\t\tMoved down from diagonal {0} at ({1},{2}) to ", k + 1, x, x - (k + 1));
                    }
                    else
                    {
                        x = forwardDiagonal[kIndex - 1] + 1; // x up     move right from previous diagonal
                        lastEdit = Edit.DeleteRight;
                        //Log.Write("\t\tMoved right from diagonal {0} at ({1},{2}) to ", k - 1, x - 1, x - 1 - (k - 1));
                    }
                    y = x - k;
                    int startX = x;
                    int startY = y;
                    //Log.WriteLine("({0},{1})", x, y);
                    while (x < N && y < M && A[x + startA] == B[y + startB])
                    {
                        x += 1;
                        y += 1;
                    }
                    //Log.WriteLine("\t\tFollowed snake to ({0},{1})", x, y);

                    forwardDiagonal[kIndex] = x;

                    if (!deltaEven && k - delta >= -D + 1 && k - delta <= D - 1)
                    {
                        int revKIndex = k - delta + HALF;
                        int revX = reverseDiagonal[revKIndex];
                        int revY = revX - k;
                        if (revX <= x && revY <= y)
                        {
                            return new EditLengthResult
                            {
                                EditLength = (2 * D) - 1,
                                StartX = startX + startA,
                                StartY = startY + startB,
                                EndX = x + startA,
                                EndY = y + startB,
                                LastEdit = lastEdit
                            };
                        }
                    }
                }

                // reverse D-path
                //Log.WriteLine("\n\tSearching for a reverse path");
                for (int k = -D; k <= D; k += 2)
                {
                    //Log.WriteLine("\n\t\tSearching diagonal {0} ({1})", k, k + delta);
                    int kIndex = k + HALF;
                    int x, y;
                    if (k == -D || (k != D && reverseDiagonal[kIndex + 1] <= reverseDiagonal[kIndex - 1]))
                    {
                        x = reverseDiagonal[kIndex + 1] - 1; // move left from k+1 diagonal
                        lastEdit = Edit.DeleteLeft;
                        //Log.Write("\t\tMoved left from diagonal {0} at ({1},{2}) to ", k + 1, x + 1, x + 1 - (k + 1 + delta));
                    }
                    else
                    {
                        x = reverseDiagonal[kIndex - 1]; //move up from k-1 diagonal
                        lastEdit = Edit.InsertUp;
                        //Log.Write("\t\tMoved up from diagonal {0} at ({1},{2}) to ", k - 1, x, x - (k - 1 + delta));
                    }
                    y = x - (k + delta);

                    int endX = x;
                    int endY = y;

                    //Log.WriteLine("({0},{1})", x, y);
                    while (x > 0 && y > 0 && A[startA + x - 1] == B[startB + y - 1])
                    {
                        x -= 1;
                        y -= 1;
                    }

                    //Log.WriteLine("\t\tFollowed snake to ({0},{1})", x, y);
                    reverseDiagonal[kIndex] = x;

                    if (deltaEven && k + delta >= -D && k + delta <= D)
                    {
                        int forKIndex = k + delta + HALF;
                        int forX = forwardDiagonal[forKIndex];
                        int forY = forX - (k + delta);
                        if (forX >= x && forY >= y)
                        {
                            return new EditLengthResult
                            {
                                EditLength = 2 * D,
                                StartX = x + startA,
                                StartY = y + startB,
                                EndX = endX + startA,
                                EndY = endY + startB,
                                LastEdit = lastEdit
                            };
                        }
                    }
                }
            }

            throw new IndexOutOfRangeException(nameof(CalculateEditLength));
        }

        protected static void BuildModificationData<T>(ModificationData<T> A, ModificationData<T> B)
        {
            int N = A.HashedPieces.Length;
            int M = B.HashedPieces.Length;
            int MAX = M + N + 1;
            var forwardDiagonal = new int[MAX + 1];
            var reverseDiagonal = new int[MAX + 1];
            BuildModificationData(A, 0, N, B, 0, M, forwardDiagonal, reverseDiagonal);
        }

        private static void BuildModificationData<T>(
            ModificationData<T> A, int startA, int endA,
            ModificationData<T> B, int startB, int endB,
            int[] forwardDiagonal, int[] reverseDiagonal)
        {
            while (startA < endA && startB < endB && A.HashedPieces[startA].Equals(B.HashedPieces[startB]))
            {
                startA++;
                startB++;
            }
            while (startA < endA && startB < endB && A.HashedPieces[endA - 1].Equals(B.HashedPieces[endB - 1]))
            {
                endA--;
                endB--;
            }

            int aLength = endA - startA;
            int bLength = endB - startB;
            if (aLength > 0 && bLength > 0)
            {
                EditLengthResult res = CalculateEditLength(A.HashedPieces, startA, endA, B.HashedPieces, startB, endB, forwardDiagonal, reverseDiagonal);
                if (res.EditLength <= 0)
                    return;

                if (res.LastEdit == Edit.DeleteRight && res.StartX - 1 > startA)
                    A.Modifications[--res.StartX] = true;
                else if (res.LastEdit == Edit.InsertDown && res.StartY - 1 > startB)
                    B.Modifications[--res.StartY] = true;
                else if (res.LastEdit == Edit.DeleteLeft && res.EndX < endA)
                    A.Modifications[res.EndX++] = true;
                else if (res.LastEdit == Edit.InsertUp && res.EndY < endB)
                    B.Modifications[res.EndY++] = true;

                BuildModificationData(A, startA, res.StartX, B, startB, res.StartY, forwardDiagonal, reverseDiagonal);

                BuildModificationData(A, res.EndX, endA, B, res.EndY, endB, forwardDiagonal, reverseDiagonal);
            }
            else if (aLength > 0)
            {
                for (int i = startA; i < endA; i++)
                    A.Modifications[i] = true;
            }
            else if (bLength > 0)
            {
                for (int i = startB; i < endB; i++)
                    B.Modifications[i] = true;
            }
        }

        private static void BuildPieceHashes<T>(ModificationData<T> data, Dictionary<T, int> pieceHash) where T: notnull
        {
            for (int i = 0; i < data.Pieces.Count; i++)
            {
                var piece = data.Pieces[i];
                if (pieceHash.TryGetValue(piece, out var value))
                {
                    data.HashedPieces[i] = value;
                }
                else
                {
                    data.HashedPieces[i] = pieceHash.Count;
                    pieceHash[piece] = pieceHash.Count;
                }
            }
        }
    }
}