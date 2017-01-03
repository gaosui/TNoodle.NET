using System;

namespace TNoodle.Puzzles
{
    public class NoInspectionThreeByThreeCubePuzzle : ThreeByThreeCubePuzzle
    {
        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            var randomOrientationMoves = GetRandomOrientationMoves(Size / 2);
            var randomOrientation = randomOrientationMoves[r.Next(randomOrientationMoves.Length)];
            string firstAxisRestriction;
            if (randomOrientation.Length > 0)
            {
                var restrictedFace = randomOrientation[0].Face;
                // Restrictions are for an entire axis, so this will also
                // prevent the opposite of restrictedFace from being the first
                // move of our solution. This ensures that randomOrientation will
                // never be redundant with our scramble.
                firstAxisRestriction = restrictedFace.ToString();
            }
            else
            {
                firstAxisRestriction = null;
            }
            var psag = GenerateRandomMoves(r, firstAxisRestriction, null);
            psag = NoInspectionFiveByFiveCubePuzzle.ApplyOrientation(this, randomOrientation, psag, false);
            return psag;
        }

        public override string GetShortName()
        {
            return "333ni";
        }

        public override string GetLongName()
        {
            return "3x3x3 no inspection";
        }
    }
}