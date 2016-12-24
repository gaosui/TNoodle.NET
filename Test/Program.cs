using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Solvers.min2phase;
using TNoodle.Solvers;
using TNoodle.Puzzles;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var p = new TwoByTwoCubePuzzle();
            Console.WriteLine(p.generateScramble());

            var p3x3 = new ThreeByThreeCubePuzzle();
            Console.WriteLine(p3x3.generateScramble());

            Console.ReadKey();
        }
    }
}
