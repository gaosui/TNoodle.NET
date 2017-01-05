using System;
using System.Text;
using TNoodle.Utils;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
    public class ClockPuzzle : Puzzle
    {
        //private static final Logger l = Logger.getLogger(ClockPuzzle.class.getName());

        private static readonly string[] Turns = {"UR", "DR", "DL", "UL", "U", "R", "D", "L", "ALL"};

        public override string GetLongName()
        {
            return "Clock";
        }

        public override string GetShortName()
        {
            return "clock";
        }

        private static readonly int[][] Moves =
        {
            new[] {0, 1, 1, 0, 1, 1, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0, 0, 0}, // UR
            new[] {0, 0, 0, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, -1, 0, 0}, // DR
            new[] {0, 0, 0, 1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, -1}, // DL
            new[] {1, 1, 0, 1, 1, 0, 0, 0, 0, 0, 0, -1, 0, 0, 0, 0, 0, 0}, // UL
            new[] {1, 1, 1, 1, 1, 1, 0, 0, 0, -1, 0, -1, 0, 0, 0, 0, 0, 0}, // U
            new[] {0, 1, 1, 0, 1, 1, 0, 1, 1, -1, 0, 0, 0, 0, 0, -1, 0, 0}, // R
            new[] {0, 0, 0, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, -1, 0, -1}, // D
            new[] {1, 1, 0, 1, 1, 0, 1, 1, 0, 0, 0, -1, 0, 0, 0, 0, 0, -1}, // L
            new[] {1, 1, 1, 1, 1, 1, 1, 1, 1, -1, 0, -1, 0, 0, 0, -1, 0, -1}, // A
        };


        public override PuzzleState GetSolvedState()
        {
            return new ClockState(this);
        }

        protected override int GetRandomMoveCount()
        {
            return 19;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            var scramble = new StringBuilder();

            for (var x = 0; x < 9; x++)
            {
                var turn = r.Next(12) - 5;
                var clockwise = (turn >= 0);
                turn = Math.Abs(turn);
                scramble.Append(Turns[x] + turn + (clockwise ? "+" : "-") + " ");
            }
            scramble.Append("y2 ");
            for (var x = 4; x < 9; x++)
            {
                var turn = r.Next(12) - 5;
                var clockwise = (turn >= 0);
                turn = Math.Abs(turn);
                scramble.Append(Turns[x] + turn + (clockwise ? "+" : "-") + " ");
            }

            var isFirst = true;
            for (var x = 0; x < 4; x++)
            {
                if (r.Next(2) == 1)
                {
                    scramble.Append((isFirst ? "" : " ") + Turns[x]);
                    isFirst = false;
                }
            }

            var scrambleStr = scramble.ToString().Trim();

            var state = GetSolvedState();
            try
            {
                state = state.ApplyAlgorithm(scrambleStr);
            }
            catch (InvalidScrambleException e)
            {
                Assert(false, e.Message, e);
                return null;
            }
            return new PuzzleStateAndGenerator(state, scrambleStr);
        }

        public class ClockState : PuzzleState
        {
            private readonly ClockPuzzle _puzzle;
            private readonly bool[] _pins;
            private readonly int[] _posit;
            private readonly bool _rightSideUp;

            public ClockState(ClockPuzzle p) : base(p)
            {
                _puzzle = p;
                _pins = new[] {false, false, false, false};
                _posit = new[] {0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0};
                _rightSideUp = true;
            }

            public ClockState(bool[] pins, int[] posit, bool rightSideUp, ClockPuzzle p) : base(p)
            {
                _puzzle = p;
                _pins = pins;
                _posit = posit;
                _rightSideUp = rightSideUp;
            }

            public override LinkedHashMap<string, PuzzleState> GetSuccessorsByName()
            {
                var successors = new LinkedHashMap<string, PuzzleState>();

                for (var turn = 0; turn < Turns.Length; turn++)
                {
                    for (var rot = 0; rot < 12; rot++)
                    {
                        // Apply the move
                        var positCopy1 = new int[18];
                        var pinsCopy1 = new bool[4];
                        for (var p = 0; p < 18; p++)
                        {
                            positCopy1[p] = (_posit[p] + rot * Moves[turn][p] + 12) % 12;
                        }
                        Array.Copy(_pins, 0, pinsCopy1, 0, 4);

                        // Build the move string
                        var clockwise = (rot < 7);
                        var move = Turns[turn] + (clockwise ? (rot + "+") : ((12 - rot) + "-"));

                        successors[move] = new ClockState(pinsCopy1, positCopy1, _rightSideUp, _puzzle);
                    }
                }

                // Still y2 to implement
                var positCopy = new int[18];
                var pinsCopy = new bool[4];
                Array.Copy(_posit, 0, positCopy, 9, 9);
                Array.Copy(_posit, 9, positCopy, 0, 9);
                Array.Copy(_pins, 0, pinsCopy, 0, 4);
                successors["y2"] = new ClockState(pinsCopy, positCopy, !_rightSideUp, _puzzle);

                // Pins position moves
                for (var pin = 0; pin < 4; pin++)
                {
                    var positC = new int[18];
                    var pinsC = new bool[4];
                    Array.Copy(_posit, 0, positC, 0, 18);
                    Array.Copy(_pins, 0, pinsC, 0, 4);
                    var pinI = (pin == 0 ? 1 : (pin == 1 ? 3 : (pin == 2 ? 2 : 0)));
                    pinsC[pinI] = true;

                    successors[Turns[pin]] = new ClockState(pinsC, positC, _rightSideUp, _puzzle);
                }

                return successors;
            }

            public override bool Equals(object other)
            {
                var o = ((ClockState) other);
                return _posit.DeepEquals(o._posit);
            }

            public override int GetHashCode()
            {
                return _posit.DeepHashCode();
            }
        }
    }
}