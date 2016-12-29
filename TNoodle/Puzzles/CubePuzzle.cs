using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Solvers;
using TNoodle.Utils;

namespace TNoodle.Puzzles
{
    public class CubePuzzle : Puzzle
    {
        private static readonly int[] DEFAULT_LENGTHS = { 0, 0, 25, 25, 40, 60, 80, 100, 120, 140, 160, 180 };

        protected int Size { get; }

        public CubePuzzle(int size)
        {
            Size = size;
        }

        protected CubeMove[][] GetRandomOrientationMoves(int thickness)
        {
            CubeMove[] randomUFaceMoves = new CubeMove[]
            {
                null,
                new CubeMove(CubeFace.R, 1, thickness, this),
                new CubeMove(CubeFace.R, 2, thickness, this),
                new CubeMove(CubeFace.R, 3, thickness, this),
                new CubeMove(CubeFace.F, 1, thickness, this),
                new CubeMove(CubeFace.F, 3, thickness, this)
            };
            CubeMove[] randomFFaceMoves = new CubeMove[]
            {
                null,
                new CubeMove(CubeFace.U, 1, thickness, this),
                new CubeMove(CubeFace.U, 2, thickness, this),
                new CubeMove(CubeFace.U, 3, thickness, this)
            };
            CubeMove[][] randomOrientationMoves = new CubeMove[randomUFaceMoves.Length * randomFFaceMoves.Length][];
            int i = 0;
            foreach (CubeMove randomUFaceMove in randomUFaceMoves)
            {
                foreach (CubeMove randomFFaceMove in randomFFaceMoves)
                {
                    List<CubeMove> moves = new List<CubeMove>();
                    if (randomUFaceMove != null)
                    {
                        moves.Add(randomUFaceMove);
                    }
                    if (randomFFaceMove != null)
                    {
                        moves.Add(randomFFaceMove);
                    }
                    randomOrientationMoves[i++] = (CubeMove[])moves.ToArray().Clone();
                }
            }
            return randomOrientationMoves;
        }

        public override string GetLongName()
        {
            return Size + "x" + Size + "x" + Size;
        }

        public override string GetShortName()
        {
            return Size + "" + Size + "" + Size;
        }

        private static void Swap(int[,,] image,
        int f1, int x1, int y1,
        int f2, int x2, int y2,
        int f3, int x3, int y3,
        int f4, int x4, int y4,
        int dir)
        {
            if (dir == 1)
            {
                int temp = image[f1, x1, y1];
                image[f1, x1, y1] = image[f2, x2, y2];
                image[f2, x2, y2] = image[f3, x3, y3];
                image[f3, x3, y3] = image[f4, x4, y4];
                image[f4, x4, y4] = temp;
            }
            else if (dir == 2)
            {
                int temp = image[f1, x1, y1];
                image[f1, x1, y1] = image[f3, x3, y3];
                image[f3, x3, y3] = temp;
                temp = image[f2, x2, y2];
                image[f2, x2, y2] = image[f4, x4, y4];
                image[f4, x4, y4] = temp;
            }
            else if (dir == 3)
            {
                int temp = image[f4, x4, y4];
                image[f4, x4, y4] = image[f3, x3, y3];
                image[f3, x3, y3] = image[f2, x2, y2];
                image[f2, x2, y2] = image[f1, x1, y1];
                image[f1, x1, y1] = temp;
            }
        }

