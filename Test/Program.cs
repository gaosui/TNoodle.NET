using System;
using System.Diagnostics;
using TNoodle.Puzzles;

namespace Test
{
    public class Program
    {
        public static void Main()
        {
            var r = new Random(2017);
            var watch = new Stopwatch();
            var tick = 0.0;
            const int count = 50;

            var puzzle = new PyraminxPuzzle();

            for (var i = 0; i < count; i++)
            {
                watch.Restart();

                var result = puzzle.GenerateWcaScramble(r);

                watch.Stop();
                Console.WriteLine(result);
                tick += watch.ElapsedTicks;
            }

            tick /= count;
            Console.WriteLine($"{tick / TimeSpan.TicksPerMillisecond} ms");
        }
    }
}