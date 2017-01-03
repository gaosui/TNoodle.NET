using System;
using TNoodle.Solvers;
using static TNoodle.Utils.Assertion;
using static TNoodle.Puzzles.AlgorithmBuilder;

namespace TNoodle.Puzzles
{
    public class TwoByTwoCubePuzzle : CubePuzzle
    {
        private const int TwoByTwoMinScrambleLength = 11;
        private readonly TwoByTwoSolver _twoSolver = new TwoByTwoSolver();

        public TwoByTwoCubePuzzle() : base(2)
        {
            WcaMinScrambleDistance = 4;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            var state = _twoSolver.RandomState(r);
            var scramble = _twoSolver.GenerateExactly(state, TwoByTwoMinScrambleLength);

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

        protected override string SolveIn(PuzzleState ps, int n)
        {
            var cs = (CubeState) ps;
            var solution = _twoSolver.SolveIn(cs.ToTwoByTwoState(), n);
            return solution;
        }
    }
}