using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Utils;

namespace TNoodle.Puzzles
{
    public class MegaminxPuzzle : Puzzle
    {
        public enum Face
        {
            U, BL, BR, R, F, L, D, DR, DBR, B, DBL, DL


        }
        private const int gap = 2;
        private const int minxRad = 30;

        public MegaminxPuzzle() { }

        public override String GetLongName()
        {
            return "Megaminx";
        }

        public override String GetShortName()
        {
            return "minx";
        }



        private static readonly double UNFOLDHEIGHT = 2 + 3 * Math.Sin(.3 * Math.PI) + Math.Sin(.1 * Math.PI);
        private static readonly double UNFOLDWIDTH = 4 * Math.Cos(.1 * Math.PI) + 2 * Math.Cos(.3 * Math.PI);

        private static void turn(int[,] image, Face side, int dir)
        {
            dir = Functions.modulo(dir, 5);
            for (int i = 0; i < dir; i++)
            {
                turn(image, side);
            }
        }

        private static void turn(int[,] image, Face face)
        {
            int s = (int)face;
            int b = (s >= 6 ? 6 : 0);
            switch (s % 6)
            {
                case 0:
                    swapOnSide(image, b, 1, 6, 5, 4, 4, 2, 3, 0, 2, 8); break;
                case 1:
                    swapOnSide(image, b, 0, 0, 2, 0, 9, 6, 10, 6, 5, 2); break;
                case 2:
                    swapOnSide(image, b, 0, 2, 3, 2, 8, 4, 9, 4, 1, 4); break;
                case 3:
                    swapOnSide(image, b, 0, 4, 4, 4, 7, 2, 8, 2, 2, 6); break;
                case 4:
                    swapOnSide(image, b, 0, 6, 5, 6, 11, 0, 7, 0, 3, 8); break;
                case 5:
                    swapOnSide(image, b, 0, 8, 1, 8, 10, 8, 11, 8, 4, 0); break;
                default:
                    //azzert(false);bre
                    break;
            }

            rotateFace(image, face);
        }

        private static void swapOnSide(int[,] image, int b, int f1, int s1, int f2, int s2, int f3, int s3, int f4, int s4, int f5, int s5)
        {
            for (int i = 0; i < 3; i++)
            {
                int temp = image[(f1 + b) % 12, (s1 + i) % 10];
                image[(f1 + b) % 12, (s1 + i) % 10] = image[(f2 + b) % 12, (s2 + i) % 10];
                image[(f2 + b) % 12, (s2 + i) % 10] = image[(f3 + b) % 12, (s3 + i) % 10];
                image[(f3 + b) % 12, (s3 + i) % 10] = image[(f4 + b) % 12, (s4 + i) % 10];
                image[(f4 + b) % 12, (s4 + i) % 10] = image[(f5 + b) % 12, (s5 + i) % 10];
                image[(f5 + b) % 12, (s5 + i) % 10] = temp;
            }
        }

        private static void swapOnFace(int[,] image, Face face, int s1, int s2, int s3, int s4, int s5)
        {
            int f = (int)face;
            int temp = image[f, s1];
            image[f, s1] = image[f, s2];
            image[f, s2] = image[f, s3];
            image[f, s3] = image[f, s4];
            image[f, s4] = image[f, s5];
            image[f, s5] = temp;
        }

        private static void rotateFace(int[,] image, Face f)
        {
            swapOnFace(image, f, 0, 8, 6, 4, 2);
            swapOnFace(image, f, 1, 9, 7, 5, 3);
        }

        private static void bigTurn(int[,] image, Face side, int dir)
        {
            dir = Functions.modulo(dir, 5);
            for (int i = 0; i < dir; i++)
            {
                bigTurn(image, side);
            }
        }

        private static void bigTurn(int[,] image, Face f)
        {
            if (f == Face.DBR)
            {
                for (int i = 0; i < 7; i++)
                {
                    swap(image, 0, (1 + i) % 10, 4, (3 + i) % 10, 11, (1 + i) % 10, 10, (1 + i) % 10, 1, (1 + i) % 10);
                }
                swapCenters(image, 0, 4, 11, 10, 1);

                swapWholeFace(image, 2, 0, 3, 0, 7, 0, 6, 8, 9, 8);

                rotateFace(image, Face.DBR);
            }
            else
            {
                //azzert(f == Face.D);
                for (int i = 0; i < 7; i++)
                {
                    swap(image, 1, (9 + i) % 10, 2, (1 + i) % 10, 3, (3 + i) % 10, 4, (5 + i) % 10, 5, (7 + i) % 10);
                }
                swapCenters(image, 1, 2, 3, 4, 5);

                swapWholeFace(image, 11, 0, 10, 8, 9, 6, 8, 4, 7, 2);

                rotateFace(image, Face.D);
            }
        }

