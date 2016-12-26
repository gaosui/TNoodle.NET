using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Puzzles
{
    public class NoInspectionThreeByThreeCubePuzzle : ThreeByThreeCubePuzzle
    {
        public NoInspectionThreeByThreeCubePuzzle() : base()
        {
        }

        public override PuzzleStateAndGenerator generateRandomMoves(Random r)
        {
            CubeMove[][] randomOrientationMoves = getRandomOrientationMoves(size / 2);
            CubeMove[] randomOrientation = randomOrientationMoves[r.Next(randomOrientationMoves.Length)];
            string firstAxisRestriction;
            if (randomOrientation.Length > 0)
            {
                Face restrictedFace = randomOrientation[0].face;
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
            string lastAxisRestriction = null;
            PuzzleStateAndGenerator psag = base.generateRandomMoves(r, firstAxisRestriction, lastAxisRestriction);
            psag = NoInspectionFiveByFiveCubePuzzle.applyOrientation(this, randomOrientation, psag, false);
            return psag;
        }

        public override string getShortName()
        {
            return "333ni";
        }

        public override string getLongName()
        {
            return "3x3x3 no inspection";
        }
    }
}
