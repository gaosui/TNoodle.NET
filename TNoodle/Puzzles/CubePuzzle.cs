using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Core;
using TNoodle.Solvers;

namespace TNoodle.Puzzles
{
    public static class ExtensionMethods
    {
        public static CubePuzzle.Face oppositeFace(this CubePuzzle.Face f)
        {
            return (CubePuzzle.Face)(((int)f + 3) % 6);
        }
    }

    public class CubePuzzle : Puzzle
    {
        public enum Face
        {
            R, U, F, L, D, B
        }


        private static readonly string[] DIR_TO_STR = new string[] { null, "", "2", "'" };
        private static Dictionary<Face, string> faceRotationsByName = new Dictionary<Face, string>();
        static CubePuzzle()
        {
            faceRotationsByName.Add(Face.R, "x");
            faceRotationsByName.Add(Face.U, "y");
            faceRotationsByName.Add(Face.F, "z");
        }

        public class CubeMove
        {
            private CubePuzzle cubePuzzle;
            internal Face face;
            internal int dir;
            internal int innerSlice, outerSlice;
            public CubeMove(Face face, int dir, CubePuzzle cp) : this(face, dir, 0, cp)
            {
            }
            public CubeMove(Face face, int dir, int innerSlice, CubePuzzle cp) : this(face, dir, innerSlice, 0, cp)
            {
            }
            public CubeMove(Face face, int dir, int innerSlice, int outerSlice, CubePuzzle cp)
            {
                cubePuzzle = cp;
                this.face = face;
                this.dir = dir;
                this.innerSlice = innerSlice;
                this.outerSlice = outerSlice;
                // We haven't come up with names for moves where outerSlice != 0
                //azzert(outerSlice == 0);
            }

            public override string ToString()
            {
                string f = face.ToString();
                string move;
                if (innerSlice == 0)
                {
                    move = f;
                }
                else if (innerSlice == 1)
                {
                    move = f + "w";
                }
                else if (innerSlice == cubePuzzle.size - 1)
                {
                    // Turning all the slices is a rotation
                    string rotationName = faceRotationsByName[face];
                    if (rotationName == null)
                    {
                        // Not all rotations are actually named.
                        return null;
                    }
                    move = rotationName;
                }
                else
                {
                    move = (innerSlice + 1) + f + "w";
                }
                move += DIR_TO_STR[dir];

                return move;
            }
        }

        private static readonly int gap = 2;
        private static readonly int cubieSize = 10;
        private static readonly int[] DEFAULT_LENGTHS = { 0, 0, 25, 25, 40, 60, 80, 100, 120, 140, 160, 180 };

        protected internal readonly int size;

        protected internal virtual CubeMove[][] getRandomOrientationMoves(int thickness)
        {
            CubeMove[] randomUFaceMoves = new CubeMove[]
            {
                null,
                new CubeMove(Face.R, 1, thickness, this),
                new CubeMove(Face.R, 2, thickness, this),
                new CubeMove(Face.R, 3, thickness, this),
                new CubeMove(Face.F, 1, thickness, this),
                new CubeMove(Face.F, 3, thickness, this)
            };
            CubeMove[] randomFFaceMoves = new CubeMove[]
            {
                null,
                new CubeMove(Face.U, 1, thickness, this),
                new CubeMove(Face.U, 2, thickness, this),
                new CubeMove(Face.U, 3, thickness, this)
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
                    CubeMove[] movesArr = new CubeMove[moves.Count];
                    moves.CopyTo(movesArr);
                    randomOrientationMoves[i++] = movesArr;
                }
            }
            return randomOrientationMoves;
        }

        public CubePuzzle(int size)
        {
            //azzert(size >= 0 && size < DEFAULT_LENGTHS.length, "Invalid cube size");
            this.size = size;
        }

        //@Override
        public override string getLongName()
        {
            return size + "x" + size + "x" + size;
        }

        //@Override
        public override string getShortName()
        {
            return size + "" + size + "" + size;
        }

