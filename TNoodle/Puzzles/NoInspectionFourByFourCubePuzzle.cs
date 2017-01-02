using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static TNoodle.Puzzles.AlgorithmBuilder;

namespace TNoodle.Puzzles
{
    public class NoInspectionFourByFourCubePuzzle : FourByFourCubePuzzle
    {
        public NoInspectionFourByFourCubePuzzle()
        {
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            CubeMove[][] randomOrientationMoves = GetRandomOrientationMoves(Size - 1);
            CubeMove[] randomOrientation = randomOrientationMoves[r.Next(randomOrientationMoves.Length)];
            PuzzleStateAndGenerator psag = base.GenerateRandomMoves(r);
            psag = applyOrientation(this, randomOrientation, psag, true);
            return psag;
        }

        public static PuzzleStateAndGenerator applyOrientation(CubePuzzle puzzle, CubeMove[] randomOrientation, PuzzleStateAndGenerator psag, bool discardRedundantMoves)
        {
            if (randomOrientation.Length == 0)
            {
                // No reorientation required
                return psag;
            }

            // Append reorientation to scramble.
            try
            {
                AlgorithmBuilder ab = new AlgorithmBuilder(puzzle, MergingMode.NO_MERGING);
                ab.appendAlgorithm(psag.generator);
                foreach (CubeMove cm in randomOrientation)
                {
                    ab.AppendMove(cm.ToString());
                }

                psag = ab.getStateAndGenerator();
                return psag;
            }
            catch //(InvalidMoveException e)
            {
                //azzert(false, e);
                return null;
            }
        }

        public override string GetShortName()
        {
            return "444ni";
        }

        public override string GetLongName()
        {
            return "4x4x4 no inspection";
        }
    }
}