        private static void swap(int[,] image, int f1, int s1, int f2, int s2, int f3, int s3, int f4, int s4, int f5, int s5)
        {
            int temp = image[f1, s1];
            image[f1, s1] = image[f2, s2];
            image[f2, s2] = image[f3, s3];
            image[f3, s3] = image[f4, s4];
            image[f4, s4] = image[f5, s5];
            image[f5, s5] = temp;
        }

        private static void swapCenters(int[,] image, int f1, int f2, int f3, int f4, int f5)
        {
            swap(image, f1, 10, f2, 10, f3, 10, f4, 10, f5, 10);
        }

        private static void swapWholeFace(int[,] image, int f1, int s1, int f2, int s2, int f3, int s3, int f4, int s4, int f5, int s5)
        {
            for (int i = 0; i < 10; i++)
            {
                int temp = image[(f1) % 12, (s1 + i) % 10];
                image[(f1) % 12, (s1 + i) % 10] = image[(f2) % 12, (s2 + i) % 10];
                image[(f2) % 12, (s2 + i) % 10] = image[(f3) % 12, (s3 + i) % 10];
                image[(f3) % 12, (s3 + i) % 10] = image[(f4) % 12, (s4 + i) % 10];
                image[(f4) % 12, (s4 + i) % 10] = image[(f5) % 12, (s5 + i) % 10];
                image[(f5) % 12, (s5 + i) % 10] = temp;
            }
            swapCenters(image, f1, f2, f3, f4, f5);
        }


        public override PuzzleState GetSolvedState()
        {
            return new MegaminxState(this);
        }

        protected  override int GetRandomMoveCount()
        {
            return 11 * 7;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            StringBuilder scramble = new StringBuilder();

            int width = 10, height = 7;
            for (int i = 0; i < height; i++)
            {
                if (i > 0)
                {
                    scramble.Append("\n");
                }
                int dir = 0;
                for (int j = 0; j < width; j++)
                {
                    if (j > 0)
                    {
                        scramble.Append(" ");
                    }
                    char side = (j % 2 == 0) ? 'R' : 'D';
                    dir = r.Next(2);
                    scramble.Append(side + ((dir == 0) ? "++" : "--"));
                }
                scramble.Append(" U");
                if (dir != 0)
                {
                    scramble.Append("'");
                }
            }

            String scrambleStr = scramble.ToString();

            PuzzleState state = GetSolvedState();
            //try
            {
                state = state.ApplyAlgorithm(scrambleStr);
            }
            //catch //(InvalidScrambleException e)
            {
                //azzert(false, e);
                //return null;
            }
            return new PuzzleStateAndGenerator(state, scrambleStr);
        }

        private int centerIndex = 10;
        private bool isNormalized(int[,] image)
        {
            return image[(int)Face.U, centerIndex] == (int)Face.U && image[(int)Face.F, centerIndex] == (int)Face.F;
        }

        private int[,] cloneImage(int[,] image)
        {
            int[,] imageCopy = new int[image.GetLength(0), image.GetLength(1)];
            Array.Copy(image, imageCopy, image.Length);
            //GwtSafeUtils.deepCopy(image, imageCopy);
            return imageCopy;
        }

        private void spinMinx(int[,] image, Face face, int dir)
        {
            turn(image, face, dir);
            bigTurn(image, (Face)face.oppositeFace(), 5 - dir);
        }

        private void spinToTop(int[,] image, Face face)
        {
            switch (face)
            {
                case Face.U:
                    break;
                case Face.BL:
                    spinMinx(image, Face.L, 1);
                    break;
                case Face.BR:
                    spinMinx(image, Face.U, 1);
                    spinToTop(image, Face.R);
                    break;
                case Face.R:
                    spinMinx(image, Face.U, 1);
                    spinToTop(image, Face.F);
                    break;
                case Face.F:
                    spinMinx(image, Face.L, -1);
                    break;
                case Face.L:
                    spinMinx(image, Face.U, 1);
                    spinToTop(image, Face.BL);
                    break;
                case Face.D:
                    spinMinx(image, Face.L, -2);
                    spinToTop(image, Face.R);
                    break;
                case Face.DR:
                    spinMinx(image, Face.L, -1);
                    spinToTop(image, Face.R);
                    break;
                case Face.DBR:
                    spinMinx(image, Face.U, 1);
                    spinMinx(image, Face.L, -1);
                    spinToTop(image, Face.R);
                    break;
                case Face.B:
                    spinMinx(image, Face.L, -3);
                    spinToTop(image, Face.R);
                    break;
                case Face.DBL:
                    spinMinx(image, Face.L, 2);
                    break;
                case Face.DL:
                    spinMinx(image, Face.L, -2);
                    break;
                default:
                    //azzert(false);
                    break;
            }
        }

