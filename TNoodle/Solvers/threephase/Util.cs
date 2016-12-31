using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Utils;

namespace TNoodle.Solvers.Threephase
{
    internal static class Util
    {
		public static int[][] Cnk { get; } = ArrayExtension.New<int>(25, 25);
        private static readonly int[] fact = new int[13];
        public static char[] ColorMap4to3 { get; } = { 'U', 'D', 'F', 'B', 'R', 'L' };

        static Util()
        {
            for (int i = 0; i < 25; i++)
            {
                Cnk[i][i] = 1;
                Cnk[i][0] = 1;
            }
            for (int i = 1; i < 25; i++)
            {
                for (int j = 1; j <= i; j++)
                {
                    Cnk[i][j] = Cnk[i - 1][j] + Cnk[i - 1][j - 1];
                }
            }
            fact[0] = 1;
            for (int i = 0; i < 12; i++)
            {
                fact[i + 1] = fact[i] * (i + 1);
            }
        }

        public static int[] Tomove(string s)
        {
            s = s.Replace(" ", "");
            int[] arr = new int[s.Length];
            int j = 0;
            for (int i = 0, length = s.Length; i < length; i++)
            {
                int axis = -1;
                switch (s[i])
                {
                    case 'U': axis = 0; break;
                    case 'R': axis = 1; break;
                    case 'F': axis = 2; break;
                    case 'D': axis = 3; break;
                    case 'L': axis = 4; break;
                    case 'B': axis = 5; break;
                    case 'u': axis = 6; break;
                    case 'r': axis = 7; break;
                    case 'f': axis = 8; break;
                    case 'd': axis = 9; break;
                    case 'l': axis = 10; break;
                    case 'b': axis = 11; break;
                    default: continue;
                }
                axis *= 3;
                if (++i < length)
                {
                    switch (s[i])
                    {
                        case '2': axis++; break;
                        case '\'': axis += 2; break;
                        default: --i; break;
                    }
                }
                arr[j++] = axis;
            }

            int[] ret = new int[j];
            while (--j >= 0)
            {
                ret[j] = arr[j];
            }
            return ret;
        }

        public static void Swap(int[] arr, int a, int b, int c, int d, int key)
        {
            int temp;
            switch (key)
            {
                case 0:
                    temp = arr[d];
                    arr[d] = arr[c];
                    arr[c] = arr[b];
                    arr[b] = arr[a];
                    arr[a] = temp;
                    return;
                case 1:
                    temp = arr[a];
                    arr[a] = arr[c];
                    arr[c] = temp;
                    temp = arr[b];
                    arr[b] = arr[d];
                    arr[d] = temp;
                    return;
                case 2:
                    temp = arr[a];
                    arr[a] = arr[b];
                    arr[b] = arr[c];
                    arr[c] = arr[d];
                    arr[d] = temp;
                    return;
            }
        }

        public static void Swap(sbyte[] arr, int a, int b, int c, int d, int key)
        {
            sbyte temp;
            switch (key)
            {
                case 0:
                    temp = arr[d];
                    arr[d] = arr[c];
                    arr[c] = arr[b];
                    arr[b] = arr[a];
                    arr[a] = temp;
                    return;
                case 1:
                    temp = arr[a];
                    arr[a] = arr[c];
                    arr[c] = temp;
                    temp = arr[b];
                    arr[b] = arr[d];
                    arr[d] = temp;
                    return;
                case 2:
                    temp = arr[a];
                    arr[a] = arr[b];
                    arr[b] = arr[c];
                    arr[c] = arr[d];
                    arr[d] = temp;
                    return;
            }
        }

        public static void Set8Perm(sbyte[] arr, int idx)
        {
            int val = 0x76543210;
            for (int i = 0; i < 7; i++)
            {
                int p = fact[7 - i];
                int v = idx / p;
                idx -= v * p;
                v <<= 2;
                arr[i] = (sbyte)((val >> v) & 0xf);
                int m = (1 << v) - 1;
                val = (val & m) + ((val >> 4) & ~m);
            }
            arr[7] = (sbyte)val;
        }

        public static int Parity(sbyte[] arr)
        {
            int parity = 0;
            for (int i = 0, len = arr.Length; i < len; i++)
            {
                for (int j = i; j < len; j++)
                {
                    if (arr[i] > arr[j])
                    {
                        parity ^= 1;
                    }
                }
            }
            return parity;
        }
    }
}
