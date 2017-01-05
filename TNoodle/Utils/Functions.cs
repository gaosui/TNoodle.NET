using System;
using System.Collections.Generic;
using System.Text;
using TNoodle.Puzzles;
using static TNoodle.Puzzles.CubePuzzle;
using static TNoodle.Utils.Assertion;

namespace TNoodle.Utils
{
    public static class Functions
    {
        public static T Choose<T>(Random r, IEnumerable<T> keySet)
        {
            var chosen = default(T);
            var count = 0;
            foreach (var element in keySet)
                if (r.Next(++count) == 0)
                    chosen = element;
            Assert(count > 0);
            return chosen;
        }

        public static int BitCount(int value)
        {
            var v = (uint) value;
            uint c;

            c = v - ((v >> 1) & 0x55555555);
            c = ((c >> 2) & 0x33333333) + (c & 0x33333333);
            c = ((c >> 4) + c) & 0x0F0F0F0F;
            c = ((c >> 8) + c) & 0x00FF00FF;
            c = ((c >> 16) + c) & 0x0000FFFF;

            return (int) c;
        }

        public static int Modulo(int x, int m)
        {
            Assert(m > 0, "m must be > 0");
            var y = x % m;
            if (y < 0)
                y += m;
            return y;
        }

        public static string Join<T>(List<T> arr, string separator)
        {
            if (separator == null)
                separator = ",";

            var sb = new StringBuilder();
            for (var i = 0; i < arr.Count; i++)
            {
                if (i > 0)
                    sb.Append(separator);
                sb.Append(arr[i]);
            }
            return sb.ToString();
        }

        public static Face OppositeFace(this Face f)
        {
            return (Face) (((int) f + 3) % 6);
        }

        // TODO We could rename faces so we can just do +6 mod 12 here instead.
        public static MegaminxPuzzle.Face OppositeFace(this MegaminxPuzzle.Face face)
        {
            switch (face)
            {
                case MegaminxPuzzle.Face.U:
                    return MegaminxPuzzle.Face.D;
                case MegaminxPuzzle.Face.Bl:
                    return MegaminxPuzzle.Face.Dr;
                case MegaminxPuzzle.Face.Br:
                    return MegaminxPuzzle.Face.Dl;
                case MegaminxPuzzle.Face.R:
                    return MegaminxPuzzle.Face.Dbl;
                case MegaminxPuzzle.Face.F:
                    return MegaminxPuzzle.Face.B;
                case MegaminxPuzzle.Face.L:
                    return MegaminxPuzzle.Face.Dbr;
                case MegaminxPuzzle.Face.D:
                    return MegaminxPuzzle.Face.U;
                case MegaminxPuzzle.Face.Dr:
                    return MegaminxPuzzle.Face.Bl;
                case MegaminxPuzzle.Face.Dbr:
                    return MegaminxPuzzle.Face.L;
                case MegaminxPuzzle.Face.B:
                    return MegaminxPuzzle.Face.F;
                case MegaminxPuzzle.Face.Dbl:
                    return MegaminxPuzzle.Face.R;
                case MegaminxPuzzle.Face.Dl:
                    return MegaminxPuzzle.Face.Br;
                default:
                    throw new ArgumentOutOfRangeException(nameof(face), face, null);
            }
        }
    }
}