        private static void Slice(CubeFace face, int slice, int dir, int[,,] image)
        {
            int size = image.GetLength(1);

            CubeFace sface = face;
            int sslice = slice;
            int sdir = dir;

            if (face != CubeFace.L && face != CubeFace.D && face != CubeFace.B)
            {
                sface = face.oppositeFace();
                sslice = size - 1 - slice;
                sdir = 4 - dir;
            }
            for (int j = 0; j < size; j++)
            {
                if (sface == CubeFace.L)
                {
                    Swap(image,
                            (int)CubeFace.U, j, sslice,
                            (int)CubeFace.B, size - 1 - j, size - 1 - sslice,
                            (int)CubeFace.D, j, sslice,
                            (int)CubeFace.F, j, sslice,
                            sdir);
                }
                else if (sface == CubeFace.D)
                {
                    Swap(image,
                            (int)CubeFace.L, size - 1 - sslice, j,
                            (int)CubeFace.B, size - 1 - sslice, j,
                            (int)CubeFace.R, size - 1 - sslice, j,
                            (int)CubeFace.F, size - 1 - sslice, j,
                            sdir);
                }
                else if (sface == CubeFace.B)
                {
                    Swap(image,
                            (int)CubeFace.U, sslice, j,
                            (int)CubeFace.R, j, size - 1 - sslice,
                            (int)CubeFace.D, size - 1 - sslice, size - 1 - j,
                            (int)CubeFace.L, size - 1 - j, sslice,
                            sdir);
                }
            }
            if (slice == 0 || slice == size - 1)
            {
                int f;
                if (slice == 0)
                {
                    f = (int)face;
                    sdir = 4 - dir;
                }
                else if (slice == size - 1)
                {
                    f = (int)face.oppositeFace();
                    sdir = dir;
                }
                else
                {
                    return;
                }
                for (int j = 0; j < (size + 1) / 2; j++)
                {
                    for (int k = 0; k < size / 2; k++)
                    {
                        Swap(image,
                                f, j, k,
                                f, k, size - 1 - j,
                                f, size - 1 - j, size - 1 - k,
                                f, size - 1 - k, j,
                                sdir);
                    }
                }
            }
        }

        public override PuzzleState GetSolvedState()
        {
            return new CubeState(this);
        }

        protected override int GetRandomMoveCount()
        {
            return DEFAULT_LENGTHS[Size];
        }

        private void SpinCube(int[,,] image, CubeFace face, int dir)
        {
            for (int slice = 0; slice < Size; slice++)
            {
                Slice(face, slice, dir, image);
            }
        }

        private int[,,] Normalize(int[,,] image)
        {
            image = (int[,,])image.Clone();

            int spins = 0;
            while (!IsNormalized(image))
            {
                int[,] stickersByPiece = GetStickersByPiece(image);

                int goal = 0;
                goal |= 1 << (int)CubeFace.B;
                goal |= 1 << (int)CubeFace.L;
                goal |= 1 << (int)CubeFace.D;
                int idx = -1;
                for (int i = 0; i < stickersByPiece.GetLength(0); i++)
                {
                    int t = 0;
                    for (int j = 0; j < stickersByPiece.GetLength(1); j++)
                    {
                        t |= 1 << stickersByPiece[i, j];
                    }
                    if (t == goal)
                    {
                        idx = i;
                        break;
                    }
                }
                CubeFace? f = null;
                int dir = 1;
                if (stickersByPiece[idx, 0] == (int)CubeFace.D)
                {
                    if (idx < 4)
                    {
                        // on U
                        f = CubeFace.F;
                        dir = 2;
                    }
                    else
                    {
                        // on D
                        f = CubeFace.U;
                        switch (idx)
                        {
                            case 4:
                                dir = 2; break;
                            case 5:
                                dir = 1; break;
                            case 6:
                                dir = 3; break;
                        }
                    }
                }
                else if (stickersByPiece[idx, 1] == (int)CubeFace.D)
                {
                    switch (idx)
                    {
                        case 0:
                        case 6:
                            f = CubeFace.F; break; // on R
                        case 1:
                        case 4:
                            f = CubeFace.L; break; // on F
                        case 2:
                        case 7:
                            f = CubeFace.R; break; // on B
                        case 3:
                        case 5:
                            f = CubeFace.B; break; // on L
                    }
                }
                else
                {
                    switch (idx)
                    {
                        case 2:
                        case 4:
                            f = CubeFace.F; break; // on R
                        case 0:
                        case 5:
                            f = CubeFace.L; break; // on F
                        case 3:
                        case 6:
                            f = CubeFace.R; break; // on B
                        case 1:
                        case 7:
                            f = CubeFace.B; break; // on L
                    }
                }
                SpinCube(image, (CubeFace)f, dir);
                spins++;
            }

            return image;
        }

        private bool IsNormalized(int[,,] image)
        {
            // A CubeState is normalized if the BLD piece is solved
            return image[(int)CubeFace.B, Size - 1, Size - 1] == (int)CubeFace.B &&
                   image[(int)CubeFace.L, Size - 1, 0] == (int)CubeFace.L &&
                   image[(int)CubeFace.D, Size - 1, 0] == (int)CubeFace.D;
        }

