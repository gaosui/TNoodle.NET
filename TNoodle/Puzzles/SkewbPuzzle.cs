using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Solvers;
using TNoodle.Utils;

namespace TNoodle.Puzzles
{
    public class SkewbPuzzle : Puzzle
    {
        private const int MIN_SCRAMBLE_LENGTH = 11;
        //private static final Logger l = Logger.getLogger(SkewbPuzzle.class.getName());
        private SkewbSolver skewbSolver = null;

        private const int pieceSize = 30;
        private const int gap = 3;

        private static readonly double sq3d2 = Math.Sqrt(3) / 2;

        public SkewbPuzzle()
        {
            skewbSolver = new SkewbSolver();
            WcaMinScrambleDistance = 7;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            SkewbSolver.SkewbSolverState state = skewbSolver.randomState(r);
            String scramble = skewbSolver.generateExactly(state, MIN_SCRAMBLE_LENGTH, r);

            PuzzleState pState;
            try
            {
                pState = GetSolvedState().applyAlgorithm(scramble);
            }
            catch //(InvalidScrambleException e)
            {
                //azzert(false, e);
                return null;
            }
            return new PuzzleStateAndGenerator(pState, scramble);
        }

        public override String GetLongName()
        {
            return "Skewb";
        }

        public override String GetShortName()
        {
            return "skewb";
        }

        public override PuzzleState GetSolvedState()
        {
            return new SkewbState(this);
        }

        protected  override int GetRandomMoveCount()
        {
            return 15;
        }

        public class SkewbState : PuzzleState
        {
            private SkewbPuzzle puzzle;

            /**
             *           +---------+
             *           | 1     2 |
             *       U > |   0-0   |
             *           | 3     4 |
             * +---------+---------+---------+---------+
             * | 1     2 | 1     2 | 1     2 | 1     2 |
             * |   4-0   |   2-0   |   1-0   |   5-0   |
             * | 3     4 | 3     4 | 3     4 | 3     4 |
             * +---------+---------+---------+---------+
             *      ^    | 1     2 |
             *      FL   |   3-0   |
             *           | 3     4 |
             *           +---------+
             */
            private int[,] image = new int[6, 5];

            internal SkewbState(SkewbPuzzle p) : base(p)
            {
                puzzle = p;
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        image[i, j] = i;
                    }
                }
            }

            internal SkewbState(int[,] _image, SkewbPuzzle p) : base(p)
            {
                puzzle = p;
                for (int i = 0; i < 6; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        image[i, j] = _image[i, j];
                    }
                }
            }

            private void turn(int axis, int pow, int[,] image)
            {
                //axis:0-R 1-U 2-L 3-B
                for (int p = 0; p < pow; p++)
                {
                    switch (axis)
                    {
                        case 0:
                            swap(2, 0, 3, 0, 1, 0, image);
                            swap(2, 4, 3, 2, 1, 3, image);
                            swap(2, 2, 3, 1, 1, 4, image);
                            swap(2, 3, 3, 4, 1, 1, image);
                            swap(4, 4, 5, 3, 0, 4, image);
                            break;
                        case 1:
                            swap(0, 0, 1, 0, 5, 0, image);
                            swap(0, 2, 1, 2, 5, 1, image);
                            swap(0, 4, 1, 4, 5, 2, image);
                            swap(0, 1, 1, 1, 5, 3, image);
                            swap(4, 1, 2, 2, 3, 4, image);
                            break;
                        case 2:
                            swap(4, 0, 5, 0, 3, 0, image);
                            swap(4, 3, 5, 4, 3, 3, image);
                            swap(4, 1, 5, 3, 3, 1, image);
                            swap(4, 4, 5, 2, 3, 4, image);
                            swap(2, 3, 0, 1, 1, 4, image);
                            break;
                        case 3:
                            swap(1, 0, 3, 0, 5, 0, image);
                            swap(1, 4, 3, 4, 5, 3, image);
                            swap(1, 3, 3, 3, 5, 1, image);
                            swap(1, 2, 3, 2, 5, 4, image);
                            swap(0, 2, 2, 4, 4, 3, image);
                            break;
                        default:
                            //azzert(false);
                            break;
                    }
                }
            }

            private void swap(int f1, int s1, int f2, int s2, int f3, int s3, int[,] image)
            {
                int temp = image[f1, s1];
                image[f1, s1] = image[f2, s2];
                image[f2, s2] = image[f3, s3];
                image[f3, s3] = temp;
            }



            public override LinkedHashMap<String, PuzzleState> GetSuccessorsByName()
            {
                LinkedHashMap<String, PuzzleState> successors = new LinkedHashMap<String, PuzzleState>();
                String axes = "RULB";
                for (int axis = 0; axis < axes.Length; axis++)
                {
                    char face = axes[axis];
                    for (int pow = 1; pow <= 2; pow++)
                    {
                        String turn = "" + face;
                        if (pow == 2)
                        {
                            turn += "'";
                        }
                        //int[][] imageCopy = new int[image.length][image[0].length];
                        //GwtSafeUtils.deepCopy(image, imageCopy);
                        int[,] imageCopy = (int[,])image.Clone();
                        this.turn(axis, pow, imageCopy);
                        successors[turn] = new SkewbState(imageCopy, puzzle);
                    }
                }

                return successors;
            }

            public override bool Equals(Object other)
            {
                // Sure this could blow up with a cast exception, but shouldn't it? =)
                return Functions.DeepEquals(image, ((SkewbState)other).image);
            }

            public override int GetHashCode()
            {
                return Functions.DeepHashCode(image);
            }
        }

    }
}
