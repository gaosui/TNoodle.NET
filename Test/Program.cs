using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using cs.min2phase;

namespace Test
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Search s = new Search();
            Console.WriteLine(s.solution(Tools.randomCube(), 30, 10000, 1, 0));
        }
    }
}
