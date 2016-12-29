using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Solvers.Min2phase;
using TNoodle.Solvers;
using TNoodle.Puzzles;
using TNoodle.Utils;
using System.Diagnostics;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Random r = new Random(2017);
            var watch = new Stopwatch();

            //list.Add(new TwoByTwoCubePuzzle());
            //list.Add(new ThreeByThreeCubePuzzle());
            //list.Add(new FourByFourCubePuzzle());
            //list.Add(new CubePuzzle(5));
            //list.Add(new CubePuzzle(6));
            //list.Add(new CubePuzzle(7));
            //list.Add(new NoInspectionThreeByThreeCubePuzzle());
            //list.Add(new NoInspectionFourByFourCubePuzzle());
            //list.Add(new NoInspectionFiveByFiveCubePuzzle());
            //list.Add(new ThreeByThreeCubeFewestMovesPuzzle());
            //list.Add(new PyraminxPuzzle());
            //list.Add(new SquareOnePuzzle());
            //list.Add(new MegaminxPuzzle());
            //list.Add(new SkewbPuzzle());
            //list.Add(new ClockPuzzle());

            //foreach (var p in list)
            //{
            //    Console.WriteLine($"{p.GetLongName()} {p.GenerateWcaScramble(r)}");
            //    Console.WriteLine();
            //}
            //var puzzle = new ThreeByThreeCubePuzzle();
            var solver = new Search();
            string state = null;

            double tick = 0;
            int count = 50;

            for (int i = 0; i < count; i++)
            {
                state = Tools.RandomCube(r);
                watch.Start();
                Console.WriteLine(solver.Solution(state, 21, 60000, 0, 0, null, null));
                watch.Stop();
                tick += watch.ElapsedTicks;
                watch.Reset();
            }

            tick /= count;
            Console.WriteLine($"{tick / TimeSpan.TicksPerMillisecond} ms");

        }
    }
}
