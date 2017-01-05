using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Utils;

namespace TNoodle.Puzzles
{
    public class ClockPuzzle : Puzzle
    {
        //private static final Logger l = Logger.getLogger(ClockPuzzle.class.getName());

        private static readonly string[] turns = { "UR", "DR", "DL", "UL", "U", "R", "D", "L", "ALL" };
        private const int STROKE_WIDTH = 2;
        private const int radius = 70;
        private const int clockRadius = 14;
        private const int clockOuterRadius = 20;
        private const int pointRadius = (clockRadius + clockOuterRadius) / 2;
        private const int tickMarkRadius = 1;
        private const int arrowHeight = 10;
        private const int arrowRadius = 2;
        private const int pinRadius = 4;
        private static readonly double arrowAngle = Math.PI / 2 - Math.Acos((double)arrowRadius / (double)arrowHeight);

        private const int gap = 5;

        public override String GetLongName()
        {
            return "Clock";
        }

        public override String GetShortName()
        {
            return "clock";
        }

        private static readonly int[,] moves = {
        {0,1,1,0,1,1,0,0,0,  -1, 0, 0, 0, 0, 0, 0, 0, 0},// UR
        {0,0,0,0,1,1,0,1,1,   0, 0, 0, 0, 0, 0,-1, 0, 0},// DR
        {0,0,0,1,1,0,1,1,0,   0, 0, 0, 0, 0, 0, 0, 0,-1},// DL
        {1,1,0,1,1,0,0,0,0,   0, 0,-1, 0, 0, 0, 0, 0, 0},// UL
        {1,1,1,1,1,1,0,0,0,  -1, 0,-1, 0, 0, 0, 0, 0, 0},// U
        {0,1,1,0,1,1,0,1,1,  -1, 0, 0, 0, 0, 0,-1, 0, 0},// R
        {0,0,0,1,1,1,1,1,1,   0, 0, 0, 0, 0, 0,-1, 0,-1},// D
        {1,1,0,1,1,0,1,1,0,   0, 0,-1, 0, 0, 0, 0, 0,-1},// L
        {1,1,1,1,1,1,1,1,1,  -1, 0,-1, 0, 0, 0,-1, 0,-1},// A
    };



        public override PuzzleState GetSolvedState()
        {
            return new ClockState(this);
        }

        protected  override int GetRandomMoveCount()
        {
            return 19;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            StringBuilder scramble = new StringBuilder();

            for (int x = 0; x < 9; x++)
            {
                int turn = r.Next(12) - 5;
                bool clockwise = (turn >= 0);
                turn = Math.Abs(turn);
                scramble.Append(turns[x] + turn + (clockwise ? "+" : "-") + " ");
            }
            scramble.Append("y2 ");
            for (int x = 4; x < 9; x++)
            {
                int turn = r.Next(12) - 5;
                bool clockwise = (turn >= 0);
                turn = Math.Abs(turn);
                scramble.Append(turns[x] + turn + (clockwise ? "+" : "-") + " ");
            }

            bool isFirst = true;
            for (int x = 0; x < 4; x++)
            {
                if (r.Next(2) == 1)
                {
                    scramble.Append((isFirst ? "" : " ") + turns[x]);
                    isFirst = false;
                }
            }

            String scrambleStr = scramble.ToString().Trim();

            PuzzleState state = GetSolvedState();
            try
            {
                state = state.ApplyAlgorithm(scrambleStr);
            }
            catch //(InvalidScrambleException e)
            {
                //azzert(false, e);
                return null;
            }
            return new PuzzleStateAndGenerator(state, scrambleStr);
        }

        public class ClockState : PuzzleState
        {
            private ClockPuzzle puzzle;
            private bool[] pins;
            private readonly int[] posit;
            private bool rightSideUp;
            public ClockState(ClockPuzzle p) : base(p)
            {
                puzzle = p;
                pins = new bool[] { false, false, false, false };
                posit = new int[] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                rightSideUp = true;
            }

            public ClockState(bool[] pins, int[] posit, bool rightSideUp, ClockPuzzle p) : base(p)
            {
                puzzle = p;
                this.pins = pins;
                this.posit = posit;
                this.rightSideUp = rightSideUp;
            }

            public override LinkedHashMap<String, PuzzleState> GetSuccessorsByName()
            {
                LinkedHashMap<String, PuzzleState> successors = new LinkedHashMap<String, PuzzleState>();

                for (int turn = 0; turn < turns.Length; turn++)
                {
                    for (int rot = 0; rot < 12; rot++)
                    {
                        // Apply the move
                        int[] positCopy1 = new int[18];
                        bool[] pinsCopy1 = new bool[4];
                        for (int p = 0; p < 18; p++)
                        {
                            positCopy1[p] = (posit[p] + rot * moves[turn, p] + 12) % 12;
                        }
                        Array.Copy(pins, 0, pinsCopy1, 0, 4);

                        // Build the move string
                        bool clockwise = (rot < 7);
                        String move = turns[turn] + (clockwise ? (rot + "+") : ((12 - rot) + "-"));

                        successors[move] = new ClockState(pinsCopy1, positCopy1, rightSideUp, puzzle);
                    }
                }

                // Still y2 to implement
                int[] positCopy = new int[18];
                bool[] pinsCopy = new bool[4];
                Array.Copy(posit, 0, positCopy, 9, 9);
                Array.Copy(posit, 9, positCopy, 0, 9);
                Array.Copy(pins, 0, pinsCopy, 0, 4);
                successors["y2"] = new ClockState(pinsCopy, positCopy, !rightSideUp, puzzle);

                // Pins position moves
                for (int pin = 0; pin < 4; pin++)
                {
                    int[] positC = new int[18];
                    bool[] pinsC = new bool[4];
                    Array.Copy(posit, 0, positC, 0, 18);
                    Array.Copy(pins, 0, pinsC, 0, 4);
                    int pinI = (pin == 0 ? 1 : (pin == 1 ? 3 : (pin == 2 ? 2 : 0)));
                    pinsC[pinI] = true;

                    successors[turns[pin]] = new ClockState(pinsC, positC, rightSideUp, puzzle);
                }

                return successors;
            }

            public override bool Equals(Object other)
            {
                ClockState o = ((ClockState)other);
                return Functions.DeepEquals(posit, o.posit);
            }

            public override int GetHashCode()
            {
                return posit.DeepHashCode();
            }

        }
    }

}
