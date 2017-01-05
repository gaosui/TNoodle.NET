using System;
using System.Collections.Generic;
using System.Linq;
using TNoodle.Solvers.sq12phase;
using TNoodle.Utils;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
    public class SquareOnePuzzle : Puzzle
    {
        private static readonly Dictionary<string, int> CostsByMove = new Dictionary<string, int>();

        static SquareOnePuzzle()
        {
            for (var top = -5; top <= 6; top++)
            for (var bottom = -5; bottom <= 6; bottom++)
            {
                if (top == 0 && bottom == 0)
                    continue;
                //int topCost = top % 12 == 0 ? 0 : 1;
                //int bottomCost = bottom % 12 == 0 ? 0 : 1;
                //int cost = topCost + bottomCost;
                const int cost = 1;
                var turn = "(" + top + "," + bottom + ")";
                CostsByMove[turn] = cost;
            }
            CostsByMove["/"] = 1;
        }

        public SquareOnePuzzle()
        {
            // TODO - we can't filter super aggresively until
            // Chen Shuang's optimal solver is fixed.
            //wcaMinScrambleDistance = 20;
            WcaMinScrambleDistance = 11;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            var s = new Search();
            var scramble = s.Solution(FullCube.RandomCube(r)).Trim();
            PuzzleState state;
            try
            {
                state = GetSolvedState().ApplyAlgorithm(scramble);
            }
            catch (InvalidScrambleException e)
            {
                Assert(false, e.Message, e);
                return null;
            }
            return new PuzzleStateAndGenerator(state, scramble);
        }


        public override string GetLongName()
        {
            return "Square-1";
        }

        public override string GetShortName()
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

        public class SquareOneState : PuzzleState
        {
            private readonly int[] _pieces;
            private readonly SquareOnePuzzle _puzzle;
            private readonly bool _sliceSolved;

            public SquareOneState(SquareOnePuzzle p) : base(p)
            {
                _puzzle = p;
                _sliceSolved = true;
                _pieces = new[] {0, 0, 1, 2, 2, 3, 4, 4, 5, 6, 6, 7, 8, 9, 9, 10, 11, 11, 12, 13, 13, 14, 15, 15};
                //piece array
            }

            public SquareOneState(bool sliceSolved, int[] pieces, SquareOnePuzzle p) : base(p)
            {
                _puzzle = p;
                _sliceSolved = sliceSolved;
                _pieces = pieces;
            }

            private int[] DoSlash()
            {
                //int[] newPieces = GwtSafeUtils.clone(pieces);
                var newPieces = new int[_pieces.Length];
                _pieces.CopyTo(newPieces, 0);
                for (var i = 0; i < 6; i++)
                {
                    var c = newPieces[i + 12];
                    newPieces[i + 12] = newPieces[i + 6];
                    newPieces[i + 6] = c;
                }
                return newPieces;
            }

            private bool CanSlash()
            {
                if (_pieces[0] == _pieces[11])
                    return false;
                if (_pieces[6] == _pieces[5])
                    return false;
                if (_pieces[12] == _pieces[23])
                    return false;
                return _pieces[12 + 6] != _pieces[12 + 6 - 1];
            }

            /**
             *
             * @param top Amount to rotate top
             * @param bottom Amount to rotate bottom
             * @return A copy of pieces with (top, bottom) applied to it
             */

            private int[] DoRotateTopAndBottom(int top, int bottom)
            {
                top = Functions.Modulo(-top, 12);
                //int[] newPieces = GwtSafeUtils.clone(pieces);
                var newPieces = new int[_pieces.Length];
                _pieces.CopyTo(newPieces, 0);
                var t = new int[12];
                for (var i = 0; i < 12; i++)
                    t[i] = newPieces[i];
                for (var i = 0; i < 12; i++)
                    newPieces[i] = t[(top + i) % 12];

                bottom = Functions.Modulo(-bottom, 12);

                for (var i = 0; i < 12; i++)
                    t[i] = newPieces[i + 12];
                for (var i = 0; i < 12; i++)
                    newPieces[i + 12] = t[(bottom + i) % 12];

                return newPieces;
            }

            public override int GetMoveCost(string move)
            {
                // TODO - We do a lookup here rather than string parsing because
                // this is a very performance critical section of code.
                // I believe the best thing to do would be to change the puzzle
                // api to return move costs as part of the object returned by
                // getScrambleSuccessors(), then subclasses wouldn't have to do
                // weird stuff like this for speed.
                return CostsByMove[move];
            }

            public override LinkedHashMap<string, PuzzleState> GetScrambleSuccessors()
            {
                var successors = GetSuccessorsByName();
                foreach (var key in successors.Keys)
                {
                    //String key = iter.next();
                    var state = (SquareOneState) successors[key];
                    if (!state.CanSlash())
                        successors.Remove(key);
                }
                return successors;
            }

            public override LinkedHashMap<string, PuzzleState> GetSuccessorsByName()
            {
                var successors = new LinkedHashMap<string, PuzzleState>();
                for (var top = -5; top <= 6; top++)
                for (var bottom = -5; bottom <= 6; bottom++)
                {
                    if (top == 0 && bottom == 0)
                        continue;
                    var newPieces = DoRotateTopAndBottom(top, bottom);
                    var turn = "(" + top + "," + bottom + ")";
                    successors[turn] = new SquareOneState(_sliceSolved, newPieces, _puzzle);
                }
                if (CanSlash())
                    successors["/"] = new SquareOneState(!_sliceSolved, DoSlash(), _puzzle);
                return successors;
            }

            public override bool Equals(object other)
            {
                var o = (SquareOneState) other;
                return _pieces.SequenceEqual(o._pieces) && _sliceSolved == o._sliceSolved;
            }

            public override int GetHashCode()
            {
                return _pieces.DeepHashCode() ^ (_sliceSolved ? 1 : 0);
            }

            public override string ToString()
            {
                return "sliceSolved: " + _sliceSolved + " " + _pieces;
            }
        }
    }
}