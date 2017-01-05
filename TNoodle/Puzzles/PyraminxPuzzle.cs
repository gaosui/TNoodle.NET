using System;
using TNoodle.Solvers;
using TNoodle.Utils;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
    public class PyraminxPuzzle : Puzzle
    {
        //private static final Logger l = Logger.getLogger(PyraminxPuzzle.class.getName());

        private const int MinScrambleLength = 11;
        private const bool ScrambleLengthIncludesTips = true;
        private readonly PyraminxSolver _pyraminxSolver = new PyraminxSolver();

        public PyraminxPuzzle()
        {
            WcaMinScrambleDistance = 6;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            var state = _pyraminxSolver.RandomState(r);
            var scramble = _pyraminxSolver.GenerateExactly(state, MinScrambleLength, false);

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
            return "Pyraminx";
        }

        public override string GetShortName()
        {
            return "pyram";
        }

        public override PuzzleState GetSolvedState()
        {
            return new PyraminxState(this);
        }

        protected override int GetRandomMoveCount()
        {
            return 15;
        }

        public class PyraminxState : PuzzleState
        {
            private readonly int[][] _image;
            private readonly PyraminxPuzzle _puzzle;
            /** Trying to make an ascii art of the pyraminx stickers position...
              *
              *                                    U
              *              ____  ____  ____              ____  ____  ____
              *             \    /\    /\    /     /\     \    /\    /\    /
              *              \0 /1 \2 /4 \3 /     /0 \     \0 /1 \2 /4 \3 /
              *               \/____\/____\/     /____\     \/____\/____\/
              *                \    /\    /     /\    /\     \    /\    /
              *        face 2   \8 /7 \5 /     /8 \1 /2 \     \8 /7 \5 / face 3
              *                  \/____\/     /____\/____\     \/____\/
              *                   \    /     /\    /\    /\     \    /
              *                    \6 /     /6 \7 /5 \4 /3 \     \6 /
              *                     \/     /____\/____\/____\     \/
              *                                  face 0
              *                        L    ____  ____  ____    R
              *                            \    /\    /\    /
              *                             \0 /1 \2 /4 \3 /
              *                              \/____\/____\/
              *                               \    /\    /
              *                                \8 /7 \5 /
              *                         face 1  \/____\/
              *                                  \    /
              *                                   \6 /
              *                                    \/
              *
              *                                    B
              */

            public PyraminxState(PyraminxPuzzle p) : base(p)
            {
                _puzzle = p;
                _image = ArrayExtension.New<int>(4, 9);
                for (var i = 0; i < _image.Length; i++)
                {
                    for (var j = 0; j < _image[0].Length; j++)
                    {
                        _image[i][j] = i;
                    }
                }
            }

            public PyraminxState(int[][] image, PyraminxPuzzle p) : base(p)
            {
                _puzzle = p;
                _image = image;
            }

            private void Turn(int side, int dir, int[][] image)
            {
                for (var i = 0; i < dir; i++)
                {
                    Turn(side, image);
                }
            }

            private void TurnTip(int side, int dir, int[][] image)
            {
                for (var i = 0; i < dir; i++)
                {
                    TurnTip(side, image);
                }
            }

            private void Turn(int s, int[][] image)
            {
                switch (s)
                {
                    case 0:
                        Swap(0, 8, 3, 8, 2, 2, image);
                        Swap(0, 1, 3, 1, 2, 4, image);
                        Swap(0, 2, 3, 2, 2, 5, image);
                        break;
                    case 1:
                        Swap(2, 8, 1, 2, 0, 8, image);
                        Swap(2, 7, 1, 1, 0, 7, image);
                        Swap(2, 5, 1, 8, 0, 5, image);
                        break;
                    case 2:
                        Swap(3, 8, 0, 5, 1, 5, image);
                        Swap(3, 7, 0, 4, 1, 4, image);
                        Swap(3, 5, 0, 2, 1, 2, image);
                        break;
                    case 3:
                        Swap(1, 8, 2, 2, 3, 5, image);
                        Swap(1, 7, 2, 1, 3, 4, image);
                        Swap(1, 5, 2, 8, 3, 2, image);
                        break;
                    default:
                        Assert(false);
                        break;
                }
                TurnTip(s, image);
            }

            private void TurnTip(int s, int[][] image)
            {
                switch (s)
                {
                    case 0:
                        Swap(0, 0, 3, 0, 2, 3, image);
                        break;
                    case 1:
                        Swap(0, 6, 2, 6, 1, 0, image);
                        break;
                    case 2:
                        Swap(0, 3, 1, 3, 3, 6, image);
                        break;
                    case 3:
                        Swap(1, 6, 2, 0, 3, 3, image);
                        break;
                    default:
                        Assert(false);
                        break;
                }
            }

            private static void Swap(int f1, int s1, int f2, int s2, int f3, int s3, int[][] image)
            {
                var temp = image[f1][s1];
                image[f1][s1] = image[f2][s2];
                image[f2][s2] = image[f3][s3];
                image[f3][s3] = temp;
            }

            public PyraminxSolverState ToPyraminxSolverState()
            {
                var state = new PyraminxSolverState();

                /** Each face color is assigned a value so that the sum of the color (minus 1) of each edge gives a unique integer.
                  * These edge values match the edge numbering in the PyraminxSolver class, making the following code simpler.
                  *                                    U
                  *              ____  ____  ____              ____  ____  ____
                  *             \    /\    /\    /     /\     \    /\    /\    /
                  *              \  /  \5 /  \  /     /  \     \  /  \5 /  \  /
                  *               \/____\/____\/     /____\     \/____\/____\/
                  *                \    /\    /     /\    /\     \    /\    /
                  *        face +2  \2 /  \1 /     /1 \  /3 \     \3 /  \4 / face +4
                  *                  \/____\/     /____\/____\     \/____\/
                  *                   \    /     /\    /\    /\     \    /
                  *                    \  /     /  \  /0 \  /  \     \  /
                  *                     \/     /____\/____\/____\     \/
                  *                                  face +0
                  *                        L    ____  ____  ____    R
                  *                            \    /\    /\    /
                  *                             \  /  \0 /  \  /
                  *                              \/____\/____\/
                  *                               \    /\    /
                  *                                \2 /  \4 /
                  *                         face +1 \/____\/
                  *                                  \    /
                  *                                   \  /
                  *                                    \/
                  *
                  *                                    B
                  */
                int[][] stickersToEdges =
                {
                    new[] {_image[0][5], _image[1][2]},
                    new[] {_image[0][8], _image[2][5]},
                    new[] {_image[1][8], _image[2][8]},
                    new[] {_image[0][2], _image[3][8]},
                    new[] {_image[1][5], _image[3][5]},
                    new[] {_image[2][2], _image[3][2]}
                };

                int[] colorToValue = {0, 1, 2, 4};

                var edges = new int[6];
                for (var i = 0; i < edges.Length; i++)
                {
                    edges[i] = colorToValue[stickersToEdges[i][0]] + colorToValue[stickersToEdges[i][1]] - 1;
                    // In the PyraminxSolver class, the primary facelet of each edge correspond to the lowest face number.
                    if (stickersToEdges[i][0] > stickersToEdges[i][1])
                        edges[i] += 8;
                }

                state.EdgePerm = PyraminxSolver.PackEdgePerm(edges);
                state.EdgeOrient = PyraminxSolver.PackEdgeOrient(edges);

                int[][] stickersToCorners =
                {
                    new[] {_image[0][1], _image[2][4], _image[3][1]},
                    new[] {_image[0][7], _image[1][1], _image[2][7]},
                    new[] {_image[0][4], _image[3][7], _image[1][4]},
                    new[] {_image[1][7], _image[3][4], _image[2][1]}
                };

                /* The corners are supposed to be fixed, so we are also checking if they are in the right place.
                 * We can use the sum trick, but here, no need for transition table :) */

                var corners = new int[4];
                for (var i = 0; i < corners.Length; i++)
                {
                    //azzertEquals(stickersToCorners[i][0] + stickersToCorners[i][1] + stickersToCorners[i][2], correctSum[i]);
                    // The following code is not pretty, sorry...
                    if (stickersToCorners[i][0] < stickersToCorners[i][1] &&
                        stickersToCorners[i][0] < stickersToCorners[i][2])
                        corners[i] = 0;
                    if (stickersToCorners[i][1] < stickersToCorners[i][0] &&
                        stickersToCorners[i][1] < stickersToCorners[i][2])
                        corners[i] = 1;
                    if (stickersToCorners[i][2] < stickersToCorners[i][1] &&
                        stickersToCorners[i][2] < stickersToCorners[i][0])
                        corners[i] = 2;
                }

                state.CornerOrient = PyraminxSolver.PackCornerOrient(corners);

                /* For the tips, we use the same numbering */
                int[][] stickersToTips =
                {
                    new[] {_image[0][0], _image[2][3], _image[3][0]},
                    new[] {_image[0][6], _image[1][0], _image[2][6]},
                    new[] {_image[0][3], _image[3][6], _image[1][3]},
                    new[] {_image[1][6], _image[3][3], _image[2][0]}
                };

                var tips = new int[4];
                for (var i = 0; i < tips.Length; i++)
                {
                    //int[] stickers = stickersToTips[i];
                    // We can use the same color check as for the corners.
                    //azzertEquals(stickers[0] + stickers[1] + stickers[2], correctSum[i]);

                    // For the tips, we don't have to check colors against face, but against the attached corner.
                    var cornerPrimaryColor = stickersToCorners[i][0];
                    var clockwiseTurnsToMatchCorner = 0;
                    while (stickersToTips[i][clockwiseTurnsToMatchCorner] != cornerPrimaryColor)
                        clockwiseTurnsToMatchCorner++;
                    tips[i] = clockwiseTurnsToMatchCorner;
                }

                state.Tips = PyraminxSolver.PackCornerOrient(tips); // Same function as for corners.

                return state;
            }

            public override string SolveIn(int n)
            {
                return _puzzle._pyraminxSolver.SolveIn(ToPyraminxSolverState(), n, ScrambleLengthIncludesTips);
            }

            public override LinkedHashMap<string, PuzzleState> GetSuccessorsByName()
            {
                var successors = new LinkedHashMap<string, PuzzleState>();

                const string axes = "ulrb";
                for (var axis = 0; axis < axes.Length; axis++)
                {
                    foreach (var tip in new[] {true, false})
                    {
                        var face = axes[axis];
                        face = tip ? char.ToLower(face) : char.ToUpper(face);
                        for (var dir = 1; dir <= 2; dir++)
                        {
                            var turn = "" + face;
                            if (dir == 2)
                                turn += "'";

                            var imageCopy = ArrayExtension.New<int>(_image.Length, _image[0].Length);
                            //GwtSafeUtils.deepCopy(image, imageCopy);
                            _image.DeepCopyTo(imageCopy);

                            if (tip)
                                TurnTip(axis, dir, imageCopy);
                            else
                                Turn(axis, dir, imageCopy);

                            successors[turn] = new PyraminxState(imageCopy, _puzzle);
                        }
                    }
                }

                return successors;
            }

            public override bool Equals(object other)
            {
                // Sure this could blow up with a cast exception, but shouldn't it? =)
                return _image.DeepEquals(((PyraminxState) other)._image);
            }

            public override int GetHashCode()
            {
                return _image.DeepHashCode();
            }
        }
    }
}