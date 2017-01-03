using System;
using System.Collections.Generic;
using System.Linq;
using TNoodle.Utils;
using static TNoodle.Puzzles.Puzzle;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
    public class AlgorithmBuilder
    {
        public enum MergingMode
        {
            // There are several degrees of manipulation we can choose to do
            // while building an algorithm. Here they are, ranging from least to
            // most aggressive. Examples are on a 3x3x3.

            // Straightforward, blindly append moves.
            // For example:
            //  - "R R" stays unmodified.
            NoMerging,

            // Merge together redundant moves, but preserve the exact state
            // of the puzzle (unlike CANONICALIZE_MOVES).
            // In other words, the resulting state will be the
            // same as if we had used NO_MERGING.
            // For example:
            //  - "R R" becomes "R2"
            //  - "L Rw" stays unmodified.
            //  - "F x U" will become something like "F2 x".
            //  TODO - add actual support for this! feel free to rename it
            //MERGE_REDUNDANT_MOVES_PRESERVE_STATE,

            // Most aggressive merging.
            // See PuzzleState.getCanonicalMovesByState() for the
            // definition of "canonical" moves.
            // Canonical moves will not necessarily let us preserve the
            // exact state we would have achieved with NO_MERGING. This is
            // because canonical moves may not let us rotate the puzzle.
            // However, the resulting state when normalized will be the
            // same as the normalization of the state we would have
            // achieved if we had used NO_MERGING.
            // For example:
            //  - "R R" becomes "R2"
            //  - "L Rw" becomes "L2"
            //  - "F x U" becomes "F2"
            CanonicalizeMoves
        }

        private readonly MergingMode _mergingMode;
        private readonly List<string> _moves = new List<string>();
        /**
         * states.get(i) = state achieved by applying moves[0]...moves[i-1]
         */
        private readonly List<PuzzleState> _states = new List<PuzzleState>();
        /**
         * If we are in CANONICALIZE_MOVES MergingMode, then something like
         * Uw Dw' on a 4x4x4 will become Uw2. This means the state we end
         * up in is actually different than the state we would have ended up in
         * if we had just naively appended moves (NO_MERGING).
         * unNormalizedState keeps track of the state we would have been in
         * if we had just naively appended turns.
         */
        private PuzzleState _originalState, _unNormalizedState;
        private int _totalCost;

        public AlgorithmBuilder(MergingMode mergingMode, PuzzleState originalState)
        {
            _mergingMode = mergingMode;
            ResetToState(originalState);
        }

        private void ResetToState(PuzzleState state)
        {
            _totalCost = 0;
            _originalState = state;
            _unNormalizedState = state;
            _moves.Clear();
            _states.Clear();
            _states.Add(_unNormalizedState);
        }

        public bool IsRedundant(string move)
        {
            // TODO - add support for MERGE_REDUNDANT_MOVES_PRESERVE_STATE
            //MergingMode mergingMode = preserveState ? MergingMode.MERGE_REDUNDANT_MOVES_PRESERVE_STATE : MergingMode.CANONICALIZE_MOVES;
            const MergingMode mode = MergingMode.CanonicalizeMoves;
            var indexAndMove = FindBestIndexForMove(move, mode);
            return indexAndMove.Index < _moves.Count || indexAndMove.Move == null;
        }

        public IndexAndMove FindBestIndexForMove(string move, MergingMode mergingMode)
        {
            if (mergingMode == MergingMode.NoMerging)
                return new IndexAndMove(_moves.Count, move);

            var newUnNormalizedState = _unNormalizedState.Apply(move);
            if (newUnNormalizedState.EqualsNormalized(_unNormalizedState))
                if (mergingMode == MergingMode.CanonicalizeMoves)
                    return new IndexAndMove(0, null);
            var newNormalizedState = newUnNormalizedState.GetNormalized();

            var successors = GetState().GetCanonicalMovesByState();
            // Search for the right move to do to our current state in
            // order to match up with newNormalizedState.
            move = (from ps in successors.Keys
                where ps.EqualsNormalized(newNormalizedState)
                select successors[ps]).FirstOrDefault();
            // One of getStates()'s successors must be newNormalizedState.
            // If not, something has gone very wrong.
            Assert(move != null);

            if (mergingMode != MergingMode.CanonicalizeMoves) return new IndexAndMove(_moves.Count, move);
            for (var lastMoveIndex = _moves.Count - 1; lastMoveIndex >= 0; lastMoveIndex--)
            {
                var lastMove = _moves[lastMoveIndex];
                var stateBeforeLastMove = _states[lastMoveIndex];
                if (!stateBeforeLastMove.MovesCommute(lastMove, move))
                    break;
                var stateAfterLastMove = _states[lastMoveIndex + 1];
                var stateAfterLastMoveAndNewMove = stateAfterLastMove.Apply(move);

                if (stateBeforeLastMove.EqualsNormalized(stateAfterLastMoveAndNewMove))
                    return new IndexAndMove(lastMoveIndex, null);
                successors = stateBeforeLastMove.GetCanonicalMovesByState();
                foreach (var ps in successors.Keys)
                {
                    if (!ps.EqualsNormalized(stateAfterLastMoveAndNewMove)) continue;
                    var alternateLastMove = successors[ps];
                    // move merges with lastMove
                    return new IndexAndMove(lastMoveIndex, alternateLastMove);
                }
            }
            return new IndexAndMove(_moves.Count, move);
        }

        public void AppendMove(string newMove)
        {
            //l.fine("appendMove(" + newMove + ")");
            var indexAndMove = FindBestIndexForMove(newMove, _mergingMode);
            int oldCostMove, newCostMove;
            if (indexAndMove.Index < _moves.Count)
            {
                // This move is redundant.
                Assert(_mergingMode != MergingMode.NoMerging);
                oldCostMove = _states[indexAndMove.Index].GetMoveCost(_moves[indexAndMove.Index]);
                if (indexAndMove.Move == null)
                {
                    // newMove cancelled perfectly with the move at
                    // indexAndMove.index.
                    _moves.RemoveAt(indexAndMove.Index);
                    _states.RemoveAt(indexAndMove.Index + 1);
                    newCostMove = 0;
                }
                else
                {
                    // newMove merged with the move at indexAndMove.index.
                    _moves[indexAndMove.Index] = indexAndMove.Move;
                    newCostMove = _states[indexAndMove.Index].GetMoveCost(indexAndMove.Move);
                }
            }
            else
            {
                oldCostMove = 0;
                newCostMove = _states[_states.Count - 1].GetMoveCost(indexAndMove.Move);
                // This move is not redundant.
                _moves.Add(indexAndMove.Move);
                // The code to update the states array is right below us,
                // but it requires that the states array be of the correct
                // size.
                _states.Add(null);
            }

            _totalCost += newCostMove - oldCostMove;

            // We modified moves[ indexAndMove.index ], so everything in
            // states[ indexAndMove.index+1, ... ] is now invalid
            for (var i = indexAndMove.Index + 1; i < _states.Count; i++)
                _states[i] = _states[i - 1].Apply(_moves[i - 1]);

            _unNormalizedState = _unNormalizedState.Apply(newMove);
            Assert(_states.Count == _moves.Count + 1);
            Assert(_unNormalizedState.EqualsNormalized(GetState()));
        }

        public string PopMove(int index)
        {
            var movesCopy = new List<string>(_moves);
            var poppedMove = movesCopy[index];
            movesCopy.RemoveAt(index);

            ResetToState(_originalState);
            foreach (var move in movesCopy)
                try
                {
                    AppendMove(move);
                }
                catch (InvalidMoveException e)
                {
                    Assert(false, e.Message, e);
                }
            return poppedMove;
        }

        public void AppendAlgorithm(string algorithm)
        {
            foreach (var move in SplitAlgorithm(algorithm))
                AppendMove(move);
        }

        public void AppendAlgorithms(string[] algorithms)
        {
            foreach (var algorithm in algorithms)
                AppendAlgorithm(algorithm);
        }

        public PuzzleState GetState()
        {
            Assert(_states.Count == _moves.Count + 1);
            return _states[_states.Count - 1];
        }

        public int GetTotalCost()
        {
            return _totalCost;
        }

        public override string ToString()
        {
            return Functions.Join(_moves, " ");
        }

        public PuzzleStateAndGenerator GetStateAndGenerator()
        {
            return new PuzzleStateAndGenerator(GetState(), ToString());
        }

        public static string[] SplitAlgorithm(string algorithm)
        {
            return algorithm.Trim().Length == 0
                ? new string[0]
                : algorithm.Split(new[] {' ', '\n'}, StringSplitOptions.RemoveEmptyEntries);
        }

        public class IndexAndMove
        {
            public IndexAndMove(int index, string move)
            {
                Index = index;
                Move = move;
            }

            public int Index { get; }
            public string Move { get; }

            public override string ToString()
            {
                return "{ index: " + Index + " move: " + Move + " }";
            }
        }
    }
}