        private static void swap(int[][][] image,
        int f1, int x1, int y1,
        int f2, int x2, int y2,
        int f3, int x3, int y3,
        int f4, int x4, int y4,
        int dir)
        {
            if (dir == 1)
            {
                int temp = image[f1][x1][y1];
                image[f1][x1][y1] = image[f2][x2][y2];
                image[f2][x2][y2] = image[f3][x3][y3];
                image[f3][x3][y3] = image[f4][x4][y4];
                image[f4][x4][y4] = temp;
            }
            else if (dir == 2)
            {
                int temp = image[f1][x1][y1];
                image[f1][x1][y1] = image[f3][x3][y3];
                image[f3][x3][y3] = temp;
                temp = image[f2][x2][y2];
                image[f2][x2][y2] = image[f4][x4][y4];
                image[f4][x4][y4] = temp;
            }
            else if (dir == 3)
            {
                int temp = image[f4][x4][y4];
                image[f4][x4][y4] = image[f3][x3][y3];
                image[f3][x3][y3] = image[f2][x2][y2];
                image[f2][x2][y2] = image[f1][x1][y1];
                image[f1][x1][y1] = temp;
            }
            else
            {
                //azzert(false);
            }
        }

        private static void slice(Face face, int slice, int dir, int[][][] image)
        {
            int size = image[0].Length;

            //azzert(slice >= 0 && slice < size);

            Face sface = face;
            int sslice = slice;
            int sdir = dir;

            if (face != Face.L && face != Face.D && face != Face.B)
            {
                sface = face.oppositeFace();
                sslice = size - 1 - slice;
                sdir = 4 - dir;
            }
            for (int j = 0; j < size; j++)
            {
                if (sface == Face.L)
                {
                    swap(image,
                            (int)Face.U, j, sslice,
                            (int)Face.B, size - 1 - j, size - 1 - sslice,
                            (int)Face.D, j, sslice,
                            (int)Face.F, j, sslice,
                            sdir);
                }
                else if (sface == Face.D)
                {
                    swap(image,
                            (int)Face.L, size - 1 - sslice, j,
                            (int)Face.B, size - 1 - sslice, j,
                            (int)Face.R, size - 1 - sslice, j,
                            (int)Face.F, size - 1 - sslice, j,
                            sdir);
                }
                else if (sface == Face.B)
                {
                    swap(image,
                            (int)Face.U, sslice, j,
                            (int)Face.R, j, size - 1 - sslice,
                            (int)Face.D, size - 1 - sslice, size - 1 - j,
                            (int)Face.L, size - 1 - j, sslice,
                            sdir);
                }
                else
                {
                    //azzert(false);
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
                    //azzert(false);
                    return;
                }
                for (int j = 0; j < (size + 1) / 2; j++)
                {
                    for (int k = 0; k < size / 2; k++)
                    {
                        swap(image,
                                f, j, k,
                                f, k, size - 1 - j,
                                f, size - 1 - j, size - 1 - k,
                                f, size - 1 - k, j,
                                sdir);
                    }
                }
            }
        }

        public override PuzzleState getSolvedState()
        {
            return new CubeState(this);
        }

        protected internal override int getRandomMoveCount()
        {
            return DEFAULT_LENGTHS[size];
        }

        private int[][][] cloneImage(int[][][] image)
        {
            int[][][] imageCopy = new int[image.Length][][];
            for (int i = 0; i < image.Length; i++)
            {
                image[i] = new int[image[i].Length][];
                for (int j = 0; j < image[i].Length; j++)
                {
                    image[i][j] = new int[image[i][j].Length];
                }
            }

            GwtSafeUtils.deepCopy(image, imageCopy);
            return imageCopy;
        }

        private void spinCube(int[][][] image, Face face, int dir)
        {
            for (int slice = 0; slice < size; slice++)
            {
                CubePuzzle.slice(face, slice, dir, image);
            }
        }

