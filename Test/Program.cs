using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.min2phase;
using TNoodle.Solvers;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var solver = new TwoByTwoSolver();
            //var state = solver.randomState(new Random());
            //var perm = new int[7];
            //var ori = new int[7];
            //TwoByTwoSolver.unpackPerm(state.permutation, perm);
            //TwoByTwoSolver.unpackOrient(state.orientation, ori);
            //foreach (var item in perm)
            //{
            //    Console.WriteLine("perm " + item);
            //}
            //foreach (var item in ori)
            //{
            //    Console.WriteLine("ori " + item);
            //}
            var perm = new int[] { 2, 1, 0, 3, 4, 5, 6 };
            var ori = new int[] { 0, 0, 0, 0, 0, 0, 0 };
            var state = new TwoByTwoState
            {
                permutation = TwoByTwoSolver.packPerm(perm),
                orientation = TwoByTwoSolver.packOrient(ori)
            };
            Console.WriteLine(solver.solveIn(state, 10));

        }
    }
}
