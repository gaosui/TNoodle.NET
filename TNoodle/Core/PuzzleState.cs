using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Core
{
    public abstract class PuzzleState
    {
        private Puzzle associatedPuzzle;
        public PuzzleState(Puzzle p)
        {
            associatedPuzzle = p;
        }

        /**
         *
         * @param algorithm A space separated String of moves to apply to state
         * @return The resulting PuzzleState
         * @throws InvalidScrambleException
         */
        public virtual PuzzleState applyAlgorithm(string algorithm)
        {
            PuzzleState state = this;
            foreach (string move in AlgorithmBuilder.splitAlgorithm(algorithm))
            {
                try
                {
                    state = state.apply(move);
                }
                catch (InvalidMoveException e)
                {
                    throw new InvalidScrambleException(algorithm, e);
                }
            }
            return state;
        }

        /**
         * Canonical successors are all the successor states that
         * are "normalized" unique.
         * @return A mapping of canonical PuzzleState's to the name of
         *         the move that gets you to them.
         */
        public virtual Dictionary<PuzzleState, string> getCanonicalMovesByState()
        {
            LinkedHashMap<string, PuzzleState> successorsByName =
                  getSuccessorsByName();
            Dictionary<PuzzleState, string> uniqueSuccessors =
                new Dictionary<PuzzleState, string>();
            HashSet<PuzzleState> statesSeenNormalized = new HashSet<PuzzleState>();
            // We're not interested in any successor states are just a
            // rotation away.
            statesSeenNormalized.Add(this.getNormalized());
            foreach (var next in successorsByName)
            {
                PuzzleState nextState = next.Value;
                PuzzleState nextStateNormalized = nextState.getNormalized();
                string moveName = next.Key;
                // Only add nextState if it's "unique"
                if (!statesSeenNormalized.Contains(nextStateNormalized))
                {
                    uniqueSuccessors[nextState] = moveName;
                    statesSeenNormalized.Add(nextStateNormalized);
                }
            }

            return uniqueSuccessors;
        }

        /**
         * There exist PuzzleState's that are 0 moves apart, but are
         * not .equal(). This is because we consider the visibly different
         * PuzzleState's to be not equals (consider the state achieved by
         * applying L to a solved 3x3x3, and the state after applying Rw.
         * These puzzles "look" different, but they are 0 moves apart.
         * @return A PuzzleState that all rotations of state will all
         *         return when normalized. This makes it possible to check
         *         if 2 puzzle states are 0 moves apart, even if they
         *         "look" different.
         * TODO - This method could be implemented in this superclass by
         *        defining a "cost" for moves (which we will have to do for
         *        sq1 anyways), and walking the complete
         *        0 cost state tree for this state. Then we'd return one
         *        element from that state tree in a deterministic way.
         *        We could do something simple like returning the state
         *        that has the smallest hash, but that wouldn't work if
         *        we have hash collisions. I think the best thing to do
         *        would be to require all PuzzleStates to implement
         *        a marshall() function that returns a unique string. Then
         *        we can just do an alphabetical sort of these and return the
         *        min or max.
         */
        public virtual PuzzleState getNormalized()
        {
            return this;
        }

        public virtual bool isNormalized()
        {
            return this.Equals(getNormalized());
        }

        /**
         * Most puzzles are happy to split an algorithm by turns, and declare
         * each turn a move. However, this simple model doesn't work for all
         * puzzles. For example, square one may wish to declare (3,3) as 1
         * move. Another possible use for this would be rotations, which
         * count as 0 moves.
         * @param move
         * @return The cost of doing this move.
         */
        public virtual int getMoveCost(string move)
        {
            return 1;
        }

        /**
         * @return A LinkedHashMap mapping move Strings to resulting PuzzleStates.
         *         The move Strings may not contain spaces.
         *         Multiple keys (moves) in the returned LinkedHashMap may
         *         map to the same state, or states that are .equal().
         *         Preferred notations should appear earlier in the
         *         LinkedHashMap.
         */
        public abstract LinkedHashMap<string, PuzzleState> getSuccessorsByName();

        /**
         * By default, this method returns getSuccessorsByName(). Some
         * puzzles may wish to override this method to provide a reduced set
         * of moves to be used for scrambling.
         * <br><br>
         * One example of where this is useful is a puzzle like the square
         * one. Someone extending Puzzle to implement SquareOnePuzzle is left
         * with the question of whether to allow turns that leave the puzzle
         * incapable of doing a /.
         * <br><br>
         * If getSuccessorsByName() returns states that cannot do a /, then
         * generateRandomMoves() will hang because any move that can be
         * applied to one of those states is redundant.
         * <br><br>
         * Alternatively, if getSuccessorsByName() only returns states that
         * can do a /, AlgorithmBuilder's isRedundant() breaks.
         * Here's why:<br>
         * Imagine a solved square one. Lets say we pick the turn (1,0) to
         * apply to it, and now we're considering applying (2,0) to it.
         * Obviously this is the exact same state you would have achieved by
         * just applying (3,0) to the solved puzzle, but isRedundant()
         * only checks for this against the previous moves that commute with
         * (2,0). movesCommute("(1,0)", "(2,0)") will only return
         * true if (2,0) can be applied to a solved square one, even though
         * it results in a state that cannot
         * be slashed.
         
         * @return A HashMap mapping move Strings to resulting PuzzleStates.
         *         The move Strings may not contain spaces.
         */
        public virtual Dictionary<string, PuzzleState> getScrambleSuccessors()
        {
            return GwtSafeUtils.reverseHashMap(getCanonicalMovesByState());
        }

        /**
         * Returns true if this state is equal to other.
         * Note that a puzzle like 4x4 must compare all orientations of the puzzle, otherwise
         * generateRandomMoves() will allow for trivial sequences of turns like Lw Rw'.
         * @param other
         * @return true if this is equal to other
         */
        public override abstract bool Equals(object other);

        public override abstract int GetHashCode();

        public virtual bool equalsNormalized(PuzzleState other)
        {
            return getNormalized().Equals(other.getNormalized());
        }

        /**
         * Draws the state of the puzzle.
         * NOTE: It is assumed that this method is thread safe! That means unless you know what you're doing,
         * use the synchronized keyword when implementing this method:<br>
         * <code>protected synchronized void drawScramble();</code>
         * @return An Svg instance representing this scramble.
         */
        //protected abstract Svg drawScramble(HashMap<String, Color> colorScheme);

        public virtual Puzzle getPuzzle()
        {
            return associatedPuzzle;
        }

        public virtual bool isSolved()
        {
            return equalsNormalized(getPuzzle().getSolvedState());
        }

        /**
         * Applies the given move to this PuzzleState. This method is non destructive,
         * that is, it does not mutate the current state, instead it returns a new state.
         * @param move The move to apply
         * @return The PuzzleState achieved after applying move
         * @throws InvalidMoveException if the move is unrecognized.
         */
        public virtual PuzzleState apply(string move)
        {
            LinkedHashMap<string, PuzzleState> successors = getSuccessorsByName();
            if (!successors.ContainsKey(move))
            {
                throw new InvalidMoveException("Unrecognized turn " + move);
            }
            return successors[move];
        }

        public virtual string solveIn(int n)
        {
            return getPuzzle().solveIn(this, n);
        }

        /**
         * Two moves A and B commute on a puzzle if regardless of
         * the order you apply A and B, you end up in the same state.
         * Interestingly enough, the set of moves that commute can change
         * with the state a puzzle is in. That's why this is a method of
         * PuzzleState instead of Puzzle.
         * @param move1
         * @param move2
         * @return True iff move1 and move2 commute.
         */
        internal bool movesCommute(string move1, string move2)
        {
            try
            {
                PuzzleState state1 = apply(move1).apply(move2);
                PuzzleState state2 = apply(move2).apply(move1);
                return state1.Equals(state2);
            }
            catch
            {
                return false;
            }
        }
    }
}
