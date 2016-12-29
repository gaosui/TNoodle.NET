using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Puzzles
{
    /**
     * Puzzle and TwistyPuzzle encapsulate all the information to filter out
     * scrambles <= wcaMinScrambleDistance (defaults to 1)
     * move away from solved (see generateWcaScramble),
     * and to generate random turn scrambles generically (see generateRandomMoves).
     *
     * The original proposal for these classes is accessible here:
     * https://docs.google.com/document/d/11ZfQPxAw0EhNNwE1yn5lZUO383qvAH6kJa2s3O9_6Zg/edit
     *
     * @author jeremy
     *
     */
    public abstract class Puzzle
    {
        public int WcaMinScrambleDistance { get; protected set; }

        /**
         * Returns a String describing this Scrambler
         * appropriate for use in a url. This shouldn't contain any periods.
         * @return a url appropriate String unique to this Scrambler
         */
        public abstract string GetShortName();

        /**
         * Returns a String fully describing this Scrambler.
         * Unlike shortName(), may contain spaces and other url-inappropriate characters.
         * This will also be used for the toString method of this Scrambler.
         * @return a String
         */
        public abstract string GetLongName();

        /**
         * Returns a number between 0 and 1 representing how "initialized" this
         * Scrambler is. 0 means nothing has been accomplished, and 1 means
         * we're done, and are generating scrambles.
         * @return A double between 0 and 1, inclusive.
         */
        public virtual double GetInitializationStatus()
        {
            return 1;
        }

        /**
         * Generates a scramble appropriate for this Scrambler. It's important to note that
         * it's ok if this method takes some time to run, as it's going to be called many times and get queued up
         * by ScrambleCacher.
         * NOTE:  If a puzzle wants to provide custom scrambles
         * (for example: Pochmann style megaminx or MRSS), it should override generateRandomMoves.
         * @param r The instance of Random you must use as your source of randomness when generating scrambles.
         * @return A String containing the scramble, where turns are assumed to be separated by whitespace.
         */
        public string GenerateWcaScramble(Random r)
        {
            PuzzleStateAndGenerator psag;
            do
            {
                psag = GenerateRandomMoves(r);
            } while (psag.state.solveIn(WcaMinScrambleDistance - 1) != null);
            return psag.generator;
        }

        /**
         * @return Simply returns getLongName()
         */
        public override string ToString()
        {
            return GetLongName();
        }

        protected virtual string SolveIn(PuzzleState ps, int n)
        {
            if (ps.isSolved())
            {
                return "";
            }

            Dictionary<PuzzleState, int> seenSolved = new Dictionary<PuzzleState, int>();
            SortedBuckets<PuzzleState> fringeSolved = new SortedBuckets<PuzzleState>();
            Dictionary<PuzzleState, int> seenScrambled = new Dictionary<PuzzleState, int>();
            SortedBuckets<PuzzleState> fringeScrambled = new SortedBuckets<PuzzleState>();

            // We're only interested in solutions of cost <= n
            int bestIntersectionCost = n + 1;
            PuzzleState bestIntersection = null;

            PuzzleState solvedNormalized = GetSolvedState().GetNormalized();
            fringeSolved.add(solvedNormalized, 0);
            seenSolved[solvedNormalized] = 0;
            fringeScrambled.add(ps.GetNormalized(), 0);
            seenScrambled[ps.GetNormalized()] = 0;

            //TimedLogRecordStart start = new TimedLogRecordStart(Level.FINER, "Searching for solution in " + n + " moves.");
            //l.log(start);

            int fringeTies = 0;

            // The task here is to do a breadth-first search starting from both the solved state and the scrambled state.
            // When we got an intersection from the two hash maps, we are done!
            int minFringeScrambled = -1, minFringeSolved = -1;
            while (!fringeSolved.isEmpty() || !fringeScrambled.isEmpty())
            {
                // We have to choose on which side we are extending our search.
                // I'm choosing the non empty fringe with the node nearest
                // its origin. In the event of a tie, we make sure to alternate.
                if (!fringeScrambled.isEmpty())
                {
                    minFringeScrambled = fringeScrambled.smallestValue();
                }
                if (!fringeSolved.isEmpty())
                {
                    minFringeSolved = fringeSolved.smallestValue();
                }
                bool extendSolved;
                if (fringeSolved.isEmpty() || fringeScrambled.isEmpty())
                {
                    // If the solved fringe is not empty, we'll expand it.
                    // Otherwise, we're expanding the scrambled fringe.
                    extendSolved = !fringeSolved.isEmpty();
                }
                else
                {
                    if (minFringeSolved < minFringeScrambled)
                    {
                        extendSolved = true;
                    }
                    else if (minFringeSolved > minFringeScrambled)
                    {
                        extendSolved = false;
                    }
                    else
                    {
                        extendSolved = (fringeTies++) % 2 == 0;
                    }
                }

                // We are using references for a more concise code.
                Dictionary<PuzzleState, int> seenExtending;
                SortedBuckets<PuzzleState> fringeExtending;
                Dictionary<PuzzleState, int> seenComparing;
                SortedBuckets<PuzzleState> fringeComparing;
                int minExtendingFringe, minComparingFringe;
                if (extendSolved)
                {
                    seenExtending = seenSolved;
                    fringeExtending = fringeSolved;
                    minExtendingFringe = minFringeSolved;
                    seenComparing = seenScrambled;
                    fringeComparing = fringeScrambled;
                    minComparingFringe = minFringeScrambled;
                }
                else
                {
                    seenExtending = seenScrambled;
                    fringeExtending = fringeScrambled;
                    minExtendingFringe = minFringeScrambled;
                    seenComparing = seenSolved;
                    fringeComparing = fringeSolved;
                    minComparingFringe = minFringeSolved;
                }

                PuzzleState node = fringeExtending.pop();
                int distance = seenExtending[node];
                if (seenComparing.ContainsKey(node))
                {
                    // We found an intersection! Compute the total cost of the
                    // path going through this node.
                    int cost = seenComparing[node] + distance;
                    if (cost < bestIntersectionCost)
                    {
                        bestIntersection = node;
                        bestIntersectionCost = cost;
                    }
                    continue;
                }
                // The best possible solution involving this node would
                // be through a child of this node that gets us across to
                // the other fringe's smallest distance node.
                int bestPossibleSolution = distance + minComparingFringe;
                if (bestPossibleSolution >= bestIntersectionCost)
                {
                    continue;
                }
                if (distance >= (n + 1) / 2)
                {
                    // The +1 is because if n is odd, we would have to search
                    // from one side with distance n/2 and from the other side
                    // distance n/2 + 1. Because we don't know which is which,
                    // let's take (n+1)/2 for both.
                    continue;
                }


                LinkedHashMap<PuzzleState, string> movesByState = node.GetCanonicalMovesByState();
                foreach (PuzzleState next in movesByState.Keys)
                {
                    int moveCost = node.getMoveCost(movesByState[next]);
                    int nextDistance = distance + moveCost;
                    //next = next.getNormalized();
                    var nNext = next.GetNormalized();
                    if (seenExtending.ContainsKey(nNext))
                    {
                        if (nextDistance >= seenExtending[nNext])
                        {
                            // We already found a better path to next.
                            continue;
                        }
                        // Go on to clobber seenExtending with our updated
                        // distance. Unfortunately, we're going have 2 copies
                        // of next in our fringe. This doesn't change correctness,
                        // it just means a bit of wasted work when we get around
                        // to popping off the second one.
                    }
                    fringeExtending.add(nNext, nextDistance);
                    seenExtending[nNext] = nextDistance;
                }
            }

            //l.log(start.finishedNow("expanded " + (seenSolved.size() + seenScrambled.size()) + " nodes"));

            if (bestIntersection == null)
            {
                return null;
            }

            // We have found a solution, but we still have to recover the move sequence.
            // the `bestIntersection` is the bound between the solved and the scrambled states.
            // We can travel from `bestIntersection` to either states, like that:
            // solved <----- bestIntersection -----> scrambled
            // However, to build a solution, we need to travel like that:
            // solved <----- bestIntersection <----- scrambled
            // So we have to travel backward for the scrambled side.

            // Step 1: bestIntersection -----> scrambled

            //azzert(bestIntersection.isNormalized());
            PuzzleState state = bestIntersection;
            int distanceFromScrambled = seenScrambled[state];

            // We have to keep track of all states we have visited
            PuzzleState[] linkedStates = new PuzzleState[distanceFromScrambled + 1];
            linkedStates[distanceFromScrambled] = state;

            while (distanceFromScrambled > 0)
            {
                foreach (PuzzleState next in state.GetCanonicalMovesByState().Keys)
                {
                    var nNext = next.GetNormalized();
                    if (seenScrambled.ContainsKey(nNext))
                    {
                        int newDistanceFromScrambled = seenScrambled[nNext];
                        if (newDistanceFromScrambled < distanceFromScrambled)
                        {
                            state = nNext;
                            distanceFromScrambled = newDistanceFromScrambled;
                            linkedStates[distanceFromScrambled] = state;
                            goto outer1;
                        }
                    }
                }
            outer1: { }
                //azzert(false);
            }

            // Step 2: bestIntersection <----- scrambled

            AlgorithmBuilder solution = new AlgorithmBuilder(this, MergingMode.CANONICALIZE_MOVES, ps);
            state = ps;
            distanceFromScrambled = 0;

            while (!state.equalsNormalized(bestIntersection))
            {
                foreach (var next in state.GetCanonicalMovesByState())
                {
                    PuzzleState nextState = next.Key;
                    string moveName = next.Value;
                    if (nextState.equalsNormalized(linkedStates[distanceFromScrambled + 1]))
                    {
                        state = nextState;
                        try
                        {
                            solution.appendMove(moveName);
                        }
                        catch
                        {
                            //azzert(false, e);
                        }
                        distanceFromScrambled = seenScrambled[state.GetNormalized()];
                        goto outer2;
                    }
                }
            outer2: { }
                //azzert(false);
            }

            // Step 3: solved <----- bestIntersection

            int distanceFromSolved = seenSolved[state.GetNormalized()];
            while (distanceFromSolved > 0)
            {
                foreach (var next in state.GetCanonicalMovesByState())
                {
                    PuzzleState nextState = next.Key;
                    PuzzleState nextStateNormalized = nextState.GetNormalized();
                    string moveName = next.Value;
                    if (seenSolved.ContainsKey(nextStateNormalized))
                    {
                        int newDistanceFromSolved = seenSolved[nextStateNormalized];
                        if (newDistanceFromSolved < distanceFromSolved)
                        {
                            state = nextState;
                            distanceFromSolved = newDistanceFromSolved;
                            try
                            {
                                solution.appendMove(moveName);
                            }
                            catch //(InvalidMoveException e)
                            {
                                ///azzert(false, e);
                            }
                            goto outer3;
                        }
                    }
                }
            outer3: { }
                //azzert(false);
            }

            return solution.ToString();
        }


        /**
         * @return A PuzzleState representing the solved state of our puzzle
         * from where we will begin scrambling.
         */
        public abstract PuzzleState GetSolvedState();

        /**
         * @return The number of random moves we must apply to call a puzzle
         * sufficiently scrambled.
         */
        protected abstract int GetRandomMoveCount();

        /**
         * This function will generate getRandomTurnCount() number of non cancelling,
         * random turns. If a puzzle wants to provide custom scrambles
         * (for example: Pochmann style megaminx or MRSS), it should override this method.
         *
         * NOTE: It is assumed that this method is thread safe! That means that if you're
         * overriding this method and you don't know what you're doing,
         * use the synchronized keyword when implementing this method:<br>
         * <code>protected synchronized String generateScramble(Random r);</code>
         * @param r An instance of Random
         * @return A PuzzleStateAndGenerator that contains a scramble string, and the
         *         state achieved by applying that scramble.
         */
        public virtual PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            AlgorithmBuilder ab = new AlgorithmBuilder(this, MergingMode.NO_MERGING);
            while (ab.getTotalCost() < GetRandomMoveCount())
            {
                LinkedHashMap<string, PuzzleState> successors = ab.getState().GetScrambleSuccessors();
                string move;

                do
                {
                    move = GwtSafeUtils.choose(r, successors.Keys);
                    // If this move happens to be redundant, there is no
                    // reason to select this move again in vain.
                    successors.Remove(move);
                } while (ab.isRedundant(move));
                ab.appendMove(move);
            }
            return ab.getStateAndGenerator();
        }

        public abstract class PuzzleState
        {
            private Puzzle puzzle;
            public PuzzleState(Puzzle p)
            {
                puzzle = p;
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
            public virtual LinkedHashMap<PuzzleState, string> GetCanonicalMovesByState()
            {
                LinkedHashMap<string, PuzzleState> successorsByName =
                      GetSuccessorsByName();
                LinkedHashMap<PuzzleState, string> uniqueSuccessors =
                    new LinkedHashMap<PuzzleState, string>();
                HashSet<PuzzleState> statesSeenNormalized = new HashSet<PuzzleState>();
                // We're not interested in any successor states are just a
                // rotation away.
                statesSeenNormalized.Add(this.GetNormalized());
                foreach (var next in successorsByName)
                {
                    PuzzleState nextState = next.Value;
                    PuzzleState nextStateNormalized = nextState.GetNormalized();
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
            public virtual PuzzleState GetNormalized()
            {
                return this;
            }

            public virtual bool IsNormalized()
            {
                return this.Equals(GetNormalized());
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
            public abstract LinkedHashMap<string, PuzzleState> GetSuccessorsByName();

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
            public virtual LinkedHashMap<string, PuzzleState> GetScrambleSuccessors()
            {
                return GwtSafeUtils.reverseHashMap(GetCanonicalMovesByState());
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

            public bool equalsNormalized(PuzzleState other)
            {
                return GetNormalized().Equals(other.GetNormalized());
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
                return puzzle;
            }

            public virtual bool isSolved()
            {
                return equalsNormalized(getPuzzle().GetSolvedState());
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
                LinkedHashMap<string, PuzzleState> successors = GetSuccessorsByName();
                if (!successors.Any(p => p.Key == move))
                {
                    throw new InvalidMoveException("Unrecognized turn " + move);
                }
                return successors.Single(p => p.Key == move).Value;
            }

            public virtual string solveIn(int n)
            {
                return getPuzzle().SolveIn(this, n);
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
}