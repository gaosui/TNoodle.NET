using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Puzzles
{
    public class AlgorithmBuilder
    {
        private List<string> moves = new List<string>();
        /**
         * states.get(i) = state achieved by applying moves[0]...moves[i-1]
         */
        private List<Puzzle.PuzzleState> states = new List<Puzzle.PuzzleState>();
        /**
         * If we are in CANONICALIZE_MOVES MergingMode, then something like
         * Uw Dw' on a 4x4x4 will become Uw2. This means the state we end
         * up in is actually different than the state we would have ended up in
         * if we had just naively appended moves (NO_MERGING).
         * unNormalizedState keeps track of the state we would have been in
         * if we had just naively appended turns.
         */
        private Puzzle.PuzzleState originalState, unNormalizedState;
        private int totalCost;
        private MergingMode mergingMode = MergingMode.NO_MERGING;
        private Puzzle puzzle;


        public AlgorithmBuilder(Puzzle puzzle, MergingMode mergingMode) : this(puzzle, mergingMode, puzzle.GetSolvedState())
        {

        }

        public AlgorithmBuilder(Puzzle puzzle, MergingMode mergingMode, Puzzle.PuzzleState originalState)
        {
            this.puzzle = puzzle;
            this.mergingMode = mergingMode;
            resetToState(originalState);
        }

        private void resetToState(Puzzle.PuzzleState originalState)
        {
            this.totalCost = 0;
            this.originalState = originalState;
            this.unNormalizedState = originalState;
            this.moves.Clear();
            this.states.Clear();
            states.Add(unNormalizedState);
        }

        public bool isRedundant(string move)
        {
            // TODO - add support for MERGE_REDUNDANT_MOVES_PRESERVE_STATE
            //MergingMode mergingMode = preserveState ? MergingMode.MERGE_REDUNDANT_MOVES_PRESERVE_STATE : MergingMode.CANONICALIZE_MOVES;
            MergingMode mergingMode = MergingMode.CANONICALIZE_MOVES;
            IndexAndMove indexAndMove = findBestIndexForMove(move, mergingMode);
            return indexAndMove.index < moves.Count || indexAndMove.move == null;
        }

        public IndexAndMove findBestIndexForMove(string move, MergingMode mergingMode)
        {
            if (mergingMode == MergingMode.NO_MERGING)
            {
                return new IndexAndMove(moves.Count, move);
            }

            Puzzle.PuzzleState newUnNormalizedState = unNormalizedState.apply(move);
            if (newUnNormalizedState.equalsNormalized(unNormalizedState))
            {
                // move must just be a rotation.
                if (mergingMode == MergingMode.CANONICALIZE_MOVES)
                {
                    return new IndexAndMove(0, null);
                }
            }
            Puzzle.PuzzleState newNormalizedState = newUnNormalizedState.GetNormalized();

            LinkedHashMap<Puzzle.PuzzleState, string> successors = getState().GetCanonicalMovesByState();
            move = null;
            // Search for the right move to do to our current state in
            // order to match up with newNormalizedState.
            foreach (Puzzle.PuzzleState ps in successors.Keys)
            {
                if (ps.equalsNormalized(newNormalizedState))
                {
                    move = successors[ps];
                    break;
                }
            }
            // One of getStates()'s successors must be newNormalizedState.
            // If not, something has gone very wrong.
            //azzert(move != null);

            if (mergingMode == MergingMode.CANONICALIZE_MOVES)
            {
                for (int lastMoveIndex = moves.Count - 1; lastMoveIndex >= 0; lastMoveIndex--)
                {
                    string lastMove = moves[lastMoveIndex];
                    Puzzle.PuzzleState stateBeforeLastMove = states[lastMoveIndex];
                    if (!stateBeforeLastMove.movesCommute(lastMove, move))
                    {
                        break;
                    }
                    Puzzle.PuzzleState stateAfterLastMove = states[lastMoveIndex + 1];
                    Puzzle.PuzzleState stateAfterLastMoveAndNewMove = stateAfterLastMove.apply(move);

                    if (stateBeforeLastMove.equalsNormalized(stateAfterLastMoveAndNewMove))
                    {
                        // move cancels with lastMove
                        return new IndexAndMove(lastMoveIndex, null);
                    }
                    else
                    {
                        successors = stateBeforeLastMove.GetCanonicalMovesByState();
                        foreach (Puzzle.PuzzleState ps in successors.Keys)
                        {
                            if (ps.equalsNormalized(stateAfterLastMoveAndNewMove))
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

        public void appendMove(string newMove)
        {
            //l.fine("appendMove(" + newMove + ")");
            IndexAndMove indexAndMove = findBestIndexForMove(newMove, mergingMode);
            int oldCostMove, newCostMove;
            if (indexAndMove.index < moves.Count)
            {
                // This move is redundant.
                //azzert(mergingMode != MergingMode.NO_MERGING);
                oldCostMove = states[indexAndMove.index].getMoveCost(moves[indexAndMove.index]);
                if (indexAndMove.move == null)
                {
                    // newMove cancelled perfectly with the move at
                    // indexAndMove.index.
                    moves.RemoveAt(indexAndMove.index);
                    states.RemoveAt(indexAndMove.index + 1);
                    newCostMove = 0;
                }
                else
                {
                    // newMove merged with the move at indexAndMove.index.
                    moves[indexAndMove.index] = indexAndMove.move;
                    newCostMove = states[indexAndMove.index].getMoveCost(indexAndMove.move);
                }
            }
            else
            {
                oldCostMove = 0;
                newCostMove = states[states.Count - 1].getMoveCost(indexAndMove.move);
                // This move is not redundant.
                moves.Add(indexAndMove.move);
                // The code to update the states array is right below us,
                // but it requires that the states array be of the correct
                // size.
                states.Add(null);
            }

            totalCost += newCostMove - oldCostMove;

            // We modified moves[ indexAndMove.index ], so everything in
            // states[ indexAndMove.index+1, ... ] is now invalid
            for (int i = indexAndMove.index + 1; i < states.Count; i++)
            {
                states[i] = states[i - 1].apply(moves[i - 1]);
            }

            unNormalizedState = unNormalizedState.apply(newMove);
            //azzert(states.Count == moves.Count + 1);
            //azzert(unNormalizedState.equalsNormalized(getState()));
        }

        public string popMove(int index)
        {
            List<string> movesCopy = new List<string>(moves);
            string poppedMove = movesCopy[index];
            movesCopy.RemoveAt(index);

            resetToState(originalState);
            foreach (string move in movesCopy)
            {
                try
                {
                    appendMove(move);
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
                appendMove(move);
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
            return GwtSafeUtils.join(moves, " ");
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
