using System;
using TNoodle.Solvers.Threephase;
using static TNoodle.Puzzles.AlgorithmBuilder;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
    public class FourByFourCubePuzzle : CubePuzzle
    {
        private readonly Search _threePhaseSearcher = new Search();

        public FourByFourCubePuzzle() : base(4)
        {
        }

        public override double GetInitializationStatus()
        {
            return Edge3.InitStatus();
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            var scramble = _threePhaseSearcher.RandomState(r);
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
    }
}