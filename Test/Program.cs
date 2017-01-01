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
		public static void Main()
		{
			var r = new Random(2017);
			var watch = new Stopwatch();
			double tick = 0;
			int count = 50;

			var puzzle = new ThreeByThreeCubePuzzle();
			string result;

			for (int i = 0; i < count; i++)
			{
				watch.Start();

				result = puzzle.GenerateWcaScramble(r);

				watch.Stop();
				Console.WriteLine(result);
				tick += watch.ElapsedTicks;
				watch.Reset();
			}

			tick /= count;
			Console.WriteLine($"{tick / TimeSpan.TicksPerMillisecond} ms");
		}
	}
}
