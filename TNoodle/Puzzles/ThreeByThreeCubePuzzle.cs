using System;
using TNoodle.Solvers.Min2phase;
using System.Threading;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
	public class ThreeByThreeCubePuzzle : CubePuzzle
	{
		//private static final Logger l = Logger.getLogger(ThreeByThreeCubePuzzle.class.getName());
		const int THREE_BY_THREE_MAX_SCRAMBLE_LENGTH = 21;
		const int THREE_BY_THREE_TIMEMIN = 200; //milliseconds
		const int THREE_BY_THREE_TIMEOUT = 60 * 1000; //milliseconds

		ThreadLocal<Search> twoPhaseSearcher;
		public ThreeByThreeCubePuzzle() : base(3)
		{
			//string newMinDistance = EnvGetter.getenv("TNOODLE_333_MIN_DISTANCE");
			//if (newMinDistance != null)
			//{
			//wcaMinScrambleDistance = Integer.parseInt(newMinDistance);
			//}
			twoPhaseSearcher = new ThreadLocal<Search>(() => new Search());
			//{
			//protected Search initialValue()
			// {/
			// return new Search();
			//};
			//};
		}

		protected override string SolveIn(PuzzleState ps, int n)
		{
			return SolveIn(ps, n, null, null);
		}

		public string SolveIn(PuzzleState ps, int n, string firstAxisRestriction, string lastAxisRestriction)
		{
			var cs = (CubeState)ps;
			if (Equals(GetSolvedState()))
			{
				// TODO - apparently min2phase can't solve the solved cube
				return "";
			}
			var solution = twoPhaseSearcher.Value.Solution(cs.ToFaceCube(), n, THREE_BY_THREE_TIMEOUT, 0, 0, firstAxisRestriction, lastAxisRestriction).Trim();
			if ("Error 7".Equals(solution))
			{
				// No solution exists for given depth
				return null;
			}
			if (solution.StartsWith("Error", StringComparison.Ordinal))
			{
				// TODO - Not really sure what to do here.
				//l.severe(solution + " while searching for solution to " + cs.toFaceCube());
				Assert(false);
				return null;
			}
			return solution;
		}

		public PuzzleStateAndGenerator GenerateRandomMoves(Random r, string firstAxisRestriction, string lastAxisRestriction)
		{
			var randomState = Tools.RandomCube(r);
			var scramble = twoPhaseSearcher.Value.Solution(randomState, THREE_BY_THREE_MAX_SCRAMBLE_LENGTH, THREE_BY_THREE_TIMEOUT, THREE_BY_THREE_TIMEMIN, Search.INVERSE_SOLUTION, firstAxisRestriction, lastAxisRestriction).Trim();

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

		public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
		{
			return GenerateRandomMoves(r, null, null);
		}
	}
}
