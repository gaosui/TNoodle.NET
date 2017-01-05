using System;
using System.Linq;
using TNoodle.Puzzles;

namespace TNoodle.Utils
{
    public static class ArrayExtension
    {
        public static T[][] New<T>(int l1, int l2)
        {
            var res = new T[l1][];
            for (var i = 0; i < l1; i++)
                res[i] = new T[l2];
            return res;
        }

        public static T[][][] New<T>(int l1, int l2, int l3)
        {
            var res = new T[l1][][];
            for (var i = 0; i < l1; i++)
            {
                res[i] = new T[l2][];
                for (var j = 0; j < l2; j++)
                    res[i][j] = new T[l3];
            }
            return res;
        }

        public static void DeepCopyTo<T>(this T[][][] src, T[][][] dest)
        {
            for (var i = 0; i < src.Length; i++)
                src[i].DeepCopyTo(dest[i]);
        }

        public static void DeepCopyTo<T>(this T[][] src, T[][] dest)
        {
            for (var i = 0; i < src.Length; i++)
                Array.Copy(src[i], 0, dest[i], 0, src[i].Length);
        }

        public static bool DeepEquals(this int[][][] src, int[][][] dest)
        {
            for (var i = 0; i < src.Length; i++)
            for (var j = 0; j < src[i].Length; j++)
            for (var k = 0; k < src[i][j].Length; k++)
                if (src[i][j][k] != dest[i][j][k]) return false;
            return true;
        }

        public static int DeepHashCode(this int[] a)
        {
            return a.Aggregate(1, (current, t) => 31 * current + t);
        }

        public static int DeepHashCode(this int[][][] src)
        {
            return src.Aggregate(1,
                (current2, t) =>
                    t.Aggregate(current2, (current1, t1) => t1.Aggregate(current1, (current, t2) => 31 * current + t2)));
        }

        public static LinkedHashMap<TB, TA> ReverseHashMap<TA, TB>(this LinkedHashMap<TA, TB> map)
        {
            var reverseMap = new LinkedHashMap<TB, TA>();
            foreach (var pair in map)
                reverseMap.Add(pair.Value, pair.Key);
            return reverseMap;
        }

        public static void Fill<T>(T[] arr, T value)
        {
            for (var i = 0; i < arr.Length; i++)
                arr[i] = value;
        }
    }
}