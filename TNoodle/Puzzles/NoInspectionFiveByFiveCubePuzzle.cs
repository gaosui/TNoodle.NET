using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Puzzles
{
    public class NoInspectionFiveByFiveCubePuzzle : CubePuzzle
    {
        public NoInspectionFiveByFiveCubePuzzle() : base(5)
        {
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            CubeMove[][] randomOrientationMoves = GetRandomOrientationMoves(Size / 2);
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
                // Check if our reorientation is going to cancel with the last
                // turn of our scramble. If it does, then we just discard
                // that last turn of our scramble. This ensures we have a scramble
                // with no redundant turns, and I can't see how it could hurt the
                // quality of our scrambles to do this.
                String firstReorientMove = randomOrientation[0].ToString();
                while (ab.isRedundant(firstReorientMove))
                {
                    //azzert(discardRedundantMoves);
                    IndexAndMove im = ab.findBestIndexForMove(firstReorientMove, MergingMode.CANONICALIZE_MOVES);
                    ab.popMove(im.index);
                }
                foreach (CubeMove cm in randomOrientation)
                {
                    ab.appendMove(cm.ToString());
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
            return "555ni";
        }

        public override string GetLongName()
        {
            return "5x5x5 no inspection";
        }
    }
}
