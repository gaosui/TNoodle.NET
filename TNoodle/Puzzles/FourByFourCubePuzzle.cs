using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TNoodle.Solvers.threephase;

namespace TNoodle.Puzzles
{
    public class FourByFourCubePuzzle : CubePuzzle
    {
        private ThreadLocal<Search> threePhaseSearcher = null;

        public FourByFourCubePuzzle() : base(4)
        {
            threePhaseSearcher = new ThreadLocal<Search>(() => new Search());

        }

        public override double GetInitializationStatus()
        {
            return Edge3.initStatus();
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            string scramble = threePhaseSearcher.Value.randomState(r);
            AlgorithmBuilder ab = new AlgorithmBuilder(this, MergingMode.CANONICALIZE_MOVES);
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
    }
}