        private int[][][] normalize(int[][][] image)
        {
            image = cloneImage(image);

            int spins = 0;
            while (!isNormalized(image))
            {
                //azzert(spins < 2);
                int[][] stickersByPiece = getStickersByPiece(image);

                int goal = 0;
                goal |= 1 << (int)Face.B;
                goal |= 1 << (int)Face.L;
                goal |= 1 << (int)Face.D;
                int idx = -1;
                for (int i = 0; i < stickersByPiece.Length; i++)
                {
                    int t = 0;
                    for (int j = 0; j < stickersByPiece[i].Length; j++)
                    {
                        t |= 1 << stickersByPiece[i][j];
                    }
                    if (t == goal)
                    {
                        idx = i;
                        break;
                    }
                }
                //azzert(idx >= 0);
                Face? f = null;
                int dir = 1;
                if (stickersByPiece[idx][0] == (int)Face.D)
                {
                    if (idx < 4)
                    {
                        // on U
                        f = Face.F;
                        dir = 2;
                    }
                    else
                    {
                        // on D
                        f = Face.U;
                        switch (idx)
                        {
                            case 4:
                                dir = 2; break;
                            case 5:
                                dir = 1; break;
                            case 6:
                                dir = 3; break;
                            default:
                                //azzert(false);
                                break;
                        }
                    }
                }
                else if (stickersByPiece[idx][1] == (int)Face.D)
                {
                    switch (idx)
                    {
                        case 0:
                        case 6:
                            f = Face.F; break; // on R
                        case 1:
                        case 4:
                            f = Face.L; break; // on F
                        case 2:
                        case 7:
                            f = Face.R; break; // on B
                        case 3:
                        case 5:
                            f = Face.B; break; // on L
                        default:
                            //azzert(false);
                            break;
                    }
                }
                else
                {
                    switch (idx)
                    {
                        case 2:
                        case 4:
                            f = Face.F; break; // on R
                        case 0:
                        case 5:
                            f = Face.L; break; // on F
                        case 3:
                        case 6:
                            f = Face.R; break; // on B
                        case 1:
                        case 7:
                            f = Face.B; break; // on L
                        default:
                            //azzert(false);
                            break;
                    }
                }
                spinCube(image, (Face)f, dir);
                spins++;
            }

            return image;
        }

        private bool isNormalized(int[][][] image)
        {
            // A CubeState is normalized if the BLD piece is solved
            return image[(int)Face.B][size - 1][size - 1] == (int)Face.B &&
                    image[(int)Face.L][size - 1][0] == (int)Face.L &&
                    image[(int)Face.D][size - 1][0] == (int)Face.D;
        }

        protected internal static int[][] getStickersByPiece(int[][][] img)
        {
            int s = img[0].Length - 1;
            return new int[][] {
            new int[] { img[(int)Face.U][s][s], img[(int)Face.R][0][0], img[(int)Face.F][0][s] },
            new int[] { img[(int)Face.U][s][0], img[(int)Face.F][0][0], img[(int)Face.L][0][s] },
            new int[] { img[(int)Face.U][0][s], img[(int)Face.B][0][0], img[(int)Face.R][0][s] },
            new int[] { img[(int)Face.U][0][0], img[(int)Face.L][0][0], img[(int)Face.B][0][s] },

            new int[] { img[(int)Face.D][0][s], img[(int)Face.F][s][s], img[(int)Face.R][s][0] },
            new int[] { img[(int)Face.D][0][0], img[(int)Face.L][s][s], img[(int)Face.F][s][0] },
            new int[] { img[(int)Face.D][s][s], img[(int)Face.R][s][s], img[(int)Face.B][s][0] },
            new int[] { img[(int)Face.D][s][0], img[(int)Face.B][s][s], img[(int)Face.L][s][0] }
        };
        }

        public class CubeState : PuzzleState
        {
            private CubePuzzle cubePuzzle;
            private readonly int[][][] image;
            private CubeState normalizedState = null;

            public CubeState(CubePuzzle cp) : base(cp)
            {
                cubePuzzle = cp;
                image = new int[6][][];
                for (int i = 0; i < 6; i++)
                {
                    image[i] = new int[cubePuzzle.size][];
                    for (int j = 0; j < cubePuzzle.size; j++)
                    {
                        image[i][j] = new int[cubePuzzle.size];
                    }
                }
                for (int face = 0; face < image.Length; face++)
                {
                    for (int j = 0; j < cubePuzzle.size; j++)
                    {
                        for (int k = 0; k < cubePuzzle.size; k++)
                        {
                            image[face][j][k] = face;
                        }
                    }
                }
                normalizedState = this;
            }

            public CubeState(int[][][] image, CubePuzzle cp) : base(cp)
            {
                cubePuzzle = cp;
                this.image = image;
            }

            public override bool isNormalized()
            {
                return cubePuzzle.isNormalized(image);
            }

            public override PuzzleState getNormalized()
            {
                if (normalizedState == null)
                {
                    int[][][] normalizedImage = cubePuzzle.normalize(image);
                    normalizedState = new CubeState(normalizedImage, cubePuzzle);
                }
                return normalizedState;
            }

