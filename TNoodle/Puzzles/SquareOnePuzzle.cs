using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Solvers.sq12phase;
using TNoodle.Utils;

namespace TNoodle.Puzzles
{
    public class SquareOnePuzzle : Puzzle
    {

        private const int radius = 32;

        public SquareOnePuzzle()
        {
            // TODO - we can't filter super aggresively until
            // Chen Shuang's optimal solver is fixed.
            //wcaMinScrambleDistance = 20;
            WcaMinScrambleDistance = 11;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            Search s = new Search();
            String scramble = s.solution(FullCube.randomCube(r)).Trim();
            PuzzleState state;
            try
            {
                state = GetSolvedState().ApplyAlgorithm(scramble);
            }
            catch //(InvalidScrambleException e)
            {
                //azzert(false, e);
                return null;
            }
            return new PuzzleStateAndGenerator(state, scramble);
        }


        public override String GetLongName()
        {
            return "Square-1";
        }

        public override String GetShortName()
        {
            return "sq1";
        }

        public override PuzzleState GetSolvedState()
        {
            return new SquareOneState(this);
        }

        protected override int GetRandomMoveCount()
        {
            return 40;
        }

        /*
        // TODO - we can't filter super aggresively until
        // Chen Shuang's optimal solver is fixed.
        @Override
        protected String solveIn(PuzzleState ps, int n) {
            FullCube f = ((SquareOneState)ps).toFullCube();
            Search s = new Search();
            String scramble = s.solutionOpt(f, n);
            return scramble == null ? null : scramble.trim();
        }
        */

        internal static Dictionary<String, int> costsByMove = new Dictionary<string, int>();
        static SquareOnePuzzle()
        {
            for (int top = -5; top <= 6; top++)
            {
                for (int bottom = -5; bottom <= 6; bottom++)
                {
                    if (top == 0 && bottom == 0)
                    {
                        // No use doing nothing =)
                        continue;
                    }
                    //int topCost = top % 12 == 0 ? 0 : 1;
                    //int bottomCost = bottom % 12 == 0 ? 0 : 1;
                    //int cost = topCost + bottomCost;
                    int cost = 1;
                    String turn = "(" + top + "," + bottom + ")";
                    costsByMove[turn] = cost;
                }
            }
            costsByMove["/"] = 1;
        }

        public class SquareOneState : PuzzleState
        {
            private SquareOnePuzzle puzzle;
            internal bool sliceSolved;
            internal int[] pieces;

            public SquareOneState(SquareOnePuzzle p) : base(p)
            {
                puzzle = p;
                sliceSolved = true;
                pieces = new int[] { 0, 0, 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 9, 9, 10, 11, 11, 12, 13, 13, 14, 15, 15 }; //piece array
            }

            public SquareOneState(bool sliceSolved, int[] pieces, SquareOnePuzzle p) : base(p)
            {
                puzzle = p;
                this.sliceSolved = sliceSolved;
                this.pieces = pieces;
            }

            FullCube toFullCube()
            {
                int[] map1 = new int[] { 3, 2, 1, 0, 7, 6, 5, 4, 0xa, 0xb, 8, 9, 0xe, 0xf, 0xc, 0xd };
                int[] map2 = new int[] { 5, 4, 3, 2, 1, 0, 11, 10, 9, 8, 7, 6, 17, 16, 15, 14, 13, 12, 23, 22, 21, 20, 19, 18 };
                FullCube f = FullCube.randomCube();
                for (int i = 0; i < 24; i++)
                {
                    f.setPiece(map2[i], map1[pieces[i]]);
                }
                f.setPiece(24, sliceSolved ? 0 : 1);
                return f;
            }

            private int[] doSlash()
            {
                //int[] newPieces = GwtSafeUtils.clone(pieces);
                int[] newPieces = new int[pieces.Length];
                pieces.CopyTo(newPieces, 0);
                for (int i = 0; i < 6; i++)
                {
                    int c = newPieces[i + 12];
                    newPieces[i + 12] = newPieces[i + 6];
                    newPieces[i + 6] = c;
                }
                return newPieces;
            }

            private bool canSlash()
            {
                if (pieces[0] == pieces[11])
                {
                    return false;
                }
                if (pieces[6] == pieces[5])
                {
                    return false;
                }
                if (pieces[12] == pieces[23])
                {
                    return false;
                }
                if (pieces[12 + 6] == pieces[(12 + 6) - 1])
                {
                    return false;
                }
                return true;
            }

            /**
             *
             * @param top Amount to rotate top
             * @param bottom Amount to rotate bottom
             * @return A copy of pieces with (top, bottom) applied to it
             */
            private int[] doRotateTopAndBottom(int top, int bottom)
            {
                top = Functions.modulo(-top, 12);
                //int[] newPieces = GwtSafeUtils.clone(pieces);
                int[] newPieces = new int[pieces.Length];
                pieces.CopyTo(newPieces, 0);
                int[] t = new int[12];
                for (int i = 0; i < 12; i++)
                {
                    t[i] = newPieces[i];
                }
                for (int i = 0; i < 12; i++)
                {
                    newPieces[i] = t[(top + i) % 12];
                }

                bottom = Functions.modulo(-bottom, 12);

                for (int i = 0; i < 12; i++)
                {
                    t[i] = newPieces[i + 12];
                }
                for (int i = 0; i < 12; i++)
                {
                    newPieces[i + 12] = t[(bottom + i) % 12];
                }

                return newPieces;
            }

            public override int getMoveCost(String move)
            {
                // TODO - We do a lookup here rather than string parsing because
                // this is a very performance critical section of code.
                // I believe the best thing to do would be to change the puzzle
                // api to return move costs as part of the object returned by
                // getScrambleSuccessors(), then subclasses wouldn't have to do
                // weird stuff like this for speed.
                return costsByMove[move];
            }

            public override LinkedHashMap<string, PuzzleState> GetScrambleSuccessors()
            {
                LinkedHashMap<String, PuzzleState> successors = GetSuccessorsByName();
                var iter = successors.Keys.GetEnumerator();
                foreach (var key in successors.Keys)
                {
                    //String key = iter.next();
                    SquareOneState state = (SquareOneState)successors[key];
                    if (!state.canSlash())
                    {
                        //iter.remove();
                        successors.Remove(key);
                    }
                }
                return successors;
            }

            public override LinkedHashMap<string, PuzzleState> GetSuccessorsByName()
            {
                LinkedHashMap<String, PuzzleState> successors = new LinkedHashMap<String, PuzzleState>();
                for (int top = -5; top <= 6; top++)
                {
                    for (int bottom = -5; bottom <= 6; bottom++)
                    {
                        if (top == 0 && bottom == 0)
                        {
                            // No use doing nothing =)
                            continue;
                        }
                        int[] newPieces = doRotateTopAndBottom(top, bottom);
                        String turn = "(" + top + "," + bottom + ")";
                        successors[turn] = new SquareOneState(sliceSolved, newPieces, puzzle);
                    }
                }
                if (canSlash())
                {
                    successors["/"] = new SquareOneState(!sliceSolved, doSlash(), puzzle);
                }
                return successors;
            }

            public override bool Equals(Object other)
            {
                SquareOneState o = ((SquareOneState)other);
                return pieces.SequenceEqual(o.pieces) && sliceSolved == o.sliceSolved;
            }

            public override int GetHashCode()
            {
                return pieces.DeepHashCode() ^ (sliceSolved ? 1 : 0);
            }

            public override string ToString()
            {
                return "sliceSolved: " + sliceSolved + " " + pieces.ToString();
            }

        }
    }

}
