using System;
using TNoodle.Solvers;
using TNoodle.Utils;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
    public class SkewbPuzzle : Puzzle
    {
        private const int MinScrambleLength = 11;
        //private static final Logger l = Logger.getLogger(SkewbPuzzle.class.getName());
        private readonly SkewbSolver _skewbSolver = new SkewbSolver();


        public SkewbPuzzle()
        {
            WcaMinScrambleDistance = 7;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            var state = _skewbSolver.RandomState(r);
            var scramble = _skewbSolver.GenerateExactly(state, MinScrambleLength, r);

            PuzzleState pState;
            try
            {
                pState = GetSolvedState().ApplyAlgorithm(scramble);
            }
            catch (InvalidScrambleException e)
            {
                Assert(false, e.Message, e);
                return null;
            }
            return new PuzzleStateAndGenerator(pState, scramble);
        }

        public override string GetLongName()
        {
            return "Skewb";
        }

        public override string GetShortName()
        {
            return "skewb";
        }

        public override PuzzleState GetSolvedState()
        {
            return new SkewbState(this);
        }

        protected override int GetRandomMoveCount()
        {
            return 15;
        }

        public class SkewbState : PuzzleState
        {
            private readonly SkewbPuzzle _puzzle;

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
            private readonly int[][] _image = ArrayExtension.New<int>(6, 5);

            internal SkewbState(SkewbPuzzle p) : base(p)
            {
                _puzzle = p;
                for (var i = 0; i < 6; i++)
                {
                    for (var j = 0; j < 5; j++)
                    {
                        _image[i][j] = i;
                    }
                }
            }

            internal SkewbState(int[][] image, SkewbPuzzle p) : base(p)
            {
                _puzzle = p;
                for (var i = 0; i < 6; i++)
                {
                    for (var j = 0; j < 5; j++)
                    {
                        _image[i][j] = image[i][j];
                    }
                }
            }

            private void Turn(int axis, int pow, int[][] image)
            {
                //axis:0-R 1-U 2-L 3-B
                for (var p = 0; p < pow; p++)
                {
                    switch (axis)
                    {
                        case 0:
                            Swap(2, 0, 3, 0, 1, 0, image);
                            Swap(2, 4, 3, 2, 1, 3, image);
                            Swap(2, 2, 3, 1, 1, 4, image);
                            Swap(2, 3, 3, 4, 1, 1, image);
                            Swap(4, 4, 5, 3, 0, 4, image);
                            break;
                        case 1:
                            Swap(0, 0, 1, 0, 5, 0, image);
                            Swap(0, 2, 1, 2, 5, 1, image);
                            Swap(0, 4, 1, 4, 5, 2, image);
                            Swap(0, 1, 1, 1, 5, 3, image);
                            Swap(4, 1, 2, 2, 3, 4, image);
                            break;
                        case 2:
                            Swap(4, 0, 5, 0, 3, 0, image);
                            Swap(4, 3, 5, 4, 3, 3, image);
                            Swap(4, 1, 5, 3, 3, 1, image);
                            Swap(4, 4, 5, 2, 3, 4, image);
                            Swap(2, 3, 0, 1, 1, 4, image);
                            break;
                        case 3:
                            Swap(1, 0, 3, 0, 5, 0, image);
                            Swap(1, 4, 3, 4, 5, 3, image);
                            Swap(1, 3, 3, 3, 5, 1, image);
                            Swap(1, 2, 3, 2, 5, 4, image);
                            Swap(0, 2, 2, 4, 4, 3, image);
                            break;
                        default:
                            //azzert(false);
                            break;
                    }
                }
            }

            private static void Swap(int f1, int s1, int f2, int s2, int f3, int s3, int[][] image)
            {
                var temp = image[f1][s1];
                image[f1][s1] = image[f2][s2];
                image[f2][s2] = image[f3][s3];
                image[f3][s3] = temp;
            }


            public override LinkedHashMap<string, PuzzleState> GetSuccessorsByName()
            {
                var successors = new LinkedHashMap<string, PuzzleState>();
                const string axes = "RULB";
                for (var axis = 0; axis < axes.Length; axis++)
                {
                    var face = axes[axis];
                    for (var pow = 1; pow <= 2; pow++)
                    {
                        var turn = "" + face;
                        if (pow == 2)
                        {
                            turn += "'";
                        }
                        var imageCopy = ArrayExtension.New<int>(_image.Length, _image[0].Length);
                        _image.DeepCopyTo(imageCopy);
                        //GwtSafeUtils.deepCopy(image, imageCopy);
                        //var imageCopy = (int[,]) _image.Clone();
                        Turn(axis, pow, imageCopy);
                        successors[turn] = new SkewbState(imageCopy, _puzzle);
                    }
                }

                return successors;
            }

            public override bool Equals(object other)
            {
                // Sure this could blow up with a cast exception, but shouldn't it? =)
                return _image.DeepEquals(((SkewbState) other)._image);
            }

            public override int GetHashCode()
            {
                return _image.DeepHashCode();
            }
        }
    }
}