using System;
using static TNoodle.Puzzles.AlgorithmBuilder;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Puzzles
{
    public class NoInspectionFiveByFiveCubePuzzle : CubePuzzle
    {
        public NoInspectionFiveByFiveCubePuzzle() : base(5)
        {
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            var randomOrientationMoves = GetRandomOrientationMoves(Size / 2);
            var randomOrientation = randomOrientationMoves[r.Next(randomOrientationMoves.Length)];
            var psag = base.GenerateRandomMoves(r);
            psag = ApplyOrientation(this, randomOrientation, psag, true);
            return psag;
        }

        public static PuzzleStateAndGenerator ApplyOrientation(CubePuzzle puzzle, CubeMove[] randomOrientation,
            PuzzleStateAndGenerator psag, bool discardRedundantMoves)
        {
            if (randomOrientation.Length == 0)
                return psag;

            // Append reorientation to scramble.
            try
            {
                var ab = new AlgorithmBuilder(MergingMode.NoMerging, puzzle.GetSolvedState());
                ab.AppendAlgorithm(psag.Generator);
                // Check if our reorientation is going to cancel with the last
                // turn of our scramble. If it does, then we just discard
                // that last turn of our scramble. This ensures we have a scramble
                // with no redundant turns, and I can't see how it could hurt the
                // quality of our scrambles to do this.
                var firstReorientMove = randomOrientation[0].ToString();
                while (ab.IsRedundant(firstReorientMove))
                {
                    //azzert(discardRedundantMoves);
                    var im = ab.FindBestIndexForMove(firstReorientMove, MergingMode.CanonicalizeMoves);
                    ab.PopMove(im.Index);
                }
                foreach (var cm in randomOrientation)
                    ab.AppendMove(cm.ToString());

                psag = ab.GetStateAndGenerator();
                return psag;
            }
            catch (InvalidMoveException e)
            {
                Assert(false, e.Message, e);
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