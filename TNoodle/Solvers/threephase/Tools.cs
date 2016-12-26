using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace TNoodle.Solvers.threephase
{
    public class Tools
    {
        private static void read(int[] arr, Stream s)
        {
            byte[] buffer = new byte[4];
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                s.Read(buffer, 0, 4);
                arr[i] = BitConverter.ToInt32(buffer, 0);
            }
            
        }
    }
}