        private static int[,] GetStickersByPiece(int[,,] img)
        {
            int s = img.GetLength(1) - 1;
            return new int[,]
            {
                { img[(int)CubeFace.U,s,s], img[(int)CubeFace.R,0,0], img[(int)CubeFace.F,0,s] },
                { img[(int)CubeFace.U,s,0], img[(int)CubeFace.F,0,0], img[(int)CubeFace.L,0,s] },
                { img[(int)CubeFace.U,0,s], img[(int)CubeFace.B,0,0], img[(int)CubeFace.R,0,s] },
                { img[(int)CubeFace.U,0,0], img[(int)CubeFace.L,0,0], img[(int)CubeFace.B,0,s] },

                { img[(int)CubeFace.D,0,s], img[(int)CubeFace.F,s,s], img[(int)CubeFace.R,s,0] },
                { img[(int)CubeFace.D,0,0], img[(int)CubeFace.L,s,s], img[(int)CubeFace.F,s,0] },
                { img[(int)CubeFace.D,s,s], img[(int)CubeFace.R,s,s], img[(int)CubeFace.B,s,0] },
                { img[(int)CubeFace.D,s,0], img[(int)CubeFace.B,s,s], img[(int)CubeFace.L,s,0] }
            };
        }

        #region CubeState

        public class CubeState : PuzzleState
        {
            private CubePuzzle puzzle;
            private readonly int[,,] image;
            private CubeState normalizedState = null;

            public CubeState(CubePuzzle cp) : base(cp)
            {
                puzzle = cp;
                image = new int[6, puzzle.Size, puzzle.Size];

                for (int face = 0; face < image.GetLength(0); face++)
                {
                    for (int j = 0; j < image.GetLength(1); j++)
                    {
                        for (int k = 0; k < image.GetLength(2); k++)
                        {
                            image[face, j, k] = face;
                        }
                    }
                }
                normalizedState = this;
            }

            public CubeState(int[,,] image, CubePuzzle cp) : base(cp)
            {
                puzzle = cp;
                this.image = image;
            }

            public override bool IsNormalized()
            {
                return puzzle.IsNormalized(image);
            }

            public override PuzzleState GetNormalized()
            {
                if (normalizedState == null)
                {
                    int[,,] normalizedImage = puzzle.Normalize(image);
                    normalizedState = new CubeState(normalizedImage, puzzle);
                }
                return normalizedState;
            }

            public TwoByTwoState ToTwoByTwoState()
            {
                TwoByTwoState state = new TwoByTwoState();

                int[,] stickersByPiece = GetStickersByPiece(image);

                // Here's a clever color value assigning system that gives each piece
                // a unique id just by summing up the values of its stickers.
                //
                //            +----------+
                //            |*3*    *2*|
                //            |   U (0)  |
                //            |*1*    *0*|
                // +----------+----------+----------+----------+
                // | 3      1 | 1      0 | 0      2 | 2      3 |
                // |   L (1)  |   F (0)  |   R (0)  |   B (2)  |
                // | 7      5 | 5      4 | 4      6 | 6      7 |
                // +----------+----------+----------+----------+
                //            |*5*    *4*|
                //            |   D (4)  |
                //            |*7*    *6*|
                //            +----------+
                //

                int dColor = stickersByPiece[7, 0];
                int bColor = stickersByPiece[7, 1];
                int lColor = stickersByPiece[7, 2];

                int uColor = (int)((CubeFace)dColor).oppositeFace();
                int fColor = (int)((CubeFace)bColor).oppositeFace();
                int rColor = (int)((CubeFace)lColor).oppositeFace();

                int[] colorToVal = new int[8];
                colorToVal[uColor] = 0;
                colorToVal[fColor] = 0;
                colorToVal[rColor] = 0;
                colorToVal[lColor] = 1;
                colorToVal[bColor] = 2;
                colorToVal[dColor] = 4;

                int[] pieces = new int[7];
                for (int i = 0; i < pieces.Length; i++)
                {
                    int pieceVal = colorToVal[stickersByPiece[i, 0]] + colorToVal[stickersByPiece[i, 1]] + colorToVal[stickersByPiece[i, 2]];

                    int clockwiseTurnsToGetToPrimaryColor = 0;
                    while (stickersByPiece[i, clockwiseTurnsToGetToPrimaryColor] != uColor && stickersByPiece[i, clockwiseTurnsToGetToPrimaryColor] != dColor)
                    {
                        clockwiseTurnsToGetToPrimaryColor++;
                    }
                    int piece = (clockwiseTurnsToGetToPrimaryColor << 3) + pieceVal;
                    pieces[i] = piece;
                }

                state.Permutation = TwoByTwoSolver.PackPerm(pieces);
                state.Orientation = TwoByTwoSolver.PackOrient(pieces);
                return state;
            }

