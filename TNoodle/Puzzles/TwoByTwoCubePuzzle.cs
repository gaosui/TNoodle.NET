using System;
using TNoodle.Solvers;
using static TNoodle.Utils.Assertion;
using static TNoodle.Puzzles.AlgorithmBuilder;

namespace TNoodle.Puzzles
{
	public class TwoByTwoCubePuzzle : CubePuzzle
	{
		const int TWO_BY_TWO_MIN_SCRAMBLE_LENGTH = 11;
		readonly TwoByTwoSolver twoSolver = new TwoByTwoSolver();

		public TwoByTwoCubePuzzle() : base(2)
		{
			WcaMinScrambleDistance = 4;
		}

		public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
		{
			var state = twoSolver.RandomState(r);
			var scramble = twoSolver.GenerateExactly(state, TWO_BY_TWO_MIN_SCRAMBLE_LENGTH);

			var ab = new AlgorithmBuilder(this, MergingMode.CANONICALIZE_MOVES);
			try
			{

				ab.appendAlgorithm(scramble);
			}
			catch (InvalidMoveException e)
			{
				Assert(false, e.Message, new InvalidScrambleException(scramble, e));
			}
			return ab.getStateAndGenerator();
		}

		protected override string SolveIn(PuzzleState ps, int n)
		{
			var cs = (CubeState)ps;
			var solution = twoSolver.SolveIn(cs.ToTwoByTwoState(), n);
			return solution;
		}
	}
}
