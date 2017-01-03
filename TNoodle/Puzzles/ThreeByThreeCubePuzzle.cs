using System;
using TNoodle.Solvers.Min2phase;
using static TNoodle.Utils.Assertion;
using static TNoodle.Puzzles.AlgorithmBuilder;

namespace TNoodle.Puzzles
{
    public class ThreeByThreeCubePuzzle : CubePuzzle
    {
        private const int ThreeByThreeMaxScrambleLength = 21;
        private const int ThreeByThreeTimemin = 200; //milliseconds
        private const int ThreeByThreeTimeout = 60 * 1000; //milliseconds

        private readonly Search _twoPhaseSearcher = new Search();

        public ThreeByThreeCubePuzzle() : base(3)
        {
        }

        protected override string SolveIn(PuzzleState ps, int n)
        {
            return SolveIn(ps, n, null, null);
        }

        protected string SolveIn(PuzzleState ps, int n, string firstAxisRestriction, string lastAxisRestriction)
        {
            var cs = (CubeState) ps;
            if (Equals(GetSolvedState()))
                return "";
            var solution =
                _twoPhaseSearcher.Solution(cs.ToFaceCube(), n, ThreeByThreeTimeout, 0, 0, firstAxisRestriction,
                    lastAxisRestriction).Trim();
            if ("Error 7".Equals(solution))
                return null;
            if (!solution.StartsWith("Error", StringComparison.Ordinal)) return solution;
            // TODO - Not really sure what to do here.
            //l.severe(solution + " while searching for solution to " + cs.toFaceCube());
            Assert(false);
            return null;
        }

        public PuzzleStateAndGenerator GenerateRandomMoves(Random r, string firstAxisRestriction,
            string lastAxisRestriction)
        {
            var randomState = Tools.RandomCube(r);
            var scramble =
                _twoPhaseSearcher.Solution(randomState, ThreeByThreeMaxScrambleLength, ThreeByThreeTimeout,
                    ThreeByThreeTimemin, Search.INVERSE_SOLUTION, firstAxisRestriction, lastAxisRestriction).Trim();

            var ab = new AlgorithmBuilder(MergingMode.CanonicalizeMoves, GetSolvedState());
            try
            {
                ab.AppendAlgorithm(scramble);
            }
            catch (InvalidMoveException e)
            {
                Assert(false, e.Message, new InvalidScrambleException(scramble, e));
            }
            return ab.GetStateAndGenerator();
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            return GenerateRandomMoves(r, null, null);
        }
    }
}