        private int[,] normalize(int[,] image)
        {
            if (isNormalized(image))
            {
                return image;
            }

            image = cloneImage(image);
            foreach (Face face in Enum.GetValues(typeof(Face)))
            {
                if (image[(int)face, centerIndex] == (int)Face.U)
                {
                    spinToTop(image, face);
                    //azzert(image[Face.U.ordinal()][centerIndex] == Face.U.ordinal());
                    for (int chooseF = 0; chooseF < 5; chooseF++)
                    {
                        spinMinx(image, Face.U, 1);
                        if (isNormalized(image))
                        {
                            return image;
                        }
                    }
                    //azzert(false);
                }
            }
            //azzert(false);
            return null;
        }

        internal class MegaminxState : PuzzleState
        {
            private MegaminxPuzzle puzzle;
            private readonly int[,] image;
            private MegaminxState normalizedState;
            public MegaminxState(MegaminxPuzzle p) : base(p)
            {
                puzzle = p;
                image = new int[12, 11];
                for (int i = 0; i < image.GetLength(0); i++)
                {
                    for (int j = 0; j < image.GetLength(1); j++)
                    {
                        image[i, j] = i;
                    }
                }
                normalizedState = this;
            }

            public MegaminxState(int[,] image, MegaminxPuzzle p) : base(p)
            {
                puzzle = p;
                this.image = image;
            }

            public override PuzzleState GetNormalized()
            {
                if (normalizedState == null)
                {
                    int[,] normalizedImage = puzzle.normalize(image);
                    normalizedState = new MegaminxState(puzzle.normalize(image), puzzle);
                }
                return normalizedState;
            }

            public override bool IsNormalized()
            {
                return puzzle.isNormalized(image);
            }

            public override LinkedHashMap<String, PuzzleState> GetSuccessorsByName()
            {
                LinkedHashMap<String, PuzzleState> successors = new LinkedHashMap<String, PuzzleState>();

                String[] prettyDir = new String[] { null, "", "2", "2'", "'" };
                foreach (Face face in Enum.GetValues(typeof(Face)))
                {
                    for (int dir = 1; dir <= 4; dir++)
                    {
                        String move = face.ToString();
                        move += prettyDir[dir];

                        int[,] imageCopy = puzzle.cloneImage(image);
                        turn(imageCopy, face, dir);

                        successors[move] = new MegaminxState(imageCopy, puzzle);
                    }
                }

                Dictionary<String, Face> pochmannFaceNames = new Dictionary<String, Face>();
                pochmannFaceNames["R"] = Face.DBR;
                pochmannFaceNames["D"] = Face.D;
                String[] prettyPochmannDir = new String[] { null, "+", "++", "--", "-" };
                foreach (String pochmannFaceName in pochmannFaceNames.Keys)
                {
                    for (int dir = 1; dir < 5; dir++)
                    {
                        String move = pochmannFaceName + prettyPochmannDir[dir];

                        int[,] imageCopy = puzzle.cloneImage(image);
                        bigTurn(imageCopy, pochmannFaceNames[pochmannFaceName], dir);

                        successors[move] = new MegaminxState(imageCopy, puzzle);
                    }
                }
                return successors;
            }

            public override LinkedHashMap<String, PuzzleState> GetScrambleSuccessors()
            {
                LinkedHashMap<String, PuzzleState> successors = GetSuccessorsByName();
                LinkedHashMap<String, PuzzleState> scrambleSuccessors = new LinkedHashMap<String, PuzzleState>();
                foreach (String turn in new String[] { "R++", "R--", "D++", "D--", "U", "U2", "U2'", "U'" })
                {
                    scrambleSuccessors[turn] = successors[turn];
                }
                return scrambleSuccessors;
            }

            public override bool Equals(Object other)
            {
                MegaminxState o = ((MegaminxState)other);
                return image.DeepEquals(o.image);
            }

            public override int GetHashCode()
            {
                return Functions.DeepHashCode(image);
            }


        }

    }

}
