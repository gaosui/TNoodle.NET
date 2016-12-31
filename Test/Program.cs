using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Solvers.Threephase;
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
            double tick = 0;
            int count = 50;

            var solver = new Search();
            FullCube state = null;


            for (int i = 0; i < count; i++)
            {
                state = new FullCube(r);
                watch.Start();

                solver.Calc(state);

                watch.Stop();
                tick += watch.ElapsedTicks;
                watch.Reset();
            }

            tick /= count;
            Console.WriteLine($"{tick / TimeSpan.TicksPerMillisecond} ms");

        }
    }
}
