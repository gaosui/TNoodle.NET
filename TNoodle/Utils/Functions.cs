using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TNoodle.Utils
{
    public static class Functions
    {
        public static int BitCount(int value)
        {
            uint v = (uint)value;
            uint c;

            c = v - ((v >> 1) & 0x55555555);
            c = ((c >> 2) & 0x33333333) + (c & 0x33333333);
            c = ((c >> 4) + c) & 0x0F0F0F0F;
            c = ((c >> 8) + c) & 0x00FF00FF;
            c = ((c >> 16) + c) & 0x0000FFFF;

            return (int)c;
        }
        public static int modulo(int x, int m)
        {
            //azzert(m > 0, "m must be > 0");
            int y = x % m;
            if (y < 0)
            {
                y += m;
            }
            return y;
        }

        public static int DeepHashCode(this int[] a)
        {
            if (a == null)
                return 0;

            int result = 1;
            foreach (int element in a)
                result = 31 * result + element;

            return result;
        }
    }
}
