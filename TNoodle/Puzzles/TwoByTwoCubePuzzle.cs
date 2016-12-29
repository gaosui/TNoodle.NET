using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNoodle.Solvers;
using System.Threading.Tasks;

namespace TNoodle.Puzzles
{
    public class TwoByTwoCubePuzzle : CubePuzzle
    {
        private const int TWO_BY_TWO_MIN_SCRAMBLE_LENGTH = 11;
        private TwoByTwoSolver twoSolver = new TwoByTwoSolver();

        public TwoByTwoCubePuzzle() : base(2)
        {
            WcaMinScrambleDistance = 4;
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            TwoByTwoState state = twoSolver.RandomState(r);
            string scramble = twoSolver.GenerateExactly(state, TWO_BY_TWO_MIN_SCRAMBLE_LENGTH);

            AlgorithmBuilder ab = new AlgorithmBuilder(this, MergingMode.CANONICALIZE_MOVES);
            ab.appendAlgorithm(scramble);
            return ab.getStateAndGenerator();
        }

        protected override string SolveIn(PuzzleState ps, int n)
        {
            CubeState cs = (CubeState)ps;
            string solution = twoSolver.SolveIn(cs.ToTwoByTwoState(), n);
            return solution;
        }
    }
}
