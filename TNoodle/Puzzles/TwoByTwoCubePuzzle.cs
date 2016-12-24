using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using TNoodle.Solvers;
using System.Threading.Tasks;
using TNoodle.Core;

namespace TNoodle.Puzzles
{
    public class TwoByTwoCubePuzzle : CubePuzzle
    {
        private const int TWO_BY_TWO_MIN_SCRAMBLE_LENGTH = 11;

        private TwoByTwoSolver twoSolver = null;
        public TwoByTwoCubePuzzle() : base(2)
        {
            wcaMinScrambleDistance = 4;
            twoSolver = new TwoByTwoSolver();
        }

        public override PuzzleStateAndGenerator generateRandomMoves(Random r)
        {
            TwoByTwoState state = twoSolver.randomState(r);
            string scramble = twoSolver.generateExactly(state, TWO_BY_TWO_MIN_SCRAMBLE_LENGTH);

            AlgorithmBuilder ab = new AlgorithmBuilder(this, AlgorithmBuilder.MergingMode.CANONICALIZE_MOVES);
            try
            {
                ab.appendAlgorithm(scramble);
            }
            catch //(InvalidMoveException e)
            {
                //azzert(false, new InvalidScrambleException(scramble, e));
            }
            return ab.getStateAndGenerator();
        }

        protected internal override string solveIn(PuzzleState ps, int n)
        {
            CubeState cs = (CubeState)ps;
            string solution = twoSolver.solveIn(cs.toTwoByTwoState(), n);
            return solution;
        }
    }
}
