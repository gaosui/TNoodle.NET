using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using TNoodle.Solvers.Threephase;
using static TNoodle.Puzzles.AlgorithmBuilder;

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
            return Edge3.InitStatus();
        }

        public override PuzzleStateAndGenerator GenerateRandomMoves(Random r)
        {
            string scramble = threePhaseSearcher.Value.RandomState(r);
            AlgorithmBuilder ab = new AlgorithmBuilder(this, MergingMode.CanonicalizeMoves);
            try
            {
                ab.AppendAlgorithm(scramble);
            }
            catch //(InvalidMoveException e)
            {
                //azzert(false, new InvalidScrambleException(scramble, e));
            }
            return ab.GetStateAndGenerator();
        }
    }
}
