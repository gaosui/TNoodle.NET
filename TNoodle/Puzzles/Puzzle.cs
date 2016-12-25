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
        protected internal int wcaMinScrambleDistance = 2;

        /**
         * Returns a String describing this Scrambler
         * appropriate for use in a url. This shouldn't contain any periods.
         * @return a url appropriate String unique to this Scrambler
         */
        public abstract string getShortName();

        /**
         * Returns a String fully describing this Scrambler.
         * Unlike shortName(), may contain spaces and other url-inappropriate characters.
         * This will also be used for the toString method of this Scrambler.
         * @return a String
         */
        public abstract string getLongName();

        /**
         * Returns a number between 0 and 1 representing how "initialized" this
         * Scrambler is. 0 means nothing has been accomplished, and 1 means
         * we're done, and are generating scrambles.
         * @return A double between 0 and 1, inclusive.
         */
        public virtual double getInitializationStatus()
        {
            return 1;
        }

        /**
         * Returns the minimum distance from solved that any scramble this Puzzle
         * generates will be.
         */
        public virtual int getWcaMinScrambleDistance()
        {
            return wcaMinScrambleDistance;
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
        public string generateWcaScramble(Random r)
        {
            PuzzleStateAndGenerator psag;
            do
            {
                psag = generateRandomMoves(r);
            } while (psag.state.solveIn(wcaMinScrambleDistance - 1) != null);
            return psag.generator;
        }

        private string[] generateScrambles(Random r, int count)
        {
            string[] scrambles = new string[count];
            for (int i = 0; i < count; i++)
            {
                scrambles[i] = generateWcaScramble(r);
            }
            return scrambles;
        }

        private Random r = new Random();

        public string generateScramble()
        {
            return generateWcaScramble(r);
        }

        public string[] generateScrambles(int count)
        {
            return generateScrambles(r, count);
        }

        /** seeded scrambles, these can't be cached, so they'll be a little slower **/
        /*
        public string generateSeededScramble(string seed)
        {
            return generateSeededScramble(Encoding.UTF8.GetBytes(seed));
        }

        public string[] generateSeededScrambles(string seed, int count)
        {
            return generateSeededScrambles(Encoding.UTF8.GetBytes(seed), count);
        }

        private string generateSeededScramble(byte[] seed)
        {
            // We must create our own Random because
            // other threads can access the static one.
            // Also, setSeed supplements an existing seed,
            // rather than replacing it.
            // TODO - consider using something other than SecureRandom for seeded scrambles,
            // because we really, really want this to be portable across platforms (desktop java, gwt, and android)
            // https://github.com/thewca/tnoodle/issues/146
            Random r = new Random(BitConverter.ToInt32(seed, 0));
            //r.setSeed(seed);
            return generateWcaScramble(r);
        }

        private string[] generateSeededScrambles(byte[] seed, int count)
        {
            // We must create our own Random because
            // other threads can access the static one.
            // Also, setSeed supplements an existing seed,
            // rather than replacing it.
            Random r = new Random(BitConverter.ToInt32(seed, 0));
            //r.setSeed(seed);
            return generateScrambles(r, count);
        }
        */

        /**
         * @return Simply returns getLongName()
         */
        public override string ToString()
        {
            return getLongName();
        }

        protected internal virtual string solveIn(PuzzleState ps, int n)
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

            PuzzleState solvedNormalized = getSolvedState().getNormalized();
            fringeSolved.add(solvedNormalized, 0);
            seenSolved[solvedNormalized] = 0;
            fringeScrambled.add(ps.getNormalized(), 0);
            seenScrambled[ps.getNormalized()] = 0;

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


                LinkedHashMap<PuzzleState, string> movesByState = node.getCanonicalMovesByState();
                foreach (PuzzleState next in movesByState.Keys)
                {
                    int moveCost = node.getMoveCost(movesByState[next]);
                    int nextDistance = distance + moveCost;
                    //next = next.getNormalized();
                    var nNext = next.getNormalized();
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
                foreach (PuzzleState next in state.getCanonicalMovesByState().Keys)
                {
                    var nNext = next.getNormalized();
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
                foreach (var next in state.getCanonicalMovesByState())
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
                        distanceFromScrambled = seenScrambled[state.getNormalized()];
                        goto outer2;
                    }
                }
            outer2: { }
                //azzert(false);
            }

            // Step 3: solved <----- bestIntersection

            int distanceFromSolved = seenSolved[state.getNormalized()];
            while (distanceFromSolved > 0)
            {
                foreach (var next in state.getCanonicalMovesByState())
                {
                    PuzzleState nextState = next.Key;
                    PuzzleState nextStateNormalized = nextState.getNormalized();
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
        public abstract PuzzleState getSolvedState();

        /**
         * @return The number of random moves we must apply to call a puzzle
         * sufficiently scrambled.
         */
        protected internal abstract int getRandomMoveCount();

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
        public virtual PuzzleStateAndGenerator generateRandomMoves(Random r)
        {
            AlgorithmBuilder ab = new AlgorithmBuilder(
                    this, MergingMode.NO_MERGING);
            while (ab.getTotalCost() < getRandomMoveCount())
            {
                LinkedHashMap<string, PuzzleState> successors =
                      ab.getState().getScrambleSuccessors();
                string move;
                try
                {
                    do
                    {
                        move = GwtSafeUtils.choose(r, successors.Keys);
                        // If this move happens to be redundant, there is no
                        // reason to select this move again in vain.
                        successors.Remove(move);
                    } while (ab.isRedundant(move));
                    ab.appendMove(move);
                }
                catch
                {
                    //l.log(Level.SEVERE, "", e);
                    //azzert(false, e);
                    return null;
                }
            }
            return ab.getStateAndGenerator();
        }
    }
}