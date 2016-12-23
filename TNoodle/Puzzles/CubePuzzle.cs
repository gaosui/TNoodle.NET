using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Core;

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

    }  
}

