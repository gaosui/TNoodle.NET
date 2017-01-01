using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Solvers;

namespace TNoodle.Puzzles
{
    public class PyraminxPuzzle : Puzzle
    {
        //private static final Logger l = Logger.getLogger(PyraminxPuzzle.class.getName());

        private const int MIN_SCRAMBLE_LENGTH = 11;
        private const bool SCRAMBLE_LENGTH_INCLUDES_TIPS = true;
        private PyraminxSolver pyraminxSolver = null;

        public PyraminxPuzzle()
        {
            pyraminxSolver = new PyraminxSolver();
            WcaMinScrambleDistance = 6;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            PyraminxSolverState state = pyraminxSolver.randomState(r);
            String scramble = pyraminxSolver.generateExactly(state, MIN_SCRAMBLE_LENGTH, false);

            PuzzleState pState;
            try
            {
                pState = GetSolvedState().ApplyAlgorithm(scramble);
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
            return "Pyraminx";
        }

        public override String GetShortName()
        {
            return "pyram";
        }

        public override PuzzleState GetSolvedState()
        {
            return new PyraminxState(this);
        }

        protected  override int GetRandomMoveCount()
        {
            return 15;
        }

        public class PyraminxState : PuzzleState
        {
            private PyraminxPuzzle puzzle;
            private int[,] image;
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
                puzzle = p;
                image = new int[4, 9];
                for (int i = 0; i < image.GetLength(0); i++)
                {
                    for (int j = 0; j < image.GetLength(1); j++)
                    {
                        image[i, j] = i;
                    }
                }
            }

            public PyraminxState(int[,] image, PyraminxPuzzle p) : base(p)
            {
                puzzle = p;
                this.image = image;
            }

            private void turn(int side, int dir, int[,] image)
            {
                for (int i = 0; i < dir; i++)
                {
                    turn(side, image);
                }
            }

            private void turnTip(int side, int dir, int[,] image)
            {
                for (int i = 0; i < dir; i++)
                {
                    turnTip(side, image);
                }
            }

            private void turn(int s, int[,] image)
            {
                switch (s)
                {
                    case 0:
                        swap(0, 8, 3, 8, 2, 2, image);
                        swap(0, 1, 3, 1, 2, 4, image);
                        swap(0, 2, 3, 2, 2, 5, image);
                        break;
                    case 1:
                        swap(2, 8, 1, 2, 0, 8, image);
                        swap(2, 7, 1, 1, 0, 7, image);
                        swap(2, 5, 1, 8, 0, 5, image);
                        break;
                    case 2:
                        swap(3, 8, 0, 5, 1, 5, image);
                        swap(3, 7, 0, 4, 1, 4, image);
                        swap(3, 5, 0, 2, 1, 2, image);
                        break;
                    case 3:
                        swap(1, 8, 2, 2, 3, 5, image);
                        swap(1, 7, 2, 1, 3, 4, image);
                        swap(1, 5, 2, 8, 3, 2, image);
                        break;
                    default:
                        //azzert(false);
                        break;
                }
                turnTip(s, image);
            }

            private void turnTip(int s, int[,] image)
            {
                switch (s)
                {
                    case 0:
                        swap(0, 0, 3, 0, 2, 3, image);
                        break;
                    case 1:
                        swap(0, 6, 2, 6, 1, 0, image);
                        break;
                    case 2:
                        swap(0, 3, 1, 3, 3, 6, image);
                        break;
                    case 3:
                        swap(1, 6, 2, 0, 3, 3, image);
                        break;
                    default:
                        //azzert(false);
                        break;
                }
            }

            private void swap(int f1, int s1, int f2, int s2, int f3, int s3, int[,] image)
            {
                int temp = image[f1, s1];
                image[f1, s1] = image[f2, s2];
                image[f2, s2] = image[f3, s3];
                image[f3, s3] = temp;
            }

            public PyraminxSolverState toPyraminxSolverState()
            {
                PyraminxSolverState state = new PyraminxSolverState();

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
                int[,] stickersToEdges = new int[,] {
                { image[0,5], image[1,2] },
                { image[0,8], image[2,5] },
                { image[1,8], image[2,8] },
                { image[0,2], image[3,8] },
                { image[1,5], image[3,5] },
                { image[2,2], image[3,2] }
            };

                int[] colorToValue = new int[] { 0, 1, 2, 4 };

                int[] edges = new int[6];
                for (int i = 0; i < edges.Length; i++)
                {
                    edges[i] = colorToValue[stickersToEdges[i, 0]] + colorToValue[stickersToEdges[i, 1]] - 1;
                    // In the PyraminxSolver class, the primary facelet of each edge correspond to the lowest face number.
                    if (stickersToEdges[i, 0] > stickersToEdges[i, 1])
                    {
                        edges[i] += 8;
                    }
                }

                state.edgePerm = PyraminxSolver.packEdgePerm(edges);
                state.edgeOrient = PyraminxSolver.packEdgeOrient(edges);

                int[,] stickersToCorners = new int[,] {
                { image[0,1], image[2,4], image[3,1] },
                { image[0,7], image[1,1], image[2,7] },
                { image[0,4], image[3,7], image[1,4] },
                { image[1,7], image[3,4], image[2,1] }
            };

                /* The corners are supposed to be fixed, so we are also checking if they are in the right place.
                 * We can use the sum trick, but here, no need for transition table :) */
                int[] correctSum = new int[] { 5, 3, 4, 6 };

                int[] corners = new int[4];
                for (int i = 0; i < corners.Length; i++)
                {
                    //azzertEquals(stickersToCorners[i][0] + stickersToCorners[i][1] + stickersToCorners[i][2], correctSum[i]);
                    // The following code is not pretty, sorry...
                    if ((stickersToCorners[i, 0] < stickersToCorners[i, 1]) && (stickersToCorners[i, 0] < stickersToCorners[i, 2]))
                    {
                        corners[i] = 0;
                    }
                    if ((stickersToCorners[i, 1] < stickersToCorners[i, 0]) && (stickersToCorners[i, 1] < stickersToCorners[i, 2]))
                    {
                        corners[i] = 1;
                    }
                    if ((stickersToCorners[i, 2] < stickersToCorners[i, 1]) && (stickersToCorners[i, 2] < stickersToCorners[i, 0]))
                    {
                        corners[i] = 2;
                    }
                }

                state.cornerOrient = PyraminxSolver.packCornerOrient(corners);

                /* For the tips, we use the same numbering */
                int[,] stickersToTips = new int[,] {
                { image[0,0], image[2,3], image[3,0] },
                { image[0,6], image[1,0], image[2,6] },
                { image[0,3], image[3,6], image[1,3] },
                { image[1,6], image[3,3], image[2,0] }
            };

                int[] tips = new int[4];
                for (int i = 0; i < tips.Length; i++)
                {
                    //int[] stickers = stickersToTips[i];
                    // We can use the same color check as for the corners.
                    //azzertEquals(stickers[0] + stickers[1] + stickers[2], correctSum[i]);

                    // For the tips, we don't have to check colors against face, but against the attached corner.
                    int cornerPrimaryColor = stickersToCorners[i, 0];
                    int clockwiseTurnsToMatchCorner = 0;
                    while (stickersToTips[i, clockwiseTurnsToMatchCorner] != cornerPrimaryColor)
                    {
                        clockwiseTurnsToMatchCorner++;
                        //azzert(clockwiseTurnsToMatchCorner < 3);
                    }
                    tips[i] = clockwiseTurnsToMatchCorner;
                }

                state.tips = PyraminxSolver.packCornerOrient(tips); // Same function as for corners.

                return state;
            }

            public override String SolveIn(int n)
            {
                return puzzle.pyraminxSolver.solveIn(toPyraminxSolverState(), n, SCRAMBLE_LENGTH_INCLUDES_TIPS);
            }

            public override LinkedHashMap<String, PuzzleState> GetSuccessorsByName()
            {
                LinkedHashMap<String, PuzzleState> successors = new LinkedHashMap<String, PuzzleState>();

                String axes = "ulrb";
                for (int axis = 0; axis < axes.Length; axis++)
                {
                    foreach (bool tip in new bool[] { true, false })
                    {
                        char face = axes[axis];
                        face = tip ? char.ToLower(face) : char.ToUpper(face);
                        for (int dir = 1; dir <= 2; dir++)
                        {
                            String turn = "" + face;
                            if (dir == 2)
                            {
                                turn += "'";
                            }

                            int[,] imageCopy = new int[image.GetLength(0), image.GetLength(1)];
                            //GwtSafeUtils.deepCopy(image, imageCopy);
                            Array.Copy(image, imageCopy, image.Length);

                            if (tip)
                            {
                                turnTip(axis, dir, imageCopy);
                            }
                            else
                            {
                                this.turn(axis, dir, imageCopy);
                            }

                            successors[turn] = new PyraminxState(imageCopy, puzzle);
                        }
                    }
                }

                return successors;
            }

            public override bool Equals(Object other)
            {
                // Sure this could blow up with a cast exception, but shouldn't it? =)
                //return Arrays.deepEquals(image, ((PyraminxState)other).image);
                PyraminxState ps = (PyraminxState)other;
                for (int i = 0; i < ps.image.GetLength(0); i++)
                {
                    for (int j = 0; j < ps.image.GetLength(1); j++)
                    {
                        if (image[i, j] != ps.image[i, j]) return false;
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
