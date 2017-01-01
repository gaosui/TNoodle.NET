using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Utils;
using static TNoodle.Puzzles.Puzzle;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
    public class AlgorithmBuilder
    {
		List<string> moves = new List<string>();
		/**
         * states.get(i) = state achieved by applying moves[0]...moves[i-1]
         */
		List<PuzzleState> states = new List<PuzzleState>();
		/**
         * If we are in CANONICALIZE_MOVES MergingMode, then something like
         * Uw Dw' on a 4x4x4 will become Uw2. This means the state we end
         * up in is actually different than the state we would have ended up in
         * if we had just naively appended moves (NO_MERGING).
         * unNormalizedState keeps track of the state we would have been in
         * if we had just naively appended turns.
         */
		PuzzleState originalState, unNormalizedState;
		int totalCost;
		MergingMode mergingMode = MergingMode.NO_MERGING;
		Puzzle puzzle;


		public AlgorithmBuilder(Puzzle puzzle, MergingMode mergingMode) : this(puzzle, mergingMode, puzzle.GetSolvedState())
        {

        }

        public AlgorithmBuilder(Puzzle puzzle, MergingMode mergingMode, PuzzleState originalState)
        {
            this.puzzle = puzzle;
            this.mergingMode = mergingMode;
            ResetToState(originalState);
        }

		void ResetToState(PuzzleState state)
		{
			totalCost = 0;
			originalState = state;
			unNormalizedState = state;
			moves.Clear();
			states.Clear();
			states.Add(unNormalizedState);
		}

		public enum MergingMode
		{
			// There are several degrees of manipulation we can choose to do
			// while building an algorithm. Here they are, ranging from least to
			// most aggressive. Examples are on a 3x3x3.

			// Straightforward, blindly append moves.
			// For example:
			//  - "R R" stays unmodified.
			NO_MERGING,

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
			CANONICALIZE_MOVES
		}

		public bool IsRedundant(string move)
        {
            // TODO - add support for MERGE_REDUNDANT_MOVES_PRESERVE_STATE
            //MergingMode mergingMode = preserveState ? MergingMode.MERGE_REDUNDANT_MOVES_PRESERVE_STATE : MergingMode.CANONICALIZE_MOVES;
			MergingMode mode = MergingMode.CANONICALIZE_MOVES;
            var indexAndMove = FindBestIndexForMove(move, mode);
            return indexAndMove.Index < moves.Count || indexAndMove.Move == null;
        }

		public class IndexAndMove
		{
			public int Index { get;}
			public string Move { get; }

			public IndexAndMove(int index, string move)
			{
				Index = index;
				Move = move;
			}

			public override string ToString()
			{
				return "{ index: " + Index + " move: " + Move + " }";
			}
		}

		public IndexAndMove FindBestIndexForMove(string move, MergingMode mergingMode)
        {
            if (mergingMode == MergingMode.NO_MERGING)
            {
                return new IndexAndMove(moves.Count, move);
            }

            var newUnNormalizedState = unNormalizedState.Apply(move);
            if (newUnNormalizedState.EqualsNormalized(unNormalizedState))
            {
                // move must just be a rotation.
                if (mergingMode == MergingMode.CANONICALIZE_MOVES)
                {
                    return new IndexAndMove(0, null);
                }
            }
            var newNormalizedState = newUnNormalizedState.GetNormalized();

            var successors = getState().GetCanonicalMovesByState();
            move = null;
            // Search for the right move to do to our current state in
            // order to match up with newNormalizedState.
            foreach (PuzzleState ps in successors.Keys)
            {
                if (ps.EqualsNormalized(newNormalizedState))
                {
                    move = successors[ps];
                    break;
                }
            }
            // One of getStates()'s successors must be newNormalizedState.
            // If not, something has gone very wrong.
			Assert(move != null);

            if (mergingMode == MergingMode.CANONICALIZE_MOVES)
            {
                for (int lastMoveIndex = moves.Count - 1; lastMoveIndex >= 0; lastMoveIndex--)
                {
                    string lastMove = moves[lastMoveIndex];
					PuzzleState stateBeforeLastMove = states[lastMoveIndex];
                    if (!stateBeforeLastMove.MovesCommute(lastMove, move))
                    {
                        break;
                    }
					PuzzleState stateAfterLastMove = states[lastMoveIndex + 1];
                    var stateAfterLastMoveAndNewMove = stateAfterLastMove.Apply(move);

                    if (stateBeforeLastMove.EqualsNormalized(stateAfterLastMoveAndNewMove))
                    {
                        // move cancels with lastMove
                        return new IndexAndMove(lastMoveIndex, null);
                    }
                    else
                    {
                        successors = stateBeforeLastMove.GetCanonicalMovesByState();
                        foreach (PuzzleState ps in successors.Keys)
                        {
                            if (ps.EqualsNormalized(stateAfterLastMoveAndNewMove))
                            {
                                string alternateLastMove = successors[ps];
                                // move merges with lastMove
                                return new IndexAndMove(lastMoveIndex, alternateLastMove);
                            }
                        }
                    }
                }
            }
            return new IndexAndMove(moves.Count, move);
        }

        public void AppendMove(string newMove)
        {
            //l.fine("appendMove(" + newMove + ")");
            var indexAndMove = FindBestIndexForMove(newMove, mergingMode);
            int oldCostMove, newCostMove;
            if (indexAndMove.Index < moves.Count)
            {
                // This move is redundant.
                //azzert(mergingMode != MergingMode.NO_MERGING);
                oldCostMove = states[indexAndMove.Index].getMoveCost(moves[indexAndMove.Index]);
                if (indexAndMove.Move == null)
                {
                    // newMove cancelled perfectly with the move at
                    // indexAndMove.index.
                    moves.RemoveAt(indexAndMove.Index);
                    states.RemoveAt(indexAndMove.Index + 1);
                    newCostMove = 0;
                }
                else
                {
                    // newMove merged with the move at indexAndMove.index.
                    moves[indexAndMove.Index] = indexAndMove.Move;
                    newCostMove = states[indexAndMove.Index].getMoveCost(indexAndMove.Move);
                }
            }
            else
            {
                oldCostMove = 0;
                newCostMove = states[states.Count - 1].getMoveCost(indexAndMove.Move);
                // This move is not redundant.
                moves.Add(indexAndMove.Move);
                // The code to update the states array is right below us,
                // but it requires that the states array be of the correct
                // size.
                states.Add(null);
            }

            totalCost += newCostMove - oldCostMove;

            // We modified moves[ indexAndMove.index ], so everything in
            // states[ indexAndMove.index+1, ... ] is now invalid
            for (int i = indexAndMove.Index + 1; i < states.Count; i++)
            {
                states[i] = states[i - 1].Apply(moves[i - 1]);
            }

            unNormalizedState = unNormalizedState.Apply(newMove);
            //azzert(states.Count == moves.Count + 1);
            //azzert(unNormalizedState.equalsNormalized(getState()));
        }

        public string popMove(int index)
        {
            var movesCopy = new List<string>(moves);
            string poppedMove = movesCopy[index];
            movesCopy.RemoveAt(index);

            ResetToState(originalState);
            foreach (string move in movesCopy)
            {
                try
                {
                    AppendMove(move);
                }
                catch //(InvalidMoveException e)
                {
                    //azzert(false, e);
                }
            }
            return poppedMove;
        }

        public void appendAlgorithm(string algorithm)
        {
            foreach (string move in splitAlgorithm(algorithm))
            {
                AppendMove(move);
            }
        }

        public void appendAlgorithms(string[] algorithms)
        {
            foreach (string algorithm in algorithms)
            {
                appendAlgorithm(algorithm);
            }
        }

        public Puzzle.PuzzleState getState()
        {
            //azzert(states.Count == moves.Count + 1);
            return states[states.Count - 1];
        }

        public int getTotalCost()
        {
            return totalCost;
        }

        public override string ToString()
        {
            return Functions.Join(moves, " ");
        }

        public PuzzleStateAndGenerator getStateAndGenerator()
        {
            return new PuzzleStateAndGenerator(getState(), ToString());
        }

        public static string[] splitAlgorithm(string algorithm)
        {
            if (algorithm.Trim().Length == 0)
            {
                return new string[0];
            }

            return algorithm.Split(new char[] { ' ', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
    }
}
