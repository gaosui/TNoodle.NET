using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Solvers.min2phase;
using TNoodle.Solvers;
using TNoodle.Puzzles;
using TNoodle.Utils;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            List<Puzzle> list = new List<Puzzle>();

            //list.Add(new TwoByTwoCubePuzzle());
            //list.Add(new ThreeByThreeCubePuzzle());
            //list.Add(new FourByFourCubePuzzle());
            //list.Add(new CubePuzzle(5));
            //list.Add(new CubePuzzle(6));
            //list.Add(new CubePuzzle(7));
            ///list.Add(new NoInspectionThreeByThreeCubePuzzle());
            //list.Add(new NoInspectionFourByFourCubePuzzle());
            //list.Add(new NoInspectionFiveByFiveCubePuzzle());
            //list.Add(new ThreeByThreeCubeFewestMovesPuzzle());
            //list.Add(new PyraminxPuzzle());
            list.Add(new SquareOnePuzzle());

            foreach (var p in list)
            {
                Console.WriteLine($"{p.getLongName()} {p.generateScramble()}");
                Console.WriteLine();
            }

            Console.ReadKey();
        }
    }
}