            public string ToFaceCube()
            {
                string state = "";
                CubeFace[] faces = { CubeFace.U, CubeFace.R, CubeFace.F, CubeFace.D, CubeFace.L, CubeFace.B };
                foreach (CubeFace f in faces)
                {
                    int idx = (int)f;
                    for (int i = 0; i < image.GetLength(1); i++)
                    {
                        for (int j = 0; j < image.GetLength(2); j++)
                        {
                            state += ((CubeFace)image[idx, i, j]).ToString();
                        }
                    }
                }
                return state;
            }

            public override LinkedHashMap<string, PuzzleState> GetSuccessorsByName()
            {
                return GetSuccessorsWithinSlice(puzzle.Size - 1, true);
            }

            public override LinkedHashMap<string, PuzzleState> GetScrambleSuccessors()
            {
                return GetSuccessorsWithinSlice(puzzle.Size / 2 - 1, false);
            }

            public override LinkedHashMap<PuzzleState, string> GetCanonicalMovesByState()
            {
                return GetScrambleSuccessors().ReverseHashMap();
            }

            private LinkedHashMap<string, PuzzleState> GetSuccessorsWithinSlice(int maxSlice, bool includeRedundant)
            {
                LinkedHashMap<string, PuzzleState> successors = new LinkedHashMap<string, PuzzleState>();
                for (int innerSlice = 0; innerSlice <= maxSlice; innerSlice++)
                {
                    foreach (CubeFace face in Enum.GetValues(typeof(CubeFace)))
                    {
                        bool halfOfEvenCube = puzzle.Size % 2 == 0 && (innerSlice == (puzzle.Size / 2) - 1);
                        if (!includeRedundant && (int)face >= 3 && halfOfEvenCube)
                        {
                            // Skip turning the other halves of even sized cubes
                            continue;
                        }
                        int outerSlice = 0;
                        for (int dir = 1; dir <= 3; dir++)
                        {
                            CubeMove move = new CubeMove(face, dir, innerSlice, outerSlice, puzzle);
                            string moveStr = move.ToString();
                            if (moveStr == null)
                            {
                                // Skip unnamed rotations.
                                continue;
                            }

                            int[,,] imageCopy = (int[,,])image.Clone();
                            for (int slice = outerSlice; slice <= innerSlice; slice++)
                            {
                                Slice(face, slice, dir, imageCopy);
                            }
                            successors[moveStr] = new CubeState(imageCopy, puzzle);
                        }
                    }
                }

                return successors;
            }

            public override bool Equals(object other)
            {
                return image.DeepEquals(((CubeState)other).image);
            }

            public override int GetHashCode()
            {
                return image.DeepHashCode();
            }
        }

        #endregion

        #region CubeMove

        public class CubeMove
        {
            private CubePuzzle puzzle;
            private static readonly string[] DIR_TO_STR = new string[] { null, "", "2", "'" };
            private static Dictionary<CubeFace, string> faceRotationsByName = new Dictionary<CubeFace, string>
            {
                [CubeFace.R] = "x",
                [CubeFace.U] = "y",
                [CubeFace.F] = "z"
            };

            public CubeFace Face { get; }
            public int Direction { get; }
            public int InnerSlice { get; }
            public int OuterSlice { get; }

            public CubeMove(CubeFace face, int dir, CubePuzzle p) : this(face, dir, 0, p)
            {
            }
            public CubeMove(CubeFace face, int dir, int innerSlice, CubePuzzle p) : this(face, dir, innerSlice, 0, p)
            {
            }
            public CubeMove(CubeFace face, int dir, int innerSlice, int outerSlice, CubePuzzle p)
            {
                puzzle = p;
                Face = face;
                Direction = dir;
                InnerSlice = innerSlice;
                OuterSlice = outerSlice;
                // We haven't come up with names for moves where outerSlice != 0
            }

            public override string ToString()
            {
                string f = Face.ToString();
                string move;
                if (InnerSlice == 0)
                {
                    move = f;
                }
                else if (InnerSlice == 1)
                {
                    move = f + "w";
                }
                else if (InnerSlice == puzzle.Size - 1)
                {
                    // Turning all the slices is a rotation
                    if (!faceRotationsByName.ContainsKey(Face)) return null;
                    move = faceRotationsByName[Face];
                }
                else
                {
                    move = (InnerSlice + 1) + f + "w";
                }
                move += DIR_TO_STR[Direction];

                return move;
            }
        }

        #endregion
    }
}

