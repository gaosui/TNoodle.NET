using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TNoodle.Puzzles;
using static TNoodle.Puzzles.CubePuzzle;

namespace TNoodle.Utils
{
    public static class Functions
    {
        #region Array



        public static void Fill<T>(T[] arr, T value)
        {
            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = value;
            }
        }

        #endregion
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

        public static bool DeepEquals(this int[] a, int[] b)
        {
            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i]) return false;
            }
            return true;
        }

        public static bool DeepEquals(this int[,] a, int[,] b)
        {
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    if (a[i, j] != b[i, j]) return false;
                }
            }
            return true;
        }

        public static bool DeepEquals(this int[,,] a, int[,,] b)
        {
            for (int i = 0; i < a.GetLength(0); i++)
            {
                for (int j = 0; j < a.GetLength(1); j++)
                {
                    for (int k = 0; k < a.GetLength(2); k++)
                    {
                        if (a[i, j, k] != b[i, j, k]) return false;
                    }
                }
            }
            return true;
        }

        public static int DeepHashCode(this int[] a)
        {
            int result = 1;
            for (int i = 0, length = a.Length; i < length; i++)
            {
                result = 31 * result + a[i];
            }
            return result;
        }

        public static int DeepHashCode(this int[,] a)
        {
            if (a == null)
                return 0;

            int result = 1;
            foreach (int element in a)
                result = 31 * result + element;

            return result;
        }

        public static int DeepHashCode(this int[,,] a)
        {
            if (a == null)
                return 0;

            int result = 1;
            foreach (int element in a)
                result = 31 * result + element;

            return result;
        }

        public static Face OppositeFace(this Face f)
        {
            return (Face)(((int)f + 3) % 6);
        }

        // TODO We could rename faces so we can just do +6 mod 12 here instead.
        public static MegaminxPuzzle.Face? oppositeFace(this MegaminxPuzzle.Face face)
        {
            switch (face)
            {
                case MegaminxPuzzle.Face.U:
                    return MegaminxPuzzle.Face.D;
                case MegaminxPuzzle.Face.BL:
                    return MegaminxPuzzle.Face.DR;
                case MegaminxPuzzle.Face.BR:
                    return MegaminxPuzzle.Face.DL;
                case MegaminxPuzzle.Face.R:
                    return MegaminxPuzzle.Face.DBL;
                case MegaminxPuzzle.Face.F:
                    return MegaminxPuzzle.Face.B;
                case MegaminxPuzzle.Face.L:
                    return MegaminxPuzzle.Face.DBR;
                case MegaminxPuzzle.Face.D:
                    return MegaminxPuzzle.Face.U;
                case MegaminxPuzzle.Face.DR:
                    return MegaminxPuzzle.Face.BL;
                case MegaminxPuzzle.Face.DBR:
                    return MegaminxPuzzle.Face.L;
                case MegaminxPuzzle.Face.B:
                    return MegaminxPuzzle.Face.F;
                case MegaminxPuzzle.Face.DBL:
                    return MegaminxPuzzle.Face.R;
                case MegaminxPuzzle.Face.DL:
                    return MegaminxPuzzle.Face.BR;
                default:
                    return null;
            }
        }




    }
}