            public TwoByTwoState toTwoByTwoState()
            {
                TwoByTwoState state = new TwoByTwoState();

                int[][] stickersByPiece = getStickersByPiece(image);

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

                int dColor = stickersByPiece[7][0];
                int bColor = stickersByPiece[7][1];
                int lColor = stickersByPiece[7][2];

                int uColor = (int)((Face)dColor).oppositeFace();
                int fColor = (int)((Face)bColor).oppositeFace();
                int rColor = (int)((Face)lColor).oppositeFace();

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
                    int[] stickers = stickersByPiece[i];
                    int pieceVal = colorToVal[stickers[0]] + colorToVal[stickers[1]] + colorToVal[stickers[2]];

                    int clockwiseTurnsToGetToPrimaryColor = 0;
                    while (stickers[clockwiseTurnsToGetToPrimaryColor] != uColor && stickers[clockwiseTurnsToGetToPrimaryColor] != dColor)
                    {
                        clockwiseTurnsToGetToPrimaryColor++;
                        //azzert(clockwiseTurnsToGetToPrimaryColor < 3);
                    }
                    int piece = (clockwiseTurnsToGetToPrimaryColor << 3) + pieceVal;
                    pieces[i] = piece;
                }

                state.permutation = TwoByTwoSolver.packPerm(pieces);
                state.orientation = TwoByTwoSolver.packOrient(pieces);
                return state;
            }

            public string toFaceCube()
            {
                //azzert(size == 3);
                string state = "";
                foreach (char f in "URFDLB")
                {
                    Face face = (Face)Enum.Parse(typeof(Face), "" + f);
                    int[][] faceArr = image[(int)face];
                    for (int i = 0; i < faceArr.Length; i++)
                    {
                        for (int j = 0; j < faceArr[i].Length; j++)
                        {
                            state += ((Face)faceArr[i][j]).ToString();
                        }
                    }
                }
                return state;
            }

            public override LinkedHashMap<string, PuzzleState> getSuccessorsByName()
            {
                return getSuccessorsWithinSlice(cubePuzzle.size - 1, true);
            }

            public override LinkedHashMap<string, PuzzleState> getScrambleSuccessors()
            {
                return getSuccessorsWithinSlice((int)(cubePuzzle.size / 2) - 1, false);
            }

            public override LinkedHashMap<PuzzleState, string> getCanonicalMovesByState()
            {
                return GwtSafeUtils.reverseHashMap(getScrambleSuccessors());
            }

            private LinkedHashMap<string, PuzzleState> getSuccessorsWithinSlice(int maxSlice, bool includeRedundant)
            {
                LinkedHashMap<String, PuzzleState> successors = new LinkedHashMap<string, PuzzleState>();
                for (int innerSlice = 0; innerSlice <= maxSlice; innerSlice++)
                {
                    foreach (Face face in Enum.GetValues(typeof(Face)))
                    {
                        bool halfOfEvenCube = cubePuzzle.size % 2 == 0 && (innerSlice == (cubePuzzle.size / 2) - 1);
                        if (!includeRedundant && (int)face >= 3 && halfOfEvenCube)
                        {
                            // Skip turning the other halves of even sized cubes
                            continue;
                        }
                        int outerSlice = 0;
                        for (int dir = 1; dir <= 3; dir++)
                        {
                            CubeMove move = new CubeMove(face, dir, innerSlice, outerSlice, cubePuzzle);
                            string moveStr = move.ToString();
                            if (moveStr == null)
                            {
                                // Skip unnamed rotations.
                                continue;
                            }

                            int[][][] imageCopy = cubePuzzle.cloneImage(image);
                            for (int slice = outerSlice; slice <= innerSlice; slice++)
                            {
                                CubePuzzle.slice(face, slice, dir, imageCopy);
                            }
                            successors[moveStr] = new CubeState(imageCopy, cubePuzzle);
                        }
                    }
                }

                return successors;
            }

            public override bool Equals(object other)
            {
                //return Arrays.deepEquals(image, ((CubeState)other).image);
                for (int i = 0; i < image.Length; i++)
                {
                    for (int j = 0; j < image[i].Length; j++)
                    {
                        if (image[i][j] != ((CubeState)other).image[i][j]) return false;
                    }
                }
                return true;
            }

            public override int GetHashCode()
            {
                return image.GetHashCode();
            }
        }

    }
}

